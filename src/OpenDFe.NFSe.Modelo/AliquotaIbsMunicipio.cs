namespace OpenDFe.NFSe;

/// <summary>As alíquotas do IBS municipal.</summary>
public sealed class AliquotaIbsMunicipio
{
    /// <summary>A alíquota (%).</summary>
    [Tag("pIBSMun")]
    public decimal? Aliquota { get; set; }

    /// <summary>O percentual de redução da alíquota; opcional.</summary>
    [Tag("pRedAliqMun")]
    public decimal? Reducao { get; set; }

    /// <summary>A alíquota efetiva (%).</summary>
    [Tag("pAliqEfetMun")]
    public decimal? Efetiva { get; set; }
}
