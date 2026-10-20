namespace OpenDFe.NFe;

/// <summary>Uma duplicata (<c>dup</c>).</summary>
public sealed class Duplicata
{
    /// <summary>O número; opcional.</summary>
    [Tag("nDup", "Y08")]
    public string? Numero { get; set; }

    /// <summary>A data de vencimento; opcional.</summary>
    [Tag("dVenc", "Y09")]
    public DateOnly? Vencimento { get; set; }

    /// <summary>O valor.</summary>
    [Tag("vDup", "Y10")]
    public decimal? Valor { get; set; }
}
