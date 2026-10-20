namespace OpenDFe.NFe;

/// <summary>
/// O repasse do ICMS ST retido anteriormente em operações interestaduais (<c>ICMSST</c>, CST 41 ou 60).
/// </summary>
[Tag("ICMSST", "N10b")]
public sealed class IcmsRepasse : Icms
{
    /// <summary>O CST: <c>41</c> ou <c>60</c>.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>A base de cálculo do ICMS ST retido anteriormente.</summary>
    [Tag("vBCSTRet", "N26")]
    public decimal? BaseCalculoStRetido { get; set; }

    /// <summary>A alíquota suportada pelo consumidor final; opcional.</summary>
    [Tag("pST", "N26a")]
    public decimal? AliquotaConsumidorFinal { get; set; }

    /// <summary>O valor do ICMS próprio do substituto; opcional.</summary>
    [Tag("vICMSSubstituto", "N26b")]
    public decimal? ValorSubstituto { get; set; }

    /// <summary>O valor do ICMS ST retido anteriormente.</summary>
    [Tag("vICMSSTRet", "N27")]
    public decimal? ValorStRetido { get; set; }

    /// <summary>A base de cálculo do FCP retido anteriormente por substituição tributária; opcional.</summary>
    [Tag("vBCFCPSTRet", "N27a")]
    public decimal? BaseCalculoFcpStRetido { get; set; }

    /// <summary>O percentual do FCP retido anteriormente por substituição tributária; opcional.</summary>
    [Tag("pFCPSTRet", "N27b")]
    public decimal? PercentualFcpStRetido { get; set; }

    /// <summary>O valor do FCP retido anteriormente por substituição tributária; opcional.</summary>
    [Tag("vFCPSTRet", "N27d")]
    public decimal? ValorFcpStRetido { get; set; }

    /// <summary>A base de cálculo do ICMS ST da UF de destino.</summary>
    [Tag("vBCSTDest", "N31")]
    public decimal? BaseCalculoStDestino { get; set; }

    /// <summary>O valor do ICMS ST da UF de destino.</summary>
    [Tag("vICMSSTDest", "N32")]
    public decimal? ValorStDestino { get; set; }

    /// <summary>O percentual de redução da base de cálculo efetiva; opcional.</summary>
    [Tag("pRedBCEfet", "N34")]
    public decimal? PercentualReducaoBaseEfetiva { get; set; }

    /// <summary>A base de cálculo efetiva; opcional.</summary>
    [Tag("vBCEfet", "N35")]
    public decimal? BaseCalculoEfetiva { get; set; }

    /// <summary>A alíquota do ICMS efetiva; opcional.</summary>
    [Tag("pICMSEfet", "N36")]
    public decimal? AliquotaEfetiva { get; set; }

    /// <summary>O valor do ICMS efetivo; opcional.</summary>
    [Tag("vICMSEfet", "N37")]
    public decimal? ValorEfetivo { get; set; }
}
