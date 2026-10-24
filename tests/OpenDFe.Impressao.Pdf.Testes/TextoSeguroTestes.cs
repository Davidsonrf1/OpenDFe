using System.Text;

namespace OpenDFe.Impressao.Pdf.Testes;

/// <summary>
/// Os caracteres do PDF (etapa 7.4, docs/CONTRATOS.md 22.4): a emissão nunca pode quebrar por causa de um caractere no nome do
/// produto. O conjunto que a Lato desenha é provado desenhando cada caractere; o que ela não tem é trocado.
/// </summary>
public class TextoSeguroTestes
{
    private static byte[] Gerar(string texto, bool negrito = false, PapelImpressao? papel = null) =>
        RenderizadorPdf.Gerar(new DocumentoImpressao(papel ?? PapelImpressao.A4Retrato, [new TextoImpressao(texto) { Negrito = negrito }]));

    // O que uma nota em português pode trazer, mais as aspas e os traços tipográficos e os caracteres do XML.
    private const string Portugues = "ç ã õ é ê í ó ú â ô à º ª § ½ ² ³ – — “ ” ‘ ’ € ° ' \" & < > ÇÃÕÉÊÍÓÚÂÔÀ ü Ü ñ ¿ ¡ © ® £ ¥ ¢ · • … ‰ ™ ±× ÷";

    [Fact]
    public void OsCaracteresDoPortuguesSaemNoPdfSemTrocaENaoQuebramAEmissao()
    {
        var pdf = Gerar(Portugues);

        using var leitor = new PdfLeitor(pdf);
        var texto = leitor.TextoCompacto(1);
        Assert.Equal(PdfLeitor.Compactar(Portugues), texto);
        Assert.Equal(Portugues, TextoSeguro.Limpar(Portugues));
    }

    [Fact]
    public void OsCaracteresDoPortuguesSaemTambemEmNegritoNaBobina()
    {
        var pdf = Gerar(Portugues, negrito: true, papel: PapelImpressao.Bobina80);

        using var leitor = new PdfLeitor(pdf);
        Assert.Equal(PdfLeitor.Compactar(Portugues), leitor.TextoCompacto(1));
    }

    [Fact]
    public void TodoCaractereQueOConjuntoAceitaEDesenhadoPelaFonteSemLancar()
    {
        var aceitos = Enumerable.Range(0, 0x10000).Where(c => TextoSeguro.Aceito((char)c)).Select(c => ((char)c).ToString()).ToList();

        // Os 553 caracteres, em lotes: o PDF só sai se a fonte tem o glifo de todos (o QuestPDF lança no que falta).
        foreach (var lote in aceitos.Chunk(120))
        {
            var texto = string.Concat(lote);
            Assert.NotEmpty(Gerar(texto));
            Assert.NotEmpty(Gerar(texto, negrito: true));
        }

        Assert.Contains("ç", aceitos);
        // 95 do ASCII, 432 do Latin-1 ao Latin Extended-B, 19 de pontuação e 7 sinais (o conjunto conferido contra a Lato 2026.9.1).
        Assert.Equal(553, aceitos.Count);
    }

    [Fact]
    public void OConjuntoAceitoTemOLatin1InteiroQueOSchemaDaNFeAdmite()
    {
        // O pattern do TString da NF-e aceita [!-ÿ] e [ -ÿ] (até U+00FF): todos desenháveis, menos os controles C1 (0x80 a 0x9F).
        for (var c = 0x20; c <= 0xFF; c++)
        {
            var esperado = c is not (>= 0x7F and <= 0x9F);
            Assert.Equal(esperado, TextoSeguro.Aceito((char)c));
        }
    }

    [Theory]
    [InlineData("日本語", "???")]
    [InlineData("a😀b", "a?b")]
    [InlineData("αβγ", "???")]
    [InlineData("a\u200Bb", "ab")]
    [InlineData("a\u0007b", "a b")]
    [InlineData("a\u2023b", "a-b")]
    [InlineData("a\u2044b", "a/b")]
    [InlineData("a\u3000b", "a b")]
    [InlineData("um\r\ndois\rtres\nquatro", "um\ndois\ntres\nquatro")]
    [InlineData("a\u2028b", "a\nb")]
    [InlineData("tab\taqui", "tab aqui")]
    [InlineData("", "")]
    public void OQueAFonteNaoTemViraOEquivalenteAsciiOuInterrogacao(string entrada, string esperado) =>
        Assert.Equal(esperado, TextoSeguro.Limpar(entrada));

    [Fact]
    public void UmSurrogadoSoltoViraInterrogacao()
    {
        Assert.Equal("a?b", TextoSeguro.Limpar("a\uD800b"));
        Assert.Equal("a?b", TextoSeguro.Limpar("a\uDC00b"));
    }

    [Fact]
    public void UmCaractereSemGlifoNaoQuebraAEmissao()
    {
        // Ideogramas, emoji, grego, setas, controles, caracteres de formatação invisíveis e um surrogado solto: nada lança.
        var estranhos = new StringBuilder("produto ");
        foreach (var c in new[] { "日", "本", "😀", "α", "→", "\u0001", "\u200B", "\uFEFF", "\uD800", "\u2603", "\uD800", "ﬁ", "\u0085" })
            estranhos.Append(c).Append(' ');

        var pdf = Gerar(estranhos.ToString());
        var pdfBobina = Gerar(estranhos.ToString(), negrito: true, papel: PapelImpressao.Bobina58);

        using var leitor = new PdfLeitor(pdf);
        using var leitorBobina = new PdfLeitor(pdfBobina);
        Assert.StartsWith("produto", leitor.TextoCompacto(1), StringComparison.Ordinal);
        Assert.Contains("?", leitor.TextoCompacto(1), StringComparison.Ordinal);
        Assert.StartsWith("produto", leitorBobina.TextoCompacto(1), StringComparison.Ordinal);
    }

    [Fact]
    public void OsCaracteresEstranhosTambemNaoQuebramOsMetadadosOsCamposEAsCelulas()
    {
        const string estranho = "ação 日本 😀 \u0001";
        var secao = new SecaoImpressao(estranho, [new LinhaCamposImpressao([new CampoImpressao(estranho, estranho)])]);
        var tabela = new TabelaImpressao(estranho, [new ColunaImpressao(estranho, 1)], [[estranho]]);
        var linha = new LinhaImpressao([new CelulaImpressao(estranho)]);
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato, [secao, tabela, linha])
        {
            Titulo = estranho,
            MarcaAgua = estranho,
            CabecalhoContinuacao = [new TextoImpressao(estranho)],
        };

        var pdf = RenderizadorPdf.Gerar(documento, new OpcoesPdf { Autor = estranho, Criador = estranho });

        using var leitor = new PdfLeitor(pdf);
        Assert.Contains("ação", leitor.TextoCompacto(1), StringComparison.Ordinal);
        Assert.Equal("ação ?? ? ", leitor.Informacao.Title?[..10].Replace('\u0001', ' '));
    }

    [Fact]
    public void ONomeDeProdutoComOEnderecoDeUmCaractereNoSchemaDaNFeNuncaQuebra()
    {
        // Todo o intervalo [ -ÿ] do TString da NF-e numa descrição só, que é o pior caso do schema.
        var tudo = new string([.. Enumerable.Range(0x20, 0xE0).Select(c => (char)c)]);

        Assert.NotEmpty(Gerar(tudo));
        Assert.NotEmpty(Gerar(tudo, papel: PapelImpressao.Bobina80));
    }
}
