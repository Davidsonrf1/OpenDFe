using System.Globalization;
using System.Text;

namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// Converte a imagem de 1 bit em SVG vetorial: um único <c>path</c> com uma corrida horizontal de pixels pretos por
/// segmento (<c>M x y h n v1 h-n z</c>), sem fundo e sem borda. Vetor imprime nítido em qualquer resolução; o mesmo
/// desenho serve à matriz do QR Code.
/// </summary>
internal static class ImagemBitSvg
{
    /// <summary>
    /// O SVG da imagem: <c>viewBox</c> de uma unidade por pixel, <c>crispEdges</c> e só o preto desenhado (uma imagem
    /// toda branca não tem <c>path</c>).
    /// </summary>
    public static string Gerar(ImagemBit imagem)
    {
        ArgumentNullException.ThrowIfNull(imagem);
        var c = CultureInfo.InvariantCulture;
        var caminho = new StringBuilder();
        for (var y = 0; y < imagem.Altura; y++)
        {
            var x = 0;
            while (x < imagem.Largura)
            {
                if (!imagem.Preto(x, y)) { x++; continue; }
                var inicio = x;
                while (x < imagem.Largura && imagem.Preto(x, y)) x++;
                var tamanho = x - inicio;
                caminho.Append(c, $"M{inicio} {y}h{tamanho}v1h-{tamanho}z");
            }
        }

        var svg = new StringBuilder();
        svg.Append(c, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{imagem.Largura}\" height=\"{imagem.Altura}\" viewBox=\"0 0 {imagem.Largura} {imagem.Altura}\" shape-rendering=\"crispEdges\">");
        if (caminho.Length > 0) svg.Append(c, $"<path fill=\"#000\" d=\"{caminho}\"/>");
        svg.Append("</svg>");
        return svg.ToString();
    }
}
