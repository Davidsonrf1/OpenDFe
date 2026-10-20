namespace OpenDFe.NFe;

/// <summary>Os totais do IBS (<c>gIBS</c>).</summary>
public sealed class TotalIbs
{
    /// <summary>Os totais do IBS da UF.</summary>
    [Tag("gIBSUF", "W37")]
    public TotalIbsUF UF { get; set; } = new();

    /// <summary>Os totais do IBS do município.</summary>
    [Tag("gIBSMun", "W42")]
    public TotalIbsMunicipio Municipio { get; set; } = new();

    /// <summary>O total do IBS.</summary>
    [Tag("vIBS", "W47")]
    public decimal? Valor { get; set; }

    /// <summary>O total do crédito presumido do IBS.</summary>
    [Tag("vCredPres", "W48")]
    public decimal? CreditoPresumido { get; set; }

    /// <summary>O total do crédito presumido do IBS em condição suspensiva.</summary>
    [Tag("vCredPresCondSus", "W49")]
    public decimal? CreditoPresumidoCondicaoSuspensiva { get; set; }
}
