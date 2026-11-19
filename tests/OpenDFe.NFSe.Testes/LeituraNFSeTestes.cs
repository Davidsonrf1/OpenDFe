using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// A leitura da NFS-e e da DPS (etapa 9.1, docs/NFSE.md 7): as NFS-e de exemplo, válidas no XSD, lidas com a nota, a DPS, a
// chave e a versão; a 1.00 com o que a 1.01 tirou (M10, M12); e os problemas da leitura, sem lançar.
public class LeituraNFSeTestes
{
    [Fact]
    public void ANFSe101DeExemploEValidaNoXSD()
    {
        Assert.Empty(SchemasNFSe101.Nota.Validar(NFSeTeste.Xml(NFSeTeste.Nota(DpsTeste.CompletaA()))));
    }

    [Fact]
    public void ANFSe101ELidaInteiraComADPSAChaveEAVersao()
    {
        var nota = NFSeTeste.Nota(DpsTeste.CompletaA());
        var xml = NFSeTeste.Xml(nota);

        var lida = LeiauteNFSe101.Instancia.Ler(xml);

        Assert.Empty(lida.Problemas);
        Assert.Equal("1.01", lida.Versao);
        Assert.Same(xml, lida.Xml);
        Assert.Equal(NFSeTeste.Chave, lida.Chave!.Value.Valor);
        Assert.Equal(EscritorNFSe.Escrever(nota.Dps, null).Id, lida.IdentificadorDps);
        Assert.Same(lida.Nota!.Dps, lida.Dps);
        Assert.Empty(ComparacaoNFSe.Diferencas(nota, lida.Nota));
    }

    [Fact]
    public void ANFSeComCadaVarianteDaDPSELidaIgual()
    {
        foreach (var (nome, criar) in DpsTeste.Todas())
        {
            var nota = NFSeTeste.Nota(criar());
            var xml = NFSeTeste.Xml(nota);
            Assert.Empty(SchemasNFSe101.Nota.Validar(xml));

            var lida = LeiauteNFSe101.Instancia.Ler(xml);

            Assert.True(lida.Problemas.Count == 0, nome);
            Assert.Empty(ComparacaoNFSe.Diferencas(nota, lida.Nota!));
        }
    }

    [Fact]
    public void ANFSe100DeExemploEValidaNoXSD100()
    {
        Assert.Empty(SchemasNFSe101.Nota100.Validar(NFSeTeste.Xml100()));
    }

    [Fact]
    public void ANFSe100ELidaComOsGruposQueA101Tirou()
    {
        var lida = LeiauteNFSe101.Instancia.Ler(NFSeTeste.Xml100());
        var nota = lida.Nota!;
        var servico = nota.Dps.Servico;

        Assert.Equal("1.00", lida.Versao);
        Assert.Equal("Uso do município na 1.00", nota.InformacoesFisco);
        Assert.Null(nota.Valores.TipoBeneficio);
        Assert.Equal(1000.00m, nota.Valores.Liquido);
        Assert.Equal((CategoriaLocacao.Locacao, ObjetoLocacao.Postes, 120, 45),
            (servico.Locacao!.Categoria, servico.Locacao.Objeto, servico.Locacao.Extensao, servico.Locacao.Postes));
        Assert.Equal(("FESTIVAL", "EVT9"), (servico.Evento!.Nome, servico.Evento.Identificacao));
        Assert.Equal((CategoriaVeiculo.Automovel, 2, TipoRodagem.Simples, 1, "ABC1234", "ABCDE12345", "AB12"),
            (servico.ExploracaoRodoviaria!.Categoria, servico.ExploracaoRodoviaria.Eixos, servico.ExploracaoRodoviaria.Rodagem,
             servico.ExploracaoRodoviaria.Sentido, servico.ExploracaoRodoviaria.Placa, servico.ExploracaoRodoviaria.CodigoAcesso,
             servico.ExploracaoRodoviaria.CodigoContrato));
        Assert.Equal(TipoBeneficioLei.Isencao, nota.Dps.Valores.Tributacao.Municipal.Beneficio!.Tipo);
        Assert.Equal(7, lida.IdentificadorDps!.Value.Numero);
    }

    [Fact]
    public void NaNFSe100OTpBMEmTextoFicaNuloComAviso()
    {
        var problema = Assert.Single(LeiauteNFSe101.Instancia.Ler(NFSeTeste.Xml100()).Problemas, p => p.Tag == "tpBM");

        Assert.Equal(("NFSE-VALOR", Severidade.Aviso), (problema.Codigo, problema.Severidade));
        Assert.Equal("Valores.TipoBeneficio", problema.Propriedade);
        Assert.Equal("NFSe/infNFSe/valores/tpBM", problema.Caminho);
    }

    // M12: o 2 da 1.00 (exportação, no texto do XSD 1.00) entra como o número, sem tradução.
    [Fact]
    public void NaNFSe100OTribISSQNEntraComoONumero()
    {
        var nota = LeiauteNFSe101.Instancia.Ler(NFSeTeste.Xml100()).Nota!;

        Assert.Equal(2, (int)nota.Dps.Valores.Tributacao.Municipal.Tributacao!.Value);
    }

    [Fact]
    public void ASituacaoForaDaListaEntraComoONumeroComAviso()
    {
        var lida = LeiauteNFSe101.Instancia.Ler(NFSeTeste.Xml100());

        Assert.Equal(101, (int)lida.Nota!.Situacao!.Value);
        var problema = Assert.Single(lida.Problemas, p => p.Tag == "cStat");
        Assert.Equal(("NFSE-ENUMERACAO", Severidade.Aviso, "Situacao"), (problema.Codigo, problema.Severidade, problema.Propriedade));
    }

    [Fact]
    public void ANFSe100TemSoOsDoisAvisosEsperados()
    {
        Assert.Equal(["cStat", "tpBM"], LeiauteNFSe101.Instancia.Ler(NFSeTeste.Xml100()).Problemas.Select(p => p.Tag).Order());
    }

    [Fact]
    public void OTextoQueNaoEXMLEProblemaSemLancar()
    {
        var lida = LeiauteNFSe101.Instancia.Ler("<NFSe>");

        Assert.Null(lida.Nota);
        Assert.Equal("NFSE-XML", Assert.Single(lida.Problemas).Codigo);
    }

    [Fact]
    public void OutraRaizEProblema()
    {
        var lida = LeiauteNFSe101.Instancia.Ler("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"/>");

        Assert.Equal("NFSE-RAIZ", Assert.Single(lida.Problemas).Codigo);
        Assert.Null(lida.Versao);
    }

    [Theory]
    [InlineData("<DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\"/>")]
    [InlineData("<DPS versao=\"2.00\" xmlns=\"http://www.sped.fazenda.gov.br/nfse\"/>")]
    public void OutraVersaoOuSemVersaoEProblema(string xml)
    {
        Assert.Equal("NFSE-VERSAO", Assert.Single(LeiauteNFSe101.Instancia.Ler(xml).Problemas).Codigo);
    }

    [Fact]
    public void ODTDERecusado()
    {
        var lida = LeiauteNFSe101.Instancia.Ler("<!DOCTYPE DPS [<!ENTITY x \"y\">]><DPS versao=\"1.01\" xmlns=\"http://www.sped.fazenda.gov.br/nfse\"/>");

        Assert.Equal("NFSE-XML", Assert.Single(lida.Problemas).Codigo);
    }

    [Fact]
    public void OCampoDesconhecidoEAvisoEORestoELido()
    {
        var xml = LeiauteNFSe101.Instancia.Escrever(DpsTeste.Minima()).Xml.Replace("<serie>", "<extra>1</extra><serie>", StringComparison.Ordinal);

        var lida = LeiauteNFSe101.Instancia.Ler(xml);

        var problema = Assert.Single(lida.Problemas);
        Assert.Equal(("NFSE-DESCONHECIDO", Severidade.Aviso, "DPS/infDPS/extra"), (problema.Codigo, problema.Severidade, problema.Caminho));
        Assert.Equal("1", lida.Dps!.Serie);
    }

    [Fact]
    public void OCampoRepetidoValeOPrimeiroComAviso()
    {
        var xml = LeiauteNFSe101.Instancia.Escrever(DpsTeste.Minima()).Xml.Replace("<serie>1</serie>", "<serie>1</serie><serie>2</serie>", StringComparison.Ordinal);

        var lida = LeiauteNFSe101.Instancia.Ler(xml);

        Assert.Equal(("NFSE-REPETIDO", "Serie"), (Assert.Single(lida.Problemas).Codigo, lida.Problemas[0].Propriedade));
        Assert.Equal("1", lida.Dps!.Serie);
    }

    [Fact]
    public void OValorQueNaoConverteFicaNuloComErroSemOValorNaMensagem()
    {
        var xml = LeiauteNFSe101.Instancia.Escrever(DpsTeste.Minima()).Xml.Replace("<vServ>1000.00</vServ>", "<vServ>12a34</vServ>", StringComparison.Ordinal);

        var lida = LeiauteNFSe101.Instancia.Ler(xml);

        var problema = Assert.Single(lida.Problemas);
        Assert.Equal(("NFSE-VALOR", Severidade.Erro, "Valores.Servico.Valor", "DPS/infDPS/valores/vServPrest/vServ"),
            (problema.Codigo, problema.Severidade, problema.Propriedade, problema.Caminho));
        Assert.DoesNotContain("12a34", problema.Mensagem, StringComparison.Ordinal);
        Assert.Null(lida.Dps!.Valores.Servico.Valor);
    }

    [Fact]
    public void OIndiceDaListaApareceNoCaminhoENaPropriedade()
    {
        var dps = DpsTeste.CompletaC();
        var xml = LeiauteNFSe101.Instancia.Escrever(dps).Xml.Replace("<nDoc>RECIBO-1</nDoc>", "<nDoc>RECIBO-1</nDoc><extra/>", StringComparison.Ordinal);

        var problema = Assert.Single(LeiauteNFSe101.Instancia.Ler(xml).Problemas);

        Assert.Equal("DPS/infDPS/valores/vDedRed/documentos/docDedRed[6]/extra", problema.Caminho);
    }

    [Fact]
    public void OIdInvalidoEProblemaEADPSELida()
    {
        var xml = LeiauteNFSe101.Instancia.Escrever(DpsTeste.Minima()).Xml.Replace("Id=\"DPS", "Id=\"XPS", StringComparison.Ordinal);

        var lida = LeiauteNFSe101.Instancia.Ler(xml);

        Assert.Equal("NFSE-CHAVE-ID", Assert.Single(lida.Problemas).Codigo);
        Assert.Null(lida.IdentificadorDps);
        Assert.NotNull(lida.Dps);
    }
}
