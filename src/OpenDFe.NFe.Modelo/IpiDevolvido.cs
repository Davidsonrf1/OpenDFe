namespace OpenDFe.NFe;

/// <summary>O IPI devolvido (<c>impostoDevol/IPI</c>).</summary>
public sealed class IpiDevolvido
{
    /// <summary>O valor do IPI devolvido.</summary>
    [Tag("vIPIDevol", "UA04")]
    public decimal? Valor { get; set; }
}
