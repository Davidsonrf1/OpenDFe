namespace OpenDFe.NFe;

/// <summary>Um crédito presumido do ICMS do produto (<c>gCred</c>).</summary>
public sealed class CreditoPresumidoIcms
{
    /// <summary>O código de benefício fiscal do crédito presumido.</summary>
    [Tag("cCredPresumido", "I05h")]
    public string? Codigo { get; set; }

    /// <summary>O percentual do crédito presumido.</summary>
    [Tag("pCredPresumido", "I05i")]
    public decimal? Percentual { get; set; }

    /// <summary>O valor do crédito presumido.</summary>
    [Tag("vCredPresumido", "I05j")]
    public decimal? Valor { get; set; }
}
