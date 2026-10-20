namespace OpenDFe.NFe;

/// <summary>Os totais do IBS do município (<c>gIBSMun</c>).</summary>
public sealed class TotalIbsMunicipio
{
    /// <summary>O total do diferimento do IBS do município.</summary>
    [Tag("vDif", "W43")]
    public decimal? Diferimento { get; set; }

    /// <summary>O total da devolução do IBS do município.</summary>
    [Tag("vDevTrib", "W44")]
    public decimal? Devolucao { get; set; }

    /// <summary>O total do IBS do município.</summary>
    [Tag("vIBSMun", "W46")]
    public decimal? Valor { get; set; }
}
