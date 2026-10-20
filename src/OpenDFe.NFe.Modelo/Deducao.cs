namespace OpenDFe.NFe;

/// <summary>Uma dedução da aquisição de cana (<c>deduc</c>).</summary>
public sealed class Deducao
{
    /// <summary>A descrição.</summary>
    [Tag("xDed", "ZC11")]
    public string? Descricao { get; set; }

    /// <summary>O valor.</summary>
    [Tag("vDed", "ZC12")]
    public decimal? Valor { get; set; }
}
