using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenDFe.Impressao.Pdf.Testes;

/// <summary>Lê de volta os SVGs gerados pela lib (um <c>path</c> de retângulos <c>M x y h n v a h-n z</c>).</summary>
internal static partial class SvgLeitor
{
    [GeneratedRegex(@"M(\d+) (\d+)h(\d+)v(\d+)h-\d+z")]
    private static partial Regex Retangulo();

    [GeneratedRegex(@"<svg [^>]*width=""(\d+)"" height=""(\d+)"" viewBox=""0 0 (\d+) (\d+)""")]
    private static partial Regex Cabecalho();

    private static int Inteiro(string texto) => int.Parse(texto, CultureInfo.InvariantCulture);

    /// <summary>Largura e altura do SVG, conferindo que o <c>viewBox</c> é uma unidade por pixel.</summary>
    public static (int Largura, int Altura) Tamanho(string svg)
    {
        var m = Cabecalho().Match(svg);
        Assert.True(m.Success, "cabeçalho do SVG não reconhecido");
        Assert.Equal(m.Groups[1].Value, m.Groups[3].Value);
        Assert.Equal(m.Groups[2].Value, m.Groups[4].Value);
        return (Inteiro(m.Groups[1].Value), Inteiro(m.Groups[2].Value));
    }

    /// <summary>Os retângulos desenhados: (x, y, largura, altura), nesta ordem.</summary>
    public static List<(int X, int Y, int Largura, int Altura)> Retangulos(string svg) =>
        [.. Retangulo().Matches(svg).Select(m => (Inteiro(m.Groups[1].Value), Inteiro(m.Groups[2].Value),
            Inteiro(m.Groups[3].Value), Inteiro(m.Groups[4].Value)))];

    /// <summary>Pinta os retângulos numa matriz [linha, coluna] do tamanho do SVG.</summary>
    public static bool[,] Pixels(string svg)
    {
        var (largura, altura) = Tamanho(svg);
        var pixels = new bool[altura, largura];
        foreach (var (x, y, l, a) in Retangulos(svg))
            for (var j = y; j < y + a; j++)
                for (var i = x; i < x + l; i++)
                    pixels[j, i] = true;
        return pixels;
    }
}
