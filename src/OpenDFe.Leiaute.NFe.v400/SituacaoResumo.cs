namespace OpenDFe.NFe;

/// <summary>
/// A situação da NF-e no resumo (<c>cSitNFe</c> do <c>resNFe</c>, NT 2014.002, 3.11.1). Aberta, como as outras enumerações
/// do leiaute: um valor que a lib não conhece chega como o número, convertido.
/// </summary>
public enum SituacaoResumo
{
    /// <summary>Uso autorizado.</summary>
    Autorizada = 1,

    /// <summary>Uso denegado.</summary>
    Denegada = 2,

    /// <summary>NF-e cancelada.</summary>
    Cancelada = 3,
}
