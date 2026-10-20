namespace OpenDFe.NFe;

/// <summary>A COFINS de outras operações (<c>COFINSOutr</c>, CST 49 a 99).</summary>
/// <remarks>
/// A base e a alíquota (<see cref="BaseCalculo"/> e <see cref="Aliquota"/>) ou a quantidade e o valor por unidade
/// (<see cref="Quantidade"/> e <see cref="AliquotaUnidade"/>): escolha do XSD.
/// </remarks>
[Tag("COFINSOutr", "S05")]
public sealed class CofinsOutros : Cofins
{
    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "S07")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota, em percentual; opcional.</summary>
    [Tag("pCOFINS", "S08")]
    public decimal? Aliquota { get; set; }

    /// <summary>A quantidade vendida; opcional.</summary>
    [Tag("qBCProd", "S09")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota em reais, por unidade; opcional.</summary>
    [Tag("vAliqProd", "S10")]
    public decimal? AliquotaUnidade { get; set; }

    /// <summary>O valor da COFINS.</summary>
    [Tag("vCOFINS", "S11")]
    public decimal? Valor { get; set; }
}
