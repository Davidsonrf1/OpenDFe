using System.Globalization;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Graphics;

namespace OpenDFe.Impressao.Pdf.Testes;

/// <summary>
/// Lê de volta o PDF gerado pelo <see cref="RenderizadorPdf"/> (PdfPig) e dá o que os testes conferem: o texto, as letras com
/// fonte e posição, os caminhos vetoriais (o QR Code, o código de barras, a imagem) e as medidas em milímetros. A origem do
/// PdfPig é o canto inferior esquerdo; aqui o <c>Y</c> é medido de cima para baixo, como o papel.
/// </summary>
internal sealed class PdfLeitor : IDisposable
{
    private readonly PdfDocument _documento;

    public PdfLeitor(byte[] pdf) => _documento = PdfDocument.Open(pdf);

    public int Paginas => _documento.NumberOfPages;

    public DocumentInformation Informacao => _documento.Information;

    public Page Pagina(int numero) => _documento.GetPage(numero);

    public static double Milimetros(double pontos) => pontos * 25.4 / 72;

    /// <summary>O texto extraído da página, sem espaços nem quebras (a extração do PdfPig varia nos espaços).</summary>
    public string TextoCompacto(int numero) => Compactar(Pagina(numero).Text);

    /// <summary>O texto de todas as páginas, sem espaços nem quebras.</summary>
    public string TextoCompacto() => string.Concat(Enumerable.Range(1, Paginas).Select(TextoCompacto));

    public static string Compactar(string texto) => string.Concat(texto.Where(c => !char.IsWhiteSpace(c)));

    /// <summary>As letras da página, com a posição em pontos medida de cima para baixo.</summary>
    public IReadOnlyList<LetraPdf> Letras(int numero)
    {
        var pagina = Pagina(numero);
        return [.. pagina.Letters.Select(l => new LetraPdf(l.Value, l.StartBaseLine.X, pagina.Height - l.StartBaseLine.Y, l.EndBaseLine.X,
            l.PointSize, l.FontDetails?.Name ?? string.Empty))];
    }

    /// <summary>A primeira letra da primeira ocorrência do texto na página (letras em sequência no fluxo do PDF).</summary>
    public LetraPdf Achar(int numero, string texto)
    {
        var letras = Letras(numero);
        var corrida = string.Concat(letras.Select(l => l.Valor));
        var indice = corrida.IndexOf(texto, StringComparison.Ordinal);
        Assert.True(indice >= 0, $"O texto \"{texto}\" não está na página {numero}: {corrida}");
        return letras[indice];
    }

    /// <summary>O caminho vetorial de maior quantidade de subcaminhos (o QR Code, o código de barras ou a imagem do documento).</summary>
    public PdfPath MaiorCaminho(int numero) => Pagina(numero).Paths.OrderByDescending(c => c.Count).First();

    /// <summary>Os retângulos (esquerda, topo, largura, altura em pontos, de cima para baixo) dos subcaminhos do caminho.</summary>
    public IReadOnlyList<RetanguloPdf> Retangulos(int numero, PdfPath caminho)
    {
        var altura = Pagina(numero).Height;
        return [.. caminho.Select(sub =>
        {
            var r = sub.GetBoundingRectangle() ?? throw new InvalidOperationException("Subcaminho sem retângulo.");
            return new RetanguloPdf(r.Left, altura - r.Top, r.Width, r.Height);
        })];
    }

    public void Dispose() => _documento.Dispose();

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"PDF com {Paginas} páginas");
}

/// <summary>Uma letra do PDF: o valor, a posição da linha de base (pontos, Y de cima para baixo), o tamanho e a fonte.</summary>
internal sealed record LetraPdf(string Valor, double X, double Y, double FimX, double Tamanho, string Fonte)
{
    public bool Negrito => Fonte.Contains("Bold", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Um retângulo do PDF em pontos, com o topo medido de cima para baixo.</summary>
internal sealed record RetanguloPdf(double Esquerda, double Topo, double Largura, double Altura);
