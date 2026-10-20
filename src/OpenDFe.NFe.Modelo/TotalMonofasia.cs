namespace OpenDFe.NFe;

/// <summary>Os totais da tributação monofásica do IBS e da CBS (<c>gMono</c>).</summary>
public sealed class TotalMonofasia
{
    /// <summary>O total do IBS monofásico.</summary>
    [Tag("vIBSMono", "W58")]
    public decimal? Ibs { get; set; }

    /// <summary>O total da CBS monofásica.</summary>
    [Tag("vCBSMono", "W59")]
    public decimal? Cbs { get; set; }

    /// <summary>O total do IBS monofásico sujeito à retenção.</summary>
    [Tag("vIBSMonoReten", "W59a")]
    public decimal? IbsRetencao { get; set; }

    /// <summary>O total da CBS monofásica sujeita à retenção.</summary>
    [Tag("vCBSMonoReten", "W59b")]
    public decimal? CbsRetencao { get; set; }

    /// <summary>O total do IBS monofásico retido anteriormente.</summary>
    [Tag("vIBSMonoRet", "W59c")]
    public decimal? IbsRetido { get; set; }

    /// <summary>O total da CBS monofásica retida anteriormente.</summary>
    [Tag("vCBSMonoRet", "W59d")]
    public decimal? CbsRetido { get; set; }
}
