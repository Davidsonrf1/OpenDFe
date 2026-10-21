using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace OpenDFe.SefazEmulada;

// O emulador no ar: o Kestrel com TLS mútuo (o certificado do servidor emitido pela AC de teste; o do
// cliente exigido no handshake e aceito sem conferir a cadeia), as rotas /<destino>/<serviço>, a lista
// das URLs em GET / e o controle em /_controle; com o painel (E.6), uma segunda porta, em HTTP só no loopback,
// com a página (Painel) e as mesmas rotas /_controle. Serve ao dotnet run (Program) e aos testes.
internal sealed class Emulador : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly X509Certificate2 _autoridade;
    private readonly X509Certificate2 _servidor;

    private Emulador(WebApplication app, X509Certificate2 autoridade, X509Certificate2 servidor, Uri endereco, Uri? painel, string arquivoAutoridade)
    {
        _app = app;
        _autoridade = autoridade;
        _servidor = servidor;
        Endereco = endereco;
        Painel = painel;
        ArquivoAutoridade = arquivoAutoridade;
        Autoridade = X509CertificateLoader.LoadCertificate(autoridade.RawData);
        Controle = app.Services.GetRequiredService<Controle>();
        Repositorio = app.Services.GetRequiredService<Repositorio>();
        Codigos = app.Services.GetRequiredService<CodigosSeguranca>();
        Irregulares = app.Services.GetRequiredService<Irregulares>();
        Cadastros = app.Services.GetRequiredService<Cadastros>();
        Pedidos = app.Services.GetRequiredService<PedidosRecentes>();
        Urls = [.. Listar(endereco)];
        var situacao = app.Services.GetRequiredService<SituacaoEmulador>();
        situacao.Endereco = endereco;
        situacao.Painel = painel;
        situacao.Urls = Urls;
    }

    // A raiz, como https://127.0.0.1:5443/.
    public Uri Endereco { get; }

    // O painel, como http://127.0.0.1:5444/; nulo sem ele.
    public Uri? Painel { get; }

    // Os últimos pedidos, com o XML.
    public PedidosRecentes Pedidos { get; }

    // A AC de teste, sem a chave: a raiz em que o cliente confia.
    public X509Certificate2 Autoridade { get; }

    // O ac.crt exportado.
    public string ArquivoAutoridade { get; }

    public Controle Controle { get; }

    // As notas e os lotes recebidos.
    public Repositorio Repositorio { get; }

    // Os CSCs da NFC-e.
    public CodigosSeguranca Codigos { get; }

    // Os CNPJs e CPFs em situação irregular.
    public Irregulares Irregulares { get; }

    // Os contribuintes da consulta cadastro.
    public Cadastros Cadastros { get; }

    public IReadOnlyList<UrlServico> Urls { get; }

    public static async Task<Emulador> IniciarAsync(OpcoesEmulador opcoes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(opcoes);
        var autoridade = AutoridadeTeste.Abrir(opcoes.Pasta, opcoes.Relogio);
        var servidor = AutoridadeTeste.EmitirServidor(autoridade, [opcoes.Nome], opcoes.Relogio);

        var construtor = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            Args = [],
            ContentRootPath = AppContext.BaseDirectory,
            EnvironmentName = Environments.Production,
        });
        construtor.Logging.ClearProviders();
        if (opcoes.Registrar)
            construtor.Logging.AddSimpleConsole(o => o.SingleLine = true);
        construtor.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(2));
        construtor.Services.AddSingleton(opcoes);
        construtor.Services.AddSingleton<Controle>();
        construtor.Services.AddSingleton<Repositorio>();
        construtor.Services.AddSingleton<CodigosSeguranca>();
        construtor.Services.AddSingleton<Irregulares>();
        construtor.Services.AddSingleton<Cadastros>();
        construtor.Services.AddSingleton<PedidosRecentes>();
        var arquivoAutoridade = Path.Combine(opcoes.Pasta, AutoridadeTeste.ArquivoCertificado);
        construtor.Services.AddSingleton(new SituacaoEmulador(arquivoAutoridade));
        construtor.Services.AddSingleton<Atendimento>();
        construtor.Services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        construtor.WebHost.UseKestrelHttpsConfiguration();
        construtor.WebHost.ConfigureKestrel(kestrel =>
        {
            kestrel.AddServerHeader = false;
            kestrel.Limits.MaxRequestBodySize = 4 * 1024 * 1024;
            kestrel.Listen(opcoes.Endereco, opcoes.Porta, escuta =>
            {
                escuta.Protocols = HttpProtocols.Http1;
                escuta.UseHttps(new HttpsConnectionAdapterOptions
                {
                    ServerCertificate = servidor,
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    ClientCertificateMode = ClientCertificateMode.RequireCertificate,
                    // Qualquer certificado, inclusive o autoassinado dos testes (13.1): a SEFAZ exige a cadeia
                    // ICP-Brasil (Anexo I, A03, A06); o emulador, não.
                    ClientCertificateValidation = (_, _, _) => true,
                    CheckCertificateRevocation = false,
                });
            });
            // O painel: o navegador não fala com a porta do TLS mútuo, que exige o certificado do cliente e usa a AC de
            // teste. Só no loopback, mesmo com o --endereco de outra rede.
            if (opcoes.Painel is { } painel)
                kestrel.Listen(IPAddress.Loopback, painel, escuta => escuta.Protocols = HttpProtocols.Http1);
        });

        var app = construtor.Build();
        // A porta do painel (a única sem TLS) só serve a página e o /_controle; a do TLS, nada da página.
        app.Use((contexto, proximo) => PaginaPainel.Separar(contexto, proximo));
        app.MapPost("/{destino}/{servico}", (HttpContext contexto, string destino, string servico, Atendimento atendimento) =>
            atendimento.AtenderAsync(contexto, destino, servico));
        // A lista com o nome pelo qual o cliente chegou; na porta do painel, a página.
        app.MapGet("/", (HttpContext contexto) => contexto.Request.IsHttps
            ? Results.Json(new { autoridade = arquivoAutoridade, urls = Listar(new Uri($"https://{contexto.Request.Host}/")) })
            : PaginaPainel.Pagina("painel.html"));
        PaginaPainel.Mapear(app);
        RotasControle.Mapear(app);
        await app.StartAsync(cancellationToken);

        // As portas de verdade (as livres, com 0), dos endereços em que o Kestrel ficou: a do TLS e a do painel.
        var enderecos = app.Urls.Select(u => new Uri(u)).ToList();
        var porta = enderecos.First(u => u.Scheme == Uri.UriSchemeHttps).Port;
        var endereco = new Uri($"https://{(IPAddress.TryParse(opcoes.Nome, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? $"[{opcoes.Nome}]" : opcoes.Nome)}:{porta}/");
        var painel = enderecos.FirstOrDefault(u => u.Scheme == Uri.UriSchemeHttp) is { } http ? new Uri($"http://127.0.0.1:{http.Port}/") : null;
        return new Emulador(app, autoridade, servidor, endereco, painel, arquivoAutoridade);
    }

    public Task AguardarAsync(CancellationToken cancellationToken = default) => _app.WaitForShutdownAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        _servidor.Dispose();
        _autoridade.Dispose();
        Autoridade.Dispose();
    }

    // As URLs prontas para trocar (PROJETO.md 8.4): cada serviço implementado, em cada autorizador que o tem.
    private static List<UrlServico> Listar(Uri endereco) =>
        [.. from autorizador in Autorizador.Todos
        from servico in Atendimento.Servicos
        where Atendimento.Procurar(autorizador, servico.Nome) is not null
        select new UrlServico(autorizador.Sigla, servico.Nome, new Uri(endereco, $"{autorizador.Sigla}/{servico.Nome}"))];
}

internal sealed record UrlServico(string Destino, string Servico, Uri Url);

// O que a rota de controle mostra do próprio emulador (GET /_controle/emulador): o ac.crt e, depois da partida, o
// endereço dos Web Services, o do painel e as URLs.
internal sealed class SituacaoEmulador(string arquivoAutoridade)
{
    public string ArquivoAutoridade { get; } = arquivoAutoridade;

    public Uri? Endereco { get; set; }

    public Uri? Painel { get; set; }

    public IReadOnlyList<UrlServico> Urls { get; set; } = [];
}
