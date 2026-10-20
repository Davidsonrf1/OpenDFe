namespace OpenDFe.NFe;

/// <summary>A exportação (<c>exporta</c>).</summary>
public sealed class Exportacao
{
    /// <summary>A UF do embarque ou da transposição de fronteira.</summary>
    [Tag("UFSaidaPais", "ZA02")]
    public UF? UFSaida { get; set; }

    /// <summary>O local do embarque ou da transposição de fronteira.</summary>
    [Tag("xLocExporta", "ZA03")]
    public string? LocalEmbarque { get; set; }

    /// <summary>O local do despacho; opcional.</summary>
    [Tag("xLocDespacho", "ZA04")]
    public string? LocalDespacho { get; set; }
}
