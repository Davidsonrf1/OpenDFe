namespace OpenDFe.NFe;

/// <summary>O item de outro documento fiscal a que este item se refere (<c>DFeReferenciado</c>).</summary>
public sealed class ItemReferenciado
{
    /// <summary>A chave de acesso do documento referenciado.</summary>
    [Tag("chaveAcesso", "VC02")]
    public string? Chave { get; set; }

    /// <summary>O número do item no documento referenciado; opcional.</summary>
    [Tag("nItem", "VC03")]
    public int? NumeroItem { get; set; }
}
