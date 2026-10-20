namespace OpenDFe.NFe;

/// <summary>Os totais do ICMS e dos valores da nota (<c>ICMSTot</c>).</summary>
public sealed class TotalIcms
{
    /// <summary>O total da base de cálculo do ICMS.</summary>
    [Tag("vBC", "W03")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O total do ICMS.</summary>
    [Tag("vICMS", "W04")]
    public decimal? Valor { get; set; }

    /// <summary>O total do ICMS desonerado.</summary>
    [Tag("vICMSDeson", "W04a")]
    public decimal? ValorDesonerado { get; set; }

    /// <summary>O total do FCP da UF de destino; opcional.</summary>
    [Tag("vFCPUFDest", "W04c")]
    public decimal? ValorFcpUFDestino { get; set; }

    /// <summary>O total do ICMS de partilha para a UF de destino; opcional.</summary>
    [Tag("vICMSUFDest", "W04e")]
    public decimal? ValorIcmsUFDestino { get; set; }

    /// <summary>O total do ICMS de partilha para a UF do remetente; opcional.</summary>
    [Tag("vICMSUFRemet", "W04g")]
    public decimal? ValorIcmsUFRemetente { get; set; }

    /// <summary>O total do FCP.</summary>
    [Tag("vFCP", "W04h")]
    public decimal? ValorFcp { get; set; }

    /// <summary>O total da base de cálculo do ICMS ST.</summary>
    [Tag("vBCST", "W05")]
    public decimal? BaseCalculoSt { get; set; }

    /// <summary>O total do ICMS ST.</summary>
    [Tag("vST", "W06")]
    public decimal? ValorSt { get; set; }

    /// <summary>O total do FCP retido por substituição tributária.</summary>
    [Tag("vFCPST", "W06a")]
    public decimal? ValorFcpSt { get; set; }

    /// <summary>O total do FCP retido anteriormente por substituição tributária.</summary>
    [Tag("vFCPSTRet", "W06b")]
    public decimal? ValorFcpStRetido { get; set; }

    /// <summary>O total da quantidade tributada no ICMS monofásico próprio; opcional.</summary>
    [Tag("qBCMono", "W06b.1")]
    public decimal? QuantidadeBaseMonofasico { get; set; }

    /// <summary>O total do ICMS monofásico próprio; opcional.</summary>
    [Tag("vICMSMono", "W06c")]
    public decimal? ValorMonofasico { get; set; }

    /// <summary>O total da quantidade tributada no ICMS monofásico sujeito à retenção; opcional.</summary>
    [Tag("qBCMonoReten", "W06c.1")]
    public decimal? QuantidadeBaseRetencao { get; set; }

    /// <summary>O total do ICMS monofásico sujeito à retenção; opcional.</summary>
    [Tag("vICMSMonoReten", "W06d")]
    public decimal? ValorMonofasicoRetencao { get; set; }

    /// <summary>O total da quantidade tributada no ICMS monofásico retido anteriormente; opcional.</summary>
    [Tag("qBCMonoRet", "W06d.1")]
    public decimal? QuantidadeBaseRetida { get; set; }

    /// <summary>O total do ICMS monofásico retido anteriormente; opcional.</summary>
    [Tag("vICMSMonoRet", "W06e")]
    public decimal? ValorMonofasicoRetido { get; set; }

    /// <summary>O total dos produtos e serviços.</summary>
    [Tag("vProd", "W07")]
    public decimal? ValorProdutos { get; set; }

    /// <summary>O total do frete.</summary>
    [Tag("vFrete", "W08")]
    public decimal? Frete { get; set; }

    /// <summary>O total do seguro.</summary>
    [Tag("vSeg", "W09")]
    public decimal? Seguro { get; set; }

    /// <summary>O total do desconto.</summary>
    [Tag("vDesc", "W10")]
    public decimal? Desconto { get; set; }

    /// <summary>O total do imposto de importação.</summary>
    [Tag("vII", "W11")]
    public decimal? ValorImpostoImportacao { get; set; }

    /// <summary>O total do IPI.</summary>
    [Tag("vIPI", "W12")]
    public decimal? ValorIpi { get; set; }

    /// <summary>O total do IPI devolvido.</summary>
    [Tag("vIPIDevol", "W12a")]
    public decimal? ValorIpiDevolvido { get; set; }

    /// <summary>O total do PIS.</summary>
    [Tag("vPIS", "W13")]
    public decimal? ValorPis { get; set; }

    /// <summary>O total da COFINS.</summary>
    [Tag("vCOFINS", "W14")]
    public decimal? ValorCofins { get; set; }

    /// <summary>O total das outras despesas acessórias.</summary>
    [Tag("vOutro", "W15")]
    public decimal? OutrasDespesas { get; set; }

    /// <summary>O valor total da nota (<c>vNF</c>).</summary>
    [Tag("vNF", "W16")]
    public decimal? ValorNota { get; set; }

    /// <summary>O valor aproximado total dos tributos (Lei 12.741/2012); opcional.</summary>
    [Tag("vTotTrib", "W16a")]
    public decimal? ValorTotalTributos { get; set; }
}
