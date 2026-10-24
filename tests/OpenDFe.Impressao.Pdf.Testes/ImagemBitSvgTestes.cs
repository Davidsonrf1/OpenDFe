namespace OpenDFe.Impressao.Pdf.Testes;

public class ImagemBitSvgTestes
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(7, 3)]
    [InlineData(8, 8)]
    [InlineData(9, 5)]
    [InlineData(20, 20)]
    [InlineData(33, 2)]
    [InlineData(1, 40)]
    [InlineData(100, 1)]
    public void ImagemAleatoriaVoltaIgualPeloSvg(int largura, int altura)
    {
        var sorteio = new Random(largura * 1000 + altura);
        var pixels = new bool[altura, largura];
        for (var y = 0; y < altura; y++)
            for (var x = 0; x < largura; x++)
                pixels[y, x] = sorteio.Next(2) == 1;

        var imagem = ImagemBit.Criar(pixels);
        Assert.Equal(ImagemBit.BytesLinha(largura) * altura, imagem.Bits.Length);
        for (var y = 0; y < altura; y++)
            for (var x = 0; x < largura; x++)
                Assert.Equal(pixels[y, x], imagem.Preto(x, y));

        var lido = SvgLeitor.Pixels(ImagemBitSvg.Gerar(imagem));
        Assert.Equal((altura, largura), (lido.GetLength(0), lido.GetLength(1)));
        Assert.Equal(pixels.Cast<bool>(), lido.Cast<bool>());
    }

    [Fact]
    public void ImagemTodaBrancaNaoDesenhaNada()
    {
        var svg = ImagemBitSvg.Gerar(new ImagemBit(13, 4, new byte[2 * 4]));
        Assert.DoesNotContain("<path", svg);
        Assert.Equal((13, 4), SvgLeitor.Tamanho(svg));
        Assert.DoesNotContain(true, SvgLeitor.Pixels(svg).Cast<bool>());
    }

    [Fact]
    public void ImagemTodaPretaDesenhaUmaCorridaPorLinha()
    {
        var pixels = new bool[3, 13];
        for (var y = 0; y < 3; y++)
            for (var x = 0; x < 13; x++)
                pixels[y, x] = true;
        var svg = ImagemBitSvg.Gerar(ImagemBit.Criar(pixels));
        Assert.Equal([(0, 0, 13, 1), (0, 1, 13, 1), (0, 2, 13, 1)], SvgLeitor.Retangulos(svg));
    }

    [Fact]
    public void ImagemDeUmPixelPreto()
    {
        var svg = ImagemBitSvg.Gerar(new ImagemBit(1, 1, [0x80]));
        Assert.Equal((1, 1), SvgLeitor.Tamanho(svg));
        Assert.Equal([(0, 0, 1, 1)], SvgLeitor.Retangulos(svg));
    }

    [Fact]
    public void OsBitsQueSobramNoFimDaLinhaSaoIgnorados()
    {
        // 3 pixels de largura: 0b101 nos três primeiros bits; os cinco restantes (1) não existem na imagem
        var imagem = new ImagemBit(3, 1, [0b1011_1111]);
        var pixels = SvgLeitor.Pixels(ImagemBitSvg.Gerar(imagem));
        Assert.Equal([true, false, true], pixels.Cast<bool>());
    }

    [Fact]
    public void OsBitsVemComOMaisSignificativoPrimeiro()
    {
        var imagem = new ImagemBit(8, 1, [0b1000_0001]);
        Assert.True(imagem.Preto(0, 0));
        Assert.True(imagem.Preto(7, 0));
        Assert.False(imagem.Preto(1, 0));
    }

    [Fact]
    public void RecusaDimensaoInvalidaETamanhoDeBytesErrado()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ImagemBit(0, 1, [0]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ImagemBit(1, 0, [0]));
        Assert.Throws<ArgumentException>(() => new ImagemBit(9, 2, new byte[3]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ImagemBit(2, 2, new byte[2]).Preto(2, 0));
    }
}
