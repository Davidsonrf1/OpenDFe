namespace OpenDFe.NFe;

/// <summary>Um lote do produto, para a rastreabilidade (<c>rastro</c>).</summary>
public sealed class Rastro
{
    /// <summary>O número do lote.</summary>
    [Tag("nLote", "I81")]
    public string? Lote { get; set; }

    /// <summary>A quantidade do produto no lote.</summary>
    [Tag("qLote", "I82")]
    public decimal? Quantidade { get; set; }

    /// <summary>A data de fabricação ou produção.</summary>
    [Tag("dFab", "I83")]
    public DateOnly? Fabricacao { get; set; }

    /// <summary>A data de validade.</summary>
    [Tag("dVal", "I84")]
    public DateOnly? Validade { get; set; }

    /// <summary>O código de agregação; opcional.</summary>
    [Tag("cAgreg", "I85")]
    public string? CodigoAgregacao { get; set; }
}
