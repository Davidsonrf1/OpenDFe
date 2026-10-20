namespace OpenDFe.NFe;

/// <summary>
/// Uma NF-e referenciada pela chave com o código numérico zerado, para manter o sigilo dela (<c>refNFeSig</c>).
/// </summary>
public sealed class NFeSigilosaReferenciada : DocumentoReferenciado
{
    /// <summary>A chave de acesso, com o <c>cNF</c> zerado.</summary>
    [Tag("refNFeSig", "BA02a")]
    public string? Chave { get; set; }
}
