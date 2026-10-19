namespace OpenDFe;

/// <summary>
/// Ambiente de emissão. O valor de cada membro é o que vai no XML (<c>tpAmb</c>).
/// </summary>
public enum Ambiente
{
    /// <summary>Produção (1): o documento tem validade jurídica.</summary>
    Producao = 1,

    /// <summary>Homologação (2): testes, sem validade jurídica.</summary>
    Homologacao = 2,
}
