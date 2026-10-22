using System.Globalization;
using System.Xml;
using System.Xml.Schema;

namespace OpenDFe.Testes;

// Os XSDs são de exemplo, escritos para o teste no estilo dos da SEFAZ (Schemas/Exemplo): o núcleo não
// conhece a NF-e. O nota_v1.00.xsd inclui o leiauteNota, que inclui o tiposBasico e importa a assinatura.
public class SchemaXmlTestes
{
    private const string Recurso = "Schemas/Exemplo/nota_v1.00.xsd";
    private const string Ns = "http://www.portalfiscal.inf.br/nfe";

    private static readonly SchemaXml Schema = SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, Recurso, "225");

    private static string Det(int n, string vProd) =>
        $"<det nItem=\"{n}\"><prod><cProd>A{n}</cProd><xProd>Produto {n}</xProd><qCom>1.5000</qCom><vProd>{vProd}</vProd></prod></det>";

    private static string Nota(string? dets = null) =>
        $"<NFe xmlns=\"{Ns}\"><infNFe versao=\"1.00\" Id=\"NFe1234\">"
        + "<ide><cUF>31</cUF><tpAmb>2</tpAmb><nNF>123</nNF><dEmi>2026-10-19</dEmi></ide>"
        + "<emit><CNPJ>11222333000181</CNPJ><xNome>CAFÉ &amp; AÇÚCAR LTDA</xNome></emit>"
        + (dets ?? Det(1, "10.00") + Det(2, "20.00") + Det(3, "30.00"))
        + "<total><vNF>60.00</vNF></total><pag><dinheiro>60.00</dinheiro></pag></infNFe></NFe>";

    private static Problema Unico(string xml)
    {
        var problemas = Schema.Validar(xml);
        return Assert.Single(problemas);
    }

    private static RestricaoSchema UnicaRestricao(Problema problema)
    {
        Assert.NotNull(problema.Schema);
        Assert.Equal(TipoFalhaSchema.Valor, problema.Schema.Tipo);
        return Assert.Single(problema.Schema.Restricoes);
    }

    // Carga

    [Fact]
    public void NotaValidaNaoTemProblema()
    {
        Assert.Empty(Schema.Validar(Nota()));
    }

    [Fact]
    public void NotaValidaComOElementoImportadoDeOutroNamespace()
    {
        var xml = Nota().Replace("</infNFe></NFe>", "</infNFe><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignatureValue>AAEC</SignatureValue></Signature></NFe>", StringComparison.Ordinal);
        Assert.Empty(Schema.Validar(xml));
    }

    [Fact]
    public void ONomeDoRecursoAceitaABarraInvertida()
    {
        var schema = SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, @"Schemas\Exemplo\nota_v1.00.xsd", "225");
        Assert.Empty(schema.Validar(Nota()));
    }

    [Fact]
    public void RecursoInexistenteEhArgumentoInvalido()
    {
        var assembly = typeof(SchemaXmlTestes).Assembly;
        Assert.Throws<ArgumentException>(() => SchemaXml.Carregar(assembly, "Schemas/Exemplo/naoExiste.xsd", "225"));
        Assert.Throws<ArgumentException>(() => SchemaXml.Carregar(assembly, " ", "225"));
        Assert.Throws<ArgumentException>(() => SchemaXml.Carregar(assembly, Recurso, ""));
        Assert.Throws<ArgumentNullException>(() => SchemaXml.Carregar(null!, Recurso, "225"));
        Assert.Throws<ArgumentNullException>(() => Schema.Validar(null!));
    }

    [Theory]
    [InlineData("Schemas/Invalidos/incluiInexistente.xsd")]
    [InlineData("Schemas/Invalidos/importaExterno.xsd")]
    [InlineData("Schemas/Invalidos/tipoIndefinido.xsd")]
    public void SchemaQueNaoCarregaEhExcecao(string recurso)
    {
        Assert.Throws<XmlSchemaException>(() => SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, recurso, "225"));
    }

    [Fact]
    public void DtdNoSchemaEhProibido()
    {
        var assembly = typeof(SchemaXmlTestes).Assembly;
        Assert.ThrowsAny<XmlException>(() => SchemaXml.Carregar(assembly, "Schemas/Invalidos/declaraDtd.xsd", "225"));
        // O XmlSchemaSet lê o incluído com DTD proibido e trata a recusa como include que não resolve.
        var incluido = Assert.Throws<XmlSchemaException>(() => SchemaXml.Carregar(assembly, "Schemas/Invalidos/incluiDtd.xsd", "225"));
        Assert.Contains("DTD", incluido.Message, StringComparison.Ordinal);
    }

    // Facetas

    [Fact]
    public void Padrao()
    {
        var problema = Unico(Nota().Replace("<vProd>30.00</vProd>", "<vProd>12,5</vProd>", StringComparison.Ordinal));

        Assert.Equal("225", problema.Codigo);
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal("vProd", problema.Tag);
        Assert.Equal("NFe/infNFe/det[3]/prod/vProd", problema.Caminho);
        var restricao = UnicaRestricao(problema);
        Assert.Equal(FacetaSchema.Padrao, restricao.Faceta);
        Assert.Equal(@"0|0\.[0-9]{2}|[1-9]{1}[0-9]{0,12}(\.[0-9]{2})?", Assert.Single(restricao.Valores));
        Assert.Equal("TDec_1302", restricao.Tipo);
        Assert.Equal("TDec_1302", problema.Schema!.TipoSchema);
        Assert.Equal("12,5", problema.Schema.Valor);
        Assert.Equal(@"vProd: o valor não segue o padrão do tipo TDec_1302 (0|0\.[0-9]{2}|[1-9]{1}[0-9]{0,12}(\.[0-9]{2})?).", problema.Mensagem);
        Assert.DoesNotContain("12,5", problema.Mensagem, StringComparison.Ordinal);
        Assert.Equal(1, problema.Schema.Linha);
        Assert.NotNull(problema.Schema.Coluna);
        Assert.NotEmpty(problema.Schema.MensagemOriginal);
    }

    [Fact]
    public void Tamanho()
    {
        var problema = Unico(Nota().Replace("<CNPJ>11222333000181</CNPJ>", "<CNPJ>1122233300018</CNPJ>", StringComparison.Ordinal));

        Assert.Equal("CNPJ", problema.Tag);
        Assert.Equal("NFe/infNFe/emit/CNPJ", problema.Caminho);
        var restricao = UnicaRestricao(problema);
        Assert.Equal(FacetaSchema.Tamanho, restricao.Faceta);
        Assert.Equal("14", Assert.Single(restricao.Valores));
        Assert.Equal("CNPJ: o valor tem 13 caracteres, e o tamanho deve ser exatamente 14 do tipo TCnpj.", problema.Mensagem);
    }

    [Fact]
    public void TamanhoMaximoHerdadoDoTipoBaseEPadraoDoTipoBase()
    {
        // TNome restringe TString: o tamanho é do TNome; o padrão (sem espaço nas pontas) é do TString.
        var longo = Unico(Nota().Replace("CAFÉ &amp; AÇÚCAR LTDA", new string('X', 61), StringComparison.Ordinal));
        var tamanho = UnicaRestricao(longo);
        Assert.Equal(FacetaSchema.TamanhoMaximo, tamanho.Faceta);
        Assert.Equal("60", Assert.Single(tamanho.Valores));
        Assert.Equal("TNome", tamanho.Tipo);
        Assert.Equal("xNome: o valor tem 61 caracteres, acima do máximo de 60 do tipo TNome.", longo.Mensagem);

        var espaco = Unico(Nota().Replace("CAFÉ &amp; AÇÚCAR LTDA", " CAFÉ", StringComparison.Ordinal));
        var padrao = UnicaRestricao(espaco);
        Assert.Equal(FacetaSchema.Padrao, padrao.Faceta);
        Assert.Equal("TString", padrao.Tipo);
        Assert.Equal("TNome", espaco.Schema!.TipoSchema);
        Assert.Contains("do tipo TString (herdado por TNome)", espaco.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public void TamanhoMinimo()
    {
        var problema = Unico(Nota().Replace("CAFÉ &amp; AÇÚCAR LTDA", "X", StringComparison.Ordinal));
        var restricao = UnicaRestricao(problema);
        Assert.Equal(FacetaSchema.TamanhoMinimo, restricao.Faceta);
        Assert.Equal("2", Assert.Single(restricao.Valores));
        Assert.Equal("xNome: o valor tem 1 caractere, abaixo do mínimo de 2 do tipo TNome.", problema.Mensagem);
    }

    [Fact]
    public void FacetaDeTipoAnonimoDentroDoElemento()
    {
        var problema = Unico(Nota().Replace("<cProd>A2</cProd>", "<cProd>ABCDEFGHIJK</cProd>", StringComparison.Ordinal));
        Assert.Equal("NFe/infNFe/det[2]/prod/cProd", problema.Caminho);
        var restricao = UnicaRestricao(problema);
        Assert.Equal(FacetaSchema.TamanhoMaximo, restricao.Faceta);
        Assert.Null(restricao.Tipo);
        Assert.Null(problema.Schema!.TipoSchema);
        Assert.Equal("cProd: o valor tem 11 caracteres, acima do máximo de 10.", problema.Mensagem);
    }

    [Fact]
    public void Enumeracao()
    {
        var problema = Unico(Nota().Replace("<cUF>31</cUF>", "<cUF>99</cUF>", StringComparison.Ordinal));
        Assert.Equal("NFe/infNFe/ide/cUF", problema.Caminho);
        var restricao = UnicaRestricao(problema);
        Assert.Equal(FacetaSchema.Enumeracao, restricao.Faceta);
        Assert.Equal(["31", "35", "41"], restricao.Valores);
        Assert.Equal("TCodUfIBGE", restricao.Tipo);
        Assert.Equal("cUF: o valor não está entre os aceitos pelo tipo TCodUfIBGE: 31, 35, 41.", problema.Mensagem);
    }

    [Fact]
    public void TotalDigitosECasasDecimais()
    {
        var casas = Unico(Nota().Replace("<qCom>1.5000</qCom>" + "<vProd>10.00", "<qCom>1.12345</qCom><vProd>10.00", StringComparison.Ordinal));
        Assert.Equal("NFe/infNFe/det[1]/prod/qCom", casas.Caminho);
        var restricao = UnicaRestricao(casas);
        Assert.Equal(FacetaSchema.CasasDecimais, restricao.Faceta);
        Assert.Equal("4", Assert.Single(restricao.Valores));
        Assert.Equal("qCom: o valor tem 5 casas decimais, acima do máximo de 4 do tipo TQtd.", casas.Mensagem);

        var total = Unico(Nota().Replace("<qCom>1.5000</qCom>" + "<vProd>10.00", "<qCom>12345678901234.56</qCom><vProd>10.00", StringComparison.Ordinal));
        var digitos = UnicaRestricao(total);
        Assert.Equal(FacetaSchema.TotalDigitos, digitos.Faceta);
        Assert.Equal("15", Assert.Single(digitos.Valores));
        Assert.Equal("qCom: o valor tem 16 dígitos, acima do total de 15 do tipo TQtd.", total.Mensagem);
    }

    [Fact]
    public void ZerosNaoContamNosDigitos()
    {
        var xml = Nota().Replace("<qCom>1.5000</qCom>" + "<vProd>10.00", "<qCom>000012345678901.1230000</qCom><vProd>10.00", StringComparison.Ordinal);
        Assert.Empty(Schema.Validar(xml));
    }

    [Fact]
    public void MinimoEMaximoInclusivos()
    {
        string NotaDesconto(string valor) =>
            Nota().Replace("<vProd>20.00</vProd>", $"<vProd>20.00</vProd><pDesc>{valor}</pDesc>", StringComparison.Ordinal);

        Assert.Empty(Schema.Validar(NotaDesconto("0")));
        Assert.Empty(Schema.Validar(NotaDesconto("100")));

        var acima = Unico(NotaDesconto("100.01"));
        Assert.Equal("NFe/infNFe/det[2]/prod/pDesc", acima.Caminho);
        var maximo = UnicaRestricao(acima);
        Assert.Equal(FacetaSchema.MaximoInclusivo, maximo.Faceta);
        Assert.Equal("100", Assert.Single(maximo.Valores));
        Assert.Equal("pDesc: o valor é maior que o máximo de 100 do tipo TPercentual.", acima.Mensagem);

        var abaixo = Unico(NotaDesconto("-1"));
        var minimo = UnicaRestricao(abaixo);
        Assert.Equal(FacetaSchema.MinimoInclusivo, minimo.Faceta);
        Assert.Equal("0", Assert.Single(minimo.Valores));
        Assert.Equal("pDesc: o valor é menor que o mínimo de 0 do tipo TPercentual.", abaixo.Mensagem);
    }

    [Fact]
    public void MinimoEMaximoExclusivos()
    {
        string NotaTributo(string valor) =>
            Nota().Replace("<vNF>60.00</vNF>", $"<vNF>60.00</vNF><vTotTrib>{valor}</vTotTrib>", StringComparison.Ordinal);

        Assert.Empty(Schema.Validar(NotaTributo("0.01")));
        Assert.Equal(FacetaSchema.MinimoExclusivo, UnicaRestricao(Unico(NotaTributo("0"))).Faceta);
        Assert.Equal(FacetaSchema.MaximoExclusivo, UnicaRestricao(Unico(NotaTributo("1000000"))).Faceta);
        Assert.Equal("vTotTrib: o valor tem de ser maior que 0 do tipo TValorPositivo.", Unico(NotaTributo("0")).Mensagem);
    }

    [Fact]
    public void TipoPrimitivo()
    {
        var decimalInvalido = Unico(Nota().Replace("<qCom>1.5000</qCom>" + "<vProd>10.00", "<qCom>1,5</qCom><vProd>10.00", StringComparison.Ordinal));
        var restricao = UnicaRestricao(decimalInvalido);
        Assert.Equal(FacetaSchema.TipoPrimitivo, restricao.Faceta);
        Assert.Equal("decimal", Assert.Single(restricao.Valores));
        Assert.Equal("xs:decimal", restricao.Tipo);
        Assert.Equal("qCom: o valor não é um número decimal (com ponto, sem separador de milhar) (tipo TQtd).", decimalInvalido.Mensagem);

        var data = Unico(Nota().Replace("<dEmi>2026-10-19</dEmi>", "<dEmi>19/10/2026</dEmi>", StringComparison.Ordinal));
        var primitivo = UnicaRestricao(data);
        Assert.Equal(FacetaSchema.TipoPrimitivo, primitivo.Faceta);
        Assert.Equal("date", Assert.Single(primitivo.Valores));
        Assert.Equal("xs:date", data.Schema!.TipoSchema);
        Assert.Equal("dEmi: o valor não é uma data (aaaa-mm-dd).", data.Mensagem);
    }

    [Fact]
    public void ValorVazio()
    {
        var problema = Unico(Nota().Replace("<nNF>123</nNF>", "<nNF></nNF>", StringComparison.Ordinal));
        Assert.Equal(FacetaSchema.Padrao, UnicaRestricao(problema).Faceta);
        Assert.Equal("", problema.Schema!.Valor);

        var autoFechado = Unico(Nota().Replace("<nNF>123</nNF>", "<nNF/>", StringComparison.Ordinal));
        Assert.Equal("NFe/infNFe/ide/nNF", autoFechado.Caminho);
        Assert.Equal(FacetaSchema.Padrao, UnicaRestricao(autoFechado).Faceta);
    }

    // Atributos

    [Fact]
    public void ValorDeAtributo()
    {
        var problema = Unico(Nota().Replace("versao=\"1.00\"", "versao=\"4.00\"", StringComparison.Ordinal));
        Assert.Equal("versao", problema.Tag);
        Assert.Equal("NFe/infNFe/@versao", problema.Caminho);
        var restricao = UnicaRestricao(problema);
        Assert.Equal(FacetaSchema.Padrao, restricao.Faceta);
        Assert.Equal(@"1\.00", Assert.Single(restricao.Valores));
        Assert.Equal("4.00", problema.Schema!.Valor);
        Assert.Equal(@"versao: o valor não segue o padrão do tipo TVerNFe (1\.00).", problema.Mensagem);
    }

    [Fact]
    public void AtributoDoItemRepetido()
    {
        var problema = Unico(Nota().Replace("<det nItem=\"2\">", "<det nItem=\"0\">", StringComparison.Ordinal));
        Assert.Equal("NFe/infNFe/det[2]/@nItem", problema.Caminho);
        Assert.Equal(FacetaSchema.Padrao, UnicaRestricao(problema).Faceta);
    }

    [Fact]
    public void AtributoObrigatorioAusente()
    {
        var problema = Unico(Nota().Replace(" versao=\"1.00\"", "", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.AtributoAusente, problema.Schema!.Tipo);
        Assert.Equal("versao", problema.Tag);
        Assert.Equal("NFe/infNFe/@versao", problema.Caminho);
        Assert.Equal(["versao"], problema.Schema.Esperados);
        Assert.Equal("infNFe: falta o atributo obrigatório versao.", problema.Mensagem);
    }

    [Fact]
    public void AtributoInesperado()
    {
        var problema = Unico(Nota().Replace("<ide>", "<ide modelo=\"55\">", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.AtributoInesperado, problema.Schema!.Tipo);
        Assert.Equal("modelo", problema.Tag);
        Assert.Equal("NFe/infNFe/ide/@modelo", problema.Caminho);
        Assert.Empty(problema.Schema.Esperados);
        Assert.Equal("modelo: o elemento ide não aceita atributos.", problema.Mensagem);

        var outro = Unico(Nota().Replace("Id=\"NFe1234\"", "Id=\"NFe1234\" chave=\"1\"", StringComparison.Ordinal));
        Assert.Equal(["Id", "versao"], outro.Schema!.Esperados);
        Assert.Equal("chave: o elemento infNFe não aceita este atributo; aceitos: Id, versao.", outro.Mensagem);
    }

    // Estrutura

    [Fact]
    public void ElementoObrigatorioAusenteNoFim()
    {
        var problema = Unico(Nota().Replace("<vProd>20.00</vProd>", "", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.ElementoAusente, problema.Schema!.Tipo);
        Assert.Equal("vProd", problema.Tag);
        Assert.Equal("NFe/infNFe/det[2]/prod/vProd", problema.Caminho);
        Assert.Equal(["vProd"], problema.Schema.Esperados);
        Assert.Equal("prod: falta o elemento obrigatório vProd.", problema.Mensagem);
    }

    [Fact]
    public void ElementoObrigatorioAusenteNoMeio()
    {
        // Sem o xProd, o qCom chega onde o xProd era esperado.
        var problema = Unico(Nota().Replace("<xProd>Produto 1</xProd>", "", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.ElementoInesperado, problema.Schema!.Tipo);
        Assert.Equal("qCom", problema.Tag);
        Assert.Equal("NFe/infNFe/det[1]/prod/qCom", problema.Caminho);
        Assert.Equal(["xProd"], problema.Schema.Esperados);
        Assert.Equal("qCom: o elemento está fora de ordem ou repetido; esperado: xProd.", problema.Mensagem);
    }

    [Fact]
    public void UmDosElementosDaEscolha()
    {
        var problema = Unico(Nota().Replace("<dinheiro>60.00</dinheiro>", "", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.ElementoAusente, problema.Schema!.Tipo);
        Assert.Equal("pag", problema.Tag);
        Assert.Equal("NFe/infNFe/pag", problema.Caminho);
        Assert.Equal(["dinheiro", "cartao"], problema.Schema.Esperados);
        Assert.Equal("pag: falta um dos elementos dinheiro, cartao.", problema.Mensagem);
    }

    [Fact]
    public void ElementoForaDeOrdem()
    {
        var problema = Unico(Nota().Replace("<cUF>31</cUF><tpAmb>2</tpAmb>", "<tpAmb>2</tpAmb><cUF>31</cUF>", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.ElementoInesperado, problema.Schema!.Tipo);
        Assert.Equal("tpAmb", problema.Tag);
        Assert.Equal("NFe/infNFe/ide/tpAmb", problema.Caminho);
        Assert.Equal(["cUF"], problema.Schema.Esperados);
    }

    [Fact]
    public void ElementoQueNaoExisteNoSchema()
    {
        var problema = Unico(Nota().Replace("<xNome>", "<xApelido>X</xApelido><xNome>", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.ElementoInesperado, problema.Schema!.Tipo);
        Assert.Equal("xApelido", problema.Tag);
        Assert.Equal("NFe/infNFe/emit/xApelido", problema.Caminho);
        Assert.Equal(["xNome"], problema.Schema.Esperados);
        Assert.Equal("xApelido: o elemento não era esperado; esperado: xNome.", problema.Mensagem);
    }

    [Fact]
    public void ConteudoDoElementoInesperadoNaoGeraRuido()
    {
        var problema = Unico(Nota().Replace("<xNome>", "<xApelido><a>1</a><b x=\"1\">2</b></xApelido><xNome>", StringComparison.Ordinal));
        Assert.Equal("NFe/infNFe/emit/xApelido", problema.Caminho);
    }

    [Fact]
    public void ElementoSemDeclaracaoAceitoPeloAnyEhAviso()
    {
        var xml = Nota().Replace("</pag>", "</pag><infAdic><x:obs xmlns:x=\"urn:exemplo\" a=\"1\"><x:t>1</x:t></x:obs></infAdic>", StringComparison.Ordinal);
        var problema = Unico(xml);
        Assert.Equal(Severidade.Aviso, problema.Severidade);
        Assert.Equal("225", problema.Codigo);
        Assert.Equal("NFe/infNFe/infAdic/obs", problema.Caminho);
        Assert.Equal(TipoFalhaSchema.Outra, problema.Schema!.Tipo);
        Assert.Equal("obs: o elemento não tem declaração no schema e não foi validado (o schema o aceita sem conferir).", problema.Mensagem);
    }

    [Fact]
    public void ElementoAMaisNoFim()
    {
        var problema = Unico(Nota().Replace("<vNF>60.00</vNF>", "<vNF>60.00</vNF><vTotTrib>1</vTotTrib><vTotTrib>2</vTotTrib>", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.ElementoInesperado, problema.Schema!.Tipo);
        Assert.Equal("NFe/infNFe/total/vTotTrib[2]", problema.Caminho);
        Assert.Empty(problema.Schema.Esperados);
        Assert.Equal("vTotTrib: o elemento não era esperado; nenhum outro elemento era esperado em total.", problema.Mensagem);
    }

    [Fact]
    public void FilhoEmElementoDeTipoSimples()
    {
        var problema = Unico(Nota().Replace("<nNF>123</nNF>", "<nNF><n>123</n></nNF>", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.ElementoInesperado, problema.Schema!.Tipo);
        Assert.Equal("NFe/infNFe/ide/nNF/n", problema.Caminho);
        Assert.Equal("n: o elemento nNF não aceita elementos filhos.", problema.Mensagem);
    }

    [Fact]
    public void TextoEmElementoComFilhos()
    {
        var problema = Unico(Nota().Replace("<ide>", "<ide>texto", StringComparison.Ordinal));
        Assert.Equal(TipoFalhaSchema.TextoInesperado, problema.Schema!.Tipo);
        Assert.Equal("ide", problema.Tag);
        Assert.Equal("NFe/infNFe/ide", problema.Caminho);
        Assert.Equal("ide: o elemento não aceita texto, só elementos filhos.", problema.Mensagem);
    }

    [Fact]
    public void RaizNaoDeclarada()
    {
        var problema = Unico($"<nfeProc xmlns=\"{Ns}\"><NFe/></nfeProc>");
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal(TipoFalhaSchema.ElementoInesperado, problema.Schema!.Tipo);
        Assert.Equal("nfeProc", problema.Caminho);
        Assert.Contains("NFe", problema.Schema.Esperados);
    }

    [Fact]
    public void NamespaceErradoNaRaiz()
    {
        var problema = Unico(Nota().Replace($"xmlns=\"{Ns}\"", "", StringComparison.Ordinal));
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal("NFe", problema.Tag);
        Assert.Contains("sem namespace", problema.Mensagem, StringComparison.Ordinal);
    }

    // Vários

    [Fact]
    public void VariosErrosVoltamTodosNaOrdemDoXml()
    {
        var dets = Det(1, "10.00") + Det(2, "20,00") + Det(3, "30.00").Replace("<xProd>Produto 3</xProd>", "<xProd>P</xProd>", StringComparison.Ordinal);
        var xml = Nota(dets).Replace("<cUF>31</cUF>", "<cUF>99</cUF>", StringComparison.Ordinal)
            .Replace("<dinheiro>60.00</dinheiro>", "", StringComparison.Ordinal);

        var problemas = Schema.Validar(xml);

        Assert.Equal(
            ["NFe/infNFe/ide/cUF", "NFe/infNFe/det[2]/prod/vProd", "NFe/infNFe/det[3]/prod/xProd", "NFe/infNFe/pag"],
            problemas.Select(p => p.Caminho));
        Assert.All(problemas, p => Assert.Equal("225", p.Codigo));
        Assert.Equal(FacetaSchema.Enumeracao, problemas[0].Schema!.Restricoes[0].Faceta);
        Assert.Equal(FacetaSchema.Padrao, problemas[1].Schema!.Restricoes[0].Faceta);
        Assert.Equal(FacetaSchema.TamanhoMinimo, problemas[2].Schema!.Restricoes[0].Faceta);
        Assert.Equal(TipoFalhaSchema.ElementoAusente, problemas[3].Schema!.Tipo);
    }

    [Fact]
    public void IndiceSoNoElementoQueSeRepete()
    {
        var problema = Unico(Nota(Det(1, "1,00")));
        Assert.Equal("NFe/infNFe/det/prod/vProd", problema.Caminho);
    }

    [Fact]
    public void LinhaEColunaDoXmlComQuebras()
    {
        var xml = Nota().Replace("<emit>", "\n  <emit>\n    ", StringComparison.Ordinal)
            .Replace("<CNPJ>11222333000181</CNPJ>", "<CNPJ>1</CNPJ>", StringComparison.Ordinal);
        var problema = Unico(xml);
        Assert.Equal(3, problema.Schema!.Linha);
        Assert.NotNull(problema.Schema.Coluna);
    }

    // XML

    [Fact]
    public void XmlMalformadoEhExcecao()
    {
        Assert.Throws<XmlException>(() => Schema.Validar(Nota().Replace("</emit>", "", StringComparison.Ordinal)));
        Assert.Throws<XmlException>(() => Schema.Validar(""));
    }

    [Fact]
    public void DtdNoXmlEhProibido()
    {
        var xml = "<!DOCTYPE NFe [ <!ENTITY x \"y\"> ]>" + Nota();
        Assert.Throws<XmlException>(() => Schema.Validar(xml));
    }

    [Fact]
    public void SchemaLocationDoXmlNaoEhSeguido()
    {
        var xml = Nota().Replace($"xmlns=\"{Ns}\"",
            $"xmlns=\"{Ns}\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"{Ns} http://exemplo.invalido/outro.xsd\"",
            StringComparison.Ordinal);
        Assert.Empty(Schema.Validar(xml));
    }

    [Fact]
    public void ComentariosEDeclaracaoNaoAtrapalham()
    {
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!-- nota -->\n"
            + Nota().Replace("<ide>", "<ide><!-- identificação -->", StringComparison.Ordinal).Replace("><", ">\n  <", StringComparison.Ordinal);
        Assert.Empty(Schema.Validar(xml));
    }

    // Paralelo e cultura

    [Fact]
    public async Task ValidaEmParalelo()
    {
        var invalido = Nota().Replace("<vProd>30.00</vProd>", "<vProd>12,5</vProd>", StringComparison.Ordinal);
        var tarefas = Enumerable.Range(0, 64).Select(i => Task.Run(() =>
        {
            for (var j = 0; j < 20; j++)
            {
                var xml = (i + j) % 2 == 0 ? Nota() : invalido;
                var problemas = Schema.Validar(xml);
                if ((i + j) % 2 == 0)
                    Assert.Empty(problemas);
                else
                    Assert.Equal("NFe/infNFe/det[3]/prod/vProd", Assert.Single(problemas).Caminho);
            }
        }, TestContext.Current.CancellationToken));
        await Task.WhenAll(tarefas);
    }

    [Theory]
    [InlineData("pt-BR")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ar-SA")]
    public void MensagemEmPortuguesEmQualquerCultura(string cultura)
    {
        var anterior = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultura);
            var problema = Unico(Nota().Replace("<qCom>1.5000</qCom>" + "<vProd>10.00", "<qCom>1.12345</qCom><vProd>10.00", StringComparison.Ordinal));
            Assert.Equal("qCom: o valor tem 5 casas decimais, acima do máximo de 4 do tipo TQtd.", problema.Mensagem);
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = anterior;
        }
    }

    // Propriedade do modelo (2.6): a conversão do leiaute, chamada com o caminho de cada problema

    [Fact]
    public void AConversaoPreencheAPropriedade()
    {
        var caminhos = new List<string>();
        var schema = SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, Recurso, "225", caminho =>
        {
            lock (caminhos)
                caminhos.Add(caminho);
            return caminho == "NFe/infNFe/det[3]/prod/vProd" ? "Itens[2].Produto.Valor" : null;
        });
        var xml = Nota().Replace("<vProd>30.00</vProd>", "<vProd>12,5</vProd>", StringComparison.Ordinal)
            .Replace("<CNPJ>11222333000181</CNPJ>", "<CNPJ>1122233300018</CNPJ>", StringComparison.Ordinal);

        var problemas = schema.Validar(xml);

        Assert.Equal(2, problemas.Count);
        var cnpj = Assert.Single(problemas, p => p.Tag == "CNPJ");
        Assert.Null(cnpj.Propriedade);
        var valor = Assert.Single(problemas, p => p.Tag == "vProd");
        Assert.Equal("Itens[2].Produto.Valor", valor.Propriedade);
        Assert.Equal("NFe/infNFe/det[3]/prod/vProd", valor.Caminho);
        Assert.Equal(["NFe/infNFe/emit/CNPJ", "NFe/infNFe/det[3]/prod/vProd"], caminhos);
        Assert.Contains("(Itens[2].Produto.Valor)", valor.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void SemAConversaoAPropriedadeFicaNula()
    {
        var problema = Unico(Nota().Replace("<vProd>30.00</vProd>", "<vProd>12,5</vProd>", StringComparison.Ordinal));
        Assert.Null(problema.Propriedade);
    }

    [Fact]
    public void AConversaoNaoEhChamadaNaNotaValida()
    {
        var schema = SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, Recurso, "225",
            _ => throw new InvalidOperationException("Não devia ser chamada."));
        Assert.Empty(schema.Validar(Nota()));
    }

    [Fact]
    public void AExcecaoDaConversaoSobeComoVeio()
    {
        var schema = SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, Recurso, "225",
            _ => throw new InvalidOperationException("Erro da conversão."));
        var xml = Nota().Replace("<vProd>30.00</vProd>", "<vProd>12,5</vProd>", StringComparison.Ordinal);

        var excecao = Assert.Throws<InvalidOperationException>(() => schema.Validar(xml));
        Assert.Equal("Erro da conversão.", excecao.Message);
    }

    [Fact]
    public void AConversaoNulaEhArgumentoInvalido()
    {
        Assert.Throws<ArgumentNullException>(() => SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, Recurso, "225", null!));
        Assert.Throws<ArgumentException>(() => SchemaXml.Carregar(typeof(SchemaXmlTestes).Assembly, Recurso, "", _ => null));
    }
}
