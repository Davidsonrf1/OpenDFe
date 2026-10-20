namespace OpenDFe.NFe;

/// <summary>A tributação monofásica sujeita à retenção da CBS ad rem (<c>gMonoReten</c>).</summary>
public sealed class MonofasiaCbsAdRemRetencao
{
    /// <summary>A quantidade tributada no ICMS monofásico sujeito à retenção.</summary>
    [Tag("qBCMonoReten", "UB97a")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota ad rem da CBS sujeita à retenção.</summary>
    [Tag("adRemCBSReten", "UB97b")]
    public decimal? AliquotaAdRem { get; set; }

    /// <summary>O valor da CBS monofásica sujeita à retenção.</summary>
    [Tag("vCBSMonoReten", "UB97c")]
    public decimal? Valor { get; set; }
}
