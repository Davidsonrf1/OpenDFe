namespace OpenDFe.NFe;

/// <summary>O ICMS com redução de base de cálculo (<c>ICMS20</c>, CST 20).</summary>
[Tag("ICMS20", "N04")]
public sealed class Icms20 : Icms
{
    /// <summary>O CST (<c>20</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A modalidade de determinação da base de cálculo.</summary>
    [Tag("modBC", "N13")]
    public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }

    /// <summary>O percentual de redução da base de cálculo.</summary>
    [Tag("pRedBC", "N14")]
    public decimal? PercentualReducaoBase { get; set; }

    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "N15")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota.</summary>
    [Tag("pICMS", "N16")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ICMS.</summary>
    [Tag("vICMS", "N17")]
    public decimal? Valor { get; set; }

    /// <summary>A base de cálculo do FCP; opcional.</summary>
    [Tag("vBCFCP", "N17a")]
    public decimal? BaseCalculoFcp { get; set; }

    /// <summary>O percentual do FCP (fundo de combate à pobreza); opcional.</summary>
    [Tag("pFCP", "N17b")]
    public decimal? PercentualFcp { get; set; }

    /// <summary>O valor do FCP; opcional.</summary>
    [Tag("vFCP", "N17c")]
    public decimal? ValorFcp { get; set; }

    /// <summary>O valor do ICMS desonerado; opcional.</summary>
    [Tag("vICMSDeson", "N28a")]
    public decimal? ValorDesonerado { get; set; }

    /// <summary>O motivo da desoneração do ICMS; opcional.</summary>
    [Tag("motDesICMS", "N28")]
    public MotivoDesoneracao? MotivoDesoneracao { get; set; }

    /// <summary>Se o ICMS desonerado deduz do valor do item e do total da nota; opcional.</summary>
    [Tag("indDeduzDeson", "N28b")]
    public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
}
