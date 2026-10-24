using OpenDFe.Impressao;

namespace OpenDFe.Testes;

/// <summary>
/// A quebra do texto em colunas, as larguras da linha de células e a imagem de 1 bit (etapa 7.2): os casos de borda e a
/// propriedade das larguras (soma exata, ninguém perde).
/// </summary>
public sealed class TextoEImagemImpressaoTestes
{
    private static IReadOnlyList<string> Quebrar(string texto, int colunas) => new TextoImpressao(texto).Quebrar(colunas);

    [Fact]
    public void OTextoQueCabeNaoQuebra()
    {
        Assert.Equal(["ABC DEF"], Quebrar("ABC DEF", 7));
        Assert.Equal(["ABC DEF"], Quebrar("ABC DEF", 80));
    }

    [Fact]
    public void QuebraNoEspacoSemCortarAPalavra()
    {
        Assert.Equal(["ABC", "DEF"], Quebrar("ABC DEF", 6));
        Assert.Equal(["ABC", "DEF GH"], Quebrar("ABC DEF GH", 6));
        Assert.Equal(["AAAA", "BBBB", "CC"], Quebrar("AAAA BBBB CC", 5));
        // A palavra que termina exatamente na coluna cabe.
        Assert.Equal(["ABCDE", "FG"], Quebrar("ABCDE FG", 5));
    }

    [Fact]
    public void APalavraMaiorQueALinhaECortadaNaLargura()
    {
        Assert.Equal(["ABCDE", "FGHIJ", "K"], Quebrar("ABCDEFGHIJK", 5));
        Assert.Equal(["AB", "CDEFG", "HIJKL", "MN"], Quebrar("AB CDEFGHIJKLMN", 5));
        Assert.Equal(["A", "B", "C"], Quebrar("ABC", 1));
    }

    [Fact]
    public void ONovaLinhaEExplicitaELinhasVaziasSaoMantidas()
    {
        Assert.Equal(["A", "B"], Quebrar("A\nB", 10));
        Assert.Equal(["A", "B"], Quebrar("A\r\nB", 10));
        Assert.Equal(["A", string.Empty, "B"], Quebrar("A\n\nB", 10));
        Assert.Equal([string.Empty], Quebrar(string.Empty, 10));
        Assert.Equal([string.Empty, string.Empty], Quebrar("\n", 10));
        Assert.Equal(["A", string.Empty], Quebrar("A\n", 10));
    }

    [Fact]
    public void OsEspacosDasPontasDeCadaLinhaSaoDescartados()
    {
        Assert.Equal(["A"], Quebrar("A   ", 10));
        Assert.Equal(["AB", "CD"], Quebrar("AB    CD", 5));
        Assert.Equal(["A", "B"], Quebrar("A \nB", 10));
    }

    [Fact]
    public void AsLetrasAcentuadasContamUmaColuna()
    {
        Assert.Equal(["AÇÃO", "ÓTIMA"], Quebrar("AÇÃO ÓTIMA", 5));
        Assert.Equal(["É É É"], Quebrar("É É É", 5));
        Assert.Equal(["ÁÉÍÓÚ", "ÃÕÂÊÔ"], Quebrar("ÁÉÍÓÚÃÕÂÊÔ", 5));
    }

    [Fact]
    public void QuebrarRecusaColunasMenoresQueUm()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Quebrar("A", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CelulaImpressao("A").Quebrar(-1));
        Assert.Equal(["AB", "C"], new CelulaImpressao("ABC").Quebrar(2));
    }

    [Fact]
    public void NenhumaLinhaQuebradaPassaDaLargura()
    {
        var aleatorio = new Random(20261024);
        for (var i = 0; i < 300; i++)
        {
            var palavras = Enumerable.Range(0, aleatorio.Next(1, 25)).Select(_ => new string('x', aleatorio.Next(1, 14)));
            var texto = string.Join(' ', palavras);
            var colunas = aleatorio.Next(1, 40);

            var linhas = Quebrar(texto, colunas);

            Assert.All(linhas, l => Assert.True(l.Length <= colunas));
            // Nada se perde: sem os espaços, as letras são as mesmas e na mesma ordem.
            Assert.Equal(texto.Replace(" ", string.Empty, StringComparison.Ordinal), string.Concat(linhas).Replace(" ", string.Empty, StringComparison.Ordinal));
        }
    }

    [Fact]
    public void AsLargurasSomamExatamenteAsColunas()
    {
        // O exemplo do DANFCE: código, descrição, quantidade e unidade, unitário, total.
        var linha = new LinhaImpressao([new("a", 7), new("b", 17), new("c", 8), new("d", 8), new("e", 9)]);
        Assert.Equal([7, 16, 8, 8, 9], linha.Larguras(48));
        Assert.Equal([1], new LinhaImpressao([new("a")]).Larguras(1));
        Assert.Equal([48], new LinhaImpressao([new("a", 3)]).Larguras(48));
        Assert.Equal([24, 24], new LinhaImpressao([new("a"), new("b")]).Larguras(48));
        // O maior resto: 3 partes iguais em 10 colunas, a sobra vai à primeira.
        Assert.Equal([4, 3, 3], new LinhaImpressao([new("a"), new("b"), new("c")]).Larguras(10));
    }

    [Fact]
    public void AsLargurasSomamAsColunasEOsPesosAleatoriosNaoPerdemCelula()
    {
        var aleatorio = new Random(20261024);
        for (var rodada = 0; rodada < 400; rodada++)
        {
            var celulas = Enumerable.Range(0, aleatorio.Next(1, 9))
                .Select(_ => new CelulaImpressao("x", Math.Pow(10, aleatorio.NextDouble() * 4 - 2))).ToArray();
            var linha = new LinhaImpressao(celulas);
            for (var colunas = 1; colunas <= 100; colunas++)
            {
                var larguras = linha.Larguras(colunas);

                Assert.Equal(celulas.Length, larguras.Count);
                Assert.Equal(colunas, larguras.Sum());
                Assert.All(larguras, l => Assert.True(l >= 0));
                if (colunas >= celulas.Length)
                    Assert.All(larguras, l => Assert.True(l >= 1, $"colunas {colunas}: {string.Join(',', larguras)}"));
            }
        }
    }

    [Fact]
    public void AsLargurasRecusamColunasMenoresQueUm() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new LinhaImpressao([new CelulaImpressao("a")]).Larguras(0));

    [Fact]
    public void ImagemDeUmaColunaEDeNoveColunasEmpacotaOsBits()
    {
        // 9 pixels de largura: 2 bytes por linha; os 7 bits do fim do segundo byte não contam.
        var imagem = new Imagem1Bit(9, 2, new byte[] { 0b1000_0000, 0b1000_0000, 0b0000_0000, 0b0111_1111 });

        Assert.True(imagem.Preto(0, 0));
        Assert.True(imagem.Preto(8, 0));
        Assert.False(imagem.Preto(1, 0));
        Assert.False(imagem.Preto(0, 1));
        Assert.False(imagem.Preto(8, 1));
        Assert.Equal((9, 2), (imagem.Largura, imagem.Altura));

        var uma = new Imagem1Bit(1, 3, new byte[] { 0x80, 0x00, 0x80 });
        Assert.Equal([true, false, true], Enumerable.Range(0, 3).Select(y => uma.Preto(0, y)));
    }

    [Fact]
    public void AImagemRecusaTamanhoDeBitsErradoEDimensoesInvalidas()
    {
        Assert.Throws<ArgumentException>(() => new Imagem1Bit(9, 2, new byte[3]));
        Assert.Throws<ArgumentException>(() => new Imagem1Bit(9, 2, new byte[5]));
        Assert.Throws<ArgumentException>(() => new Imagem1Bit(8, 1, new byte[2]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Imagem1Bit(0, 1, new byte[1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Imagem1Bit(1, 0, new byte[1]));
        _ = new Imagem1Bit(8, 1, new byte[1]);
    }

    [Fact]
    public void APretoRecusaPixelForaDaImagem()
    {
        var imagem = new Imagem1Bit(3, 2, new byte[2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => imagem.Preto(3, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => imagem.Preto(0, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => imagem.Preto(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => imagem.Preto(0, -1));
    }

    [Fact]
    public void OsBitsPassadosSaoCopiados()
    {
        var bits = new byte[] { 0x80 };
        var imagem = new Imagem1Bit(1, 1, bits);

        bits[0] = 0x00;

        Assert.True(imagem.Preto(0, 0));
        Assert.Equal(new byte[] { 0x80 }, imagem.Bits.ToArray());
    }

    [Fact]
    public void LimiarizarPassaOEscuroParaPretoNoLimiar()
    {
        // 3x2: 0 e 127 são pretos; 128 e 255, brancos (o limiar é exclusivo).
        var cinza = new byte[] { 0, 127, 128, 255, 10, 200 };

        var imagem = Imagem1Bit.Limiarizar(cinza, 3, 2);

        Assert.Equal([true, true, false], Enumerable.Range(0, 3).Select(x => imagem.Preto(x, 0)));
        Assert.Equal([false, true, false], Enumerable.Range(0, 3).Select(x => imagem.Preto(x, 1)));
        Assert.Equal(new byte[] { 0b1100_0000, 0b0100_0000 }, imagem.Bits.ToArray());
        // Com o limiar 1, só o zero é preto.
        var estreito = Imagem1Bit.Limiarizar(cinza, 3, 2, limiar: 1);
        Assert.Equal(new byte[] { 0b1000_0000, 0b0000_0000 }, estreito.Bits.ToArray());
    }

    [Fact]
    public void LimiarizarDeLarguraNoveGuardaOsNovePixels()
    {
        var cinza = Enumerable.Repeat((byte)0, 9).Concat(Enumerable.Repeat((byte)255, 9)).ToArray();

        var imagem = Imagem1Bit.Limiarizar(cinza, 9, 2);

        Assert.Equal(new byte[] { 0xFF, 0x80, 0x00, 0x00 }, imagem.Bits.ToArray());
        Assert.True(imagem.Preto(8, 0));
        Assert.False(imagem.Preto(8, 1));
    }

    [Fact]
    public void LimiarizarRecusaTamanhoErrado()
    {
        Assert.Throws<ArgumentException>(() => Imagem1Bit.Limiarizar(new byte[5], 3, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => Imagem1Bit.Limiarizar([], 0, 1));
    }
}
