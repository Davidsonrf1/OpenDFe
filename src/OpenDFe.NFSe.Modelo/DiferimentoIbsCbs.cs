namespace OpenDFe.NFSe;

/// <summary>O diferimento do IBS e da CBS (<c>gDif</c>).</summary>
public sealed class DiferimentoIbsCbs
{
    /// <summary>O percentual de diferimento do IBS estadual.</summary>
    [Tag("pDifUF")]
    public decimal? PercentualUF { get; set; }

    /// <summary>O percentual de diferimento do IBS municipal.</summary>
    [Tag("pDifMun")]
    public decimal? PercentualMunicipio { get; set; }

    /// <summary>O percentual de diferimento da CBS.</summary>
    [Tag("pDifCBS")]
    public decimal? PercentualCbs { get; set; }
}
