namespace OpenDFe.NFe;

/// <summary>O total do imposto seletivo (<c>ISTot</c>).</summary>
public sealed class TotalImpostoSeletivo
{
    /// <summary>O total do imposto seletivo.</summary>
    [Tag("vIS", "W33")]
    public decimal? Valor { get; set; }
}
