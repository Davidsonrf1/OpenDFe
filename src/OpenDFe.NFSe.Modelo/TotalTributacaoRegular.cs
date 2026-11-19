namespace OpenDFe.NFSe;

/// <summary>A tributação regular calculada (<c>gTribRegular</c> da NFS-e).</summary>
public sealed class TotalTributacaoRegular
{
    /// <summary>A alíquota efetiva do IBS estadual (%).</summary>
    [Tag("pAliqEfeRegIBSUF")]
    public decimal? AliquotaIbsUF { get; set; }

    /// <summary>O valor do IBS estadual.</summary>
    [Tag("vTribRegIBSUF")]
    public decimal? ValorIbsUF { get; set; }

    /// <summary>A alíquota efetiva do IBS municipal (%).</summary>
    [Tag("pAliqEfeRegIBSMun")]
    public decimal? AliquotaIbsMunicipio { get; set; }

    /// <summary>O valor do IBS municipal.</summary>
    [Tag("vTribRegIBSMun")]
    public decimal? ValorIbsMunicipio { get; set; }

    /// <summary>A alíquota efetiva da CBS (%).</summary>
    [Tag("pAliqEfeRegCBS")]
    public decimal? AliquotaCbs { get; set; }

    /// <summary>O valor da CBS.</summary>
    [Tag("vTribRegCBS")]
    public decimal? ValorCbs { get; set; }
}
