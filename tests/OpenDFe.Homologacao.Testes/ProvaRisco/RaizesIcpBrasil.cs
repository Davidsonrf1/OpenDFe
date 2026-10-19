using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// As ACs raiz da ICP-Brasil vigentes (v5, v6, v7, v10, v11, v12, v13), embutidas: nenhum repositório de
// sistema (Windows, as distribuições Linux) as traz, e a maioria dos autorizadores tem o certificado do
// servidor emitido abaixo da Raiz v10.
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
