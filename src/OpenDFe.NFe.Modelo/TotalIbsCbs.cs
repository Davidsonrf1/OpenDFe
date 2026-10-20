namespace OpenDFe.NFe;

/// <summary>Os totais do IBS e da CBS (<c>IBSCBSTot</c>).</summary>
public sealed class TotalIbsCbs
{
    /// <summary>O total da base de cálculo do IBS e da CBS.</summary>
    [Tag("vBCIBSCBS", "W35")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>Os totais do IBS; opcional.</summary>
    [Tag("gIBS", "W36")]
    public TotalIbs? Ibs { get; set; }

    /// <summary>Os totais da CBS; opcional.</summary>
    [Tag("gCBS", "W50")]
    public TotalCbs? Cbs { get; set; }

    /// <summary>Os totais da tributação monofásica; opcional.</summary>
    [Tag("gMono", "W57")]
    public TotalMonofasia? Monofasia { get; set; }

    /// <summary>Os totais do estorno de crédito; opcional.</summary>
    [Tag("gEstornoCred", "W59e")]
    public EstornoCredito? EstornoCredito { get; set; }
}
