using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O NFeInutilizacao4 (MOC 7.0, Visão Geral, 5.3): a faixa inutilizada com o protocolo, as regras da Tabela 5-12
// (com a NT 2026.001) e a numeração inutilizada barrando a autorização (3B08-100, NT 2020.005).
public sealed class InutilizacaoTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public InutilizacaoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Controle.Limpar();
        _fixture.Emulador.Irregulares.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _fixture.Emulador.Controle.Limpar();
        _fixture.Emulador.Irregulares.Limpar();
    }

    private Task<XElement> InutilizarAsync(InutilizacaoTeste pedido, string destino = "MG") =>
        _fixture.Http.InutilizarAsync(_fixture.Url(destino, Servico.NomeInutilizacao), pedido.Gerar(), Ct);

    private Task<XElement> InutilizarAsync(string pedido, string destino = "MG") =>
        _fixture.Http.InutilizarAsync(_fixture.Url(destino, Servico.NomeInutilizacao), pedido, Ct);

    private static XElement Info(XElement retorno) => retorno.Element(Mensagens.NFe + "infInut")!;

    private static string Codigo(XElement retorno) => Info(retorno).Campo("cStat");

    // Homologada

    [Fact]
    public async Task InutilizaAFaixaComOProtocolo()
    {
        var pedido = InutilizacaoTeste.Nova(5);
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);

        var retorno = await InutilizarAsync(pedido);

        Assert.Equal(Mensagens.NFe + "retInutNFe", retorno.Name);
        Assert.Equal("4.00", retorno.Attribute("versao")?.Value);
        var info = Info(retorno);
        Assert.Null(info.Attribute("Id"));
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "ano", "CNPJ", "mod", "serie", "nNFIni", "nNFFin", "dhRecbto", "nProt"],
            info.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("2", "MG-EMULADA", "102", "Inutilização de número homologado", "31"),
            (info.Campo("tpAmb"), info.Campo("verAplic"), info.Campo("cStat"), info.Campo("xMotivo"), info.Campo("cUF")));
        Assert.Equal((pedido.Ano, pedido.Cnpj, "55", "1", pedido.Inicio.ToString(CultureInfo.InvariantCulture), pedido.Fim.ToString(CultureInfo.InvariantCulture)),
            (info.Campo("ano"), info.Campo("CNPJ"), info.Campo("mod"), info.Campo("serie"), info.Campo("nNFIni"), info.Campo("nNFFin")));
        // O protocolo (4.3.5): o tipo de autorizador, o cUF, o ano e 10 posições.
        Assert.Matches("^131" + DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3)).ToString("yy", CultureInfo.InvariantCulture) + "[0-9]{10}$", info.Campo("nProt"));
        var recebimento = DateTimeOffset.ParseExact(info.Campo("dhRecbto"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        Assert.InRange(recebimento, antes, DateTimeOffset.UtcNow.AddSeconds(1));
        Assert.Empty(Esquemas.Validar("retInutNFe_v4.00.xsd", retorno));
    }

    [Fact]
    public async Task InutilizaUmNumeroSoNaSvrs()
    {
        var faixa = InutilizacaoTeste.Nova();
        var pedido = faixa with { Uf = "42", Modelo = "65", Serie = "15" };

        var info = Info(await InutilizarAsync(pedido, "SVRS"));

        Assert.Equal(("102", "43", "SVRS-EMULADA", "65", "15"), (info.Campo("cStat"), info.Campo("cUF"), info.Campo("verAplic"), info.Campo("mod"), info.Campo("serie")));
        Assert.StartsWith("342", info.Campo("nProt"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task InutilizaDezMilNumeros()
    {
        Assert.Equal("102", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova(10_000))));
    }

    // Na base

    [Fact]
    public async Task RecusaAMesmaFaixaComOProtocoloDaAnterior()
    {
        var pedido = InutilizacaoTeste.Nova(3);
        var primeiro = Info(await InutilizarAsync(pedido));

        var info = Info(await InutilizarAsync(pedido with { Justificativa = "OUTRA JUSTIFICATIVA DO TESTE" }));

        Assert.Equal(("563", "Rejeição: Já existe pedido de Inutilização com a mesma faixa de inutilização"), (info.Campo("cStat"), info.Campo("xMotivo")));
        Assert.Equal(primeiro.Campo("nProt"), info.Campo("nProt"));
        Assert.Equal(pedido.Inicio.ToString(CultureInfo.InvariantCulture), info.Campo("nNFIni"));
        Assert.Empty(Esquemas.Validar("retInutNFe_v4.00.xsd", info.Parent!));
    }

    [Fact]
    public async Task RecusaAFaixaComUmNumeroJaInutilizado()
    {
        var pedido = InutilizacaoTeste.Nova(10);
        Assert.Equal("102", Codigo(await InutilizarAsync(pedido)));

        var retorno = await InutilizarAsync(pedido with { Inicio = pedido.Fim, Fim = pedido.Fim + 5 });

        Assert.Equal("256", Codigo(retorno));
        Assert.Null(Info(retorno).Element(Mensagens.NFe + "nProt"));
        Assert.Null(Info(retorno).Element(Mensagens.NFe + "nNFIni"));
    }

    [Fact]
    public async Task AFaixaDeOutraSerieModeloOuCnpjNaoSeCruza()
    {
        var pedido = InutilizacaoTeste.Nova(2);
        Assert.Equal("102", Codigo(await InutilizarAsync(pedido)));

        Assert.Equal("102", Codigo(await InutilizarAsync(pedido with { Serie = "2" })));
        Assert.Equal("102", Codigo(await InutilizarAsync(pedido with { Modelo = "65" })));
        Assert.Equal("102", Codigo(await InutilizarAsync(pedido with { Cnpj = "11222333000262" })));
    }

    [Fact]
    public async Task RecusaAFaixaComUmNumeroJaUsadoPorUmaNota()
    {
        var nota = new NotaTeste();
        var autorizada = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        Assert.Equal("100", autorizada.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("cStat"));

        var retorno = await InutilizarAsync(new InutilizacaoTeste { Inicio = nota.Numero, Fim = nota.Numero });

        Assert.Equal(("241", "Rejeição: Um número da faixa já foi utilizado"), (Codigo(retorno), Info(retorno).Campo("xMotivo")));
    }

    [Theory]
    [InlineData("1", "MG", "206")]
    [InlineData("6", "SVC-AN", "206")]
    [InlineData("9", "MG", "100")]
    public async Task AAutorizacaoDoNumeroInutilizadoERecusadaForaDaOffline(string tipoEmissao, string destino, string esperado)
    {
        var nota = new NotaTeste { Modelo = tipoEmissao == "9" ? "65" : "55", TipoEmissao = tipoEmissao };
        var inutilizacao = new InutilizacaoTeste { Modelo = nota.Modelo, Inicio = nota.Numero, Fim = nota.Numero };
        Assert.Equal("102", Codigo(await InutilizarAsync(inutilizacao)));

        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url(destino, Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);

        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
        Assert.Equal(esperado, protocolo.Campo("cStat"));
        if (esperado == "206")
            Assert.Equal("Rejeição: NF-e já está inutilizada na Base de Dados da SEFAZ", protocolo.Campo("xMotivo"));
    }

    [Fact]
    public async Task OControleDasNotasEsqueceAsFaixasEOsEventos()
    {
        var pedido = InutilizacaoTeste.Nova();
        Assert.Equal("102", Codigo(await InutilizarAsync(pedido)));
        var nota = new NotaTeste();
        var autorizada = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = autorizada.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt");
        await _fixture.Http.EnviarEventosAsync(_fixture.Url("MG", Servico.NomeRecepcaoEvento), [new EventoTeste { Chave = nota.Chave, Protocolo = protocolo }.Gerar()], Ct);
        Assert.Single(_fixture.Emulador.Repositorio.Eventos(nota.Chave));

        using (var resposta = await _fixture.Http.DeleteAsync(new Uri(_fixture.Emulador.Endereco, "_controle/notas"), Ct))
            Assert.Equal(System.Net.HttpStatusCode.NoContent, resposta.StatusCode);

        Assert.Empty(_fixture.Emulador.Repositorio.Eventos(nota.Chave));
        Assert.Equal("102", Codigo(await InutilizarAsync(pedido)));
    }

    [Fact]
    public async Task RecusaOEmitenteIrregular()
    {
        _fixture.Emulador.Irregulares.Cadastrar("11222333000181");

        Assert.Equal("240", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova())));
    }

    // As regras do pedido

    [Fact]
    public async Task RecusaOAmbienteDeProducao()
    {
        Assert.Equal("252", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Ambiente = "1" })));
    }

    [Fact]
    public async Task RecusaAUfQueOAutorizadorNaoAtende()
    {
        var retorno = await InutilizarAsync(InutilizacaoTeste.Nova() with { Uf = "42" });

        Assert.Equal(("250", "Rejeição: UF diverge da UF autorizadora"), (Codigo(retorno), Info(retorno).Campo("xMotivo")));
    }

    [Theory]
    [InlineData("910")]
    [InlineData("969")]
    [InlineData("970")]
    [InlineData("989")]
    public async Task RecusaASerieDoEmitenteCpfEADoPaa(string serie)
    {
        Assert.Equal("266", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Serie = serie })));
    }

    [Theory]
    [InlineData("990")]
    [InlineData("909")]
    public async Task AceitaAsOutrasSeries(string serie)
    {
        Assert.Equal("102", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Serie = serie })));
    }

    [Fact]
    public async Task RecusaOAnoFuturoEOAnteriorA2006()
    {
        var proximo = (NotaTeste.Agora().Year % 100 + 1).ToString("D2", CultureInfo.InvariantCulture);

        Assert.Equal("453", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Ano = proximo })));
        Assert.Equal("454", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Ano = "05" })));
        Assert.Equal("102", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Ano = "06" })));
    }

    [Fact]
    public async Task RecusaAFaixaInvertidaEAAcimaDoLimite()
    {
        var faixa = InutilizacaoTeste.Nova(10_001);

        Assert.Equal("224", Codigo(await InutilizarAsync(faixa with { Inicio = faixa.Fim, Fim = faixa.Inicio })));
        Assert.Equal("201", Codigo(await InutilizarAsync(faixa)));
    }

    [Fact]
    public async Task RecusaOIdQueNaoEAConcatenacaoDosCampos()
    {
        var pedido = InutilizacaoTeste.Nova();
        var id = "ID3125112223330001815500" + "2" + pedido.Inicio.ToString("D9", CultureInfo.InvariantCulture) + pedido.Fim.ToString("D9", CultureInfo.InvariantCulture);

        Assert.Equal("502", Codigo(await InutilizarAsync(pedido with { Id = id })));
    }

    [Fact]
    public async Task RecusaOCnpjComODigitoErrado()
    {
        // Sem regra própria na Tabela 5-12: o CNPJ que não existe não está no cadastro (I05).
        Assert.Equal("203", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Cnpj = "11222333000180" })));
    }

    [Fact]
    public async Task ConfereAAssinaturaEOTitular()
    {
        var pedido = InutilizacaoTeste.Nova();
        var alterado = pedido.Gerar().Replace("NUMERACAO PULADA NO TESTE", "NUMERACAO ALTERADA NO TESTE", StringComparison.Ordinal);

        Assert.Equal("297", Codigo(await InutilizarAsync(alterado)));
        Assert.Equal("213", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Certificado = Clientes.OutroCertificado })));
    }

    [Fact]
    public async Task AFilialInutilizaComOCertificadoDaMatriz()
    {
        Assert.Equal("102", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Cnpj = "11222333000343" })));
    }

    [Fact]
    public async Task RecusaARaizEOSchema()
    {
        Assert.Equal("516", Codigo(await InutilizarAsync(Mensagens.Status())));
        Assert.Equal("239", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova().Gerar().Replace("versao=\"4.00\"", "versao=\"3.10\"", StringComparison.Ordinal))));
        Assert.Equal("215", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova().Gerar().Replace("INUTILIZAR", "CANCELAR", StringComparison.Ordinal))));
    }

    [Fact]
    public async Task RespondeAParalisacaoDepoisDasRegrasDoPedido()
    {
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Servico.NomeInutilizacao, Codigo: "109"));

        Assert.Equal("252", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova() with { Ambiente = "1" })));
        Assert.Equal("109", Codigo(await InutilizarAsync(InutilizacaoTeste.Nova())));
    }
}
