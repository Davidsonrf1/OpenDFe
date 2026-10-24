namespace OpenDFe.Impressao.Pdf.Testes;

/// <summary>
/// O renderizador por elemento do modelo (etapa 7.4, docs/CONTRATOS.md 22): cada elemento vira PDF e o PdfPig confere de volta o
/// texto, a fonte, a posição, os vetores e o papel. Os documentos são montados à mão, um elemento por vez.
/// </summary>
public class RenderizadorPdfElementosTestes
{
    private const double MmPt = 72 / 25.4;

    private static PdfLeitor Ler(DocumentoImpressao documento, OpcoesPdf? opcoes = null) => new(RenderizadorPdf.Gerar(documento, opcoes));

    private static DocumentoImpressao Doc(PapelImpressao papel, params ElementoImpressao[] elementos) => new(papel, elementos);

    private static TextoImpressao Texto(string texto, AlinhamentoImpressao alinhamento = AlinhamentoImpressao.Esquerda, bool negrito = false,
        TamanhoTexto tamanho = TamanhoTexto.Normal, bool invertido = false) =>
        new(texto) { Alinhamento = alinhamento, Negrito = negrito, Tamanho = tamanho, Invertido = invertido };

    // ---- Texto ------------------------------------------------------------------------------------------------

    [Fact]
    public void OTextoSaiNaOrdemDoDocumentoComOsAcentos()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("Primeiro: ação, coração, ÁÉÍÓÚ, ç ã õ"), Texto("Segundo"), Texto("Terceiro")));

        Assert.Equal(1, pdf.Paginas);
        Assert.Contains("Primeiro:ação,coração,ÁÉÍÓÚ,çãõ", pdf.TextoCompacto(1), StringComparison.Ordinal);
        var primeiro = pdf.Achar(1, "Primeiro");
        var segundo = pdf.Achar(1, "Segundo");
        var terceiro = pdf.Achar(1, "Terceiro");
        Assert.True(primeiro.Y < segundo.Y && segundo.Y < terceiro.Y, "os textos saem de cima para baixo, na ordem do documento");
        Assert.True(pdf.TextoCompacto(1).IndexOf("Segundo", StringComparison.Ordinal) < pdf.TextoCompacto(1).IndexOf("Terceiro", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(TamanhoTexto.Pequeno, 6, false)]
    [InlineData(TamanhoTexto.Normal, 8, false)]
    [InlineData(TamanhoTexto.Duplo, 12, true)]
    public void ONegritoEOTamanhoDoTextoNaBobinaSaoOsDaEscalaDaTermica(TamanhoTexto tamanho, double pontos, bool negrito)
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("Letra", negrito: negrito, tamanho: tamanho)));

        var letra = pdf.Achar(1, "Letra");
        Assert.Equal(pontos, letra.Tamanho, 0.05);
        Assert.Equal(negrito, letra.Negrito);
    }

    [Theory]
    [InlineData(TamanhoTexto.Pequeno, 6)]
    [InlineData(TamanhoTexto.Normal, 8)]
    [InlineData(TamanhoTexto.Duplo, 12)]
    public void OTamanhoDoTextoNaFolhaTemOsMinimosDoAnexoIIDoDanfe(TamanhoTexto tamanho, double pontos)
    {
        // 3.7: o texto pequeno de no mínimo 6 pt, os dados do emitente e o "DOCUMENTO AUXILIAR" de 8 pt e o "DANFE" de 12 pt.
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, Texto("Letra", tamanho: tamanho, negrito: true)));

        var letra = pdf.Achar(1, "Letra");
        Assert.Equal(pontos, letra.Tamanho, 0.05);
        Assert.True(letra.Negrito);
    }

    [Fact]
    public void OTextoNaoNegritoNaoSaiEmNegrito()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("Normal"), Texto("Forte", negrito: true)));

        Assert.False(pdf.Achar(1, "Normal").Negrito);
        Assert.True(pdf.Achar(1, "Forte").Negrito);
    }

    [Fact]
    public void OAlinhamentoDoTextoMudaAPosicaoHorizontal()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80,
            Texto("Esquerda", AlinhamentoImpressao.Esquerda), Texto("Centro", AlinhamentoImpressao.Centro), Texto("Direita", AlinhamentoImpressao.Direita)));

        var esquerda = pdf.Achar(1, "Esquerda");
        var centro = pdf.Achar(1, "Centro");
        var direita = pdf.Achar(1, "Direita");
        var margem = 4 * MmPt;
        var largura = 72 * MmPt;
        Assert.Equal(margem, esquerda.X, 0.5);
        Assert.True(esquerda.X < centro.X && centro.X < direita.X);
        Assert.Equal(margem + largura / 2, (centro.X + pdf.Letras(1).Where(l => l.Y == centro.Y).Max(l => l.FimX)) / 2, 0.8);
        Assert.Equal(margem + largura, pdf.Letras(1).Where(l => l.Y == direita.Y).Max(l => l.FimX), 0.8);
    }

    [Fact]
    public void OTextoInvertidoTemFundoPretoELetraBranca()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("Invertido", invertido: true)));

        var pagina = pdf.Pagina(1);
        var caminho = Assert.Single(pagina.Paths);
        Assert.True(caminho.IsFilled);
        Assert.Equal(0, caminho.FillColor!.ToRGBValues().r, 0.01);
        var letra = pagina.Letters[0];
        Assert.Equal(1, letra.Color.ToRGBValues().r, 0.01);
    }

    [Fact]
    public void ONomeDoProdutoComQuebraDeLinhaSaiEmDuasLinhas()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("Linha um\nLinha dois")));

        var um = pdf.Achar(1, "Linha um");
        var dois = pdf.Achar(1, "Linha dois");
        Assert.Equal(8 * 1.15, dois.Y - um.Y, 0.2);
    }

    [Fact]
    public void ATextoLongoQuebraENuncaSeCorta()
    {
        var longo = string.Join(' ', Enumerable.Range(1, 120).Select(i => $"palavra{i:000}"));
        using var pdf = Ler(Doc(PapelImpressao.Bobina58, Texto(longo)));

        // Cada palavra está inteira no PDF, na ordem, e o texto passou das linhas de uma bobina de 58 mm.
        var texto = pdf.TextoCompacto(1);
        for (var i = 1; i <= 120; i++)
            Assert.Contains($"palavra{i:000}", texto, StringComparison.Ordinal);
        Assert.True(pdf.Letras(1).Select(l => Math.Round(l.Y)).Distinct().Count() > 10);
        Assert.All(pdf.Letras(1), l => Assert.True(l.FimX <= 58 * MmPt - 5 * MmPt + 0.5, "nenhuma letra passa da margem direita"));
    }

    [Fact]
    public void ApalavraMaiorQueALinhaQuebraNaLetraENaoSomeDaPagina()
    {
        var palavra = new string('W', 80);
        using var pdf = Ler(Doc(PapelImpressao.Bobina58, Texto(palavra)));

        Assert.Equal(palavra, pdf.TextoCompacto(1));
        Assert.All(pdf.Letras(1), l => Assert.True(l.FimX <= 58 * MmPt - 5 * MmPt + 0.5));
    }

    // ---- Linha de células ---------------------------------------------------------------------------------------

    [Fact]
    public void AsCelulasDaLinhaTemLarguraProporcionalAoPeso()
    {
        var linha = new LinhaImpressao([new CelulaImpressao("AA", 1), new CelulaImpressao("BB", 2), new CelulaImpressao("CC", 1)]);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, linha));

        var margem = 4 * MmPt;
        var largura = 72 * MmPt;
        Assert.Equal(margem, pdf.Achar(1, "AA").X, 0.5);
        Assert.Equal(margem + largura * 1 / 4, pdf.Achar(1, "BB").X, 0.5);
        Assert.Equal(margem + largura * 3 / 4, pdf.Achar(1, "CC").X, 0.5);
    }

    [Fact]
    public void OValorDaUltimaCelulaADireitaTerminaNaMargemEMesmoComPesoPequeno()
    {
        var linha = new LinhaImpressao([new CelulaImpressao("FORMA PAGAMENTO", 35), new CelulaImpressao("VALOR PAGO R$", 13) { Alinhamento = AlinhamentoImpressao.Direita }]);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, linha));

        // O peso 13/48 não comportaria "VALOR PAGO R$" numa fonte proporcional: a célula toma a largura do conteúdo e o valor não quebra.
        var letras = pdf.Letras(1);
        Assert.Single(letras.Select(l => Math.Round(l.Y)).Distinct());
        Assert.Equal((4 + 72) * MmPt, letras.Max(l => l.FimX), 0.8);
        Assert.Equal("FORMAPAGAMENTOVALORPAGOR$", pdf.TextoCompacto(1));
    }

    [Fact]
    public void ACelulaEmNegritoSaiEmNegrito()
    {
        var linha = new LinhaImpressao([new CelulaImpressao("Normal"), new CelulaImpressao("Forte") { Negrito = true }]);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, linha));

        Assert.False(pdf.Achar(1, "Normal").Negrito);
        Assert.True(pdf.Achar(1, "Forte").Negrito);
    }

    [Fact]
    public void OTextoLongoDeUmaCelulaQuebraDentroDelaEAlinhaOsVizinhosNoTopo()
    {
        var linha = new LinhaImpressao([new CelulaImpressao("um dois tres quatro cinco seis sete oito nove dez onze doze", 1), new CelulaImpressao("Vizinha", 1)]);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, linha));

        Assert.Equal(pdf.Achar(1, "um").Y, pdf.Achar(1, "Vizinha").Y, 0.01);
        Assert.Contains("doze", pdf.TextoCompacto(1), StringComparison.Ordinal);
    }

    // ---- Separador e espaço -----------------------------------------------------------------------------------

    [Theory]
    [InlineData(EstiloSeparador.Cheio)]
    [InlineData(EstiloSeparador.Tracejado)]
    public void OSeparadorEUmaLinhaDaLarguraUtil(EstiloSeparador estilo)
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("antes"), new SeparadorImpressao { Estilo = estilo }, Texto("depois")));

        var caminho = Assert.Single(pdf.Pagina(1).Paths);
        var retangulo = caminho.GetBoundingRectangle()!.Value;
        Assert.Equal(72 * MmPt, retangulo.Width, 1.0);
        var linha = pdf.Pagina(1).Height - retangulo.Top;
        Assert.True(pdf.Achar(1, "antes").Y < linha && linha < pdf.Achar(1, "depois").Y, "o separador fica entre os dois textos");
        // O cheio é um traço contínuo (um subcaminho); o tracejado, um segmento por traço.
        Assert.Equal(estilo == EstiloSeparador.Tracejado, caminho.Count > 10);
    }

    [Fact]
    public void OEspacoTemAAlturaDeLinhasDoTextoNormal()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("A"), new EspacoImpressao(3), Texto("B")));

        // A linha do texto normal tem 8 pt x 1,15 = 9,2 pt: o texto "A", as três linhas do espaço e a linha do "B".
        Assert.Equal(4 * 9.2, pdf.Achar(1, "B").Y - pdf.Achar(1, "A").Y, 0.3);
    }

    // ---- QR Code ---------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("https://nfce.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml?p=31150311222333000181650010000001911123456788|2|1|1|ABCDEF0123456789ABCDEF0123456789ABCDEF01")]
    [InlineData("https://exemplo.gov.br/qr?p=ÇÃO|3|2 com espaço e acento")]
    [InlineData("curto")]
    public void OQrCodeDoPdfTemAMatrizDoQrCoderNoNivelM(string texto)
    {
        var esperada = PdfVetores.MatrizQrCode(texto);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, new QrCodeImpressao(texto)));

        var (matriz, _, _) = PdfVetores.MatrizQrCodePdf(pdf, 1, esperada.GetLength(0));
        Assert.Equal(esperada.Cast<bool>(), matriz.Cast<bool>());
    }

    [Fact]
    public void OQrCodeTem30mmComAMargemDeSilencioDentroEFicaCentrado()
    {
        const string texto = "https://nfce.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml?p=31150311222333000181650010000001911123456788|2|1|1|ABCDEF0123456789ABCDEF0123456789ABCDEF01";
        var lado = PdfVetores.MatrizQrCode(texto).GetLength(0);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, new QrCodeImpressao(texto) { LadoMm = 30 }));

        var (_, caixa, modulo) = PdfVetores.MatrizQrCodePdf(pdf, 1, lado);
        var margemEsperada = RenderizadorPdf.MargemQrCode(30, lado);
        // O símbolo ocupa o lado total menos as duas margens; a margem do manual (10% de 30 mm = 3 mm) cede aos 4 módulos se maiores.
        Assert.Equal((30 - 2 * margemEsperada) * MmPt, caixa.Largura, 0.1);
        Assert.Equal(caixa.Largura, caixa.Altura, 0.1);
        Assert.True(margemEsperada >= 3 - 1e-9, "a margem de silêncio de ao menos 3 mm (Manual do DANFE NFC-e 3.4)");
        Assert.True(margemEsperada >= 4 * modulo / MmPt - 1e-6, "e de ao menos 4 módulos (ISO/IEC 18004)");
        // Centrado na área impressa: o centro do símbolo é o centro da bobina de 80 mm.
        Assert.Equal(80 * MmPt / 2, caixa.Esquerda + caixa.Largura / 2, 0.3);
        // Dentro da página, depois da margem superior de 3 mm mais a margem de silêncio.
        Assert.Equal((3 + margemEsperada) * MmPt, caixa.Topo, 0.3);
    }

    [Theory]
    [InlineData(25, 3.0)]
    [InlineData(22, 3.0)]
    [InlineData(40, 4.0)]
    public void AMargemDeSilencioSegueOManualAteOsVinteECincoMilimetrosEDezPorCentoAcima(double lado, double margemMinima)
    {
        // Versão 3 (29 módulos): 4 módulos de 25 mm são 25 * 4 / 37 = 2,7 mm, menos que os 3 mm do manual.
        var margem = RenderizadorPdf.MargemQrCode(lado, 29);

        Assert.Equal(Math.Max(margemMinima, 4 * lado / 37), margem, 1e-9);
    }

    [Fact]
    public void ComUmQrCodeDeMuitosModulosOsQuatroModulosVencemAMargemDoManual()
    {
        // Versão 40 (177 módulos) em 30 mm: 4 módulos são 30 * 4 / 185 = 0,65 mm, menos que 3 mm, e o manual vence (3 mm).
        Assert.Equal(3.0, RenderizadorPdf.MargemQrCode(30, 177), 1e-9);
        // Em 100 mm com 10 módulos de margem de 4 módulos (modulos = 21): 100 * 4 / 29 = 13,8 mm, mais que os 10% (10 mm).
        Assert.Equal(100 * 4.0 / 29, RenderizadorPdf.MargemQrCode(100, 21), 1e-9);
    }

    [Theory]
    [InlineData(AlinhamentoImpressao.Esquerda)]
    [InlineData(AlinhamentoImpressao.Direita)]
    public void OAlinhamentoDoQrCodeLevaOBlocoParaAEsquerdaOuADireita(AlinhamentoImpressao alinhamento)
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, new QrCodeImpressao("teste") { Alinhamento = alinhamento, LadoMm = 25 }));

        var lado = PdfVetores.MatrizQrCode("teste").GetLength(0);
        var (_, caixa, _) = PdfVetores.MatrizQrCodePdf(pdf, 1, lado);
        var margem = RenderizadorPdf.MargemQrCode(25, lado) * MmPt;
        if (alinhamento == AlinhamentoImpressao.Esquerda)
            Assert.Equal(4 * MmPt + margem, caixa.Esquerda, 0.3);
        else
            Assert.Equal((4 + 72) * MmPt - margem, caixa.Esquerda + caixa.Largura, 0.3);
    }

    // ---- Código de barras ------------------------------------------------------------------------------------------

    private const string Chave = "31261011222333000181550010000001231123456784";

    [Fact]
    public void OCodigoDeBarrasDaChaveTem277ModulosDeLarguraEBarrasEmNumeroInteiroDeModulos()
    {
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, new CodigoBarrasImpressao(Chave)));

        // Cada barra na posição e na largura do Code 128 da chave: o PDF desenha a chave dada, não outra.
        PdfVetores.ConferirBarras(pdf, 1, pdf.MaiorCaminho(1), Chave);
        var retangulos = pdf.Retangulos(1, pdf.MaiorCaminho(1));
        var esquerda = retangulos.Min(r => r.Esquerda);
        var direita = retangulos.Max(r => r.Esquerda + r.Largura);
        // 22 pares de 11 módulos, o início C (11), o verificador (11) e o fim (13): 277 módulos (Anexo II 2: 242 + 11 + 11 + 13).
        var modulo = (direita - esquerda) / 277;
        Assert.Equal(76, retangulos.Count);
        Assert.All(retangulos, r => Assert.Equal(Math.Round(r.Largura / modulo), r.Largura / modulo, 0.02));
        Assert.All(retangulos, r => Assert.InRange(r.Largura / modulo, 0.98, 4.02));
        // A largura de 11,5 cm do Anexo II 3.8.1 (bem acima do mínimo de 6 cm do item 2) e o módulo acima de 0,2 mm.
        Assert.Equal(115 * MmPt, direita - esquerda, 0.2);
        Assert.True(modulo / MmPt >= 0.2, "o módulo da barra tem ao menos 0,2 mm (Anexo II 2)");
        Assert.True(direita - esquerda >= 60 * MmPt, "a largura total é de ao menos 6 cm (Anexo II 2)");
    }

    [Fact]
    public void OCodigoDeBarrasTemAAlturaPedidaEAMargemClaraDeDezModulos()
    {
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, new CodigoBarrasImpressao(Chave) { AlturaMm = 12 }));

        var retangulos = pdf.Retangulos(1, pdf.MaiorCaminho(1));
        var topo = retangulos.Min(r => r.Topo);
        var baixo = retangulos.Max(r => r.Topo + r.Altura);
        var esquerda = retangulos.Min(r => r.Esquerda);
        var direita = retangulos.Max(r => r.Esquerda + r.Largura);
        var modulo = (direita - esquerda) / 277;
        Assert.Equal(12 * MmPt, baixo - topo, 0.2);
        Assert.True(12 >= 8, "a altura mínima da barra é de 0,8 cm (Anexo II 2)");
        // Centrado na área impressa (a margem do A4 é de 8 mm): sobra a margem clara dos dois lados, de ao menos 10 módulos.
        var margemEsquerda = esquerda - 8 * MmPt;
        var margemDireita = (210 - 8) * MmPt - direita;
        Assert.Equal(margemEsquerda, margemDireita, 0.3);
        Assert.True(margemEsquerda >= 10 * modulo - 0.01);
    }

    [Fact]
    public void NaBobinaOCodigoDeBarrasCabeNaLarguraUtilComAMargemClara()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, new CodigoBarrasImpressao(Chave)));

        var retangulos = pdf.Retangulos(1, pdf.MaiorCaminho(1));
        var esquerda = retangulos.Min(r => r.Esquerda);
        var direita = retangulos.Max(r => r.Esquerda + r.Largura);
        var modulo = (direita - esquerda) / 277;
        Assert.True(esquerda - 4 * MmPt >= 10 * modulo - 0.01);
        Assert.True((80 - 4) * MmPt - direita >= 10 * modulo - 0.01);
        Assert.True(direita - esquerda < 72 * MmPt);
    }

    [Fact]
    public void OCodigoDeBarrasDeOutraChaveTemOutrasBarrasEOMesmoTotalDeModulos()
    {
        using var um = Ler(Doc(PapelImpressao.A4Retrato, new CodigoBarrasImpressao(Chave)));
        using var dois = Ler(Doc(PapelImpressao.A4Retrato, new CodigoBarrasImpressao("35260311222333000181550010000004561987654326")));

        var a = um.Retangulos(1, um.MaiorCaminho(1));
        var b = dois.Retangulos(1, dois.MaiorCaminho(1));
        Assert.NotEqual(a.Select(r => Math.Round(r.Esquerda, 1)), b.Select(r => Math.Round(r.Esquerda, 1)));
        Assert.Equal(a.Max(r => r.Esquerda + r.Largura) - a.Min(r => r.Esquerda), b.Max(r => r.Esquerda + r.Largura) - b.Min(r => r.Esquerda), 0.2);
    }

    // ---- Imagem de 1 bit ----------------------------------------------------------------------------------------

    private static Imagem1Bit Moldura(int lado)
    {
        var bytesLinha = (lado + 7) / 8;
        var bits = new byte[bytesLinha * lado];
        for (var y = 0; y < lado; y++)
            for (var x = 0; x < lado; x++)
                if (x == 0 || y == 0 || x == lado - 1 || y == lado - 1 || x == y)
                    bits[y * bytesLinha + x / 8] |= (byte)(0x80 >> (x % 8));
        return new Imagem1Bit(lado, lado, bits);
    }

    [Fact]
    public void AImagemDe1BitSaiComOsPixelsNaLarguraPedida()
    {
        var imagem = Moldura(20);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, new ImagemImpressao(imagem) { LarguraMm = 10, Alinhamento = AlinhamentoImpressao.Esquerda }));

        var (matriz, caixa, modulo) = PdfVetores.MatrizQrCodePdf(pdf, 1, 20);
        Assert.Equal(10 * MmPt, caixa.Largura, 0.1);
        Assert.Equal(0.5 * MmPt, modulo, 0.01);
        for (var y = 0; y < 20; y++)
            for (var x = 0; x < 20; x++)
                Assert.Equal(imagem.Preto(x, y), matriz[y, x]);
        Assert.Equal(4 * MmPt, caixa.Esquerda, 0.3);
    }

    [Fact]
    public void AImagemSemLarguraTemOTamanhoNaturalA203Dpi()
    {
        var imagem = Moldura(203);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, new ImagemImpressao(imagem)));

        var (_, caixa, _) = PdfVetores.MatrizQrCodePdf(pdf, 1, 203);
        Assert.Equal(25.4 * MmPt, caixa.Largura, 0.1);
        Assert.Equal(80 * MmPt / 2, caixa.Esquerda + caixa.Largura / 2, 0.3);
    }

    [Fact]
    public void AImagemMaiorQueALinhaEncolheParaALarguraUtil()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina58, new ImagemImpressao(Moldura(20)) { LarguraMm = 200 }));

        var (_, caixa, _) = PdfVetores.MatrizQrCodePdf(pdf, 1, 20);
        Assert.Equal(48 * MmPt, caixa.Largura, 0.2);
    }

    [Fact]
    public void AImagemTodaBrancaNaoDesenhaNadaMasGastaOEspaco()
    {
        var branca = new Imagem1Bit(120, 40, new byte[15 * 40]);
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("antes"), new ImagemImpressao(branca) { LarguraMm = 30 }, Texto("depois")));

        Assert.Empty(pdf.Pagina(1).Paths);
        // 30 mm de largura e 10 mm de altura (120 x 40) entre os dois textos.
        Assert.True(pdf.Achar(1, "depois").Y - pdf.Achar(1, "antes").Y >= 10 * MmPt);
    }

    // ---- Seção e tabela -------------------------------------------------------------------------------------------

    private static SecaoImpressao Secao(string? titulo, params LinhaCamposImpressao[] linhas) => new(titulo, linhas);

    [Fact]
    public void ASecaoTemOTituloOsRotulosPequenosEOsValoresMaioresEmQuadros()
    {
        var secao = Secao("DESTINATÁRIO / REMETENTE", new LinhaCamposImpressao([
            new CampoImpressao("NOME / RAZÃO SOCIAL", "Loja Centro Ltda", 3), new CampoImpressao("CNPJ / CPF", "34.028.316/0001-03", 1) { Alinhamento = AlinhamentoImpressao.Direita }]));
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, secao));

        var titulo = pdf.Achar(1, "DESTINATÁRIO");
        var rotulo = pdf.Achar(1, "NOME");
        var valor = pdf.Achar(1, "Loja");
        // 3.7: o título do bloco de 5 pt em negrito, o rótulo do campo de 6 pt e o conteúdo de 10 pt.
        Assert.Equal(5, titulo.Tamanho, 0.05);
        Assert.True(titulo.Negrito);
        Assert.Equal(6, rotulo.Tamanho, 0.05);
        Assert.False(rotulo.Negrito);
        Assert.Equal(10, valor.Tamanho, 0.05);
        Assert.True(titulo.Y < rotulo.Y && rotulo.Y < valor.Y);
        // Os dois campos na mesma fileira, na proporção 3:1 da largura útil (194 mm).
        var margem = 8 * MmPt;
        Assert.Equal(margem + 194 * MmPt * 3 / 4 , pdf.Achar(1, "CNPJ").X, 2.5);
        // A borda fina: dois quadros.
        Assert.Equal(2, pdf.Pagina(1).Paths.Count(c => c.IsStroked));
    }

    [Fact]
    public void OCampoQueQuebraEmVariasLinhasEsticaOsVizinhosNaMesmaFileira()
    {
        var secao = Secao(null, new LinhaCamposImpressao([
            new CampoImpressao("ALTO", "um\ndois\ntres\nquatro", 1), new CampoImpressao("BAIXO", "x", 1)]));
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, secao));

        var quadros = pdf.Pagina(1).Paths.Where(c => c.IsStroked).Select(c => c.GetBoundingRectangle()!.Value).ToList();
        Assert.Equal(2, quadros.Count);
        Assert.Equal(quadros[0].Height, quadros[1].Height, 0.1);
        Assert.Equal(quadros[0].Top, quadros[1].Top, 0.1);
        Assert.Equal(pdf.Achar(1, "ALTO").Y, pdf.Achar(1, "BAIXO").Y, 0.01);
    }

    private static TabelaImpressao Tabela(int linhas, string titulo = "DADOS DOS PRODUTOS / SERVIÇOS") => new(titulo,
        [new ColunaImpressao("CODIGO", 1), new ColunaImpressao("DESCRICAO", 4), new ColunaImpressao("VALOR", 1.5, AlinhamentoImpressao.Direita)],
        [.. Enumerable.Range(1, linhas).Select(i => (IReadOnlyList<string>)[$"L{i:000}", $"Produto número {i}", $"{i},00"])])
    {
        LinhasIndivisiveis = true,
        DivisorLinhas = true,
    };

    [Fact]
    public void ATabelaDe150LinhasPassaDeUmaPaginaERepeteOCabecalhoEmCadaUma()
    {
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, Tabela(150)));

        Assert.True(pdf.Paginas > 1);
        // O título da coluna aparece uma vez por página, e o título do bloco só na primeira.
        for (var pagina = 1; pagina <= pdf.Paginas; pagina++)
            Assert.Equal(1, Contar(pdf.TextoCompacto(pagina), "CODIGO"));
        Assert.Equal(1, Contar(pdf.TextoCompacto(), "DADOSDOSPRODUTOS/SERVIÇOS"));
        // Todas as linhas estão, uma vez cada, e na ordem.
        var texto = pdf.TextoCompacto();
        Assert.Equal(150, Contar(texto, "Produtonúmero"));
        var ultima = -1;
        for (var i = 1; i <= 150; i++)
        {
            var posicao = texto.IndexOf($"L{i:000}Produtonúmero{i}{i},00", StringComparison.Ordinal);
            Assert.True(posicao > ultima, $"a linha {i} não está na ordem");
            ultima = posicao;
        }
    }

    [Fact]
    public void ATabelaCabeEmUmaPaginaComPoucasLinhasESemCabecalhoRepetido()
    {
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, Tabela(5)));

        Assert.Equal(1, pdf.Paginas);
        Assert.Equal(1, Contar(pdf.TextoCompacto(), "CODIGO"));
    }

    [Fact]
    public void AsColunasDaTabelaTemLarguraProporcionalAoPesoEOAlinhamentoDaColuna()
    {
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, Tabela(3)));

        var margem = 8 * MmPt;
        var util = 194 * MmPt;
        var codigo = pdf.Achar(1, "L001");
        var descricao = pdf.Achar(1, "Produto número 1");
        Assert.Equal(margem, codigo.X, 1.5);
        Assert.Equal(margem + util * 1 / 6.5, descricao.X, 1.5);
        // A coluna do valor, à direita: o texto termina perto do fim da coluna.
        var valor = pdf.Letras(1).Where(l => Math.Abs(l.Y - codigo.Y) < 0.5).Max(l => l.FimX);
        Assert.Equal(margem + util, valor, 2.0);
        // A célula em 6 pt (3.7.7) e o título da coluna em 5 pt negrito (3.7.2).
        Assert.Equal(6, codigo.Tamanho, 0.05);
        var cabecalho = pdf.Achar(1, "CODIGO");
        Assert.Equal(5, cabecalho.Tamanho, 0.05);
        Assert.True(cabecalho.Negrito);
    }

    [Fact]
    public void OTextoDeUmaCelulaDaTabelaComQuebraDeLinhaSaiEmLinhasDaMesmaCelula()
    {
        var tabela = new TabelaImpressao(null, [new ColunaImpressao("A", 1), new ColunaImpressao("B", 1)],
            [(IReadOnlyList<string>)["primeira\nsegunda\nterceira", "ao lado"]]) { LinhasIndivisiveis = true };
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, tabela));

        Assert.True(pdf.Achar(1, "primeira").Y < pdf.Achar(1, "segunda").Y && pdf.Achar(1, "segunda").Y < pdf.Achar(1, "terceira").Y);
        Assert.Equal(pdf.Achar(1, "primeira").Y, pdf.Achar(1, "ao lado").Y, 0.5);
    }

    [Fact]
    public void ALinhaIndivisivelNaoSeDivideEntreDuasPaginas()
    {
        // Cada linha tem 12 linhas de texto (uma célula multilinha): em nenhuma página a linha aparece pela metade.
        var multilinha = string.Join('\n', Enumerable.Range(1, 12).Select(i => $"parte{i:00}"));
        var tabela = new TabelaImpressao("ITENS", [new ColunaImpressao("COL", 1)],
            [.. Enumerable.Range(1, 40).Select(i => (IReadOnlyList<string>)[$"item{i:00}\n{multilinha}"])])
        { LinhasIndivisiveis = true, DivisorLinhas = true };
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, tabela));

        Assert.True(pdf.Paginas > 1);
        for (var pagina = 1; pagina <= pdf.Paginas; pagina++)
        {
            var texto = pdf.TextoCompacto(pagina);
            foreach (var i in Enumerable.Range(1, 40).Where(i => texto.Contains($"item{i:00}", StringComparison.Ordinal)))
                Assert.Contains($"item{i:00}parte01", texto, StringComparison.Ordinal);
            foreach (var i in Enumerable.Range(1, 40).Where(i => texto.Contains($"item{i:00}", StringComparison.Ordinal)))
                Assert.Contains($"item{i:00}parte01parte02parte03parte04parte05parte06parte07parte08parte09parte10parte11parte12", texto, StringComparison.Ordinal);
        }
    }

    private static int Contar(string texto, string trecho)
    {
        var total = 0;
        for (var i = texto.IndexOf(trecho, StringComparison.Ordinal); i >= 0; i = texto.IndexOf(trecho, i + trecho.Length, StringComparison.Ordinal))
            total++;
        return total;
    }

    // ---- Quebra de página, cabeçalho de continuação, marca d'água e fichas -----------------------------------------

    [Fact]
    public void ANaFolhaAQuebraDePaginaComecaUmaPaginaNova()
    {
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, Texto("Primeira"), new QuebraPaginaImpressao(), Texto("Segunda")));

        Assert.Equal(2, pdf.Paginas);
        Assert.Contains("Primeira", pdf.TextoCompacto(1), StringComparison.Ordinal);
        Assert.DoesNotContain("Segunda", pdf.TextoCompacto(1), StringComparison.Ordinal);
        Assert.Contains("Segunda", pdf.TextoCompacto(2), StringComparison.Ordinal);
    }

    [Fact]
    public void NaBobinaAQuebraDePaginaCortaEFazDoisBilhetesDeAlturaContinua()
    {
        using var pdf = Ler(Doc(PapelImpressao.Bobina80, Texto("Via um"), Texto("linha"), new QuebraPaginaImpressao(), Texto("Via dois")));

        Assert.Equal(2, pdf.Paginas);
        Assert.Contains("Viaum", pdf.TextoCompacto(1), StringComparison.Ordinal);
        Assert.Contains("Viadois", pdf.TextoCompacto(2), StringComparison.Ordinal);
        Assert.True(pdf.Pagina(1).Height > pdf.Pagina(2).Height, "cada bilhete tem a altura do próprio conteúdo");
        Assert.Equal(80 * MmPt, pdf.Pagina(2).Width, 0.5);
    }

    [Fact]
    public void OCabecalhoDeContinuacaoSaiSoDaSegundaPaginaEmDiante()
    {
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato, [Texto("Corpo um"), new QuebraPaginaImpressao(), Texto("Corpo dois"), new QuebraPaginaImpressao(), Texto("Corpo tres")])
        {
            CabecalhoContinuacao = [Texto("CABECALHO-CONTINUACAO")],
        };
        using var pdf = Ler(documento);

        Assert.Equal(3, pdf.Paginas);
        Assert.DoesNotContain("CABECALHO-CONTINUACAO", pdf.TextoCompacto(1), StringComparison.Ordinal);
        Assert.Contains("CABECALHO-CONTINUACAO", pdf.TextoCompacto(2), StringComparison.Ordinal);
        Assert.Contains("CABECALHO-CONTINUACAO", pdf.TextoCompacto(3), StringComparison.Ordinal);
        // O cabeçalho vem antes do corpo e a primeira página começa no topo, sem o espaço dele.
        Assert.True(pdf.Achar(2, "CABECALHO").Y < pdf.Achar(2, "Corpo").Y);
        Assert.True(pdf.Achar(1, "Corpo").Y < pdf.Achar(2, "Corpo").Y);
    }

    [Fact]
    public void AMarcaDAguaSaiEmTodasAsPaginasEmDiagonalEEmCinzaClaro()
    {
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato, [Texto("Um"), new QuebraPaginaImpressao(), Texto("Dois")])
        {
            MarcaAgua = "CANCELADA",
        };
        using var pdf = Ler(documento);

        for (var pagina = 1; pagina <= 2; pagina++)
        {
            var letras = pdf.Letras(pagina).Where(l => l.Tamanho > 20).ToList();
            Assert.Equal("CANCELADA", string.Concat(letras.Select(l => l.Valor)));
            // Em diagonal: as letras sobem da esquerda para a direita (o Y de cima para baixo diminui) em 45 graus.
            Assert.True(letras[^1].Y < letras[0].Y);
            Assert.Equal(letras[^1].X - letras[0].X, letras[0].Y - letras[^1].Y, 1.0);
            // Cinza claro e atrás do texto: a letra é desenhada antes do texto do corpo (a ordem do fluxo).
            var cor = pdf.Pagina(pagina).Letters.First(l => l.PointSize > 20).Color.ToRGBValues();
            Assert.InRange(cor.r, 0.85, 0.95);
            Assert.Equal(cor.r, cor.g, 0.01);
            Assert.Equal("C", pdf.Pagina(pagina).Letters[0].Value);
            // Centrada na página (a diagonal passa pelo centro).
            var centroX = (letras[0].X + letras[^1].FimX) / 2;
            var centroY = (letras[0].Y + letras[^1].Y) / 2;
            Assert.Equal(595.28 / 2, centroX, 40.0);
            Assert.Equal(841.89 / 2, centroY, 40.0);
        }
    }

    [Theory]
    [InlineData("SEM VALOR FISCAL")]
    [InlineData("CANCELADA")]
    public void AMarcaDAguaDoA4SaiInteiraEmDiagonal(string marca)
    {
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato, [Texto("corpo")]) { MarcaAgua = marca };
        using var pdf = Ler(documento);

        var letras = pdf.Letras(1).Where(l => l.Tamanho > 20).ToList();
        Assert.Equal(marca, string.Concat(letras.Select(l => l.Valor)));
        Assert.True(letras[^1].Y < letras[0].Y && letras[^1].X > letras[0].X);
    }

    [Fact]
    public void AMarcaDAguaLongaEncolheParaCaberNaLarguraDoPapel()
    {
        // Um bilhete comprido: a marca d'água gira no centro dele e a metade de cima e a de baixo ficam dentro da página.
        var documento = new DocumentoImpressao(PapelImpressao.Bobina58, [.. Enumerable.Range(1, 60).Select(i => Texto($"linha {i}"))]) { MarcaAgua = "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO" };
        using var pdf = Ler(documento);

        var grande = pdf.Letras(1).Where(l => Math.Abs(l.Tamanho - 8) > 0.05).ToList();
        Assert.Equal("EMITIDAEMAMBIENTEDEHOMOLOGAÇÃO", string.Concat(grande.Where(l => l.Valor != " ").Select(l => l.Valor)));
        Assert.True(grande[0].Tamanho < Medidas.MarcaTamanhoMaximo);
    }

    [Fact]
    public void AsFichasDePaginaTrocamParaONumeroDaPaginaEOTotalEmCadaPaginaInclusiveNoCabecalho()
    {
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato,
            [Texto("Folha {pagina}/{paginas}"), new QuebraPaginaImpressao(), Texto("Corpo dois"), new QuebraPaginaImpressao(), Texto("Corpo tres")])
        {
            CabecalhoContinuacao = [Texto("FOLHA {pagina}/{paginas}")],
        };
        using var pdf = Ler(documento);

        Assert.Equal(3, pdf.Paginas);
        Assert.Contains("Folha1/3", pdf.TextoCompacto(1), StringComparison.Ordinal);
        Assert.DoesNotContain("{pagina}", pdf.TextoCompacto(), StringComparison.Ordinal);
        Assert.DoesNotContain("{paginas}", pdf.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains("FOLHA2/3", pdf.TextoCompacto(2), StringComparison.Ordinal);
        Assert.Contains("FOLHA3/3", pdf.TextoCompacto(3), StringComparison.Ordinal);
    }

    [Fact]
    public void AsFichasValemNoCampoDaSecaoEEmCelulaDeLinha()
    {
        var secao = Secao(null, new LinhaCamposImpressao([new CampoImpressao("FOLHA", "{pagina}/{paginas}")]));
        var linha = new LinhaImpressao([new CelulaImpressao("pag {pagina} de {paginas}")]);
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, secao, linha));

        Assert.Contains("1/1", pdf.TextoCompacto(1), StringComparison.Ordinal);
        Assert.Contains("pag1de1", pdf.TextoCompacto(1), StringComparison.Ordinal);
    }

    // ---- Papel -----------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(80, 226.77)]
    [InlineData(58, 164.41)]
    public void ABobinaTemALarguraDoPapelEAlturaQueCresceComOConteudo(int milimetros, double pontos)
    {
        var papel = milimetros == 80 ? PapelImpressao.Bobina80 : PapelImpressao.Bobina58;
        using var curto = Ler(Doc(papel, Texto("uma linha")));
        using var longo = Ler(Doc(papel, [.. Enumerable.Range(1, 30).Select(i => Texto($"linha {i}"))]));

        Assert.Equal(pontos, curto.Pagina(1).Width, 0.5);
        Assert.Equal(pontos, longo.Pagina(1).Width, 0.5);
        Assert.True(longo.Pagina(1).Height > curto.Pagina(1).Height * 5, "a altura cresce com o conteúdo (altura contínua)");
        // A margem de 3 mm no alto e embaixo e as 30 linhas de 9,2 pt.
        Assert.Equal(2 * 3 * MmPt + 30 * 9.2, longo.Pagina(1).Height, 1.0);
        Assert.Equal(2 * 3 * MmPt + 9.2, curto.Pagina(1).Height, 1.0);
    }

    [Fact]
    public void OA4TemAsDimensoesDoPapelNoRetratoENaPaisagem()
    {
        using var retrato = Ler(Doc(PapelImpressao.A4Retrato, Texto("a")));
        using var paisagem = Ler(Doc(PapelImpressao.A4Paisagem, Texto("a")));

        Assert.Equal(595.28, retrato.Pagina(1).Width, 0.5);
        Assert.Equal(841.89, retrato.Pagina(1).Height, 0.5);
        Assert.Equal(841.89, paisagem.Pagina(1).Width, 0.5);
        Assert.Equal(595.28, paisagem.Pagina(1).Height, 0.5);
    }

    [Fact]
    public void AFolhaTemAMargemDoPapelEABobinaAsLateraisDoPapel()
    {
        using var a4 = Ler(Doc(PapelImpressao.A4Retrato, Texto("margem")));
        using var bobina = Ler(Doc(PapelImpressao.Bobina58, Texto("margem")));

        Assert.Equal(8 * MmPt, a4.Achar(1, "margem").X, 0.5);
        Assert.Equal(5 * MmPt, bobina.Achar(1, "margem").X, 0.5);
        // A linha de base do primeiro texto fica abaixo da margem superior (8 mm no A4, 3 mm na bobina) e acima de margem + a linha.
        Assert.InRange(a4.Achar(1, "margem").Y, 8 * MmPt, 8 * MmPt + 9.2);
        Assert.InRange(bobina.Achar(1, "margem").Y, 3 * MmPt, 3 * MmPt + 9.2);
    }

    [Fact]
    public void OPdfSemElementosTemUmaPaginaEmBranco()
    {
        using var folha = Ler(Doc(PapelImpressao.A4Retrato));
        using var bobina = Ler(Doc(PapelImpressao.Bobina80));

        Assert.Equal(1, folha.Paginas);
        Assert.Equal(1, bobina.Paginas);
    }

    // ---- Metadados, fluxo e opções -----------------------------------------------------------------------------------

    [Fact]
    public void OsMetadadosDoPdfSaoOTituloDoDocumentoOAutorEOCriador()
    {
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato, [Texto("x")]) { Titulo = "DANFE NF-e 000000123 série 001" };

        using var padrao = Ler(documento);
        using var opcoes = Ler(documento, new OpcoesPdf { Titulo = "Outro título", Autor = "Emitente Ltda", Criador = "ERP Core" });

        Assert.Equal("DANFE NF-e 000000123 série 001", padrao.Informacao.Title);
        Assert.Equal("OpenDFe", padrao.Informacao.Creator);
        Assert.Equal("Outro título", opcoes.Informacao.Title);
        Assert.Equal("Emitente Ltda", opcoes.Informacao.Author);
        Assert.Equal("ERP Core", opcoes.Informacao.Creator);
    }

    [Fact]
    public void OFluxoRecebeOMesmoPdfDaVersaoEmMemoriaSemSerFechado()
    {
        var documento = Doc(PapelImpressao.Bobina80, Texto("Fluxo"), new QrCodeImpressao("teste"));
        using var fluxo = new MemoryStream();

        RenderizadorPdf.Gerar(documento, fluxo);

        Assert.True(fluxo.CanWrite);
        using var lido = new PdfLeitor(fluxo.ToArray());
        Assert.Contains("Fluxo", lido.TextoCompacto(1), StringComparison.Ordinal);
        Assert.InRange(fluxo.Length, RenderizadorPdf.Gerar(documento).Length - 64, RenderizadorPdf.Gerar(documento).Length + 64);
    }

    [Fact]
    public void OsArgumentosNulosSaoRecusados()
    {
        Assert.Throws<ArgumentNullException>(() => RenderizadorPdf.Gerar(null!));
        Assert.Throws<ArgumentNullException>(() => RenderizadorPdf.Gerar(null!, new MemoryStream()));
        Assert.Throws<ArgumentNullException>(() => RenderizadorPdf.Gerar(Doc(PapelImpressao.A4Retrato), (Stream)null!));
    }

    [Fact]
    public void AFonteDasOpcoesEAFamiliaPedidaComALatoDeReserva()
    {
        var documento = Doc(PapelImpressao.A4Retrato, Texto("Reserva"));

        using var pdf = Ler(documento, new OpcoesPdf { Fonte = "Lato" });

        Assert.Contains("Lato", pdf.Achar(1, "Reserva").Fonte, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComUmaFamiliaQueNaoEstaRegistradaAFonteDeReservaDesenhaOTexto()
    {
        // O aplicativo pede "Times New Roman" (o do Anexo II) e a máquina não a registrou: a Lato, a embutida, desenha.
        using var pdf = Ler(Doc(PapelImpressao.A4Retrato, Texto("Reserva ação")), new OpcoesPdf { Fonte = "Familia Que Nao Existe" });

        Assert.Contains("Reservaação", pdf.TextoCompacto(1), StringComparison.Ordinal);
    }

    [Fact]
    public void OPdfEDeterministicoNoTextoEntreGeracoesDoMesmoDocumento()
    {
        var documento = Doc(PapelImpressao.Bobina80, Texto("Igual"), new LinhaImpressao([new CelulaImpressao("a"), new CelulaImpressao("b")]), new QrCodeImpressao("teste"));

        using var um = Ler(documento);
        using var dois = Ler(documento);

        Assert.Equal(um.TextoCompacto(1), dois.TextoCompacto(1));
        Assert.Equal(um.Pagina(1).Height, dois.Pagina(1).Height, 0.001);
    }

    [Fact]
    public async Task DezesseisGeracoesSimultaneasDeDocumentosDiferentesDaoOMesmoResultadoDaSequencial()
    {
        var documentos = Enumerable.Range(1, 16).Select(i => i % 2 == 0
            ? Doc(PapelImpressao.Bobina80, Texto($"Bilhete {i}"), new CodigoBarrasImpressao(Chave), new QrCodeImpressao($"https://exemplo.gov.br/qr?p={i}"))
            : Doc(PapelImpressao.A4Retrato, Texto($"Folha {i}"), Tabela(80 + i))).ToArray();
        var sequencial = documentos.Select(d => { using var p = Ler(d); return (p.Paginas, p.TextoCompacto()); }).ToArray();

        var paralelo = await Task.WhenAll(documentos.Select(d => Task.Run(() => RenderizadorPdf.Gerar(d))));

        for (var i = 0; i < documentos.Length; i++)
        {
            using var lido = new PdfLeitor(paralelo[i]);
            Assert.Equal(sequencial[i].Paginas, lido.Paginas);
            Assert.Equal(sequencial[i].Item2, lido.TextoCompacto());
        }
    }

    [Fact]
    public void ADocumentosDePapeisDiferentesNaoDividemEstado()
    {
        // A margem e o papel de um documento não vazam para o seguinte (nenhuma configuração global tocada).
        using var a4 = Ler(Doc(PapelImpressao.A4Retrato, Texto("a")));
        using var bobina = Ler(Doc(PapelImpressao.Bobina58, Texto("a")));
        using var a4De = Ler(Doc(PapelImpressao.A4Retrato, Texto("a")));

        Assert.Equal(a4.Pagina(1).Width, a4De.Pagina(1).Width, 0.001);
        Assert.NotEqual(a4.Pagina(1).Width, bobina.Pagina(1).Width);
    }
}

/// <summary>A licença do QuestPDF no caminho do <see cref="RenderizadorPdf.Gerar(DocumentoImpressao, OpcoesPdf?)"/>: mexe no estado global do QuestPDF, então roda sozinha.</summary>
[Collection(nameof(LicencaGlobal))]
public class RenderizadorPdfGerarLicencaTestes
{
    [Fact]
    public void SemLicencaDefinidaGerarLancaComOEnderecoDoReadmeNasDuasSobrecargas()
    {
        var anterior = QuestPDF.Settings.License;
        var documento = new DocumentoImpressao(PapelImpressao.Bobina80, [new TextoImpressao("x")]);
        try
        {
            QuestPDF.Settings.License = null;
            var erroMemoria = Assert.Throws<InvalidOperationException>(() => RenderizadorPdf.Gerar(documento));
            var erroFluxo = Assert.Throws<InvalidOperationException>(() => RenderizadorPdf.Gerar(documento, new MemoryStream()));
            Assert.Contains("https://github.com/Davidsonrf1/OpenDFe/blob/main/src/OpenDFe.Impressao.Pdf/README.md", erroMemoria.Message, StringComparison.Ordinal);
            Assert.Equal(erroMemoria.Message, erroFluxo.Message);
        }
        finally
        {
            QuestPDF.Settings.License = anterior;
        }
    }
}
