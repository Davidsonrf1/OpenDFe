namespace OpenDFe.NFe;

/// <summary>O ICMS tributado e com cobrança por substituição tributária (<c>ICMS10</c>, CST 10).</summary>
[Tag("ICMS10", "N03")]
public sealed class Icms10 : Icms
{
    /// <summary>O CST (<c>10</c>). Nulo na escrita: o único valor do grupo.</summary>
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

    /// <summary>A base de cálculo do FCP; opcional.</summary>
    [Tag("vBCFCP", "N17a")]
    public decimal? BaseCalculoFcp { get; set; }

    /// <summary>O percentual do FCP (fundo de combate à pobreza); opcional.</summary>
    [Tag("pFCP", "N17b")]
    public decimal? PercentualFcp { get; set; }

    /// <summary>O valor do FCP; opcional.</summary>
    [Tag("vFCP", "N17c")]
    public decimal? ValorFcp { get; set; }

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

    /// <summary>O valor do ICMS ST desonerado; opcional.</summary>
    [Tag("vICMSSTDeson", "N33a")]
    public decimal? ValorStDesonerado { get; set; }

    /// <summary>O motivo da desoneração do ICMS ST; opcional.</summary>
    [Tag("motDesICMSST", "N33b")]
    public MotivoDesoneracao? MotivoDesoneracaoSt { get; set; }
}
