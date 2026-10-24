using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

// Os gráficos: o QR Code, o código de barras e a imagem de 1 bit, todos vetoriais (SVG), com as medidas dos manuais
// (docs/CONTRATOS.md 22.2).
public static partial class RenderizadorPdf
{
    // O alinhamento do bloco gráfico dentro da largura da linha.
    private static IContainer Posicionar(IContainer container, AlinhamentoImpressao alinhamento) => alinhamento switch
    {
        AlinhamentoImpressao.Centro => container.AlignCenter(),
        AlinhamentoImpressao.Direita => container.AlignRight(),
        _ => container.AlignLeft(),
    };

    /// <summary>
    /// A margem de silêncio do QR Code de lado total <paramref name="ladoMm"/> com <paramref name="modulos"/> módulos por
    /// lado: a do manual (3 mm até 25 mm, 10% do lado acima), ou os 4 módulos do ISO/IEC 18004 se forem maiores.
    /// </summary>
    internal static double MargemQrCode(double ladoMm, int modulos)
    {
        var manual = ladoMm <= Medidas.QrCodeLadoMinimoMm ? Medidas.QrCodeSilencioMm : Medidas.QrCodeSilencioProporcao * ladoMm;
        var iso = Medidas.QrCodeSilencioModulos * ladoMm / (modulos + 2 * Medidas.QrCodeSilencioModulos);
        return Math.Max(manual, iso);
    }

    // O lado do QR Code é o total, com a margem de silêncio dentro (Manual do DANFE NFC-e 3.4): o símbolo ocupa o lado menos
    // as duas margens, e nunca fica menor que o necessário para o lado não caber (o lado do modelo nunca encolhe).
    private static void QrCode(IContainer item, QrCodeImpressao qr)
    {
        var matriz = QrCodeSvg.Matriz(qr.Texto);
        var lado = (float)qr.LadoMm;
        var margem = (float)MargemQrCode(qr.LadoMm, matriz.GetLength(0));
        var svg = ImagemBitSvg.Gerar(ImagemBit.Criar(matriz));
        Posicionar(item, qr.Alinhamento).Width(lado, Unit.Millimetre).Height(lado, Unit.Millimetre)
            .Padding(margem, Unit.Millimetre).Svg(svg);
    }

    // O código de barras: as barras com a largura do contêiner (até os 11,5 cm do Anexo II), a altura pedida e os 10 módulos
    // de margem clara de cada lado (Anexo II 2). O SVG sai na proporção exata da caixa: o QuestPDF mantém a proporção do SVG.
    private static void CodigoBarras(IContainer item, CodigoBarrasImpressao barras, Contexto contexto)
    {
        var modulos = CodigoBarras128.Larguras(barras.Digitos).Sum();
        var disponivel = contexto.LarguraUtilMm;
        var larguraBarras = Math.Min(Medidas.CodigoBarrasLarguraMaximaMm, disponivel * modulos / (modulos + 2 * Medidas.CodigoBarrasMargemModulos));
        var modulo = larguraBarras / modulos;
        var svg = CodigoBarras128.Gerar(barras.Digitos, barras.AlturaMm / modulo);
        // A margem clara é a folga em volta: o centro da linha, com as barras na largura calculada.
        item.AlignCenter().PaddingHorizontal((float)(Medidas.CodigoBarrasMargemModulos * modulo), Unit.Millimetre)
            .Width((float)larguraBarras, Unit.Millimetre).Height((float)barras.AlturaMm, Unit.Millimetre).Svg(svg);
    }

    // A imagem de 1 bit em vetor: a largura pedida, ou o tamanho natural a 203 dpi (a térmica); nunca maior que a linha.
    private static void Imagem(IContainer item, ImagemImpressao imagem, Contexto contexto)
    {
        var bits = imagem.Imagem;
        var natural = bits.Largura * 25.4 / Medidas.ImagemDpi;
        var largura = Math.Min(imagem.LarguraMm ?? natural, contexto.LarguraUtilMm);
        var altura = largura * bits.Altura / bits.Largura;
        var svg = ImagemBitSvg.Gerar(new ImagemBit(bits.Largura, bits.Altura, bits.Bits.ToArray()));
        Posicionar(item, imagem.Alinhamento).Width((float)largura, Unit.Millimetre).Height((float)altura, Unit.Millimetre).Svg(svg);
    }
}
