using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml;

namespace OpenDFe.Testes;

// As falhas comandadas pela rota /_controle (PROJETO.md 13.5): a paralisação (108, 109) e as de transporte.
public sealed class FalhasTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public FalhasTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Controle.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Uri Controle(string caminho = "") => new(_fixture.Emulador.Endereco, "_controle/falhas" + caminho);

    public void Dispose() => _fixture.Emulador.Controle.Limpar();

    private async Task<int> ComandarAsync(object pedido)
    {
        using var resposta = await _fixture.Http.PostAsJsonAsync(Controle(), pedido, Ct);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(Ct));
        var id = json.RootElement.GetProperty("id").GetInt32();
        Assert.Equal(Controle("/" + id), resposta.Headers.Location is { IsAbsoluteUri: false } local ? new Uri(_fixture.Emulador.Endereco, local) : resposta.Headers.Location);
        return id;
    }

    // Paralisação

    [Fact]
    public async Task ParalisaMomentaneamentePorPadrao()
    {
        await ComandarAsync(new { tipo = "paralisacao", destino = "MG" });

        var retorno = await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct);

        Assert.Equal("108", retorno.Campo("cStat"));
        Assert.Equal("Serviço Paralisado Momentaneamente (curto prazo)", retorno.Campo("xMotivo"));
        Assert.Null(retorno.Element(Mensagens.NFe + "tMed"));
        Assert.Null(retorno.Element(Mensagens.NFe + "xObs"));
    }

    [Fact]
    public async Task ParalisaSemPrevisaoComAObservacao()
    {
        await ComandarAsync(new { tipo = "paralisacao", destino = "SVRS", servico = "NFeStatusServico4", codigo = "109", observacao = "Manutenção programada" });

        var retorno = await _fixture.Http.ConsultarStatusAsync(_fixture.Url("SVRS"), Ct, uf: "42");

        Assert.Equal("109", retorno.Campo("cStat"));
        Assert.Equal("Serviço Paralisado sem Previsão", retorno.Campo("xMotivo"));
        Assert.Equal("42", retorno.Campo("cUF"));
        Assert.Equal("Manutenção programada", retorno.Campo("xObs"));
    }

    [Fact]
    public async Task ParalisaSoODestinoComandado()
    {
        await ComandarAsync(new { tipo = "paralisacao", destino = "MG" });

        Assert.Equal("107", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("SVC-AN"), Ct)).Campo("cStat"));
        Assert.Equal("108", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct)).Campo("cStat"));
    }

    [Fact]
    public async Task ParalisaAsVezesComandadasEVolta()
    {
        await ComandarAsync(new { tipo = "paralisacao", vezes = 2 });

        Assert.Equal("108", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct)).Campo("cStat"));
        Assert.Equal("108", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("SP"), Ct, uf: "35")).Campo("cStat"));
        Assert.Equal("107", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct)).Campo("cStat"));
    }

    [Fact]
    public async Task AsRejeicoesDoPedidoVemAntesDaParalisacao()
    {
        await ComandarAsync(new { tipo = "paralisacao" });

        Assert.Equal("252", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct, ambiente: "1")).Campo("cStat"));
    }

    // Transporte

    [Fact]
    public async Task AtrasaEDepoisResponde()
    {
        await ComandarAsync(new { tipo = "atraso", milissegundos = 400, vezes = 1 });
        var cronometro = Stopwatch.StartNew();

        var retorno = await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct);

        Assert.True(cronometro.ElapsedMilliseconds >= 390, $"Respondeu em {cronometro.ElapsedMilliseconds} ms.");
        Assert.Equal("107", retorno.Campo("cStat"));
    }

    [Fact]
    public async Task SeguraOPedidoAteOClienteDesistir()
    {
        await ComandarAsync(new { tipo = "tempoEsgotado", vezes = 1 });
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        limite.CancelAfter(TimeSpan.FromMilliseconds(800));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), limite.Token));

        Assert.Equal("107", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct)).Campo("cStat"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DerrubaAConexaoSemResposta(bool depois)
    {
        await ComandarAsync(new { tipo = "conexaoDerrubada", depois, vezes = 1 });

        await Assert.ThrowsAsync<HttpRequestException>(() => _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct));

        Assert.Equal("107", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct)).Campo("cStat"));
    }

    [Fact]
    public async Task CortaARespostaAoMeio()
    {
        await ComandarAsync(new { tipo = "respostaIlegivel", vezes = 1 });

        using var resposta = await _fixture.Http.EnviarAsync(_fixture.Url("MG"), Mensagens.Envelope(Mensagens.Status()), Ct);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var texto = await resposta.Content.ReadAsStringAsync(Ct);
        Assert.StartsWith("<?xml", texto, StringComparison.Ordinal);
        Assert.Throws<XmlException>(() => Mensagens.Retorno(texto));
    }

    [Fact]
    public async Task RespondeAFalhaSoapReceiver()
    {
        await ComandarAsync(new { tipo = "falhaSoap", destino = "MG", vezes = 1 });

        using var resposta = await _fixture.Http.EnviarAsync(_fixture.Url("MG"), Mensagens.Envelope(Mensagens.Status()), Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("soap:Receiver", Mensagens.Falha(await resposta.Content.ReadAsStringAsync(Ct)));
    }

    [Fact]
    public async Task ATransporteEAParalisacaoValemJuntas()
    {
        await ComandarAsync(new { tipo = "atraso", milissegundos = 50, vezes = 1 });
        await ComandarAsync(new { tipo = "paralisacao", codigo = "109", vezes = 1 });

        Assert.Equal("109", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct)).Campo("cStat"));
        Assert.Empty(_fixture.Emulador.Controle.Listar());
    }

    // A rota

    [Fact]
    public async Task ListaERemoveAsFalhas()
    {
        var primeira = await ComandarAsync(new { tipo = "paralisacao", destino = "MG" });
        var segunda = await ComandarAsync(new { tipo = "atraso", milissegundos = 10 });

        using (var lista = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle(), Ct)))
        {
            Assert.Equal([primeira, segunda], lista.RootElement.EnumerateArray().Select(f => f.GetProperty("id").GetInt32()));
            var paralisacao = lista.RootElement[0];
            Assert.Equal("paralisacao", paralisacao.GetProperty("tipo").GetString());
            Assert.Equal("MG", paralisacao.GetProperty("destino").GetString());
            Assert.Equal("108", paralisacao.GetProperty("codigo").GetString());
            Assert.False(paralisacao.TryGetProperty("restantes", out _));
        }

        using (var removida = await _fixture.Http.DeleteAsync(Controle("/" + primeira), Ct))
            Assert.Equal(HttpStatusCode.NoContent, removida.StatusCode);
        using (var inexistente = await _fixture.Http.DeleteAsync(Controle("/" + primeira), Ct))
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        Assert.Equal([segunda], _fixture.Emulador.Controle.Listar().Select(f => f.Id));

        using (var todas = await _fixture.Http.DeleteAsync(Controle(), Ct))
            Assert.Equal(HttpStatusCode.NoContent, todas.StatusCode);
        Assert.Empty(_fixture.Emulador.Controle.Listar());
    }

    [Theory]
    [InlineData("{\"tipo\":\"atraso\"}", "O atraso precisa de milissegundos maior que zero.")]
    [InlineData("{\"tipo\":\"paralisacao\",\"destino\":\"XX\"}", "Destino desconhecido: XX.")]
    [InlineData("{\"tipo\":\"paralisacao\",\"codigo\":\"107\"}", "A paralisação é 108 ou 109.")]
    public async Task RecusaOPedidoErrado(string json, string mensagem)
    {
        using var conteudo = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        using var resposta = await _fixture.Http.PostAsync(Controle(), conteudo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        using var erro = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(Ct));
        Assert.Equal(mensagem, erro.RootElement.GetProperty("erro").GetString());
        Assert.Empty(_fixture.Emulador.Controle.Listar());
    }

    [Fact]
    public async Task RecusaOTipoDesconhecido()
    {
        using var conteudo = new StringContent("{\"tipo\":\"terremoto\"}", System.Text.Encoding.UTF8, "application/json");

        using var resposta = await _fixture.Http.PostAsync(Controle(), conteudo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
