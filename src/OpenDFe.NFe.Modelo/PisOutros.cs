namespace OpenDFe.NFe;

/// <summary>O PIS de outras operações (<c>PISOutr</c>, CST 49 a 99).</summary>
/// <remarks>
/// A base e a alíquota (<see cref="BaseCalculo"/> e <see cref="Aliquota"/>) ou a quantidade e o valor por unidade
/// (<see cref="Quantidade"/> e <see cref="AliquotaUnidade"/>): escolha do XSD.
/// </remarks>
[Tag("PISOutr", "Q05")]
public sealed class PisOutros : Pis
{
    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "Q07")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota, em percentual; opcional.</summary>
    [Tag("pPIS", "Q08")]
    public decimal? Aliquota { get; set; }

    /// <summary>A quantidade vendida; opcional.</summary>
    [Tag("qBCProd", "Q10")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota em reais, por unidade; opcional.</summary>
    [Tag("vAliqProd", "Q11")]
    public decimal? AliquotaUnidade { get; set; }

    /// <summary>O valor do PIS.</summary>
    [Tag("vPIS", "Q09")]
    public decimal? Valor { get; set; }
}
