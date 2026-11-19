namespace OpenDFe.Impressao.Pdf.Testes;

/// <summary>
/// O formulário de posição fixa no PDF (etapa 9.5, docs/NFSE.md 12): cada quadro na posição e no tamanho pedidos a partir do
/// canto da área útil, o rótulo e o valor nos tamanhos em pontos, o negrito e a cor, o sombreamento, as bordas, o corte com
/// reticências, o QR Code, a imagem PNG, a marca d'água da NT 008/2026 e as recusas.
/// </summary>
public sealed class RenderizadorPdfFormularioTestes
{
    private const double MmPt = 72 / 25.4;

    // Um PNG de 1 x 1 pixel (o menor PNG válido).
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static PdfLeitor Ler(params QuadroFormulario[] quadros) => Ler(new FormularioImpressao(quadros));

    private static PdfLeitor Ler(FormularioImpressao formulario) =>
        new(RenderizadorPdf.Gerar(new DocumentoImpressao(PapelImpressao.A4RetratoEstreito, [formulario])));

    // A posição da área útil do A4 estreito: 1,5 mm de margem.
    private static double Pt(double mmUtil) => (mmUtil + 1.5) * MmPt;

    [Fact]
    public void ORotuloEOValorSaemNaPosicaoDoQuadroNosTamanhosPedidos()
    {
        using var pdf = Ler(new TextoFormulario(10, 20, 50, 6.3, "Rótulo", "Valor"));

        Assert.Equal(1, pdf.Paginas);
        var rotulo = pdf.Achar(1, "Rótulo");
        var valor = pdf.Achar(1, "Valor");
        // O respiro de 0,5 mm à esquerda; o rótulo em cima, o valor embaixo, dentro da altura do quadro.
        Assert.Equal(Pt(10.5), rotulo.X, 0.6);
        Assert.Equal(Pt(10.5), valor.X, 0.6);
        Assert.InRange(rotulo.Y, Pt(20), Pt(26.3));
        Assert.InRange(valor.Y, rotulo.Y + 5, Pt(26.3));
        Assert.Equal((6d, true), (rotulo.Tamanho, rotulo.Negrito), new TamanhoNegrito());
        Assert.Equal((7d, false), (valor.Tamanho, valor.Negrito), new TamanhoNegrito());
    }

    [Fact]
    public void OTextoTemOTamanhoONegritoOAlinhamentoEACorPedidos()
    {
        using var pdf = Ler(new TextoFormulario(50, 10, 100, 4, null, "SEM VALIDADE")
        {
            TamanhoValorPt = 9,
            ValorNegrito = true,
            Alinhamento = AlinhamentoImpressao.Centro,
            Cor = CorImpressao.Vermelho,
        });

        var letras = pdf.Letras(1);
        Assert.All(letras, l => Assert.Equal((9d, true), (l.Tamanho, l.Negrito), new TamanhoNegrito()));
        // Centrado no quadro de 50 a 150 mm.
        Assert.Equal(Pt(100), (letras[0].X + letras[^1].FimX) / 2, 1.0);
        var cor = pdf.Pagina(1).Letters[0].Color.ToRGBValues();
        Assert.Equal((1d, 0d, 0d), ((double)cor.r, (double)cor.g, (double)cor.b), new Cor());
    }

    [Fact]
    public void OTextoQueNaoCabeNoQuadroSaiCortadoComReticenciasDentroDele()
    {
        var longo = string.Join(' ', Enumerable.Repeat("palavra comprida", 200));
        using var pdf = Ler(new TextoFormulario(10, 10, 60, 8, "Descrição", longo));

        var texto = pdf.TextoCompacto(1);
        Assert.EndsWith("...", texto, StringComparison.Ordinal);
        Assert.True(texto.Length < longo.Length / 2);
        // Nenhuma letra passa do quadro (8 mm a partir de 10 mm).
        Assert.All(pdf.Letras(1), l => Assert.InRange(l.Y, Pt(10), Pt(18)));
    }

    [Fact]
    public void OQuadroSombreadoTemOFundoCinzaClaroNaPosicaoDele()
    {
        using var pdf = Ler(new TextoFormulario(20, 30, 40, 5, null, "Título") { Sombreado = true });

        var altura = pdf.Pagina(1).Height;
        var fundo = Assert.Single(pdf.Pagina(1).Paths, c => c.IsFilled);
        var cor = fundo.FillColor!.ToRGBValues();
        // 5% de densidade (NT 008/2026 2.2.3): #F2F2F2.
        Assert.Equal(0.949, cor.r, 0.01);
        Assert.Equal(cor.r, cor.b, 0.001);
        var caixa = fundo.GetBoundingRectangle()!.Value;
        Assert.Equal(Pt(20), caixa.Left, 0.5);
        Assert.Equal(Pt(30), altura - caixa.Top, 0.5);
        Assert.Equal(40 * MmPt, caixa.Width, 0.5);
        Assert.Equal(5 * MmPt, caixa.Height, 0.5);
    }

    [Fact]
    public void ABordaDoFormularioEASDoQuadroSaoLinhas()
    {
        using var semBorda = Ler(new TextoFormulario(10, 10, 50, 5, null, "x"));
        using var comBorda = Ler(new FormularioImpressao([new TextoFormulario(10, 10, 50, 5, null, "x") { Bordas = BordaQuadro.Todas }]) { BordaPt = 1 });

        Assert.DoesNotContain(semBorda.Pagina(1).Paths, c => c.IsStroked || c.IsFilled);
        // A borda da área útil (1 pt) e as quatro linhas do quadro (0,5 pt).
        var caminhos = comBorda.Pagina(1).Paths;
        Assert.NotEmpty(caminhos);
        var maior = caminhos.Select(c => c.GetBoundingRectangle()!.Value).MaxBy(r => r.Width * r.Height);
        Assert.Equal(207 * MmPt, maior.Width, 1.5);
        Assert.Equal(294 * MmPt, maior.Height, 2.0);
    }

    [Fact]
    public void OQrCodeOcupaOQuadroComAMatrizDoQrCoder()
    {
        const string texto = "https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=31062002211222333000181000000000000422611123456789";
        using var pdf = Ler(new QrCodeFormulario(173.3, 15.2, 15.2, texto));

        var esperada = PdfVetores.MatrizQrCode(texto);
        var (matriz, caixa, _) = PdfVetores.MatrizQrCodePdf(pdf, 1, esperada.GetLength(0));
        Assert.Equal(esperada, matriz);
        Assert.Equal(Pt(173.3), caixa.Esquerda, 0.5);
        Assert.Equal(Pt(15.2), caixa.Topo, 0.5);
        Assert.Equal(15.2 * MmPt, caixa.Largura, 0.5);
    }

    [Fact]
    public void AImagemPngSaiNoQuadro()
    {
        using var pdf = Ler(new ImagemFormulario(3.4, 2.9, 40, 8.5, Png));

        var imagem = Assert.Single(pdf.Pagina(1).GetImages());
        var altura = pdf.Pagina(1).Height;
        // Encaixada sem distorcer e centrada: o pixel quadrado vira um quadrado de 8,5 mm no meio do quadro de 40 mm.
        Assert.Equal(8.5 * MmPt, imagem.BoundingBox.Height, 0.5);
        Assert.Equal(8.5 * MmPt, imagem.BoundingBox.Width, 0.5);
        Assert.Equal(Pt(3.4 + 20), imagem.BoundingBox.Left + imagem.BoundingBox.Width / 2, 0.5);
        Assert.Equal(Pt(2.9), altura - imagem.BoundingBox.Top, 0.5);
    }

    [Theory]
    [InlineData("CANCELADA")]
    [InlineData("SUBSTITUÍDA")]
    public void AMarcaDAguaDoFormularioEDiagonalSemNegritoCinzaK35ComNoMinimo50Pontos(string marca)
    {
        using var pdf = Ler(new FormularioImpressao([new TextoFormulario(10, 10, 50, 5, null, "corpo")]) { MarcaAgua = marca });

        var letras = pdf.Pagina(1).Letters.Where(l => l.PointSize > 20).ToList();
        Assert.Equal(marca, string.Concat(letras.Select(l => l.Value)));
        Assert.All(letras, l => Assert.DoesNotContain("Bold", l.FontName, StringComparison.OrdinalIgnoreCase));
        var cor = letras[0].Color.ToRGBValues();
        Assert.Equal(0.651, cor.r, 0.01);
        Assert.Equal(cor.r, cor.g, 0.001);
        // Em diagonal, atrás do corpo (desenhada antes) e com 50 pontos ou mais.
        Assert.True(letras[^1].StartBaseLine.Y > letras[0].StartBaseLine.Y);
        Assert.Equal(marca[0].ToString(), pdf.Pagina(1).Letters[0].Value);
        var lida = pdf.Letras(1).Where(l => l.Tamanho > 20).ToList();
        Assert.All(lida, l => Assert.True(l.Tamanho >= 49.5, $"A letra tem {l.Tamanho} pt."));
    }

    [Fact]
    public void OQuadroForaDaAreaUtilEOFormularioNaBobinaSaoRecusados()
    {
        Assert.Throws<ArgumentException>(() => RenderizadorPdf.Gerar(new DocumentoImpressao(PapelImpressao.A4RetratoEstreito,
            [new FormularioImpressao([new TextoFormulario(200, 10, 10, 5, null, "x")])])));
        Assert.Throws<ArgumentException>(() => RenderizadorPdf.Gerar(new DocumentoImpressao(PapelImpressao.A4RetratoEstreito,
            [new FormularioImpressao([new TextoFormulario(10, 290, 10, 5, null, "x")])])));
        Assert.Throws<ArgumentException>(() => RenderizadorPdf.Gerar(new DocumentoImpressao(PapelImpressao.Bobina80,
            [new FormularioImpressao([new TextoFormulario(1, 1, 10, 5, null, "x")])])));
    }

    [Fact]
    public void OQuadroNaBeiradaDaAreaUtilCabe()
    {
        using var pdf = Ler(new TextoFormulario(0, 0, 207, 293.95, null, "canto"));

        Assert.Equal(1, pdf.Paginas);
        Assert.Equal(Pt(0.5), pdf.Achar(1, "canto").X, 0.6);
    }

    // O tamanho em pontos com folga de 0,1 e o negrito.
    private sealed class TamanhoNegrito : IEqualityComparer<(double Tamanho, bool Negrito)>
    {
        public bool Equals((double Tamanho, bool Negrito) x, (double Tamanho, bool Negrito) y) =>
            Math.Abs(x.Tamanho - y.Tamanho) < 0.1 && x.Negrito == y.Negrito;

        public int GetHashCode((double Tamanho, bool Negrito) obj) => obj.Negrito.GetHashCode();
    }

    private sealed class Cor : IEqualityComparer<(double R, double G, double B)>
    {
        public bool Equals((double R, double G, double B) x, (double R, double G, double B) y) =>
            Math.Abs(x.R - y.R) < 0.01 && Math.Abs(x.G - y.G) < 0.01 && Math.Abs(x.B - y.B) < 0.01;

        public int GetHashCode((double R, double G, double B) obj) => 0;
    }
}
