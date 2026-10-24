using QRCoder;
using UglyToad.PdfPig.Graphics;

namespace OpenDFe.Impressao.Pdf.Testes;

/// <summary>
/// Lê de volta o QR Code e o código de barras que o PDF desenhou como vetor e os compara com a referência: a matriz do QRCoder
/// (nível M, UTF-8, sem a borda que ele acrescenta) e as larguras do Code 128 conjunto C. Compartilhado com o
/// <c>NFe.Testes</c> (os documentos completos e a ponta a ponta com o emulador).
/// </summary>
internal static class PdfVetores
{
    /// <summary>A matriz de módulos do texto pelo QRCoder (nível M, UTF-8), <c>[linha, coluna]</c>, sem a zona de silêncio dele.</summary>
    public static bool[,] MatrizQrCode(string texto)
    {
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

    /// <summary>
    /// A matriz de módulos que o PDF desenha: o tamanho do módulo sai do caminho inteiro (os três localizadores tocam os cantos) e
    /// cada retângulo é uma corrida de módulos pretos. Sem <paramref name="caminho"/>, o de mais subcaminhos da página.
    /// </summary>
    public static (bool[,] Matriz, RetanguloPdf Caixa, double Modulo) MatrizQrCodePdf(PdfLeitor pdf, int pagina, int lado, PdfPath? caminho = null)
    {
        var retangulos = pdf.Retangulos(pagina, caminho ?? pdf.MaiorCaminho(pagina));
        var esquerda = retangulos.Min(r => r.Esquerda);
        var topo = retangulos.Min(r => r.Topo);
        var direita = retangulos.Max(r => r.Esquerda + r.Largura);
        var baixo = retangulos.Max(r => r.Topo + r.Altura);
        var modulo = (direita - esquerda) / lado;
        var matriz = new bool[lado, lado];
        foreach (var r in retangulos)
        {
            var linha = (int)Math.Round((r.Topo - topo) / modulo);
            var coluna = (int)Math.Round((r.Esquerda - esquerda) / modulo);
            var corrida = (int)Math.Round(r.Largura / modulo);
            for (var i = 0; i < corrida; i++)
                matriz[linha, coluna + i] = true;
        }

        return (matriz, new RetanguloPdf(esquerda, topo, direita - esquerda, baixo - topo), modulo);
    }

    /// <summary>O caminho da página que tem as barras do código de barras de <paramref name="digitos"/> (a quantidade de barras e a mesma altura).</summary>
    public static PdfPath CaminhoBarras(PdfLeitor pdf, int pagina, string digitos)
    {
        var barras = (CodigoBarras128.Larguras(digitos).Length + 1) / 2;
        var candidatos = pdf.Pagina(pagina).Paths.Where(c => c.Count == barras).ToList();
        Assert.True(candidatos.Count > 0, $"a página {pagina} não tem um caminho de {barras} barras (o código de barras de {digitos.Length} dígitos)");
        return candidatos.First(c => pdf.Retangulos(pagina, c).Select(r => Math.Round(r.Altura, 1)).Distinct().Count() == 1);
    }

    /// <summary>
    /// Confere que o caminho desenha o Code 128 conjunto C de <paramref name="digitos"/>: cada barra na posição e na largura (em
    /// módulos inteiros) do padrão, e o conjunto com <c>módulos</c> de largura. Devolve o módulo, em pontos, e a caixa das barras.
    /// </summary>
    public static (double Modulo, RetanguloPdf Caixa) ConferirBarras(PdfLeitor pdf, int pagina, PdfPath caminho, string digitos)
    {
        var larguras = CodigoBarras128.Larguras(digitos);
        var modulos = larguras.Sum();
        var barras = pdf.Retangulos(pagina, caminho).OrderBy(r => r.Esquerda).ToList();
        var esquerda = barras[0].Esquerda;
        var direita = barras[^1].Esquerda + barras[^1].Largura;
        var modulo = (direita - esquerda) / modulos;
        Assert.Equal((larguras.Length + 1) / 2, barras.Count);
        var posicao = 0;
        for (var i = 0; i < larguras.Length; i++)
        {
            if (i % 2 == 0)
            {
                var barra = barras[i / 2];
                Assert.True(Math.Abs(barra.Esquerda - (esquerda + posicao * modulo)) < 0.05 * modulo, $"a barra {i / 2} fora da posição");
                Assert.True(Math.Abs(barra.Largura - larguras[i] * modulo) < 0.05 * modulo, $"a barra {i / 2} fora da largura de {larguras[i]} módulos");
            }

            posicao += larguras[i];
        }

        var topo = barras.Min(r => r.Topo);
        var baixo = barras.Max(r => r.Topo + r.Altura);
        return (modulo, new RetanguloPdf(esquerda, topo, direita - esquerda, baixo - topo));
    }
}
