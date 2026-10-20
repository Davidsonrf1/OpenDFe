namespace OpenDFe.NFe;

/// <summary>O ICMS com diferimento (<c>ICMS51</c>, CST 51).</summary>
[Tag("ICMS51", "N07")]
public sealed class Icms51 : Icms
{
    /// <summary>O CST (<c>51</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A modalidade de determinação da base de cálculo; opcional.</summary>
    [Tag("modBC", "N13")]
    public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }

    /// <summary>O percentual de redução da base de cálculo; opcional.</summary>
    [Tag("pRedBC", "N14")]
    public decimal? PercentualReducaoBase { get; set; }

    /// <summary>O código de benefício fiscal da redução da base de cálculo; opcional.</summary>
    [Tag("cBenefRBC", "N14a")]
    public string? CodigoBeneficioReducao { get; set; }

    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "N15")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota; opcional.</summary>
    [Tag("pICMS", "N16")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ICMS da operação; opcional.</summary>
    [Tag("vICMSOp", "N16a")]
    public decimal? ValorOperacao { get; set; }

    /// <summary>O percentual do diferimento; opcional.</summary>
    [Tag("pDif", "N16b")]
    public decimal? PercentualDiferimento { get; set; }

    /// <summary>O valor do ICMS diferido; opcional.</summary>
    [Tag("vICMSDif", "N16c")]
    public decimal? ValorDiferido { get; set; }

    /// <summary>O valor do ICMS; opcional.</summary>
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

    /// <summary>O percentual do diferimento do FCP; opcional.</summary>
    [Tag("pFCPDif", "N17d")]
    public decimal? PercentualDiferimentoFcp { get; set; }

    /// <summary>O valor do FCP diferido; opcional.</summary>
    [Tag("vFCPDif", "N17e")]
    public decimal? ValorFcpDiferido { get; set; }

    /// <summary>O valor efetivo do FCP; opcional.</summary>
    [Tag("vFCPEfet", "N17f")]
    public decimal? ValorFcpEfetivo { get; set; }
}
