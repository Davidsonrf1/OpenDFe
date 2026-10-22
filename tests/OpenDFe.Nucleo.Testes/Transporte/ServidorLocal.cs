using System.Collections.Concurrent;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDFe.SefazEmulada;

namespace OpenDFe.Testes;

/// <summary>
/// Um Kestrel local com TLS mútuo, para o transporte (4.1): responde o que o teste mandar, nos formatos que a SEFAZ
/// emulada não produz (o 403 em HTML do IIS, o HTTP sem envelope, a falha SOAP do remetente). O certificado do
/// servidor é emitido pela AC de teste do emulador; o do cliente é exigido no handshake e aceito sem a cadeia.
/// </summary>
internal sealed class ServidorLocal : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly string _pasta;

    private ServidorLocal(WebApplication app, string pasta, X509Certificate2 autoridade, Uri endereco)
    {
        _app = app;
        _pasta = pasta;
        Autoridade = autoridade;
        Endereco = endereco;
    }

    public X509Certificate2 Autoridade { get; }

    public Uri Endereco { get; }

    /// <summary>A resposta; o padrão é o envelope com o retStatus do leiaute falso.</summary>
    public Func<HttpContext, Task> Responder { get; set; } = contexto => EscreverAsync(contexto, 200, Envelope(RetornoStatus));

    /// <summary>Os pedidos recebidos.</summary>
    public ConcurrentQueue<PedidoRecebido> Pedidos { get; } = new();

    public const string RetornoStatus = "<retStatus><cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo><host>local</host></retStatus>";

    public Uri Url(string servico = LeiauteFalso.ServicoStatus) => new(Endereco, "MG/" + servico);

    public static async Task<ServidorLocal> IniciarAsync()
    {
        var pasta = Path.Combine(Path.GetTempPath(), "opendfe-servidor-" + Guid.NewGuid().ToString("N"));
        var autoridade = AutoridadeTeste.Abrir(pasta, TimeProvider.System);
        var certificado = AutoridadeTeste.EmitirServidor(autoridade, [], TimeProvider.System);

        var construtor = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], EnvironmentName = Environments.Production });
        construtor.Logging.ClearProviders();
        construtor.WebHost.UseKestrelHttpsConfiguration();
        construtor.WebHost.ConfigureKestrel(kestrel => kestrel.Listen(IPAddress.Loopback, 0, escuta =>
        {
            escuta.Protocols = HttpProtocols.Http1;
            escuta.UseHttps(new HttpsConnectionAdapterOptions
            {
                ServerCertificate = certificado,
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                ClientCertificateMode = ClientCertificateMode.RequireCertificate,
                ClientCertificateValidation = (_, _, _) => true,
                CheckCertificateRevocation = false,
            });
        }));
        var app = construtor.Build();
        ServidorLocal? servidor = null;
        app.MapPost("/{destino}/{servico}", async (HttpContext contexto) =>
        {
            using var leitor = new StreamReader(contexto.Request.Body);
            var corpo = await leitor.ReadToEndAsync(contexto.RequestAborted);
            var cliente = await contexto.Connection.GetClientCertificateAsync(contexto.RequestAborted);
            servidor!.Pedidos.Enqueue(new PedidoRecebido(contexto.Request.Path, contexto.Request.ContentType, corpo, cliente?.Thumbprint));
            await servidor.Responder(contexto);
        });
        await app.StartAsync();
        var porta = new Uri(app.Urls.First()).Port;
        servidor = new ServidorLocal(app, pasta, autoridade, new Uri($"https://127.0.0.1:{porta}/"));
        return servidor;
    }

    public static string Envelope(string mensagem, string namespaceWsdl = LeiauteFalso.NamespaceWsdl) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>"
        + $"<r:nfeResultMsg xmlns:r=\"{namespaceWsdl}\">{mensagem}</r:nfeResultMsg></soap:Body></soap:Envelope>";

    public static string FalhaSoap(string codigo, string motivo) =>
        "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><soap:Fault>"
        + $"<soap:Code><soap:Value>soap:{codigo}</soap:Value></soap:Code><soap:Reason><soap:Text xml:lang=\"pt\">{motivo}</soap:Text></soap:Reason>"
        + "</soap:Fault></soap:Body></soap:Envelope>";

    public static async Task EscreverAsync(HttpContext contexto, int status, string corpo, string tipo = "application/soap+xml; charset=utf-8")
    {
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = tipo;
        await contexto.Response.WriteAsync(corpo, contexto.RequestAborted);
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
        Autoridade.Dispose();
        try
        {
            Directory.Delete(_pasta, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}

internal sealed record PedidoRecebido(string Caminho, string? Tipo, string Corpo, string? Cliente);
