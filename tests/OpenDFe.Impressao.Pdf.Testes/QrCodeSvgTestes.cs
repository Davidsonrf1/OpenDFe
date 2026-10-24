using QRCoder;

namespace OpenDFe.Impressao.Pdf.Testes;

public class QrCodeSvgTestes
{
    private const string Chave = "31261011222333000181650010000001231123456780";

    // Duas URLs de NFC-e no formato das versões 2 e 3 do QR Code (online), fictícias; e textos com acento e mínimo.
    private static readonly string[] Textos =
    [
        $"https://www.exemplo.gov.br/nfce/qrcode?p={Chave}|2|1|1|0123456789ABCDEF0123456789ABCDEF01234567",
        $"https://www.exemplo.gov.br/nfce/qrcode?p={Chave}|3|1|ABCDEF0123456789ABCDEF0123456789ABCDEF01",
        "Emissão em contingência: coração, ação, ônibus — ÁÉÍÓÚ",
        "A",
    ];

    // A matriz do QRCoder (UTF-8), sem a zona de silêncio de 4 módulos que ele acrescenta.
    private static bool[,] Referencia(string texto, QRCodeGenerator.ECCLevel nivel)
    {
        using var gerador = new QRCodeGenerator();
        using var dados = gerador.CreateQrCode(texto, nivel, forceUtf8: true);
        var lado = 17 + 4 * dados.Version;
        var margem = (dados.ModuleMatrix.Count - lado) / 2;
        Assert.Equal(4, margem);
        var matriz = new bool[lado, lado];
        for (var y = 0; y < lado; y++)
            for (var x = 0; x < lado; x++)
                matriz[y, x] = dados.ModuleMatrix[y + margem][x + margem];
        return matriz;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AMatrizDoSvgIgualAModuleMatrixDoQrCoderNivelM(int indice)
    {
        var texto = Textos[indice];
        var esperado = Referencia(texto, QRCodeGenerator.ECCLevel.M);
        var lido = SvgLeitor.Pixels(QrCodeSvg.Gerar(texto));
        Assert.Equal(esperado.GetLength(0), lido.GetLength(0));
        Assert.Equal(esperado.Cast<bool>(), lido.Cast<bool>());
    }

    [Fact]
    public void ONivelEMENaoOL()
    {
        var texto = Textos[0];
        var m = Referencia(texto, QRCodeGenerator.ECCLevel.M);
        var l = Referencia(texto, QRCodeGenerator.ECCLevel.L);
        Assert.NotEqual(m.Cast<bool>(), l.Cast<bool>());   // o nível muda os bits de formato e os dados: as matrizes diferem
        Assert.Equal(m.Cast<bool>(), QrCodeSvg.Matriz(texto).Cast<bool>());
        Assert.NotEqual(l.Cast<bool>(), QrCodeSvg.Matriz(texto).Cast<bool>());
    }

    [Fact]
    public void AsUrlsDeNfceTemVersaoDeQrCodeCompativelComONivelM()
    {
        foreach (var texto in Textos.Take(2))
        {
            var lado = QrCodeSvg.Matriz(texto).GetLength(0);
            Assert.Equal(0, (lado - 17) % 4);
            // cerca de 120 caracteres em nível M cabem numa versão por volta de 7 a 9; a faixa larga só protege de um nível trocado
            Assert.InRange((lado - 17) / 4, 5, 12);
        }
    }

    [Fact]
    public void OTextoComAcentoSaiEmUtf8()
    {
        const string texto = "Ação";
        using var gerador = new QRCodeGenerator();
        using var utf8 = gerador.CreateQrCode(texto, QRCodeGenerator.ECCLevel.M, forceUtf8: true);
        var lido = SvgLeitor.Pixels(QrCodeSvg.Gerar(texto));
        var lado = lido.GetLength(0);
        for (var y = 0; y < lado; y++)
            for (var x = 0; x < lado; x++)
                Assert.Equal(utf8.ModuleMatrix[y + 4][x + 4], lido[y, x]);
    }

    [Fact]
    public void OSvgSaiSemBordaComOsLocalizadoresNosCantos()
    {
        var svg = QrCodeSvg.Gerar(Textos[0]);
        var (largura, altura) = SvgLeitor.Tamanho(svg);
        Assert.Equal(largura, altura);
        Assert.Equal(0, (largura - 17) % 4);
        var pixels = SvgLeitor.Pixels(svg);
        // o localizador de posição ocupa três cantos: 7 módulos escuros na borda
        for (var i = 0; i < 7; i++)
        {
            Assert.True(pixels[0, i]);
            Assert.True(pixels[i, 0]);
            Assert.True(pixels[0, largura - 1 - i]);
            Assert.True(pixels[largura - 1 - i, 0]);
        }
        Assert.Contains("crispEdges", svg);
    }

    [Fact]
    public void RecusaTextoVazio() => Assert.ThrowsAny<ArgumentException>(() => QrCodeSvg.Gerar(""));
}
