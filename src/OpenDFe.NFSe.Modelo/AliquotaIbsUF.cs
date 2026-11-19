namespace OpenDFe.NFSe;

/// <summary>As alíquotas do IBS estadual.</summary>
public sealed class AliquotaIbsUF
{
    /// <summary>A alíquota (%).</summary>
    [Tag("pIBSUF")]
    public decimal? Aliquota { get; set; }

    /// <summary>O percentual de redução da alíquota; opcional.</summary>
    [Tag("pRedAliqUF")]
    public decimal? Reducao { get; set; }

    /// <summary>A alíquota efetiva (%).</summary>
    [Tag("pAliqEfetUF")]
    public decimal? Efetiva { get; set; }
}
