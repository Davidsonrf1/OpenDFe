using System.Net;
using System.Text.Json;

namespace OpenDFe.Testes;

// As rotas /<destino>/<serviço>, a lista das URLs e o envelope SOAP 1.2 (MOC 7.0, Visão Geral, 4.2.2).
public class AtendimentoTestes(EmuladorFixture fixture) : IClassFixture<EmuladorFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("XX", Servico.NomeStatus)]
    [InlineData("MG", Servico.NomeDistribuicao)]
    [InlineData("SVRS", Servico.NomeDistribuicao)]
    [InlineData("SVAN", Servico.NomeConsultaCadastro)]
    [InlineData("SVC-AN", Servico.NomeConsultaCadastro)]
    [InlineData("SVC-RS", Servico.NomeConsultaCadastro)]
    [InlineData("AN", Servico.NomeConsultaCadastro)]
    [InlineData("SVC-AN", Servico.NomeInutilizacao)]
    [InlineData("SVC-RS", Servico.NomeInutilizacao)]
    [InlineData("AN", Servico.NomeAutorizacao)]
    [InlineData("AN", Servico.NomeStatus)]
    [InlineData("MG", "NFeServicoInexistente")]
    public async Task Responde404AoDestinoOuServicoQueNaoAtende(string destino, string servico)
    {
        using var resposta = await fixture.Http.EnviarAsync(fixture.Url(destino, servico), Mensagens.Envelope(Mensagens.Status(), servico), Ct);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Responde415ForaDoSoap12()
    {
        using var resposta = await fixture.Http.EnviarAsync(fixture.Url("MG"), Mensagens.Envelope(Mensagens.Status()), Ct, tipo: "text/xml");

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, resposta.StatusCode);
    }

    [Fact]
    public async Task RespondeVersionMismatchAoEnvelopeSoap11()
    {
        var envelope = Mensagens.Envelope(Mensagens.Status()).Replace(Mensagens.NamespaceSoap, "http://schemas.xmlsoap.org/soap/envelope/", StringComparison.Ordinal);

        using var resposta = await fixture.Http.EnviarAsync(fixture.Url("MG"), envelope, Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("soap:VersionMismatch", Mensagens.Falha(await resposta.Content.ReadAsStringAsync(Ct)));
    }

    [Theory]
    [InlineData("<soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body>")]
    [InlineData("<?xml version=\"1.0\"?><!DOCTYPE x [<!ENTITY e \"e\">]><soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body/></soap12:Envelope>")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"/>")]
    [InlineData("<soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"/>")]
    [InlineData("<soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"><soap12:Body><nfeDadosMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4\"/></soap12:Body></soap12:Envelope>")]
    public async Task RespondeSenderAoEnvelopeQueNaoServe(string corpo)
    {
        using var resposta = await fixture.Http.EnviarAsync(fixture.Url("MG"), corpo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("soap:Sender", Mensagens.Falha(await resposta.Content.ReadAsStringAsync(Ct)));
    }

    [Fact]
    public async Task RespondeNoEnvelopeSoap12ComOTipoDoSoap12()
    {
        using var resposta = await fixture.Http.EnviarAsync(fixture.Url("MG"), Mensagens.Envelope(Mensagens.Status()), Ct);

        Assert.Equal("application/soap+xml", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", resposta.Content.Headers.ContentType?.CharSet);
        var texto = await resposta.Content.ReadAsStringAsync(Ct);
        var raiz = System.Xml.Linq.XDocument.Parse(texto).Root!;
        Assert.Equal(Mensagens.Soap + "Envelope", raiz.Name);
        var resultado = raiz.Element(Mensagens.Soap + "Body")!.Element((System.Xml.Linq.XNamespace)"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4" + "nfeResultMsg");
        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task ListaAsUrlsDeCadaServicoImplementadoEmCadaAutorizador()
    {
        using var resposta = await fixture.Http.GetAsync(fixture.Emulador.Endereco, Ct);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(Ct));
        Assert.Equal(Path.Combine(fixture.Pasta, "ac.crt"), json.RootElement.GetProperty("autoridade").GetString());
        var urls = json.RootElement.GetProperty("urls").EnumerateArray()
            .Select(u => (Destino: u.GetProperty("destino").GetString(), Servico: u.GetProperty("servico").GetString(), Url: u.GetProperty("url").GetString()))
            .ToList();
        // O status, a autorização, o recibo e a consulta nos 14 autorizadores que os têm; os eventos nos 14 e no
        // AN (a manifestação); a inutilização nos 12 que não são a SVC (Anexo III, 2.1.3, e); a consulta cadastro
        // nas 10 UFs e na SVRS; a distribuição só no AN.
        Assert.Equal(95, urls.Count);
        foreach (var servico in (string[])[Servico.NomeStatus, Servico.NomeAutorizacao, Servico.NomeRetAutorizacao, Servico.NomeConsultaProtocolo])
            Assert.Equal(14, urls.Count(u => u.Servico == servico));
        Assert.Equal(15, urls.Count(u => u.Servico == Servico.NomeRecepcaoEvento));
        Assert.Equal(12, urls.Count(u => u.Servico == Servico.NomeInutilizacao));
        Assert.Equal(11, urls.Count(u => u.Servico == Servico.NomeConsultaCadastro));
        Assert.Equal(["AN/NFeDistribuicaoDFe", "AN/NFeRecepcaoEvento4"],
            urls.Where(u => u.Destino == "AN").Select(u => u.Destino + "/" + u.Servico).Order(StringComparer.Ordinal));
        Assert.DoesNotContain(urls, u => u.Destino is "SVAN" or "SVC-AN" or "SVC-RS" && u.Servico == Servico.NomeConsultaCadastro);
        Assert.DoesNotContain(urls, u => u.Destino is "SVC-AN" or "SVC-RS" && u.Servico == Servico.NomeInutilizacao);
        Assert.Contains(urls, u => u.Destino == "MG" && u.Url == new Uri(fixture.Emulador.Endereco, "MG/NFeStatusServico4").ToString());
        Assert.Contains(urls, u => u.Destino == "SVC-RS" && u.Servico == Servico.NomeRetAutorizacao);
        Assert.Equal(urls.Count, fixture.Emulador.Urls.Count);
    }
}
