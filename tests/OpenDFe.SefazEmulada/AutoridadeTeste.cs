using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.SefazEmulada;

// A AC de teste do emulador (PROJETO.md 13.5): gerada na primeira partida e guardada na pasta do emulador
// (ac.pfx, com a chave, sem senha; ac.crt, só o certificado, em PEM, para quem usa confiar). O certificado
// do servidor é emitido por ela a cada partida, com os nomes em que o emulador atende.
internal static class AutoridadeTeste
{
    public const string ArquivoChave = "ac.pfx";
    public const string ArquivoCertificado = "ac.crt";

    private const string NomeAutoridade = "CN=OpenDFe SEFAZ Emulada - AC de teste, O=OpenDFe, C=BR";
    private static readonly Oid AutenticacaoServidor = new("1.3.6.1.5.5.7.3.1");
    private static readonly string[] NomesLocais = ["localhost", "127.0.0.1", "::1"];

    // A AC com a chave privada: a da pasta, se existe e vale por mais um dia; senão, uma nova.
    public static X509Certificate2 Abrir(string pasta, TimeProvider relogio)
    {
        Directory.CreateDirectory(pasta);
        var arquivoChave = Path.Combine(pasta, ArquivoChave);
        var agora = relogio.GetUtcNow();
        if (File.Exists(arquivoChave))
        {
            var existente = X509CertificateLoader.LoadPkcs12FromFile(arquivoChave, null);
            if (existente.NotAfter.ToUniversalTime() > agora.UtcDateTime.AddDays(1) && existente.HasPrivateKey)
            {
                GravarCertificado(pasta, existente);
                return existente;
            }
            existente.Dispose();
        }

        using var chave = RSA.Create(2048);
        var pedido = new CertificateRequest(NomeAutoridade, chave, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        pedido.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
        pedido.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        pedido.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(pedido.PublicKey, false));
        using var gerada = pedido.CreateSelfSigned(agora.AddDays(-1), agora.AddYears(10));
        File.WriteAllBytes(arquivoChave, gerada.Export(X509ContentType.Pkcs12));
        GravarCertificado(pasta, gerada);
        return X509CertificateLoader.LoadPkcs12FromFile(arquivoChave, null);
    }

    // O certificado do servidor, emitido pela AC, para os nomes dados (DNS ou IP) e sempre localhost,
    // 127.0.0.1 e ::1. Recarregado do PKCS#12: no Windows, o SChannel não usa a chave efêmera.
    public static X509Certificate2 EmitirServidor(X509Certificate2 autoridade, IEnumerable<string> nomes, TimeProvider relogio)
    {
        ArgumentNullException.ThrowIfNull(autoridade);
        var todos = NomesLocais.Concat(nomes).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var alternativos = new SubjectAlternativeNameBuilder();
        foreach (var nome in todos)
        {
            if (IPAddress.TryParse(nome, out var ip))
                alternativos.AddIpAddress(ip);
            else
                alternativos.AddDnsName(nome);
        }

        using var chave = RSA.Create(2048);
        var pedido = new CertificateRequest("CN=" + todos[^1], chave, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        pedido.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        pedido.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
        pedido.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([AutenticacaoServidor], false));
        pedido.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(pedido.PublicKey, false));
        pedido.CertificateExtensions.Add(X509AuthorityKeyIdentifierExtension.CreateFromCertificate(autoridade, true, false));
        pedido.CertificateExtensions.Add(alternativos.Build());

        var agora = relogio.GetUtcNow();
        var serie = RandomNumberGenerator.GetBytes(16);
        serie[0] &= 0x7F;
        using var emitido = pedido.Create(autoridade, agora.AddDays(-1), agora.AddDays(90), serie);
        using var comChave = emitido.CopyWithPrivateKey(chave);
        return X509CertificateLoader.LoadPkcs12(comChave.Export(X509ContentType.Pkcs12), null);
    }

    private static void GravarCertificado(string pasta, X509Certificate2 autoridade) =>
        File.WriteAllText(Path.Combine(pasta, ArquivoCertificado), autoridade.ExportCertificatePem() + "\n");
}
