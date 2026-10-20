namespace OpenDFe.NFe;

/// <summary>Um CT-e referenciado pela chave de acesso (<c>refCTe</c>).</summary>
public sealed class CTeReferenciado : DocumentoReferenciado
{
    /// <summary>A chave de acesso do CT-e.</summary>
    [Tag("refCTe", "BA19")]
    public string? Chave { get; set; }
}
