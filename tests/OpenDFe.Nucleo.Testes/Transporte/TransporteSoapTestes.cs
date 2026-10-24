using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenDFe.Testes;

/// <summary>Um Kestrel local por classe de teste (<see cref="ServidorLocal"/>).</summary>
public sealed class ServidorLocalFixture : IAsyncLifetime
{
    internal ServidorLocal Servidor { get; private set; } = null!;

    public async ValueTask InitializeAsync() => Servidor = await ServidorLocal.IniciarAsync();

    public async ValueTask DisposeAsync() => await Servidor.DisposeAsync();
}

/// <summary>
/// O transporte da 4.1 pelo caminho inteiro do <see cref="DFe.ExecutarAsync{TRetorno}"/>, com o leiaute falso e o
/// <see cref="ConexoesSefaz"/> real contra um Kestrel local com TLS mútuo: o envelope e a ação, o certificado do
/// cliente, cada falha com a espécie e se pode ter sido processada, os tempos, o pool e a confiança padrão.
/// </summary>
public sealed class TransporteSoapTestes : IClassFixture<ServidorLocalFixture>
{
    private const string Cnpj = "11222333000181";
    private const string OutroCnpj = "12345678000195";

    // Recarregados do PKCS#12: o SChannel não usa a chave efêmera no TLS.
    private static readonly X509Certificate2 Certificado = Abrir(Cnpj);
    private static readonly X509Certificate2 OutroCertificado = Abrir(OutroCnpj);

    private readonly ServidorLocal _servidor;

    public TransporteSoapTestes(ServidorLocalFixture fixture)
    {
        _servidor = fixture.Servidor;
        _servidor.Responder = contexto => ServidorLocal.EscreverAsync(contexto, 200, ServidorLocal.Envelope(ServidorLocal.RetornoStatus));
        _servidor.Pedidos.Clear();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static X509Certificate2 Abrir(string cnpj, string nome = "EMPRESA DE TESTE LTDA") =>
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = cnpj, Nome = nome }.GerarPfx("teste"), "teste");

    private ConexoesSefaz Conexoes(Action<ConexaoSefaz>? criada = null, TimeProvider? relogio = null) =>
        new(ConexaoTeste.Fabrica(_servidor.Autoridade, criada), relogio ?? TimeProvider.System);

    private DFe Nucleo(ConexoesSefaz conexoes, Uri? url = null, X509Certificate2? certificado = null) =>
        DFe.Criar(new LeiauteFalso(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado ?? Certificado,
            Conexoes = conexoes,
            Urls = [new EnderecoServico(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, url ?? _servidor.Url())],
        });

    // O caminho feliz

    [Fact]
    public async Task TransmiteOEnvelopeSoap12ComAAcaoEOCertificadoDoCliente()
    {
        using var conexoes = Conexoes();

        var resultado = await Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(107, resultado.CStat);
        Assert.Equal("local", resultado.Retorno.Host);
        Assert.Equal(ServidorLocal.RetornoStatus, resultado.Rastro.XmlRetorno);
        Assert.Equal(_servidor.Url(), resultado.Rastro.Url);

        var pedido = Assert.Single(_servidor.Pedidos);
        Assert.Equal("/MG/StatusFalso", pedido.Caminho);
        Assert.Equal("application/soap+xml; charset=utf-8; action=\"urn:opendfe:teste:wsdl/status\"", pedido.Tipo);
        Assert.Equal(Certificado.Thumbprint, pedido.Cliente);
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", pedido.Corpo, StringComparison.Ordinal);
        var envelope = XDocument.Parse(pedido.Corpo).Root!;
        Assert.Equal(XName.Get("Envelope", EnvelopeSoap.NamespaceSoap), envelope.Name);
        var corpo = Assert.Single(envelope.Elements());
        Assert.Equal(XName.Get("Body", EnvelopeSoap.NamespaceSoap), corpo.Name);
        var dados = Assert.Single(corpo.Elements());
        Assert.Equal(XName.Get("nfeDadosMsg", LeiauteFalso.NamespaceWsdl), dados.Name);
        Assert.Equal(resultado.Rastro.XmlPedido, Assert.Single(dados.Elements()).ToString(SaveOptions.DisableFormatting));
    }

    // As falhas da resposta

    public static TheoryData<string, TipoFalhaTransmissao, bool> Respostas => new()
    {
        { "403 html", TipoFalhaTransmissao.CertificadoRecusado, false },
        { "404 html", TipoFalhaTransmissao.Http, false },
        { "500 html", TipoFalhaTransmissao.Http, true },
        { "502 vazio", TipoFalhaTransmissao.Http, true },
        { "400 Sender", TipoFalhaTransmissao.FalhaSoap, false },
        { "500 VersionMismatch", TipoFalhaTransmissao.FalhaSoap, false },
        { "500 Receiver", TipoFalhaTransmissao.FalhaSoap, true },
        { "200 Receiver", TipoFalhaTransmissao.FalhaSoap, true },
        { "200 html", TipoFalhaTransmissao.RespostaIlegivel, true },
        { "200 sem nfeResultMsg", TipoFalhaTransmissao.RespostaIlegivel, true },
        { "200 nfeResultMsg vazio", TipoFalhaTransmissao.RespostaIlegivel, true },
        { "200 cortado", TipoFalhaTransmissao.RespostaIlegivel, true },
        { "500 envelope cortado", TipoFalhaTransmissao.Http, true },
        { "200 leiaute não lê", TipoFalhaTransmissao.RespostaIlegivel, true },
    };

    [Theory]
    [MemberData(nameof(Respostas))]
    public async Task CadaRespostaFalhaComAEspecieCerta(string caso, TipoFalhaTransmissao tipo, bool podeTerSidoProcessado)
    {
        var (status, corpo, contentType) = caso switch
        {
            "403 html" => (403, "<html><body>403 - Forbidden: Access is denied.</body></html>", "text/html"),
            "404 html" => (404, "<html><body>Not Found</body></html>", "text/html"),
            "500 html" => (500, "<html><body>Erro</body></html>", "text/html"),
            "502 vazio" => (502, "", "text/plain"),
            "400 Sender" => (400, ServidorLocal.FalhaSoap("Sender", "Envelope invalido"), null),
            "500 VersionMismatch" => (500, ServidorLocal.FalhaSoap("VersionMismatch", "Versao do SOAP"), null),
            "500 Receiver" => (500, ServidorLocal.FalhaSoap("Receiver", "Erro interno"), null),
            "200 Receiver" => (200, ServidorLocal.FalhaSoap("Receiver", "Erro interno"), null),
            "200 html" => (200, "<html><body>Manutencao</body></html>", "text/html"),
            "200 sem nfeResultMsg" => (200, "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><outro/></soap:Body></soap:Envelope>", null),
            "200 nfeResultMsg vazio" => (200, ServidorLocal.Envelope(""), null),
            "200 cortado" => (200, ServidorLocal.Envelope(ServidorLocal.RetornoStatus)[..90], null),
            "500 envelope cortado" => (500, ServidorLocal.Envelope(ServidorLocal.RetornoStatus)[..90], null),
            "200 leiaute não lê" => (200, ServidorLocal.Envelope("<retStatus><xMotivo>sem cStat</xMotivo></retStatus>"), null),
            _ => throw new ArgumentOutOfRangeException(nameof(caso)),
        };
        _servidor.Responder = contexto => contentType is null
            ? ServidorLocal.EscreverAsync(contexto, status, corpo)
            : ServidorLocal.EscreverAsync(contexto, status, corpo, contentType);
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct));

        Assert.Equal(tipo, falha.Tipo);
        Assert.Equal(podeTerSidoProcessado, falha.PodeTerSidoProcessado);
        Assert.NotNull(falha.InnerException);
        Assert.NotNull(falha.Rastro.XmlPedido);
        Assert.Equal(Destino.MG, falha.Rastro.Autorizador);
        Assert.NotNull(falha.Rastro.DuracaoTransmissao);
        if (caso == "200 leiaute não lê")
            Assert.Equal("<retStatus><xMotivo>sem cStat</xMotivo></retStatus>", falha.Rastro.XmlRetorno);
        else
            Assert.Equal(corpo, falha.Rastro.XmlRetorno);
        if (tipo is TipoFalhaTransmissao.Http or TipoFalhaTransmissao.CertificadoRecusado)
            Assert.Equal((HttpStatusCode)status, Assert.IsType<HttpRequestException>(falha.InnerException).StatusCode);
        if (tipo == TipoFalhaTransmissao.FalhaSoap)
            Assert.Contains(XElement.Parse(corpo).Descendants().Single(e => e.Name.LocalName == "Text").Value, falha.Message, StringComparison.Ordinal);
    }

    // O serviço com a operação no Body (a distribuição DF-e, NT 2014.002)

    private DFe NucleoOperacao(ConexoesSefaz conexoes) =>
        DFe.Criar(new LeiauteFalso { Acrescimo = OperacaoEnvelopeFalsa.Declarar }, Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Urls = [new EnderecoServico(Destino.AN, Ambiente.Homologacao, OperacaoEnvelopeFalsa.Servico, _servidor.Url(OperacaoEnvelopeFalsa.Servico))],
        });

    [Fact]
    public async Task ComOperacaoOPedidoVaiDentroDoElementoDaOperacaoEARespostaSeLeNoResult()
    {
        _servidor.Responder = contexto => ServidorLocal.EscreverAsync(contexto, 200, OperacaoEnvelopeFalsa.Envelope(ServidorLocal.RetornoStatus));
        using var conexoes = Conexoes();

        var resultado = await NucleoOperacao(conexoes).ExecutarAsync(new StatusOperacaoFalso(), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(107, resultado.CStat);
        Assert.Equal("local", resultado.Retorno.Host);
        var pedido = Assert.Single(_servidor.Pedidos);
        Assert.Equal("application/soap+xml; charset=utf-8; action=\"urn:opendfe:teste:wsdl/operacao\"", pedido.Tipo);
        var corpo = XDocument.Parse(pedido.Corpo).Root!.Element(XName.Get("Body", EnvelopeSoap.NamespaceSoap))!;
        var operacao = Assert.Single(corpo.Elements());
        Assert.Equal(XName.Get(OperacaoEnvelopeFalsa.Operacao, LeiauteFalso.NamespaceWsdl), operacao.Name);
        var dados = Assert.Single(operacao.Elements());
        Assert.Equal(XName.Get("nfeDadosMsg", LeiauteFalso.NamespaceWsdl), dados.Name);
        Assert.Equal(resultado.Rastro.XmlPedido, Assert.Single(dados.Elements()).ToString(SaveOptions.DisableFormatting));
    }

    [Fact]
    public async Task ComOperacaoORetornoNoNfeResultMsgDiretoERespostaIlegivel()
    {
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => NucleoOperacao(conexoes).ExecutarAsync(new StatusOperacaoFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
    }

    // A rede e os tempos

    [Fact]
    public async Task OTempoDeRespostaEsgotadoPodeTerSidoProcessado()
    {
        _servidor.Responder = contexto => Task.Delay(TimeSpan.FromSeconds(30), contexto.RequestAborted);
        using var conexoes = new ConexoesSefaz(ConexaoTeste.Fabrica(_servidor.Autoridade)) { TempoResposta = TimeSpan.FromMilliseconds(500) };

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.TempoEsgotado, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Null(falha.Rastro.XmlRetorno);
    }

    [Fact]
    public async Task OCancelamentoDeQuemChamaNaoViraFalha()
    {
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _servidor.Responder = async contexto =>
        {
            chegou.TrySetResult();
            await Task.Delay(TimeSpan.FromSeconds(30), contexto.RequestAborted);
        };
        using var conexoes = Conexoes();
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(Ct);

        var execucao = Nucleo(conexoes).ExecutarAsync(new StatusFalso(), cancelamento.Token);
        await chegou.Task.WaitAsync(Ct);
        await cancelamento.CancelAsync();

        var erro = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execucao);
        Assert.IsNotType<FalhaTransmissaoException>(erro);
    }

    [Fact]
    public async Task AConexaoDerrubadaDepoisDoEnvioPodeTerSidoProcessada()
    {
        _servidor.Responder = contexto =>
        {
            contexto.Abort();
            return Task.CompletedTask;
        };
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
    }

    [Fact]
    public async Task AConexaoRecusadaNaoFoiProcessada()
    {
        var livre = new TcpListener(IPAddress.Loopback, 0);
        livre.Start();
        var porta = ((IPEndPoint)livre.LocalEndpoint).Port;
        livre.Stop();
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Nucleo(conexoes, new Uri($"https://127.0.0.1:{porta}/MG/StatusFalso")).ExecutarAsync(new StatusFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.False(falha.PodeTerSidoProcessado);
        Assert.IsType<HttpRequestException>(falha.InnerException);
    }

    [Fact]
    public async Task ONomeQueNaoResolveNaoFoiProcessado()
    {
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Nucleo(conexoes, new Uri("https://autorizador.invalid/MG/StatusFalso")).ExecutarAsync(new StatusFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.False(falha.PodeTerSidoProcessado);
    }

    // A confiança

    [Fact]
    public async Task OPadraoRecusaOServidorDeUmaACDesconhecida()
    {
        using var conexoes = new ConexoesSefaz();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Tls, falha.Tipo);
        Assert.False(falha.PodeTerSidoProcessado);
        Assert.Empty(_servidor.Pedidos);
    }

    [Fact]
    public async Task AFabricaQueDevolveNuloFalhaSemTransmitir()
    {
        using var conexoes = new ConexoesSefaz(_ => null!);

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.False(falha.PodeTerSidoProcessado);
        Assert.IsType<InvalidOperationException>(falha.InnerException);
    }

    // O pool

    [Fact]
    public async Task UmHandlerPorCertificadoCompartilhadoEntreOsNucleos()
    {
        var criadas = new List<ConexaoSefaz>();
        using var conexoes = Conexoes(c => criadas.Add(c));

        await Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct);
        await Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct);
        await Nucleo(conexoes, certificado: OutroCertificado).ExecutarAsync(new StatusFalso(), Ct);

        Assert.Equal(2, criadas.Count);
        Assert.Equal(Certificado.Thumbprint, criadas[0].Certificado.Thumbprint);
        Assert.NotSame(Certificado, criadas[0].Certificado);
        Assert.Null(criadas[0].Cifras);
        Assert.Equal(OutroCertificado.Thumbprint, criadas[1].Certificado.Thumbprint);
        Assert.Equal(2, Transporte(conexoes).Handlers);
        Assert.Equal([Certificado.Thumbprint, Certificado.Thumbprint, OutroCertificado.Thumbprint], _servidor.Pedidos.Select(p => p.Cliente));
    }

    [Fact]
    public async Task OPoolGuardaACopiaDoCertificado()
    {
        var criadas = 0;
        using var conexoes = Conexoes(_ => criadas++);
        var pfx = new CertificadoTeste { Cnpj = Cnpj }.GerarPfx("teste");
        var primeiro = CertificadoDigital.AbrirPfx(pfx, "teste");
        await Nucleo(conexoes, certificado: primeiro).ExecutarAsync(new StatusFalso(), Ct);

        // Quem usa descarta o seu e abre de novo (o mesmo certificado): o handler do pool continua servindo.
        primeiro.Dispose();
        using var segundo = CertificadoDigital.AbrirPfx(pfx, "teste");
        var resultado = await Nucleo(conexoes, certificado: segundo).ExecutarAsync(new StatusFalso(), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(1, criadas);
        Assert.Equal(2, _servidor.Pedidos.Count);
    }

    [Fact]
    public async Task OHandlerOciosoEDescartadoEOEmUsoNao()
    {
        var relogio = new RelogioFalso(DateTimeOffset.UnixEpoch);
        var criadas = new List<string>();
        using var conexoes = Conexoes(c => criadas.Add(c.Certificado.Thumbprint), relogio);
        var transporte = Transporte(conexoes);
        var liberar = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var chegou = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _servidor.Responder = async contexto =>
        {
            var cliente = await contexto.Connection.GetClientCertificateAsync(contexto.RequestAborted);
            if (cliente?.Thumbprint == OutroCertificado.Thumbprint && !liberar.Task.IsCompleted)
            {
                chegou.TrySetResult();
                await liberar.Task.WaitAsync(contexto.RequestAborted);
            }
            await ServidorLocal.EscreverAsync(contexto, 200, ServidorLocal.Envelope(ServidorLocal.RetornoStatus));
        };

        // O do certificado fica ocioso: descartado na transmissão seguinte, de outro certificado.
        await Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct);
        relogio.Avancar(conexoes.TempoOcioso + TimeSpan.FromSeconds(1));
        var emUso = Nucleo(conexoes, certificado: OutroCertificado).ExecutarAsync(new StatusFalso(), Ct);
        await chegou.Task.WaitAsync(Ct);
        Assert.Equal(1, transporte.Handlers);

        // O outro, em uso além do tempo ocioso, não é descartado pela transmissão do primeiro.
        relogio.Avancar(conexoes.TempoOcioso + TimeSpan.FromSeconds(1));
        await Nucleo(conexoes).ExecutarAsync(new StatusFalso(), Ct);
        Assert.Equal(2, transporte.Handlers);
        liberar.SetResult();
        Assert.True((await emUso).Processado);

        Assert.Equal([Certificado.Thumbprint, OutroCertificado.Thumbprint, Certificado.Thumbprint], criadas);
    }

    [Fact]
    public async Task AsConexoesDescartadasRecusamATransmissao()
    {
        var conexoes = Conexoes();
        var dfe = Nucleo(conexoes);
        await dfe.ExecutarAsync(new StatusFalso(), Ct);

        conexoes.Dispose();
        conexoes.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => dfe.ExecutarAsync(new StatusFalso(), Ct));
        Assert.Equal(0, Transporte(conexoes).Handlers);
    }

    private static TransporteSoap Transporte(ConexoesSefaz conexoes) => Assert.IsType<TransporteSoap>(conexoes.Soap);
}
