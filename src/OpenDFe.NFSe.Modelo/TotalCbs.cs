namespace OpenDFe.NFSe;

/// <summary>Os totais da CBS (<c>gCBS</c>).</summary>
public sealed class TotalCbs
{
    /// <summary>O crédito presumido; opcional.</summary>
    [Tag("gCBSCredPres")]
    public CreditoPresumidoCbs? CreditoPresumido { get; set; }

    /// <summary>O diferimento; opcional.</summary>
    [Tag("vDifCBS")]
    public decimal? Diferimento { get; set; }

    /// <summary>O valor.</summary>
    [Tag("vCBS")]
    public decimal? Valor { get; set; }
}
