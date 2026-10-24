using System.Runtime.CompilerServices;
using QuestPDF.Infrastructure;

namespace OpenDFe.Testes;

internal static class LicencaPdf
{
    // A licença do QuestPDF é escolha do aplicativo (PROJETO.md 10.2): o assembly de teste a define uma vez, na carga. O
    // projeto OpenDFe é open source (MIT), o que a Community do QuestPDF cobre (README do pacote OpenDFe.Impressao.Pdf).
    [ModuleInitializer]
    internal static void Definir() => QuestPDF.Settings.License = LicenseType.Community;
}
