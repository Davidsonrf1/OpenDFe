namespace OpenDFe.NFe;

/// <summary>Uma origem do combustível (<c>origComb</c>).</summary>
public sealed class OrigemCombustivel
{
    /// <summary>Se o combustível é nacional ou importado.</summary>
    [Tag("indImport", "LA19")]
    public IndicadorImportacao? Importacao { get; set; }

    /// <summary>A UF de origem do produtor ou do importador.</summary>
    [Tag("cUFOrig", "LA20")]
    public UF? UF { get; set; }

    /// <summary>O percentual originário da UF.</summary>
    [Tag("pOrig", "LA21")]
    public decimal? Percentual { get; set; }
}
