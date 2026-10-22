using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A queda no meio (etapa 4.5, CONTRATOS.md 15), pelo ExecutarAsync com o handler que responde conforme o pedido: a
// consulta do protocolo, a recuperação da autorização, a duplicidade identificada e as notas tiradas da mensagem
// enviada. Contra a SEFAZ emulada, em QuedaEmuladaTestes.
public partial class AutorizacaoTestes
{
    private const string ChaveZerada = "00000000000000000000000000000000000000000000";

    // O suporte e os parâmetros

    [Fact]
    public void AConsultaValeNosDoisDocumentosEARecuperacaoSoNaNFe()
    {
        using var conexoes = new ConexoesSefaz();
        var nfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));
        var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, Configuracao(conexoes));

        Assert.True(nfe.Suporta<ConsultaProtocolo>());
        Assert.True(nfe.Suporta<RecuperacaoAutorizacao>());
        Assert.True(nfce.Suporta<ConsultaProtocolo>());
        Assert.False(nfce.Suporta<RecuperacaoAutorizacao>());
    }

    [Fact]
    public void OsParametrosDaQuedaRecusamOVazio()
    {
        var chave = ChaveAcesso.Ler(Chave(Assinada(Nota())));

        Assert.Throws<ArgumentNullException>(() => new ConsultaProtocolo(null!));
        Assert.Throws<ArgumentNullException>(() => new RecuperacaoAutorizacao(null!));
        Assert.Throws<ArgumentException>(() => new RecuperacaoAutorizacao(" "));
        Assert.Same(chave, new ConsultaProtocolo(chave).Chave);
        Assert.Equal("<NFe/>", new RecuperacaoAutorizacao("<NFe/>").Xml);
    }

    // A consulta do protocolo

    [Fact]
    public async Task AConsultaVaiAoAutorizadorDaChaveNoSchema()
    {
        var chave = ChaveAcesso.Ler(Chave(Assinada(Nota())));

        var (resultado, envio) = await Executar(new ConsultaProtocolo(chave), _ => Situacao(217, "Rejeição: NF-e não consta na base de dados da SEFAZ", chave.ToString()));

        Assert.True(resultado.Processado);
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.EndsWith("/NFeConsultaProtocolo4", envio!.Url.AbsolutePath.Replace(".asmx", "", StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase);
        Assert.Empty(SchemasNFe400.Situacao.Validar(envio.Mensagem));
        var pedido = XElement.Parse(envio.Mensagem);
        Assert.Equal(XName.Get("consSitNFe", NamespaceNFe), pedido.Name);
        Assert.Equal(["2", "CONSULTAR", chave.ToString()], pedido.Elements().Select(e => e.Value));
    }

    [Fact]
    public async Task AChaveEmitidaNaSvcVaiASvc()
    {
        var chave = ChaveAcesso.Ler(Chave(Assinada(Nota(), TipoEmissao.ContingenciaSvcAN)));

        var (resultado, _) = await Executar(new ConsultaProtocolo(chave), _ => Situacao(217, "Rejeição: NF-e não consta", chave.ToString()));
        var (normal, _) = await Executar(new ConsultaProtocolo(chave) { Destino = Destino.MG }, _ => Situacao(217, "Rejeição: NF-e não consta", chave.ToString()));

        Assert.Equal(Destino.SvcAN, resultado.Rastro.Autorizador);
        Assert.Equal(Destino.MG, normal.Rastro.Autorizador);
    }

    [Fact]
    public async Task AChaveDeOutroModeloERecusada()
    {
        var chave = ChaveAcesso.Ler(Chave(NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()))));

        var (resultado, envio) = await Executar(new ConsultaProtocolo(chave), _ => Situacao(100, "Autorizado o uso da NF-e", chave.ToString()));

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-CHAVE", Severidade.Erro, "consSitNFe/chNFe", "Chave"), (problema.Codigo, problema.Severidade, problema.Caminho, problema.Propriedade));
    }

    [Fact]
    public async Task AConsultaDaAutorizadaTrazOProtocoloEOsEventos()
    {
        var xml = Assinada(Nota());
        var chave = ChaveAcesso.Ler(Chave(xml));
        var evento = "<procEventoNFe versao=\"1.00\"><evento versao=\"1.00\"/><retEvento versao=\"1.00\"/></procEventoNFe>";

        var (resultado, _) = await Executar(new ConsultaProtocolo(chave),
            _ => Situacao(100, "Autorizado o uso da NF-e", chave.ToString(), Protocolo(chave.ToString(), Digest(xml), 100), evento, evento));

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        var retorno = resultado.Retorno;
        Assert.Equal((100, Ambiente.Homologacao, UF.MG, "MG-TESTE"), (retorno.Status, retorno.Ambiente, retorno.UF, retorno.VersaoAplicativo));
        Assert.Equal(DateTimeOffset.Parse(Recebimento, System.Globalization.CultureInfo.InvariantCulture), retorno.Recebimento);
        Assert.Equal(chave, retorno.Chave);
        Assert.True(retorno.Autorizada);
        Assert.False(retorno.Cancelada);
        Assert.Equal(("131260000000001", Digest(xml)), (retorno.Protocolo!.Numero, retorno.Protocolo.Digest));
        Assert.Equal(2, retorno.Eventos.Count);
        Assert.All(retorno.Eventos, e => Assert.StartsWith($"<procEventoNFe versao=\"1.00\" xmlns=\"{NamespaceNFe}\">", e, StringComparison.Ordinal));
        Assert.Null(retorno.ChaveRegistrada);
    }

    [Theory]
    [InlineData(101, false, true, true)]
    [InlineData(150, true, false, true)]
    [InlineData(217, false, false, false)]
    public async Task ASituacaoDizSeEstaAutorizadaOuCancelada(int status, bool autorizada, bool cancelada, bool protocolo)
    {
        var xml = Assinada(Nota());
        var chave = Chave(xml);

        var (resultado, _) = await Executar(new ConsultaProtocolo(ChaveAcesso.Ler(chave)),
            _ => Situacao(status, "Motivo", chave, protocolo ? Protocolo(chave, Digest(xml), 100) : ""));

        Assert.Equal((autorizada, cancelada), (resultado.Retorno!.Autorizada, resultado.Retorno.Cancelada));
        Assert.Equal(protocolo, resultado.Retorno.Protocolo is not null);
        Assert.Empty(resultado.Retorno.Eventos);
    }

    [Theory]
    [InlineData("Rejeição: Divergência no código numérico [chNFe:{0}]", true)]
    [InlineData("Rejeição: Divergência no código numérico [chNFe: {0}]", true)]
    [InlineData("Rejeição: Divergência no código numérico", false)]
    [InlineData("Rejeição: Divergência no código numérico [chNFe:123]", false)]
    public async Task O562TrazAChaveRegistradaQuandoOMotivoTraz(string motivo, bool registrada)
    {
        var chave = ChaveAcesso.Ler(Chave(Assinada(Nota())));
        var outra = Chave(Assinada(Nota(2)));

        var (resultado, _) = await Executar(new ConsultaProtocolo(chave),
            _ => Situacao(562, string.Format(System.Globalization.CultureInfo.InvariantCulture, motivo, outra), chave.ToString()));

        Assert.Empty(resultado.Problemas);
        Assert.Equal(registrada ? ChaveAcesso.Ler(outra) : null, resultado.Retorno!.ChaveRegistrada);
    }

    [Fact]
    public async Task AChaveIlegivelDoRetornoEAvisoEAZeradaNao()
    {
        var chave = ChaveAcesso.Ler(Chave(Assinada(Nota())));

        var (ilegivel, _) = await Executar(new ConsultaProtocolo(chave), _ => Situacao(217, "Motivo", "123"));
        var (zerada, _) = await Executar(new ConsultaProtocolo(chave), _ => Situacao(215, "Rejeição: Falha no schema XML", ChaveZerada));

        var problema = Assert.Single(ilegivel.Problemas);
        Assert.Equal(("NFE-VALOR", Severidade.Aviso, "retConsSitNFe/chNFe"), (problema.Codigo, problema.Severidade, problema.Caminho));
        Assert.Null(ilegivel.Retorno!.Chave);
        Assert.Empty(zerada.Problemas);
        Assert.Null(zerada.Retorno!.Chave);
    }

    [Fact]
    public async Task OProtocoloIlegivelEAvisoComOCaminhoDoRetorno()
    {
        var xml = Assinada(Nota());
        var chave = Chave(xml);
        var protocolo = Protocolo(chave, Digest(xml), 100).Replace(Recebimento, "ontem", StringComparison.Ordinal);

        var (consulta, _) = await Executar(new ConsultaProtocolo(ChaveAcesso.Ler(chave)), _ => Situacao(100, "Autorizado", chave, protocolo));
        var (recuperacao, _) = await Executar(new RecuperacaoAutorizacao(xml), _ => Situacao(100, "Autorizado", chave, protocolo));

        var problema = Assert.Single(consulta.Problemas);
        Assert.Equal(("NFE-VALOR", Severidade.Aviso, "retConsSitNFe/protNFe/infProt/dhRecbto", "Protocolo.Recebimento"),
            (problema.Codigo, problema.Severidade, problema.Caminho, problema.Propriedade));
        Assert.Equal("Notas[0].Protocolo.Recebimento", Assert.Single(recuperacao.Problemas).Propriedade);
        Assert.NotNull(recuperacao.Retorno!.Notas[0].XmlProcessado);
    }

    [Theory]
    [InlineData("<retEnviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><cStat>104</cStat><xMotivo>x</xMotivo></retEnviNFe>")]
    [InlineData("<retConsSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><xMotivo>x</xMotivo></retConsSitNFe>")]
    [InlineData("<retConsSitNFe><cStat>100</cStat><xMotivo>x</xMotivo></retConsSitNFe>")]
    public async Task ARespostaSemASituacaoEIlegivel(string resposta)
    {
        var chave = ChaveAcesso.Ler(Chave(Assinada(Nota())));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Executar(new ConsultaProtocolo(chave), _ => resposta));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
    }

    // A recuperação da autorização

    [Fact]
    public async Task ARecuperacaoDaAutorizadaTrazONfeProc()
    {
        var xml = Assinada(Nota());
        var chave = Chave(xml);

        var (resultado, envio) = await Executar(new RecuperacaoAutorizacao("﻿<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + xml),
            _ => Situacao(100, "Autorizado o uso da NF-e", chave, Protocolo(chave, Digest(xml), 100)));

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Empty(SchemasNFe400.Situacao.Validar(envio!.Mensagem));
        Assert.Equal(chave, XElement.Parse(envio.Mensagem).Elements().Last().Value);
        var retorno = resultado.Retorno;
        Assert.Equal((100, "Autorizado o uso da NF-e", UF.MG), (retorno.Status, retorno.Motivo, retorno.UF));
        Assert.Null(retorno.Recibo);
        Assert.Null(retorno.TempoMedio);
        var nota = Assert.Single(retorno.Notas);
        Assert.True(nota.Autorizada);
        Assert.Null(nota.Duplicidade);
        Assert.Equal((chave, xml), (nota.Chave!.ToString(), nota.Xml));
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        Assert.StartsWith($"<nfeProc xmlns=\"{NamespaceNFe}\" versao=\"4.00\">{xml}<protNFe", nota.XmlProcessado, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(nota.XmlProcessado!, "NFe" + chave, NotasTeste.CnpjEmitente));
    }

    [Theory]
    [InlineData(110, 110, false)]
    [InlineData(101, 100, true)]
    public async Task ARecuperacaoDaDenegadaEDaCanceladaTambemTrazONfeProc(int situacao, int protocolo, bool autorizada)
    {
        var xml = Assinada(Nota());
        var chave = Chave(xml);

        var (resultado, _) = await Executar(new RecuperacaoAutorizacao(xml), _ => Situacao(situacao, "Motivo", chave, Protocolo(chave, Digest(xml), protocolo)));

        Assert.Equal(situacao, resultado.Retorno!.Status);
        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.Equal(autorizada, nota.Autorizada);
        Assert.NotNull(nota.XmlProcessado);
    }

    [Fact]
    public async Task ARecuperacaoDaQueNaoConstaDeixaReenviar()
    {
        var xml = Assinada(Nota());
        var chave = Chave(xml);

        var (resultado, _) = await Executar(new RecuperacaoAutorizacao(xml), _ => Situacao(217, "Rejeição: NF-e não consta na base de dados da SEFAZ", chave));

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(217, resultado.Retorno.Status);
        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.Equal((xml, chave), (nota.Xml, nota.Chave!.ToString()));
        Assert.Null(nota.Protocolo);
        Assert.Null(nota.XmlProcessado);
    }

    [Theory]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAA=", "O digVal do protocolo não é")]
    [InlineData("", "O protocolo não traz o digVal")]
    public async Task OProtocoloDeOutroXmlNaoDaONfeProc(string digest, string mensagem)
    {
        var xml = Assinada(Nota());
        var chave = Chave(xml);

        var (resultado, _) = await Executar(new RecuperacaoAutorizacao(xml), _ => Situacao(100, "Autorizado o uso da NF-e", chave, Protocolo(chave, digest, 100)));

        var nota = Assert.Single(resultado.Retorno!.Notas);
        Assert.Equal(100, nota.Protocolo!.Status);
        Assert.Null(nota.XmlProcessado);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-PROTOCOLO", Severidade.Aviso, "Notas[0].Protocolo.Digest"), (problema.Codigo, problema.Severidade, problema.Propriedade));
        Assert.StartsWith(mensagem, problema.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OProtocoloDeOutraChaveFicaFora()
    {
        var xml = Assinada(Nota());
        var outra = Assinada(Nota(2));

        var (resultado, _) = await Executar(new RecuperacaoAutorizacao(xml),
            _ => Situacao(100, "Autorizado o uso da NF-e", Chave(outra), Protocolo(Chave(outra), Digest(outra), 100)));

        Assert.Null(Assert.Single(resultado.Retorno!.Notas).Protocolo);
        Assert.Equal("NFE-PROTOCOLO", Assert.Single(resultado.Problemas).Codigo);
    }

    [Theory]
    [InlineData("<NFe", "NFE-XML")]
    [InlineData("<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\"/>", "NFE-RAIZ")]
    [InlineData("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"NFe123\"/></NFe>", "NFE-CHAVE")]
    public async Task ARecuperacaoDoQueNaoENotaERecusada(string xml, string codigo)
    {
        var (resultado, envio) = await Executar(new RecuperacaoAutorizacao(xml), _ => Situacao(217, "Motivo", ChaveZerada));

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        Assert.Equal(codigo, Assert.Single(resultado.Problemas).Codigo);
    }

    [Fact]
    public async Task ARecuperacaoConfereAAssinaturaEOModelo()
    {
        var alterada = Assinada(Nota()).Replace("<natOp>VENDA DE MERCADORIA</natOp>", "<natOp>OUTRA OPERACAO</natOp>", StringComparison.Ordinal);
        var nfce = NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()));

        var (assinatura, _) = await Executar(new RecuperacaoAutorizacao(alterada), _ => Situacao(217, "Motivo", ChaveZerada));
        var (modelo, envio) = await Executar(new RecuperacaoAutorizacao(nfce), _ => Situacao(217, "Motivo", ChaveZerada));

        Assert.Equal("297", Assert.Single(assinatura.Problemas).Codigo);
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, modelo.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(modelo.Problemas);
        Assert.Equal(("NFE-CHAVE", "NFe/infNFe/@Id", "Xml"), (problema.Codigo, problema.Caminho, problema.Propriedade));
    }

    // A duplicidade identificada (decisão 10)

    [Theory]
    [InlineData(204, "Rejeição: Duplicidade de NF-e [nRec:311000000000009]", true, "311000000000009")]
    [InlineData(204, "Rejeição: Duplicidade de NF-e", true, null)]
    [InlineData(539, "Rejeição: Duplicidade de NF-e com diferença na Chave de Acesso [chNFe: {0}][nRec:311000000000009]", true, "311000000000009")]
    [InlineData(539, "Rejeição: Duplicidade de NF-e com diferença na Chave de Acesso [chNFe:{0}]", true, null)]
    [InlineData(539, "Rejeição: Duplicidade de NF-e com diferença na Chave de Acesso", false, null)]
    public async Task ADuplicidadeTrazANotaRegistrada(int status, string motivo, bool motivoChave, string? recibo)
    {
        var outra = Chave(Assinada(Nota(2)));
        string? chave = null;

        var (resultado, _) = await Executar(new Autorizacao(Nota()), pedido =>
        {
            var nota = Enviadas(pedido)[0];
            chave = Chave(nota);
            var texto = string.Format(System.Globalization.CultureInfo.InvariantCulture, motivo, outra);
            return Retorno(104, "Lote processado", Protocolo(chave, Digest(nota), status, texto));
        });

        Assert.Empty(resultado.Problemas);
        var duplicidade = Assert.Single(resultado.Retorno!.Notas).Duplicidade!;
        Assert.Equal(status == 204, duplicidade.MesmaChave);
        var esperada = status == 204 ? chave : motivoChave ? outra : null;
        Assert.Equal(esperada, duplicidade.Chave?.ToString());
        Assert.Equal(recibo, duplicidade.Recibo);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(218)]
    public async Task ASemDuplicidadeNaoTrazANota(int status)
    {
        var (resultado, _) = await Executar(new Autorizacao(Nota()), Sincrona(status));

        Assert.Null(Assert.Single(resultado.Retorno!.Notas).Duplicidade);
    }

    // As notas da mensagem enviada (decisão 9)

    [Fact]
    public async Task AsNotasEnviadasSaemComOsBytesDaMensagem()
    {
        var (resultado, envio) = await Executar(new Autorizacao(Nota(1), Nota(2)), Recebido);

        var notas = Autorizacao.NotasEnviadas(resultado.Rastro.XmlPedido!);

        Assert.Equal(Enviadas(envio!.Mensagem), notas);
        Assert.Equal(resultado.Retorno!.Notas.Select(n => n.Xml), notas);
        Assert.All(notas, n => Assert.Empty(AssinaturaDigital.Verificar(n, "NFe" + Chave(n), NotasTeste.CnpjEmitente)));
    }

    [Theory]
    [InlineData("<consSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"/>")]
    [InlineData("<enviNFe><idLote>1</idLote></enviNFe>")]
    [InlineData("<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">")]
    public void OQueNaoEEnviNFeNaoTemNotasEnviadas(string mensagem)
    {
        Assert.Throws<FormatException>(() => Autorizacao.NotasEnviadas(mensagem));
        Assert.Throws<ArgumentNullException>(() => Autorizacao.NotasEnviadas(null!));
    }

    private static string Situacao(int status, string motivo, string chave, params string[] resto) =>
        $"<retConsSitNFe xmlns=\"{NamespaceNFe}\" versao=\"4.00\"><tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic>"
        + $"<cStat>{status}</cStat><xMotivo>{motivo}</xMotivo><cUF>31</cUF><dhRecbto>{Recebimento}</dhRecbto>"
        + $"<chNFe>{chave}</chNFe>{string.Concat(resto)}</retConsSitNFe>";
}
