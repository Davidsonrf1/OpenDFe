using System.Reflection;
using OpenDFe.Impressao;

namespace OpenDFe.Testes;

/// <summary>
/// O modelo de impressão neutro (etapa 7.2, docs/CONTRATOS.md 20): o papel, o documento, os elementos, as recusas dos
/// construtores, a imutabilidade e o conjunto fechado de elementos.
/// </summary>
public sealed class ModeloImpressaoTestes
{
    [Fact]
    public void OPapelDaBobinaTemAsColunasDaFonteA()
    {
        Assert.Equal((TipoPapel.Bobina, 80d, 48), (PapelImpressao.Bobina80.Tipo, PapelImpressao.Bobina80.LarguraMm, PapelImpressao.Bobina80.Colunas));
        Assert.Equal((TipoPapel.Bobina, 58d, 32), (PapelImpressao.Bobina58.Tipo, PapelImpressao.Bobina58.LarguraMm, PapelImpressao.Bobina58.Colunas));
        Assert.Null(PapelImpressao.Bobina80.AlturaMm);
        Assert.Null(PapelImpressao.Bobina58.AlturaMm);
        foreach (var papel in new[] { PapelImpressao.Bobina80, PapelImpressao.Bobina58 })
        {
            // 12 pontos de largura a 203 dpi (8 pontos por mm): as colunas cabem na área impressa.
            Assert.True(papel.Colunas * 12 / 8d <= papel.LarguraMm - 2 * papel.MargemMm);
            // O manual do DANFE NFC-e (3.3): margem lateral de ao menos 2 mm.
            Assert.True(papel.MargemMm >= 2);
        }
    }

    [Fact]
    public void OPapelDaFolhaTemAlturaESemColunas()
    {
        Assert.Equal((TipoPapel.Folha, 210d, 297d, 0), (PapelImpressao.A4Retrato.Tipo, PapelImpressao.A4Retrato.LarguraMm, PapelImpressao.A4Retrato.AlturaMm, PapelImpressao.A4Retrato.Colunas));
        Assert.Equal((TipoPapel.Folha, 297d, 210d, 0), (PapelImpressao.A4Paisagem.Tipo, PapelImpressao.A4Paisagem.LarguraMm, PapelImpressao.A4Paisagem.AlturaMm, PapelImpressao.A4Paisagem.Colunas));
    }

    [Fact]
    public void AsColunasDeCadaTamanhoSaoAsDaFonte()
    {
        Assert.Equal((64, 48, 24), (PapelImpressao.Bobina80.ColunasTamanho(TamanhoTexto.Pequeno), PapelImpressao.Bobina80.ColunasTamanho(TamanhoTexto.Normal), PapelImpressao.Bobina80.ColunasTamanho(TamanhoTexto.Duplo)));
        Assert.Equal((42, 32, 16), (PapelImpressao.Bobina58.ColunasTamanho(TamanhoTexto.Pequeno), PapelImpressao.Bobina58.ColunasTamanho(TamanhoTexto.Normal), PapelImpressao.Bobina58.ColunasTamanho(TamanhoTexto.Duplo)));
        Assert.Equal(0, PapelImpressao.A4Retrato.ColunasTamanho(TamanhoTexto.Normal));
        Assert.Throws<ArgumentOutOfRangeException>(() => PapelImpressao.Bobina80.ColunasTamanho((TamanhoTexto)9));
    }

    [Fact]
    public void ODocumentoRecusaArgumentosNulos()
    {
        Assert.Throws<ArgumentNullException>(() => new DocumentoImpressao(null!, []));
        Assert.Throws<ArgumentNullException>(() => new DocumentoImpressao(PapelImpressao.Bobina80, null!));
        Assert.Throws<ArgumentNullException>(() => new DocumentoImpressao(PapelImpressao.Bobina80, [new TextoImpressao("a"), null!]));
    }

    [Fact]
    public void ODocumentoCopiaAListaDeElementos()
    {
        var lista = new List<ElementoImpressao> { new TextoImpressao("a"), new SeparadorImpressao() };
        var documento = new DocumentoImpressao(PapelImpressao.Bobina80, lista) { Titulo = "Título" };

        lista.Add(new TextoImpressao("b"));
        lista[0] = new TextoImpressao("trocado");

        Assert.Equal(2, documento.Elementos.Count);
        Assert.Equal("a", Assert.IsType<TextoImpressao>(documento.Elementos[0]).Texto);
        Assert.Equal("Título", documento.Titulo);
        Assert.Same(PapelImpressao.Bobina80, documento.Papel);
        Assert.Null(new DocumentoImpressao(PapelImpressao.A4Retrato, []).Titulo);
    }

    [Fact]
    public void OTextoTemOsPadroesECompilaOsDestaques()
    {
        var padrao = new TextoImpressao("x");
        Assert.Equal((AlinhamentoImpressao.Esquerda, false, TamanhoTexto.Normal, false), (padrao.Alinhamento, padrao.Negrito, padrao.Tamanho, padrao.Invertido));

        var texto = new TextoImpressao("y") { Alinhamento = AlinhamentoImpressao.Centro, Negrito = true, Tamanho = TamanhoTexto.Duplo, Invertido = true };
        Assert.Equal((AlinhamentoImpressao.Centro, true, TamanhoTexto.Duplo, true), (texto.Alinhamento, texto.Negrito, texto.Tamanho, texto.Invertido));
        Assert.Equal(string.Empty, new TextoImpressao(string.Empty).Texto);
        Assert.Throws<ArgumentNullException>(() => new TextoImpressao(null!));
    }

    [Fact]
    public void ACelulaERecusaPesoInvalido()
    {
        Assert.Equal(1, new CelulaImpressao("a").Peso);
        Assert.Throws<ArgumentNullException>(() => new CelulaImpressao(null!));
        foreach (var peso in new[] { 0d, -1d, double.NaN, double.PositiveInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => new CelulaImpressao("a", peso));
    }

    [Fact]
    public void ALinhaRecusaVaziaENulaECopiaAsCelulas()
    {
        Assert.Throws<ArgumentNullException>(() => new LinhaImpressao(null!));
        Assert.Throws<ArgumentException>(() => new LinhaImpressao([]));
        Assert.Throws<ArgumentNullException>(() => new LinhaImpressao([new CelulaImpressao("a"), null!]));

        var celulas = new List<CelulaImpressao> { new("a"), new("b", 2) };
        var linha = new LinhaImpressao(celulas);
        celulas.Clear();
        Assert.Equal(["a", "b"], linha.Celulas.Select(c => c.Texto));
    }

    [Fact]
    public void OSeparadorEOEspacoTemOsPadroesERecusas()
    {
        Assert.Equal(EstiloSeparador.Tracejado, new SeparadorImpressao().Estilo);
        Assert.Equal(EstiloSeparador.Cheio, new SeparadorImpressao { Estilo = EstiloSeparador.Cheio }.Estilo);
        Assert.Equal(3, new EspacoImpressao(3).Linhas);
        Assert.Throws<ArgumentOutOfRangeException>(() => new EspacoImpressao(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EspacoImpressao(-2));
    }

    [Fact]
    public void ASecaoAsLinhasDeCamposEATabelaRecusamEntradasInvalidasECopiam()
    {
        Assert.Throws<ArgumentNullException>(() => new CampoImpressao(null!, "v"));
        Assert.Throws<ArgumentNullException>(() => new CampoImpressao("r", null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CampoImpressao("r", "v", 0));
        Assert.Throws<ArgumentException>(() => new LinhaCamposImpressao([]));
        Assert.Throws<ArgumentNullException>(() => new LinhaCamposImpressao(null!));
        Assert.Throws<ArgumentException>(() => new SecaoImpressao("t", []));
        Assert.Throws<ArgumentNullException>(() => new SecaoImpressao("t", null!));

        var campos = new List<CampoImpressao> { new("Rótulo", "Valor", 2) { Negrito = true, Alinhamento = AlinhamentoImpressao.Direita } };
        var linhas = new List<LinhaCamposImpressao> { new(campos) };
        var secao = new SecaoImpressao(null, linhas);
        campos.Clear();
        linhas.Clear();

        Assert.Null(secao.Titulo);
        var campo = Assert.Single(Assert.Single(secao.Linhas).Campos);
        Assert.Equal(("Rótulo", "Valor", 2d, true, AlinhamentoImpressao.Direita), (campo.Rotulo, campo.Valor, campo.Peso, campo.Negrito, campo.Alinhamento));
    }

    [Fact]
    public void ATabelaConferePorColunaECopiaAsListas()
    {
        var colunas = new List<ColunaImpressao> { new("A", 1), new("B", 2, AlinhamentoImpressao.Direita) };
        Assert.Throws<ArgumentException>(() => new TabelaImpressao("t", [], []));
        Assert.Throws<ArgumentNullException>(() => new TabelaImpressao("t", null!, []));
        Assert.Throws<ArgumentNullException>(() => new TabelaImpressao("t", colunas, null!));
        Assert.Throws<ArgumentException>(() => new TabelaImpressao("t", colunas, [["1"]]));
        Assert.Throws<ArgumentNullException>(() => new TabelaImpressao("t", colunas, [["1", null!]]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ColunaImpressao("A", 0));

        var linha = new List<string> { "1", "2" };
        var linhas = new List<IReadOnlyList<string>> { linha };
        var tabela = new TabelaImpressao(null, colunas, linhas);
        linha[0] = "trocado";
        linhas.Clear();
        colunas.Clear();

        Assert.Equal(2, tabela.Colunas.Count);
        Assert.Equal(["1", "2"], Assert.Single(tabela.Linhas));
        Assert.Equal(AlinhamentoImpressao.Direita, tabela.Colunas[1].Alinhamento);
        Assert.Empty(new TabelaImpressao("vazia", [new ColunaImpressao("A", 1)], []).Linhas);
    }

    [Fact]
    public void AImagemImpressaoTemOsPadroesERecusas()
    {
        var imagem = new Imagem1Bit(1, 1, new byte[] { 0x80 });
        var elemento = new ImagemImpressao(imagem);
        Assert.Same(imagem, elemento.Imagem);
        Assert.Equal(AlinhamentoImpressao.Centro, elemento.Alinhamento);
        Assert.Null(elemento.LarguraMm);
        Assert.Equal(20, new ImagemImpressao(imagem) { LarguraMm = 20 }.LarguraMm);
        Assert.Throws<ArgumentNullException>(() => new ImagemImpressao(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ImagemImpressao(imagem) { LarguraMm = 0 });
    }

    [Fact]
    public void OConjuntoDeElementosEFechado()
    {
        var tipo = typeof(ElementoImpressao);
        var construtores = tipo.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var construtor = Assert.Single(construtores);
        // private protected: só quem está no assembly e deriva enxerga.
        Assert.True(construtor.IsFamilyAndAssembly);
        Assert.True(tipo.IsAbstract);

        // Todos os derivados são da biblioteca, selados.
        var derivados = tipo.Assembly.GetTypes().Where(t => t.IsSubclassOf(tipo)).ToArray();
        string[] esperados =
            ["EspacoImpressao", "ImagemImpressao", "CodigoBarrasImpressao", "LinhaImpressao", "QrCodeImpressao", "QuebraPaginaImpressao", "SecaoImpressao", "SeparadorImpressao", "TabelaImpressao", "TextoImpressao"];
        Assert.Equal(esperados.Order(StringComparer.Ordinal), derivados.Select(t => t.Name).Order(StringComparer.Ordinal));
        Assert.All(derivados, t => Assert.True(t.IsSealed && t.IsPublic));
    }

    [Fact]
    public void OQrCodeEOCodigoDeBarrasRecusamEntradasInvalidas()
    {
        Assert.Throws<ArgumentNullException>(() => new QrCodeImpressao(null!));
        Assert.Throws<ArgumentException>(() => new QrCodeImpressao(string.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => new QrCodeImpressao("x") { LadoMm = 0 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new QrCodeImpressao("x") { LadoMm = double.NaN });

        Assert.Throws<ArgumentNullException>(() => new CodigoBarrasImpressao(null!));
        Assert.Throws<ArgumentException>(() => new CodigoBarrasImpressao(string.Empty));
        Assert.Throws<ArgumentException>(() => new CodigoBarrasImpressao("123"));
        Assert.Throws<ArgumentException>(() => new CodigoBarrasImpressao("12a4"));
        Assert.Throws<ArgumentException>(() => new CodigoBarrasImpressao("١٢"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CodigoBarrasImpressao("1234") { AlturaMm = -1 });
    }
}
