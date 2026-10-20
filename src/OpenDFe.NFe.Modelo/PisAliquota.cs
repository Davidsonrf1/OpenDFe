namespace OpenDFe.NFe;

/// <summary>O PIS tributado pela alíquota (<c>PISAliq</c>, CST 01 ou 02).</summary>
[Tag("PISAliq", "Q02")]
public sealed class PisAliquota : Pis
{
    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "Q07")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota, em percentual.</summary>
    [Tag("pPIS", "Q08")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do PIS.</summary>
    [Tag("vPIS", "Q09")]
    public decimal? Valor { get; set; }
}
