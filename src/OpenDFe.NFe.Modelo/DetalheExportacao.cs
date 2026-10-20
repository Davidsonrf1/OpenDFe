namespace OpenDFe.NFe;

/// <summary>Um detalhe da exportação do produto (<c>detExport</c>).</summary>
public sealed class DetalheExportacao
{
    /// <summary>O número do ato concessório de drawback; opcional.</summary>
    [Tag("nDraw", "I51")]
    public string? Drawback { get; set; }

    /// <summary>A exportação indireta; opcional.</summary>
    [Tag("exportInd", "I52")]
    public ExportacaoIndireta? Indireta { get; set; }
}
