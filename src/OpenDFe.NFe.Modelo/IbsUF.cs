namespace OpenDFe.NFe;

/// <summary>O IBS da UF (<c>gIBSUF</c>).</summary>
public sealed class IbsUF
{
    /// <summary>A alíquota do IBS da UF.</summary>
    [Tag("pIBSUF", "UB18")]
    public decimal? Aliquota { get; set; }

    /// <summary>O diferimento; opcional.</summary>
    [Tag("gDif", "UB21")]
    public Diferimento? Diferimento { get; set; }

    /// <summary>A devolução de tributo; opcional.</summary>
    [Tag("gDevTrib", "UB24")]
    public DevolucaoTributo? Devolucao { get; set; }

    /// <summary>A redução da alíquota; opcional.</summary>
    [Tag("gRed", "UB26")]
    public ReducaoAliquota? Reducao { get; set; }

    /// <summary>O valor do IBS da UF.</summary>
    [Tag("vIBSUF", "UB35")]
    public decimal? Valor { get; set; }
}
