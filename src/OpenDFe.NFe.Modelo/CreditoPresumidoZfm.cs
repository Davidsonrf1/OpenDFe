namespace OpenDFe.NFe;

/// <summary>O crédito presumido do IBS na Zona Franca de Manaus (<c>gCredPresIBSZFM</c>).</summary>
public sealed class CreditoPresumidoZfm
{
    /// <summary>O ano e o mês da competência de apuração, como no XML (<c>AAAA-MM</c>).</summary>
    [Tag("competApur", "UB132")]
    public string? Competencia { get; set; }

    /// <summary>A classificação para o crédito presumido do IBS na Zona Franca de Manaus.</summary>
    [Tag("tpCredPresIBSZFM", "UB133")]
    public ClassificacaoCreditoPresumidoZfm? Classificacao { get; set; }

    /// <summary>O valor do crédito presumido do IBS na Zona Franca de Manaus.</summary>
    [Tag("vCredPresIBSZFM", "UB134")]
    public decimal? Valor { get; set; }
}
