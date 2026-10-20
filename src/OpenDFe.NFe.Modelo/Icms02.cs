namespace OpenDFe.NFe;

/// <summary>O ICMS monofásico próprio sobre combustíveis (<c>ICMS02</c>, CST 02).</summary>
[Tag("ICMS02", "N02a")]
public sealed class Icms02 : Icms
{
    /// <summary>O CST (<c>02</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A quantidade tributada no ICMS monofásico próprio; opcional.</summary>
    [Tag("qBCMono", "N37a")]
    public decimal? QuantidadeBaseMonofasico { get; set; }

    /// <summary>A alíquota ad rem do ICMS monofásico próprio.</summary>
    [Tag("adRemICMS", "N38")]
    public decimal? AliquotaAdRem { get; set; }

    /// <summary>O valor do ICMS monofásico próprio.</summary>
    [Tag("vICMSMono", "N39")]
    public decimal? ValorMonofasico { get; set; }
}
