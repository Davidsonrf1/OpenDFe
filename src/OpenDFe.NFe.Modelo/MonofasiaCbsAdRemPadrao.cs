namespace OpenDFe.NFe;

/// <summary>A tributação monofásica padrão da CBS ad rem (<c>gMonoPadrao</c>).</summary>
public sealed class MonofasiaCbsAdRemPadrao
{
    /// <summary>A quantidade tributada no ICMS monofásico próprio.</summary>
    [Tag("qBCMono", "UB96a")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota ad rem da CBS.</summary>
    [Tag("adRemCBS", "UB96b")]
    public decimal? AliquotaAdRem { get; set; }

    /// <summary>O valor da CBS monofásica.</summary>
    [Tag("vCBSMono", "UB96c")]
    public decimal? Valor { get; set; }
}
