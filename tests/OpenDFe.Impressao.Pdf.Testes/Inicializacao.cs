using System.Runtime.CompilerServices;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf.Testes;

internal static class Inicializacao
{
    // A licença do QuestPDF é escolha do aplicativo (PROJETO.md 10.2): o assembly de teste a define uma vez, na carga.
    // O projeto OpenDFe é open source (MIT), o que a Community do QuestPDF cobre (README do pacote).
    [ModuleInitializer]
    internal static void DefinirLicenca() => QuestPDF.Settings.License = LicenseType.Community;
}
