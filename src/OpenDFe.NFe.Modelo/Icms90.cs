namespace OpenDFe.NFe;

/// <summary>O ICMS de outras situações (<c>ICMS90</c>, CST 90).</summary>
[Tag("ICMS90", "N10")]
public sealed class Icms90 : Icms
{
    /// <summary>O CST (<c>90</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A modalidade de determinação da base de cálculo; opcional.</summary>
    [Tag("modBC", "N13")]
    public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }

    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "N15")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O percentual de redução da base de cálculo; opcional.</summary>
    [Tag("pRedBC", "N14")]
    public decimal? PercentualReducaoBase { get; set; }

    /// <summary>O código de benefício fiscal da redução da base de cálculo; opcional.</summary>
    [Tag("cBenefRBC", "N14a")]
    public string? CodigoBeneficioReducao { get; set; }

    /// <summary>A alíquota; opcional.</summary>
    [Tag("pICMS", "N16")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ICMS da operação; opcional.</summary>
    [Tag("vICMSOp", "N16b")]
    public decimal? ValorOperacao { get; set; }

    /// <summary>O percentual do diferimento; opcional.</summary>
    [Tag("pDif", "N16c")]
    public decimal? PercentualDiferimento { get; set; }

    /// <summary>O valor do ICMS diferido; opcional.</summary>
    [Tag("vICMSDif", "N16d")]
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

    /// <summary>A modalidade de determinação da base de cálculo do ICMS ST; opcional.</summary>
    [Tag("modBCST", "N18")]
    public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }

    /// <summary>O percentual da margem de valor adicionado do ICMS ST; opcional.</summary>
    [Tag("pMVAST", "N19")]
    public decimal? PercentualMargemSt { get; set; }

    /// <summary>O percentual de redução da base de cálculo do ICMS ST; opcional.</summary>
    [Tag("pRedBCST", "N20")]
    public decimal? PercentualReducaoBaseSt { get; set; }

    /// <summary>A base de cálculo do ICMS ST; opcional.</summary>
    [Tag("vBCST", "N21")]
    public decimal? BaseCalculoSt { get; set; }

    /// <summary>A alíquota do ICMS ST; opcional.</summary>
    [Tag("pICMSST", "N22")]
    public decimal? AliquotaSt { get; set; }

    /// <summary>O valor do ICMS ST; opcional.</summary>
    [Tag("vICMSST", "N23")]
    public decimal? ValorSt { get; set; }

    /// <summary>A base de cálculo do FCP retido por substituição tributária; opcional.</summary>
    [Tag("vBCFCPST", "N23a")]
    public decimal? BaseCalculoFcpSt { get; set; }

    /// <summary>O percentual do FCP retido por substituição tributária; opcional.</summary>
    [Tag("pFCPST", "N23b")]
    public decimal? PercentualFcpSt { get; set; }

    /// <summary>O valor do FCP retido por substituição tributária; opcional.</summary>
    [Tag("vFCPST", "N23d")]
    public decimal? ValorFcpSt { get; set; }

    /// <summary>O valor do ICMS desonerado; opcional.</summary>
    [Tag("vICMSDeson", "N28a")]
    public decimal? ValorDesonerado { get; set; }

    /// <summary>O motivo da desoneração do ICMS; opcional.</summary>
    [Tag("motDesICMS", "N28")]
    public MotivoDesoneracao? MotivoDesoneracao { get; set; }

    /// <summary>Se o ICMS desonerado deduz do valor do item e do total da nota; opcional.</summary>
    [Tag("indDeduzDeson", "N28b")]
    public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }

    /// <summary>O valor do ICMS ST desonerado; opcional.</summary>
    [Tag("vICMSSTDeson", "N33a")]
    public decimal? ValorStDesonerado { get; set; }

    /// <summary>O motivo da desoneração do ICMS ST; opcional.</summary>
    [Tag("motDesICMSST", "N33b")]
    public MotivoDesoneracao? MotivoDesoneracaoSt { get; set; }
}
