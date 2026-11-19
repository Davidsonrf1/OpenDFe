using System.IO.Compression;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace OpenDFe.Testes;

// O caminho REST do núcleo (a 9.2; docs/NFSE.md, D3, D4, D5 e D12) com o leiaute REST falso e o transporte falso: o Criar
// sem atendimento e sem o envelope SOAP, a montagem, a assinatura, o schema, as regras e os callbacks sobre o XML do pedido,
// o JSON com o XML compactado, a URL, a leitura e o rastro, a coerência do pedido e as falhas.
public class RestTestes
{
    private static readonly X509Certificate2 Certificado = new CertificadoTeste { Cnpj = "11222333000181" }.Gerar();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ConfiguracaoDFe Configuracao(ConexoesSefaz conexoes, IReadOnlyList<EnderecoServico>? urls = null,
        TipoEmissao emissao = TipoEmissao.Normal, ValidacaoCallback? validacao = null) => new()
    {
        Ambiente = Ambiente.Homologacao,
        UF = UF.SP,
        Certificado = Certificado,
        Conexoes = conexoes,
        Emissao = emissao,
        Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(DateTimeOffset.UtcNow, "Autorizador fora do ar"),
        Urls = urls ?? [],
        Validacao = validacao,
    };

    private static (DFe Nucleo, TransporteFalso Transporte) Criar(ValidacaoCallback? validacao = null, IReadOnlyList<EnderecoServico>? urls = null)
    {
        var transporte = new TransporteFalso();
        var conexoes = new ConexoesSefaz(transporte);
        return (DFe.Criar(new LeiauteRestFalso(), Documento.NFSe, Configuracao(conexoes, urls, validacao: validacao)), transporte);
    }

    private static string Descompactar(string base64)
    {
        using var entrada = new MemoryStream(Convert.FromBase64String(base64));
        using var gzip = new GZipStream(entrada, CompressionMode.Decompress);
        using var leitor = new StreamReader(gzip, new UTF8Encoding(false, true));
        return leitor.ReadToEnd();
    }

    // Criar

    [Fact]
    public void CriaONucleoSemAtendimentoESemOEnvelopeSoapNasUrlsDaSefin()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());

        var dfe = DFe.Criar(new LeiauteRestFalso(), Documento.NFSe, Configuracao(conexoes));

        Assert.Empty(dfe.Atendimentos);
        Assert.Equal(2, dfe.Enderecos.Count);
        Assert.All(dfe.Enderecos, e => Assert.Equal(Destino.SefinNacional, e.Autorizador));
        Assert.True(dfe.Suporta<RestFalso>());
        Assert.Equal(typeof(RestFalso), Assert.Single(dfe.Operacoes).Parametro);
    }

    [Theory]
    [InlineData(TipoEmissao.ContingenciaSvcAN)]
    [InlineData(TipoEmissao.ContingenciaOffline)]
    public void ANFSeNaoTemContingencia(TipoEmissao emissao)
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());

        var erro = Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteRestFalso(), Documento.NFSe, Configuracao(conexoes, emissao: emissao)));

        Assert.Contains("contingência", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AUrlTrocadaDaSefinEDoAdnNaoPedeOEnvelopeSoapEADeOutroDestinoPede()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        var adn = new EnderecoServico(Destino.ADN, Ambiente.Homologacao, "Parametrizacao", new Uri("https://adn.exemplo/parametrizacao"));
        var mg = new EnderecoServico(Destino.MG, Ambiente.Homologacao, LeiauteRestFalso.Servico, new Uri("https://mg.exemplo/base"));

        Assert.NotNull(DFe.Criar(new LeiauteRestFalso(), Documento.NFSe, Configuracao(conexoes, [adn])));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteRestFalso(), Documento.NFSe, Configuracao(conexoes, [mg])));
    }

    [Fact]
    public void ORegistroRecusaORepetidoEntreSoapERestEOAbstrato()
    {
        Assert.Throws<ArgumentException>(() => DeclaracaoLeiaute.Criar(
            new LeiauteRestFalso { Acrescimo = d => d.RegistrarOperacao(new OperacaoRestFalsa()) }, Documento.NFSe));
        Assert.Throws<ArgumentException>(() => DeclaracaoLeiaute.Criar(
            new LeiauteFalso { Acrescimo = d => d.RegistrarOperacao(new OperacaoRestStatusFalsa()) }, Documento.NFe));
        Assert.Throws<ArgumentException>(() => DeclaracaoLeiaute.Criar(
            new LeiauteRestFalso { Acrescimo = d => d.RegistrarOperacao(new OperacaoRestAbstrataFalsa()) }, Documento.NFSe));
        Assert.Throws<ArgumentNullException>(() => DeclaracaoLeiaute.Criar(
            new LeiauteRestFalso { Acrescimo = d => d.RegistrarOperacao((IOperacaoRest<RestFalso, RetornoRestFalso>)null!) }, Documento.NFSe));
    }

    [Fact]
    public void ORegistroRestSoValeDentroDoDeclarar()
    {
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteRestFalso(), Documento.NFSe);

        Assert.Throws<InvalidOperationException>(() => declaracao.RegistrarOperacao(new OperacaoRestStatusFalsa()));
    }

    // O POST

    [Fact]
    public async Task OPostAssinaValidaECompactaOXmlNoCampoDoJson()
    {
        var (dfe, transporte) = Criar();
        transporte.ResponderRest = (envio, _) =>
        {
            envio.MarcarEnviado();
            return Task.FromResult(new RespostaRest(201, "{\"chaveAcesso\": \"123\"}"));
        };

        var resultado = await dfe.ExecutarAsync(new RestFalso { Texto = "olá & até" }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(201, resultado.CStat);
        Assert.Equal("ok", resultado.Motivo);
        var envio = Assert.Single(transporte.EnviosRest);
        Assert.Equal(MetodoRest.Post, envio.Metodo);
        Assert.Equal(new Uri("https://sefin.hom.exemplo/Base/eco"), envio.Url);
        Assert.Equal(Destino.SefinNacional, envio.Autorizador);
        Assert.Equal(LeiauteRestFalso.Servico, envio.Servico);
        Assert.Same(Certificado, envio.Certificado);

        using var json = JsonDocument.Parse(envio.Corpo!);
        var propriedade = Assert.Single(json.RootElement.EnumerateObject());
        Assert.Equal(LeiauteRestFalso.Campo, propriedade.Name);
        var xml = Descompactar(propriedade.Value.GetString()!);
        Assert.Equal(resultado.Rastro.XmlPedido, xml);
        Assert.Equal(xml, resultado.Retorno.XmlPedido);
        Assert.Empty(AssinaturaDigital.Verificar(xml, "ECO1", "11222333000181"));
        Assert.Contains("olá &amp; até", xml, StringComparison.Ordinal);

        Assert.Equal(201, resultado.Rastro.StatusHttp);
        Assert.Equal("{\"chaveAcesso\": \"123\"}", resultado.Rastro.XmlRetorno);
        Assert.Equal(envio.Url, resultado.Rastro.Url);
        Assert.Equal(Destino.SefinNacional, resultado.Rastro.Autorizador);
        Assert.NotNull(resultado.Rastro.DuracaoTransmissao);
    }

    [Fact]
    public void OCorpoEUtf8SemBomEmGZip()
    {
        var corpo = CorpoRest.Montar("campo", "<a>ç</a>");

        using var json = JsonDocument.Parse(corpo);
        var bytes = Convert.FromBase64String(json.RootElement.GetProperty("campo").GetString()!);
        Assert.Equal(0x1f, bytes[0]);
        Assert.Equal(0x8b, bytes[1]);
        using var gzip = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
        using var saida = new MemoryStream();
        gzip.CopyTo(saida);
        Assert.Equal(Encoding.UTF8.GetBytes("<a>ç</a>"), saida.ToArray());
    }

    [Fact]
    public async Task OAmbienteDeProducaoUsaABaseDeProducaoEODestinoDoParametroSobrepoe()
    {
        var transporte = new TransporteFalso();
        var conexoes = new ConexoesSefaz(transporte);
        var configuracao = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Producao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Urls = [new EnderecoServico(Destino.ADN, Ambiente.Producao, LeiauteRestFalso.Servico, new Uri("https://adn.exemplo/outra/"))],
        };
        var dfe = DFe.Criar(new LeiauteRestFalso(), Documento.NFSe, configuracao);

        await dfe.ExecutarAsync(new RestFalso { Metodo = MetodoRest.Get, Caminho = "nfse/1" }, Ct);
        await dfe.ExecutarAsync(new RestFalso { Metodo = MetodoRest.Get, Caminho = "nfse/2", Destino = Destino.ADN }, Ct);

        var envios = transporte.EnviosRest.ToArray();
        Assert.Equal(new Uri("https://sefin.prod.exemplo/Base/nfse/1"), envios[0].Url);
        Assert.Equal(new Uri("https://adn.exemplo/outra/nfse/2"), envios[1].Url);
        Assert.Equal(Destino.ADN, envios[1].Autorizador);
    }

    // O GET e o HEAD

    [Theory]
    [InlineData(MetodoRest.Get)]
    [InlineData(MetodoRest.Head)]
    public async Task AConsultaVaiSemCorpoEOLerRecebeOXmlNulo(MetodoRest metodo)
    {
        var (dfe, transporte) = Criar();
        transporte.ResponderRest = (_, _) => Task.FromResult(new RespostaRest(404, null));

        var resultado = await dfe.ExecutarAsync(new RestFalso { Metodo = metodo, Caminho = "dps/DPS123" }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(404, resultado.CStat);
        Assert.Null(resultado.Retorno.XmlPedido);
        Assert.Null(resultado.Retorno.Corpo);
        Assert.Null(resultado.Rastro.XmlPedido);
        Assert.Null(resultado.Rastro.XmlRetorno);
        Assert.Equal(404, resultado.Rastro.StatusHttp);
        var envio = Assert.Single(transporte.EnviosRest);
        Assert.Equal(metodo, envio.Metodo);
        Assert.Null(envio.Corpo);
        Assert.Equal(new Uri("https://sefin.hom.exemplo/Base/dps/DPS123"), envio.Url);
    }

    // A recusa local

    [Fact]
    public async Task OProblemaDaMontagemRecusaSemTransmitir()
    {
        var (dfe, transporte) = Criar();

        var resultado = await dfe.ExecutarAsync(new RestFalso { ProblemaMontagem = new Problema("M01", Severidade.Erro, "falta") }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal("M01", Assert.Single(resultado.Problemas).Codigo);
        Assert.NotNull(resultado.Rastro.XmlPedido);
        Assert.DoesNotContain("Signature", resultado.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Null(resultado.Rastro.StatusHttp);
        Assert.Empty(transporte.EnviosRest);
    }

    [Fact]
    public async Task OAvisoDaMontagemNaoRecusa()
    {
        var (dfe, _) = Criar();

        var resultado = await dfe.ExecutarAsync(new RestFalso { ProblemaMontagem = new Problema("M02", Severidade.Aviso, "atenção") }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal("M02", Assert.Single(resultado.Problemas).Codigo);
    }

    [Fact]
    public async Task OSchemaRecusaOXmlAssinadoSemTransmitir()
    {
        var (dfe, transporte) = Criar();

        var resultado = await dfe.ExecutarAsync(new RestFalso { XmlBruto = $"<outro xmlns=\"{LeiauteFalso.Namespace}\"/>" }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal(LeiauteFalso.CodigoSchema, Assert.Single(resultado.Problemas).Codigo);
        Assert.Empty(transporte.EnviosRest);
    }

    [Fact]
    public async Task OsCallbacksRecebemODocumentoDoPedidoERecusam()
    {
        var recebidos = new List<DocumentoVerificado>();
        var (dfe, transporte) = Criar(new ValidacaoCallback
        {
            Documento = d =>
            {
                recebidos.Add(d);
                return [new Problema("CB01", Severidade.Erro, "não")];
            },
        });

        var resultado = await dfe.ExecutarAsync(new RestFalso { MontarDocumento = true, Texto = "abc" }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal("CB01", Assert.Single(resultado.Problemas).Codigo);
        var documento = Assert.Single(recebidos);
        Assert.Equal(Documento.NFSe, documento.Documento);
        Assert.Equal("abc", documento.Modelo);
        Assert.Contains("<Signature", documento.Xml, StringComparison.Ordinal);
        Assert.Empty(transporte.EnviosRest);
    }

    // A coerência do pedido

    [Theory]
    [InlineData("post sem xml")]
    [InlineData("get com xml")]
    [InlineData("head com xml")]
    [InlineData("sem schema")]
    [InlineData("sem campo")]
    [InlineData("sem destino")]
    public async Task OPedidoIncoerenteEErroDeProgramacao(string caso)
    {
        var parametro = caso switch
        {
            "post sem xml" => new RestFalso { IncluirXml = false },
            "get com xml" => new RestFalso { Metodo = MetodoRest.Get, IncluirXml = true },
            "head com xml" => new RestFalso { Metodo = MetodoRest.Head, IncluirXml = true },
            "sem schema" => new RestFalso { OmitirSchema = true },
            "sem campo" => new RestFalso { OmitirCampo = true },
            _ => new RestFalso { OmitirDestino = true },
        };
        var (dfe, transporte) = Criar();

        await Assert.ThrowsAsync<InvalidOperationException>(() => dfe.ExecutarAsync(parametro, Ct));

        Assert.Empty(transporte.EnviosRest);
    }

    [Fact]
    public async Task ODestinoDoParametroSupreOQueFaltaNoPedido()
    {
        var (dfe, transporte) = Criar();

        var resultado = await dfe.ExecutarAsync(new RestFalso { OmitirDestino = true, Destino = Destino.SefinNacional }, Ct);

        Assert.True(resultado.Processado);
        Assert.Single(transporte.EnviosRest);
    }

    [Fact]
    public async Task ODestinoSemUrlNaoESuportado()
    {
        var (dfe, transporte) = Criar();

        await Assert.ThrowsAsync<NotSupportedException>(() => dfe.ExecutarAsync(new RestFalso { Destino = Destino.ADN }, Ct));

        Assert.Empty(transporte.EnviosRest);
    }

    // As falhas

    [Fact]
    public async Task AFalhaClassificadaDoTransporteVaiComOStatusEOCorpoNoRastro()
    {
        var (dfe, transporte) = Criar();
        transporte.ResponderRest = (envio, _) =>
        {
            envio.MarcarEnviado();
            throw new FalhaTransporte(TipoFalhaTransmissao.Http, true, "HTTP 503", new HttpRequestException("503"), "{}", 503);
        };

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new RestFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Http, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(503, falha.Rastro.StatusHttp);
        Assert.Equal("{}", falha.Rastro.XmlRetorno);
        Assert.Contains("<Signature", falha.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Equal(new Uri("https://sefin.hom.exemplo/Base/eco"), falha.Rastro.Url);
        Assert.Contains("POST eco", falha.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AFalhaSemClassificacaoUsaOEnvio(bool enviado)
    {
        var (dfe, transporte) = Criar();
        transporte.ResponderRest = (envio, _) =>
        {
            if (enviado)
                envio.MarcarEnviado();
            throw new IOException("caiu");
        };

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new RestFalso(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.Equal(enviado, falha.PodeTerSidoProcessado);
        Assert.Null(falha.Rastro.StatusHttp);
    }

    [Theory]
    [InlineData(MetodoRest.Post, true)]
    [InlineData(MetodoRest.Get, false)]
    public async Task ARespostaQueOLeiauteNaoLeEIlegivelESoOPostPodeTerSidoProcessado(MetodoRest metodo, bool processado)
    {
        var (dfe, transporte) = Criar();
        transporte.ResponderRest = (envio, _) => Task.FromResult(new RespostaRest(200, "{ quebrado"));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new RestFalso { Metodo = metodo }, Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.Equal(processado, falha.PodeTerSidoProcessado);
        Assert.IsAssignableFrom<JsonException>(falha.InnerException);
        Assert.Equal(200, falha.Rastro.StatusHttp);
        Assert.Equal("{ quebrado", falha.Rastro.XmlRetorno);
    }

    [Fact]
    public async Task AXmlExceptionEAFormatExceptionDaLeituraTambemSaoIlegiveisEOutraSobe()
    {
        var (dfe, _) = Criar();

        await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new RestFalso { ExcecaoLeitura = new XmlException("x") }, Ct));
        await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new RestFalso { ExcecaoLeitura = new FormatException("f") }, Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => dfe.ExecutarAsync(new RestFalso { ExcecaoLeitura = new ArgumentException("a") }, Ct));
    }

    [Fact]
    public async Task OCancelamentoAntesDeTransmitirNaoTransmite()
    {
        var (dfe, transporte) = Criar();
        using var cancelado = new CancellationTokenSource();
        await cancelado.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dfe.ExecutarAsync(new RestFalso(), cancelado.Token));

        Assert.Empty(transporte.EnviosRest);
    }

    // Os tipos

    [Fact]
    public void OPedidoRecusaOCaminhoAbsolutoEOsValoresInvalidos()
    {
        Assert.Throws<ArgumentException>(() => new PedidoRest("", MetodoRest.Get, "x"));
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Get, " "));
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Get, "/nfse"));
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Get, "https://outro/nfse"));
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Get, "nfse?x=1"));
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Get, "nfse#x"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PedidoRest("s", (MetodoRest)9, "nfse"));
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Post, "nfse") { CampoXml = " " });
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Post, "nfse") { IdsAssinatura = [""] });
        Assert.Throws<ArgumentOutOfRangeException>(() => new PedidoRest("s", MetodoRest.Post, "nfse") { Destino = (Destino)99 });
        Assert.Throws<ArgumentNullException>(() => new PedidoRest("s", MetodoRest.Post, "nfse") { Problemas = [null!] });
        Assert.Throws<ArgumentException>(() => new PedidoRest("s", MetodoRest.Post, "nfse") { Documentos = [new("A"), new("A")] });

        var pedido = new PedidoRest("SefinNacional", MetodoRest.Head, "dps/DPS1");
        Assert.Equal("SefinNacional", pedido.Servico);
        Assert.Equal(MetodoRest.Head, pedido.Metodo);
        Assert.Equal("dps/DPS1", pedido.Caminho);
        Assert.Null(pedido.Xml);
        Assert.Empty(pedido.IdsAssinatura);
    }

    [Fact]
    public void ARespostaGuardaOStatusEOCorpoVazioComoNulo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RespostaRest(99, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RespostaRest(600, null));
        Assert.Null(new RespostaRest(200, "").Corpo);
        Assert.Equal("{}", new RespostaRest((int)HttpStatusCode.BadRequest, "{}").Corpo);
    }
}

internal sealed class OperacaoRestStatusFalsa : IOperacaoRest<StatusFalso, RetornoStatusFalso>
{
    public PedidoRest Montar(StatusFalso parametro, ContextoOperacao contexto) => throw new NotSupportedException();

    public LeituraWS<RetornoStatusFalso> Ler(StatusFalso parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto) =>
        throw new NotSupportedException();
}

internal sealed class OperacaoRestAbstrataFalsa : IOperacaoRest<AbstratoFalso, RetornoStatusFalso>
{
    public PedidoRest Montar(AbstratoFalso parametro, ContextoOperacao contexto) => throw new NotSupportedException();

    public LeituraWS<RetornoStatusFalso> Ler(AbstratoFalso parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto) =>
        throw new NotSupportedException();
}
