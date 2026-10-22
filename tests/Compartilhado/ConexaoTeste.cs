using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

/// <summary>
/// A fábrica de handlers dos testes contra a SEFAZ emulada e o servidor local (PROJETO.md 13.5): o handler da lib
/// (<see cref="ConexaoSefaz.CriarPadrao"/>), que também aceita o servidor cuja cadeia sobe à AC de teste. A raiz do
/// emulador entra pela fábrica, sem API pública própria na lib; o ErpCore e o PDV montam a sua igual, só no build de
/// teste.
/// </summary>
internal static class ConexaoTeste
{
    /// <summary>A fábrica que aceita a <paramref name="autoridade"/>; <paramref name="criada"/> vê cada conexão criada.</summary>
    public static Func<ConexaoSefaz, HttpMessageHandler> Fabrica(X509Certificate2 autoridade, Action<ConexaoSefaz>? criada = null) =>
        conexao =>
        {
            criada?.Invoke(conexao);
            var handler = conexao.CriarPadrao();
            var padrao = handler.SslOptions.RemoteCertificateValidationCallback!;
            handler.SslOptions.RemoteCertificateValidationCallback = (remetente, certificado, cadeia, erros) =>
                padrao(remetente, certificado, cadeia, erros) || SobeAutoridade(autoridade, certificado, erros);
            return handler;
        };

    /// <summary>Se o certificado do servidor sobe à AC de teste, com o nome certo.</summary>
    public static bool SobeAutoridade(X509Certificate2 autoridade, X509Certificate? certificado, SslPolicyErrors erros)
    {
        if (certificado is null || erros != SslPolicyErrors.RemoteCertificateChainErrors)
            return false;
        using var servidor = X509CertificateLoader.LoadCertificate(certificado.GetRawCertData());
        using var cadeia = new X509Chain();
        cadeia.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        cadeia.ChainPolicy.CustomTrustStore.Add(autoridade);
        cadeia.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        return cadeia.Build(servidor);
    }
}
