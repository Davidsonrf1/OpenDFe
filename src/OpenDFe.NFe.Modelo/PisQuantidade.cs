namespace OpenDFe.NFe;

/// <summary>O PIS tributado por quantidade (<c>PISQtde</c>, CST 03).</summary>
[Tag("PISQtde", "Q03")]
public sealed class PisQuantidade : Pis
{
    /// <summary>A quantidade vendida.</summary>
    [Tag("qBCProd", "Q10")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota em reais, por unidade.</summary>
    [Tag("vAliqProd", "Q11")]
    public decimal? AliquotaUnidade { get; set; }

    /// <summary>O valor do PIS.</summary>
    [Tag("vPIS", "Q09")]
    public decimal? Valor { get; set; }
}
