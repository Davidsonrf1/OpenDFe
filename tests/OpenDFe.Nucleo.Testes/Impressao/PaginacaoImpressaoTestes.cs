using System.Globalization;
using OpenDFe.Impressao;

namespace OpenDFe.Testes;

/// <summary>
/// Os acréscimos da etapa 7.3 ao modelo de impressão neutro (docs/CONTRATOS.md 21): as fichas de paginação, o cabeçalho de
/// continuação, a marca d'água e as opções da tabela (a linha indivisível e o divisor).
/// </summary>
public sealed class PaginacaoImpressaoTestes
{
    [Fact]
    public void AsFichasSaoAsDoContrato()
    {
        Assert.Equal("{pagina}", TextoPaginado.Pagina);
        Assert.Equal("{paginas}", TextoPaginado.Paginas);
    }

    [Theory]
    [InlineData("FOLHA {pagina}/{paginas}", 2, 5, "FOLHA 2/5")]
    [InlineData("FOLHA {pagina}/{paginas}", 1, 1, "FOLHA 1/1")]
    [InlineData("{pagina} de {paginas}, {pagina} de {paginas}", 10, 12, "10 de 12, 10 de 12")]
    [InlineData("{paginas}", 3, 3, "3")]
    [InlineData("sem fichas", 3, 4, "sem fichas")]
    [InlineData("", 1, 1, "")]
    [InlineData("{Pagina} {pagina", 1, 2, "{Pagina} {pagina")]
    public void ResolverTrocaAsFichas(string texto, int pagina, int paginas, string esperado) =>
        Assert.Equal(esperado, TextoPaginado.Resolver(texto, pagina, paginas));

    [Fact]
    public void ResolverNaoDependeDaCulturaDaThread()
    {
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            // O ar-SA e o fa-IR trocam os algarismos quando a cultura entra no ToString.
            foreach (var nome in new[] { "ar-SA", "fa-IR", "pt-BR", "en-US" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(nome);
                Assert.Equal("FOLHA 12/34", TextoPaginado.Resolver("FOLHA {pagina}/{paginas}", 12, 34));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = anterior;
        }
    }

    [Fact]
    public void ResolverRecusaOsNumerosForaDaFaixaEOTextoNulo()
    {
        Assert.Throws<ArgumentNullException>(() => TextoPaginado.Resolver(null!, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextoPaginado.Resolver("x", 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextoPaginado.Resolver("x", -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextoPaginado.Resolver("x", 3, 2));
    }

    [Fact]
    public void ODocumentoSemOsAcrescimosTemOCabecalhoVazioESemMarca()
    {
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato, []);
        Assert.Empty(documento.CabecalhoContinuacao);
        Assert.Null(documento.MarcaAgua);
    }

    [Fact]
    public void OCabecalhoDeContinuacaoECopiadoEImutavel()
    {
        var origem = new List<ElementoImpressao> { new TextoImpressao("DANFE"), new TextoImpressao("FOLHA {pagina}/{paginas}") };
        var documento = new DocumentoImpressao(PapelImpressao.A4Retrato, []) { CabecalhoContinuacao = origem };

        origem.Clear();

        Assert.Equal(2, documento.CabecalhoContinuacao.Count);
        Assert.Equal("DANFE", ((TextoImpressao)documento.CabecalhoContinuacao[0]).Texto);
        Assert.False(documento.CabecalhoContinuacao is List<ElementoImpressao>);
        Assert.Throws<NotSupportedException>(() => ((IList<ElementoImpressao>)documento.CabecalhoContinuacao).Add(new TextoImpressao("x")));
    }

    [Fact]
    public void OCabecalhoDeContinuacaoRecusaListaOuElementoNulo()
    {
        Assert.Throws<ArgumentNullException>(() => new DocumentoImpressao(PapelImpressao.A4Retrato, []) { CabecalhoContinuacao = null! });
        Assert.Throws<ArgumentNullException>(() =>
            new DocumentoImpressao(PapelImpressao.A4Retrato, []) { CabecalhoContinuacao = new ElementoImpressao[] { null! } });
    }

    [Fact]
    public void AMarcaDAguaGuardaOTextoERecusaOVazio()
    {
        var documento = new DocumentoImpressao(PapelImpressao.A4Paisagem, []) { MarcaAgua = "CANCELADA" };
        Assert.Equal("CANCELADA", documento.MarcaAgua);
        Assert.Throws<ArgumentException>(() => new DocumentoImpressao(PapelImpressao.A4Paisagem, []) { MarcaAgua = string.Empty });
        Assert.Throws<ArgumentException>(() => new DocumentoImpressao(PapelImpressao.A4Paisagem, []) { MarcaAgua = "   " });
        Assert.Null(new DocumentoImpressao(PapelImpressao.A4Paisagem, []) { MarcaAgua = null }.MarcaAgua);
    }

    [Fact]
    public void AsOpcoesDaTabelaComecamDesligadasELigamPorInit()
    {
        var colunas = new[] { new ColunaImpressao("A", 1) };
        var linhas = new IReadOnlyList<string>[] { ["1"] };

        var padrao = new TabelaImpressao("T", colunas, linhas);
        Assert.False(padrao.LinhasIndivisiveis);
        Assert.False(padrao.DivisorLinhas);

        var ligada = new TabelaImpressao("T", colunas, linhas) { LinhasIndivisiveis = true, DivisorLinhas = true };
        Assert.True(ligada.LinhasIndivisiveis);
        Assert.True(ligada.DivisorLinhas);
    }

    [Fact]
    public void OCampoEATabelaLevamAsFichasSemMudarOTexto()
    {
        // As fichas são texto comum: só quem renderiza as troca.
        var campo = new CampoImpressao("FOLHA", "{pagina}/{paginas}");
        Assert.Equal("{pagina}/{paginas}", campo.Valor);
        Assert.Equal("1/2", TextoPaginado.Resolver(campo.Valor, 1, 2));
    }

    [Fact]
    public void AMargemDoA4CabeNosLimitesDoAnexoIIDoDanfe()
    {
        // Anexo II 3.6.2: entre o corpo impresso e a borda da folha, de 0,2 a 0,8 cm em cada lado.
        foreach (var papel in new[] { PapelImpressao.A4Retrato, PapelImpressao.A4Paisagem })
        {
            Assert.InRange(papel.MargemMm, 2d, 8d);
            Assert.True(papel.LarguraMm - 2 * papel.MargemMm > 0);
        }
    }
}
