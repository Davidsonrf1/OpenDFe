namespace OpenDFe.NFe;

/// <summary>O IBS do município (<c>gIBSMun</c>).</summary>
public sealed class IbsMunicipio
{
    /// <summary>A alíquota do IBS do município.</summary>
    [Tag("pIBSMun", "UB37")]
    public decimal? Aliquota { get; set; }

    /// <summary>O diferimento; opcional.</summary>
    [Tag("gDif", "UB40")]
    public Diferimento? Diferimento { get; set; }

    /// <summary>A devolução de tributo; opcional.</summary>
    [Tag("gDevTrib", "UB43")]
    public DevolucaoTributo? Devolucao { get; set; }

    /// <summary>A redução da alíquota; opcional.</summary>
    [Tag("gRed", "UB45")]
    public ReducaoAliquota? Reducao { get; set; }

    /// <summary>O valor do IBS do município.</summary>
    [Tag("vIBSMun", "UB54")]
    public decimal? Valor { get; set; }
}
