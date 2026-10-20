namespace OpenDFe.NFe;

/// <summary>O ICMS tributado integralmente (<c>ICMS00</c>, CST 00).</summary>
[Tag("ICMS00", "N02")]
public sealed class Icms00 : Icms
{
    /// <summary>O CST (<c>00</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A modalidade de determinação da base de cálculo.</summary>
    [Tag("modBC", "N13")]
    public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }

    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "N15")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota.</summary>
    [Tag("pICMS", "N16")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ICMS.</summary>
    [Tag("vICMS", "N17")]
    public decimal? Valor { get; set; }

    /// <summary>O percentual do FCP (fundo de combate à pobreza); opcional.</summary>
    [Tag("pFCP", "N17b")]
    public decimal? PercentualFcp { get; set; }

    /// <summary>O valor do FCP; opcional.</summary>
    [Tag("vFCP", "N17c")]
    public decimal? ValorFcp { get; set; }
}
