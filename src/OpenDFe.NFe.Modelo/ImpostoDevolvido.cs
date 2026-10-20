namespace OpenDFe.NFe;

/// <summary>O imposto devolvido do item (<c>impostoDevol</c>).</summary>
public sealed class ImpostoDevolvido
{
    /// <summary>O percentual da mercadoria devolvida.</summary>
    [Tag("pDevol", "UA02")]
    public decimal? Percentual { get; set; }

    /// <summary>O IPI devolvido.</summary>
    [Tag("IPI", "UA03")]
    public IpiDevolvido Ipi { get; set; } = new();
}
