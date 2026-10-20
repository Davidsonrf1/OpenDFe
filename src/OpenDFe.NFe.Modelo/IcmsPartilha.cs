namespace OpenDFe.NFe;

/// <summary>
/// A partilha do ICMS entre a UF de origem e a de destino, ou a definida na legislação (<c>ICMSPart</c>, CST 10 ou 90).
/// </summary>
[Tag("ICMSPart", "N10a")]
public sealed class IcmsPartilha : Icms
{
    /// <summary>O CST: <c>10</c>, <c>20</c> ou <c>90</c>.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A modalidade de determinação da base de cálculo.</summary>
    [Tag("modBC", "N13")]
    public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }

    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "N15")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O percentual de redução da base de cálculo; opcional.</summary>
    [Tag("pRedBC", "N14")]
    public decimal? PercentualReducaoBase { get; set; }

    /// <summary>A alíquota.</summary>
    [Tag("pICMS", "N16")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ICMS.</summary>
    [Tag("vICMS", "N17")]
    public decimal? Valor { get; set; }

    /// <summary>A modalidade de determinação da base de cálculo do ICMS ST.</summary>
    [Tag("modBCST", "N18")]
    public ModalidadeBaseCalculoSt? ModalidadeBaseCalculoSt { get; set; }

    /// <summary>O percentual da margem de valor adicionado do ICMS ST; opcional.</summary>
    [Tag("pMVAST", "N19")]
    public decimal? PercentualMargemSt { get; set; }

    /// <summary>O percentual de redução da base de cálculo do ICMS ST; opcional.</summary>
    [Tag("pRedBCST", "N20")]
    public decimal? PercentualReducaoBaseSt { get; set; }

    /// <summary>A base de cálculo do ICMS ST.</summary>
    [Tag("vBCST", "N21")]
    public decimal? BaseCalculoSt { get; set; }

    /// <summary>A alíquota do ICMS ST.</summary>
    [Tag("pICMSST", "N22")]
    public decimal? AliquotaSt { get; set; }

    /// <summary>O valor do ICMS ST.</summary>
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

    /// <summary>O percentual da base de cálculo da operação própria.</summary>
    [Tag("pBCOp", "N25")]
    public decimal? PercentualBaseOperacao { get; set; }

    /// <summary>A UF a que é devido o ICMS ST.</summary>
    [Tag("UFST", "N24")]
    public UF? UFSt { get; set; }

    /// <summary>
    /// Se a UF do ICMS ST do XML é <c>EX</c> (exterior); com ela, <see cref="UFSt"/> fica nulo. Sem
    /// <see cref="TagAttribute"/>: é outro valor do mesmo campo <c>UFST</c>.
    /// </summary>
    public bool Exterior { get; set; }

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
