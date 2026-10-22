using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe;

/// <summary>De onde veio a confiança no certificado do servidor.</summary>
internal enum OrigemConfianca
{
    Recusado = 0,

    /// <summary>A AC raiz está no repositório da máquina (MG, o AN e a SVAN: ACs públicas).</summary>
    Sistema = 1,

    /// <summary>A cadeia sobe a uma das raízes ICP-Brasil embutidas (a SVRS, SP, o PR e a maioria).</summary>
    IcpBrasil = 2,
}

/// <summary>
/// A validação do certificado do servidor (PROJETO.md 8.5, o achado da 1.7): a do sistema ou, quando ela falha só
/// pela cadeia, a cadeia montada de novo com as raízes ICP-Brasil embutidas (<c>CustomRootTrust</c>). Nome
/// divergente nunca passa. Sem a revogação, que fica a critério da fábrica de quem usa.
/// </summary>
internal static class ConfiancaServidor
{
    private static readonly Oid AutenticacaoServidor = new("1.3.6.1.5.5.7.3.1");

    /// <summary>A validação do <see cref="SslClientAuthenticationOptions.RemoteCertificateValidationCallback"/>.</summary>
    public static bool Validar(object remetente, X509Certificate? certificado, X509Chain? cadeia, SslPolicyErrors erros) =>
        Avaliar(certificado, cadeia, erros) != OrigemConfianca.Recusado;

    public static OrigemConfianca Avaliar(X509Certificate? certificado, X509Chain? cadeia, SslPolicyErrors erros)
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

/// <summary>
/// As ACs raiz da ICP-Brasil vigentes (v5, v6, v7, v10, v11, v12, v13), embutidas em <c>Raizes/</c>: as distribuições
/// Linux não as trazem, e a maioria dos autorizadores tem o certificado do servidor emitido abaixo da Raiz v10.
/// Baixadas do ITI (<c>acraiz.icpbrasil.gov.br/credenciadas/RAIZ</c>) na etapa 1.7.
/// </summary>
internal static class RaizesIcpBrasil
{
    private static readonly Lazy<X509Certificate2Collection> Colecao = new(Carregar);

    public static X509Certificate2Collection Todas => Colecao.Value;

    private static X509Certificate2Collection Carregar()
    {
        var assembly = typeof(RaizesIcpBrasil).Assembly;
        var colecao = new X509Certificate2Collection();
        foreach (var recurso in assembly.GetManifestResourceNames().Where(r => r.StartsWith("Raizes/", StringComparison.Ordinal)).Order(StringComparer.Ordinal))
        {
            using var fluxo = assembly.GetManifestResourceStream(recurso)!;
            using var leitor = new StreamReader(fluxo);
            colecao.Add(X509Certificate2.CreateFromPem(leitor.ReadToEnd()));
        }
        return colecao;
    }
}
