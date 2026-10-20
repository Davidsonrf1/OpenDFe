namespace OpenDFe.NFe;

/// <summary>O ICMS do Simples Nacional com permissão de crédito (<c>ICMSSN101</c>, CSOSN 101).</summary>
[Tag("ICMSSN101", "N10c")]
public sealed class IcmsSn101 : Icms
{
    /// <summary>O CSOSN (<c>101</c>). Nulo na escrita: o único valor do grupo.</summary>
    [Tag("CSOSN", "N12a")]
    public CsosnIcms? Csosn { get; set; }

    /// <summary>A alíquota do crédito que pode ser aproveitado (Simples Nacional).</summary>
    [Tag("pCredSN", "N29")]
    public decimal? AliquotaCredito { get; set; }

    /// <summary>O valor do crédito que pode ser aproveitado (Simples Nacional).</summary>
    [Tag("vCredICMSSN", "N30")]
    public decimal? ValorCredito { get; set; }
}
