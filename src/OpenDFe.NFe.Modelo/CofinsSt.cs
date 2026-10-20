namespace OpenDFe.NFe;

/// <summary>A COFINS por substituição tributária (<c>COFINSST</c>).</summary>
/// <remarks>
/// A base e a alíquota (<see cref="BaseCalculo"/> e <see cref="Aliquota"/>) ou a quantidade e o valor por unidade
/// (<see cref="Quantidade"/> e <see cref="AliquotaUnidade"/>): escolha do XSD.
/// </remarks>
public sealed class CofinsSt
{
    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "T02")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota, em percentual; opcional.</summary>
    [Tag("pCOFINS", "T03")]
    public decimal? Aliquota { get; set; }

    /// <summary>A quantidade vendida; opcional.</summary>
    [Tag("qBCProd", "T04")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota em reais, por unidade; opcional.</summary>
    [Tag("vAliqProd", "T05")]
    public decimal? AliquotaUnidade { get; set; }

    /// <summary>O valor da COFINS.</summary>
    [Tag("vCOFINS", "T06")]
    public decimal? Valor { get; set; }

    /// <summary>Se o valor da COFINS ST compõe o total da nota; opcional.</summary>
    [Tag("indSomaCOFINSST", "T07")]
    public IndicadorTotal? Totalizacao { get; set; }
}
