namespace OpenDFe.NFSe;

/// <summary>O total aproximado dos tributos (<c>totTrib</c>): os valores, os percentuais, o indicador de não informar ou o percentual do Simples Nacional.</summary>
public sealed class TotalTributos
{
    /// <summary>Os valores; um dos quatro.</summary>
    [Tag("vTotTrib")]
    public TotalTributosValor? Valores { get; set; }

    /// <summary>Os percentuais.</summary>
    [Tag("pTotTrib")]
    public TotalTributosPercentual? Percentuais { get; set; }

    /// <summary>O indicador de não informar o total (Decreto 8.264/2014).</summary>
    [Tag("indTotTrib")]
    public IndicadorTotalTributos? Indicador { get; set; }

    /// <summary>O percentual aproximado do total dos tributos da alíquota do Simples Nacional.</summary>
    [Tag("pTotTribSN")]
    public decimal? PercentualSimples { get; set; }
}
