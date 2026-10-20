namespace OpenDFe.NFe;

/// <summary>A CBS (<c>gCBS</c>).</summary>
public sealed class Cbs
{
    /// <summary>A alíquota da CBS.</summary>
    [Tag("pCBS", "UB56")]
    public decimal? Aliquota { get; set; }

    /// <summary>O diferimento; opcional.</summary>
    [Tag("gDif", "UB59")]
    public Diferimento? Diferimento { get; set; }

    /// <summary>A devolução de tributo; opcional.</summary>
    [Tag("gDevTrib", "UB62")]
    public DevolucaoTributo? Devolucao { get; set; }

    /// <summary>A redução da alíquota; opcional.</summary>
    [Tag("gRed", "UB64")]
    public ReducaoAliquota? Reducao { get; set; }

    /// <summary>A CBS com alíquota zero em área incentivada (ALC ou ZFM); opcional.</summary>
    [Tag("gALCZFMCBS", "UB66a")]
    public CbsAreaIncentivada? AreaIncentivada { get; set; }

    /// <summary>O valor da CBS.</summary>
    [Tag("vCBS", "UB67")]
    public decimal? Valor { get; set; }
}
