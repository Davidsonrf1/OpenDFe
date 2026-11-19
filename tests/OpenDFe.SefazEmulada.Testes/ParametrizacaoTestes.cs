using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OpenDFe.Testes;

// A parametrização municipal do ADN emulada (E.8): as rotas sob /parametrizacao respondendo com a configuração dos
// municípios da E.7, as recusas do município, do serviço e da competência, e as falhas do destino ADN.
public sealed class ParametrizacaoTestes : IClassFixture<EmuladorFixture>
{
    private readonly EmuladorFixture _fixture;

    public ParametrizacaoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Municipios.Limpar();
        _fixture.Emulador.Controle.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<(HttpStatusCode Status, JsonElement Json)> ObterAsync(string caminho)
    {
        using var resposta = await _fixture.Http.GetAsync(new Uri(_fixture.Emulador.Parametrizacao, caminho), Ct);
        var texto = await resposta.Content.ReadAsStringAsync(Ct);
        return (resposta.StatusCode, texto.Length == 0 ? default : JsonDocument.Parse(texto).RootElement.Clone());
    }

    private void Cadastrar(Municipio municipio) => _fixture.Emulador.Municipios.Cadastrar(municipio);

    // O convênio

    [Fact]
    public async Task OMunicipioNaoConfiguradoEConveniadoComOEmissorNacional()
    {
        var (status, json) = await ObterAsync("3106200/convenio");

        Assert.Equal(HttpStatusCode.OK, status);
        var convenio = json.GetProperty("parametrosConvenio");
        Assert.True(convenio.GetProperty("aderenteAmbienteNacional").GetBoolean());
        Assert.True(convenio.GetProperty("aderenteEmissorNacional").GetBoolean());
    }

    [Fact]
    public async Task OConvenioInativoEOEmissorProprio()
    {
        Cadastrar(new Municipio(3550308, Convenio: SituacaoConvenio.Inativo, Emissor: EmissorMunicipio.Proprio));

        var (_, json) = await ObterAsync("3550308/convenio");

        var convenio = json.GetProperty("parametrosConvenio");
        Assert.False(convenio.GetProperty("aderenteAmbienteNacional").GetBoolean());
        Assert.False(convenio.GetProperty("aderenteEmissorNacional").GetBoolean());
    }

    [Fact]
    public async Task OMunicipioSemConvenioE404EmTodasAsRotas()
    {
        Cadastrar(new Municipio(3550308, Convenio: SituacaoConvenio.Inexistente));

        var (convenio, json) = await ObterAsync("3550308/convenio");
        var (aliquota, _) = await ObterAsync("3550308/01.01.01.000/11-19-2026/aliquota");

        Assert.Equal(HttpStatusCode.NotFound, convenio);
        Assert.Contains("3550308", json.GetProperty("mensagem").GetString(), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, aliquota);
    }

    [Theory]
    [InlineData("310620/convenio", "município")]
    [InlineData("9906200/convenio", "município")]
    [InlineData("3106200/010101/11-19-2026/aliquota", "serviço")]
    [InlineData("3106200/01.01.01.000/2026-11-19/aliquota", "competência")]
    [InlineData("3106200/13-01-2026/retencoes", "competência")]
    public async Task RecusaOParametroMalFormado(string caminho, string trecho)
    {
        var (status, json) = await ObterAsync(caminho);

        Assert.Equal(HttpStatusCode.BadRequest, status);
        Assert.Contains(trecho, json.GetProperty("mensagem").GetString(), StringComparison.Ordinal);
    }

    // As alíquotas e os regimes

    [Fact]
    public async Task AAliquotaDoServicoEAPadrao()
    {
        Cadastrar(new Municipio(3106200, AliquotaPadrao: 3.00m, Aliquotas: new Dictionary<string, decimal> { ["010101"] = 2.50m }));

        var (_, propria) = await ObterAsync("3106200/01.01.01.000/11-19-2026/aliquota");
        var (_, padrao) = await ObterAsync("3106200/01.02.01.000/11-19-2026/aliquota");
        var (_, historico) = await ObterAsync("3106200/01.01.01.000/historicoaliquotas");

        var item = Assert.Single(propria.GetProperty("aliquotas").GetProperty("01.01.01.000").EnumerateArray());
        Assert.Equal(2.50m, item.GetProperty("Aliq").GetDecimal());
        Assert.Equal("2022-07-01T00:00:00", item.GetProperty("DtIni").GetString());
        Assert.Equal(3.00m, padrao.GetProperty("aliquotas").GetProperty("01.02.01.000")[0].GetProperty("Aliq").GetDecimal());
        Assert.Equal(2.50m, Assert.Single(historico.GetProperty("aliquotas").GetProperty("01.01.01.000").EnumerateArray()).GetProperty("Aliq").GetDecimal());
    }

    [Fact]
    public async Task OsRegimesEspeciaisAdmitidos()
    {
        Cadastrar(new Municipio(3106200, RegimesEspeciais: [1, 6]));

        var (_, json) = await ObterAsync("3106200/01.01.01.000/11-19-2026/regimes_especiais");

        var regimes = json.GetProperty("regimesEspeciais").GetProperty("01.01.01.000");
        Assert.Equal(["1", "6"], regimes.EnumerateObject().Select(r => r.Name));
        Assert.Equal(1, regimes.GetProperty("6")[0].GetProperty("sit").GetInt32());
    }

    // As retenções e os benefícios

    [Fact]
    public async Task AsRetencoesComOsCpfsAutorizados()
    {
        Cadastrar(new Municipio(3106200, Retencoes: ["52998224725"]));

        var (_, json) = await ObterAsync("3106200/11-19-2026/retencoes");
        var (_, vazio) = await ObterAsync("3550308/11-19-2026/retencoes");

        var retencoes = json.GetProperty("retencoes");
        Assert.False(retencoes.GetProperty("art6").GetProperty("habilitado").GetBoolean());
        var responsavel = Assert.Single(Assert.Single(retencoes.GetProperty("retMun").EnumerateArray()).GetProperty("respTrib").EnumerateArray());
        Assert.Equal("52998224725", responsavel.GetProperty("insc").GetString());
        Assert.Equal(1, responsavel.GetProperty("tpInsc").GetInt32());
        Assert.Empty(vazio.GetProperty("retencoes").GetProperty("retMun").EnumerateArray());
    }

    [Fact]
    public async Task OBeneficioPeloNumero()
    {
        Cadastrar(new Municipio(3106200, Beneficios: [new BeneficioMunicipio("31062000000001", 4, 2.00m)]));

        var (status, json) = await ObterAsync("3106200/31062000000001/11-19-2026/beneficio");
        var (outro, _) = await ObterAsync("3106200/31062000000002/11-19-2026/beneficio");

        Assert.Equal(HttpStatusCode.OK, status);
        var beneficio = json.GetProperty("beneficio");
        Assert.Equal("31062000000001", beneficio.GetProperty("numBenef").GetString());
        Assert.Equal(4, beneficio.GetProperty("tpoBenef").GetInt32());
        Assert.Equal(2.00m, beneficio.GetProperty("aliqDiferenc").GetDecimal());
        Assert.Equal(HttpStatusCode.NotFound, outro);
    }

    // As falhas e a base

    [Fact]
    public async Task AParalisacaoDoAdnE503ENaoAfetaASefin()
    {
        using var falha = await _fixture.Http.PostAsJsonAsync(new Uri(_fixture.Emulador.Endereco, "_controle/falhas"),
            new { tipo = "paralisacao", destino = "ADN", vezes = 1 }, Ct);
        Assert.Equal(HttpStatusCode.Created, falha.StatusCode);

        var (status, _) = await ObterAsync("3106200/convenio");
        var (resposta, _) = await Sefin.ObterAsync(_fixture, $"nfse/{new string('1', 50)}");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        var pedido = _fixture.Emulador.Pedidos.Listar().Last(p => p.Destino == "ADN");
        Assert.Equal("ConsultaConvenio", pedido.Servico);
    }

    [Theory]
    [InlineData("{\"tipo\":\"atraso\",\"milissegundos\":10,\"destino\":\"ADN\",\"servico\":\"EmissaoNFSe\"}", "tem os serviços")]
    [InlineData("{\"tipo\":\"atraso\",\"milissegundos\":10,\"destino\":\"MG\",\"servico\":\"ConsultaConvenio\"}", "é do destino ADN")]
    [InlineData("{\"tipo\":\"loteProcessando\",\"destino\":\"ADN\"}", "não vale no ADN")]
    [InlineData("{\"tipo\":\"paralisacao\",\"destino\":\"ADN\",\"codigo\":\"108\"}", "do ADN não tem código")]
    public async Task RecusaAFalhaQueNaoServeNoAdn(string corpo, string trecho)
    {
        using var conteudo = new StringContent(corpo, System.Text.Encoding.UTF8, "application/json");

        using var resposta = await _fixture.Http.PostAsync(new Uri(_fixture.Emulador.Endereco, "_controle/falhas"), conteudo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(trecho, await resposta.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ABaseAparecaNaRaizENoControle()
    {
        using var raiz = JsonDocument.Parse(await _fixture.Http.GetStringAsync(_fixture.Emulador.Endereco, Ct));
        using var emulador = JsonDocument.Parse(await _fixture.Http.GetStringAsync(new Uri(_fixture.Emulador.Endereco, "_controle/emulador"), Ct));

        Assert.EndsWith("/parametrizacao/", raiz.RootElement.GetProperty("nfse").GetProperty("parametrizacao").GetString(), StringComparison.Ordinal);
        Assert.Equal(_fixture.Emulador.Parametrizacao.ToString(), emulador.RootElement.GetProperty("nfse").GetProperty("parametrizacao").GetString());
    }
}
