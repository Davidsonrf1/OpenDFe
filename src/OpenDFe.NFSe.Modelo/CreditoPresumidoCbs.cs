namespace OpenDFe.NFSe;

/// <summary>O crédito presumido da CBS (<c>gCBSCredPres</c>).</summary>
public sealed class CreditoPresumidoCbs
{
    /// <summary>A alíquota (%).</summary>
    [Tag("pCredPresCBS")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor.</summary>
    [Tag("vCredPresCBS")]
    public decimal? Valor { get; set; }
}
