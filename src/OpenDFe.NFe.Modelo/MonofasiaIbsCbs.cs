namespace OpenDFe.NFe;

/// <summary>A tributação monofásica de combustíveis no IBS e na CBS (<c>gIBSCBSMono</c>).</summary>
/// <remarks>
/// O ad rem e o ad valorem de cada tributo são uma escolha do XSD: no IBS, no máximo um de <see cref="IbsAdRem"/> e
/// <see cref="IbsAdValorem"/>; na CBS, no máximo um de <see cref="CbsAdRem"/> e <see cref="CbsAdValorem"/>.
/// </remarks>
public sealed class MonofasiaIbsCbs
{
    /// <summary>O IBS monofásico ad rem; ou o <see cref="IbsAdValorem"/> (um dos dois).</summary>
    [Tag("gIBSMonoAdRem", "UB85a")]
    public MonofasiaIbsAdRem? IbsAdRem { get; set; }

    /// <summary>O IBS monofásico ad valorem; ou o <see cref="IbsAdRem"/> (um dos dois).</summary>
    [Tag("gIBSMonoAdValorem", "UB90")]
    public MonofasiaIbsAdValorem? IbsAdValorem { get; set; }

    /// <summary>A CBS monofásica ad rem; ou a <see cref="CbsAdValorem"/> (uma das duas).</summary>
    [Tag("gCBSMonoAdRem", "UB95a")]
    public MonofasiaCbsAdRem? CbsAdRem { get; set; }

    /// <summary>A CBS monofásica ad valorem; ou a <see cref="CbsAdRem"/> (uma das duas).</summary>
    [Tag("gCBSMonoAdValorem", "UB100")]
    public MonofasiaCbsAdValorem? CbsAdValorem { get; set; }

    /// <summary>O total do IBS monofásico do item.</summary>
    [Tag("vTotIBSMonoItem", "UB105a")]
    public decimal? TotalIbs { get; set; }

    /// <summary>O total da CBS monofásica do item.</summary>
    [Tag("vTotCBSMonoItem", "UB105b")]
    public decimal? TotalCbs { get; set; }
}
