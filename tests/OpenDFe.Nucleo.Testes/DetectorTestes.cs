namespace OpenDFe.Testes;

// O detector (PROJETO.md 3.3, docs/LEITURA.md 2.1) com o leiaute falso, sem a NF-e: a forma declarada, a raiz, a versão
// e o modelo; e as recusas da FormaLeitura e da declaração.
public class DetectorTestes
{
    private static readonly FormaLeitura Eco = new(LeiauteFalso.Namespace, "eco", "@versao", "doc/@modelo");

    private static LeiauteFalso LeiauteLeitura() => new() { Acrescimo = d => d.DeclararLeitura(Eco) };

    private static string Xml(string versao = "1.00", string modelo = "65", string raiz = "eco", string ns = LeiauteFalso.Namespace) =>
        $"<{raiz} xmlns=\"{ns}\" versao=\"{versao}\"><doc modelo=\"{modelo}\" /></{raiz}>";

    [Theory]
    [InlineData("55", Documento.NFe)]
    [InlineData("65", Documento.NFCe)]
    public void AchaOLeiauteEODocumentoPeloModelo(string modelo, Documento documento)
    {
        var leiaute = LeiauteLeitura();

        var deteccao = DFe.Detectar(Xml(modelo: modelo), leiaute);

        Assert.NotNull(deteccao);
        Assert.Same(leiaute, deteccao.Leiaute);
        Assert.Equal(documento, deteccao.Documento);
        Assert.Equal("eco", deteccao.Raiz);
        Assert.Equal("1.00", deteccao.Versao);
    }

    [Fact]
    public void AceitaODocumentoComDeclaracaoBomEComentarios()
    {
        var xml = "﻿<?xml version=\"1.0\" encoding=\"UTF-8\"?><!-- recebido --><eco xmlns=\"" + LeiauteFalso.Namespace
            + "\" versao=\"1.00\">\n  <doc modelo=\"55\" />\n</eco>";

        Assert.Equal(Documento.NFe, DFe.Detectar(xml, LeiauteLeitura())?.Documento);
    }

    [Theory]
    [InlineData("2.00", "65", "eco", LeiauteFalso.Namespace)]     // outra versão
    [InlineData("1.00", "57", "eco", LeiauteFalso.Namespace)]     // modelo que o leiaute não atende
    [InlineData("1.00", "65", "outra", LeiauteFalso.Namespace)]   // outra raiz
    [InlineData("1.00", "65", "eco", "urn:outro")]                // outro namespace
    public void DevolveNuloQuandoNenhumLeiauteLe(string versao, string modelo, string raiz, string ns)
    {
        Assert.Null(DFe.Detectar(Xml(versao, modelo, raiz, ns), LeiauteLeitura()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("não é XML")]
    [InlineData("<eco xmlns=\"urn:opendfe:teste:eco\" versao=\"1.00\"><doc modelo=\"65\"></eco>")]
    [InlineData("<!DOCTYPE eco [<!ENTITY x \"y\">]><eco xmlns=\"urn:opendfe:teste:eco\" versao=\"1.00\"><doc modelo=\"65\" /></eco>")]
    public void DevolveNuloNoXmlMalFormadoOuComDtd(string xml)
    {
        Assert.Null(DFe.Detectar(xml, LeiauteLeitura()));
    }

    [Fact]
    public void SemFormaDeclaradaNaoLe()
    {
        Assert.Null(DFe.Detectar(Xml(), new LeiauteFalso()));
    }

    [Fact]
    public void OPrimeiroLeiauteQueLeVale()
    {
        var primeiro = LeiauteLeitura();
        var segundo = LeiauteLeitura();

        Assert.Same(primeiro, DFe.Detectar(Xml(), new LeiauteFalso(), primeiro, segundo)?.Leiaute);
        Assert.Same(segundo, DFe.Detectar(Xml(), [segundo, primeiro])?.Leiaute);
    }

    [Fact]
    public void OModeloPodeSerOTextoDoElemento()
    {
        var leiaute = new LeiauteFalso { Acrescimo = d => d.DeclararLeitura(new FormaLeitura(LeiauteFalso.Namespace, "eco", "@versao", "ide/mod")) };
        var xml = $"<eco xmlns=\"{LeiauteFalso.Namespace}\" versao=\"1.00\"><ide><mod>55</mod></ide></eco>";

        Assert.Equal(Documento.NFe, DFe.Detectar(xml, leiaute)?.Documento);
    }

    // A forma do documento sem modelo no XML (a NFS-e, docs/NFSE.md D7): o documento é o da forma.
    [Fact]
    public void AFormaComODocumentoFixoAchaODocumentoSemModeloNoXml()
    {
        var forma = new FormaLeitura(LeiauteFalso.Namespace, "eco", "@versao", Documento.NFSe);
        var leiaute = new LeiauteFalso { Documentos = [Documento.NFSe], Acrescimo = d => d.DeclararLeitura(forma) };

        var deteccao = DFe.Detectar($"<eco xmlns=\"{LeiauteFalso.Namespace}\" versao=\"1.00\" />", leiaute);

        Assert.Equal((Documento.NFSe, "eco", "1.00"), (deteccao?.Documento, deteccao?.Raiz, deteccao?.Versao));
        Assert.Null(forma.CaminhoModelo);
        Assert.Equal(Documento.NFSe, forma.Documento);
        Assert.Null(Eco.Documento);
    }

    [Fact]
    public void AFormaComODocumentoFixoSoValeParaAqueleDocumento()
    {
        var leiaute = new LeiauteFalso { Acrescimo = d => d.DeclararLeitura(new FormaLeitura(LeiauteFalso.Namespace, "eco", "@versao", Documento.NFCe)) };

        Assert.Equal(Documento.NFCe, DFe.Detectar($"<eco xmlns=\"{LeiauteFalso.Namespace}\" versao=\"1.00\" />", leiaute)?.Documento);
    }

    // As outras versões que o leiaute lê (o 1.01 da NFS-e lê a 1.00, D8): a detecção devolve a versão do XML.
    [Theory]
    [InlineData("1.00", true)]
    [InlineData("0.90", true)]
    [InlineData("0.80", false)]
    public void AsOutrasVersoesDaFormaSaoDetectadas(string versao, bool detecta)
    {
        var forma = new FormaLeitura(LeiauteFalso.Namespace, "eco", "@versao", "doc/@modelo") { OutrasVersoes = ["0.90"] };
        var leiaute = new LeiauteFalso { Acrescimo = d => d.DeclararLeitura(forma) };

        var deteccao = DFe.Detectar(Xml(versao: versao), leiaute);

        Assert.Equal(detecta ? versao : null, deteccao?.Versao);
        Assert.Empty(Eco.OutrasVersoes);
    }

    [Theory]
    [InlineData("", "eco", "@versao")]
    [InlineData(LeiauteFalso.Namespace, " ", "@versao")]
    [InlineData(LeiauteFalso.Namespace, "eco", "versao")]
    [InlineData(LeiauteFalso.Namespace, "eco", "@versao/doc")]
    public void AFormaComODocumentoFixoRecusaCaminhoInvalido(string espacoNomes, string raiz, string versao)
    {
        Assert.Throws<ArgumentException>(() => new FormaLeitura(espacoNomes, raiz, versao, Documento.NFSe));
    }

    [Fact]
    public void RecusaOsArgumentosNulos()
    {
        Assert.Throws<ArgumentNullException>(() => DFe.Detectar(null!, new LeiauteFalso()));
        Assert.Throws<ArgumentNullException>(() => DFe.Detectar(Xml(), (IEnumerable<ILeiaute>)null!));
        Assert.Throws<ArgumentNullException>(() => DFe.Detectar(Xml(), new LeiauteFalso(), null!));
    }

    [Theory]
    [InlineData("", "eco", "@versao", "doc/@modelo")]
    [InlineData(LeiauteFalso.Namespace, " ", "@versao", "doc/@modelo")]
    [InlineData(LeiauteFalso.Namespace, "eco", "versao", "doc/@modelo")]       // a versão sem atributo no fim
    [InlineData(LeiauteFalso.Namespace, "eco", "@versao/doc", "doc/@modelo")]  // atributo antes do fim
    [InlineData(LeiauteFalso.Namespace, "eco", "doc//@versao", "doc/@modelo")] // passo vazio
    [InlineData(LeiauteFalso.Namespace, "eco", "@versao", "doc/@")]            // atributo sem nome
    [InlineData(LeiauteFalso.Namespace, "eco", "@versao", "")]
    public void AFormaRecusaCaminhoInvalido(string espacoNomes, string raiz, string versao, string modelo)
    {
        Assert.Throws<ArgumentException>(() => new FormaLeitura(espacoNomes, raiz, versao, modelo));
    }

    [Fact]
    public void ADeclaracaoListaAsFormasERecusaARaizRepetida()
    {
        Assert.Equal([Eco], DeclaracaoLeiaute.Criar(LeiauteLeitura(), Documento.NFe).Leituras);

        var repetida = new LeiauteFalso
        {
            Acrescimo = d =>
            {
                d.DeclararLeitura(Eco);
                d.DeclararLeitura(new FormaLeitura(LeiauteFalso.Namespace, "eco", "@outra", "doc/@modelo"));
            },
        };
        Assert.Throws<ArgumentException>(() => DeclaracaoLeiaute.Criar(repetida, Documento.NFe));
        Assert.Throws<ArgumentNullException>(() => DeclaracaoLeiaute.Criar(new LeiauteFalso { Acrescimo = d => d.DeclararLeitura(null!) }, Documento.NFe));
    }

    [Fact]
    public void RecusaADeclaracaoDaFormaDepoisDoDeclarar()
    {
        var leiaute = new LeiauteFalso();
        DeclaracaoLeiaute.Criar(leiaute, Documento.NFe);

        Assert.Throws<InvalidOperationException>(() => leiaute.Ultima!.DeclararLeitura(Eco));
    }
}
