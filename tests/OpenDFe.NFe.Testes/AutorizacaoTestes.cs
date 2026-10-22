using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A autorização, a consulta do recibo e o preparo da NF-e (etapa 4.4, CONTRATOS.md 14) pelo ExecutarAsync com o
// leiaute de verdade e um handler que responde conforme o pedido (o protocolo leva a chave e o digVal da nota
// enviada): a montagem, as regras do lote, o problema com o índice da nota, a leitura e o nfeProc. Contra a SEFAZ
// emulada, em AutorizacaoEmuladaTestes.
public partial class AutorizacaoTestes
{
    private const string NamespaceNFe = "http://www.portalfiscal.inf.br/nfe";

    private static readonly Lazy<X509Certificate2> Certificado = new(() => new CertificadoTeste { Cnpj = NotasTeste.CnpjEmitente }.Gerar());

    // Uma hora depois da emissão das notas de teste, para as regras de data.
    private static readonly DateTimeOffset Agora = NotasTeste.Emissao.AddHours(1);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // O suporte

    [Fact]
    public void ANFeTemAAutorizacaoAConsultaDoReciboEOPreparo()
    {
        using var conexoes = new ConexoesSefaz();
        var nfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));

        Assert.True(nfe.Suporta<Autorizacao>());
        Assert.True(nfe.Suporta<ConsultaRecibo>());
        Assert.True(nfe.Preparar(Nota()).Preparado);
    }

    [Fact]
    public void ANFCeAindaNaoTemAAutorizacaoNemOPreparo()
    {
        using var conexoes = new ConexoesSefaz();
        var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, Configuracao(conexoes));

        Assert.False(nfce.Suporta<Autorizacao>());
        Assert.False(nfce.Suporta<ConsultaRecibo>());
        Assert.Throws<NotSupportedException>(() => nfce.Preparar(NotasTeste.NFCe()));
    }

    // Os parâmetros

    [Fact]
    public void OsParametrosRecusamOQueASefazRejeitaria()
    {
        Assert.Throws<ArgumentException>(() => new Autorizacao(Array.Empty<NotaFiscal>()));
        Assert.Throws<ArgumentException>(() => new Autorizacao(Array.Empty<string>()));
        Assert.Throws<ArgumentException>(() => new Autorizacao(Enumerable.Range(0, 51).Select(_ => new NotaFiscal())));
        Assert.Throws<ArgumentException>(() => new Autorizacao(new NotaFiscal(), null!));
        Assert.Throws<ArgumentException>(() => new Autorizacao("<NFe/>", " "));
        Assert.Throws<ArgumentNullException>(() => new Autorizacao((IEnumerable<NotaFiscal>)null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Autorizacao(new NotaFiscal()) { Lote = 0 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Autorizacao(new NotaFiscal()) { Lote = 1_000_000_000_000_000 });
        Assert.Equal(999_999_999_999_999, new Autorizacao(new NotaFiscal()) { Lote = 999_999_999_999_999 }.Lote);

        Assert.Throws<ArgumentException>(() => new ConsultaRecibo("12345678901234"));
        Assert.Throws<ArgumentException>(() => new ConsultaRecibo("31100000000000A"));
        Assert.Throws<ArgumentNullException>(() => new ConsultaRecibo(null!));
        Assert.Throws<ArgumentException>(() => new ConsultaRecibo("311000000000001") { Xmls = [""] });
        Assert.Throws<ArgumentException>(() => new ConsultaRecibo("311000000000001") { Xmls = [.. Enumerable.Repeat("<NFe/>", 51)] });
    }

    [Fact]
    public void OsParametrosGuardamAsNotasComoVieram()
    {
        var nota = new NotaFiscal();
        var autorizacao = new Autorizacao(nota);
        var assinadas = new Autorizacao("<a/>", "<b/>");

        Assert.Same(nota, Assert.Single(autorizacao.Notas));
        Assert.Empty(autorizacao.Xmls);
        Assert.Null(autorizacao.Lote);
        Assert.Equal(["<a/>", "<b/>"], assinadas.Xmls);
        Assert.Empty(assinadas.Notas);
        Assert.Empty(new ConsultaRecibo("311000000000001").Xmls);
    }

    // A síncrona

    [Fact]
    public async Task UmaNotaVaiNaSincronaEVoltaComONfeProc()
    {
        var (resultado, envio) = await Executar(new Autorizacao(Nota()), Sincrona(100));

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal((104, "Lote processado"), (resultado.CStat, resultado.Motivo));
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.EndsWith("/NFeAutorizacao4", envio!.Url.AbsolutePath.Replace(".asmx", "", StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase);

        var pedido = XElement.Parse(envio!.Mensagem);
        Assert.Equal(XName.Get("enviNFe", NamespaceNFe), pedido.Name);
        Assert.Equal(["idLote", "indSinc", "NFe"], pedido.Elements().Select(e => e.Name.LocalName));
        Assert.Equal("261020183000000", pedido.Elements().First().Value);
        Assert.Equal("1", pedido.Elements().ElementAt(1).Value);
        Assert.Empty(SchemasNFe400.Lote.Validar(envio!.Mensagem));

        var retorno = resultado.Retorno;
        Assert.Equal((104, Ambiente.Homologacao, UF.MG), (retorno.Status, retorno.Ambiente, retorno.UF));
        Assert.Null(retorno.Recibo);
        var nota = Assert.Single(retorno.Notas);
        Assert.True(nota.Autorizada);
        Assert.Equal(Chave(envio!.Mensagem), nota.Chave!.ToString());
        Assert.Equal(Enviadas(envio!.Mensagem)[0], nota.Xml);
        Assert.Equal((100, "131260000000001"), (nota.Protocolo!.Status, nota.Protocolo.Numero));
        Assert.Empty(AssinaturaDigital.Verificar(nota.Xml!, "NFe" + nota.Chave, NotasTeste.CnpjEmitente));

        var processada = nota.XmlProcessado!;
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(processada));
        Assert.StartsWith($"<nfeProc xmlns=\"{NamespaceNFe}\" versao=\"4.00\">{nota.Xml}<protNFe", processada, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(processada, "NFe" + nota.Chave, NotasTeste.CnpjEmitente));
        var lida = new LeiauteNFe400().Ler(processada);
        Assert.Empty(lida.Problemas);
        Assert.Equal(100, lida.Protocolo!.Status);
    }

    [Theory]
    [InlineData(150, true, true)]
    [InlineData(110, false, true)]
    [InlineData(301, false, true)]
    [InlineData(302, false, true)]
    [InlineData(539, false, false)]
    [InlineData(225, false, false)]
    public async Task ONfeProcSaiNaAutorizacaoENaDenegacao(int status, bool autorizada, bool processada)
    {
        var (resultado, _) = await Executar(new Autorizacao(Nota()), Sincrona(status));

        var nota = Assert.Single(resultado.Retorno!.Notas);
        Assert.Equal(status, nota.Protocolo!.Status);
        Assert.Equal(autorizada, nota.Autorizada);
        Assert.Equal(processada, nota.XmlProcessado is not null);
        Assert.NotNull(nota.Xml);
    }

    [Fact]
    public async Task OLoteInformadoVaiNoIdLote()
    {
        var (_, envio) = await Executar(new Autorizacao(Nota()) { Lote = 42 }, Sincrona(100));

        Assert.Equal("42", XElement.Parse(envio!.Mensagem).Elements().First().Value);
    }

    // O lote

    [Fact]
    public async Task DuasNotasVaoNoLoteEVoltamComORecibo()
    {
        var (resultado, envio) = await Executar(new Autorizacao(Nota(1), Nota(2)), Recebido);

        var pedido = XElement.Parse(envio!.Mensagem);
        Assert.Equal("0", pedido.Elements().ElementAt(1).Value);
        Assert.Equal(2, pedido.Elements(XName.Get("NFe", NamespaceNFe)).Count());
        Assert.Empty(SchemasNFe400.Lote.Validar(envio!.Mensagem));

        Assert.Empty(resultado.Problemas);
        var retorno = resultado.Retorno!;
        Assert.Equal((103, "311000000000001", TimeSpan.FromSeconds(3)), (retorno.Status, retorno.Recibo, retorno.TempoMedio));
        Assert.Equal(2, retorno.Notas.Count);
        var enviadas = Enviadas(envio!.Mensagem);
        for (var i = 0; i < 2; i++)
        {
            var nota = retorno.Notas[i];
            Assert.Equal(enviadas[i], nota.Xml);
            Assert.Null(nota.Protocolo);
            Assert.Null(nota.XmlProcessado);
            Assert.False(nota.Autorizada);
            Assert.Empty(AssinaturaDigital.Verificar(nota.Xml!, "NFe" + nota.Chave, NotasTeste.CnpjEmitente));
        }
        Assert.Equal([1, 2], retorno.Notas.Select(n => n.Chave!.Numero));
    }

    [Fact]
    public async Task OLoteEmProcessamentoTrazAsNotasSemProtocolo()
    {
        var (resultado, _) = await Executar(new Autorizacao(Nota(1), Nota(2)), _ => Retorno(105, "Lote em processamento"));

        Assert.Empty(resultado.Problemas);
        Assert.All(resultado.Retorno!.Notas, n => Assert.Null(n.Protocolo));
        Assert.Equal(2, resultado.Retorno.Notas.Count);
    }

    [Fact]
    public async Task ARejeicaoDoLoteNaoTrazAsNotas()
    {
        var (resultado, _) = await Executar(new Autorizacao(Nota()), _ => Retorno(656, "Rejeicao: Consumo Indevido"));

        Assert.True(resultado.Processado);
        Assert.Equal(656, resultado.CStat);
        Assert.Empty(resultado.Retorno.Notas);
    }

    // As notas assinadas

    [Fact]
    public async Task ANotaPreparadaVaiComOsMesmosBytes()
    {
        using var conexoes = new ConexoesSefaz();
        var preparado = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes)).Preparar(Nota());
        Assert.True(preparado.Preparado);

        var (resultado, envio) = await Executar(new Autorizacao("﻿<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + preparado.Xml), Sincrona(100));

        Assert.Empty(resultado.Problemas);
        Assert.Contains("<indSinc>1</indSinc>" + preparado.Xml + "</enviNFe>", envio!.Mensagem, StringComparison.Ordinal);
        var nota = Assert.Single(resultado.Retorno!.Notas);
        Assert.Equal(preparado.Xml, nota.Xml);
        Assert.Equal(preparado.Chave, nota.Chave);
        Assert.Contains(preparado.Xml!, nota.XmlProcessado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ANotaAssinadaVaiAoAutorizadorDaChave()
    {
        // Assinada em contingência SVC-AN e transmitida por um núcleo normal: vai à SVC-AN (PROJETO.md 8.4).
        var xml = Assinada(Nota(), TipoEmissao.ContingenciaSvcAN);

        var (resultado, _) = await Executar(new Autorizacao(xml), Sincrona(100));

        Assert.Equal(Destino.SvcAN, resultado.Rastro.Autorizador);
        Assert.True(Assert.Single(resultado.Retorno!.Notas).Autorizada);
    }

    // As regras do lote (decisão 10)

    [Fact]
    public async Task AAssinaturaQueNaoConfereERecusada()
    {
        var xml = Assinada(Nota()).Replace("<natOp>VENDA DE MERCADORIA</natOp>", "<natOp>OUTRA OPERACAO</natOp>", StringComparison.Ordinal);
        Assert.Contains("OUTRA OPERACAO", xml, StringComparison.Ordinal);

        var (resultado, envio) = await Executar(new Autorizacao(xml), Sincrona(100));

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("297", Severidade.Erro), (problema.Codigo, problema.Severidade));
    }

    [Fact]
    public async Task AsRegrasDoLoteRecusamAComposicaoErrada()
    {
        var nfce = NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()));
        var sp = Assinada(Nota(2), uf: UF.SP);
        var svc = Assinada(Nota(3), TipoEmissao.ContingenciaSvcAN);
        var normal = Assinada(Nota(1));

        var (resultado, envio) = await Executar(new Autorizacao(normal, nfce, sp, svc, normal), Recebido);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        Assert.Equal(
            [
                ("765", "Xmls[1].Identificacao.Modelo"),
                ("476", "Xmls[2].Identificacao.UF"),
                (CodigoLote, "Xmls[3].Identificacao.TipoEmissao"),
                (CodigoLote, null),
            ],
            resultado.Problemas.Select(p => (p.Codigo, p.Propriedade)));
        Assert.All(resultado.Problemas, p => Assert.Equal(Severidade.Erro, p.Severidade));
        Assert.Equal("NFe/infNFe/@Id", resultado.Problemas[3].Caminho);
    }

    [Fact]
    public async Task OEmitenteDiferenteNoLoteEAvisoEOLoteSegue()
    {
        var outro = Nota(2);
        outro.Emitente.Cnpj = "11444777000161";

        var (resultado, envio) = await Executar(new Autorizacao(Nota(1), outro), Recebido);

        Assert.True(resultado.Processado);
        Assert.NotNull(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("560", Severidade.Aviso, "NFe/infNFe/emit"), (problema.Codigo, problema.Severidade, problema.Caminho));
    }

    [Theory]
    [InlineData("<NFe", "NFE-XML")]
    [InlineData("<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\"/>", "NFE-RAIZ")]
    [InlineData("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"NFe123\"/></NFe>", "NFE-CHAVE")]
    public async Task ONaoNFeAssinadoERecusado(string xml, string codigo)
    {
        var (resultado, envio) = await Executar(new Autorizacao(xml), Sincrona(100));

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        Assert.Equal(codigo, Assert.Single(resultado.Problemas).Codigo);
    }

    // O problema com o índice da nota (decisão 9)

    [Fact]
    public async Task OSchemaDaNotaApontaANotaDoLote()
    {
        var invalida = Nota(2);
        invalida.Emitente.Nome = new string('A', 61);

        var (resultado, envio) = await Executar(new Autorizacao(Nota(1), invalida), Recebido);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("225", "NFe/infNFe/emit/xNome", "Notas[1].Emitente.Nome"), (problema.Codigo, problema.Caminho, problema.Propriedade));
    }

    [Fact]
    public async Task AEscritaEAsRegrasApontamANotaDoLote()
    {
        var numeroNulo = Nota(1);
        numeroNulo.Identificacao.Numero = null;
        var (escrita, _) = await Executar(new Autorizacao(Nota(2), numeroNulo), Recebido);
        Assert.Contains(escrita.Problemas, p => (p.Codigo, p.Propriedade) == ("NFE-CHAVE", "Notas[1].Identificacao.Numero"));

        var destinatario = Nota(1);
        destinatario.Destinatario!.Cnpj = "11444777000160";
        var (regra, _) = await Executar(new Autorizacao(Nota(2), destinatario), Recebido);
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, regra.Situacao);
        var problema = Assert.Single(regra.Problemas);
        Assert.Equal(("208", "NFe/infNFe/dest/CNPJ", "Notas[1].Destinatario.Cnpj"), (problema.Codigo, problema.Caminho, problema.Propriedade));
    }

    // A leitura

    [Fact]
    public async Task OProtocoloDeOutraChaveEAvisoEANotaSemProtocoloTambem()
    {
        var outraChave = "31261011222333000181550010000099991123456780";

        var (resultado, _) = await Executar(new Autorizacao(Nota()), pedido =>
            Retorno(104, "Lote processado", Protocolo(outraChave, "", 100)));

        Assert.True(resultado.Processado);
        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.Null(nota.Protocolo);
        Assert.Equal(
            [("NFE-PROTOCOLO", "retEnviNFe", "Notas[0]"), ("NFE-PROTOCOLO", "retEnviNFe/protNFe/infProt/chNFe", null)],
            resultado.Problemas.Select(p => (p.Codigo, p.Caminho, p.Propriedade)));
        Assert.All(resultado.Problemas, p => Assert.Equal(Severidade.Aviso, p.Severidade));
    }

    [Fact]
    public async Task OValorIlegivelDoProtocoloEAvisoNaNota()
    {
        var (resultado, _) = await Executar(new Autorizacao(Nota()), pedido =>
            Sincrona(100)(pedido).Replace("<dhRecbto>2026-10-20T15:30:01-03:00</dhRecbto><nProt>", "<dhRecbto>ontem</dhRecbto><nProt>", StringComparison.Ordinal));

        var nota = Assert.Single(resultado.Retorno!.Notas);
        Assert.Null(nota.Protocolo!.Recebimento);
        Assert.True(nota.Autorizada);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-VALOR", Severidade.Aviso, "retEnviNFe/protNFe/infProt/dhRecbto", "Notas[0].Protocolo.Recebimento"),
            (problema.Codigo, problema.Severidade, problema.Caminho, problema.Propriedade));
    }

    [Theory]
    [InlineData("<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\"><cStat>107</cStat><xMotivo>x</xMotivo></retConsStatServ>")]
    [InlineData("<retEnviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><xMotivo>x</xMotivo></retEnviNFe>")]
    [InlineData("<retEnviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><cStat>104</cStat></retEnviNFe>")]
    [InlineData("<retEnviNFe><cStat>104</cStat><xMotivo>x</xMotivo></retEnviNFe>")]
    public async Task ARespostaSemOLoteEIlegivel(string resposta)
    {
        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Executar(new Autorizacao(Nota()), _ => resposta));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
    }

    // A consulta do recibo

    [Fact]
    public async Task AConsultaDoReciboComAsNotasTrazONfeProc()
    {
        var notas = new[] { Assinada(Nota(1)), Assinada(Nota(2)) };
        var chaves = notas.Select(n => Chave(n)).ToArray();

        var (resultado, envio) = await Executar(new ConsultaRecibo("311000000000001") { Xmls = notas }, _ =>
            RetornoRecibo(104, "Lote processado", Protocolo(chaves[1], Digest(notas[1]), 100), Protocolo(chaves[0], Digest(notas[0]), 539)));

        var pedido = XElement.Parse(envio!.Mensagem);
        Assert.Equal(XName.Get("consReciNFe", NamespaceNFe), pedido.Name);
        Assert.Equal(["2", "311000000000001"], pedido.Elements().Select(e => e.Value));
        Assert.Empty(SchemasNFe400.Recibo.Validar(envio!.Mensagem));
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);

        Assert.Empty(resultado.Problemas);
        var retorno = resultado.Retorno!;
        Assert.Equal((104, "311000000000001"), (retorno.Status, retorno.Recibo));
        Assert.Equal(chaves, retorno.Notas.Select(n => n.Chave!.ToString()));
        Assert.Equal(notas, retorno.Notas.Select(n => n.Xml));
        Assert.Equal([539, 100], retorno.Notas.Select(n => n.Protocolo!.Status!.Value));
        Assert.Null(retorno.Notas[0].XmlProcessado);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(retorno.Notas[1].XmlProcessado!));
    }

    [Fact]
    public async Task AConsultaDoReciboSemAsNotasTrazSoOsProtocolos()
    {
        var chave = Chave(Assinada(Nota(1)));

        var (resultado, _) = await Executar(new ConsultaRecibo("311000000000001"), _ =>
            RetornoRecibo(104, "Lote processado", Protocolo(chave, "", 100), "<cMsg>12</cMsg><xMsg>Mensagem ao emissor</xMsg>"));

        var nota = Assert.Single(resultado.Retorno!.Notas);
        Assert.Equal(chave, nota.Chave!.ToString());
        Assert.Null(nota.Xml);
        Assert.Null(nota.XmlProcessado);
        Assert.True(nota.Autorizada);
        Assert.Equal((12, "Mensagem ao emissor"), (resultado.Retorno.Mensagem!.Codigo, resultado.Retorno.Mensagem.Texto));
    }

    [Fact]
    public async Task AConsultaDoReciboVaiAoAutorizadorDaChave()
    {
        var nota = Assinada(Nota(), TipoEmissao.ContingenciaSvcAN);

        var (resultado, _) = await Executar(new ConsultaRecibo("311000000000001") { Xmls = [nota] }, _ => RetornoRecibo(106, "Lote nao localizado"));

        Assert.Equal(Destino.SvcAN, resultado.Rastro.Autorizador);
        Assert.Empty(resultado.Retorno!.Notas);
    }

    // O preparo

    [Fact]
    public void OPreparoDaNFeAssinaEValida()
    {
        using var conexoes = new ConexoesSefaz();
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));

        var preparado = dfe.Preparar(Nota());

        Assert.True(preparado.Preparado);
        Assert.Empty(preparado.Problemas);
        Assert.StartsWith($"<NFe xmlns=\"{NamespaceNFe}\"><infNFe", preparado.Xml, StringComparison.Ordinal);
        Assert.Empty(SchemasNFe400.Nota.Validar(preparado.Xml!));
        Assert.Empty(AssinaturaDigital.Verificar(preparado.Xml!, "NFe" + preparado.Chave, NotasTeste.CnpjEmitente));
        Assert.Equal(Documento.NFe, preparado.Chave!.Modelo);
    }

    [Fact]
    public void OProblemaDoPreparoApontaAPropriedadeDaNota()
    {
        using var conexoes = new ConexoesSefaz();
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));
        var nota = Nota();
        nota.Emitente.Nome = new string('A', 61);

        var preparado = dfe.Preparar(nota);

        Assert.False(preparado.Preparado);
        var problema = Assert.Single(preparado.Problemas);
        Assert.Equal(("225", "NFe/infNFe/emit/xNome", "Emitente.Nome"), (problema.Codigo, problema.Caminho, problema.Propriedade));
        Assert.Contains("<Signature", preparado.Xml, StringComparison.Ordinal);
    }
}
