namespace OpenDFe.NFe;

/// <summary>A guia de trânsito de produtos agropecuários (<c>guiaTransito</c>).</summary>
public sealed class GuiaTransito
{
    /// <summary>O tipo da guia.</summary>
    [Tag("tpGuia", "ZF05")]
    public TipoGuiaTransito? Tipo { get; set; }

    /// <summary>A UF de emissão; opcional.</summary>
    [Tag("UFGuia", "ZF06")]
    public UF? UF { get; set; }

    /// <summary>A série; opcional.</summary>
    [Tag("serieGuia", "ZF07")]
    public string? Serie { get; set; }

    /// <summary>O número.</summary>
    [Tag("nGuia", "ZF08")]
    public int? Numero { get; set; }
}
