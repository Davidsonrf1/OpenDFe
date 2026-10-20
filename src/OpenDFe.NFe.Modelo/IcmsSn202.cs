namespace OpenDFe.NFe;

/// <summary>
/// O ICMS do Simples Nacional sem permissão de crédito e com cobrança por substituição tributária (<c>ICMSSN202</c>,
/// CSOSN 202 ou 203).
/// </summary>
[Tag("ICMSSN202", "N10f")]
public sealed class IcmsSn202 : Icms
{
    /// <summary>O CSOSN: <c>202</c> ou <c>203</c>.</summary>
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
}
