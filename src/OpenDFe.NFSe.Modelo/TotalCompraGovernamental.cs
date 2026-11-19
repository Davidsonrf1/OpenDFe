namespace OpenDFe.NFSe;

/// <summary>A composição do IBS e da CBS em compra governamental (<c>gTribCompraGov</c>).</summary>
public sealed class TotalCompraGovernamental
{
    /// <summary>A alíquota do IBS estadual (%).</summary>
    [Tag("pIBSUF")]
    public decimal? AliquotaIbsUF { get; set; }

    /// <summary>O valor do IBS estadual.</summary>
    [Tag("vIBSUF")]
    public decimal? ValorIbsUF { get; set; }

    /// <summary>A alíquota do IBS municipal (%).</summary>
    [Tag("pIBSMun")]
    public decimal? AliquotaIbsMunicipio { get; set; }

    /// <summary>O valor do IBS municipal.</summary>
    [Tag("vIBSMun")]
    public decimal? ValorIbsMunicipio { get; set; }

    /// <summary>A alíquota da CBS (%).</summary>
    [Tag("pCBS")]
    public decimal? AliquotaCbs { get; set; }

    /// <summary>O valor da CBS.</summary>
    [Tag("vCBS")]
    public decimal? ValorCbs { get; set; }
}
