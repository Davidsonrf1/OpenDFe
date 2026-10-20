namespace OpenDFe.NFe;

/// <summary>A tributação monofásica sujeita à retenção do IBS ad rem (<c>gMonoReten</c>).</summary>
public sealed class MonofasiaIbsAdRemRetencao
{
    /// <summary>A quantidade tributada no ICMS monofásico sujeito à retenção.</summary>
    [Tag("qBCMonoReten", "UB87a")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota ad rem do IBS sujeito à retenção.</summary>
    [Tag("adRemIBSReten", "UB87b")]
    public decimal? AliquotaAdRem { get; set; }

    /// <summary>O valor do IBS monofásico sujeito à retenção.</summary>
    [Tag("vIBSMonoReten", "UB87c")]
    public decimal? Valor { get; set; }
}
