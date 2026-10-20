namespace OpenDFe.NFe;

/// <summary>Os totais do ISSQN (<c>ISSQNtot</c>).</summary>
public sealed class TotalIssqn
{
    /// <summary>O total dos serviços sob não incidência ou não tributados pelo ICMS; opcional.</summary>
    [Tag("vServ", "W18")]
    public decimal? ValorServicos { get; set; }

    /// <summary>O total da base de cálculo do ISS; opcional.</summary>
    [Tag("vBC", "W19")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O total do ISS; opcional.</summary>
    [Tag("vISS", "W20")]
    public decimal? Valor { get; set; }

    /// <summary>O total do PIS sobre os serviços; opcional.</summary>
    [Tag("vPIS", "W21")]
    public decimal? ValorPis { get; set; }

    /// <summary>O total da COFINS sobre os serviços; opcional.</summary>
    [Tag("vCOFINS", "W22")]
    public decimal? ValorCofins { get; set; }

    /// <summary>A data da prestação do serviço.</summary>
    [Tag("dCompet", "W22a")]
    public DateOnly? Competencia { get; set; }

    /// <summary>O total da dedução, para a redução da base de cálculo; opcional.</summary>
    [Tag("vDeducao", "W22b")]
    public decimal? Deducao { get; set; }

    /// <summary>O total de outras retenções; opcional.</summary>
    [Tag("vOutro", "W22c")]
    public decimal? OutrasRetencoes { get; set; }

    /// <summary>O total do desconto incondicionado; opcional.</summary>
    [Tag("vDescIncond", "W22d")]
    public decimal? DescontoIncondicionado { get; set; }

    /// <summary>O total do desconto condicionado; opcional.</summary>
    [Tag("vDescCond", "W22e")]
    public decimal? DescontoCondicionado { get; set; }

    /// <summary>O total da retenção do ISS; opcional.</summary>
    [Tag("vISSRet", "W22f")]
    public decimal? Retencao { get; set; }

    /// <summary>O regime especial de tributação; opcional.</summary>
    [Tag("cRegTrib", "W22g")]
    public RegimeEspecialTributacao? RegimeEspecial { get; set; }
}
