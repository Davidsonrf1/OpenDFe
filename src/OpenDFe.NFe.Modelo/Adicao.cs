namespace OpenDFe.NFe;

/// <summary>Uma adição da declaração de importação (<c>adi</c>).</summary>
public sealed class Adicao
{
    /// <summary>O número da adição; opcional.</summary>
    [Tag("nAdicao", "I26")]
    public int? Numero { get; set; }

    /// <summary>O número sequencial do item na adição.</summary>
    [Tag("nSeqAdic", "I27")]
    public int? Sequencial { get; set; }

    /// <summary>O código do fabricante estrangeiro.</summary>
    [Tag("cFabricante", "I28")]
    public string? CodigoFabricante { get; set; }

    /// <summary>O valor do desconto do item na DI; opcional.</summary>
    [Tag("vDescDI", "I29")]
    public decimal? Desconto { get; set; }

    /// <summary>O número do ato concessório de drawback; opcional.</summary>
    [Tag("nDraw", "I29a")]
    public string? Drawback { get; set; }
}
