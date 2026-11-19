using System.Reflection;
using System.Security.Cryptography;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// O leiaute 1.01 (etapa 9.1): os schemas embutidos, as formas do detector (D7, D8), o caminho até a propriedade e a
// coerência entre o modelo e o mapa.
public class LeiauteNFSe101Testes
{
    // O SHA-256 de cada arquivo, igual ao do arquivo no ZIP do gov.br/nfse (docs/NTS.md 12.1).
    public static TheoryData<string, string> Arquivos => new()
    {
        { "1.00/CNC_v1.00.xsd", "9aa2f8bff12530e3fc3ee839c2768c3f78990ca33557e20e0416ed4e40b291ec" },
        { "1.00/DPS_v1.00.xsd", "35b1efecd3a1a7eeea218f62a65becc3d27522ded22b7614e676bbf222cae651" },
        { "1.00/NFSe_v1.00.xsd", "2c78fa25f38345209f2dbd0b5f2bd1be224fd92404f6c86af219c1294af455ba" },
        { "1.00/evento_v1.00.xsd", "3c34425d9b46b58628ecde8818f65b8c11bac958c3fc71ee1af4e9980e8bc08c" },
        { "1.00/pedRegEvento_v1.00.xsd", "ded678013de82b489f19d340824e07a45b2a01995af15ef56f7d2cdd27a5b317" },
        { "1.00/tiposCnc_v1.00.xsd", "9632fde4654d17ba5f130e9163155e44085799e8147e584effd2de8632efb36b" },
        { "1.00/tiposComplexos_v1.00.xsd", "52ecf2a78ffea4cb7f35c30d5c4b6dab8a8e7e84aa4565074eef22b2ab3669e1" },
        { "1.00/tiposEventos_v1.00.xsd", "6743a30763b087f1a5da036ea2864c8798ba4d3c48a293d00ecafefe31770bdf" },
        { "1.00/tiposSimples_v1.00.xsd", "9e35101b2a29b7b09f656ee4d83ed3752bd84d4b7ae370823f24efac744d0f04" },
        { "1.00/xmldsig-core-schema.xsd", "d87432c5f6ebb2bd61a1de357797401c203f14daefde834b73946f25f926a216" },
        { "1.01/CNC_v1.00.xsd", "7032188bb6f137d52b16512583b739a361e6b1434c42cb539a29b8efa32f8321" },
        { "1.01/DPS_v1.01.xsd", "fe45e5250a48e519aba89fc6a472863b8e602ed957778fb64692804933a00d0c" },
        { "1.01/NFSe_v1.01.xsd", "af0bd2d8c50acba3d9c3f3f515426eca083b3f66e3211ce8dd48a7cf101818b8" },
        { "1.01/evento_v1.01.xsd", "986d0a1c4d27454f712169849aa7c2380aaa4560bd9c920e0ff03ba6599ae28b" },
        { "1.01/pedRegEvento_v1.01.xsd", "e90b6816d29cca0bd5ed8f86ad98d3b7b0d8bbfc11a7583e6ff8f98170fd4009" },
        { "1.01/tiposCnc_v1.00.xsd", "af606b7317824fa8fa7ad8e44bac2ad8530cd5d162ebcdec647205473c41d525" },
        { "1.01/tiposComplexos_v1.01.xsd", "e8e09d525574cc224ca6d1f8d8eb0366043ab2a3aa8d0d234058e6244d60e371" },
        { "1.01/tiposEventos_v1.01.xsd", "1b32bea21089dc232d78b62d805e9c07382f440d4e0ea2cb3fb6b82ebde68c11" },
        { "1.01/tiposSimples_v1.01.xsd", "830ea116c34d7310699e34b214b7214a65f7e5d3b1f09aeaa702f7e3f4283b17" },
        // O xmldsig da 1.01 sem o bloco DOCTYPE do W3C (o núcleo proíbe DTD na carga, etapa 1.5); o resto, byte a byte o do
        // pacote, cujo SHA-256 é 49848f73...6c726046f (docs/NTS.md 12.1).
        { "1.01/xmldsig-core-schema.xsd", "6fa2eb631350cc5ba24d931b22469ecdda668ab466f9e38f2844077fc8303bec" },
    };

    [Theory]
    [MemberData(nameof(Arquivos))]
    public void OXSDEmbutidoTemOsBytesDoPacote(string arquivo, string sha256)
    {
        var assembly = typeof(LeiauteNFSe101).Assembly;
        // o nome do recurso leva a barra do sistema que compilou (o RecursiveDir); o carregador dos schemas normaliza
        var nome = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Replace('\\', '/') == $"Schemas/{SchemasNFSe101.Pacote}/{arquivo}");
        using var recurso = nome is null ? null : assembly.GetManifestResourceStream(nome);

        Assert.NotNull(recurso);
        Assert.Equal(sha256, Convert.ToHexStringLower(SHA256.HashData(recurso)));
    }

    [Fact]
    public void OsOitoSchemasCarregam()
    {
        SchemaXml[] schemas =
        [
            SchemasNFSe101.Dps, SchemasNFSe101.Nota, SchemasNFSe101.Pedido, SchemasNFSe101.Evento,
            SchemasNFSe101.Dps100, SchemasNFSe101.Nota100, SchemasNFSe101.Pedido100, SchemasNFSe101.Evento100,
        ];

        Assert.All(schemas, Assert.NotNull);
    }

    [Fact]
    public void OProblemaDoSchemaTemOCodigoE1235EAPropriedade()
    {
        var dps = DpsTeste.CompletaA();
        dps.Tomador!.Endereco = new Endereco { Exterior = new EnderecoExterior { Pais = "usa", CodigoPostal = "1", Cidade = "X", EstadoProvincia = "Y" }, Logradouro = "A", Numero = "1", Bairro = "B" };

        var problema = Assert.Single(SchemasNFSe101.Dps.Validar(LeiauteNFSe101.Instancia.Escrever(dps).Xml));

        Assert.Equal("E1235", problema.Codigo);
        Assert.Equal("Tomador.Endereco.Exterior.Pais", problema.Propriedade);
    }

    [Theory]
    [InlineData("DPS/infDPS/serv/cServ/xDescServ", "Servico.Codigo.Descricao")]
    [InlineData("DPS/infDPS/toma/end/endNac/CEP", "Tomador.Endereco.Nacional.Cep")]
    [InlineData("DPS/infDPS/valores/vDedRed/documentos/docDedRed[3]/NFSeMun/nNFSeMun", "Valores.DeducaoReducao.Documentos.Itens[2].NFSeMunicipal.Numero")]
    [InlineData("DPS/infDPS/valores/vDedRed/documentos/docDedRed/tpDedRed", "Valores.DeducaoReducao.Documentos.Itens[0].Tipo")]
    [InlineData("DPS/infDPS/serv/infoCompl/gItemPed/xItemPed[2]", "Servico.InformacoesComplementares.ItensPedido.Itens[1]")]
    [InlineData("DPS/infDPS/IBSCBS/valores/trib/gIBSCBS/gDif/pDifUF", "IbsCbs.Valores.Tributos.Classificacao.Diferimento.PercentualUF")]
    [InlineData("NFSe/infNFSe/emit/enderNac/UF", "Emitente.Endereco.UF")]
    [InlineData("NFSe/infNFSe/IBSCBS/totCIBS/gIBS/gIBSUFTot/vIBSUF", "IbsCbs.Totais.Ibs.UF.Valor")]
    [InlineData("NFSe/infNFSe/DPS/infDPS/prest/CNPJ", "Dps.Prestador.Cnpj")]
    [InlineData("pedRegEvento/infPedReg/chNFSe", "Chave")]
    [InlineData("pedRegEvento/infPedReg/e101101/xMotivo", "Descricao")]
    [InlineData("evento/infEvento/nSeqEvento", "Sequencia")]
    [InlineData("evento/infEvento/pedRegEvento/infPedReg/e105102/chSubstituta", "Pedido.ChaveSubstituta")]
    public void OCaminhoDoXMLViraAPropriedade(string caminho, string propriedade)
    {
        Assert.Equal(propriedade, CaminhoModeloNFSe.Propriedade(caminho));
    }

    [Theory]
    [InlineData("DPS/infDPS/@Id")]
    [InlineData("DPS/infDPS/naoExiste")]
    [InlineData("DPS/infDPS/serv/cServ/xDescServ/mais")]
    [InlineData("NFe/infNFe/ide")]
    [InlineData("DPS")]
    [InlineData("pedRegEvento/infPedReg/e101101/xDesc")]
    public void OCaminhoSemPropriedadeDaNulo(string caminho)
    {
        Assert.Null(CaminhoModeloNFSe.Propriedade(caminho));
    }

    // Cada propriedade [Tag] do modelo está no mapa da classe dela, com a mesma tag, e cada campo do mapa é uma
    // propriedade com aquela tag: o modelo e o mapa saem da mesma especificação, e o teste guarda a coerência.
    [Fact]
    public void OModeloEOMapaTemAsMesmasTags()
    {
        var mapas = typeof(MapasNFSe).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(f => f.FieldType == typeof(MapaGrupo)).Select(f => (MapaGrupo)f.GetValue(null)!).ToList();
        var diferencas = new List<string>();
        foreach (var mapa in mapas)
        {
            var propriedades = mapa.Tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => p.GetCustomAttribute<TagAttribute>() is not null).ToList();
            if (mapa.Tipo.BaseType == typeof(EventoNFSe) || mapa.Tipo == typeof(EventoNFSe))
                propriedades = [.. propriedades.Where(p => p.DeclaringType == mapa.Tipo)];
            foreach (var p in propriedades)
                if (mapa.Campo(p.GetCustomAttribute<TagAttribute>()!.Tag) is not { } campo || campo.Propriedade != p.Name)
                    diferencas.Add($"{mapa.Tipo.Name}.{p.Name}: fora do mapa.");
            foreach (var campo in mapa.Campos.Where(c => c.Propriedade.Length > 0))
                if (propriedades.FirstOrDefault(p => p.Name == campo.Propriedade) is not { } p || p.GetCustomAttribute<TagAttribute>()!.Tag != campo.Tag)
                    diferencas.Add($"{mapa.Tipo.Name}.{campo.Propriedade}: o mapa tem {campo.Tag}.");
        }

        Assert.Empty(diferencas);
        Assert.Equal(typeof(Dps).Assembly.GetTypes().Count(t => t.IsClass && t.IsPublic && t.GetProperties().Any(p => p.GetCustomAttribute<TagAttribute>() is not null)),
            mapas.Count);
    }

    [Fact]
    public void OLeiauteAtendeANFSeNaVersao101()
    {
        Assert.Equal("1.01", LeiauteNFSe101.Instancia.Versao);
        Assert.Equal([Documento.NFSe], LeiauteNFSe101.Instancia.Documentos);
        Assert.Equal(99, (int)Documento.NFSe);
    }

    [Fact]
    public void OLeiauteDeclaraAsQuatroFormasComAVersao100()
    {
        var formas = DeclaracaoLeiaute.Criar(LeiauteNFSe101.Instancia, Documento.NFSe).Leituras;

        Assert.Equal(["DPS", "NFSe", "pedRegEvento", "evento"], formas.Select(f => f.Raiz));
        Assert.All(formas, f =>
        {
            Assert.Equal(Documento.NFSe, f.Documento);
            Assert.Null(f.CaminhoModelo);
            Assert.Equal("@versao", f.CaminhoVersao);
            Assert.Equal(["1.00"], f.OutrasVersoes);
        });
    }

    [Fact]
    public void ODetectorAchaADPSANFSeEOsEventosNasDuasVersoes()
    {
        var dps = LeiauteNFSe101.Instancia.Escrever(DpsTeste.Minima()).Xml;
        var nota = NFSeTeste.Xml(NFSeTeste.Nota(DpsTeste.Minima()));
        var pedido = EscritorNFSe.EscreverPedido(new CancelamentoNFSe { Chave = DpsTeste.Chave }, null).Xml;
        var evento = EventosNFSeTestes.Registrado(pedido, "101101");

        foreach (var (xml, raiz, versao) in new[]
        {
            (dps, "DPS", "1.01"), (nota, "NFSe", "1.01"), (pedido, "pedRegEvento", "1.01"), (evento, "evento", "1.01"),
            (NFSeTeste.Xml100(), "NFSe", "1.00"),
        })
        {
            var deteccao = DFe.Detectar(xml, LeiauteNFSe101.Instancia);
            Assert.NotNull(deteccao);
            Assert.Equal((Documento.NFSe, raiz, versao), (deteccao.Documento, deteccao.Raiz, deteccao.Versao));
            Assert.Same(LeiauteNFSe101.Instancia, deteccao.Leiaute);
        }
    }

    [Theory]
    [InlineData("<DPS versao=\"2.00\" xmlns=\"http://www.sped.fazenda.gov.br/nfse\"/>")]
    [InlineData("<DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\"/>")]
    [InlineData("<DPS versao=\"1.01\"/>")]
    [InlineData("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe versao=\"4.00\"/></NFe>")]
    public void ODetectorNaoAchaOutraVersaoOutroNamespaceNemOutroDocumento(string xml)
    {
        Assert.Null(DFe.Detectar(xml, LeiauteNFSe101.Instancia));
    }

    [Fact]
    public void AsRegrasEstaoDeclaradas()
    {
        var problemas = RegrasNFSe101.Conferir(DpsTeste.CompletaB(), DpsTeste.Emissao);

        Assert.Contains(problemas, p => p.Codigo == "NFSE-EMITENTE");
    }
}
