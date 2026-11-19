namespace OpenDFe.NFSe;

/// <summary>Os totais do IBS (<c>gIBS</c>).</summary>
public sealed class TotalIbs
{
    /// <summary>O IBS total (estadual mais municipal).</summary>
    [Tag("vIBSTot")]
    public decimal? Total { get; set; }

    /// <summary>O crédito presumido; opcional.</summary>
    [Tag("gIBSCredPres")]
    public CreditoPresumidoIbs? CreditoPresumido { get; set; }

    /// <summary>O IBS estadual.</summary>
    [Tag("gIBSUFTot")]
    public TotalIbsUF UF { get; set; } = new();

    /// <summary>O IBS municipal.</summary>
    [Tag("gIBSMunTot")]
    public TotalIbsMunicipio Municipio { get; set; } = new();
}
