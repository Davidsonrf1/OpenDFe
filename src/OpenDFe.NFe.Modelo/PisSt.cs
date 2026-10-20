namespace OpenDFe.NFe;

/// <summary>O PIS por substituição tributária (<c>PISST</c>).</summary>
/// <remarks>
/// A base e a alíquota (<see cref="BaseCalculo"/> e <see cref="Aliquota"/>) ou a quantidade e o valor por unidade
/// (<see cref="Quantidade"/> e <see cref="AliquotaUnidade"/>): escolha do XSD.
/// </remarks>
public sealed class PisSt
{
    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "R02")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota, em percentual; opcional.</summary>
    [Tag("pPIS", "R03")]
    public decimal? Aliquota { get; set; }

    /// <summary>A quantidade vendida; opcional.</summary>
    [Tag("qBCProd", "R04")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota em reais, por unidade; opcional.</summary>
    [Tag("vAliqProd", "R05")]
    public decimal? AliquotaUnidade { get; set; }

    /// <summary>O valor do PIS.</summary>
    [Tag("vPIS", "R06")]
    public decimal? Valor { get; set; }

    /// <summary>Se o valor do PIS ST compõe o total da nota; opcional.</summary>
    [Tag("indSomaPISST", "R07")]
    public IndicadorTotal? Totalizacao { get; set; }
}
