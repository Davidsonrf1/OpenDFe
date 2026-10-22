using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace OpenDFe.Testes;

// Os contratos do núcleo (CONTRATOS.md 10) com o leiaute falso (Falso/LeiauteFalso.cs), o transporte falso no
// lugar do SOAP (a costura interna do ConexoesSefaz) e o certificado autoassinado.
public class DFeTestes
{
    private const string Cnpj = "11222333000181";

    private static readonly X509Certificate2 Certificado = new CertificadoTeste { Cnpj = Cnpj }.Gerar();
    private static readonly DateTimeOffset Agora = new(2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3));

    private static ConfiguracaoDFe Configuracao(
        ConexoesSefaz conexoes, UF uf = UF.MG, TipoEmissao emissao = TipoEmissao.Normal, TimeProvider? relogio = null) => new()
    {
        Ambiente = Ambiente.Homologacao,
        UF = uf,
        Certificado = Certificado,
        Conexoes = conexoes,
        Emissao = emissao,
        Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(Agora, "Autorizador fora do ar"),
        Relogio = relogio ?? TimeProvider.System,
    };

    private static (DFe Nucleo, TransporteFalso Transporte) Criar(
        UF uf = UF.MG, TipoEmissao emissao = TipoEmissao.Normal, RelogioFalso? relogio = null, TimeSpan demora = default,
        IReadOnlyList<EnderecoServico>? urls = null)
    {
        var transporte = new TransporteFalso { Relogio = relogio, Demora = demora };
        var conexoes = new ConexoesSefaz(transporte);
        var configuracao = Configuracao(conexoes, uf, emissao, relogio);
        if (urls is not null)
        {
            configuracao = new ConfiguracaoDFe
            {
                Ambiente = configuracao.Ambiente,
                UF = configuracao.UF,
                Certificado = configuracao.Certificado,
                Conexoes = configuracao.Conexoes,
                Emissao = configuracao.Emissao,
                Contingencia = configuracao.Contingencia,
                Relogio = configuracao.Relogio,
                Urls = urls,
            };
        }
        return (DFe.Criar(new LeiauteFalso(), Documento.NFe, configuracao), transporte);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // Criar

    [Fact]
    public void CriaONucleoComOQueOLeiauteDeclara()
    {
        var leiaute = new LeiauteFalso();
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        var configuracao = Configuracao(conexoes);

        var dfe = DFe.Criar(leiaute, Documento.NFe, configuracao);

        Assert.Same(leiaute, dfe.Leiaute);
        Assert.Equal(Documento.NFe, dfe.Documento);
        Assert.Same(configuracao, dfe.Configuracao);
        Assert.Equal([typeof(StatusFalso), typeof(EcoFalso)], dfe.Operacoes.Select(o => o.Parametro));
        Assert.Equal([typeof(RetornoStatusFalso), typeof(RetornoEcoFalso)], dfe.Operacoes.Select(o => o.Retorno));
        Assert.Equal(4, dfe.Atendimentos.Count);
        Assert.Equal((UF.AC, TipoEmissao.Normal, Destino.SVRS), (dfe.Atendimentos[2].UF, dfe.Atendimentos[2].Emissao, dfe.Atendimentos[2].Autorizador));
        Assert.Equal(16, dfe.Enderecos.Count);
        Assert.Equal(LeiauteFalso.Url(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoStatus), dfe.Enderecos[0].Url);
    }

    [Fact]
    public void RecusaOsArgumentosNulos()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        Assert.Throws<ArgumentNullException>(() => DFe.Criar(null!, Documento.NFe, Configuracao(conexoes)));
        Assert.Throws<ArgumentNullException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, null!));
        Assert.Throws<ArgumentNullException>(() => DeclaracaoLeiaute.Criar(null!, Documento.NFe));
    }

    [Fact]
    public void RecusaOLeiauteQueNaoAtendeODocumento()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        var leiaute = new LeiauteFalso { Documentos = [Documento.NFe] };

        Assert.Throws<ArgumentException>(() => DFe.Criar(leiaute, Documento.NFCe, Configuracao(conexoes)));
        Assert.Throws<ArgumentException>(() => DeclaracaoLeiaute.Criar(leiaute, Documento.NFCe));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), (Documento)57, Configuracao(conexoes)));
    }

    [Fact]
    public void ConfereAConfiguracaoDoDocumento()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        var configuracaoVazia = Configuracao(conexoes);
        ConfiguracaoDFe Configurar(ConfiguracaoDocumento documento) => new()
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes, Documento = documento,
        };

        // A NFC-e exige a ConfiguracaoFalsa; a NF-e não declara nenhuma.
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFCe, configuracaoVazia));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFCe, Configurar(new OutraConfiguracaoFalsa())));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configurar(new ConfiguracaoFalsa())));

        var configuracao = Configurar(new ConfiguracaoFalsa { Csc = "123" });
        var dfe = DFe.Criar(new LeiauteFalso(), Documento.NFCe, configuracao);
        Assert.Equal("123", Assert.IsType<ConfiguracaoFalsa>(dfe.Configuracao.Documento).Csc);
        Assert.NotNull(DFe.Criar(new LeiauteFalso(), Documento.NFe, configuracaoVazia));
    }

    [Fact]
    public void RecusaAUFSemAtendimentoNoTipoDeEmissao()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());

        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configuracao(conexoes, UF.SP)));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configuracao(conexoes, UF.MG, TipoEmissao.ContingenciaSvcRS)));
        Assert.NotNull(DFe.Criar(new LeiauteFalso(), Documento.NFe, Configuracao(conexoes, UF.AC, TipoEmissao.ContingenciaSvcAN)));
    }

    [Fact]
    public void ExigeAContingenciaForaDaEmissaoNormal()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        var contingencia = new Contingencia(Agora, "Autorizador fora do ar");

        var contingenciaAusente = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes,
            Emissao = TipoEmissao.ContingenciaSvcAN,
        };
        var normalContingencia = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes,
            Contingencia = contingencia,
        };
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, contingenciaAusente));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, normalContingencia));

        var dfe = DFe.Criar(new LeiauteFalso(), Documento.NFe, Configuracao(conexoes, UF.MG, TipoEmissao.ContingenciaSvcAN));
        Assert.Equal("Autorizador fora do ar", dfe.Configuracao.Contingencia!.Justificativa);
        Assert.Equal(Agora, dfe.Configuracao.Contingencia.Inicio);
    }

    [Fact]
    public void ExigeOCertificadoComAChavePrivadaRsa()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        using var publico = X509CertificateLoader.LoadCertificate(Certificado.RawData);
        using var ecdsa = new CertificadoTeste { Cnpj = Cnpj, ChaveEcdsa = true }.Gerar();
        ConfiguracaoDFe Configurar(X509Certificate2 certificado) => new()
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = certificado, Conexoes = conexoes,
        };

        Assert.Throws<CryptographicException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configurar(publico)));
        Assert.Throws<CryptographicException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configurar(ecdsa)));
    }

    [Fact]
    public void NaoConfereAValidadeDoCertificado()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        using var vencido = new CertificadoTeste
        {
            Cnpj = Cnpj, NaoAntes = Agora.AddYears(-2), NaoDepois = Agora.AddYears(-1),
        }.Gerar();
        var configuracao = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = vencido, Conexoes = conexoes,
        };

        Assert.NotNull(DFe.Criar(new LeiauteFalso(), Documento.NFe, configuracao));
    }

    [Fact]
    public void RecusaAConfiguracaoIncompletaOuComValorDesconhecido()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        ConfiguracaoDFe Base() => Configuracao(conexoes);
        ConfiguracaoDFe[] invalidas =
        [
            new() { Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = null!, Conexoes = conexoes },
            new() { Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = null! },
            new() { Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes, Relogio = null! },
            new() { Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes, Urls = null! },
            new() { Ambiente = (Ambiente)3, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes },
            new() { Ambiente = Ambiente.Homologacao, UF = (UF)99, Certificado = Certificado, Conexoes = conexoes },
            new() { Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes, Emissao = (TipoEmissao)3 },
        ];

        Assert.NotNull(DFe.Criar(new LeiauteFalso(), Documento.NFe, Base()));
        foreach (var configuracao in invalidas)
            Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, configuracao));
    }

    [Fact]
    public void RecusaAUrlTrocadaNulaOuRepetida()
    {
        var url = new EnderecoServico(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, new Uri("https://nova.exemplo/status"));
        var outra = new EnderecoServico(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, new Uri("https://outra.exemplo/status"));

        Assert.Throws<ArgumentException>(() => Criar(urls: [url, outra]));
        Assert.Throws<ArgumentException>(() => Criar(urls: [url, null!]));
    }

    [Fact]
    public void RecusaAsConexoesDescartadas()
    {
        var conexoes = new ConexoesSefaz(new TransporteFalso());
        conexoes.Dispose();

        Assert.Throws<ObjectDisposedException>(() => DFe.Criar(new LeiauteFalso(), Documento.NFe, Configuracao(conexoes)));
    }

    [Fact]
    public void ACopiaDasListasNaoMudaComAConfiguracao()
    {
        var acrescentada = new EnderecoServico(Destino.SVAN, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, new Uri("https://svan.exemplo/status"));
        var urls = new List<EnderecoServico> { acrescentada };
        var (dfe, _) = Criar(urls: urls);

        urls.Clear();

        Assert.Same(acrescentada, dfe.Enderecos[^1]);
        Assert.Equal(17, dfe.Enderecos.Count);
    }

    // A declaração

    [Fact]
    public void ADeclaracaoSemNucleoListaOQueOLeiauteDeclara()
    {
        var nfce = DeclaracaoLeiaute.Criar(new LeiauteFalso(), Documento.NFCe);
        var nfe = DeclaracaoLeiaute.Criar(new LeiauteFalso(), Documento.NFe);

        Assert.Equal(Documento.NFCe, nfce.Documento);
        Assert.Equal(typeof(ConfiguracaoFalsa), nfce.TipoConfiguracaoDocumento);
        Assert.True(nfce.ConfiguracaoDocumentoObrigatoria);
        Assert.Null(nfe.TipoConfiguracaoDocumento);
        Assert.False(nfe.ConfiguracaoDocumentoObrigatoria);
        Assert.Equal([typeof(StatusFalso), typeof(EcoFalso)], nfce.Operacoes.Select(o => o.Parametro));
        Assert.Equal(4, nfce.Atendimentos.Count);
        Assert.Equal(16, nfce.Enderecos.Count);
    }

    [Fact]
    public void RecusaADeclaracaoDepoisDoDeclarar()
    {
        var leiaute = new LeiauteFalso();
        _ = DeclaracaoLeiaute.Criar(leiaute, Documento.NFe);
        var declaracao = leiaute.Ultima!;

        Assert.Throws<InvalidOperationException>(() => declaracao.RegistrarOperacao(new OperacaoStatusFalsa()));
        Assert.Throws<InvalidOperationException>(() => declaracao.DeclararAtendimento(UF.SP, TipoEmissao.Normal, Destino.SP));
        Assert.Throws<InvalidOperationException>(() => declaracao.DeclararEndereco(
            new EnderecoServico(Destino.SP, Ambiente.Producao, "X", new Uri("https://sp.exemplo"))));
        Assert.Throws<InvalidOperationException>(() => declaracao.DeclararConfiguracaoDocumento<ConfiguracaoFalsa>(false));
        Assert.Equal(2, declaracao.Operacoes.Count);
    }

    [Fact]
    public void RecusaADeclaracaoRepetidaOuInvalida()
    {
        var endereco = new EnderecoServico(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, new Uri("https://mg.exemplo"));
        Action<DeclaracaoLeiaute>[] acrescimos =
        [
            d => d.RegistrarOperacao(new OperacaoStatusFalsa()),
            d => d.RegistrarOperacao(new OperacaoAbstrataFalsa()),
            d => d.RegistrarOperacao<StatusFalso, RetornoStatusFalso>(null!),
            d => d.DeclararAtendimento(UF.MG, TipoEmissao.Normal, Destino.SVRS),
            d => d.DeclararAtendimento((UF)99, TipoEmissao.Normal, Destino.SVRS),
            d => d.DeclararAtendimento(UF.SP, (TipoEmissao)3, Destino.SVRS),
            d => d.DeclararAtendimento(UF.SP, TipoEmissao.Normal, (Destino)99),
            d => d.DeclararEndereco(endereco),
            d => d.DeclararEndereco(null!),
        ];
        foreach (var acrescimo in acrescimos)
            Assert.ThrowsAny<ArgumentException>(() => DeclaracaoLeiaute.Criar(new LeiauteFalso { Acrescimo = acrescimo }, Documento.NFe));

        // Na NFC-e, o leiaute falso já declarou a configuração do documento.
        Assert.Throws<InvalidOperationException>(() => DeclaracaoLeiaute.Criar(
            new LeiauteFalso { Acrescimo = d => d.DeclararConfiguracaoDocumento<OutraConfiguracaoFalsa>(false) }, Documento.NFCe));
    }

    // Suporta, Operacoes

    [Fact]
    public async Task SuportaSoAsOperacoesRegistradas()
    {
        var (dfe, transporte) = Criar();

        Assert.True(dfe.Suporta<StatusFalso>());
        Assert.True(dfe.Suporta<EcoFalso>());
        Assert.False(dfe.Suporta<NaoRegistradoFalso>());
        Assert.False(dfe.Suporta<AbstratoFalso>());
        await Assert.ThrowsAsync<NotSupportedException>(() => dfe.ExecutarAsync(new NaoRegistradoFalso(), Ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => dfe.ExecutarAsync<RetornoStatusFalso>(null!, Ct));
        Assert.Empty(transporte.Envios);
    }

    // As URLs trocadas

    [Fact]
    public async Task AUrlTrocadaSubstituiNoLugarEAAcrescentadaVaiNoFim()
    {
        var trocada = new EnderecoServico(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, new Uri("https://nova-mg.exemplo/status"));
        var acrescentada = new EnderecoServico(Destino.SVAN, Ambiente.Homologacao, LeiauteFalso.ServicoStatus, new Uri("https://svan.exemplo/status"));
        var (dfe, _) = Criar(urls: [acrescentada, trocada]);

        Assert.Equal(17, dfe.Enderecos.Count);
        Assert.Same(trocada, dfe.Enderecos[0]);
        Assert.Same(acrescentada, dfe.Enderecos[^1]);
        Assert.Equal(LeiauteFalso.Url(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoEco), dfe.Enderecos[1].Url);

        var status = await dfe.ExecutarAsync(new StatusFalso(), Ct);
        Assert.Equal(trocada.Url, status.Rastro.Url);
        Assert.Equal("nova-mg.exemplo", status.Retorno!.Host);

        var svan = await dfe.ExecutarAsync(new StatusFalso { Destino = Destino.SVAN }, Ct);
        Assert.Equal(acrescentada.Url, svan.Rastro.Url);
        Assert.Equal(Destino.SVAN, svan.Rastro.Autorizador);
    }

    // A escolha do destino: parâmetro → pedido → núcleo; uma UF vai ao autorizador do atendimento

    [Theory]
    [InlineData(UF.MG, TipoEmissao.Normal, null, null, null, Destino.MG)]
    [InlineData(UF.AC, TipoEmissao.Normal, null, null, null, Destino.SVRS)]
    [InlineData(UF.MG, TipoEmissao.ContingenciaSvcAN, null, null, null, Destino.SvcAN)]
    [InlineData(UF.MG, TipoEmissao.Normal, null, Destino.AC, null, Destino.SVRS)]
    [InlineData(UF.MG, TipoEmissao.Normal, null, null, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN)]
    [InlineData(UF.AC, TipoEmissao.ContingenciaSvcAN, null, null, TipoEmissao.Normal, Destino.SVRS)]
    [InlineData(UF.MG, TipoEmissao.Normal, Destino.AN, Destino.AC, null, Destino.AN)]
    [InlineData(UF.MG, TipoEmissao.Normal, Destino.AC, Destino.MG, null, Destino.SVRS)]
    [InlineData(UF.AC, TipoEmissao.Normal, Destino.MG, null, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN)]
    [InlineData(UF.MG, TipoEmissao.Normal, null, Destino.SVRS, TipoEmissao.ContingenciaSvcAN, Destino.SVRS)]
    public async Task EscolheOAutorizador(
        UF uf, TipoEmissao emissao, Destino? destinoParametro, Destino? destinoPedido, TipoEmissao? emissaoPedido, Destino esperado)
    {
        var (dfe, transporte) = Criar(uf, emissao);

        var resultado = await dfe.ExecutarAsync(
            new EcoFalso { Destino = destinoParametro, DestinoPedido = destinoPedido, EmissaoPedido = emissaoPedido }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(esperado, resultado.Rastro.Autorizador);
        var url = LeiauteFalso.Url(esperado, Ambiente.Homologacao, LeiauteFalso.ServicoEco);
        Assert.Equal(url, resultado.Rastro.Url);
        var envio = Assert.Single(transporte.Envios);
        Assert.Equal(url, envio.Url);
        Assert.Equal(esperado, envio.Autorizador);
    }

    [Fact]
    public async Task OAmbienteEscolheAUrl()
    {
        using var conexoes = new ConexoesSefaz(new TransporteFalso());
        var dfe = DFe.Criar(new LeiauteFalso(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Producao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes,
        });

        var resultado = await dfe.ExecutarAsync(new StatusFalso(), Ct);

        Assert.Equal(LeiauteFalso.Url(Destino.MG, Ambiente.Producao, LeiauteFalso.ServicoStatus), resultado.Rastro.Url);
    }

    [Fact]
    public async Task SemAtendimentoOuSemUrlNaoESuportado()
    {
        var (dfe, transporte) = Criar();

        // SP não tem atendimento; o SVAN não tem URL de eco; MG não é atendida em SVC-RS.
        await Assert.ThrowsAsync<NotSupportedException>(() => dfe.ExecutarAsync(new EcoFalso { Destino = Destino.SP }, Ct));
        await Assert.ThrowsAsync<NotSupportedException>(() => dfe.ExecutarAsync(new EcoFalso { Destino = Destino.SVAN }, Ct));
        await Assert.ThrowsAsync<NotSupportedException>(() => dfe.ExecutarAsync(new EcoFalso { EmissaoPedido = TipoEmissao.ContingenciaSvcRS }, Ct));
        Assert.Empty(transporte.Envios);
    }

    // O caminho completo

    [Fact]
    public async Task ExecutaOCaminhoCompletoComORastro()
    {
        var relogio = new RelogioFalso(Agora);
        var (dfe, transporte) = Criar(relogio: relogio, demora: TimeSpan.FromSeconds(2));

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["um", "dois & três"] }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(SituacaoResultado.Processado, resultado.Situacao);
        Assert.Equal(100, resultado.CStat);
        Assert.Equal("Eco processado", resultado.Motivo);
        Assert.Equal(["um", "dois & três"], resultado.Retorno.Textos);
        Assert.Empty(resultado.Problemas);

        var envio = Assert.Single(transporte.Envios);
        var rastro = resultado.Rastro;
        Assert.Equal(envio.Xml, rastro.XmlPedido);
        Assert.Equal(OperacaoEcoFalsa.Responder(envio.Xml), rastro.XmlRetorno);
        Assert.Equal(LeiauteFalso.ServicoEco, envio.Servico);
        Assert.Same(Certificado, envio.Certificado);
        Assert.Equal(Destino.MG, rastro.Autorizador);
        Assert.Equal(LeiauteFalso.Url(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoEco), rastro.Url);
        Assert.Equal(Agora, rastro.Inicio);
        Assert.Equal(TimeSpan.FromSeconds(2), rastro.DuracaoTransmissao);
        Assert.Equal(TimeSpan.FromSeconds(2), rastro.Duracao);

        // Cada eco assinado, com a assinatura no padrão da 1.4, e a mensagem assinada passou no schema.
        Assert.Empty(AssinaturaDigital.Verificar(rastro.XmlPedido!, "ECO1", Cnpj));
        Assert.Empty(AssinaturaDigital.Verificar(rastro.XmlPedido!, "ECO2", Cnpj));
        Assert.Empty(LeiauteFalso.Schema.Validar(rastro.XmlPedido!));
    }

    [Fact]
    public async Task AssinaAntesDeValidarPeloSchema()
    {
        var (dfe, transporte) = Criar();

        // O schema falso exige a assinatura: sem ela, a mensagem é recusada pelo schema.
        var resultado = await dfe.ExecutarAsync(new EcoFalso { Assinar = false }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(LeiauteFalso.CodigoSchema, problema.Codigo);
        Assert.Equal(TipoFalhaSchema.ElementoAusente, problema.Schema!.Tipo);
        Assert.Empty(transporte.Envios);
    }

    [Fact]
    public async Task OStatusVaiSemAssinatura()
    {
        var (dfe, transporte) = Criar();

        var resultado = await dfe.ExecutarAsync(new StatusFalso(), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(107, resultado.CStat);
        Assert.Equal("Servico em Operacao", resultado.Motivo);
        Assert.DoesNotContain("Signature", Assert.Single(transporte.Envios).Xml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ORetornoTipadoEDeduzidoDoParametro()
    {
        var (dfe, _) = Criar();

        ResultadoWS<RetornoStatusFalso> status = await dfe.ExecutarAsync(new StatusFalso(), Ct);
        ResultadoWS<RetornoEcoFalso> eco = await dfe.ExecutarAsync(new EcoFalso(), Ct);

        Assert.NotNull(status.Retorno);
        Assert.NotNull(eco.Retorno);
    }

    [Fact]
    public async Task OHandlerRecebeODocumentoEAConfiguracao()
    {
        var (dfe, _) = Criar();
        var parametro = new EcoFalso();

        await dfe.ExecutarAsync(parametro, Ct);

        Assert.Equal(Documento.NFe, parametro.ContextoRecebido!.Documento);
        Assert.Same(dfe.Configuracao, parametro.ContextoRecebido.Configuracao);
    }

    // A recusa local

    [Fact]
    public async Task RecusaLocalmentePelaMontagemSemAssinarNemTransmitir()
    {
        var relogio = new RelogioFalso(Agora);
        var (dfe, transporte) = Criar(relogio: relogio);
        var erro = new Problema("M01", Severidade.Erro, "Campo obrigatório ausente.");
        var aviso = new Problema("M02", Severidade.Aviso, "Campo não suportado.");
        var parametro = new EcoFalso { ProblemaMontagem = erro };

        var resultado = await dfe.ExecutarAsync(parametro, Ct);

        Assert.False(resultado.Processado);
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Same(erro, Assert.Single(resultado.Problemas));
        Assert.Null(resultado.CStat);
        Assert.Null(resultado.Motivo);
        Assert.Null(resultado.Retorno);
        var rastro = resultado.Rastro;
        Assert.NotNull(rastro.XmlPedido);
        Assert.DoesNotContain("Signature", rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Null(rastro.XmlRetorno);
        Assert.Null(rastro.Autorizador);
        Assert.Null(rastro.Url);
        Assert.Null(rastro.DuracaoTransmissao);
        Assert.Equal(Agora, rastro.Inicio);
        Assert.Empty(transporte.Envios);

        var vazia = await dfe.ExecutarAsync(new EcoFalso { ProblemaMontagem = erro, MensagemVazia = true }, Ct);
        Assert.Null(vazia.Rastro.XmlPedido);

        var apenasAviso = await dfe.ExecutarAsync(new EcoFalso { ProblemaMontagem = aviso }, Ct);
        Assert.True(apenasAviso.Processado);
    }

    [Fact]
    public async Task RecusaLocalmentePeloSchemaSemTransmitir()
    {
        var (dfe, transporte) = Criar();

        var resultado = await dfe.ExecutarAsync(new EcoFalso { Textos = ["curto", new string('x', 21)] }, Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(LeiauteFalso.CodigoSchema, problema.Codigo);
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal("ecoLote/eco[2]/infEco/texto", problema.Caminho);
        Assert.Equal(FacetaSchema.TamanhoMaximo, Assert.Single(problema.Schema!.Restricoes).Faceta);
        Assert.Contains("<Signature", resultado.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Null(resultado.Rastro.Url);
        Assert.Empty(transporte.Envios);
    }

    [Fact]
    public async Task OsAvisosVoltamNoProcessadoNaOrdemDoCaminho()
    {
        var (dfe, transporte) = Criar();
        transporte.Responder = (envio, _) =>
        {
            envio.MarcarEnviado();
            return Task.FromResult("<retEco><cStat>100</cStat><xMotivo>ok</xMotivo><texto>ola</texto><aviso>Campo novo no retorno.</aviso></retEco>");
        };
        var aviso = new Problema("M02", Severidade.Aviso, "Campo não suportado.");

        var resultado = await dfe.ExecutarAsync(new EcoFalso { ProblemaMontagem = aviso }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(["M02", "L01"], resultado.Problemas.Select(p => p.Codigo));
    }

    [Fact]
    public async Task AExcecaoDaMontagemSobeComoVeio()
    {
        var (dfe, transporte) = Criar();
        var excecao = new InvalidOperationException("erro do leiaute");

        var lancada = await Assert.ThrowsAsync<InvalidOperationException>(() => dfe.ExecutarAsync(new EcoFalso { ExcecaoMontagem = excecao }, Ct));

        Assert.Same(excecao, lancada);
        Assert.Empty(transporte.Envios);
    }

    // A falha de transmissão

    [Fact]
    public async Task AFalhaAntesDeEnviarNaoFoiProcessada()
    {
        var relogio = new RelogioFalso(Agora);
        var (dfe, transporte) = Criar(relogio: relogio, demora: TimeSpan.FromMilliseconds(300));
        var causa = new HttpRequestException("conexão recusada");
        transporte.Responder = (_, _) => Task.FromException<string>(causa);

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new EcoFalso(), Ct));

        Assert.False(falha.PodeTerSidoProcessado);
        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.Same(causa, falha.InnerException);
        Assert.Equal(Assert.Single(transporte.Envios).Xml, falha.Rastro.XmlPedido);
        Assert.Contains("<Signature", falha.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Null(falha.Rastro.XmlRetorno);
        Assert.Equal(Destino.MG, falha.Rastro.Autorizador);
        Assert.Equal(LeiauteFalso.Url(Destino.MG, Ambiente.Homologacao, LeiauteFalso.ServicoEco), falha.Rastro.Url);
        Assert.Equal(TimeSpan.FromMilliseconds(300), falha.Rastro.DuracaoTransmissao);
        Assert.Equal(Agora, falha.Rastro.Inicio);
        Assert.DoesNotContain("ola", falha.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AFalhaDepoisDeEnviarPodeTerSidoProcessada()
    {
        var (dfe, transporte) = Criar();
        transporte.Responder = (envio, _) =>
        {
            envio.MarcarEnviado();
            return Task.FromException<string>(new IOException("a conexão caiu"));
        };

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new EcoFalso(), Ct));

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.IsType<IOException>(falha.InnerException);
    }

    [Fact]
    public async Task OTempoEsgotadoEFalhaDeTransmissao()
    {
        var (dfe, transporte) = Criar();
        transporte.Responder = (envio, _) =>
        {
            envio.MarcarEnviado();
            return Task.FromException<string>(new TaskCanceledException("tempo esgotado"));
        };

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new EcoFalso(), Ct));

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(TipoFalhaTransmissao.TempoEsgotado, falha.Tipo);
        Assert.IsType<TaskCanceledException>(falha.InnerException);
    }

    [Theory]
    [InlineData("<retEco><cStat>100</cStat>", typeof(XmlException))]
    [InlineData("<retEco><xMotivo>sem cStat</xMotivo></retEco>", typeof(FormatException))]
    public async Task ARespostaIlegivelPodeTerSidoProcessada(string resposta, Type causa)
    {
        var (dfe, transporte) = Criar();
        transporte.Responder = (envio, _) =>
        {
            envio.MarcarEnviado();
            return Task.FromResult(resposta);
        };

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new EcoFalso(), Ct));

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.IsType(causa, falha.InnerException);
        Assert.Equal(resposta, falha.Rastro.XmlRetorno);
        Assert.NotNull(falha.Rastro.DuracaoTransmissao);
    }

    [Fact]
    public async Task AsConexoesDescartadasRecusamATransmissao()
    {
        var transporte = new TransporteFalso();
        var conexoes = new ConexoesSefaz(transporte);
        var dfe = DFe.Criar(new LeiauteFalso(), Documento.NFe, Configuracao(conexoes));

        conexoes.Dispose();
        conexoes.Dispose();

        Assert.True(transporte.Descartado);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => dfe.ExecutarAsync(new StatusFalso(), Ct));
        Assert.Empty(transporte.Envios);
    }

    // O cancelamento

    [Fact]
    public async Task OCancelamentoDuranteATransmissaoSobeComoCancelamento()
    {
        var (dfe, transporte) = Criar();
        var saiu = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        transporte.Responder = async (envio, token) =>
        {
            envio.MarcarEnviado();
            saiu.SetResult();
            await Task.Delay(Timeout.Infinite, token);
            return "";
        };
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(Ct);

        var execucao = dfe.ExecutarAsync(new EcoFalso(), cancelamento.Token);
        await saiu.Task;
        await cancelamento.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execucao);
    }

    [Fact]
    public async Task OCanceladoAntesNaoMontaNemTransmite()
    {
        var (dfe, transporte) = Criar();
        using var cancelamento = new CancellationTokenSource();
        await cancelamento.CancelAsync();

        // A montagem lançaria outra exceção, se fosse chamada.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => dfe.ExecutarAsync(new EcoFalso { ExcecaoMontagem = new InvalidOperationException() }, cancelamento.Token));
        Assert.Empty(transporte.Envios);
    }

    // O uso concorrente

    [Fact]
    public async Task ExecutaEmParaleloNoMesmoNucleo()
    {
        var (dfe, transporte) = Criar(relogio: new RelogioFalso(Agora));
        transporte.Responder = async (envio, token) =>
        {
            await Task.Yield();
            envio.MarcarEnviado();
            return TransporteFalso.Resposta(envio);
        };

        var tarefas = Enumerable.Range(0, 64)
            .Select(i => Task.Run(() => dfe.ExecutarAsync(new EcoFalso { Textos = [$"texto {i}", $"mais {i}"] }, Ct), Ct))
            .ToArray();
        var resultados = await Task.WhenAll(tarefas);

        for (var i = 0; i < resultados.Length; i++)
        {
            Assert.True(resultados[i].Processado);
            Assert.Equal([$"texto {i}", $"mais {i}"], resultados[i].Retorno!.Textos);
            Assert.Empty(AssinaturaDigital.Verificar(resultados[i].Rastro.XmlPedido!, "ECO2", Cnpj));
        }
        Assert.Equal(64, transporte.Envios.Count);
    }

    // Os callbacks de validação: só guardados na configuração (a chamada é da 3.1)

    [Fact]
    public async Task OsCallbacksDeValidacaoFicamNaConfiguracao()
    {
        var chamadas = 0;
        var validacao = new ValidacaoCallback
        {
            Campo = c =>
            {
                Interlocked.Increment(ref chamadas);
                return c.Tag == "xProd" && c.Texto.Contains("TESTE", StringComparison.Ordinal)
                    ? [new Problema("PDV-01", Severidade.Erro, "Produto de teste na nota.") { Caminho = c.Caminho }]
                    : [];
            },
            Documento = d =>
            {
                Interlocked.Increment(ref chamadas);
                return d.Chave is null ? [new Problema("PDV-02", Severidade.Aviso, "Sem chave.")] : [];
            },
        };
        var transporte = new TransporteFalso();
        using var conexoes = new ConexoesSefaz(transporte);
        var dfe = DFe.Criar(new LeiauteFalso(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = Certificado, Conexoes = conexoes, Validacao = validacao,
        });

        var resultado = await dfe.ExecutarAsync(new EcoFalso(), Ct);

        Assert.True(resultado.Processado);
        Assert.Same(validacao, dfe.Configuracao.Validacao);
        Assert.Equal(0, chamadas);

        // A forma dos dados que os callbacks recebem.
        var campo = new CampoVerificado(Documento.NFe, "xProd", "NFe/infNFe/det[3]/prod/xProd", "Itens[2].Produto.Descricao", "PRODUTO TESTE");
        var problema = Assert.Single(validacao.Campo!(campo));
        Assert.Equal("NFe/infNFe/det[3]/prod/xProd", problema.Caminho);
        var documento = new DocumentoVerificado(Documento.NFe, modelo: null, "<NFe/>", chave: null);
        Assert.Equal("PDV-02", Assert.Single(validacao.Documento!(documento)).Codigo);
        Assert.Equal(2, chamadas);
    }
}
