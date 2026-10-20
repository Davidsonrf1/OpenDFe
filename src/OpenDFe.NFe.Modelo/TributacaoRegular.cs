namespace OpenDFe.NFe;

/// <summary>
/// A tributação regular, como seria se não cumprida a condição resolutória ou suspensiva (<c>gTribRegular</c>).
/// </summary>
public sealed class TributacaoRegular
{
    /// <summary>O CST da tributação regular, como no XML.</summary>
    [Tag("CSTReg", "UB69")]
    public string? Cst { get; set; }

    /// <summary>A classificação tributária da tributação regular, como no XML.</summary>
    [Tag("cClassTribReg", "UB70")]
    public string? ClassificacaoTributaria { get; set; }

    /// <summary>A alíquota efetiva do IBS da UF na tributação regular.</summary>
    [Tag("pAliqEfetRegIBSUF", "UB71")]
    public decimal? AliquotaIbsUF { get; set; }

    /// <summary>O valor do IBS da UF na tributação regular.</summary>
    [Tag("vTribRegIBSUF", "UB72")]
    public decimal? ValorIbsUF { get; set; }

    /// <summary>A alíquota efetiva do IBS do município na tributação regular.</summary>
    [Tag("pAliqEfetRegIBSMun", "UB72a")]
    public decimal? AliquotaIbsMunicipio { get; set; }

    /// <summary>O valor do IBS do município na tributação regular.</summary>
    [Tag("vTribRegIBSMun", "UB72b")]
    public decimal? ValorIbsMunicipio { get; set; }

    /// <summary>A alíquota efetiva da CBS na tributação regular.</summary>
    [Tag("pAliqEfetRegCBS", "UB72c")]
    public decimal? AliquotaCbs { get; set; }

    /// <summary>O valor da CBS na tributação regular.</summary>
    [Tag("vTribRegCBS", "UB72d")]
    public decimal? ValorCbs { get; set; }
}
