namespace OpenDFe.NFe;

/// <summary>Os produtos agropecuários (<c>agropecuario</c>): os defensivos ou a guia de trânsito.</summary>
public sealed class Agropecuario
{
    /// <summary>Os defensivos (até 20); exclusivos com a <see cref="GuiaTransito"/>.</summary>
    [Tag("defensivo", "ZF02")]
    public IList<Defensivo> Defensivos { get; } = new List<Defensivo>();

    /// <summary>A guia de trânsito; exclusiva com os <see cref="Defensivos"/>.</summary>
    [Tag("guiaTransito", "ZF04")]
    public GuiaTransito? GuiaTransito { get; set; }
}
