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
        { "envEvento_v1.00.xsd", "e4d61ba81582b5f1f9a2a4b135d33f4c1ec1c8f9138a2343a9abfffa02bb2777" },
        { "leiauteEvento_v1.00.xsd", "9a48df8603f685653dfde27dbda72e0f47b5d3c13be0090952f1e82f8d2bc476" },
        { "procEventoNFe_v1.00.xsd", "4263ee5d12e7a731efab126310aba4edb25612e5986a1c5dde0c87e6c6173798" },
        { "retEnvEvento_v1.00.xsd", "5eb93a244c5b26c983c08f725f8eafaa1923664a992181b9d78f60cc8523fa72" },
        { "tiposBasico_v1.03.xsd", "d89d4fe1136f89abc7878b440d91ac0e770fbc128dd35b483284c961d6c3fd5d" },
        { "e110110_v1.00.xsd", "0a56d7efa780579eb82f28c2069748bad1bf0fdbae57f3c59b0a198cecf3b718" },
        { "e110111_v1.00.xsd", "56fcdad336b281a9de5e7aae27850bc2c5bd7714e267c891e1b4d0dc227c65da" },
        { "e110112_v1.00.xsd", "73c555577c21a10d9f572efaafe56ad52314b86e98d9ba3a8d679a2750ee944e" },
        { "e210200_v1.00.xsd", "afe48b729cdefbdf122d1dee81382aa7832b9bdf1596ed4de6c5451070071b98" },
        { "e210210_v1.00.xsd", "ebc9171b9b64192599270cafdf51e13756b09cebce83d2e78ac0140dee4003e7" },
        { "e210220_v1.00.xsd", "636e742e2f6784b8626a8f3b284d3eac01e2ea28a1fb8a56d9d847e5b92abcfd" },
        { "e210240_v1.00.xsd", "e7df7243f0a6d5b7b590e5251b83f34a52d0ec3a68a8e6b398bcb4cfc862b14e" },
        { "distDFeInt_v1.01.xsd", "2d95df1430b0f0e2b04c6f29cf6bb69cdb5a392ac52102987653902b9a34ff7a" },
        { "retDistDFeInt_v1.01.xsd", "059c4183e190282851337e80725c6ac4e996736a85077246d8711b3b31406236" },
        { "resNFe_v1.01.xsd", "6ca072e27d72d3a74065d3414af669f7ad525f8883a22af89d22da96095856fa" },
        { "resEvento_v1.01.xsd", "77bd5c05e336fa2854f8ab71316e07fe30759c5efedf493bb684a77c26af9d55" },
        { "tiposDistDFe_v1.01.xsd", "0bc215b9bda6422e37c2c28b80590b4c5ba3041f0db85ca17ebba61edadd7547" },
        // A consulta cadastro numa subpasta, como no PL_010d: o tiposBasico_v1.03 dela não é o dos eventos.
        { "CadConsultaCadastro/consCad_v2.00.xsd", "8e5cfa3386c1e61999fc290ad0c3cf166258125447c53a342bea5036524b0fc0" },
        { "CadConsultaCadastro/leiauteConsultaCadastro_v2.00.xsd", "85e613e63109398d79561bd2d7c372c62fdd033bde537c2c74cd2abaed9b5d9d" },
        { "CadConsultaCadastro/retConsCad_v2.00.xsd", "2662459d90461de342e9aeee5b9b9505d25f698e773e1d5c3a0207d7eb2fc9e2" },
        { "CadConsultaCadastro/tiposBasico_v1.03.xsd", "0822f1b36b535dc090df244409cd9d8b86628950236e79f5b716ec2b1a208c1a" },
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
        "envEvento_v1.00.xsd", "retEnvEvento_v1.00.xsd", "procEventoNFe_v1.00.xsd",
        // O detEvento de cada evento, validado à parte do evento genérico (MOC 7.0, Anexo I, D06).
        "e110110_v1.00.xsd", "e110111_v1.00.xsd", "e110112_v1.00.xsd",
        "e210200_v1.00.xsd", "e210210_v1.00.xsd", "e210220_v1.00.xsd", "e210240_v1.00.xsd",
        // A distribuição DF-e, com os resumos que vão no docZip, e a consulta cadastro.
        "distDFeInt_v1.01.xsd", "retDistDFeInt_v1.01.xsd", "resNFe_v1.01.xsd", "resEvento_v1.01.xsd",
        "CadConsultaCadastro/consCad_v2.00.xsd", "CadConsultaCadastro/retConsCad_v2.00.xsd",
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
