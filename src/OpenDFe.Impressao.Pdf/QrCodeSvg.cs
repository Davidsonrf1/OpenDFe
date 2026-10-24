using QRCoder;

namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// O QR Code como SVG vetorial. Nível de correção M e UTF-8, como o manual do DANFE NFC-e manda; o SVG sai sem borda:
/// a zona de silêncio de 4 módulos é desenhada por quem usa o QR Code.
/// </summary>
internal static class QrCodeSvg
{
    /// <summary>
    /// A matriz de módulos do texto (<c>true</c> é escuro), indexada [linha, coluna], sem a zona de silêncio que o
    /// QRCoder acrescenta ao redor.
    /// </summary>
    public static bool[,] Matriz(string texto)
    {
        ArgumentException.ThrowIfNullOrEmpty(texto);
        using var gerador = new QRCodeGenerator();
        using var dados = gerador.CreateQrCode(texto, QRCodeGenerator.ECCLevel.M, forceUtf8: true);
        var lado = 17 + 4 * dados.Version;
        var margem = (dados.ModuleMatrix.Count - lado) / 2;
        var matriz = new bool[lado, lado];
        for (var y = 0; y < lado; y++)
            for (var x = 0; x < lado; x++)
                matriz[y, x] = dados.ModuleMatrix[y + margem][x + margem];
        return matriz;
    }

    /// <summary>O SVG do QR Code do <paramref name="texto"/>.</summary>
    public static string Gerar(string texto) => ImagemBitSvg.Gerar(ImagemBit.Criar(Matriz(texto)));
}
