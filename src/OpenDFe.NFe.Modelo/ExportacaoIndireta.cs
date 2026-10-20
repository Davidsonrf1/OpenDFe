namespace OpenDFe.NFe;

/// <summary>A exportação indireta (<c>exportInd</c>).</summary>
public sealed class ExportacaoIndireta
{
    /// <summary>O número do registro de exportação.</summary>
    [Tag("nRE", "I53")]
    public string? RegistroExportacao { get; set; }

    /// <summary>A chave da NF-e recebida para exportação.</summary>
    [Tag("chNFe", "I54")]
    public string? Chave { get; set; }

    /// <summary>A quantidade do item efetivamente exportada.</summary>
    [Tag("qExport", "I55")]
    public decimal? Quantidade { get; set; }
}
