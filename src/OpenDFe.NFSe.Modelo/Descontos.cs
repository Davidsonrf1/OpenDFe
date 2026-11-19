namespace OpenDFe.NFSe;

/// <summary>Os descontos condicionado e incondicionado (<c>vDescCondIncond</c>).</summary>
public sealed class Descontos
{
    /// <summary>O desconto incondicionado; opcional.</summary>
    [Tag("vDescIncond")]
    public decimal? Incondicionado { get; set; }

    /// <summary>O desconto condicionado; opcional.</summary>
    [Tag("vDescCond")]
    public decimal? Condicionado { get; set; }
}
