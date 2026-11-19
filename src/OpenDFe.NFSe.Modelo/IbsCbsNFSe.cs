namespace OpenDFe.NFSe;

/// <summary>O IBS e a CBS calculados pelo ambiente gerador (<c>IBSCBS</c>, <c>TCRTCIBSCBS</c>).</summary>
public sealed class IbsCbsNFSe
{
    /// <summary>O código IBGE da localidade de incidência.</summary>
    [Tag("cLocalidadeIncid")]
    public int? MunicipioIncidencia { get; set; }

    /// <summary>O nome da localidade de incidência.</summary>
    [Tag("xLocalidadeIncid")]
    public string? NomeMunicipioIncidencia { get; set; }

    /// <summary>O percentual de redução de alíquota em compra governamental; opcional.</summary>
    [Tag("pRedutor")]
    public decimal? PercentualRedutor { get; set; }

    /// <summary>A base de cálculo e as alíquotas.</summary>
    [Tag("valores")]
    public ValoresIbsCbsNFSe Valores { get; set; } = new();

    /// <summary>Os totais.</summary>
    [Tag("totCIBS")]
    public TotaisIbsCbs Totais { get; set; } = new();
}
