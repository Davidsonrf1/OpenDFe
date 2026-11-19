namespace OpenDFe.NFSe;

/// <summary>O documento não fiscal (<c>docOutro</c>).</summary>
public sealed class DocumentoOutro
{
    /// <summary>O número do documento.</summary>
    [Tag("nDoc")]
    public string? Numero { get; set; }

    /// <summary>A descrição do documento.</summary>
    [Tag("xDoc")]
    public string? Descricao { get; set; }
}
