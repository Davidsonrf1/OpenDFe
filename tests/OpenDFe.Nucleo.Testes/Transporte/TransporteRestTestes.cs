using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace OpenDFe.Testes;

/// <summary>
/// O transporte REST da 9.2 (docs/NFSE.md, D4 e D5) pelo caminho inteiro do <see cref="DFe.ExecutarAsync{TRetorno}"/>, com
/// o leiaute REST falso e o <see cref="ConexoesSefaz"/> real contra o Kestrel local com TLS mútuo: o método, o caminho, os
/// cabeçalhos, o certificado do cliente e o corpo; o que vai ao leiaute e o que é falha, com a espécie e se pode ter sido
/// processado.
/// </summary>
public sealed class TransporteRestTestes : IClassFixture<ServidorLocalFixture>
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("teste"), "teste");

    private readonly ServidorLocal _servidor;

    public TransporteRestTestes(ServidorLocalFixture fixture)
    {
        _servidor = fixture.Servidor;
        _servidor.Responder = contexto => Json(contexto, 200, "{\"ok\": true}");
        _servidor.Pedidos.Clear();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Task Json(HttpContext contexto, int status, string corpo) =>
        ServidorLocal.EscreverAsync(contexto, status, corpo, "application/json; charset=utf-8");

    private ConexoesSefaz Conexoes() => new(ConexaoTeste.Fabrica(_servidor.Autoridade), TimeProvider.System);

    private DFe Nucleo(ConexoesSefaz conexoes) =>
        DFe.Criar(new LeiauteRestFalso(), Documento.NFSe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Urls = [new EnderecoServico(Destino.SefinNacional, Ambiente.Homologacao, LeiauteRestFalso.Servico, new Uri(_servidor.Endereco, "rest/Base"))],
        });

    [Fact]
    public async Task OPostLevaOJsonComOCertificadoDoCliente()
    {
        _servidor.Responder = contexto => Json(contexto, 201, "{\"chaveAcesso\": \"1\"}");
        using var conexoes = Conexoes();

        var resultado = await Nucleo(conexoes).ExecutarAsync(new RestFalso(), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(201, resultado.CStat);
        Assert.Equal(201, resultado.Rastro.StatusHttp);
        Assert.Equal("{\"chaveAcesso\": \"1\"}", resultado.Rastro.XmlRetorno);
        var pedido = Assert.Single(_servidor.Pedidos);
        Assert.Equal("POST", pedido.Metodo);
        Assert.Equal("/rest/Base/eco", pedido.Caminho);
        Assert.Equal("application/json; charset=utf-8", pedido.Tipo);
        Assert.Equal("application/json", pedido.Aceita);
        Assert.Equal(Certificado.Thumbprint, pedido.Cliente);
        using var json = JsonDocument.Parse(pedido.Corpo);
        Assert.True(json.RootElement.TryGetProperty(LeiauteRestFalso.Campo, out _));
    }

    [Theory]
    [InlineData(MetodoRest.Get, "GET")]
    [InlineData(MetodoRest.Head, "HEAD")]
    public async Task AConsultaVaiSemCorpo(MetodoRest metodo, string nome)
    {
        using var conexoes = Conexoes();

        var resultado = await Nucleo(conexoes).ExecutarAsync(new RestFalso { Metodo = metodo, Caminho = "dps/DPS1" }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(200, resultado.CStat);
        var pedido = Assert.Single(_servidor.Pedidos);
        Assert.Equal(nome, pedido.Metodo);
        Assert.Equal("/rest/Base/dps/DPS1", pedido.Caminho);
        Assert.Equal("", pedido.Corpo);
        Assert.Null(pedido.Tipo);
        if (metodo == MetodoRest.Head)
            Assert.Null(resultado.Retorno.Corpo);
    }

    [Theory]
    [InlineData(400, "{\"erros\": []}")]
    [InlineData(404, "{\"erros\": []}")]
    [InlineData(422, "[1, 2]")]
    [InlineData(403, "{\"erros\": []}")]
    public async Task O4xxComJsonVaiAoLeiaute(int status, string corpo)
    {
        _servidor.Responder = contexto => Json(contexto, status, corpo);
        using var conexoes = Conexoes();

        var resultado = await Nucleo(conexoes).ExecutarAsync(new RestFalso(), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(status, resultado.CStat);
        Assert.Equal(corpo, resultado.Retorno.Corpo);
    }

    [Fact]
    public async Task O404DoHeadVaiAoLeiauteEODoGetSemJsonNao()
    {
        _servidor.Responder = contexto =>
        {
            contexto.Response.StatusCode = 404;
            return Task.CompletedTask;
        };
        using var conexoes = Conexoes();
        var dfe = Nucleo(conexoes);

        var head = await dfe.ExecutarAsync(new RestFalso { Metodo = MetodoRest.Head, Caminho = "dps/DPS1" }, Ct);
        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new RestFalso { Metodo = MetodoRest.Get, Caminho = "dps/DPS1" }, Ct));

        Assert.Equal(404, head.CStat);
        Assert.Equal(TipoFalhaTransmissao.Http, falha.Tipo);
        Assert.False(falha.PodeTerSidoProcessado);
        Assert.Equal(404, falha.Rastro.StatusHttp);
    }

    [Theory]
    [InlineData(MetodoRest.Post, true)]
    [InlineData(MetodoRest.Get, false)]
    public async Task O2xxQueNaoEJsonEIlegivel(MetodoRest metodo, bool processado)
    {
        _servidor.Responder = contexto => ServidorLocal.EscreverAsync(contexto, 200, "<html>erro</html>", "text/html");
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new RestFalso { Metodo = metodo }, Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.Equal(processado, falha.PodeTerSidoProcessado);
        Assert.Equal(200, falha.Rastro.StatusHttp);
        Assert.Equal("<html>erro</html>", falha.Rastro.XmlRetorno);
    }

    [Fact]
    public async Task O403SemJsonEOCertificadoRecusado()
    {
        _servidor.Responder = contexto => ServidorLocal.EscreverAsync(contexto, 403, "Forbidden", "text/plain");
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new RestFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.CertificadoRecusado, falha.Tipo);
        Assert.False(falha.PodeTerSidoProcessado);
        Assert.Equal(403, falha.Rastro.StatusHttp);
        Assert.Equal("Forbidden", falha.Rastro.XmlRetorno);
    }

    [Theory]
    [InlineData(400, MetodoRest.Post, false)]
    [InlineData(500, MetodoRest.Post, true)]
    [InlineData(503, MetodoRest.Post, true)]
    [InlineData(500, MetodoRest.Get, false)]
    public async Task ORestoEFalhaHttp(int status, MetodoRest metodo, bool processado)
    {
        // O 5xx com JSON também é falha (D5): a Sefin fora do ar.
        _servidor.Responder = contexto => status == 400
            ? ServidorLocal.EscreverAsync(contexto, status, "Bad Request", "text/plain")
            : Json(contexto, status, "{\"erros\": [{\"codigo\": \"X\"}]}");
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new RestFalso { Metodo = metodo }, Ct));

        Assert.Equal(TipoFalhaTransmissao.Http, falha.Tipo);
        Assert.Equal(processado, falha.PodeTerSidoProcessado);
        Assert.Equal(status, falha.Rastro.StatusHttp);
    }

    [Fact]
    public async Task OSoapEORestDividemOHandlerDoCertificado()
    {
        _servidor.Responder = contexto => contexto.Request.Path.StartsWithSegments("/rest")
            ? Json(contexto, 200, "{}")
            : ServidorLocal.EscreverAsync(contexto, 200, ServidorLocal.Envelope(ServidorLocal.RetornoStatus));
        using var conexoes = Conexoes();
        var soap = DFe.Criar(new LeiauteFalso(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Urls = [new EnderecoServico(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, _servidor.Url())],
        });

        await soap.ExecutarAsync(new StatusFalso(), Ct);
        await Nucleo(conexoes).ExecutarAsync(new RestFalso(), Ct);

        Assert.Equal(1, conexoes.Soap!.Handlers);
        Assert.Equal(2, _servidor.Pedidos.Count);
    }
}
