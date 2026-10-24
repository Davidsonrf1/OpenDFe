using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// A conferência da licença do QuestPDF. O pacote não escolhe a licença (é estado global e escolha do aplicativo,
/// PROJETO.md 10.2): confere se o aplicativo escolheu e, se não, explica como em vez de deixar o erro do QuestPDF.
/// </summary>
internal static class LicencaQuestPdf
{
    /// <summary>O endereço do README do pacote, onde a licença é explicada.</summary>
    public const string EnderecoReadme = "https://github.com/Davidsonrf1/OpenDFe/blob/main/src/OpenDFe.Impressao.Pdf/README.md";

    /// <summary>Confere a licença definida no QuestPDF; <see cref="InvalidOperationException"/> se não houver.</summary>
    public static void Exigir() => Exigir(QuestPDF.Settings.License);

    /// <summary>Confere a licença informada (nula: não definida); <see cref="InvalidOperationException"/> se não houver.</summary>
    public static void Exigir(LicenseType? licenca)
    {
        if (licenca is null)
            throw new InvalidOperationException(
                "O QuestPDF, que gera o PDF, exige que o aplicativo escolha a licença dele antes do primeiro uso: " +
                "defina QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community; na partida do aplicativo " +
                "(ou a licença comercial que se aplica, se for o caso). " +
                $"As condições de cada licença estão no README do pacote: {EnderecoReadme}");
    }
}
