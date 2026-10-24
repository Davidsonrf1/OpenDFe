namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// As opções do PDF: os metadados do arquivo e a família de fonte. O conteúdo e o papel vêm do
/// <see cref="DocumentoImpressao"/>; as opções não mudam o que é impresso.
/// </summary>
public sealed class OpcoesPdf
{
    /// <summary>O título do PDF; <see langword="null"/> usa o <see cref="DocumentoImpressao.Titulo"/> do documento.</summary>
    public string? Titulo { get; init; }

    /// <summary>O autor do PDF; <see langword="null"/> não grava autor.</summary>
    public string? Autor { get; init; }

    /// <summary>O criador do PDF (o programa que o gerou).</summary>
    public string Criador { get; init; } = "OpenDFe";

    /// <summary>
    /// A família de fonte registrada pelo aplicativo (por exemplo <c>Times New Roman</c>, que o Anexo II do MOC pede para o
    /// DANFE), ou <see langword="null"/> para a fonte embutida do QuestPDF (Lato). O pacote não registra fonte: o aplicativo
    /// a registra uma vez, na partida, antes do primeiro PDF (README do pacote). Se a família pedida não estiver registrada
    /// (nem ativada em <c>QuestPDF.Settings.UseSystemFonts</c>), o PDF sai em Lato: a emissão não quebra por falta de fonte.
    /// </summary>
    public string? Fonte { get; init; }
}
