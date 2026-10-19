using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OpenDFe.Testes;

// O HttpClient da prova: TLS mútuo com o certificado do titular, a confiança do sistema, da ICP-Brasil ou
// da AC do emulador (ConfiancaServidor) e o POST SOAP 1.2. Um por certificado, como o pool da 4.1
// (PROJETO.md 8.5).
internal sealed class ClienteSefaz : IDisposable
{
    private readonly HttpClient _http;
    private volatile int _origem;

    public ClienteSefaz(X509Certificate2 certificado, X509Certificate2Collection intermediarias, X509Certificate2? raizEmulador = null)
    {
        var handler = new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(15),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            SslOptions = new SslClientAuthenticationOptions
            {
                // O contexto leva as intermediárias no handshake; offline: não busca as que faltam na rede.
                ClientCertificateContext = SslStreamCertificateContext.Create(certificado, intermediarias, offline: true),
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                RemoteCertificateValidationCallback = (_, servidor, cadeia, erros) =>
                {
                    var origem = ConfiancaServidor.Avaliar(servidor, cadeia, erros, raizEmulador);
                    _origem = (int)origem;
                    return origem != OrigemConfianca.Recusado;
                },
            },
        };
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
    }

    // A confiança da última conexão aberta.
    public OrigemConfianca UltimaOrigem => (OrigemConfianca)_origem;

    public async Task<RespostaSoap> EnviarAsync(Uri url, string acao, string envelope, CancellationToken cancellationToken)
    {
        using var conteudo = new StringContent(envelope, Encoding.UTF8);
        conteudo.Headers.ContentType = MediaTypeHeaderValue.Parse($"application/soap+xml; charset=utf-8; action=\"{acao}\"");
        using var resposta = await _http.PostAsync(url, conteudo, cancellationToken);
        var texto = await resposta.Content.ReadAsStringAsync(cancellationToken);
        return new RespostaSoap(resposta.StatusCode, resposta.Version, texto);
    }

    public void Dispose() => _http.Dispose();
}

internal sealed record RespostaSoap(HttpStatusCode Status, Version VersaoHttp, string Texto);
