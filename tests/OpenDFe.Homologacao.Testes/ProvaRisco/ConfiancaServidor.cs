using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// De onde veio a confiança no certificado do servidor. Público: é parâmetro de teste.
public enum OrigemConfianca
{
    Recusado = 0,
    Sistema = 1,      // a AC raiz está no repositório da máquina (MG e o Ambiente Nacional: ACs públicas)
    IcpBrasil = 2,    // a cadeia sobe a uma das raízes ICP-Brasil embutidas (SVRS, SP, PR e a maioria)
}

// A validação do certificado do servidor: a do sistema ou, quando ela falha só pela cadeia, a cadeia
// montada de novo com as raízes ICP-Brasil (CustomRootTrust). Nome divergente nunca é aceito.
internal static class ConfiancaServidor
{
    private static readonly Oid AutenticacaoServidor = new("1.3.6.1.5.5.7.3.1");

    public static OrigemConfianca Avaliar(X509Certificate? certificado, X509Chain? cadeia, SslPolicyErrors erros)
    {
        if (certificado is null)
            return OrigemConfianca.Recusado;
        if (erros == SslPolicyErrors.None)
            return OrigemConfianca.Sistema;
        if (erros != SslPolicyErrors.RemoteCertificateChainErrors)
            return OrigemConfianca.Recusado;

        using var icp = new X509Chain();
        icp.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        icp.ChainPolicy.CustomTrustStore.AddRange(RaizesIcpBrasil.Todas);
        // A revogação fica fora da prova: a LCR das ACs ICP-Brasil é decisão da 4.1.
        icp.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        icp.ChainPolicy.ApplicationPolicy.Add(AutenticacaoServidor);
        if (cadeia is not null)
        {
            // As intermediárias que o servidor mandou: no Linux, no ExtraStore; no Windows, na cadeia montada.
            icp.ChainPolicy.ExtraStore.AddRange(cadeia.ChainPolicy.ExtraStore);
            foreach (var elemento in cadeia.ChainElements)
                icp.ChainPolicy.ExtraStore.Add(elemento.Certificate);
        }

        var servidor = certificado as X509Certificate2 ?? X509CertificateLoader.LoadCertificate(certificado.GetRawCertData());
        try
        {
            return icp.Build(servidor) ? OrigemConfianca.IcpBrasil : OrigemConfianca.Recusado;
        }
        finally
        {
            if (!ReferenceEquals(servidor, certificado))
                servidor.Dispose();
        }
    }
}
