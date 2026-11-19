namespace OpenDFe.NFSe;

/// <summary>As alíquotas da CBS.</summary>
public sealed class AliquotaCbs
{
    /// <summary>A alíquota (%).</summary>
    [Tag("pCBS")]
    public decimal? Aliquota { get; set; }

    /// <summary>O percentual de redução da alíquota; opcional.</summary>
    [Tag("pRedAliqCBS")]
    public decimal? Reducao { get; set; }

    /// <summary>A alíquota efetiva (%).</summary>
    [Tag("pAliqEfetCBS")]
    public decimal? Efetiva { get; set; }
}
