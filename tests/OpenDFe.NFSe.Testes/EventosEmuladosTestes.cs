using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFSe;
using Emulador = OpenDFe.SefazEmulada.Emulador;
using PedidoFalha = OpenDFe.SefazEmulada.PedidoFalha;
using TipoFalha = OpenDFe.SefazEmulada.TipoFalha;

namespace OpenDFe.Testes;

/// <summary>
/// Os eventos da 9.3 pela lib inteira contra a Sefin emulada (E.8): o cancelamento e a análise fiscal registrados, a
/// matriz de eventos, a substituição ponta a ponta com o 105102, a consulta dos eventos lendo também os do município, as
/// recusas locais e a queda no meio. A produção restrita fica para o PFX (PROJETO.md 17).
/// </summary>
public sealed class EventosEmuladosTestes : IClassFixture<SefinEmuladaFixture>, IDisposable
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = DpsTeste.CnpjPrestador }.GerarPfx("teste"), "teste");

    private static readonly TimeSpan Brasilia = TimeSpan.FromHours(-3);

    private static int _numero = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100_000) * 100;

    private readonly Emulador _emulador;
    private readonly ConexoesSefaz _conexoes;
    private readonly DFe _dfe;

    public EventosEmuladosTestes(SefinEmuladaFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Municipios.Limpar();
        _conexoes = new ConexoesSefaz(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };
        _dfe = DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = _conexoes,
            Urls = [new EnderecoServico(Destino.SefinNacional, Ambiente.Homologacao, "SefinNacional", _emulador.SefinNacional)],
        });
    }

    public void Dispose() => _conexoes.Dispose();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Dps Dps(Action<Dps>? ajustar = null)
    {
        var agora = DateTimeOffset.UtcNow.ToOffset(Brasilia).AddMinutes(-1);
        var dps = DpsTeste.Minima();
        dps.Numero = Interlocked.Increment(ref _numero);
        dps.Emissao = agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerSecond));
        dps.Competencia = DateOnly.FromDateTime(dps.Emissao.Value.DateTime);
        dps.Prestador.Cpf = null;
        dps.Prestador.Cnpj = DpsTeste.CnpjPrestador;
        ajustar?.Invoke(dps);
        return dps;
    }

    private async Task<ChaveNFSe> EmitirAsync(Action<Dps>? ajustar = null)
    {
        var emissao = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps(ajustar)), Ct);
        Assert.True(emissao.Retorno!.Emitida, emissao.Motivo);
        return emissao.Retorno.Chave!.Value;
    }

    // O cancelamento do prestador, com a data e o ambiente do núcleo.
    private static CancelamentoNFSe Cancelamento(ChaveNFSe chave) => new()
    {
        CnpjAutor = DpsTeste.CnpjPrestador,
        Chave = chave.Valor,
        Motivo = MotivoCancelamento.ErroEmissao,
        Descricao = "Erro no valor do serviço",
    };

    private static AnaliseFiscalCancelamento Analise(ChaveNFSe chave) => new()
    {
        CnpjAutor = DpsTeste.CnpjPrestador,
        Chave = chave.Valor,
        Motivo = MotivoAnaliseFiscal.ServicoNaoPrestado,
        Descricao = "Serviço cancelado pelo cliente",
    };

    // Um evento do município pela rota de controle do emulador (o deferimento, o indeferimento, o ofício).
    private async Task MunicipioAsync(ChaveNFSe chave, object pedido)
    {
        using var handler = new SocketsHttpHandler();
        handler.SslOptions.ClientCertificates = [Certificado];
        handler.SslOptions.RemoteCertificateValidationCallback = (_, certificado, _, erros) =>
            ConexaoTeste.SobeAutoridade(_emulador.Autoridade, certificado, erros);
        using var http = new HttpClient(handler);
        using var resposta = await http.PostAsJsonAsync(new Uri(_emulador.Endereco, $"_controle/nfse/{chave.Valor}/eventos"), pedido, Ct);
        Assert.Equal(System.Net.HttpStatusCode.Created, resposta.StatusCode);
    }

    // O registro

    [Fact]
    public async Task CancelaANFSeEConsultaOEvento()
    {
        var chave = await EmitirAsync();

        var resultado = await _dfe.ExecutarAsync(new EnvioEventoNFSe(Cancelamento(chave)), Ct);

        Assert.True(resultado.Processado, resultado.Motivo);
        Assert.Equal((201, "Evento registrado"), (resultado.CStat, resultado.Motivo));
        Assert.Empty(resultado.Problemas);
        var evento = resultado.Retorno.Evento!;
        Assert.Equal(("101101", 1), (evento.Tipo, evento.Sequencia));
        Assert.Equal(AmbienteGeradorEvento.SefinNacional, evento.Registrado!.AmbienteGerador);
        var pedido = Assert.IsType<CancelamentoNFSe>(evento.Registrado.Pedido);
        Assert.Equal((chave.Valor, Ambiente.Homologacao), (pedido.Chave, pedido.Ambiente));
        Assert.NotNull(pedido.Data);
        Assert.Equal(new Uri(_emulador.SefinNacional, $"nfse/{chave.Valor}/eventos"), resultado.Rastro.Url);
        Assert.Contains("<Signature", resultado.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Contains("eventoXmlGZipB64", resultado.Rastro.XmlRetorno, StringComparison.Ordinal);

        // O evento lido de volta pelo leiaute é o assinado pela AC, com o pedido da lib dentro.
        Assert.Empty(SchemasNFSe101.Evento.Validar(evento.Xml));
        var lido = LeiauteNFSe101.Instancia.LerEvento(evento.Xml);
        Assert.Equal("EVT" + chave.Valor + "101101001", lido.Id);
        Assert.Contains(resultado.Rastro.XmlPedido!.Replace("<pedRegEvento ", "", StringComparison.Ordinal)[..60], evento.Xml, StringComparison.Ordinal);

        var consulta = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave), Ct);
        Assert.True(consulta.Retorno!.Encontrada);
        Assert.Equal(evento.Xml, Assert.Single(consulta.Retorno.Eventos).Xml);
    }

    [Fact]
    public async Task OCancelamentoRepetidoEhRecusadoPelaMatriz()
    {
        var chave = await EmitirAsync();
        await _dfe.ExecutarAsync(new EnvioEventoNFSe(Cancelamento(chave)), Ct);

        var repetido = await _dfe.ExecutarAsync(new EnvioEventoNFSe(Cancelamento(chave)), Ct);
        var analise = await _dfe.ExecutarAsync(new EnvioEventoNFSe(Analise(chave)), Ct);

        Assert.Equal(400, repetido.CStat);
        Assert.False(repetido.Retorno!.Registrado);
        Assert.Equal("E0840", Assert.Single(repetido.Retorno.Erros).Codigo);
        Assert.Equal(400, analise.CStat);
        Assert.StartsWith("E08", Assert.Single(analise.Retorno!.Erros).Codigo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AAnaliseFiscalEODeferimentoDoMunicipioSaoLidosNaConsulta()
    {
        var chave = await EmitirAsync();

        var analise = await _dfe.ExecutarAsync(new EnvioEventoNFSe(Analise(chave)), Ct);
        var cancelamento = await _dfe.ExecutarAsync(new EnvioEventoNFSe(Cancelamento(chave)), Ct);
        await MunicipioAsync(chave, new { tipo = "105104", cpfAgente = "52998224725", processo = "123", descricao = "Cancelamento deferido no teste" });
        var consulta = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave), Ct);
        var soDeferido = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave) { Tipo = "105104", Sequencia = 1 }, Ct);

        Assert.True(analise.Retorno!.Registrado, analise.Motivo);
        Assert.Equal("101103", analise.Retorno.Evento!.Tipo);
        Assert.Equal(400, cancelamento.CStat);
        Assert.Empty(consulta.Problemas);
        Assert.Collection(consulta.Retorno!.Eventos,
            e => Assert.IsType<AnaliseFiscalCancelamento>(e.Registrado!.Pedido),
            e =>
            {
                var deferido = Assert.IsType<CancelamentoDeferido>(e.Registrado!.Pedido);
                Assert.Equal(("52998224725", "123", MotivoDeferimento.Deferido), (deferido.CpfAgente, deferido.ProcessoAdministrativo, deferido.Motivo));
            });
        Assert.Equal("105104", Assert.Single(soDeferido.Retorno!.Eventos).Tipo);
    }

    [Fact]
    public async Task OCancelamentoDeOficioELido()
    {
        var chave = await EmitirAsync();

        await MunicipioAsync(chave, new { tipo = "305101", cpfAgente = "52998224725", processo = "999", descricao = "Processo administrativo de ofício" });
        var consulta = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave) { Tipo = "305101" }, Ct);

        var oficio = Assert.IsType<CancelamentoOficio>(Assert.Single(consulta.Retorno!.Eventos).Registrado!.Pedido);
        Assert.Equal("999", oficio.ProcessoAdministrativo);
    }

    // A substituição ponta a ponta

    [Fact]
    public async Task ASubstituicaoGeraO105102LidoNaSubstituida()
    {
        var original = await EmitirAsync();

        var substituta = await EmitirAsync(d => d.Substituicao = new Substituicao
        {
            Chave = original.Valor,
            Motivo = MotivoSubstituicao.Outros,
            Descricao = "Correção do valor do serviço",
        });
        var consulta = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(original) { Tipo = "105102" }, Ct);
        var outra = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps(d => d.Substituicao = new Substituicao
        {
            Chave = original.Valor,
            Motivo = MotivoSubstituicao.Outros,
            Descricao = "Segunda substituição da mesma nota",
        })), Ct);
        var cancelamento = await _dfe.ExecutarAsync(new EnvioEventoNFSe(Cancelamento(original)), Ct);

        var evento = Assert.Single(consulta.Retorno!.Eventos);
        Assert.Equal("105102", evento.Tipo);
        var substituicao = Assert.IsType<CancelamentoSubstituicao>(evento.Registrado!.Pedido);
        Assert.Equal((substituta.Valor, MotivoSubstituicao.Outros), (substituicao.ChaveSubstituta, substituicao.Motivo));
        Assert.Equal("E0046", Assert.Single(outra.Retorno!.Erros).Codigo);
        Assert.Equal(400, cancelamento.CStat);
        Assert.Empty((await _dfe.ExecutarAsync(new ConsultaEventosNFSe(substituta), Ct)).Retorno!.Eventos);
    }

    // A consulta

    [Fact]
    public async Task AConsultaDoQueNaoExiste()
    {
        var chave = await EmitirAsync();

        var semNota = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(ChaveNFSe.Ler(NFSeTeste.Chave)), Ct);
        var semEventos = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave), Ct);
        var semNumero = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave) { Tipo = "101101", Sequencia = 1 }, Ct);

        Assert.Equal(404, semNota.CStat);
        Assert.Equal("E0044", Assert.Single(semNota.Retorno!.Erros).Codigo);
        Assert.Equal(200, semEventos.CStat);
        Assert.True(semEventos.Retorno!.Encontrada);
        Assert.Empty(semEventos.Retorno.Eventos);
        Assert.Equal(404, semNumero.CStat);
        Assert.False(semNumero.Retorno!.Encontrada);
    }

    // As recusas

    [Fact]
    public async Task OAutorQueNaoEOTitularEhRejeitado()
    {
        var chave = await EmitirAsync();
        var evento = Cancelamento(chave);
        evento.CnpjAutor = DpsTeste.CnpjTomador;

        var resultado = await _dfe.ExecutarAsync(new EnvioEventoNFSe(evento), Ct);

        Assert.Equal(400, resultado.CStat);
        Assert.Equal("E0812", Assert.Single(resultado.Retorno!.Erros).Codigo);
    }

    [Fact]
    public async Task AChaveInvalidaEOSchemaRecusamSemTransmitir()
    {
        var chave = await EmitirAsync();
        var semChave = Cancelamento(chave);
        semChave.Chave = "123";
        var semAutor = Cancelamento(chave);
        semAutor.CnpjAutor = null;
        var antes = _emulador.Pedidos.Listar().Count;

        var montagem = await _dfe.ExecutarAsync(new EnvioEventoNFSe(semChave), Ct);
        var schema = await _dfe.ExecutarAsync(new EnvioEventoNFSe(semAutor), Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, montagem.Situacao);
        Assert.Equal(("NFSE-ID", "Chave"), (Assert.Single(montagem.Problemas).Codigo, montagem.Problemas[0].Propriedade));
        Assert.Null(montagem.Rastro.Url);
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, schema.Situacao);
        Assert.Contains(schema.Problemas, p => p.Codigo == "E1235");
        Assert.Contains("<Signature", schema.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Equal(antes, _emulador.Pedidos.Listar().Count);
    }

    [Fact]
    public async Task OAmbienteDiferenteDoNucleoEhRecusadoNaMontagem()
    {
        var evento = Cancelamento(await EmitirAsync());
        evento.Ambiente = Ambiente.Producao;

        var resultado = await _dfe.ExecutarAsync(new EnvioEventoNFSe(evento), Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal("NFSE-NUCLEO", Assert.Single(resultado.Problemas).Codigo);
    }

    // A queda no meio

    [Fact]
    public async Task AQuedaDepoisDeRegistrarSeDescobrePelaConsulta()
    {
        var chave = await EmitirAsync();
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "SefinNacional", "EventoNFSe", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new EnvioEventoNFSe(Cancelamento(chave)), Ct));
        var consulta = await _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave) { Tipo = "101101" }, Ct);
        var reenvio = await _dfe.ExecutarAsync(new EnvioEventoNFSe(Cancelamento(chave)), Ct);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal("101101", Assert.Single(consulta.Retorno!.Eventos).Tipo);
        Assert.Equal("E0840", Assert.Single(reenvio.Retorno!.Erros).Codigo);
    }

    [Fact]
    public async Task AFalhaNaConsultaDosEventosEhFalhaHttp()
    {
        var chave = await EmitirAsync();
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "SefinNacional", "ConsultaEventosNFSe", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new ConsultaEventosNFSe(chave), Ct));

        Assert.Equal(503, falha.Rastro.StatusHttp);
        Assert.False(falha.PodeTerSidoProcessado);
    }
}
