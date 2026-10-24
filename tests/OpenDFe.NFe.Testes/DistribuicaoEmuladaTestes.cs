using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using ConsultaProtocolo = OpenDFe.NFe.ConsultaProtocolo;
using DocumentoDistribuido = OpenDFe.NFe.DocumentoDistribuido;

namespace OpenDFe.Testes;

/// <summary>
/// A distribuição DF-e pela lib inteira contra a SEFAZ emulada (etapa 8.1), com dois ou três núcleos de CNPJ diferente, cada um
/// com o seu certificado autoassinado: o emitente A autoriza a NF-e, o destinatário B e o autorizado no <c>autXML</c> a
/// consultam. Cobre o <c>distNSU</c>, o <c>consNSU</c> e o <c>consChNFe</c>, os quatro tipos de documento, o uso indevido, o
/// certificado, a paginação em dois lotes e a conexão derrubada depois de processar. O repositório do emulador é limpo a cada
/// teste.
/// </summary>
public sealed class DistribuicaoEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private const string CnpjA = NotasTeste.CnpjEmitente; // 11222333000181: o emitente
    private const string CnpjB = "11444777000161"; // o destinatário da nota de teste
    private const string CnpjC = "45723174000110"; // um terceiro
    private const string CpfD = "52998224725";
    private const string Justificativa = "Cancelamento por erro de digitacao";

    private static readonly Dictionary<string, X509Certificate2> Certificados = [];

    private readonly Emulador _emulador;

    public DistribuicaoEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // O certificado autoassinado do documento (CNPJ ou CPF), gerado uma vez.
    private static X509Certificate2 Certificado(string documento)
    {
        lock (Certificados)
        {
            if (!Certificados.TryGetValue(documento, out var certificado))
            {
                var teste = documento.Length == 11 ? new CertificadoTeste { Cpf = documento } : new CertificadoTeste { Cnpj = documento };
                Certificados[documento] = certificado = CertificadoDigital.AbrirPfx(teste.GerarPfx("teste"), "teste");
            }
            return certificado;
        }
    }

    private static DFe Criar(Emulador emulador, ConexoesSefaz conexoes, string documento, TimeProvider? relogio = null) =>
        DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado(documento),
            Conexoes = conexoes,
            Relogio = relogio ?? TimeProvider.System,
            Urls = ApoioEmulado.Urls(emulador, Documento.NFe),
        });

    private DFe Criar(ConexoesSefaz conexoes, string documento) => Criar(_emulador, conexoes, documento);

    // A NF-e de teste do emitente A para o destinatário B (que é também o transportador); o ajuste muda quem recebe.
    private static Task<ResultadoNota> Autorizar(DFe emitente, int numero, Action<NotaFiscal>? ajustar = null, TimeProvider? relogio = null) =>
        ApoioEmulado.Autorizar(emitente, Documento.NFe, numero, relogio, ajustar);

    // O destinatário é o terceiro C (e o transportador também) e B é só um autorizado no autXML.
    private static void NotaTerceiro(NotaFiscal nota)
    {
        nota.Destinatario!.Cnpj = CnpjC;
        nota.Transporte.Transportador!.Cnpj = CnpjC;
        nota.AutorizadosXml.Clear();
        nota.AutorizadosXml.Add(new PessoaAutorizada { Cnpj = CnpjB });
    }

    // A mesma nota e o mesmo protocolo: o nfeProc que a lib montou na autorização e o que a distribuição entrega, que o Ambiente
    // Nacional escreve de novo (a declaração XML e a ordem dos atributos da raiz mudam; a NFe assinada e o protNFe, não).
    private static void MesmaNfeProc(string esperado, string recebido)
    {
        var (a, b) = (XElement.Parse(esperado), XElement.Parse(recebido));
        Assert.Equal(a.Name, b.Name);
        Assert.Equal((string?)a.Attribute("versao"), (string?)b.Attribute("versao"));
        foreach (var nome in new[] { "NFe", "protNFe" })
        {
            var (x, y) = (a.Elements().Single(e => e.Name.LocalName == nome), b.Elements().Single(e => e.Name.LocalName == nome));
            Assert.Equal(x.ToString(SaveOptions.DisableFormatting), y.ToString(SaveOptions.DisableFormatting));
        }
    }

    private static async Task<RetornoDistribuicao> Distribuir(DFe dfe, DistribuicaoDFe consulta)
    {
        var resultado = await dfe.ExecutarAsync(consulta, Ct);
        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        return resultado.Retorno;
    }

    // O destinatário, o uso indevido e o 137

    [Fact]
    public async Task ODestinatarioRecebeOResumoNaAutorizacaoDepoisNenhumDocumentoEDepoisOUsoIndevido()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);

        var primeira = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(138, primeira.Status);
        Assert.Equal("Documento(s) localizado(s)", primeira.Motivo);
        Assert.Equal((1L, 1L), (primeira.UltimoNsu, primeira.MaximoNsu));
        Assert.False(primeira.TemMais);
        Assert.Equal(TimeSpan.FromHours(1), primeira.EsperaSugerida);
        Assert.Equal(Ambiente.Homologacao, primeira.Ambiente);
        Assert.NotNull(primeira.Resposta);
        var documento = Assert.Single(primeira.Documentos);
        Assert.Equal(1L, documento.Nsu);
        Assert.Equal(TipoDocumentoDistribuido.ResumoNFe, documento.Tipo);
        Assert.Equal("resNFe_v1.01.xsd", documento.Schema);
        Assert.Equal(nota.Chave, documento.Chave);
        var resumo = documento.ResumoNFe!;
        Assert.Equal(nota.Chave, resumo.Chave);
        Assert.Equal(CnpjA, resumo.Emitente);
        Assert.Equal(TipoNota.Saida, resumo.TipoNota);
        Assert.True(resumo.ValorTotal > 0);
        Assert.Equal(SituacaoResumo.Autorizada, resumo.Situacao);
        Assert.Equal(nota.Protocolo!.Numero, resumo.Protocolo);
        Assert.NotNull(resumo.Digest);
        Assert.Equal(nota.Protocolo.Recebimento, resumo.Recebimento);
        Assert.Empty(SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso("resNFe_v1.01.xsd"), "215").Validar(documento.Xml));

        var segunda = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, primeira.UltimoNsu!.Value));

        Assert.Equal(137, segunda.Status);
        Assert.True(segunda.NenhumDocumento);
        Assert.Empty(segunda.Documentos);
        Assert.Equal(TimeSpan.FromHours(1), segunda.EsperaSugerida);
        Assert.Equal((1L, 1L), (segunda.UltimoNsu, segunda.MaximoNsu));

        var indevida = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, segunda.UltimoNsu!.Value));

        Assert.Equal(656, indevida.Status);
        Assert.True(indevida.ConsumoIndevido);
        Assert.Contains("Consumo Indevido", indevida.Motivo, StringComparison.Ordinal);
        Assert.Contains("Deve ser aguardado 1 hora", indevida.Motivo, StringComparison.Ordinal);
        Assert.Equal(TimeSpan.FromHours(1), indevida.EsperaSugerida);
        Assert.Empty(indevida.Documentos);
    }

    [Fact]
    public async Task OUltNsuForaDaSequenciaE656ComOUltimoNsuDoServidor()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        await Autorizar(emitente, 1);
        await Autorizar(emitente, 2);
        var primeira = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));
        Assert.Equal(2L, primeira.UltimoNsu);

        var fora = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(656, fora.Status);
        Assert.Contains("Deve ser utilizado o ultNSU", fora.Motivo, StringComparison.Ordinal);
        Assert.Equal(2L, fora.UltimoNsu);
        Assert.Equal(TimeSpan.FromHours(1), fora.EsperaSugerida);
    }

    [Fact]
    public async Task OUltNsuAcimaDoMaximoDoInteressadoE589ComOMaximoDele()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        await Autorizar(emitente, 1);
        await Autorizar(emitente, 2);

        var acima = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 999));

        Assert.Equal(589, acima.Status);
        Assert.Contains("NSU", acima.Motivo, StringComparison.Ordinal);
        Assert.Empty(acima.Documentos);
        Assert.Equal((2L, 2L), (acima.UltimoNsu, acima.MaximoNsu));
        Assert.False(acima.TemMais);
        Assert.Null(acima.EsperaSugerida);
    }

    [Fact]
    public async Task OEmitenteNaoRecebeAPropriaNotaMasONsuDoDestinatarioNaoEDele()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        await Autorizar(emitente, 1);

        var retorno = await Distribuir(emitente, DistribuicaoDFe.UltimoNsu(CnpjA, 0));

        Assert.Equal(137, retorno.Status);
        Assert.Equal((0L, 0L), (retorno.UltimoNsu, retorno.MaximoNsu));
    }

    [Fact]
    public async Task OPrimeiroAcessoDeQuemNuncaRecebeuNadaE137()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);

        var retorno = await Distribuir(Criar(conexoes, CnpjC), DistribuicaoDFe.UltimoNsu(CnpjC));

        Assert.True(retorno.NenhumDocumento);
        Assert.Equal(TimeSpan.FromHours(1), retorno.EsperaSugerida);
    }

    // O autorizado no autXML e os quatro tipos de documento

    [Fact]
    public async Task OAutorizadoNoAutXmlRecebeONfeProcNaAutorizacaoLidoEIgualANota()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var autorizado = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1, NotaTerceiro);

        var retorno = await Distribuir(autorizado, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(138, retorno.Status);
        var documento = Assert.Single(retorno.Documentos);
        Assert.Equal(TipoDocumentoDistribuido.NFeProcessada, documento.Tipo);
        Assert.Equal("procNFe_v4.00.xsd", documento.Schema);
        Assert.Equal(1L, documento.Nsu);
        Assert.Equal(nota.Chave, documento.Chave);
        Assert.Null(documento.ResumoNFe);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(documento.Xml));
        MesmaNfeProc(nota.XmlProcessado!, documento.Xml);
        var lida = new LeiauteNFe400().Ler(documento.Xml);
        Assert.DoesNotContain(lida.Problemas, p => p.Severidade == Severidade.Erro);
        Assert.Equal(nota.Chave, lida.Chave);
        Assert.Equal(nota.Protocolo!.Numero, lida.Protocolo!.Numero);
        Assert.Equal(CnpjA, lida.Nota!.Emitente!.Cnpj);
        Assert.Equal(CnpjC, lida.Nota.Destinatario!.Cnpj);
        Assert.Equal(CnpjB, Assert.Single(lida.Nota.AutorizadosXml).Cnpj);
        Assert.Empty(AssinaturaDigital.Verificar(documento.Xml, "NFe" + nota.Chave, CnpjA));
    }

    [Fact]
    public async Task OCancelamentoChegaAoDestinatarioComoResEventoEAoAutorizadoComoProcEventoNFe()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjC);
        var autorizado = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1, NotaTerceiro);
        var cancelado = await emitente.ExecutarAsync(new EnvioEvento(new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, Justificativa)), Ct);
        var evento = Assert.Single(cancelado.Retorno!.Eventos);
        Assert.Equal(135, evento.Status);

        var doDestinatario = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjC, 0));
        var doAutorizado = await Distribuir(autorizado, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        // O destinatário que não se manifestou: o resumo da nota e o cancelamento como resumo do evento.
        Assert.Equal([TipoDocumentoDistribuido.ResumoNFe, TipoDocumentoDistribuido.ResumoEvento], doDestinatario.Documentos.Select(d => d.Tipo));
        Assert.Equal([1L, 2L], doDestinatario.Documentos.Select(d => d.Nsu));
        var resumo = doDestinatario.Documentos[1].ResumoEvento!;
        Assert.Equal(TipoEvento.Cancelamento, resumo.Tipo);
        Assert.Equal(nota.Chave, resumo.Chave);
        Assert.Equal(CnpjA, resumo.Autor);
        Assert.Equal(1, resumo.Sequencia);
        Assert.Equal(evento.Protocolo, resumo.Protocolo);
        Assert.Equal("Cancelamento", resumo.Descricao[..12]);
        Assert.Equal(nota.Chave, doDestinatario.Documentos[1].Chave);
        Assert.Empty(SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso("resEvento_v1.01.xsd"), "215").Validar(doDestinatario.Documentos[1].Xml));

        // O autorizado: a nota inteira e o evento inteiro.
        Assert.Equal([TipoDocumentoDistribuido.NFeProcessada, TipoDocumentoDistribuido.EventoProcessado], doAutorizado.Documentos.Select(d => d.Tipo));
        var processado = doAutorizado.Documentos[1];
        Assert.Equal("procEventoNFe_v1.00.xsd", processado.Schema);
        Assert.Equal(nota.Chave, processado.Chave);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(processado.Xml));
        var (esperado, recebido) = (XElement.Parse(evento.XmlProcessado!), XElement.Parse(processado.Xml));
        Assert.Equal(esperado.Name, recebido.Name);
        foreach (var nome in new[] { "evento", "retEvento" })
        {
            Assert.Equal(esperado.Elements().Single(e => e.Name.LocalName == nome).ToString(SaveOptions.DisableFormatting),
                recebido.Elements().Single(e => e.Name.LocalName == nome).ToString(SaveOptions.DisableFormatting));
        }
    }

    // O consNSU e o consChNFe

    [Fact]
    public async Task OConsNsuTrazOMesmoDocumentoDoNsuEOsNsuQueNaoExistemSao137Ou589()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjC);
        var nota = await Autorizar(emitente, 1, NotaTerceiro);
        await emitente.ExecutarAsync(new EnvioEvento(new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, Justificativa)), Ct);

        var primeiro = await Distribuir(destinatario, DistribuicaoDFe.Nsu(CnpjC, 1));
        var segundo = await Distribuir(destinatario, DistribuicaoDFe.Nsu(CnpjC, 2));
        var acima = await Distribuir(destinatario, DistribuicaoDFe.Nsu(CnpjC, 3));

        Assert.Equal(138, primeiro.Status);
        Assert.Equal("Documento localizado", primeiro.Motivo);
        Assert.Equal(1L, Assert.Single(primeiro.Documentos).Nsu);
        Assert.Equal(TipoDocumentoDistribuido.ResumoNFe, primeiro.Documentos[0].Tipo);
        Assert.Equal(1L, primeiro.UltimoNsu);
        Assert.Equal(2L, primeiro.MaximoNsu);
        Assert.False(primeiro.TemMais);
        Assert.Null(primeiro.EsperaSugerida);
        Assert.Equal(TipoDocumentoDistribuido.ResumoEvento, Assert.Single(segundo.Documentos).Tipo);
        Assert.Equal(2L, segundo.Documentos[0].Nsu);
        Assert.Equal(589, acima.Status);
        Assert.Empty(acima.Documentos);
        Assert.Null(acima.EsperaSugerida);
    }

    [Fact]
    public async Task OConsNsuDeUmNsuQueNaoTemDocumentoE137SemEsperaDoUltNsu()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        await Autorizar(emitente, 1);
        await Autorizar(emitente, 2);

        // O NSU 0 não existe (o primeiro é o 1): 137, e a consulta pontual não bloqueia o distNSU seguinte.
        var inexistente = await Distribuir(Criar(conexoes, CnpjB), DistribuicaoDFe.Nsu(CnpjB, 0));
        var seguinte = await Distribuir(Criar(conexoes, CnpjB), DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(137, inexistente.Status);
        Assert.Null(inexistente.EsperaSugerida);
        Assert.True(inexistente.NenhumDocumento);
        Assert.Equal(138, seguinte.Status);
        Assert.Equal(2, seguinte.Documentos.Count);
    }

    [Fact]
    public async Task OConsChNFeTrazOResumoAoDestinatarioEONfeProcAoAutorizadoSemNsuNoDocZip()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var nota = await Autorizar(emitente, 1, NotaTerceiro);

        var doDestinatario = await Distribuir(Criar(conexoes, CnpjC), DistribuicaoDFe.Chave(CnpjC, nota.Chave!));
        var doAutorizado = await Distribuir(Criar(conexoes, CnpjB), DistribuicaoDFe.Chave(CnpjB, nota.Chave!));

        Assert.Equal(138, doDestinatario.Status);
        var resumo = Assert.Single(doDestinatario.Documentos);
        Assert.Null(resumo.Nsu);
        Assert.Equal(TipoDocumentoDistribuido.ResumoNFe, resumo.Tipo);
        Assert.Equal(nota.Chave, resumo.Chave);
        Assert.Equal(0L, doDestinatario.UltimoNsu);
        Assert.False(doDestinatario.TemMais);
        Assert.Null(doDestinatario.EsperaSugerida);
        var processada = Assert.Single(doAutorizado.Documentos);
        Assert.Null(processada.Nsu);
        Assert.Equal(TipoDocumentoDistribuido.NFeProcessada, processada.Tipo);
        MesmaNfeProc(nota.XmlProcessado!, processada.Xml);
    }

    [Fact]
    public async Task AsRejeicoesDoConsChNFe()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var viva = await Autorizar(emitente, 1, NotaTerceiro);
        var cancelada = await Autorizar(emitente, 2, NotaTerceiro);
        await emitente.ExecutarAsync(new EnvioEvento(new Cancelamento(cancelada.Chave!, cancelada.Protocolo!.Numero!, Justificativa)), Ct);
        var inexistente = emitente.Preparar(ApoioEmulado.Nota(Documento.NFe, 3, DateTimeOffset.UtcNow));
        Assert.True(inexistente.Preparado);

        var naoExiste = await Distribuir(Criar(conexoes, CnpjC), DistribuicaoDFe.Chave(CnpjC, inexistente.Chave!));
        var doEmitente = await Distribuir(emitente, DistribuicaoDFe.Chave(CnpjA, viva.Chave!));
        var semPermissao = await Distribuir(Criar(conexoes, "98765432000198"), DistribuicaoDFe.Chave("98765432000198", viva.Chave!));
        var canceladaConsultada = await Distribuir(Criar(conexoes, CnpjC), DistribuicaoDFe.Chave(CnpjC, cancelada.Chave!));

        Assert.Equal(217, naoExiste.Status);
        Assert.Equal(641, doEmitente.Status);
        Assert.Equal(640, semPermissao.Status);
        Assert.Equal(653, canceladaConsultada.Status);
        Assert.All([naoExiste, doEmitente, semPermissao, canceladaConsultada], r =>
        {
            Assert.Empty(r.Documentos);
            Assert.False(r.ConsumoIndevido);
        });
    }

    [Fact]
    public async Task AConsultaPorChaveDepoisDe90DiasE632()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Emulador.IniciarAsync(new OpcoesEmulador
        {
            Porta = 0,
            Pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N")),
            Relogio = relogio,
        }, Ct);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        var emitente = Criar(emulador, conexoes, CnpjA, relogio);
        var nota = await Autorizar(emitente, 1, NotaTerceiro, relogio);

        relogio.Avancar(TimeSpan.FromDays(91));
        var tarde = await Distribuir(Criar(emulador, conexoes, CnpjB, relogio), DistribuicaoDFe.Chave(CnpjB, nota.Chave!));
        var distribuicao = await Distribuir(Criar(emulador, conexoes, CnpjB, relogio), DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(632, tarde.Status);
        Assert.Empty(tarde.Documentos);
        // O que passou de 90 dias deixa de ser distribuído: o distNSU não traz mais a nota.
        Assert.Equal(137, distribuicao.Status);
    }

    // O certificado e o interessado

    [Fact]
    public async Task OCertificadoDeOutroCnpjBaseE593EODeCnpjParaCpfE472()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var destinatario = Criar(conexoes, CnpjB);

        var outroCnpj = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjC, 0));
        var cpf = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CpfD, 0));

        Assert.Equal(593, outroCnpj.Status);
        Assert.Contains("CNPJ-Base", outroCnpj.Motivo, StringComparison.Ordinal);
        Assert.Equal(472, cpf.Status);
        Assert.Empty(outroCnpj.Documentos);
        Assert.Null(outroCnpj.EsperaSugerida);
    }

    [Fact]
    public async Task AMatrizConsultaAsNotasDaFilialComOMesmoCertificado()
    {
        var filial = "114447770002" + OpenDFe.Cnpj.CalcularDigitos("114447770002");
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var nota = await Autorizar(emitente, 1, n =>
        {
            n.Destinatario!.Cnpj = filial;
            n.Transporte.Transportador!.Cnpj = CnpjC;
            n.AutorizadosXml.Clear();
        });

        var daFilial = await Distribuir(Criar(conexoes, CnpjB), DistribuicaoDFe.UltimoNsu(filial, 0));
        var daMatriz = await Distribuir(Criar(conexoes, CnpjB), DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(nota.Chave, Assert.Single(daFilial.Documentos).Chave);
        // Cada estabelecimento (as 14 posições) tem a sua sequência: a matriz não vê a nota da filial.
        Assert.Equal(137, daMatriz.Status);
    }

    [Fact]
    public async Task ODestinatarioPessoaFisicaConsultaPeloCpfComOCertificadoDele()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var nota = await Autorizar(emitente, 1, n =>
        {
            n.Destinatario!.Cnpj = null;
            n.Destinatario.Cpf = CpfD;
            n.Destinatario.IndicadorIE = IndicadorIEDestinatario.NaoContribuinte;
            n.Destinatario.InscricaoEstadual = null;
            n.Identificacao.ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal;
            n.Transporte.Transportador!.Cnpj = CnpjC;
            n.AutorizadosXml.Clear();
        });

        var retorno = await Distribuir(Criar(conexoes, CpfD), DistribuicaoDFe.UltimoNsu("529.982.247-25", 0, UF.SP));

        Assert.Equal(138, retorno.Status);
        Assert.Equal(nota.Chave, Assert.Single(retorno.Documentos).ResumoNFe!.Chave);
    }

    // O limite das consultas pontuais e o serviço paralisado

    [Fact]
    public async Task AVigesimaPrimeiraConsultaPontualE656ComUmaHoraDeEspera()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var destinatario = Criar(conexoes, CnpjC);
        for (var i = 1; i <= 20; i++)
        {
            var dentro = await Distribuir(destinatario, DistribuicaoDFe.Nsu(CnpjC, 0));
            Assert.Equal(137, dentro.Status);
        }

        var excedida = await Distribuir(destinatario, DistribuicaoDFe.Nsu(CnpjC, 0));

        Assert.Equal(656, excedida.Status);
        Assert.True(excedida.ConsumoIndevido);
        Assert.Contains("ultrapassou o limite de 20 consultas por hora", excedida.Motivo, StringComparison.Ordinal);
        Assert.Equal(TimeSpan.FromHours(1), excedida.EsperaSugerida);
    }

    [Theory]
    [InlineData("108")]
    [InlineData("109")]
    public async Task OServicoParalisadoDevolveOStatusDaParalisacao(string codigo)
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "AN", Codigo: codigo));
        using var conexoes = ApoioEmulado.Conexoes(_emulador);

        var retorno = await Distribuir(Criar(conexoes, CnpjB), DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(int.Parse(codigo, System.Globalization.CultureInfo.InvariantCulture), retorno.Status);
        Assert.Empty(retorno.Documentos);
        Assert.False(retorno.TemMais);
        Assert.Null(retorno.EsperaSugerida);
    }

    // A queda no meio

    [Fact]
    public async Task AConexaoDerrubadaDepoisDeProcessarFazARepeticaoCairNo656ComOUltNsuDoServidorEOsNsuSeRecuperamPeloConsNsu()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Emulador.IniciarAsync(new OpcoesEmulador
        {
            Porta = 0,
            Pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N")),
            Relogio = relogio,
        }, Ct);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        var emitente = Criar(emulador, conexoes, CnpjA, relogio);
        var destinatario = Criar(emulador, conexoes, CnpjB, relogio);
        var notas = new List<ResultadoNota>();
        for (var i = 1; i <= 3; i++)
            notas.Add(await Autorizar(emitente, i, relogio: relogio));
        emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "AN", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => destinatario.ExecutarAsync(DistribuicaoDFe.UltimoNsu(CnpjB, 0), Ct));
        Assert.True(falha.PodeTerSidoProcessado);
        var repeticao = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        // O servidor já devolveu até o 3 (a resposta se perdeu): a repetição com o mesmo ultNSU é fora da sequência.
        Assert.Equal(656, repeticao.Status);
        Assert.Equal(3L, repeticao.UltimoNsu);
        Assert.Equal(TimeSpan.FromHours(1), repeticao.EsperaSugerida);

        // Passada a hora, os NSU da resposta perdida se recuperam um a um (o consNSU, que é pontual).
        relogio.Avancar(repeticao.EsperaSugerida!.Value + TimeSpan.FromMinutes(1));
        var recuperados = new List<DocumentoDistribuido>();
        for (long nsu = 1; nsu <= repeticao.UltimoNsu; nsu++)
            recuperados.Add(Assert.Single((await Distribuir(destinatario, DistribuicaoDFe.Nsu(CnpjB, nsu))).Documentos));
        Assert.Equal(notas.Select(n => n.Chave), recuperados.Select(d => d.Chave));
        Assert.Equal([1L, 2L, 3L], recuperados.Select(d => d.Nsu));

        // E a distribuição continua do ultNSU que o servidor devolveu.
        var seguinte = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, repeticao.UltimoNsu!.Value));
        Assert.Equal(137, seguinte.Status);
    }

    [Fact]
    public async Task AConexaoDerrubadaAntesDeProcessarPermiteRepetirComOMesmoUltNsu()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        await Autorizar(emitente, 1);
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "AN", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => destinatario.ExecutarAsync(DistribuicaoDFe.UltimoNsu(CnpjB, 0), Ct));
        var repeticao = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.NotNull(falha.Rastro.XmlPedido);
        Assert.Equal(138, repeticao.Status);
        Assert.Single(repeticao.Documentos);
    }

    // A paginação

    [Fact]
    public async Task CinquentaEPoucasNotasVemEmDoisLotesComTemMaisNoPrimeiro()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var notas = new List<ResultadoNota>();
        for (var i = 1; i <= 55; i++)
            notas.Add(await Autorizar(emitente, i));

        var primeiro = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));
        var segundo = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, primeiro.UltimoNsu!.Value));

        Assert.Equal(138, primeiro.Status);
        Assert.Equal(50, primeiro.Documentos.Count);
        Assert.Equal((50L, 55L), (primeiro.UltimoNsu, primeiro.MaximoNsu));
        Assert.True(primeiro.TemMais);
        Assert.Null(primeiro.EsperaSugerida);
        Assert.Equal(138, segundo.Status);
        Assert.Equal(5, segundo.Documentos.Count);
        Assert.Equal((55L, 55L), (segundo.UltimoNsu, segundo.MaximoNsu));
        Assert.False(segundo.TemMais);
        Assert.Equal(TimeSpan.FromHours(1), segundo.EsperaSugerida);
        var todos = primeiro.Documentos.Concat(segundo.Documentos).ToList();
        Assert.Equal(Enumerable.Range(1, 55).Select(i => (long?)i), todos.Select(d => d.Nsu));
        Assert.Equal(notas.Select(n => n.Chave), todos.Select(d => d.Chave));
        Assert.All(todos, d => Assert.Equal(TipoDocumentoDistribuido.ResumoNFe, d.Tipo));
    }

    [Fact]
    public async Task AConsultaAoVoltarAoMesmoCaminhoDaNotaDoEmulador()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var nota = await Autorizar(emitente, 1);

        var consulta = await emitente.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);
        var distribuicao = await Distribuir(Criar(conexoes, CnpjB), DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal(consulta.Retorno!.Protocolo!.Numero, Assert.Single(distribuicao.Documentos).ResumoNFe!.Protocolo);
    }
}
