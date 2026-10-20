namespace OpenDFe.NFe;

/// <summary>Os totais do IBS da UF (<c>gIBSUF</c>).</summary>
public sealed class TotalIbsUF
{
    /// <summary>O total do diferimento do IBS da UF.</summary>
    [Tag("vDif", "W38")]
    public decimal? Diferimento { get; set; }

    /// <summary>O total da devolução do IBS da UF.</summary>
    [Tag("vDevTrib", "W39")]
    public decimal? Devolucao { get; set; }

    /// <summary>O total do IBS da UF.</summary>
    [Tag("vIBSUF", "W41")]
    public decimal? Valor { get; set; }
}
