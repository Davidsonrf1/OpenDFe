using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

/// <summary>O renderizador PDF do modelo de impressão. Nesta versão, só o diagnóstico (<see cref="GerarPaginaTeste"/>).</summary>
public static class RenderizadorPdf
{
    // Uma chave de acesso de 44 dígitos (só o formato) e uma URL de NFC-e (fictícia) para o QR Code.
    private const string ChaveExemplo = "31261011222333000181650010000001231123456780";
    private const string UrlExemplo = "https://www.exemplo.gov.br/nfce/qrcode?p=31261011222333000181650010000001231123456780|3|1|1|ABCDEF";

    /// <summary>
    /// Gera um PDF de uma página A4 com texto acentuado, um Code 128, um QR Code e uma imagem de 1 bit: serve para
    /// conferir que o pacote, a licença do QuestPDF e as fontes funcionam no ambiente.
    /// </summary>
    /// <returns>Os bytes do PDF.</returns>
    /// <exception cref="InvalidOperationException">A licença do QuestPDF não foi definida; a mensagem aponta o README do pacote.</exception>
    public static byte[] GerarPaginaTeste()
    {
        LicencaQuestPdf.Exigir();

        var codigoBarras = CodigoBarras128.Gerar(ChaveExemplo);
        var qrCode = QrCodeSvg.Gerar(UrlExemplo);
        var imagem = ImagemBitSvg.Gerar(ImagemExemplo());

        return Document.Create(documento => documento.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(2, Unit.Centimetre);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(12));
            pagina.Content().Column(coluna =>
            {
                coluna.Spacing(16);
                coluna.Item().Text("OpenDFe: página de teste").FontSize(20).Bold();
                coluna.Item().Text("Emissão, autorização, ação, coração, ônibus, açúcar, ÁÉÍÓÚ, àèìòù, âêôã, õüç.");
                coluna.Item().Text("Código de barras (Code 128, conjunto C):");
                coluna.Item().Width(8, Unit.Centimetre).Height(2, Unit.Centimetre).Svg(codigoBarras);
                coluna.Item().Text("QR Code (nível M, UTF-8):");
                coluna.Item().Width(4, Unit.Centimetre).Height(4, Unit.Centimetre).Svg(qrCode);
                coluna.Item().Text("Imagem de 1 bit:");
                coluna.Item().Width(2, Unit.Centimetre).Height(2, Unit.Centimetre).Svg(imagem);
            });
        })).GeneratePdf();
    }

    // Uma moldura com uma diagonal, 20 x 20 pixels (20 não é múltiplo de 8: exercita o fim de linha empacotado).
    private static ImagemBit ImagemExemplo()
    {
        const int lado = 20;
        var pixels = new bool[lado, lado];
        for (var y = 0; y < lado; y++)
            for (var x = 0; x < lado; x++)
                pixels[y, x] = x == 0 || y == 0 || x == lado - 1 || y == lado - 1 || x == y;
        return ImagemBit.Criar(pixels);
    }
}
