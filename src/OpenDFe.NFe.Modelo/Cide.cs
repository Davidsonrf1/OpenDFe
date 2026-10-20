namespace OpenDFe.NFe;

/// <summary>A CIDE dos combustíveis (<c>CIDE</c>).</summary>
public sealed class Cide
{
    /// <summary>A base de cálculo da CIDE: a quantidade comercializada.</summary>
    [Tag("qBCProd", "LA08")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota da CIDE, em reais por unidade.</summary>
    [Tag("vAliqProd", "LA09")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor da CIDE.</summary>
    [Tag("vCIDE", "LA10")]
    public decimal? Valor { get; set; }
}
