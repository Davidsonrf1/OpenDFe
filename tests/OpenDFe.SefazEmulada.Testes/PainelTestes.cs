using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O emulador com o painel, numa porta livre, guardando os 5 últimos pedidos.
public sealed class PainelFixture : EmuladorFixture
{
    private protected override OpcoesEmulador Configurar(OpcoesEmulador opcoes) => opcoes with { Painel = 0, Pedidos = 5 };
}

// O painel (E.6): a porta em HTTP só com a página e o /_controle, os pedidos recentes com o XML, o fluxo ao vivo e as
// rotas de controle novas (o emulador, o ac.crt, os autorizadores, os eventos, a nota com o XML, zerar tudo).
public sealed class PainelTestes : IClassFixture<PainelFixture>, IDisposable
{
    private readonly PainelFixture _fixture;
    private readonly HttpClient _painel;

    public PainelTestes(PainelFixture fixture)
    {
        _fixture = fixture;
        _painel = new HttpClient { BaseAddress = fixture.Emulador.Painel, Timeout = TimeSpan.FromSeconds(30) };
        Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        Limpar();
        _painel.Dispose();
    }

    private void Limpar()
    {
        _fixture.Emulador.Controle.Limpar();
        _fixture.Emulador.Repositorio.Limpar();
        _fixture.Emulador.Pedidos.Limpar();
    }

    private Task<XElement> StatusAsync() => _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct);

    private async Task<JsonElement> JsonAsync(string caminho)
    {
        using var resposta = await _painel.GetAsync(caminho, Ct);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(Ct));
        return json.RootElement.Clone();
    }

    // A porta do painel

    [Fact]
    public async Task APortaDoPainelEHttpNoLoopback()
    {
        Assert.Equal(Uri.UriSchemeHttp, _fixture.Emulador.Painel!.Scheme);
        Assert.Equal("127.0.0.1", _fixture.Emulador.Painel.Host);
        Assert.NotEqual(_fixture.Emulador.Endereco.Port, _fixture.Emulador.Painel.Port);
        using var resposta = await _painel.GetAsync("/_controle/falhas", Ct);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task ServeAPaginaComACsp()
    {
        using var resposta = await _painel.GetAsync("/", Ct);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("text/html", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<title>SEFAZ emulada</title>", await resposta.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        var csp = Assert.Single(resposta.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("script-src 'self'", csp, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", csp, StringComparison.Ordinal);
        Assert.Equal("nosniff", Assert.Single(resposta.Headers.GetValues("X-Content-Type-Options")));
        Assert.True(resposta.Headers.CacheControl?.NoStore);
    }

    [Theory]
    [InlineData("/painel.js", "text/javascript")]
    [InlineData("/painel.css", "text/css")]
    public async Task ServeOsArquivosDaPagina(string caminho, string tipo)
    {
        using var resposta = await _painel.GetAsync(caminho, Ct);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(tipo, resposta.Content.Headers.ContentType?.MediaType);
        Assert.NotEmpty(await resposta.Content.ReadAsByteArrayAsync(Ct));
    }

    [Fact]
    public async Task APortaDoPainelNaoServeOsWebServices()
    {
        using var servico = await _painel.PostAsync("/MG/NFeStatusServico4", new StringContent(Mensagens.Envelope(Mensagens.Status())), Ct);
        using var outro = await _painel.GetAsync("/qualquer", Ct);

        Assert.Equal(HttpStatusCode.NotFound, servico.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, outro.StatusCode);
    }

    [Theory]
    [InlineData("exemplo.com", HttpStatusCode.Forbidden)]
    [InlineData("127.0.0.1.nip.io", HttpStatusCode.Forbidden)]
    [InlineData("localhost", HttpStatusCode.OK)]
    public async Task OHostTemDeSerOLoopback(string host, HttpStatusCode esperado)
    {
        using var pedido = new HttpRequestMessage(HttpMethod.Get, "/_controle/falhas");
        pedido.Headers.Host = $"{host}:{_fixture.Emulador.Painel!.Port}";

        using var resposta = await _painel.SendAsync(pedido, Ct);

        Assert.Equal(esperado, resposta.StatusCode);
    }

    [Fact]
    public async Task APortaDoTlsNaoServeAPagina()
    {
        using var pagina = await _fixture.Http.GetAsync(new Uri(_fixture.Emulador.Endereco, "painel.js"), Ct);
        using var raiz = await _fixture.Http.GetAsync(_fixture.Emulador.Endereco, Ct);

        Assert.Equal(HttpStatusCode.NotFound, pagina.StatusCode);
        Assert.Equal("application/json", raiz.Content.Headers.ContentType?.MediaType);
    }

    // Os pedidos recentes

    [Fact]
    public async Task GuardaOPedidoComOXml()
    {
        await StatusAsync();

        var lista = await JsonAsync("/_controle/pedidos");
        var resumo = Assert.Single(lista.EnumerateArray());
        Assert.Equal(("MG", "NFeStatusServico4", 200, "cStat 107", "11222333000181"),
            (resumo.GetProperty("destino").GetString(), resumo.GetProperty("servico").GetString(), resumo.GetProperty("status").GetInt32(),
                resumo.GetProperty("situacao").GetString(), resumo.GetProperty("transmissor").GetString()));
        Assert.False(resumo.TryGetProperty("pedido", out _));
        var pedido = await JsonAsync($"/_controle/pedidos/{resumo.GetProperty("id").GetInt64()}");
        Assert.Contains("<consStatServ", pedido.GetProperty("pedido").GetString(), StringComparison.Ordinal);
        Assert.Contains("<retConsStatServ", pedido.GetProperty("resposta").GetString(), StringComparison.Ordinal);
        Assert.Equal(pedido.GetProperty("pedido").GetString()!.Length, resumo.GetProperty("tamanhoPedido").GetInt32());
    }

    [Fact]
    public async Task AFilaGuardaOsUltimosN()
    {
        for (var i = 0; i < 7; i++)
            await StatusAsync();

        var ids = (await JsonAsync("/_controle/pedidos")).EnumerateArray().Select(p => p.GetProperty("id").GetInt64()).ToList();
        var depois = (await JsonAsync($"/_controle/pedidos?depois={ids[2]}")).EnumerateArray().Select(p => p.GetProperty("id").GetInt64());

        Assert.Equal(5, ids.Count);
        Assert.Equal(Enumerable.Range(0, 5).Select(i => ids[0] + i), ids);
        Assert.Equal(ids.Skip(3), depois);
        using var antigo = await _painel.GetAsync($"/_controle/pedidos/{ids[0] - 1}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, antigo.StatusCode);
    }

    [Fact]
    public async Task GuardaO404EAQuedaComARespostaQueSairia()
    {
        using (var inexistente = await _fixture.Http.EnviarAsync(_fixture.Url("XX"), Mensagens.Envelope(Mensagens.Status()), Ct))
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Depois: true, Vezes: 1));
        await Assert.ThrowsAnyAsync<HttpRequestException>(() => StatusAsync());

        var pedidos = (await JsonAsync("/_controle/pedidos")).EnumerateArray().ToList();
        Assert.StartsWith("HTTP 404", pedidos[0].GetProperty("situacao").GetString(), StringComparison.Ordinal);
        Assert.Equal(404, pedidos[0].GetProperty("status").GetInt32());
        Assert.False(pedidos[1].TryGetProperty("status", out _));
        Assert.Contains("conexão derrubada", pedidos[1].GetProperty("situacao").GetString(), StringComparison.Ordinal);
        var queda = await JsonAsync($"/_controle/pedidos/{pedidos[1].GetProperty("id").GetInt64()}");
        Assert.Contains("<cStat>107</cStat>", queda.GetProperty("resposta").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task EsqueceOsPedidos()
    {
        await StatusAsync();

        using (var apagado = await _painel.DeleteAsync("/_controle/pedidos", Ct))
            Assert.Equal(HttpStatusCode.NoContent, apagado.StatusCode);

        Assert.Equal(0, (await JsonAsync("/_controle/pedidos")).GetArrayLength());
    }

    [Fact]
    public async Task OFluxoAvisaOPedidoEOEstado()
    {
        using var tempo = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        tempo.CancelAfter(TimeSpan.FromSeconds(20));
        using var resposta = await _painel.GetAsync("/_controle/pedidos/fluxo", HttpCompletionOption.ResponseHeadersRead, tempo.Token);
        Assert.Equal("text/event-stream", resposta.Content.Headers.ContentType?.MediaType);
        using var leitor = new StreamReader(await resposta.Content.ReadAsStreamAsync(tempo.Token));
        Assert.Equal(": aberto", await leitor.ReadLineAsync(tempo.Token));

        await StatusAsync();
        var pedido = await LerEventoAsync(leitor, tempo.Token);
        using (var falha = await _painel.PostAsJsonAsync("/_controle/falhas", new { tipo = "atraso", milissegundos = 10, vezes = 1 }, tempo.Token))
            Assert.Equal(HttpStatusCode.Created, falha.StatusCode);
        var estado = await LerEventoAsync(leitor, tempo.Token);

        Assert.Equal("pedido", pedido.Tipo);
        using (var json = JsonDocument.Parse(pedido.Dados))
            Assert.Equal(("MG", "cStat 107"), (json.RootElement.GetProperty("destino").GetString(), json.RootElement.GetProperty("situacao").GetString()));
        Assert.Equal(("estado", "{}"), (estado.Tipo, estado.Dados));
    }

    // Um evento do text/event-stream: as linhas "event:" e "data:" até a linha vazia; os comentários são pulados.
    private static async Task<(string Tipo, string Dados)> LerEventoAsync(StreamReader leitor, CancellationToken ct)
    {
        string? tipo = null, dados = null;
        while (await leitor.ReadLineAsync(ct) is { } linha)
        {
            if (linha.StartsWith("event: ", StringComparison.Ordinal))
                tipo = linha["event: ".Length..];
            else if (linha.StartsWith("data: ", StringComparison.Ordinal))
                dados = linha["data: ".Length..];
            else if (linha.Length == 0 && tipo is not null)
                return (tipo, dados ?? "");
        }
        throw new InvalidOperationException("O fluxo terminou.");
    }

    // O controle

    [Fact]
    public async Task MostraOEmuladorEOAcCrt()
    {
        var emulador = await JsonAsync("/_controle/emulador");

        Assert.Equal(_fixture.Emulador.Endereco.ToString(), emulador.GetProperty("endereco").GetString());
        Assert.Equal(_fixture.Emulador.Painel!.ToString(), emulador.GetProperty("painel").GetString());
        Assert.Equal(_fixture.Emulador.ArquivoAutoridade, emulador.GetProperty("autoridade").GetString());
        Assert.Equal(5, emulador.GetProperty("pedidos").GetInt32());
        Assert.Equal(_fixture.Emulador.Urls.Count, emulador.GetProperty("urls").GetArrayLength());
        using var ac = await _painel.GetAsync("/_controle/ac.crt", Ct);
        Assert.Equal("application/x-pem-file", ac.Content.Headers.ContentType?.MediaType);
        Assert.Equal(await File.ReadAllTextAsync(_fixture.Emulador.ArquivoAutoridade, Ct), await ac.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task ListaOsAutorizadoresComAContingencia()
    {
        var autorizadores = (await JsonAsync("/_controle/autorizadores")).EnumerateArray().ToDictionary(a => a.GetProperty("sigla").GetString()!);

        Assert.Equal(15, autorizadores.Count);
        var mg = autorizadores["MG"];
        var contingencia = Assert.Single(mg.GetProperty("contingencia").EnumerateArray());
        Assert.Equal(("MG", "SVC-AN"), (contingencia.GetProperty("uf").GetString(), contingencia.GetProperty("svc").GetString()));
        Assert.Equal(7, mg.GetProperty("servicos").GetArrayLength());
        Assert.Equal(["NFeRecepcaoEvento4", "NFeDistribuicaoDFe"], autorizadores["AN"].GetProperty("servicos").EnumerateArray().Select(s => s.GetString()));
        Assert.Equal(["AC", "ES", "RN", "PB", "SC"], autorizadores["SVRS"].GetProperty("cadastro").EnumerateArray().Select(s => s.GetString()));
        var svrs = autorizadores["SVRS"].GetProperty("contingencia").EnumerateArray().ToList();
        Assert.Equal(16, svrs.Count);
        Assert.All(svrs, c => Assert.Equal("SVC-AN", c.GetProperty("svc").GetString()));
        Assert.Equal(0, autorizadores["SVC-RS"].GetProperty("contingencia").GetArrayLength());
        Assert.Equal("7", autorizadores["SVC-RS"].GetProperty("tipoEmissao").GetString());
    }

    [Fact]
    public async Task MostraANotaComOsEventosEOXml()
    {
        var nota = new NotaTeste();
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt");
        await _fixture.Http.EnviarEventosAsync(_fixture.Url("MG", Servico.NomeRecepcaoEvento), [new EventoTeste { Chave = nota.Chave, Protocolo = protocolo }.Gerar()], Ct);

        var detalhe = await JsonAsync($"/_controle/notas/{nota.Chave}");
        var eventos = await JsonAsync("/_controle/eventos");

        Assert.True(detalhe.GetProperty("nota").GetProperty("cancelada").GetBoolean());
        var processada = XElement.Parse(detalhe.GetProperty("xml").GetString()!);
        Assert.Equal(Mensagens.NFe + "nfeProc", processada.Name);
        var evento = Assert.Single(detalhe.GetProperty("eventos").EnumerateArray());
        Assert.Equal(("110111", 1), (evento.GetProperty("evento").GetProperty("tipo").GetString(), evento.GetProperty("evento").GetProperty("sequencia").GetInt32()));
        Assert.Equal(Mensagens.NFe + "procEventoNFe", XElement.Parse(evento.GetProperty("xml").GetString()!).Name);
        Assert.False(evento.GetProperty("evento").TryGetProperty("processado", out _));
        var registrado = Assert.Single(eventos.EnumerateArray());
        Assert.Equal(nota.Chave, registrado.GetProperty("chave").GetString());
        Assert.Equal(0, (await JsonAsync("/_controle/inutilizacoes")).GetArrayLength());
        using var inexistente = await _painel.GetAsync("/_controle/notas/" + new string('1', 44), Ct);
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
    }

    [Fact]
    public async Task ZerarTudoLimpaOEstado()
    {
        await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [new NotaTeste().Gerar()], Ct);
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Atraso, Milissegundos: 1));
        _fixture.Emulador.Irregulares.Cadastrar("98765432000198");
        _fixture.Emulador.Cadastros.Cadastrar(new Contribuinte("MG", "0623079040081", "98765432000198", null, "TESTE"));
        _fixture.Emulador.Codigos.Cadastrar(new PedidoCsc("MG", "11222333", "2", "ABCDEFGHIJKLMNOPQRSTUV"));

        using (var zerado = await _painel.DeleteAsync("/_controle", Ct))
            Assert.Equal(HttpStatusCode.NoContent, zerado.StatusCode);

        Assert.Empty(_fixture.Emulador.Repositorio.Notas());
        Assert.Empty(_fixture.Emulador.Controle.Listar());
        Assert.Empty(_fixture.Emulador.Irregulares.Listar());
        Assert.Empty(_fixture.Emulador.Cadastros.Listar());
        Assert.Empty(_fixture.Emulador.Codigos.Listar());
        Assert.Empty(_fixture.Emulador.Pedidos.Listar());
    }
}
