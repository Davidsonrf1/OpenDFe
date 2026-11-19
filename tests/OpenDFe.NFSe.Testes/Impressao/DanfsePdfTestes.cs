using OpenDFe.Impressao;
using OpenDFe.Impressao.Pdf;
using OpenDFe.Impressao.Pdf.Testes;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

/// <summary>
/// O PDF do DANFSe (etapa 9.5, NT 008/2026), lido de volta pelo PdfPig: a página única no A4, o texto dos quadros, as fontes
/// nos tamanhos da NT (nenhuma encolhe), o QR Code na posição e com a matriz da URL da consulta, o vermelho da produção
/// restrita e a marca d'água. Com <c>OPENDFE_AMOSTRAS</c>, grava o PDF de cada caso para a inspeção visual.
/// </summary>
public sealed class DanfsePdfTestes
{
    private const double MmPt = 72 / 25.4;

    public static TheoryData<string> Casos() => [.. CatalogoDanfse.Todos().Select(c => c.Nome)];

    private static (DocumentoImpressao Documento, byte[] Pdf) Gerar(string nome)
    {
        var caso = CatalogoDanfse.Achar(nome);
        var documento = LeiauteNFSe101.Instancia.Danfse(caso.Lida, caso.Opcoes);
        return (documento, RenderizadorPdf.Gerar(documento));
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void ODanfseSaiNumaPaginaA4ComOTextoDosQuadros(string nome)
    {
        var (documento, pdf) = Gerar(nome);
        AmostrasPdf.Gravar("danfse-" + nome, pdf);
        using var leitor = new PdfLeitor(pdf);

        Assert.Equal(1, leitor.Paginas);
        Assert.Equal(595.28, leitor.Pagina(1).Width, 0.5);
        Assert.Equal(841.89, leitor.Pagina(1).Height, 0.5);
        Assert.Equal(documento.Titulo, leitor.Informacao.Title);
        // Todo rótulo e todo valor curto (os longos podem sair cortados pelo PDF, com reticências) estão no PDF.
        var texto = leitor.TextoCompacto(1);
        var formulario = Assert.IsType<FormularioImpressao>(documento.Elementos[0]);
        foreach (var quadro in formulario.Quadros.OfType<TextoFormulario>())
        {
            if (quadro.Rotulo is { } rotulo)
                Assert.Contains(PdfLeitor.Compactar(rotulo), texto, StringComparison.Ordinal);
            if (quadro.Valor is { Length: <= 30 } valor)
                Assert.Contains(PdfLeitor.Compactar(valor), texto, StringComparison.Ordinal);
        }
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void AsFontesTemOsTamanhosDaNtENenhumaEncolhe(string nome)
    {
        var (_, pdf) = Gerar(nome);
        using var leitor = new PdfLeitor(pdf);

        // 6 pt (rótulos, ambiente, texto do QR Code), 7 pt (conteúdo, títulos, rótulos da identificação), 8 pt (município),
        // 9 pt (o título e a frase da produção restrita), e a marca d'água de 50 pt ou mais (NT 008/2026 2.4 e 2.5).
        double[] permitidos = [6, 7, 8, 9];
        Assert.All(leitor.Letras(1), l => Assert.True(
            permitidos.Any(p => Math.Abs(l.Tamanho - p) < 0.05) || l.Tamanho >= 49.5,
            $"A letra \"{l.Valor}\" tem {l.Tamanho} pt."));
    }

    [Fact]
    public void OQrCodeEstaNaPosicaoDaNtComAMatrizDaUrlDaConsulta()
    {
        var (_, pdf) = Gerar("CompletaA");
        using var leitor = new PdfLeitor(pdf);
        var url = "https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=" + CatalogoDanfse.Achar("CompletaA").Lida.Chave!.Value.Valor;

        var esperada = PdfVetores.MatrizQrCode(url);
        var caminho = leitor.Pagina(1).Paths.First(c => c.IsFilled && c.Count > 50);
        var (matriz, caixa, _) = PdfVetores.MatrizQrCodePdf(leitor, 1, esperada.GetLength(0), caminho);
        Assert.Equal(esperada, matriz);
        // X 17,48 cm e Y 1,67 cm da borda do papel, com 1,52 cm de lado (2.4.3).
        Assert.Equal(174.8 * MmPt, caixa.Esquerda, 0.5);
        Assert.Equal(16.7 * MmPt, caixa.Topo, 0.5);
        Assert.Equal(15.2 * MmPt, caixa.Largura, 0.5);
    }

    [Fact]
    public void AFraseDaProducaoRestritaSaiEmVermelhoNoCabecalho()
    {
        var (_, pdf) = Gerar("CompletaA");
        using var leitor = new PdfLeitor(pdf);

        var pagina = leitor.Pagina(1);
        var vermelhas = pagina.Letters.Where(l => l.Color.ToRGBValues() is { r: > 0.99, g: < 0.01, b: < 0.01 }).ToList();
        Assert.Equal(PdfLeitor.Compactar("NFS-e SEM VALIDADE JURÍDICA"), string.Concat(vermelhas.Select(l => l.Value)).Replace(" ", "", StringComparison.Ordinal));
        Assert.All(vermelhas, l => Assert.Equal(9, l.PointSize, 0.05));
        var titulo = leitor.Achar(1, "Documento");
        Assert.True(pagina.Height - vermelhas[0].StartBaseLine.Y > titulo.Y, "A frase fica abaixo do título.");

        var (_, producao) = Gerar("Producao");
        using var outro = new PdfLeitor(producao);
        Assert.DoesNotContain(outro.Pagina(1).Letters, l => l.Color.ToRGBValues() is { r: > 0.99, g: < 0.01 });
    }

    [Theory]
    [InlineData("Cancelada", "CANCELADA")]
    [InlineData("Substituida", "SUBSTITUÍDA")]
    public void ACanceladaEASubstituidaTemAMarcaDAguaDaNt(string caso, string marca)
    {
        var (_, pdf) = Gerar(caso);
        using var leitor = new PdfLeitor(pdf);

        var letras = leitor.Pagina(1).Letters.Where(l => l.PointSize > 20).ToList();
        Assert.Equal(marca, string.Concat(letras.Select(l => l.Value)));
        Assert.All(letras, l => Assert.DoesNotContain("Bold", l.FontName, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0.651, letras[0].Color.ToRGBValues().r, 0.01);
    }

    [Fact]
    public void OCampoDaChaveEOCanhotoEstaoNasPosicoesDaNt()
    {
        var (_, pdf) = Gerar("CompletaA");
        using var leitor = new PdfLeitor(pdf);

        // O rótulo começa no quadro (0,30 cm da borda) mais o respiro de 0,5 mm; o canhoto em Y 28,10 cm.
        var chave = leitor.Achar(1, "CHAVE DE ACESSO");
        Assert.Equal(3.5 * MmPt, chave.X, 0.6);
        Assert.InRange(chave.Y, 14.8 * MmPt, 22.5 * MmPt);
        var canhoto = leitor.Achar(1, "DATA CIENTIFICA");
        Assert.InRange(canhoto.Y, 281 * MmPt, 287.7 * MmPt);
    }
}
