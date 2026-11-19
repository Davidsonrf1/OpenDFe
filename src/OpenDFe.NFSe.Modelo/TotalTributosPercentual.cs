namespace OpenDFe.NFSe;

/// <summary>Os percentuais aproximados dos tributos (<c>pTotTrib</c>).</summary>
public sealed class TotalTributosPercentual
{
    /// <summary>Os tributos federais (%).</summary>
    [Tag("pTotTribFed")]
    public decimal? Federal { get; set; }

    /// <summary>Os tributos estaduais (%).</summary>
    [Tag("pTotTribEst")]
    public decimal? Estadual { get; set; }

    /// <summary>Os tributos municipais (%).</summary>
    [Tag("pTotTribMun")]
    public decimal? Municipal { get; set; }
}
