namespace OpenDFe.NFe;

/// <summary>A fatura (<c>fat</c>).</summary>
public sealed class Fatura
{
    /// <summary>O número; opcional.</summary>
    [Tag("nFat", "Y03")]
    public string? Numero { get; set; }

    /// <summary>O valor original; opcional.</summary>
    [Tag("vOrig", "Y04")]
    public decimal? ValorOriginal { get; set; }

    /// <summary>O valor do desconto; opcional.</summary>
    [Tag("vDesc", "Y05")]
    public decimal? Desconto { get; set; }

    /// <summary>O valor líquido; opcional.</summary>
    [Tag("vLiq", "Y06")]
    public decimal? ValorLiquido { get; set; }
}
