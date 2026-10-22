using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

/// <summary>
/// A forma do <see cref="ConexoesSefaz"/> e do <see cref="ConexaoSefaz"/>, os serviços SOAP e as cifras na declaração,
/// e o caminho deles até o transporte (com o transporte falso).
/// </summary>
public class ConexoesTestes
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("teste"), "teste");

    private static readonly TlsCipherSuite[] CifrasAntigas =
        [TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384, TlsCipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA256];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ConfiguracaoDFe Configuracao(ConexoesSefaz conexoes, IReadOnlyList<EnderecoServico>? urls = null) => new()
    {
        Ambiente = Ambiente.Homologacao,
        UF = UF.MG,
        Certificado = Certificado,
        Conexoes = conexoes,
        Urls = urls ?? [],
    };

    [Fact]
    public void OsTemposTemOsPadroesESoAceitamPositivos()
    {
        using var conexoes = new ConexoesSefaz();

        Assert.Equal(TimeSpan.FromSeconds(15), conexoes.TempoConexao);
        Assert.Equal(TimeSpan.FromSeconds(60), conexoes.TempoResposta);
        Assert.Equal(TimeSpan.FromMinutes(30), conexoes.TempoOcioso);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConexoesSefaz { TempoConexao = TimeSpan.Zero });
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConexoesSefaz { TempoResposta = TimeSpan.FromSeconds(-1) });
        var erro = Assert.Throws<ArgumentOutOfRangeException>(() => new ConexoesSefaz { TempoOcioso = TimeSpan.Zero });
        Assert.Equal("TempoOcioso", erro.ParamName);
        Assert.Throws<ArgumentNullException>(() => new ConexoesSefaz((Func<ConexaoSefaz, HttpMessageHandler>)null!));
    }

    [Fact]
    public void OHandlerPadraoTemOTlsDaLib()
    {
        using var conexoes = new ConexoesSefaz { TempoConexao = TimeSpan.FromSeconds(7) };
        var conexao = new ConexaoSefaz(Certificado, null, conexoes.TempoConexao);

        using var handler = conexao.CriarPadrao();

        Assert.Equal(TimeSpan.FromSeconds(7), handler.ConnectTimeout);
        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseCookies);
        var tls = handler.SslOptions;
        Assert.Equal(SslProtocols.Tls12 | SslProtocols.Tls13, tls.EnabledSslProtocols);
        Assert.Equal(X509RevocationMode.NoCheck, tls.CertificateRevocationCheckMode);
        Assert.NotNull(tls.ClientCertificateContext);
        Assert.Equal(Certificado.Thumbprint, tls.ClientCertificateContext.TargetCertificate.Thumbprint);
        Assert.NotNull(tls.RemoteCertificateValidationCallback);
        Assert.Null(tls.CipherSuitesPolicy);
    }

    [Fact]
    public void AsCifrasSoEntramNoLinux()
    {
        var conexao = new ConexaoSefaz(Certificado, CifrasAntigas, TimeSpan.FromSeconds(15));

        using var handler = conexao.CriarPadrao();

        Assert.Equal(CifrasAntigas, conexao.Cifras);
        if (OperatingSystem.IsLinux())
            Assert.Equal(CifrasAntigas, handler.SslOptions.CipherSuitesPolicy!.AllowedCipherSuites);
        else
            Assert.Null(handler.SslOptions.CipherSuitesPolicy);
    }

    // A declaração

    [Fact]
    public void ADeclaracaoListaOsServicosEAsCifras()
    {
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteFalso { Acrescimo = d => d.DeclararCifras(Destino.SVRS, CifrasAntigas) }, Documento.NFe);

        Assert.Equal([LeiauteFalso.SoapStatus, LeiauteFalso.SoapEco], declaracao.Servicos);
        var cifras = Assert.Single(declaracao.Cifras);
        Assert.Equal(Destino.SVRS, cifras.Key);
        Assert.Equal(CifrasAntigas, cifras.Value);
        Assert.Throws<InvalidOperationException>(() => declaracao.DeclararServico(new ServicoSoap("Outro", "urn:a", "urn:a/b")));
        Assert.Throws<InvalidOperationException>(() => declaracao.DeclararCifras(Destino.SP, CifrasAntigas));
    }

    public static TheoryData<string> Recusas => ["servico repetido", "servico nulo", "cifras vazias", "cifras nulas", "cifras repetidas", "destino desconhecido"];

    [Theory]
    [MemberData(nameof(Recusas))]
    public void ADeclaracaoRecusaServicoECifrasInvalidos(string caso)
    {
        Action<DeclaracaoLeiaute> acrescimo = caso switch
        {
            "servico repetido" => d => d.DeclararServico(new ServicoSoap(LeiauteFalso.ServicoStatus, "urn:outro", "urn:outro/x")),
            "servico nulo" => d => d.DeclararServico(null!),
            "cifras vazias" => d => d.DeclararCifras(Destino.MG, []),
            "cifras nulas" => d => d.DeclararCifras(Destino.MG, null!),
            "cifras repetidas" => d =>
            {
                d.DeclararCifras(Destino.MG, CifrasAntigas);
                d.DeclararCifras(Destino.MG, CifrasAntigas);
            },
            "destino desconhecido" => d => d.DeclararCifras((Destino)999, CifrasAntigas),
            _ => throw new ArgumentOutOfRangeException(nameof(caso)),
        };

        Assert.ThrowsAny<ArgumentException>(() => DeclaracaoLeiaute.Criar(new LeiauteFalso { Acrescimo = acrescimo }, Documento.NFe));
    }

    [Fact]
    public void OCriarRecusaAUrlDeUmServicoSemOEnvelope()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        var trocada = new EnderecoServico(Destino.MG, Ambiente.Homologacao, "ServicoNovo", new Uri("https://mg.exemplo/novo"));

        var erro = Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configuracao(conexoes, [trocada])));

        Assert.Contains("ServicoNovo", erro.Message, StringComparison.Ordinal);
        var doLeiaute = new LeiauteFalso
        {
            Acrescimo = d => d.DeclararEndereco(new EnderecoServico(Destino.SP, Ambiente.Producao, "ServicoAvulso", new Uri("https://sp.exemplo"))),
        };
        Assert.Throws<ArgumentException>(() => DFe.Criar(doLeiaute, Documento.NFe, Configuracao(conexoes)));
    }

    [Fact]
    public async Task OEnvioLevaOServicoSoapEAsCifrasDoAutorizador()
    {
        var transporte = new TransporteFalso();
        using var conexoes = new ConexoesSefaz(transporte);
        var leiaute = new LeiauteFalso { Acrescimo = d => d.DeclararCifras(Destino.MG, CifrasAntigas) };
        var dfe = DFe.Criar(leiaute, Documento.NFe, Configuracao(conexoes));

        await dfe.ExecutarAsync(new StatusFalso(), Ct);
        await dfe.ExecutarAsync(new EcoFalso(), Ct);
        await dfe.ExecutarAsync(new StatusFalso { Destino = Destino.SVRS }, Ct);

        var envios = transporte.Envios.ToArray();
        Assert.Same(LeiauteFalso.SoapStatus, envios[0].Soap);
        Assert.Equal(LeiauteFalso.ServicoStatus, envios[0].Servico);
        Assert.Equal(CifrasAntigas, envios[0].Cifras);
        Assert.Same(LeiauteFalso.SoapEco, envios[1].Soap);
        Assert.Null(envios[2].Cifras);
    }

    [Fact]
    public async Task OPoolSeparaOHandlerPelasCifras()
    {
        var criadas = new List<ConexaoSefaz>();
        using var conexoes = new ConexoesSefaz(c =>
        {
            criadas.Add(c);
            return new RespostaFixa();
        });
        var transporte = conexoes.Soap!;
        EnvioSefaz Envio(IReadOnlyList<TlsCipherSuite>? cifras) =>
            new(Certificado, Destino.MG, new Uri("https://mg.exemplo/status"), LeiauteFalso.SoapStatus, "<consStatus/>", cifras);

        Assert.Equal("<ok />", await transporte.EnviarAsync(Envio(null), Ct));
        await transporte.EnviarAsync(Envio(CifrasAntigas), Ct);
        await transporte.EnviarAsync(Envio([.. CifrasAntigas]), Ct);
        await transporte.EnviarAsync(Envio(null), Ct);

        Assert.Equal(2, criadas.Count);
        Assert.Null(criadas[0].Cifras);
        Assert.Equal(CifrasAntigas, criadas[1].Cifras);
    }

    // Um handler que responde sem rede: o envelope com <ok/>, depois de ler o pedido inteiro.
    private sealed class RespostaFixa : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await request.Content!.ReadAsByteArrayAsync(cancellationToken);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><nfeResultMsg><ok/></nfeResultMsg></soap:Body></soap:Envelope>"),
            };
        }
    }
}
