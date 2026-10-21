using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O NFeDistribuicaoDFe do AN (NT 2014.002 v1.40): o que cada ator recebe e quando, o NSU por interessado, os três
// tipos de consulta, o uso indevido (656) e as regras do pedido. A nota é de outra empresa (o OutroCertificado),
// para o destinatário ser o certificado dos testes. Cada teste começa com a base vazia (o NSU recomeça).
public sealed class DistribuicaoTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private const string Emitente = "12345678000195";
    private const string Destinatario = "11222333000181";

    // O e-CPF do destinatário pessoa física.
    private static readonly X509Certificate2 CertificadoCpf =
        X509CertificateLoader.LoadPkcs12(new CertificadoTeste { Cpf = "52998224725", Nome = "PESSOA DE TESTE" }.GerarPfx("teste"), "teste");

    private readonly EmuladorFixture _fixture;
    private readonly HttpClient _emitente;

    public DistribuicaoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _emitente = Clientes.Criar(fixture.Emulador.Autoridade, Clientes.OutroCertificado);
        Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        Limpar();
        _emitente.Dispose();
    }

    private void Limpar()
    {
        _fixture.Emulador.Controle.Limpar();
        _fixture.Emulador.Repositorio.Limpar();
        _fixture.Relogio.Avanco = TimeSpan.Zero;
    }

    private static string Cnpj(string raiz, string ordem)
    {
        var parte = raiz + ordem;
        var primeiro = NotaTeste.Modulo11(parte);
        return parte + primeiro + NotaTeste.Modulo11(parte + primeiro);
    }

    private static NotaTeste Nota(string? destinatario = Destinatario) =>
        new() { Cnpj = Emitente, Certificado = Clientes.OutroCertificado, Destinatario = destinatario };

    private async Task<(NotaTeste Nota, string Protocolo)> AutorizarAsync(NotaTeste? nota = null)
    {
        nota ??= Nota();
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
        Assert.Equal("100", protocolo.Campo("cStat"));
        return (nota, protocolo.Campo("nProt"));
    }

    private async Task EventoAsync(string destino, EventoTeste evento)
    {
        var retorno = await _fixture.Http.EnviarEventosAsync(_fixture.Url(destino, Servico.NomeRecepcaoEvento), [evento.Gerar()], Ct);
        Assert.Equal("135", retorno.Element(Mensagens.NFe + "retEvento")!.Element(Mensagens.NFe + "infEvento")!.Campo("cStat"));
    }

    private Task ManifestarAsync(NotaTeste nota, string tipo = EventoTeste.Ciencia) =>
        EventoAsync("AN", new EventoTeste { Chave = nota.Chave, Tipo = tipo, Justificativa = null });

    private Task CancelarAsync(NotaTeste nota, string protocolo) =>
        EventoAsync("MG", new EventoTeste { Chave = nota.Chave, Protocolo = protocolo, Cnpj = Emitente, Certificado = Clientes.OutroCertificado });

    private Task CorrigirAsync(NotaTeste nota, int sequencia = 1) =>
        EventoAsync("MG", new EventoTeste { Chave = nota.Chave, Tipo = EventoTeste.CartaCorrecao, Sequencia = sequencia, Cnpj = Emitente, Certificado = Clientes.OutroCertificado });

    private Task<XElement> DistribuirAsync(string pedido, HttpClient? http = null) =>
        (http ?? _fixture.Http).DistribuirAsync(_fixture.Url("AN", Servico.NomeDistribuicao), pedido, Ct);

    private Task<XElement> DistribuirAsync(long ultimo, string? cnpj = Destinatario, HttpClient? http = null) =>
        DistribuirAsync(Mensagens.Distribuicao(Mensagens.DistribuirNsu(ultimo), cnpj), http);

    // Os documentos do lote: o NSU, o schema e o XML descompactado.
    private static List<(string? Nsu, string Schema, XElement Xml)> Documentos(XElement retorno) =>
        [.. retorno.Element(Mensagens.NFe + "loteDistDFeInt")?.Elements(Mensagens.NFe + "docZip")
            .Select(d => (d.Attribute("NSU")?.Value, d.Attribute("schema")!.Value, Mensagens.Descompactar(d))) ?? []];

    private static string Nsu(long numero) => numero.ToString("D15", CultureInfo.InvariantCulture);

    // O que cada ator recebe

    [Fact]
    public async Task ODestinatarioRecebeOResumoDaNota()
    {
        var (nota, protocolo) = await AutorizarAsync();

        var retorno = await DistribuirAsync(0);

        Assert.Equal(Mensagens.NFe + "retDistDFeInt", retorno.Name);
        Assert.Equal("1.01", retorno.Attribute("versao")?.Value);
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "dhResp", "ultNSU", "maxNSU", "loteDistDFeInt"], retorno.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("2", "AN-EMULADA", "138", "Documento(s) localizado(s)", Nsu(1), Nsu(1)),
            (retorno.Campo("tpAmb"), retorno.Campo("verAplic"), retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("ultNSU"), retorno.Campo("maxNSU")));
        Assert.Empty(Esquemas.Validar("retDistDFeInt_v1.01.xsd", retorno));
        var (numero, schema, resumo) = Assert.Single(Documentos(retorno));
        Assert.Equal((Nsu(1), "resNFe_v1.01.xsd"), (numero, schema));
        Assert.Equal(["chNFe", "CNPJ", "xNome", "IE", "dhEmi", "tpNF", "vNF", "digVal", "dhRecbto", "nProt", "cSitNFe"], resumo.Elements().Select(e => e.Name.LocalName));
        Assert.Equal((nota.Chave, Emitente, "EMPRESA DE TESTE LTDA", "0623079040081", "1", "10.00", protocolo, "1"),
            (resumo.Campo("chNFe"), resumo.Campo("CNPJ"), resumo.Campo("xNome"), resumo.Campo("IE"), resumo.Campo("tpNF"), resumo.Campo("vNF"),
                resumo.Campo("nProt"), resumo.Campo("cSitNFe")));
        Assert.Empty(Esquemas.Validar("resNFe_v1.01.xsd", resumo));
    }

    [Theory]
    [InlineData(EventoTeste.Ciencia)]
    [InlineData(EventoTeste.Confirmacao)]
    [InlineData(EventoTeste.NaoRealizada)]
    public async Task AManifestacaoLiberaANotaAoDestinatarioEChegaAoEmitente(string tipo)
    {
        var (nota, _) = await AutorizarAsync();
        await EventoAsync("AN", new EventoTeste { Chave = nota.Chave, Tipo = tipo, Justificativa = "MERCADORIA RECUSADA NA ENTREGA" });

        var destinatario = await DistribuirAsync(0);
        var emitente = await DistribuirAsync(0, Emitente, _emitente);

        Assert.Equal(["resNFe_v1.01.xsd", "procNFe_v4.00.xsd"], Documentos(destinatario).Select(d => d.Schema));
        var processada = Documentos(destinatario)[1].Xml;
        Assert.Equal(Mensagens.NFe + "nfeProc", processada.Name);
        Assert.Equal(["NFe", "protNFe"], processada.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(XElement.Parse(nota.Gerar()).ToString(SaveOptions.DisableFormatting), processada.Element(Mensagens.NFe + "NFe")!.ToString(SaveOptions.DisableFormatting));
        Assert.Empty(Esquemas.Validar("procNFe_v4.00.xsd", processada));
        var (numero, schema, evento) = Assert.Single(Documentos(emitente));
        Assert.Equal((Nsu(1), "procEventoNFe_v1.00.xsd", tipo), (numero, schema, evento.Element(Mensagens.NFe + "evento")!.Element(Mensagens.NFe + "infEvento")!.Campo("tpEvento")));
        Assert.Empty(Esquemas.Validar("procEventoNFe_v1.00.xsd", evento));
    }

    [Fact]
    public async Task ODesconhecimentoNaoLiberaANota()
    {
        var (nota, _) = await AutorizarAsync();
        await ManifestarAsync(nota, EventoTeste.Desconhecimento);

        Assert.Equal(["resNFe_v1.01.xsd"], Documentos(await DistribuirAsync(0)).Select(d => d.Schema));
        Assert.Single(Documentos(await DistribuirAsync(0, Emitente, _emitente)));
    }

    [Fact]
    public async Task ASegundaManifestacaoNaoRepeteANota()
    {
        var (nota, _) = await AutorizarAsync();
        await ManifestarAsync(nota);
        await ManifestarAsync(nota, EventoTeste.Confirmacao);

        Assert.Equal(["resNFe_v1.01.xsd", "procNFe_v4.00.xsd"], Documentos(await DistribuirAsync(0)).Select(d => d.Schema));
        Assert.Equal(2, Documentos(await DistribuirAsync(0, Emitente, _emitente)).Count);
    }

    [Fact]
    public async Task OCancelamentoChegaComoResumoAntesDaManifestacao()
    {
        var (nota, protocolo) = await AutorizarAsync();
        await CancelarAsync(nota, protocolo);

        var documentos = Documentos(await DistribuirAsync(0));

        Assert.Equal(["resNFe_v1.01.xsd", "resEvento_v1.01.xsd"], documentos.Select(d => d.Schema));
        var resumo = documentos[1].Xml;
        Assert.Equal(["cOrgao", "CNPJ", "chNFe", "dhEvento", "tpEvento", "nSeqEvento", "xEvento", "dhRecbto", "nProt"], resumo.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("31", Emitente, nota.Chave, "110111", "1", "Cancelamento homologado"),
            (resumo.Campo("cOrgao"), resumo.Campo("CNPJ"), resumo.Campo("chNFe"), resumo.Campo("tpEvento"), resumo.Campo("nSeqEvento"), resumo.Campo("xEvento")));
        Assert.Matches("^131[0-9]{12}$", resumo.Campo("nProt"));
        Assert.Empty(Esquemas.Validar("resEvento_v1.01.xsd", resumo));
        Assert.Empty(Documentos(await DistribuirAsync(0, Emitente, _emitente)));
    }

    [Fact]
    public async Task DepoisDaManifestacaoOCancelamentoChegaInteiro()
    {
        var (nota, protocolo) = await AutorizarAsync();
        await ManifestarAsync(nota);
        await CancelarAsync(nota, protocolo);

        var documentos = Documentos(await DistribuirAsync(0));

        Assert.Equal(["resNFe_v1.01.xsd", "procNFe_v4.00.xsd", "procEventoNFe_v1.00.xsd"], documentos.Select(d => d.Schema));
        Assert.Equal("110111", documentos[2].Xml.Element(Mensagens.NFe + "evento")!.Element(Mensagens.NFe + "infEvento")!.Campo("tpEvento"));
    }

    [Fact]
    public async Task ACartaDeCorrecaoEsperaAManifestacao()
    {
        var (nota, _) = await AutorizarAsync();
        await CorrigirAsync(nota);
        Assert.Equal(["resNFe_v1.01.xsd"], Documentos(await DistribuirAsync(0)).Select(d => d.Schema));

        await ManifestarAsync(nota);
        await CorrigirAsync(nota, 2);

        var documentos = Documentos(await DistribuirAsync(1));
        Assert.Equal(["procNFe_v4.00.xsd", "procEventoNFe_v1.00.xsd", "procEventoNFe_v1.00.xsd"], documentos.Select(d => d.Schema));
        Assert.Equal(["1", "2"], documentos.Skip(1).Select(d => d.Xml.Element(Mensagens.NFe + "evento")!.Element(Mensagens.NFe + "infEvento")!.Campo("nSeqEvento")));
        Assert.Equal([Nsu(2), Nsu(3), Nsu(4)], documentos.Select(d => d.Nsu));
    }

    [Fact]
    public async Task OTransportadorEOsAutorizadosRecebemANotaEOsEventos()
    {
        var transportador = Cnpj("11222333", "0002");
        var autorizado = Cnpj("11222333", "0003");
        var (nota, protocolo) = await AutorizarAsync(Nota("98765432000198") with { Transportador = transportador, Autorizados = [autorizado, "52998224725"] });
        await CorrigirAsync(nota);
        await CancelarAsync(nota, protocolo);

        foreach (var documento in (string[])[transportador, autorizado])
        {
            var documentos = Documentos(await DistribuirAsync(0, documento));
            Assert.Equal(["procNFe_v4.00.xsd", "procEventoNFe_v1.00.xsd", "procEventoNFe_v1.00.xsd"], documentos.Select(d => d.Schema));
        }
        using var pessoa = Clientes.Criar(_fixture.Emulador.Autoridade, CertificadoCpf);
        var cpf = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.DistribuirNsu(0), cnpj: null, cpf: "52998224725"), pessoa);
        Assert.Equal(3, Documentos(cpf).Count);
        Assert.Empty(Documentos(await DistribuirAsync(0)));
    }

    [Fact]
    public async Task OAutorizadoRecebeAManifestacao()
    {
        var autorizado = Cnpj("11222333", "0003");
        var (nota, _) = await AutorizarAsync(Nota() with { Autorizados = [autorizado] });
        await ManifestarAsync(nota, EventoTeste.Confirmacao);

        Assert.Equal(["procNFe_v4.00.xsd", "procEventoNFe_v1.00.xsd"], Documentos(await DistribuirAsync(0, autorizado)).Select(d => d.Schema));
    }

    [Fact]
    public async Task OTransportadorQueEODestinatarioEsperaAManifestacao()
    {
        var (nota, _) = await AutorizarAsync(Nota() with { Transportador = Destinatario, Autorizados = [Destinatario] });

        Assert.Equal(["resNFe_v1.01.xsd"], Documentos(await DistribuirAsync(0)).Select(d => d.Schema));
    }

    [Fact]
    public async Task ANfceNaoEDistribuida()
    {
        await AutorizarAsync(Nota() with { Modelo = "65" });

        var retorno = await DistribuirAsync(0);

        Assert.Equal(("137", "Nenhum documento localizado", Nsu(0), Nsu(0)), (retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("ultNSU"), retorno.Campo("maxNSU")));
        Assert.Null(retorno.Element(Mensagens.NFe + "loteDistDFeInt"));
        Assert.Empty(Esquemas.Validar("retDistDFeInt_v1.01.xsd", retorno));
    }

    [Fact]
    public async Task CadaEstabelecimentoTemOSeuNsu()
    {
        var filial = Cnpj("11222333", "0002");
        await AutorizarAsync();
        await AutorizarAsync(Nota(filial));
        await AutorizarAsync();

        Assert.Equal((Nsu(2), Nsu(2)), Ultimos(await DistribuirAsync(0)));
        Assert.Equal((Nsu(1), Nsu(1)), Ultimos(await DistribuirAsync(0, filial)));
    }

    private static (string, string) Ultimos(XElement retorno) => (retorno.Campo("ultNSU"), retorno.Campo("maxNSU"));

    // O distNSU

    [Fact]
    public async Task OLoteTemAte50DocumentosNaOrdemDoNsu()
    {
        for (var i = 0; i < 51; i++)
            await AutorizarAsync();

        var primeiro = await DistribuirAsync(0);
        var segundo = await DistribuirAsync(50);

        Assert.Equal(50, Documentos(primeiro).Count);
        Assert.Equal([.. Enumerable.Range(1, 50).Select(n => Nsu(n))], Documentos(primeiro).Select(d => d.Nsu));
        Assert.Equal((Nsu(50), Nsu(51)), Ultimos(primeiro));
        Assert.Equal(Nsu(51), Assert.Single(Documentos(segundo)).Nsu);
        Assert.Equal((Nsu(51), Nsu(51)), Ultimos(segundo));
    }

    [Fact]
    public async Task ONsuAcimaDoMaiorE589()
    {
        await AutorizarAsync();

        var retorno = await DistribuirAsync(2);

        Assert.Equal(("589", "Rejeição: Número do NSU informado superior ao maior NSU do Ambiente Nacional", Nsu(1), Nsu(1)),
            (retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("ultNSU"), retorno.Campo("maxNSU")));
    }

    [Fact]
    public async Task ConsultarDeNovoNaHoraDepoisDo137E656()
    {
        Assert.Equal("137", (await DistribuirAsync(0)).Campo("cStat"));

        var repetida = await DistribuirAsync(0);
        _fixture.Relogio.Avanco = TimeSpan.FromMinutes(30);
        var bloqueada = await DistribuirAsync(0);
        _fixture.Relogio.Avanco = TimeSpan.FromMinutes(89);
        var ainda = await DistribuirAsync(0);
        _fixture.Relogio.Avanco = TimeSpan.FromMinutes(151);
        var liberada = await DistribuirAsync(0);

        Assert.Equal(("656", "Rejeição: Consumo Indevido. Deve ser aguardado 1 hora para efetuar nova solicitação caso não existam mais documentos a serem pesquisados. Tente após 1 hora"),
            (repetida.Campo("cStat"), repetida.Campo("xMotivo")));
        Assert.Equal(repetida.Campo("xMotivo"), bloqueada.Campo("xMotivo"));
        Assert.Equal("656", ainda.Campo("cStat"));
        Assert.Equal("137", liberada.Campo("cStat"));
        Assert.Empty(Esquemas.Validar("retDistDFeInt_v1.01.xsd", repetida));
    }

    [Fact]
    public async Task OUltNsuForaDaSequenciaE656ComOUltimoDevolvido()
    {
        await AutorizarAsync();
        await AutorizarAsync();
        var primeiro = await DistribuirAsync(0);
        Assert.Equal(Nsu(2), primeiro.Campo("ultNSU"));

        var fora = await DistribuirAsync(1);

        Assert.Equal(("656", "Rejeição: Consumo Indevido. Deve ser utilizado o ultNSU nas solicitações subsequentes. Tente após 1 hora", Nsu(2), Nsu(2)),
            (fora.Campo("cStat"), fora.Campo("xMotivo"), fora.Campo("ultNSU"), fora.Campo("maxNSU")));
        Assert.Equal("656", (await DistribuirAsync(2)).Campo("cStat"));
        _fixture.Relogio.Avanco = TimeSpan.FromMinutes(61);
        Assert.Equal("137", (await DistribuirAsync(2)).Campo("cStat"));
    }

    [Fact]
    public async Task ODocumentoDeMaisDe90DiasNaoEDistribuido()
    {
        await AutorizarAsync();

        _fixture.Relogio.Avanco = TimeSpan.FromDays(91);
        var retorno = await DistribuirAsync(0);

        Assert.Equal(("137", Nsu(1), Nsu(1)), (retorno.Campo("cStat"), retorno.Campo("ultNSU"), retorno.Campo("maxNSU")));
    }

    // O consNSU

    [Fact]
    public async Task OConsNsuDevolveODocumentoDoNsu()
    {
        await AutorizarAsync();
        await AutorizarAsync();

        var achado = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarNsu(2)));
        var inexistente = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarNsu(0)));
        var acima = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarNsu(3)));

        Assert.Equal(("138", "Documento localizado", Nsu(2), Nsu(2)), (achado.Campo("cStat"), achado.Campo("xMotivo"), achado.Campo("ultNSU"), achado.Campo("maxNSU")));
        Assert.Equal(Nsu(2), Assert.Single(Documentos(achado)).Nsu);
        Assert.Equal("137", inexistente.Campo("cStat"));
        Assert.Equal("589", acima.Campo("cStat"));
    }

    [Fact]
    public async Task AVigesimaPrimeiraConsultaPontualNaHoraE656()
    {
        var (nota, _) = await AutorizarAsync();
        for (var i = 0; i < 10; i++)
        {
            Assert.Equal("138", (await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarNsu(1)))).Campo("cStat"));
            Assert.Equal("138", (await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave)))).Campo("cStat"));
        }

        var excedida = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave)));

        Assert.Equal(("656", "Rejeição: Consumo Indevido - ultrapassou o limite de 20 consultas por hora"), (excedida.Campo("cStat"), excedida.Campo("xMotivo")));
        Assert.Equal("656", (await DistribuirAsync(0)).Campo("cStat"));
        _fixture.Relogio.Avanco = TimeSpan.FromMinutes(61);
        Assert.Equal("138", (await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarNsu(1)))).Campo("cStat"));
    }

    // O consChNFe

    [Fact]
    public async Task OConsChNFeDevolveOResumoAntesDaManifestacaoEANotaDepois()
    {
        var (nota, _) = await AutorizarAsync();

        var antes = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave)));
        await ManifestarAsync(nota);
        var depois = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave)));

        Assert.Equal(("138", "Documento localizado", Nsu(0), Nsu(1)), (antes.Campo("cStat"), antes.Campo("xMotivo"), antes.Campo("ultNSU"), antes.Campo("maxNSU")));
        var resumo = Assert.Single(Documentos(antes));
        Assert.Equal((null, "resNFe_v1.01.xsd"), (resumo.Nsu, resumo.Schema));
        Assert.Equal("procNFe_v4.00.xsd", Assert.Single(Documentos(depois)).Schema);
        Assert.Empty(Esquemas.Validar("retDistDFeInt_v1.01.xsd", antes));
    }

    [Fact]
    public async Task OConsChNFeDoTransportadorDevolveANota()
    {
        var transportador = Cnpj("11222333", "0002");
        var (nota, _) = await AutorizarAsync(Nota("98765432000198") with { Transportador = transportador });

        var retorno = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave), transportador));

        Assert.Equal("procNFe_v4.00.xsd", Assert.Single(Documentos(retorno)).Schema);
    }

    [Fact]
    public async Task OConsChNFeRecusaOEmitenteEQuemNaoEstaNaNota()
    {
        var (nota, _) = await AutorizarAsync(Nota("98765432000198"));

        var emitente = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave), Emitente), _emitente);
        var outro = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave)));

        Assert.Equal(("641", "Rejeição: NF-e indisponível para o emitente"), (emitente.Campo("cStat"), emitente.Campo("xMotivo")));
        Assert.Equal(("640", "Rejeição: CNPJ/CPF do interessado não possui permissão para consultar esta NF-e"), (outro.Campo("cStat"), outro.Campo("xMotivo")));
    }

    [Fact]
    public async Task OConsChNFeConfereAChaveEANota()
    {
        var (nota, protocolo) = await AutorizarAsync();
        var chave = nota.Chave;
        string Trocar(string parte, int inicio)
        {
            var outra = chave[..inicio] + parte + chave[(inicio + parte.Length)..43];
            return outra + NotaTeste.Modulo11(outra);
        }
        async Task<(string, string)> CodigoAsync(string consultada)
        {
            var retorno = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(consultada)));
            return (retorno.Campo("cStat"), retorno.Campo("xMotivo"));
        }

        Assert.Equal("236", (await CodigoAsync(chave[..43] + (chave[43] == '0' ? '1' : '0'))).Item1);
        Assert.Equal("614", (await CodigoAsync(Trocar("99", 0))).Item1);
        Assert.Equal("615", (await CodigoAsync(Trocar("05", 2))).Item1);
        Assert.Equal("616", (await CodigoAsync(Trocar("13", 4))).Item1);
        Assert.Equal(("618", "Rejeição: Chave de Acesso inválida (modelo diferente de 55)"), await CodigoAsync(Trocar("65", 20)));
        Assert.Equal("619", (await CodigoAsync(Trocar("000000000", 25))).Item1);
        Assert.Equal(("217", "Rejeição: NF-e inexistente para a chave de acesso informada"), await CodigoAsync(Trocar("87654321", 35)));

        await CancelarAsync(nota, protocolo);
        Assert.Equal(("653", "Rejeição: NF-e Cancelada, arquivo indisponível para download"), await CodigoAsync(chave));
    }

    [Fact]
    public async Task OConsChNFeDepoisDe90DiasE632()
    {
        var (nota, _) = await AutorizarAsync();

        _fixture.Relogio.Avanco = TimeSpan.FromDays(91);
        var retorno = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.ConsultarChave(nota.Chave)));

        Assert.Equal(("632", "Rejeição: Solicitação fora de prazo, a NF-e não está mais disponível para download"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    // O pedido

    [Fact]
    public async Task OInteressadoPessoaFisicaConsultaComOCpf()
    {
        await AutorizarAsync(Nota("52998224725"));
        using var pessoa = Clientes.Criar(_fixture.Emulador.Autoridade, CertificadoCpf);

        var retorno = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.DistribuirNsu(0), cnpj: null, cpf: "52998224725"), pessoa);
        var outro = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.DistribuirNsu(0), cnpj: null, cpf: "52998224725"));
        var cnpj = await DistribuirAsync(0, Destinatario, pessoa);

        Assert.Equal("138", retorno.Campo("cStat"));
        Assert.Equal(("472", "Rejeição: CPF consultado difere do CPF do Certificado Digital"), (outro.Campo("cStat"), outro.Campo("xMotivo")));
        Assert.Equal("593", cnpj.Campo("cStat"));
    }

    [Theory]
    [InlineData("1", Destinatario, null, "252")]
    [InlineData("2", "11222333000182", null, "489")]
    [InlineData("2", null, "52998224726", "490")]
    [InlineData("2", "98765432000198", null, "593")]
    public async Task ConfereOAmbienteEOInteressado(string ambiente, string? cnpj, string? cpf, string codigo)
    {
        var retorno = await DistribuirAsync(Mensagens.Distribuicao(Mensagens.DistribuirNsu(0), cnpj, cpf, ambiente));

        Assert.Equal((codigo, Nsu(0), Nsu(0)), (retorno.Campo("cStat"), retorno.Campo("ultNSU"), retorno.Campo("maxNSU")));
        Assert.Empty(Esquemas.Validar("retDistDFeInt_v1.01.xsd", retorno));
    }

    [Fact]
    public async Task AFilialConsultaComOCertificadoDaMatriz()
    {
        Assert.Equal("137", (await DistribuirAsync(0, Cnpj("11222333", "0002"))).Campo("cStat"));
    }

    [Theory]
    [InlineData("<distDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><tpAmb>2</tpAmb><CNPJ>11222333000181</CNPJ></distDFeInt>", "215")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"/>", "516")]
    [InlineData("<distDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\"><tpAmb>2</tpAmb></distDFeInt>", "517")]
    [InlineData("<distDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\"><tpAmb>2</tpAmb></distDFeInt>", "239")]
    [InlineData("<n:distDFeInt xmlns:n=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><n:tpAmb>2</n:tpAmb></n:distDFeInt>", "404")]
    public async Task ConfereARaizEOSchema(string pedido, string codigo)
    {
        Assert.Equal(codigo, (await DistribuirAsync(pedido)).Campo("cStat"));
    }

    [Fact]
    public async Task OLimiteEDe10Kb()
    {
        var pedido = "<!--" + new string('x', 10 * 1024) + "-->" + Mensagens.Distribuicao(Mensagens.DistribuirNsu(0));

        Assert.Equal("214", (await DistribuirAsync(pedido)).Campo("cStat"));
    }

    [Fact]
    public async Task OPedidoSemAOperacaoNoBodyEFalhaSoap()
    {
        using var resposta = await _fixture.Http.EnviarAsync(_fixture.Url("AN", Servico.NomeDistribuicao),
            Mensagens.Envelope(Mensagens.Distribuicao(Mensagens.DistribuirNsu(0)), Servico.NomeDistribuicao), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("soap:Sender", Mensagens.Falha(await resposta.Content.ReadAsStringAsync(Ct)));
    }

    [Fact]
    public async Task AParalisacaoVemDepoisDasRegrasDoPedido()
    {
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "AN", Servico.NomeDistribuicao, Codigo: "109"));

        Assert.Equal("252", (await DistribuirAsync(Mensagens.Distribuicao(Mensagens.DistribuirNsu(0), ambiente: "1"))).Campo("cStat"));
        Assert.Equal("109", (await DistribuirAsync(0)).Campo("cStat"));
    }

    // O controle

    [Fact]
    public async Task OControleListaADistribuicaoEDeleteNotasEsquece()
    {
        var (nota, _) = await AutorizarAsync();
        var controle = new Uri(_fixture.Emulador.Endereco, "_controle/distribuicao");

        using (var lista = await _fixture.Http.GetAsync(controle, Ct))
        {
            using var json = JsonDocument.Parse(await lista.Content.ReadAsStringAsync(Ct));
            var interessado = Assert.Single(json.RootElement.EnumerateArray());
            Assert.Equal(Destinatario, interessado.GetProperty("interessado").GetString());
            var documento = Assert.Single(interessado.GetProperty("documentos").EnumerateArray());
            Assert.Equal((1, nota.Chave, "resNFe_v1.01.xsd"),
                (documento.GetProperty("nsu").GetInt64(), documento.GetProperty("chave").GetString(), documento.GetProperty("schema").GetString()));
        }
        Assert.Equal("138", (await DistribuirAsync(0)).Campo("cStat"));

        using (var apagado = await _fixture.Http.DeleteAsync(new Uri(_fixture.Emulador.Endereco, "_controle/notas"), Ct))
            Assert.Equal(HttpStatusCode.NoContent, apagado.StatusCode);
        Assert.Equal("137", (await DistribuirAsync(0)).Campo("cStat"));
    }
}
