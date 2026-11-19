namespace OpenDFe.NFSe;

/// <summary>Os valores aproximados dos tributos (<c>vTotTrib</c>).</summary>
public sealed class TotalTributosValor
{
    /// <summary>Os tributos federais.</summary>
    [Tag("vTotTribFed")]
    public decimal? Federal { get; set; }

    /// <summary>Os tributos estaduais.</summary>
    [Tag("vTotTribEst")]
    public decimal? Estadual { get; set; }

    /// <summary>Os tributos municipais.</summary>
    [Tag("vTotTribMun")]
    public decimal? Municipal { get; set; }
}
