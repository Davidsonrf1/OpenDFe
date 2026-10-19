using System.Globalization;

namespace OpenDFe.Testes;

// O NFeStatusServico4 (MOC 7.0, Visão Geral, 5.5; Anexo I, grupos B, D e K) pelo TLS mútuo.
public class StatusServicoTestes(EmuladorFixture fixture) : IClassFixture<EmuladorFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RespondeEmOperacaoNoAutorizadorDaUF()
    {
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);

        var retorno = await fixture.Http.ConsultarStatusAsync(fixture.Url("MG"), Ct);

        Assert.Equal(Mensagens.NFe + "retConsStatServ", retorno.Name);
        Assert.Equal("4.00", retorno.Attribute("versao")?.Value);
        Assert.Equal("2", retorno.Campo("tpAmb"));
        Assert.Equal("MG-EMULADA", retorno.Campo("verAplic"));
        Assert.Equal("107", retorno.Campo("cStat"));
        Assert.Equal("Serviço em Operação", retorno.Campo("xMotivo"));
        Assert.Equal("31", retorno.Campo("cUF"));
        Assert.Equal("1", retorno.Campo("tMed"));
        Assert.Null(retorno.Element(Mensagens.NFe + "xObs"));
        var recebimento = DateTimeOffset.ParseExact(retorno.Campo("dhRecbto"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        Assert.Equal(TimeSpan.FromHours(-3), recebimento.Offset);
        Assert.InRange(recebimento, antes, DateTimeOffset.UtcNow.AddSeconds(1));
        // A ordem da Tabela 5-18.
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "dhRecbto", "tMed"], retorno.Elements().Select(e => e.Name.LocalName));
    }

    [Theory]
    [InlineData("SVRS", "42")]
    [InlineData("SVRS", "33")]
    [InlineData("SVAN", "21")]
    [InlineData("SVC-AN", "31")]
    [InlineData("SVC-RS", "29")]
    [InlineData("SP", "35")]
    public async Task RespondeEmOperacaoComAUFDoPedido(string destino, string uf)
    {
        var retorno = await fixture.Http.ConsultarStatusAsync(fixture.Url(destino), Ct, uf: uf);

        Assert.Equal("107", retorno.Campo("cStat"));
        Assert.Equal(uf, retorno.Campo("cUF"));
        Assert.Equal(destino + "-EMULADA", retorno.Campo("verAplic"));
    }

    [Fact]
    public async Task AceitaODestinoEOServicoSemDistinguirMaiusculas()
    {
        var retorno = await fixture.Http.ConsultarStatusAsync(new Uri(fixture.Emulador.Endereco, "mg/nfestatusservico4"), Ct);

        Assert.Equal("107", retorno.Campo("cStat"));
    }

    [Fact]
    public async Task RejeitaOAmbienteDeProducao()
    {
        var retorno = await fixture.Http.ConsultarStatusAsync(fixture.Url("MG"), Ct, ambiente: "1");

        Assert.Equal("252", retorno.Campo("cStat"));
        Assert.Equal("Rejeição: Ambiente informado diverge do Ambiente de recebimento", retorno.Campo("xMotivo"));
        Assert.Equal("2", retorno.Campo("tpAmb"));
        Assert.Null(retorno.Element(Mensagens.NFe + "tMed"));
    }

    [Theory]
    [InlineData("MG", "35", "31")]
    [InlineData("SVRS", "31", "43")]
    [InlineData("SVC-RS", "31", "43")]
    [InlineData("SVC-AN", "29", "91")]
    public async Task RejeitaAUFQueOAutorizadorNaoAtende(string destino, string uf, string ufRetorno)
    {
        var retorno = await fixture.Http.ConsultarStatusAsync(fixture.Url(destino), Ct, uf: uf);

        Assert.Equal("289", retorno.Campo("cStat"));
        Assert.Equal(ufRetorno, retorno.Campo("cUF"));
    }

    [Theory]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>STATUS</xServ><extra/></consStatServ>")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><cUF>31</cUF><tpAmb>2</tpAmb><xServ>STATUS</xServ></consStatServ>")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>31</cUF></consStatServ>")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\" Id=\"x\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>3</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>99</cUF><xServ>STATUS</xServ></consStatServ>")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>status</xServ></consStatServ>")]
    public async Task RejeitaAEstruturaForaDoSchema(string dados)
    {
        var retorno = await EnviarAsync(dados);

        Assert.Equal("215", retorno.Campo("cStat"));
        Assert.Equal("Rejeição: Falha no schema XML", retorno.Campo("xMotivo"));
        Assert.Equal("31", retorno.Campo("cUF"));
    }

    [Theory]
    [InlineData("<consSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"/>", "516")]
    [InlineData("<consStatServ versao=\"4.00\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>", "516")]
    [InlineData("", "516")]
    [InlineData("<nfe:consStatServ xmlns:nfe=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><nfe:tpAmb>2</nfe:tpAmb><nfe:cUF>31</nfe:cUF><nfe:xServ>STATUS</nfe:xServ></nfe:consStatServ>", "404")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" xmlns:nfe=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><nfe:tpAmb>2</nfe:tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>", "404")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>", "517")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"3.10\"><tpAmb>2</tpAmb><cUF>31</cUF><xServ>STATUS</xServ></consStatServ>", "239")]
    public async Task RejeitaARaizDaMensagem(string dados, string codigo)
    {
        var retorno = await EnviarAsync(dados);

        Assert.Equal(codigo, retorno.Campo("cStat"));
        Assert.StartsWith("Rejeição: ", retorno.Campo("xMotivo"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RejeitaAMensagemAcimaDe500KB()
    {
        var envelope = Mensagens.Envelope("<!--" + new string('x', Atendimento.LimiteDados) + "-->" + Mensagens.Status());

        using var resposta = await fixture.Http.EnviarAsync(fixture.Url("MG"), envelope, Ct);

        var retorno = Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(Ct));
        Assert.Equal("214", retorno.Campo("cStat"));
        Assert.Equal("31", retorno.Campo("cUF"));
    }

    [Fact]
    public async Task IgnoraOCabecalhoSoap()
    {
        var envelope = Mensagens.Envelope(Mensagens.Status()).Replace(
            "<soap12:Body>",
            "<soap12:Header><nfeCabecMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\"><cUF>31</cUF><versaoDados>4.00</versaoDados></nfeCabecMsg></soap12:Header><soap12:Body>",
            StringComparison.Ordinal);

        using var resposta = await fixture.Http.EnviarAsync(fixture.Url("MG"), envelope, Ct);

        Assert.Equal("107", Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(Ct)).Campo("cStat"));
    }

    private async Task<System.Xml.Linq.XElement> EnviarAsync(string dados)
    {
        using var resposta = await fixture.Http.EnviarAsync(fixture.Url("MG"), Mensagens.Envelope(dados), Ct);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(Ct));
    }
}
