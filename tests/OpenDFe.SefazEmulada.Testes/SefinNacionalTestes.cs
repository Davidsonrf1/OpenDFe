using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// As consultas da Sefin emulada (pela chave e pela DPS), as falhas comandadas nas rotas novas, a marca EMULADA: no
// xInfComp e o controle dos municípios e das NFS-e (E.7).
public sealed class SefinNacionalTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public SefinNacionalTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Controle.Limpar();
        _fixture.Emulador.Municipios.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Uri Controle(string caminho) => new(_fixture.Emulador.Endereco, "_controle/" + caminho);

    public void Dispose() => _fixture.Emulador.Controle.Limpar();

    private async Task<string> EmitirAsync(DpsTeste? dps = null)
    {
        var (resposta, json) = await Sefin.EmitirAsync(_fixture, dps ?? new DpsTeste());
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return json.GetProperty("chaveAcesso").GetString()!;
    }

    private async Task ComandarAsync(object pedido)
    {
        using var resposta = await _fixture.Http.PostAsJsonAsync(Controle("falhas"), pedido, Ct);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // As consultas

    [Fact]
    public async Task ConsultaANFSePelaChave()
    {
        var chave = await EmitirAsync();

        var (resposta, json) = await Sefin.ObterAsync(_fixture, "nfse/" + chave);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(chave, json.GetProperty("chaveAcesso").GetString());
        Assert.Equal("NFS" + chave, Sefin.NFSe(json).Root!.Element((XNamespace)DpsTeste.Namespace + "infNFSe")!.Attribute("Id")!.Value);
        Assert.Equal(_fixture.Emulador.NFSe.Chave(chave)!.Xml, DpsTeste.Descompactar(json.GetProperty("nfseXmlGZipB64").GetString()!));
    }

    [Fact]
    public async Task ConsultaPelaChaveQueNaoExisteE404()
    {
        var (resposta, json) = await Sefin.ObterAsync(_fixture, "nfse/" + new string('1', 50));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("E0044", Sefin.Erro(json));
    }

    [Fact]
    public async Task ConsultaPelaChaveMalFormadaE400()
    {
        var (resposta, json) = await Sefin.ObterAsync(_fixture, "nfse/123");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("EMULADA-CONSULTA", Sefin.Erro(json));
    }

    [Fact]
    public async Task ConsultaAChavePelaDps()
    {
        var dps = new DpsTeste();
        var chave = await EmitirAsync(dps);

        var (resposta, json) = await Sefin.ObterAsync(_fixture, "dps/" + dps.IdComposto);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(dps.IdComposto, json.GetProperty("idDps").GetString());
        Assert.Equal(chave, json.GetProperty("chaveAcesso").GetString());
    }

    [Fact]
    public async Task ConsultaPelaDpsSemNFSeE404()
    {
        var (resposta, json) = await Sefin.ObterAsync(_fixture, "dps/" + new DpsTeste().IdComposto);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("EMULADA-DPS", Sefin.Erro(json));
    }

    [Fact]
    public async Task OHeadDaDpsDiz200Ou404SemCorpo()
    {
        var dps = new DpsTeste();
        await EmitirAsync(dps);

        using var existe = await _fixture.Http.SendAsync(new HttpRequestMessage(HttpMethod.Head, new Uri(_fixture.Emulador.SefinNacional, "dps/" + dps.IdComposto)), Ct);
        using var naoExiste = await _fixture.Http.SendAsync(new HttpRequestMessage(HttpMethod.Head, new Uri(_fixture.Emulador.SefinNacional, "dps/" + new DpsTeste().IdComposto)), Ct);

        Assert.Equal(HttpStatusCode.OK, existe.StatusCode);
        Assert.Empty(await existe.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(HttpStatusCode.NotFound, naoExiste.StatusCode);
    }

    // As falhas comandadas

    [Fact]
    public async Task AParalisacaoDaSefinResponde503()
    {
        await ComandarAsync(new { tipo = "paralisacao", destino = "SefinNacional", servico = "EmissaoNFSe", vezes = 1 });

        var (resposta, json) = await Sefin.EmitirAsync(_fixture, new DpsTeste());
        var (depois, _) = await Sefin.EmitirAsync(_fixture, new DpsTeste());

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        Assert.Equal("EMULADA-PARALISACAO", Sefin.Erro(json));
        Assert.Equal(HttpStatusCode.Created, depois.StatusCode);
    }

    [Fact]
    public async Task AFalhaDaNFeNaoValeNaSefin()
    {
        await ComandarAsync(new { tipo = "paralisacao", destino = "MG", vezes = 1 });

        var (resposta, _) = await Sefin.EmitirAsync(_fixture, new DpsTeste());

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task AConexaoDerrubadaDepoisDeixaANFSeParaAConsultaPelaDps()
    {
        await ComandarAsync(new { tipo = "conexaoDerrubada", destino = "SefinNacional", servico = "EmissaoNFSe", depois = true, vezes = 1 });
        var dps = new DpsTeste();

        await Assert.ThrowsAsync<HttpRequestException>(() => Sefin.EmitirAsync(_fixture, dps));

        var (resposta, json) = await Sefin.ObterAsync(_fixture, "dps/" + dps.IdComposto);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(_fixture.Emulador.NFSe.Chave(json.GetProperty("chaveAcesso").GetString()!));
        var guardado = _fixture.Emulador.Pedidos.Listar().Last(p => p.Servico == "EmissaoNFSe");
        Assert.Null(guardado.Status);
        Assert.Contains("NFS-e gerada", guardado.Situacao, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AConexaoDerrubadaAntesNaoGera()
    {
        await ComandarAsync(new { tipo = "conexaoDerrubada", destino = "SefinNacional", vezes = 1 });
        var dps = new DpsTeste();

        await Assert.ThrowsAsync<HttpRequestException>(() => Sefin.EmitirAsync(_fixture, dps));

        Assert.Null(_fixture.Emulador.NFSe.Dps(dps.IdComposto));
    }

    [Fact]
    public async Task OErroInternoComandadoE500()
    {
        await ComandarAsync(new { tipo = "falhaSoap", destino = "SefinNacional", servico = "ConsultaNFSe", vezes = 1 });

        var (resposta, json) = await Sefin.ObterAsync(_fixture, "nfse/" + new string('1', 50));

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("EMULADA-FALHA", Sefin.Erro(json));
    }

    [Fact]
    public async Task ARespostaIlegivelCortaOJson()
    {
        await ComandarAsync(new { tipo = "respostaIlegivel", destino = "SefinNacional", servico = "ConsultaDps", vezes = 1 });

        using var resposta = await _fixture.Http.GetAsync(new Uri(_fixture.Emulador.SefinNacional, "dps/" + new DpsTeste().IdComposto), Ct);
        var texto = await resposta.Content.ReadAsStringAsync(Ct);

        Assert.ThrowsAny<JsonException>(() => JsonDocument.Parse(texto));
    }

    [Theory]
    [InlineData("{\"tipo\":\"paralisacao\",\"destino\":\"SefinNacional\",\"codigo\":\"109\"}", "não tem código")]
    [InlineData("{\"tipo\":\"loteProcessando\",\"destino\":\"SefinNacional\"}", "não vale na Sefin")]
    [InlineData("{\"tipo\":\"atraso\",\"milissegundos\":10,\"destino\":\"SefinNacional\",\"servico\":\"NFeStatusServico4\"}", "tem os serviços")]
    [InlineData("{\"tipo\":\"atraso\",\"milissegundos\":10,\"destino\":\"MG\",\"servico\":\"EmissaoNFSe\"}", "é do destino SefinNacional")]
    public async Task RecusaAFalhaQueNaoServeNaSefin(string pedido, string motivo)
    {
        using var conteudo = new StringContent(pedido, System.Text.Encoding.UTF8, "application/json");
        using var resposta = await _fixture.Http.PostAsync(Controle("falhas"), conteudo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(motivo, await resposta.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    // A marca no xInfComp

    [Fact]
    public async Task AMarcaDeParalisacaoResponde503SoParaADps()
    {
        var (resposta, _) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Observacao = "Teste EMULADA:paralisacao fim" });
        var (outra, _) = await Sefin.EmitirAsync(_fixture, new DpsTeste());

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.Created, outra.StatusCode);
    }

    [Fact]
    public async Task AMarcaDeConexaoDerrubadaDepoisGeraANota()
    {
        var dps = new DpsTeste { Observacao = "EMULADA:conexaoDerrubada:depois" };

        await Assert.ThrowsAsync<HttpRequestException>(() => Sefin.EmitirAsync(_fixture, dps));

        Assert.NotNull(_fixture.Emulador.NFSe.Dps(dps.IdComposto));
    }

    [Theory]
    [InlineData("EMULADA:irregular:emitente")]
    [InlineData("EMULADA:loteProcessando")]
    [InlineData("EMULADA:paralisacao:108")]
    [InlineData("EMULADA:desconhecida")]
    [InlineData("EMULADA:atraso:5 EMULADA:falhaSoap")]
    public async Task AMarcaQueNaoServeE400(string marca)
    {
        var (resposta, json) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Observacao = marca });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("EMULADA-MARCA", Sefin.Erro(json));
    }

    // Os pedidos recentes e as URLs

    [Fact]
    public async Task GuardaOPedidoDaSefinComOTransmissor()
    {
        await Sefin.ObterAsync(_fixture, "nfse/" + new string('2', 50));

        var pedido = _fixture.Emulador.Pedidos.Listar()[^1];
        Assert.Equal(("SefinNacional", "ConsultaNFSe", 404), (pedido.Destino, pedido.Servico, pedido.Status));
        Assert.Equal("11222333000181", pedido.Transmissor);
        Assert.StartsWith("GET /SefinNacional/nfse/", _fixture.Emulador.Pedidos.Ler(pedido.Id)!.Pedido, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARaizListaABaseDaSefin()
    {
        using var json = JsonDocument.Parse(await _fixture.Http.GetStringAsync(_fixture.Emulador.Endereco, Ct));

        Assert.Equal(_fixture.Emulador.SefinNacional.ToString(), json.RootElement.GetProperty("nfse").GetProperty("sefinNacional").GetString());
        Assert.EndsWith("/SefinNacional/", _fixture.Emulador.SefinNacional.AbsoluteUri, StringComparison.Ordinal);
    }

    // O controle dos municípios e das NFS-e

    [Fact]
    public async Task CadastraTrocaLeETiraOMunicipio()
    {
        var municipio = new { codigo = 3550308, nome = "São Paulo", convenio = "ativo", emissor = "nacional", aliquotaPadrao = 5.00m,
            aliquotas = new Dictionary<string, decimal> { ["010101"] = 2.90m }, regimesEspeciais = new[] { 1, 6 },
            beneficios = new[] { new { numero = "35503080000001", tipo = 4, aliquota = 2.00m } } };

        using var criado = await _fixture.Http.PostAsJsonAsync(Controle("municipios"), municipio, Ct);
        using var trocado = await _fixture.Http.PostAsJsonAsync(Controle("municipios"), new { codigo = 3550308, paralisado = true }, Ct);
        using var lido = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle("municipios/3550308"), Ct));
        using var padrao = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle("municipios/3106200"), Ct));

        Assert.Equal(HttpStatusCode.Created, criado.StatusCode);
        using (var json = JsonDocument.Parse(await criado.Content.ReadAsStringAsync(Ct)))
            Assert.Equal(2.90m, json.RootElement.GetProperty("aliquotas").GetProperty("010101").GetDecimal());
        Assert.Equal(HttpStatusCode.OK, trocado.StatusCode);
        Assert.True(lido.RootElement.GetProperty("configurado").GetBoolean());
        Assert.True(lido.RootElement.GetProperty("municipio").GetProperty("paralisado").GetBoolean());
        Assert.Equal(2.00m, lido.RootElement.GetProperty("municipio").GetProperty("aliquotaPadrao").GetDecimal());
        Assert.False(padrao.RootElement.GetProperty("configurado").GetBoolean());
        Assert.Equal("ativo", padrao.RootElement.GetProperty("municipio").GetProperty("convenio").GetString());
        Assert.Equal("nacional", padrao.RootElement.GetProperty("municipio").GetProperty("emissor").GetString());
        Assert.Single(_fixture.Emulador.Municipios.Listar());

        using var tirado = await _fixture.Http.DeleteAsync(Controle("municipios/3550308"), Ct);
        using var deNovo = await _fixture.Http.DeleteAsync(Controle("municipios/3550308"), Ct);
        Assert.Equal(HttpStatusCode.NoContent, tirado.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deNovo.StatusCode);
    }

    [Theory]
    [InlineData("{\"codigo\":9900000}", "7 dígitos")]
    [InlineData("{\"codigo\":123}", "7 dígitos")]
    [InlineData("{\"codigo\":3106200,\"aliquotaPadrao\":5.5}", "de 0 a 5")]
    [InlineData("{\"codigo\":3106200,\"aliquotas\":{\"0101\":2}}", "6 dígitos")]
    [InlineData("{\"codigo\":3106200,\"regimesEspeciais\":[7]}", "regime especial")]
    [InlineData("{\"codigo\":3106200,\"retencoes\":[\"12345678901\"]}", "CPF")]
    [InlineData("{\"codigo\":3106200,\"beneficios\":[{\"numero\":\"1\",\"tipo\":1}]}", "14 dígitos")]
    [InlineData("{\"codigo\":3106200,\"beneficios\":[{\"numero\":\"12345678901234\",\"tipo\":4}]}", "precisa da alíquota")]
    [InlineData("{\"codigo\":3106200,\"beneficios\":[{\"numero\":\"12345678901234\",\"tipo\":1,\"aliquota\":2}]}", "só vale")]
    [InlineData("{\"codigo\":3106200,\"convenio\":\"talvez\"}", "")]
    public async Task RecusaOMunicipioInvalido(string corpo, string motivo)
    {
        using var conteudo = new StringContent(corpo, System.Text.Encoding.UTF8, "application/json");
        using var resposta = await _fixture.Http.PostAsync(Controle("municipios"), conteudo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(motivo, await resposta.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Empty(_fixture.Emulador.Municipios.Listar());
    }

    [Fact]
    public async Task ListaLeEEsqueceAsNFSe()
    {
        var chave = await EmitirAsync();

        using var lista = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle("nfse"), Ct));
        using var nota = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle("nfse/" + chave), Ct));

        Assert.Contains(lista.RootElement.EnumerateArray(), n => n.GetProperty("chave").GetString() == chave);
        Assert.False(lista.RootElement[0].TryGetProperty("xml", out _));
        Assert.StartsWith("<NFSe", nota.RootElement.GetProperty("xml").GetString(), StringComparison.Ordinal);

        using var esquecido = await _fixture.Http.DeleteAsync(Controle("nfse"), Ct);
        using var depois = await _fixture.Http.GetAsync(Controle("nfse/" + chave), Ct);
        Assert.Equal(HttpStatusCode.NoContent, esquecido.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, depois.StatusCode);
    }

    [Fact]
    public async Task ZerarTudoEsqueceOsMunicipiosEAsNFSe()
    {
        await EmitirAsync();
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Paralisado: true));

        using var resposta = await _fixture.Http.DeleteAsync(new Uri(_fixture.Emulador.Endereco, "_controle"), Ct);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Empty(_fixture.Emulador.Municipios.Listar());
        Assert.Empty(_fixture.Emulador.NFSe.Listar());
    }
}
