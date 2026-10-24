using System.Text;
using UglyToad.PdfPig;

namespace OpenDFe.Impressao.Pdf.Testes;

public class RenderizadorPdfTestes
{
    [Fact]
    public void GeraUmPdfDeUmaPaginaA4()
    {
        var pdf = RenderizadorPdf.GerarPaginaTeste();

        Assert.Equal("%PDF-", Encoding.ASCII.GetString(pdf, 0, 5));
        using var documento = PdfDocument.Open(pdf);
        Assert.Equal(1, documento.NumberOfPages);
        var pagina = documento.GetPage(1);
        Assert.Equal(595.3, pagina.Width, 0.5);     // A4 em pontos: 210 x 297 mm
        Assert.Equal(841.9, pagina.Height, 0.5);
    }

    [Fact]
    public void OTextoAcentuadoSaiNoTextoExtraido()
    {
        using var documento = PdfDocument.Open(RenderizadorPdf.GerarPaginaTeste());
        var texto = documento.GetPage(1).Text;

        Assert.Contains("OpenDFe", texto);
        Assert.Contains("página de teste", texto);
        Assert.Contains("ação", texto);
        Assert.Contains("coração", texto);
        Assert.Contains("ÁÉÍÓÚ", texto);
        Assert.Contains("õüç", texto);
    }

    [Fact]
    public void ContemOsTresElementosGraficosComoVetor()
    {
        using var documento = PdfDocument.Open(RenderizadorPdf.GerarPaginaTeste());
        var pagina = documento.GetPage(1);

        // Três caminhos vetoriais, um por elemento. O código de barras tem 76 barras (24 símbolos de 3 barras e o fim
        // com 4), a mesma conta do Code 128 da chave; o QR Code e a imagem de 1 bit têm uma corrida por segmento. E
        // nenhum bitmap.
        Assert.Equal(3, pagina.Paths.Count);
        Assert.Contains(pagina.Paths, caminho => caminho.Count == 76);
        Assert.All(pagina.Paths, caminho => Assert.True(caminho.GetBoundingRectangle() is { Width: > 0, Height: > 0 }));
        Assert.Empty(pagina.GetImages());
    }

    [Fact]
    public async Task OitoGeracoesSimultaneasNaoDaoErroEGeramPaginasIguaisEmTexto()
    {
        var tarefas = Enumerable.Range(0, 8).Select(_ => Task.Run(RenderizadorPdf.GerarPaginaTeste)).ToArray();
        var pdfs = await Task.WhenAll(tarefas);

        var textos = pdfs.Select(pdf =>
        {
            using var documento = PdfDocument.Open(pdf);
            Assert.Equal(1, documento.NumberOfPages);
            return documento.GetPage(1).Text;
        }).ToList();
        Assert.Single(textos.Distinct());
    }
}

/// <summary>
/// O erro de licença não definida pelo caminho inteiro, o <c>GerarPaginaTeste</c>. Mexe no estado global do QuestPDF
/// (<c>Settings.License</c>), então a coleção desliga o paralelismo: roda sozinha, e o <c>finally</c> restaura a
/// licença do assembly de teste.
/// </summary>
[Collection(nameof(LicencaGlobal))]
public class RenderizadorPdfLicencaTestes
{
    [Fact]
    public void SemLicencaDefinidaGerarPaginaTesteLancaComOEnderecoDoReadme()
    {
        var anterior = QuestPDF.Settings.License;
        try
        {
            QuestPDF.Settings.License = null;
            var erro = Assert.Throws<InvalidOperationException>(RenderizadorPdf.GerarPaginaTeste);
            Assert.Contains("https://github.com/Davidsonrf1/OpenDFe/blob/main/src/OpenDFe.Impressao.Pdf/README.md", erro.Message);
            Assert.Contains("LicenseType.Community", erro.Message);
        }
        finally
        {
            QuestPDF.Settings.License = anterior;
        }
    }
}

[CollectionDefinition(nameof(LicencaGlobal), DisableParallelization = true)]
public sealed class LicencaGlobal;
