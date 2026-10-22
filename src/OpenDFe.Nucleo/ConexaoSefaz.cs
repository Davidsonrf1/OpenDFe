using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe;

/// <summary>
/// O que a fábrica de handlers do <see cref="ConexoesSefaz"/> recebe: o certificado do TLS mútuo e as cifras do
/// autorizador. O pool chama a fábrica uma vez para cada certificado e política de cifras (PROJETO.md 8.5).
/// Criada pelo núcleo.
/// </summary>
/// <remarks>
/// <see cref="CriarPadrao"/> devolve o handler que a lib usaria; a fábrica de quem usa o ajusta (outra validação do
/// servidor, um proxy, as intermediárias do certificado) ou monta outro.
/// </remarks>
public sealed class ConexaoSefaz
{
    private readonly TimeSpan _tempoConexao;

    internal ConexaoSefaz(X509Certificate2 certificado, IReadOnlyList<TlsCipherSuite>? cifras, TimeSpan tempoConexao)
    {
        Certificado = certificado;
        Cifras = cifras;
        _tempoConexao = tempoConexao;
    }

    /// <summary>
    /// O certificado do cliente no TLS mútuo: uma cópia do pool, que vive enquanto o handler viver (quem usa pode
    /// descartar o seu). Não o descarte.
    /// </summary>
    public X509Certificate2 Certificado { get; }

    /// <summary>
    /// As cifras que o leiaute declara para o autorizador (<see cref="DeclaracaoLeiaute.DeclararCifras"/>);
    /// <see langword="null"/> = o padrão da máquina.
    /// </summary>
    public IReadOnlyList<TlsCipherSuite>? Cifras { get; }

    /// <summary>
    /// O handler da lib: TLS 1.2 e 1.3, o certificado do cliente no handshake (sem buscar as intermediárias na
    /// rede), o servidor aceito pelo sistema ou pelas raízes ICP-Brasil embutidas, sem conferir a revogação, as
    /// <see cref="Cifras"/> (só no Linux: o .NET não as aplica no Windows), o
    /// <see cref="ConexoesSefaz.TempoConexao"/>, sem redirecionamento nem cookies.
    /// </summary>
    /// <returns>Um handler novo, que é de quem o recebe.</returns>
    public SocketsHttpHandler CriarPadrao()
    {
        var tls = new SslClientAuthenticationOptions
        {
            ClientCertificateContext = SslStreamCertificateContext.Create(Certificado, additionalCertificates: null, offline: true),
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
            RemoteCertificateValidationCallback = ConfiancaServidor.Validar,
        };
        if (Cifras is not null && OperatingSystem.IsLinux())
            tls.CipherSuitesPolicy = new CipherSuitesPolicy(Cifras);

        return new SocketsHttpHandler
        {
            SslOptions = tls,
            ConnectTimeout = _tempoConexao,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            AllowAutoRedirect = false,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.None,
        };
    }
}
