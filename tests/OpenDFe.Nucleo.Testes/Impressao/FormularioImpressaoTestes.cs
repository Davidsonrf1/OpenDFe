using System.Reflection;
using OpenDFe.Impressao;

namespace OpenDFe.Testes;

/// <summary>
/// O formulário de posição fixa do modelo de impressão (etapa 9.5, docs/NFSE.md 12, decisão G2): os quadros, as validações, a
/// cópia das listas e dos bytes, o conjunto fechado dos quadros e o papel do DANFSe.
/// </summary>
public sealed class FormularioImpressaoTestes
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3];

    [Fact]
    public void OFormularioCopiaAListaETemOsPadroes()
    {
        var lista = new List<QuadroFormulario> { new TextoFormulario(1, 2, 3, 4, "Rótulo", "Valor") };
        var formulario = new FormularioImpressao(lista);
        lista.Clear();

        Assert.Single(formulario.Quadros);
        Assert.Equal(0, formulario.BordaPt);
        Assert.Null(formulario.MarcaAgua);
        Assert.Empty(new FormularioImpressao([]).Quadros);
        var cheio = new FormularioImpressao([]) { BordaPt = 1, MarcaAgua = "CANCELADA" };
        Assert.Equal((1d, "CANCELADA"), (cheio.BordaPt, cheio.MarcaAgua));
    }

    [Fact]
    public void OFormularioRecusaListaNulaQuadroNuloBordaInvalidaEMarcaVazia()
    {
        Assert.Throws<ArgumentNullException>(() => new FormularioImpressao(null!));
        Assert.Throws<ArgumentNullException>(() => new FormularioImpressao([null!]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FormularioImpressao([]) { BordaPt = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new FormularioImpressao([]) { BordaPt = double.NaN });
        Assert.Throws<ArgumentException>(() => new FormularioImpressao([]) { MarcaAgua = " " });
    }

    [Fact]
    public void OTextoTemAPosicaoOTamanhoEOsPadroesDaNt008()
    {
        var texto = new TextoFormulario(0, 0.5, 50.9, 6.3, "Telefone", null);

        Assert.Equal((0d, 0.5d, 50.9d, 6.3d), (texto.XMm, texto.YMm, texto.LarguraMm, texto.AlturaMm));
        Assert.Equal("Telefone", texto.Rotulo);
        Assert.Null(texto.Valor);
        // O rótulo de 6 pt em negrito e o conteúdo de 7 pt normal (NT 008/2026 2.4.2 e 2.4.4), preto, à esquerda, sem fundo e sem borda.
        Assert.Equal((6d, true, 7d, false), (texto.TamanhoRotuloPt, texto.RotuloNegrito, texto.TamanhoValorPt, texto.ValorNegrito));
        Assert.Equal((AlinhamentoImpressao.Esquerda, CorImpressao.Preto, false, BordaQuadro.Nenhuma),
            (texto.Alinhamento, texto.Cor, texto.Sombreado, texto.Bordas));

        var outro = new TextoFormulario(1, 1, 1, 1, null, "x")
        {
            TamanhoRotuloPt = 7,
            RotuloNegrito = false,
            TamanhoValorPt = 9,
            ValorNegrito = true,
            Alinhamento = AlinhamentoImpressao.Centro,
            Cor = CorImpressao.Vermelho,
            Sombreado = true,
            Bordas = BordaQuadro.Cima | BordaQuadro.Esquerda,
        };
        Assert.Equal((7d, false, 9d, true), (outro.TamanhoRotuloPt, outro.RotuloNegrito, outro.TamanhoValorPt, outro.ValorNegrito));
        Assert.Equal((AlinhamentoImpressao.Centro, CorImpressao.Vermelho, true), (outro.Alinhamento, outro.Cor, outro.Sombreado));
        Assert.Equal(BordaQuadro.Cima | BordaQuadro.Esquerda, outro.Bordas);
        Assert.Equal(BordaQuadro.Cima | BordaQuadro.Baixo | BordaQuadro.Esquerda | BordaQuadro.Direita, BordaQuadro.Todas);
    }

    [Theory]
    [InlineData(-0.1, 0, 1, 1)]
    [InlineData(0, -1, 1, 1)]
    [InlineData(0, 0, 0, 1)]
    [InlineData(0, 0, 1, 0)]
    [InlineData(double.NaN, 0, 1, 1)]
    [InlineData(0, double.PositiveInfinity, 1, 1)]
    [InlineData(0, 0, double.NaN, 1)]
    public void OQuadroRecusaPosicaoNegativaEMedidaQueNaoEMaiorQueZero(double x, double y, double largura, double altura)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextoFormulario(x, y, largura, altura, null, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ImagemFormulario(x, y, largura, altura, Png));
    }

    [Fact]
    public void OTextoRecusaTamanhoDeFonteInvalido()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextoFormulario(0, 0, 1, 1, null, null) { TamanhoRotuloPt = 0 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextoFormulario(0, 0, 1, 1, null, null) { TamanhoValorPt = -7 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextoFormulario(0, 0, 1, 1, null, null) { TamanhoValorPt = double.NaN });
    }

    [Fact]
    public void OQrCodeEQuadradoERecusaTextoVazio()
    {
        var qr = new QrCodeFormulario(173.3, 15.2, 15.2, "https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=1");

        Assert.Equal((15.2d, 15.2d), (qr.LarguraMm, qr.AlturaMm));
        Assert.StartsWith("https://", qr.Texto, StringComparison.Ordinal);
        Assert.Throws<ArgumentNullException>(() => new QrCodeFormulario(0, 0, 1, null!));
        Assert.Throws<ArgumentException>(() => new QrCodeFormulario(0, 0, 1, string.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => new QrCodeFormulario(0, 0, 0, "x"));
    }

    [Fact]
    public void AImagemExigeOPngECopiaOsBytes()
    {
        var bytes = (byte[])Png.Clone();
        var imagem = new ImagemFormulario(0, 0, 40, 8.5, bytes);
        bytes[^1] = 99;

        Assert.Equal(Png, imagem.Png.ToArray());
        Assert.Throws<ArgumentException>(() => new ImagemFormulario(0, 0, 1, 1, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }));
        Assert.Throws<ArgumentException>(() => new ImagemFormulario(0, 0, 1, 1, ReadOnlyMemory<byte>.Empty));
    }

    [Fact]
    public void OConjuntoDeQuadrosEFechado()
    {
        var tipo = typeof(QuadroFormulario);
        var construtor = Assert.Single(tipo.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.True(construtor.IsFamilyAndAssembly);
        Assert.True(tipo.IsAbstract);
        var derivados = tipo.Assembly.GetTypes().Where(t => t.IsSubclassOf(tipo)).Select(t => t.Name).Order(StringComparer.Ordinal);
        Assert.Equal(["ImagemFormulario", "QrCodeFormulario", "TextoFormulario"], derivados);
    }

    [Fact]
    public void OPapelDoDanfseEOA4EmPeComAMargemMinimaDaNt008()
    {
        var papel = PapelImpressao.A4RetratoEstreito;

        Assert.Equal((TipoPapel.Folha, 210d, 297d, 0), (papel.Tipo, papel.LarguraMm, papel.AlturaMm, papel.Colunas));
        // De 0,15 a 0,20 cm em cada lado (NT 008/2026 2.2.2).
        Assert.InRange(papel.MargemMm, 1.5, 2.0);
    }
}
