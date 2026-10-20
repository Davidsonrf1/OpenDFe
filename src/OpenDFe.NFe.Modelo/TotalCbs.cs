namespace OpenDFe.NFe;

/// <summary>Os totais da CBS (<c>gCBS</c>).</summary>
public sealed class TotalCbs
{
    /// <summary>O total do diferimento da CBS.</summary>
    [Tag("vDif", "W53")]
    public decimal? Diferimento { get; set; }

    /// <summary>O total da devolução da CBS.</summary>
    [Tag("vDevTrib", "W54")]
    public decimal? Devolucao { get; set; }

    /// <summary>O total da CBS.</summary>
    [Tag("vCBS", "W56")]
    public decimal? Valor { get; set; }

    /// <summary>O total do crédito presumido da CBS.</summary>
    [Tag("vCredPres", "W56a")]
    public decimal? CreditoPresumido { get; set; }

    /// <summary>O total do crédito presumido da CBS em condição suspensiva.</summary>
    [Tag("vCredPresCondSus", "W56b")]
    public decimal? CreditoPresumidoCondicaoSuspensiva { get; set; }
}
