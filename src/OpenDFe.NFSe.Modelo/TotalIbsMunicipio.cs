namespace OpenDFe.NFSe;

/// <summary>O total do IBS municipal (<c>gIBSMunTot</c>).</summary>
public sealed class TotalIbsMunicipio
{
    /// <summary>O diferimento; opcional.</summary>
    [Tag("vDifMun")]
    public decimal? Diferimento { get; set; }

    /// <summary>O valor.</summary>
    [Tag("vIBSMun")]
    public decimal? Valor { get; set; }
}
