namespace OpenDFe.NFSe;

/// <summary>O total do IBS estadual (<c>gIBSUFTot</c>).</summary>
public sealed class TotalIbsUF
{
    /// <summary>O diferimento; opcional.</summary>
    [Tag("vDifUF")]
    public decimal? Diferimento { get; set; }

    /// <summary>O valor.</summary>
    [Tag("vIBSUF")]
    public decimal? Valor { get; set; }
}
