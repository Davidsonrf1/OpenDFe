namespace OpenDFe.NFSe;

/// <summary>Os totais do IBS e da CBS (<c>totCIBS</c>).</summary>
public sealed class TotaisIbsCbs
{
    /// <summary>O valor total da nota com o IBS e a CBS por fora.</summary>
    [Tag("vTotNF")]
    public decimal? TotalNota { get; set; }

    /// <summary>O IBS.</summary>
    [Tag("gIBS")]
    public TotalIbs Ibs { get; set; } = new();

    /// <summary>A CBS.</summary>
    [Tag("gCBS")]
    public TotalCbs Cbs { get; set; } = new();

    /// <summary>A tributação regular; opcional.</summary>
    [Tag("gTribRegular")]
    public TotalTributacaoRegular? TributacaoRegular { get; set; }

    /// <summary>A composição em compra governamental; opcional.</summary>
    [Tag("gTribCompraGov")]
    public TotalCompraGovernamental? CompraGovernamental { get; set; }
}
