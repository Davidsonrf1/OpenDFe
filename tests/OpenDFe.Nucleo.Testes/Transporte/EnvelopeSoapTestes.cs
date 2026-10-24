using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.Testes;

/// <summary>O envelope SOAP 1.2 escrito e lido à mão (PROJETO.md 8.2).</summary>
public class EnvelopeSoapTestes
{
    private static readonly ServicoSoap Status = new(
        "NFeStatusServico4", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4",
        "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4/nfeStatusServicoNF");

    private const string Pedido =
        "<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>";

    private const string Retorno =
        "<retConsStatServ versao=\"4.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\"><tpAmb>2</tpAmb><cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo></retConsStatServ>";

    [Fact]
    public void MontaAMensagemDentroDoNfeDadosMsgNoNamespaceDoWsdl()
    {
        var envelope = EnvelopeSoap.Montar(Status, Pedido);

        Assert.Equal(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body>"
            + "<nfeDadosMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\">" + Pedido + "</nfeDadosMsg>"
            + "</soap12:Body></soap12:Envelope>",
            envelope);
        var pedido = XDocument.Parse(envelope).Descendants(XName.Get("consStatServ", "http://www.portalfiscal.inf.br/nfe")).Single();
        Assert.Equal("4.00", pedido.Attribute("versao")?.Value);
    }

    [Theory]
    [InlineData("<?xml version=\"1.0\" encoding=\"UTF-8\"?>")]
    [InlineData("﻿<?xml version=\"1.0\"?>\n")]
    [InlineData("﻿")]
    public void TiraADeclaracaoEOBomDaMensagem(string inicio)
    {
        var envelope = EnvelopeSoap.Montar(Status, inicio + Pedido);

        Assert.Contains("<nfeDadosMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\">" + Pedido, envelope, StringComparison.Ordinal);
        Assert.Single(XDocument.Parse(envelope).Descendants(XName.Get("consStatServ", "http://www.portalfiscal.inf.br/nfe")));
    }

    [Theory]
    [InlineData("soap", "nfeResultMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\"")]
    [InlineData("env", "nfeResultMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\"")]
    [InlineData("soap", "nfeResultMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/OutroNome\"")]
    [InlineData("soap", "nfeResultMsg")]
    public void LeAMensagemDoNfeResultMsg(string prefixo, string resultado)
    {
        var resposta = $"﻿<?xml version=\"1.0\" encoding=\"utf-8\"?><{prefixo}:Envelope xmlns:{prefixo}=\"http://www.w3.org/2003/05/soap-envelope\">"
            + $"<{prefixo}:Header/><{prefixo}:Body><{resultado}>{Retorno}</nfeResultMsg></{prefixo}:Body></{prefixo}:Envelope>";

        Assert.Equal(Retorno, EnvelopeSoap.Ler(resposta));
    }

    [Fact]
    public void LeOResultadoDentroDeUmElementoDaOperacao()
    {
        var resposta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><nfeStatusServicoNFResult xmlns=\"urn:x\">"
            + "<nfeResultMsg>" + Retorno + "</nfeResultMsg></nfeStatusServicoNFResult></soap:Body></soap:Envelope>";

        Assert.Equal(Retorno, EnvelopeSoap.Ler(resposta));
    }

    [Theory]
    [InlineData("Sender", true)]
    [InlineData("VersionMismatch", true)]
    [InlineData("MustUnderstand", true)]
    [InlineData("DataEncodingUnknown", true)]
    [InlineData("Receiver", false)]
    public void AFalhaSoapDizSeFoiDoRemetente(string codigo, bool remetente)
    {
        var resposta = "<env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Body><env:Fault>"
            + $"<env:Code><env:Value>env:{codigo}</env:Value></env:Code><env:Reason><env:Text xml:lang=\"pt\"> Motivo dado </env:Text></env:Reason>"
            + "</env:Fault></env:Body></env:Envelope>";

        var falha = Assert.Throws<FalhaSoapException>(() => EnvelopeSoap.Ler(resposta));

        Assert.Equal(codigo, falha.Codigo);
        Assert.Equal("Motivo dado", falha.Motivo);
        Assert.Equal(remetente, falha.Remetente);
        Assert.Equal($"Falha SOAP {codigo}: Motivo dado", falha.Message);
    }

    [Fact]
    public void AFalhaSemCodigoNemMotivoEDoReceptor()
    {
        var falha = Assert.Throws<FalhaSoapException>(() => EnvelopeSoap.Ler(
            "<env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Body><env:Fault/></env:Body></env:Envelope>"));

        Assert.False(falha.Remetente);
        Assert.Equal("Falha SOAP sem código: (sem motivo)", falha.Message);
    }

    [Theory]
    [InlineData("<html><body>erro</body></html>")]
    [InlineData("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><nfeResultMsg/></soap:Body></soap:Envelope>")]
    [InlineData("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"/>")]
    [InlineData("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body/></soap:Envelope>")]
    [InlineData("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><nfeResultMsg>  </nfeResultMsg></soap:Body></soap:Envelope>")]
    public void RecusaAQuiloQueNaoEOEnvelopeComAMensagem(string resposta) =>
        Assert.Throws<InvalidDataException>(() => EnvelopeSoap.Ler(resposta));

    [Theory]
    [InlineData("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>")]
    [InlineData("")]
    [InlineData("<!DOCTYPE x [<!ENTITY e \"x\">]><soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"/>")]
    public void ORespostaMalFormadaOuComDtdEXmlException(string resposta) =>
        Assert.Throws<XmlException>(() => EnvelopeSoap.Ler(resposta));

    [Fact]
    public void OServicoSoapExigeOsTresValores()
    {
        Assert.Throws<ArgumentNullException>(() => new ServicoSoap(null!, "urn:a", "urn:a/b"));
        Assert.Throws<ArgumentException>(() => new ServicoSoap("S", " ", "urn:a/b"));
        Assert.Throws<ArgumentException>(() => new ServicoSoap("S", "urn:a", ""));
        var servico = new ServicoSoap("S", "urn:a", "urn:a/b");
        Assert.Equal(("S", "urn:a", "urn:a/b"), (servico.Nome, servico.NamespaceWsdl, servico.Acao));
    }

    // O serviço com a operação no Body (a distribuição DF-e, NT 2014.002)

    private static readonly ServicoSoap Distribuicao = new(
        "NFeDistribuicaoDFe", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe",
        "http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe/nfeDistDFeInteresse") { Operacao = "nfeDistDFeInteresse" };

    private const string PedidoDistribuicao =
        "<distDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><tpAmb>2</tpAmb><CNPJ>11222333000181</CNPJ><distNSU><ultNSU>000000000000000</ultNSU></distNSU></distDFeInt>";

    private const string RetornoDistribuicao =
        "<retDistDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><tpAmb>2</tpAmb><cStat>137</cStat><xMotivo>Nenhum documento localizado</xMotivo></retDistDFeInt>";

    [Fact]
    public void ComOperacaoOMontaDadosMsgDentroDoElementoDaOperacao()
    {
        var envelope = EnvelopeSoap.Montar(Distribuicao, PedidoDistribuicao);

        Assert.Equal(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body>"
            + "<nfeDistDFeInteresse xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe\">"
            + "<nfeDadosMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe\">" + PedidoDistribuicao + "</nfeDadosMsg>"
            + "</nfeDistDFeInteresse></soap12:Body></soap12:Envelope>",
            envelope);
        XNamespace wsdl = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe";
        var corpo = XDocument.Parse(envelope).Root!.Element(XName.Get("Body", EnvelopeSoap.NamespaceSoap))!;
        var operacao = Assert.Single(corpo.Elements());
        Assert.Equal(wsdl + "nfeDistDFeInteresse", operacao.Name);
        Assert.Equal(wsdl + "nfeDadosMsg", Assert.Single(operacao.Elements()).Name);
        Assert.Single(operacao.Descendants(XName.Get("distDFeInt", "http://www.portalfiscal.inf.br/nfe")));
    }

    [Fact]
    public void SemOperacaoOEnvelopeNaoMudaUmByte()
    {
        Assert.Null(Status.Operacao);
        Assert.Equal(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body>"
            + "<nfeDadosMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\">" + Pedido + "</nfeDadosMsg>"
            + "</soap12:Body></soap12:Envelope>",
            EnvelopeSoap.Montar(Status, Pedido));
        Assert.Equal(EnvelopeSoap.Montar(Status, Pedido),
            EnvelopeSoap.Montar(new ServicoSoap(Status.Nome, Status.NamespaceWsdl, Status.Acao) { Operacao = null }, Pedido));
    }

    [Theory]
    [InlineData("<nfeDistDFeInteresseResponse xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe\"><nfeDistDFeInteresseResult>{0}</nfeDistDFeInteresseResult></nfeDistDFeInteresseResponse>")]
    [InlineData("<r:nfeDistDFeInteresseResponse xmlns:r=\"urn:outro\"><r:nfeDistDFeInteresseResult>{0}</r:nfeDistDFeInteresseResult></r:nfeDistDFeInteresseResponse>")]
    [InlineData("<a><b><nfeDistDFeInteresseResult xmlns=\"urn:x\">{0}</nfeDistDFeInteresseResult></b></a>")]
    public void ComOperacaoALeituraAchaOResultadoEmQualquerProfundidadeENamespace(string corpo)
    {
        var resultado = string.Format(System.Globalization.CultureInfo.InvariantCulture, corpo, RetornoDistribuicao);
        var resposta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>" + resultado + "</soap:Body></soap:Envelope>";

        var mensagem = EnvelopeSoap.Ler(resposta, Distribuicao);

        Assert.StartsWith("<retDistDFeInt ", mensagem, StringComparison.Ordinal);
        Assert.Contains("<cStat>137</cStat>", mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public void ComOperacaoONfeResultMsgDiretoNaoServeENemOResultadoSemOperacao()
    {
        var comResultMsg = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><nfeResultMsg>" + RetornoDistribuicao
            + "</nfeResultMsg></soap:Body></soap:Envelope>";
        var comResultado = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><nfeDistDFeInteresseResult>" + RetornoDistribuicao
            + "</nfeDistDFeInteresseResult></soap:Body></soap:Envelope>";

        var semResultado = Assert.Throws<InvalidDataException>(() => EnvelopeSoap.Ler(comResultMsg, Distribuicao));
        var semResultMsg = Assert.Throws<InvalidDataException>(() => EnvelopeSoap.Ler(comResultado));

        Assert.Contains("nfeDistDFeInteresseResult", semResultado.Message, StringComparison.Ordinal);
        Assert.Contains("nfeResultMsg", semResultMsg.Message, StringComparison.Ordinal);
        Assert.Equal(RetornoDistribuicao, EnvelopeSoap.Ler(comResultMsg));
    }

    [Fact]
    public void ComOperacaoOResultadoVazioEAFalhaSoapSaemComoNosOutros()
    {
        var vazio = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><nfeDistDFeInteresseResponse><nfeDistDFeInteresseResult/>"
            + "</nfeDistDFeInteresseResponse></soap:Body></soap:Envelope>";
        var falha = "<env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Body><env:Fault><env:Code><env:Value>env:Sender</env:Value></env:Code>"
            + "<env:Reason><env:Text xml:lang=\"pt\">Operacao desconhecida</env:Text></env:Reason></env:Fault></env:Body></env:Envelope>";

        Assert.Throws<InvalidDataException>(() => EnvelopeSoap.Ler(vazio, Distribuicao));
        var erro = Assert.Throws<FalhaSoapException>(() => EnvelopeSoap.Ler(falha, Distribuicao));
        Assert.True(erro.Remetente);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1abc")]
    [InlineData("com espaco")]
    [InlineData("a:b")]
    [InlineData("a<b")]
    public void AOperacaoTemDeSerUmNomeXmlSimples(string operacao) =>
        Assert.Throws<ArgumentException>(() => new ServicoSoap("S", "urn:a", "urn:a/b") { Operacao = operacao });

    [Fact]
    public void AOperacaoAceitaNomeSimplesENulo()
    {
        Assert.Equal("nfeDistDFeInteresse", Distribuicao.Operacao);
        Assert.Equal("op_1", new ServicoSoap("S", "urn:a", "urn:a/b") { Operacao = "op_1" }.Operacao);
        Assert.Null(new ServicoSoap("S", "urn:a", "urn:a/b") { Operacao = null }.Operacao);
    }
}
