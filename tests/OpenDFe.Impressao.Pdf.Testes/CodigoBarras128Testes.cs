using System.Globalization;

namespace OpenDFe.Impressao.Pdf.Testes;

public class CodigoBarras128Testes
{
    private const string Chave = "31261011222333000181650010000001231123456780";

    [Fact]
    public void O1234UsaOsSimbolosEOVerificadorDaTabelaPadrao()
    {
        // (105 + 12×1 + 34×2) mod 103 = 185 mod 103 = 82
        Assert.Equal([105, 12, 34, 82, 106], CodigoBarras128.Simbolos("1234"));
    }

    [Fact]
    public void O1234TemAsLarguraDosPadroesDaTabelaPadrao()
    {
        // início C 211232, 12 = 112232, 34 = 131123, 82 = 121241, fim 2331112
        Assert.Equal("211232" + "112232" + "131123" + "121241" + "2331112", string.Concat(CodigoBarras128.Larguras("1234")));
    }

    [Fact]
    public void OsPadroesDaTabelaSomam11ModulosEOFimSoma13()
    {
        for (var v = 0; v <= 105; v++)
        {
            var padrao = CodigoBarras128.Padrao(v);
            Assert.Equal(6, padrao.Count);
            Assert.Equal(11, padrao.Sum());
            Assert.All(padrao, w => Assert.InRange(w, 1, 4));
        }
        var fim = CodigoBarras128.Padrao(CodigoBarras128.Fim);
        Assert.Equal(7, fim.Count);
        Assert.Equal(13, fim.Sum());
    }

    [Fact]
    public void OsPadroesSaoTodosDiferentes()
    {
        var textos = Enumerable.Range(0, 107).Select(v => string.Concat(CodigoBarras128.Padrao(v))).ToList();
        Assert.Equal(107, textos.Distinct().Count());
    }

    [Fact]
    public void OsPadroesDeInicioEFimSaoOsDaNorma()
    {
        Assert.Equal("211232", string.Concat(CodigoBarras128.Padrao(CodigoBarras128.InicioC)));
        Assert.Equal("2331112", string.Concat(CodigoBarras128.Padrao(CodigoBarras128.Fim)));
        Assert.Equal("212222", string.Concat(CodigoBarras128.Padrao(0)));
        Assert.Equal("211412", string.Concat(CodigoBarras128.Padrao(103)));
    }

    [Fact]
    public void AsCemCombinacoesDePares00A99TemOSimboloIgualAoValor()
    {
        for (var n = 0; n < 100; n++)
        {
            var par = n.ToString("00", CultureInfo.InvariantCulture);
            var simbolos = CodigoBarras128.Simbolos(par);
            Assert.Equal([105, n, (105 + n) % 103, 106], simbolos);
            Assert.Equal(par, Decodificar(CodigoBarras128.Larguras(par)));
        }
    }

    [Fact]
    public void AChaveDe44DigitosTem25SimbolosE277Modulos()
    {
        var simbolos = CodigoBarras128.Simbolos(Chave);
        Assert.Equal(25, simbolos.Length);                 // início + 22 pares + verificador + fim
        var larguras = CodigoBarras128.Larguras(Chave);
        Assert.Equal(24 * 11 + 13, larguras.Sum());        // 24 símbolos de 11 módulos + o fim de 13 = 277
        Assert.Equal(24 * 6 + 7, larguras.Length);
        Assert.Equal(Chave, Decodificar(larguras));
    }

    [Fact]
    public void ChavesAleatoriasVoltamIguaisPeloDecodificador()
    {
        var sorteio = new Random(20261024);
        for (var i = 0; i < 500; i++)
        {
            var tamanho = 2 * sorteio.Next(1, 30);
            var digitos = string.Concat(Enumerable.Range(0, tamanho).Select(_ => (char)('0' + sorteio.Next(10))));
            Assert.Equal(digitos, Decodificar(CodigoBarras128.Larguras(digitos)));
        }
    }

    [Theory]
    [InlineData("12A4")]
    [InlineData("12 4")]
    [InlineData("12-4")]
    [InlineData("١٢")]       // dígitos árabes não são 0 a 9
    public void RecusaCaractereQueNaoESejaDigito(string texto) =>
        Assert.Throws<ArgumentException>(() => CodigoBarras128.Simbolos(texto));

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("3126101122233300018165001000000123112345678")]
    public void RecusaQuantidadeImparDeDigitos(string texto) =>
        Assert.Throws<ArgumentException>(() => CodigoBarras128.Simbolos(texto));

    [Fact]
    public void RecusaTextoVazio() =>
        Assert.ThrowsAny<ArgumentException>(() => CodigoBarras128.Simbolos(""));

    [Fact]
    public void OSvgTemUmaBarraPorElementoImparNasPosicoesDosModulos()
    {
        var svg = CodigoBarras128.Gerar(Chave, 40);
        var (largura, altura) = SvgLeitor.Tamanho(svg);
        Assert.Equal(277, largura);
        Assert.Equal(40, altura);

        var larguras = CodigoBarras128.Larguras(Chave);
        var esperado = new List<(int, int, int, int)>();
        var x = 0;
        for (var i = 0; i < larguras.Length; i++)
        {
            if (i % 2 == 0) esperado.Add((x, 0, larguras[i], 40));
            x += larguras[i];
        }
        Assert.Equal(esperado, SvgLeitor.Retangulos(svg));
        // sem zona de silêncio: a primeira barra encosta na borda esquerda e a última na direita
        Assert.Equal(0, esperado[0].Item1);
        Assert.Equal(largura, esperado[^1].Item1 + esperado[^1].Item3);
    }

    // Decodificador de teste: lê as larguras de volta, confere início, verificador e fim, e recompõe os dígitos.
    private static string Decodificar(int[] larguras)
    {
        var tabela = Enumerable.Range(0, 106).ToDictionary(v => string.Concat(CodigoBarras128.Padrao(v)), v => v);
        var corpo = larguras.Length - 7;
        Assert.Equal(0, corpo % 6);
        Assert.Equal("2331112", string.Concat(larguras.Skip(corpo)));

        var valores = new List<int>();
        for (var i = 0; i < corpo; i += 6)
            valores.Add(tabela[string.Concat(larguras.Skip(i).Take(6))]);

        Assert.Equal(105, valores[0]);
        var pares = valores.Skip(1).Take(valores.Count - 2).ToList();
        var soma = 105 + pares.Select((v, i) => v * (i + 1)).Sum();
        Assert.Equal(soma % 103, valores[^1]);
        return string.Concat(pares.Select(v => v.ToString("00", CultureInfo.InvariantCulture)));
    }
}
