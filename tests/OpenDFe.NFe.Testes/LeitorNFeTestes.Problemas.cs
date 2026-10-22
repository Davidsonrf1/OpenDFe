using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// Os problemas da leitura (docs/LEITURA.md 4), a tolerância (3) e o detector com o leiaute 4.00 (2.1).
public partial class LeitorNFeTestes
{
    // A NF-e de teste escrita (sem assinatura: a leitura não confere), para os XMLs alterados.
    private static string Escrita() => NotasTeste.Escrever(NotasTeste.NFe()).Xml;

    [Theory]
    [InlineData("")]
    [InlineData("não é XML")]
    [InlineData("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe versao=\"4.00\">")]
    [InlineData("<!DOCTYPE NFe [<!ENTITY x \"y\">]><NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" />")]
    public void OXmlMalFormadoNaoTemNota(string xml)
    {
        var lida = Ler(xml);

        Assert.Null(lida.Nota);
        Assert.Equal(Severidade.Erro, Unico(lida, "NFE-XML").Severidade);
        Assert.Same(xml, lida.Xml);
    }

    [Theory]
    [InlineData("<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\" />")]
    [InlineData("<NFe><infNFe versao=\"4.00\" /></NFe>")]                                      // sem o namespace
    [InlineData("<CTe xmlns=\"http://www.portalfiscal.inf.br/cte\"><infCte versao=\"4.00\" /></CTe>")]
    [InlineData("<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\" />")]  // sem a NFe
    [InlineData("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" />")]                       // sem o infNFe
    public void OutraRaizNaoTemNota(string xml)
    {
        var lida = Ler(xml);

        Assert.Null(lida.Nota);
        Unico(lida, "NFE-RAIZ");
    }

    [Fact]
    public void OutraVersaoNaoTemNota()
    {
        var nfe = Trocar(Escrita(), "<infNFe versao=\"4.00\"", "<infNFe versao=\"3.10\"");
        var (assinada, chave) = Assinada();
        var proc = Trocar(Processada(assinada, chave.ToString()), "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\">",
            "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"3.10\">");

        Assert.Null(Ler(nfe).Nota);
        Unico(Ler(nfe), "NFE-VERSAO");
        Assert.Null(Ler(proc).Nota);
        Unico(Ler(proc), "NFE-VERSAO");
    }

    [Theory]
    [InlineData(null, "Falta o Id")]
    [InlineData("NFe123", "não é NFe seguido")]
    [InlineData("CTe31261011222333000181550010000001231009740371", "não é NFe seguido")]
    [InlineData("NFe31261011222333000181550010000001231009740372", "não é válida")]   // o DV errado
    public void OIdSemChaveValidaEProblemaEANotaELida(string? id, string mensagem)
    {
        var xml = Escrita();
        var original = System.Text.RegularExpressions.Regex.Match(xml, " Id=\"[^\"]*\"").Value;
        xml = Trocar(xml, original, id is null ? "" : $" Id=\"{id}\"");

        var lida = Ler(xml);

        Assert.NotNull(lida.Nota);
        Assert.Null(lida.Chave);
        var problema = Unico(lida, "NFE-CHAVE");
        Assert.Contains(mensagem, problema.Mensagem, StringComparison.Ordinal);
        Assert.Equal("NFe/infNFe/Id", problema.Caminho);
    }

    [Fact]
    public void OCDvDiferenteDaChaveEProblema()
    {
        var xml = Escrita();
        var digito = System.Text.RegularExpressions.Regex.Match(xml, "<cDV>(\\d)</cDV>").Groups[1].Value;
        xml = Trocar(xml, $"<cDV>{digito}</cDV>", $"<cDV>{(digito == "0" ? 1 : 0)}</cDV>");

        var problema = Unico(Ler(xml), "NFE-CHAVE");

        Assert.Equal(("NFe/infNFe/ide/cDV", "cDV"), (problema.Caminho, problema.Tag));
    }

    [Fact]
    public void AChaveDeOutroProtocoloEErro()
    {
        var (nfe, chave) = Assinada();
        var outra = ChaveAcesso.Montar(UF.MG, NotasTeste.Emissao, NotasTeste.CnpjEmitente, Documento.NFe, 1, 999, TipoEmissao.Normal, 12345678);

        var lida = Ler(Processada(nfe, chave.ToString(), outra.ToString()));

        var problema = Unico(lida, "NFE-PROTOCOLO");
        Assert.Equal((Severidade.Erro, "Protocolo.Chave"), (problema.Severidade, problema.Propriedade));
        Assert.NotNull(lida.Protocolo);
        Assert.NotNull(lida.Nota);
    }

    [Theory]
    [InlineData("<vProd>1000.00</vProd>", "<vProd>1000,00</vProd>", "NFe/infNFe/det[1]/prod/vProd", "Itens[0].Produto.Valor")]
    [InlineData("<nNF>4567</nNF>", "<nNF>45a7</nNF>", "NFe/infNFe/ide/nNF", "Identificacao.Numero")]
    [InlineData("<dhEmi>2026-10-20T14:30:00-03:00</dhEmi>", "<dhEmi>2026-10-20T14:30:00</dhEmi>", "NFe/infNFe/ide/dhEmi", "Identificacao.Emissao")]
    [InlineData("<dFab>2026-01-10</dFab>", "<dFab>10/01/2026</dFab>", "NFe/infNFe/det[1]/prod/rastro[1]/dFab", "Itens[0].Produto.Rastros[0].Fabricacao")]
    [InlineData("<UF>SP</UF>", "<UF>XX</UF>", null, null)]
    [InlineData("<tPag>03</tPag>", "<tPag>ABC</tPag>", "NFe/infNFe/pag/detPag[1]/tPag", "Pagamento.Formas[0].Meio")]
    [InlineData("<xNome>LOJA EXEMPLO LTDA</xNome>", "<xNome>LOJA <b>EXEMPLO</b></xNome>", "NFe/infNFe/emit/xNome", "Emitente.Nome")]
    public void OValorQueNaoConverteFicaNuloComErro(string de, string para, string? caminho, string? propriedade)
    {
        var lida = Ler(Trocar(Escrita(), de, para));

        var problema = Unico(lida, "NFE-VALOR");
        Assert.Equal(Severidade.Erro, problema.Severidade);
        if (caminho is not null)
            Assert.Equal((caminho, propriedade), (problema.Caminho, problema.Propriedade));
        Assert.DoesNotContain(para[(para.IndexOf('>') + 1)..para.LastIndexOf('<')], problema.Mensagem, StringComparison.Ordinal);
        Assert.NotNull(lida.Nota);
    }

    [Fact]
    public void OsCamposRetiradosSaoAvisoEFicamForaDoObjeto()
    {
        var xml = Escrita();
        xml = Trocar(xml, "<cEnq>999</cEnq>", "<cEnq>999</cEnq><clEnq>12345</clEnq>");
        xml = Trocar(xml, "<rastro>", "<cCredPresumido>MG800001</cCredPresumido><pCredPresumido>1.0000</pCredPresumido><rastro>");
        xml = Trocar(xml, "</gIBSCBS>", "</gIBSCBS><gIBSCBSMono><qBCMono>1.0000</qBCMono><gMonoPadrao><qBCMono>1.0000</qBCMono></gMonoPadrao></gIBSCBSMono>");
        xml = Trocar(xml, "<gCBS>", "<gCBS><gDif><vCBSOp>9.00</vCBSOp></gDif>");

        var lida = Ler(xml);

        var retirados = lida.Problemas.Where(p => p.Codigo == "NFE-RETIRADO").ToList();
        Assert.Equal(["cCredPresumido", "pCredPresumido", "clEnq", "vCBSOp", "qBCMono", "gMonoPadrao"], retirados.Select(p => p.Tag));
        Assert.All(retirados, p => Assert.Equal(Severidade.Aviso, p.Severidade));
        Assert.Equal("NFe/infNFe/det[1]/imposto/IPI/clEnq", retirados[2].Caminho);
        Assert.Equal("Itens[0].Imposto.Ipi", retirados[2].Propriedade);
        Assert.Contains("NT 2016.002 v1.40", retirados[2].Mensagem, StringComparison.Ordinal);
        Assert.Equal(retirados.Count, lida.Problemas.Count);
        var ibsCbs = lida.Nota!.Itens[0].Imposto.IbsCbs!;
        Assert.NotNull(ibsCbs.Monofasia);   // o grupo vigente entra, vazio
        Assert.Equal("999", lida.Nota.Itens[0].Imposto.Ipi!.CodigoEnquadramento);
    }

    [Fact]
    public void ODesconhecidoEAvisoComOQueTemDentro()
    {
        var xml = Trocar(Escrita(), "<emit>", "<emit><campoNovo><x>1</x></campoNovo>");
        xml = Trocar(xml, "<ide>", "<ide outro=\"1\">");
        xml = Trocar(xml, "</transp>", "</transp>texto solto");

        var lida = Ler(xml);

        var desconhecidos = lida.Problemas.Where(p => p.Codigo == "NFE-DESCONHECIDO").ToList();
        Assert.Equal(3, desconhecidos.Count);
        Assert.Equal(lida.Problemas.Count, desconhecidos.Count);
        Assert.Contains(desconhecidos, p => p.Caminho == "NFe/infNFe/emit/campoNovo" && p.Propriedade == "Emitente");
        Assert.Contains(desconhecidos, p => p.Caminho == "NFe/infNFe/ide/outro");
        Assert.Contains(desconhecidos, p => p.Caminho == "NFe/infNFe" && p.Mensagem.Contains("texto solto", StringComparison.Ordinal));
        Assert.All(desconhecidos, p => Assert.Equal(Severidade.Aviso, p.Severidade));
        Assert.Equal("LOJA EXEMPLO LTDA", lida.Nota!.Emitente.Nome);
    }

    [Fact]
    public void ORepetidoValeOPrimeiro()
    {
        var xml = Trocar(Escrita(), "<xNome>LOJA EXEMPLO LTDA</xNome>", "<xNome>LOJA EXEMPLO LTDA</xNome><xNome>OUTRA</xNome>");
        xml = Trocar(xml, "<ICMS00>", "<ICMS00>");   // o grupo está lá
        xml = Trocar(xml, "</ICMS00>", "</ICMS00><ICMS40><orig>0</orig><CST>40</CST></ICMS40>");

        var lida = Ler(xml);

        var repetidos = lida.Problemas.Where(p => p.Codigo == "NFE-REPETIDO").ToList();
        Assert.Equal(["xNome", "ICMS40"], repetidos.Select(p => p.Tag));
        Assert.Equal("LOJA EXEMPLO LTDA", lida.Nota!.Emitente.Nome);
        Assert.IsType<Icms00>(lida.Nota.Itens[0].Imposto.Icms);
    }

    [Fact]
    public void OCodigoForaDaListaEntraComoONumero()
    {
        var lida = Ler(Trocar(Escrita(), "<tPag>03</tPag>", "<tPag>77</tPag>"));

        var problema = Unico(lida, "NFE-ENUMERACAO");
        Assert.Equal((Severidade.Aviso, "Pagamento.Formas[0].Meio"), (problema.Severidade, problema.Propriedade));
        Assert.Equal((MeioPagamento)77, lida.Nota!.Pagamento.Formas[0].Meio);
    }

    [Fact]
    public void OsValoresQueNaoSaoNumeroSaoTraduzidosDeVolta()
    {
        var nota = NotasTeste.NFe();
        nota.Itens[0].Produto.Escala = IndicadorEscala.NaoRelevante;
        nota.Identificacao.Referencias.Add(new CupomFiscalReferenciado { Modelo = ModeloCupomFiscal.Ecf, NumeroEcf = 1, Coo = 2 });

        var lida = Ler(NotasTeste.Escrever(nota).Xml);

        Assert.Empty(lida.Problemas);
        Assert.Equal(IndicadorEscala.NaoRelevante, lida.Nota!.Itens[0].Produto.Escala);
        Assert.Equal(ModeloCupomFiscal.Ecf, lida.Nota.Identificacao.Referencias.OfType<CupomFiscalReferenciado>().Last().Modelo);
    }

    [Theory]
    [InlineData("<det nItem=\"2\">", "<det nItem=\"5\">")]
    [InlineData("<det nItem=\"2\">", "<det nItem=\"1\">")]
    [InlineData("<det nItem=\"2\">", "<det>")]
    public void ONItemForaDaSequenciaEAvisoEOItemFicaNaPosicao(string de, string para)
    {
        var lida = Ler(Trocar(Escrita(), de, para));

        var problema = Unico(lida, "NFE-ITEM");
        Assert.Equal(("NFe/infNFe/det[2]/nItem", Severidade.Aviso), (problema.Caminho, problema.Severidade));
        Assert.Equal(2, lida.Nota!.Itens.Count);
        Assert.NotNull(lida.Nota.Itens[1].Imposto.Issqn);   // o segundo é o serviço
    }

    [Fact]
    public void LeOXmlIndentadoComBomComentariosCdataEPrefixo()
    {
        var (xml, _) = Assinada();
        var esperada = Ler(xml).Nota!;

        var indentado = XDocument.Parse(xml).ToString();
        var comBom = "﻿<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!-- recebido por e-mail -->\n" + xml;
        var comentarios = Trocar(Trocar(xml, "<emit>", "<emit><!-- o emitente --><?instrucao x?>"),
            "<xNome>LOJA EXEMPLO LTDA</xNome>", "<xNome><![CDATA[LOJA EXEMPLO LTDA]]></xNome>");
        var atributos = System.Text.RegularExpressions.Regex.Replace(xml, "<infNFe versao=\"4.00\" (Id=\"[^\"]+\")>", "<infNFe $1 versao=\"4.00\">");
        var documento = XDocument.Parse(xml);
        documento.Root!.Add(new XAttribute(XNamespace.Xmlns + "nfe", Ns));
        var prefixo = documento.ToString(SaveOptions.DisableFormatting).Replace(" xmlns=\"" + Ns + "\"", "", StringComparison.Ordinal);
        Assert.Contains("<nfe:NFe", prefixo, StringComparison.Ordinal);

        foreach (var variante in new[] { indentado, comBom, comentarios, atributos, prefixo })
        {
            var lida = Ler(variante);
            Assert.True(lida.Problemas.Count == 0, string.Join("; ", lida.Problemas));
            Assert.Empty(ComparacaoModelo.Diferencas(esperada, lida.Nota!));
        }
    }

    [Theory]
    [InlineData(false, Documento.NFe)]
    [InlineData(true, Documento.NFCe)]
    public void ODetectorAchaOLeiaute400(bool consumidor, Documento documento)
    {
        var (xml, chave) = Assinada(consumidor ? NotasTeste.NFCe() : NotasTeste.NFe());

        var nfe = DFe.Detectar(xml, Leiaute);
        var proc = DFe.Detectar(Processada(xml, chave.ToString()), Leiaute);

        Assert.Equal((Leiaute, documento, "NFe", "4.00"), (nfe?.Leiaute as LeiauteNFe400, nfe?.Documento, nfe?.Raiz, nfe?.Versao));
        Assert.Equal((documento, "nfeProc"), (proc?.Documento, proc?.Raiz));
    }

    [Fact]
    public void ODetectorAchaANotaDoErpCoreENaoAcha310()
    {
        var exemplo = Exemplo("erpcore-nfeproc-entrada.xml");

        Assert.Equal(Documento.NFe, DFe.Detectar(exemplo, Leiaute)?.Documento);
        Assert.Null(DFe.Detectar(Trocar(Escrita(), "<infNFe versao=\"4.00\"", "<infNFe versao=\"3.10\""), Leiaute));
        Assert.Null(DFe.Detectar("<NFe />", Leiaute));
    }
}
