namespace OpenDFe.NFe;

/// <summary>Uma NF-e ou NFC-e referenciada pela chave de acesso (<c>refNFe</c>).</summary>
public sealed class NFeReferenciada : DocumentoReferenciado
{
    /// <summary>A chave de acesso (44 caracteres).</summary>
    [Tag("refNFe", "BA02")]
    public string? Chave { get; set; }
}
