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
}
