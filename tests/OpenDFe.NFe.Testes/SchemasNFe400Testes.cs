using System.Security.Cryptography;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// Os XSDs embutidos no leiaute (docs/NTS.md): os bytes do pacote oficial, a carga e a compilação de cada mensagem
// pelo SchemaXml do núcleo, e a validação com os códigos do leiaute.
public class SchemasNFe400Testes
{
    private const string Ns = "http://www.portalfiscal.inf.br/nfe";
    private const string Xs = "http://www.w3.org/2001/XMLSchema";

    // O SHA-256 de cada arquivo, igual ao do arquivo no ZIP do Portal Nacional (a tabela de docs/NTS.md).
    public static TheoryData<string, string> Arquivos => new()
    {
        { "DFeTiposBasicos_v1.00.xsd", "173577a4e3a9dc1d0deced85b89b955b6064bb5a4ae5a96f6727d2da8a694d09" },
        { "leiauteNFe_v4.00.xsd", "2bace939973916d54184ff3e2740041a932de5d79772f3363504504160f22542" },
        { "nfe_v4.00.xsd", "adce3646c13ceb54922ec3142fc1dc45bd4fb839ac35ad583e86c733c07d27df" },
        { "tiposBasico_v4.00.xsd", "772619c85723e598840667ca66e7298a250442df47eeb94b397d2a333ce62047" },
        { "xmldsig-core-schema_v1.01.xsd", "f56744a5f51c03f027de13f39f869307091781a9ef1d91b1ebe14719ce28e1ac" },
        { "consSitNFe_v4.00.xsd", "44d816a725553a0c53508be919629c9235b42c5d41c358a05be8fdf61945f5c1" },
        { "leiauteConsSitNFe_v4.00.xsd", "646e3a6bd26720b12a1322d5d25181504d82175c3650e14a1120dfb65895b618" },
        { "leiauteInutNFe_v4.00.xsd", "e6aa8f817d445cd7796f5bf448ee462598cc00393608ffa038f3a7539de4b184" },
        { "procInutNFe_v4.00.xsd", "c71ce9ed656e22161923d77af47839421fa41c63703a064655880fed76e20334" },
        { "retConsReciNFe_v4.00.xsd", "5cc8219feac16f43a21e168ec4cbf8d0dcf8dade5dccf3f07a42f538519129e9" },
        { "retConsSitNFe_v4.00.xsd", "8b7188d6fe9e879ca057bfb68600746c7491d2e092e3dc5121a6cd5aa79272d5" },
        { "consReciNFe_v4.00.xsd", "0520356a8ac619c8730f793ed2a955418ce5a138b6811f3a6f2c0e72017002c5" },
        { "consStatServ_v4.00.xsd", "20c2cae522ac79ea52353ca66d7595c6616a46a491e561b076145fe8574b8773" },
        { "enviNFe_v4.00.xsd", "9ba17a12e3fd6e153bd8676450526ba3bcfc15b75ecb372823826ee827b8c3ef" },
        { "inutNFe_v4.00.xsd", "d50dd40f010634d8576ad880552a3d186bd4cfe0a4b554f3097dae2cc5c6a59e" },
        { "leiauteConsStatServ_v4.00.xsd", "cdc0813b5bf0087c7c8510f6ea0c2a367c829d9ea4b44c23533d18d3bcf23375" },
        { "procNFe_v4.00.xsd", "b7af3072ff7398aa557c42bc6d8198dd3c006b1a6ecaad66bc839a5c34cbcb84" },
        { "retConsStatServ_v4.00.xsd", "337ceed7f74c0764edbc8a18feacaf8c33f66aa951c46b4b29b561cda7c2f7fb" },
        { "retEnviNFe_v4.00.xsd", "d58b700f849ea7df9f1d35580ae0de425dfa1e295408c02609b81905b04ce4ce" },
        { "retInutNFe_v4.00.xsd", "8b5590f79c157510c5f65a43c7325008c01677ebf24e14eef26e4817ea2673a1" },
    };

    // Os schemas principais: os que declaram o elemento raiz de uma mensagem (os outros são incluídos por eles).
    public static TheoryData<string> Mensagens => new()
    {
        "nfe_v4.00.xsd", "procNFe_v4.00.xsd",
        "enviNFe_v4.00.xsd", "retEnviNFe_v4.00.xsd",
        "consReciNFe_v4.00.xsd", "retConsReciNFe_v4.00.xsd",
        "consSitNFe_v4.00.xsd", "retConsSitNFe_v4.00.xsd",
        "consStatServ_v4.00.xsd", "retConsStatServ_v4.00.xsd",
        "inutNFe_v4.00.xsd", "retInutNFe_v4.00.xsd", "procInutNFe_v4.00.xsd",
    };

    // O nome do recurso com a barra normalizada: no Windows, o RecursiveDir do LogicalName vem com "\" (o
    // SchemaXml aceita as duas).
    private static string[] Recursos() =>
        [.. typeof(LeiauteNFe400).Assembly.GetManifestResourceNames().Select(nome => nome.Replace('\\', '/'))];

    private static Stream Abrir(string arquivo)
    {
        var assembly = typeof(LeiauteNFe400).Assembly;
        var nome = assembly.GetManifestResourceNames().Single(n => n.Replace('\\', '/') == SchemasNFe400.Recurso(arquivo));
        return assembly.GetManifestResourceStream(nome)!;
    }

    // Os arquivos

    [Fact]
    public void OsRecursosSaoExatamenteOsArquivosDoPacote()
    {
        var esperados = Arquivos.Select(linha => SchemasNFe400.Recurso(linha.Data.Item1)).Order(StringComparer.Ordinal);
        var embutidos = Recursos().Order(StringComparer.Ordinal);

        Assert.Equal(esperados, embutidos);
    }

    [Theory]
    [MemberData(nameof(Arquivos))]
    public void CadaArquivoTemOsBytesDoPacoteOficial(string arquivo, string sha256)
    {
        using var conteudo = Abrir(arquivo);

        Assert.Equal(sha256, Convert.ToHexStringLower(SHA256.HashData(conteudo)));
    }

    [Theory]
    [InlineData("IBSCBS")]
    [InlineData("IS")]
    [InlineData("IBSCBSTot")]
    [InlineData("ISTot")]
    [InlineData("vNFTot")]
    public void ALeiauteDaNotaTemOsGruposDaReformaTributaria(string elemento)
    {
        using var conteudo = Abrir("leiauteNFe_v4.00.xsd");
        var schema = XDocument.Load(conteudo);

        Assert.Contains(schema.Descendants(XName.Get("element", Xs)), e => (string?)e.Attribute("name") == elemento);
    }

    // Carga e compilação

    [Theory]
    [MemberData(nameof(Mensagens))]
    public void CadaMensagemCarregaECompilaPeloNucleo(string arquivo)
    {
        var schema = SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso(arquivo), SchemasNFe400.CodigoMensagem);

        Assert.NotNull(schema);
    }

    [Fact]
    public void OsSchemasGuardadosCarregamUmaVez()
    {
        SchemaXml[] primeira = [SchemasNFe400.Nota, SchemasNFe400.Lote, SchemasNFe400.Recibo, SchemasNFe400.Situacao,
            SchemasNFe400.Status, SchemasNFe400.Inutilizacao, SchemasNFe400.NotaProcessada];
        SchemaXml[] segunda = [SchemasNFe400.Nota, SchemasNFe400.Lote, SchemasNFe400.Recibo, SchemasNFe400.Situacao,
            SchemasNFe400.Status, SchemasNFe400.Inutilizacao, SchemasNFe400.NotaProcessada];

        Assert.Equal(7, primeira.Distinct().Count());
        Assert.All(primeira.Zip(segunda), par => Assert.Same(par.First, par.Second));
    }

    // Validação

    [Fact]
    public void OStatusValidoNaoTemProblema()
    {
        var xml = $"<consStatServ xmlns=\"{Ns}\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>";

        Assert.Empty(SchemasNFe400.Status.Validar(xml));
    }

    [Fact]
    public void OStatusComAmbienteInvalidoTemOProblemaDaMensagem()
    {
        var xml = $"<consStatServ xmlns=\"{Ns}\" versao=\"4.00\"><tpAmb>3</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>";

        var problema = Assert.Single(SchemasNFe400.Status.Validar(xml));

        Assert.Equal("215", problema.Codigo);
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal("tpAmb", problema.Tag);
        Assert.Equal("consStatServ/tpAmb", problema.Caminho);
        Assert.Equal("TAmb", problema.Schema?.TipoSchema);
        var restricao = Assert.Single(problema.Schema!.Restricoes);
        Assert.Equal(FacetaSchema.Enumeracao, restricao.Faceta);
        Assert.Equal(["1", "2"], restricao.Valores);
    }

    [Fact]
    public void ANotaIncompletaTemOProblemaDaNota()
    {
        var chave = ChaveAcesso.Montar(UF.MG, new DateTimeOffset(2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3)),
            "11222333000181", Documento.NFe, 1, 1, TipoEmissao.Normal, 12345670);
        var xml = $"<NFe xmlns=\"{Ns}\"><infNFe versao=\"4.00\" Id=\"NFe{chave}\"></infNFe></NFe>";

        var problemas = SchemasNFe400.Nota.Validar(xml);

        // O primeiro grupo do infNFe e a assinatura, que o TNFe exige (daí assinar antes do schema, CONTRATOS.md 12).
        Assert.Collection(problemas,
            p => Assert.Equal(("225", "NFe/infNFe/ide", TipoFalhaSchema.ElementoAusente), (p.Codigo, p.Caminho, p.Schema?.Tipo)),
            p => Assert.Equal(("225", "NFe/Signature", TipoFalhaSchema.ElementoAusente), (p.Codigo, p.Caminho, p.Schema?.Tipo)));
    }

    [Fact]
    public void AConsultaAceitaAChaveComCnpjAlfanumerico()
    {
        // NT 2026.004 (CNPJ alfanumérico): o TChNFe do pacote aceita letras nas posições do CNPJ.
        var chave = ChaveAcesso.Montar(UF.SP, new DateTimeOffset(2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3)),
            "12.ABC.345/01DE-35", Documento.NFe, 1, 123, TipoEmissao.Normal, 12345670);
        var xml = $"<consSitNFe xmlns=\"{Ns}\" versao=\"4.00\"><tpAmb>2</tpAmb><xServ>CONSULTAR</xServ><chNFe>{chave}</chNFe></consSitNFe>";

        Assert.Matches("[A-Z]", chave.ToString());
        Assert.Empty(SchemasNFe400.Situacao.Validar(xml));
    }

    [Fact]
    public void OProblemaDoValorTrazADocumentacaoDoCampoEDoTipo()
    {
        var xml = NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()));
        var errado = xml.Replace("<vProd>50.00</vProd>", "<vProd>50.123</vProd>", StringComparison.Ordinal);
        Assert.NotEqual(xml, errado);

        // O vProd do item e o do total têm o mesmo valor: o do item.
        var problema = Assert.Single(SchemasNFe400.Nota.Validar(errado), p => p.Caminho == "NFe/infNFe/det/prod/vProd");

        Assert.Equal("TDec_1302", problema.Schema!.TipoSchema);
        Assert.Equal("Valor bruto do produto ou serviço.", problema.Schema.Documentacao);
        Assert.Equal("Tipo Decimal com 15 dígitos, sendo 13 de corpo e 2 decimais", problema.Schema.DocumentacaoTipo);
    }

    [Fact]
    public void OElementoQueFaltaTrazADocumentacaoDele()
    {
        var xml = NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()));
        var errado = xml.Replace("<xProd>CAFE TORRADO 500G</xProd>", "", StringComparison.Ordinal);
        Assert.NotEqual(xml, errado);

        // O NCM chega no lugar do xProd: o problema é o elemento inesperado, com a documentação do que faltou.
        var problema = Assert.Single(SchemasNFe400.Nota.Validar(errado));

        Assert.Equal(TipoFalhaSchema.ElementoInesperado, problema.Schema!.Tipo);
        Assert.Contains("xProd", problema.Schema.Esperados);
        Assert.Equal("Descrição do produto ou serviço", problema.Schema!.Documentacao);
    }
}
