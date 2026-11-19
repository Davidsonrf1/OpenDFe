namespace OpenDFe.NFSe;

/// <summary>Os valores calculados pelo ambiente gerador (<c>valores</c>, <c>TCValoresNFSe</c>).</summary>
public sealed class ValoresNFSe
{
    /// <summary>O valor de dedução ou redução da base de cálculo; opcional.</summary>
    [Tag("vCalcDR")]
    public decimal? DeducaoReducao { get; set; }

    /// <summary>O tipo do benefício municipal (na versão 1.00, texto livre: a leitura aproveita só os códigos 1 a 4, docs/NFSE.md 7, M10); opcional.</summary>
    [Tag("tpBM")]
    public TipoBeneficio? TipoBeneficio { get; set; }

    /// <summary>O valor da redução pelo benefício municipal; opcional.</summary>
    [Tag("vCalcBM")]
    public decimal? ReducaoBeneficio { get; set; }

    /// <summary>A base de cálculo do ISSQN; opcional.</summary>
    [Tag("vBC")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota aplicada (%); opcional.</summary>
    [Tag("pAliqAplic")]
    public decimal? AliquotaAplicada { get; set; }

    /// <summary>O valor do ISSQN; opcional.</summary>
    [Tag("vISSQN")]
    public decimal? Issqn { get; set; }

    /// <summary>O total das retenções; opcional.</summary>
    [Tag("vTotalRet")]
    public decimal? TotalRetencoes { get; set; }

    /// <summary>O valor líquido.</summary>
    [Tag("vLiq")]
    public decimal? Liquido { get; set; }
}
