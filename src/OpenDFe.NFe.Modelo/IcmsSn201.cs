namespace OpenDFe.NFe;

/// <summary>
/// O ICMS do Simples Nacional com permissão de crédito e cobrança por substituição tributária (<c>ICMSSN201</c>, CSOSN
/// 201).
/// </summary>
[Tag("ICMSSN201", "N10e")]
public sealed class IcmsSn201 : Icms
{
    /// <summary>O CSOSN (<c>201</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CSOSN", "N12a")]
    public CsosnIcms? Csosn { get; set; }

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

    /// <summary>A alíquota do crédito que pode ser aproveitado (Simples Nacional).</summary>
    [Tag("pCredSN", "N29")]
    public decimal? AliquotaCredito { get; set; }

    /// <summary>O valor do crédito que pode ser aproveitado (Simples Nacional).</summary>
    [Tag("vCredICMSSN", "N30")]
    public decimal? ValorCredito { get; set; }
}
