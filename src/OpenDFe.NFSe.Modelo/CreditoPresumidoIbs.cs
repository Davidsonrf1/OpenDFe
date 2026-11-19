namespace OpenDFe.NFSe;

/// <summary>O crédito presumido do IBS (<c>gIBSCredPres</c>).</summary>
public sealed class CreditoPresumidoIbs
{
    /// <summary>A alíquota (%).</summary>
    [Tag("pCredPresIBS")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor.</summary>
    [Tag("vCredPresIBS")]
    public decimal? Valor { get; set; }
}
