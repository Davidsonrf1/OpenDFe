namespace OpenDFe.NFe;

/// <summary>
/// O ICMS monofásico próprio e com responsabilidade pela retenção sobre combustíveis (<c>ICMS15</c>, CST 15).
/// </summary>
[Tag("ICMS15", "N03a")]
public sealed class Icms15 : Icms
{
    /// <summary>O CST (<c>15</c>). Nulo na escrita: o único valor do grupo.</summary>
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

    /// <summary>A quantidade tributada no ICMS monofásico sujeito à retenção; opcional.</summary>
    [Tag("qBCMonoReten", "N39a")]
    public decimal? QuantidadeBaseRetencao { get; set; }

    /// <summary>A alíquota ad rem do ICMS monofásico sujeito à retenção.</summary>
    [Tag("adRemICMSReten", "N40")]
    public decimal? AliquotaAdRemRetencao { get; set; }

    /// <summary>O valor do ICMS monofásico sujeito à retenção.</summary>
    [Tag("vICMSMonoReten", "N41")]
    public decimal? ValorMonofasicoRetencao { get; set; }

    /// <summary>O percentual de redução da alíquota ad rem; opcional.</summary>
    [Tag("pRedAdRem", "N47")]
    public decimal? PercentualReducaoAdRem { get; set; }

    /// <summary>O motivo da redução da alíquota ad rem; opcional.</summary>
    [Tag("motRedAdRem", "N48")]
    public MotivoReducaoAdRem? MotivoReducaoAdRem { get; set; }
}
