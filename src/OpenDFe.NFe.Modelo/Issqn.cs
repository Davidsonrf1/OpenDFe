namespace OpenDFe.NFe;

/// <summary>O ISSQN do item (<c>ISSQN</c>).</summary>
public sealed class Issqn
{
    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "U02")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota.</summary>
    [Tag("vAliq", "U03")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do ISSQN.</summary>
    [Tag("vISSQN", "U04")]
    public decimal? Valor { get; set; }

    /// <summary>O município de ocorrência do fato gerador (código do IBGE).</summary>
    [Tag("cMunFG", "U05")]
    public int? MunicipioFatoGerador { get; set; }

    /// <summary>O item da lista de serviços (LC 116/2003).</summary>
    [Tag("cListServ", "U06")]
    public string? ItemListaServico { get; set; }

    /// <summary>O valor da dedução, para a redução da base de cálculo; opcional.</summary>
    [Tag("vDeducao", "U07")]
    public decimal? Deducao { get; set; }

    /// <summary>O valor de outras retenções; opcional.</summary>
    [Tag("vOutro", "U08")]
    public decimal? OutrasRetencoes { get; set; }

    /// <summary>O valor do desconto incondicionado; opcional.</summary>
    [Tag("vDescIncond", "U09")]
    public decimal? DescontoIncondicionado { get; set; }

    /// <summary>O valor do desconto condicionado; opcional.</summary>
    [Tag("vDescCond", "U10")]
    public decimal? DescontoCondicionado { get; set; }

    /// <summary>O valor da retenção do ISS; opcional.</summary>
    [Tag("vISSRet", "U11")]
    public decimal? Retencao { get; set; }

    /// <summary>A exigibilidade do ISS.</summary>
    [Tag("indISS", "U12")]
    public ExigibilidadeIss? Exigibilidade { get; set; }

    /// <summary>O código do serviço no município; opcional.</summary>
    [Tag("cServico", "U13")]
    public string? CodigoServico { get; set; }

    /// <summary>O município de incidência do imposto (código do IBGE); opcional.</summary>
    [Tag("cMun", "U14")]
    public int? MunicipioIncidencia { get; set; }

    /// <summary>O país onde o serviço foi prestado (código do BACEN); opcional.</summary>
    [Tag("cPais", "U15")]
    public int? CodigoPais { get; set; }

    /// <summary>O número do processo judicial ou administrativo de suspensão da exigibilidade; opcional.</summary>
    [Tag("nProcesso", "U16")]
    public string? Processo { get; set; }

    /// <summary>Se há incentivo fiscal.</summary>
    [Tag("indIncentivo", "U17")]
    public IndicadorIncentivo? IncentivoFiscal { get; set; }
}
