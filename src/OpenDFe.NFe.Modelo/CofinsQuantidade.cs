namespace OpenDFe.NFe;

/// <summary>A COFINS tributada por quantidade (<c>COFINSQtde</c>, CST 03).</summary>
[Tag("COFINSQtde", "S03")]
public sealed class CofinsQuantidade : Cofins
{
    /// <summary>A quantidade vendida.</summary>
    [Tag("qBCProd", "S09")]
    public decimal? Quantidade { get; set; }

    /// <summary>A alíquota em reais, por unidade.</summary>
    [Tag("vAliqProd", "S10")]
    public decimal? AliquotaUnidade { get; set; }

    /// <summary>O valor da COFINS.</summary>
    [Tag("vCOFINS", "S11")]
    public decimal? Valor { get; set; }
}
