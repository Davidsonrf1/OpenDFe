namespace OpenDFe.NFe;

/// <summary>O ICMS do Simples Nacional de outras situações (<c>ICMSSN900</c>, CSOSN 900).</summary>
[Tag("ICMSSN900", "N10h")]
public sealed class IcmsSn900 : Icms
{
    /// <summary>O CSOSN (<c>900</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CSOSN", "N12a")]
    public CsosnIcms? Csosn { get; set; }

    /// <summary>A modalidade de determinação da base de cálculo; opcional.</summary>
    [Tag("modBC", "N13")]
    public ModalidadeBaseCalculo? ModalidadeBaseCalculo { get; set; }

    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "N15")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O percentual de redução da base de cálculo; opcional.</summary>
    [Tag("pRedBC", "N14")]
    public decimal? PercentualReducaoBase { get; set; }

    /// <summary>A alíquota; opcional.</summary>
    [Tag("pICMS", "N16")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ICMS; opcional.</summary>
    [Tag("vICMS", "N17")]
    public decimal? Valor { get; set; }

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

    /// <summary>A alíquota do crédito que pode ser aproveitado (Simples Nacional); opcional.</summary>
    [Tag("pCredSN", "N29")]
    public decimal? AliquotaCredito { get; set; }

    /// <summary>O valor do crédito que pode ser aproveitado (Simples Nacional); opcional.</summary>
    [Tag("vCredICMSSN", "N30")]
    public decimal? ValorCredito { get; set; }
}
