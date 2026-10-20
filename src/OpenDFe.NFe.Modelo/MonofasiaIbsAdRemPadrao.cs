namespace OpenDFe.NFe;

/// <summary>A tributação monofásica padrão do IBS ad rem (<c>gMonoPadrao</c>).</summary>
public sealed class MonofasiaIbsAdRemPadrao
{
    /// <summary>A quantidade tributada no ICMS monofásico próprio.</summary>
    [Tag("qBCMono", "UB86a")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota ad rem do IBS.</summary>
    [Tag("adRemIBS", "UB86b")]
    public decimal? AliquotaAdRem { get; set; }

    /// <summary>O valor do IBS monofásico.</summary>
    [Tag("vIBSMono", "UB86c")]
    public decimal? Valor { get; set; }
}
