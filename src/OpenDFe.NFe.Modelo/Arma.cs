namespace OpenDFe.NFe;

/// <summary>Uma arma de fogo (<c>arma</c>).</summary>
public sealed class Arma
{
    /// <summary>O tipo da arma de fogo.</summary>
    [Tag("tpArma", "L02")]
    public TipoArma? Tipo { get; set; }

    /// <summary>O número de série da arma.</summary>
    [Tag("nSerie", "L03")]
    public string? Serie { get; set; }

    /// <summary>O número de série do cano.</summary>
    [Tag("nCano", "L04")]
    public string? SerieCano { get; set; }

    /// <summary>A descrição completa da arma (calibre, marca, capacidade e afins).</summary>
    [Tag("descr", "L05")]
    public string? Descricao { get; set; }
}
