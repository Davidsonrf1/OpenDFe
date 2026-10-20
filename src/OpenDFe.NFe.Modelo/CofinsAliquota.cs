namespace OpenDFe.NFe;

/// <summary>A COFINS tributada pela alíquota (<c>COFINSAliq</c>, CST 01 ou 02).</summary>
[Tag("COFINSAliq", "S02")]
public sealed class CofinsAliquota : Cofins
{
    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "S07")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota, em percentual.</summary>
    [Tag("pCOFINS", "S08")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor da COFINS.</summary>
    [Tag("vCOFINS", "S11")]
    public decimal? Valor { get; set; }
}
