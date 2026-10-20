namespace OpenDFe.NFe;

/// <summary>O ICMS monofásico sobre combustíveis com recolhimento diferido (<c>ICMS53</c>, CST 53).</summary>
/// <remarks>
/// Na NT 2023.001 v1.60, os identificadores N41a e N42 aparecem duas vezes neste grupo; as propriedades os levam como
/// na NT.
/// </remarks>
[Tag("ICMS53", "N07a")]
public sealed class Icms53 : Icms
{
    /// <summary>O CST (<c>53</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A quantidade tributada no ICMS monofásico próprio; opcional.</summary>
    [Tag("qBCMono", "N37a")]
    public decimal? QuantidadeBaseMonofasico { get; set; }

    /// <summary>A alíquota ad rem do ICMS monofásico próprio; opcional.</summary>
    [Tag("adRemICMS", "N38")]
    public decimal? AliquotaAdRem { get; set; }

    /// <summary>O valor do ICMS monofásico da operação; opcional.</summary>
    [Tag("vICMSMonoOp", "N41a")]
    public decimal? ValorMonofasicoOperacao { get; set; }

    /// <summary>O percentual do diferimento; opcional.</summary>
    [Tag("pDif", "N42")]
    public decimal? PercentualDiferimento { get; set; }

    /// <summary>O valor do ICMS monofásico diferido; opcional.</summary>
    [Tag("vICMSMonoDif", "N43")]
    public decimal? ValorMonofasicoDiferido { get; set; }

    /// <summary>O valor do ICMS monofásico próprio; opcional.</summary>
    [Tag("vICMSMono", "N39")]
    public decimal? ValorMonofasico { get; set; }

    /// <summary>A quantidade tributada no ICMS monofásico diferido; opcional.</summary>
    [Tag("qBCMonoDif", "N41a")]
    public decimal? QuantidadeBaseDiferida { get; set; }

    /// <summary>A alíquota ad rem do ICMS monofásico diferido; opcional.</summary>
    [Tag("adRemICMSDif", "N42")]
    public decimal? AliquotaAdRemDiferida { get; set; }
}
