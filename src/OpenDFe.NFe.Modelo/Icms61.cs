namespace OpenDFe.NFe;

/// <summary>O ICMS monofásico sobre combustíveis cobrado anteriormente (<c>ICMS61</c>, CST 61).</summary>
[Tag("ICMS61", "N08a")]
public sealed class Icms61 : Icms
{
    /// <summary>O CST (<c>61</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A quantidade tributada no ICMS monofásico retido anteriormente; opcional.</summary>
    [Tag("qBCMonoRet", "N43a")]
    public decimal? QuantidadeBaseRetida { get; set; }

    /// <summary>A alíquota ad rem do ICMS monofásico retido anteriormente.</summary>
    [Tag("adRemICMSRet", "N44")]
    public decimal? AliquotaAdRemRetida { get; set; }

    /// <summary>O valor do ICMS monofásico retido anteriormente.</summary>
    [Tag("vICMSMonoRet", "N45")]
    public decimal? ValorMonofasicoRetido { get; set; }
}
