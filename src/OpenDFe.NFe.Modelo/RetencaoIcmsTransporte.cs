namespace OpenDFe.NFe;

/// <summary>A retenção do ICMS do transporte (<c>retTransp</c>).</summary>
public sealed class RetencaoIcmsTransporte
{
    /// <summary>O valor do serviço.</summary>
    [Tag("vServ", "X12")]
    public decimal? ValorServico { get; set; }

    /// <summary>A base de cálculo da retenção.</summary>
    [Tag("vBCRet", "X13")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota da retenção.</summary>
    [Tag("pICMSRet", "X14")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ICMS retido.</summary>
    [Tag("vICMSRet", "X15")]
    public decimal? Valor { get; set; }

    /// <summary>O CFOP do serviço de transporte.</summary>
    [Tag("CFOP", "X16")]
    public int? Cfop { get; set; }

    /// <summary>O município do fato gerador do ICMS do transporte (IBGE).</summary>
    [Tag("cMunFG", "X17")]
    public int? MunicipioFatoGerador { get; set; }
}
