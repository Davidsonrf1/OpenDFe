namespace OpenDFe.NFe;

/// <summary>A composição do IBS e da CBS na compra governamental (<c>gTribCompraGov</c>).</summary>
public sealed class TributacaoCompraGovernamental
{
    /// <summary>A alíquota do IBS da UF na compra governamental.</summary>
    [Tag("pAliqIBSUF", "UB82b")]
    public decimal? AliquotaIbsUF { get; set; }

    /// <summary>O valor do IBS da UF na compra governamental.</summary>
    [Tag("vTribIBSUF", "UB82c")]
    public decimal? ValorIbsUF { get; set; }

    /// <summary>A alíquota do IBS do município na compra governamental.</summary>
    [Tag("pAliqIBSMun", "UB82d")]
    public decimal? AliquotaIbsMunicipio { get; set; }

    /// <summary>O valor do IBS do município na compra governamental.</summary>
    [Tag("vTribIBSMun", "UB82e")]
    public decimal? ValorIbsMunicipio { get; set; }

    /// <summary>A alíquota da CBS na compra governamental.</summary>
    [Tag("pAliqCBS", "UB82f")]
    public decimal? AliquotaCbs { get; set; }

    /// <summary>O valor da CBS na compra governamental.</summary>
    [Tag("vTribCBS", "UB82g")]
    public decimal? ValorCbs { get; set; }
}
