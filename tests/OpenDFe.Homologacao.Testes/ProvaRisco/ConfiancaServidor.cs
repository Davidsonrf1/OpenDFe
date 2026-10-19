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
    Emulador = 3,     // a cadeia sobe à AC de teste da SEFAZ emulada (PROJETO.md 13.5), só quando informada
}

// A validação do certificado do servidor: a do sistema ou, quando ela falha só pela cadeia, a cadeia
// montada de novo com a AC do emulador (se informada) ou com as raízes ICP-Brasil (CustomRootTrust). Nome
// divergente nunca é aceito.
internal static class ConfiancaServidor
{
    private static readonly Oid AutenticacaoServidor = new("1.3.6.1.5.5.7.3.1");

    public static OrigemConfianca Avaliar(X509Certificate? certificado, X509Chain? cadeia, SslPolicyErrors erros, X509Certificate2? raizEmulador = null)
    {
        if (certificado is null)
            return OrigemConfianca.Recusado;
        if (erros == SslPolicyErrors.None)
            return OrigemConfianca.Sistema;
        if (erros != SslPolicyErrors.RemoteCertificateChainErrors)
            return OrigemConfianca.Recusado;

        var servidor = certificado as X509Certificate2 ?? X509CertificateLoader.LoadCertificate(certificado.GetRawCertData());
        try
        {
            if (raizEmulador is not null && Montar(servidor, cadeia, [raizEmulador]))
                return OrigemConfianca.Emulador;
            return Montar(servidor, cadeia, RaizesIcpBrasil.Todas) ? OrigemConfianca.IcpBrasil : OrigemConfianca.Recusado;
        }
        finally
        {
            if (!ReferenceEquals(servidor, certificado))
                servidor.Dispose();
        }
    }

    private static bool Montar(X509Certificate2 servidor, X509Chain? cadeia, X509Certificate2Collection raizes)
    {
        using var propria = new X509Chain();
        propria.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        propria.ChainPolicy.CustomTrustStore.AddRange(raizes);
        // A revogação fica fora da prova: a LCR das ACs ICP-Brasil é decisão da 4.1.
        propria.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        propria.ChainPolicy.ApplicationPolicy.Add(AutenticacaoServidor);
        if (cadeia is not null)
        {
            // As intermediárias que o servidor mandou: no Linux, no ExtraStore; no Windows, na cadeia montada.
            propria.ChainPolicy.ExtraStore.AddRange(cadeia.ChainPolicy.ExtraStore);
            foreach (var elemento in cadeia.ChainElements)
                propria.ChainPolicy.ExtraStore.Add(elemento.Certificate);
        }
        return propria.Build(servidor);
    }
}
