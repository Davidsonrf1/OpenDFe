namespace OpenDFe.NFSe;

/// <summary>O documento fiscal fora do repositório nacional (<c>docFiscalOutro</c>).</summary>
public sealed class DocumentoFiscalOutro
{
    /// <summary>O código IBGE do município emissor.</summary>
    [Tag("cMunDocFiscal")]
    public int? Municipio { get; set; }

    /// <summary>O número do documento.</summary>
    [Tag("nDocFiscal")]
    public string? Numero { get; set; }

    /// <summary>A descrição do documento.</summary>
    [Tag("xDocFiscal")]
    public string? Descricao { get; set; }
}
