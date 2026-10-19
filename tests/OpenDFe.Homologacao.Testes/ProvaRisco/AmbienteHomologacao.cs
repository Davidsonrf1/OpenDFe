using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// As variáveis de ambiente que ligam os testes de homologação (PROJETO.md 13.3). Sem elas, o teste é pulado:
// a CI pública não tem certificado nem fala com a SEFAZ.
internal static class AmbienteHomologacao
{
    // "1" liga os testes que só precisam da rede (a cadeia do servidor), sem o PFX.
    public const string VariavelRede = "OPENDFE_HOMOLOGACAO";

    // O PFX A1 ICP-Brasil de homologação, fora do repositório. Com ele, a rede também está ligada.
    public const string VariavelPfx = "OPENDFE_PFX_CAMINHO";
    public const string VariavelSenha = "OPENDFE_PFX_SENHA";

    public static bool PfxInformado => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(VariavelPfx));

    public static bool RedeLigada => Environment.GetEnvironmentVariable(VariavelRede) == "1" || PfxInformado;

    public static void ExigirRede() =>
        Assert.SkipUnless(RedeLigada, $"Sem {VariavelRede}=1 nem {VariavelPfx}: o teste fala com a SEFAZ.");

    public static void ExigirPfx() =>
        Assert.SkipUnless(PfxInformado, $"Sem {VariavelPfx} (e {VariavelSenha}): o teste precisa do PFX A1 ICP-Brasil.");

    // O certificado do titular, com a chave privada (pela API da lib, 1.3).
    public static X509Certificate2 AbrirPfx()
    {
        ExigirPfx();
        return CertificadoDigital.AbrirArquivoPfx(Caminho(), Senha());
    }

    // As outras do PFX (as ACs intermediárias, quando a AC as exporta junto), sem a chave privada: vão no
    // handshake, para o servidor montar a cadeia do cliente.
    public static X509Certificate2Collection AbrirIntermediarias()
    {
        ExigirPfx();
        var todas = X509CertificateLoader.LoadPkcs12CollectionFromFile(Caminho(), Senha());
        var intermediarias = new X509Certificate2Collection();
        foreach (var certificado in todas)
        {
            if (certificado.HasPrivateKey)
                certificado.Dispose();
            else
                intermediarias.Add(certificado);
        }
        return intermediarias;
    }

    private static string Caminho() => Environment.GetEnvironmentVariable(VariavelPfx)!;

    private static string Senha() => Environment.GetEnvironmentVariable(VariavelSenha) ?? "";
}
