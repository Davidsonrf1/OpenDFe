using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using ConsultaProtocolo = OpenDFe.NFe.ConsultaProtocolo;

namespace OpenDFe.Testes;

/// <summary>
/// A carta de correção da NF-e pela lib inteira contra a SEFAZ emulada (etapa 6.2): as cartas 1 e 2 (135) e a consulta com
/// as duas em ordem, a sequência repetida (573), a nota cancelada (580), o <c>dhEvento</c> antes da emissão (577), o lote
/// com a carta e o cancelamento da mesma nota, a NFC-e (o construtor recusa; o 784 do emulador está nos testes dele) e a
/// queda depois de processar.
/// </summary>
public sealed class CartaCorrecaoEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private const string Texto = "Correcao do endereco do destinatario";

    private readonly Emulador _emulador;

    public CartaCorrecaoEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static CartaCorrecao Carta(ResultadoNota nota, int sequencia = 1, string texto = Texto) => new(nota.Chave!, sequencia, texto);

    private static ResultadoEvento Unico(ResultadoWS<RetornoEvento> resultado)
    {
        Assert.True(resultado.Processado);
        Assert.Equal(128, resultado.CStat);
        return Assert.Single(resultado.Retorno!.Eventos);
    }

    private static async Task<ResultadoEvento> Enviar(DFe dfe, Evento evento) =>
        Unico(await dfe.ExecutarAsync(new EnvioEvento(evento), Ct));

    [Fact]
    public async Task AsCartas1E2SaoRegistradasEAConsultaTrazAsDuasEmOrdem()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);

        var resultado = await dfe.ExecutarAsync(new EnvioEvento(Carta(nota, 1)), Ct);
        var primeira = Unico(resultado);
        var segunda = await Enviar(dfe, Carta(nota, 2, "Segunda correcao do endereco"));

        Assert.Empty(resultado.Problemas);
        foreach (var (evento, sequencia) in new[] { (primeira, 1), (segunda, 2) })
        {
            Assert.True(evento.Registrado);
            Assert.Equal(135, evento.Status);
            Assert.Equal("Carta de Correção registrada", evento.Descricao);
            Assert.Equal(nota.Chave, evento.Chave);
            Assert.NotNull(evento.Protocolo);
            Assert.Empty(SchemasNFe400.EventoProcessado.Validar(evento.XmlProcessado!));
            Assert.Contains(evento.XmlEvento!, evento.XmlProcessado, StringComparison.Ordinal);
            Assert.Empty(AssinaturaDigital.Verificar(evento.XmlProcessado!, $"ID110110{nota.Chave}{sequencia:D2}", NotasTeste.CnpjEmitente));
        }

        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);
        Assert.Equal(100, consulta.CStat);
        Assert.False(consulta.Retorno!.Cancelada);
        Assert.Equal(2, consulta.Retorno.Eventos.Count);
        Assert.Contains("<nSeqEvento>1</nSeqEvento>", consulta.Retorno.Eventos[0], StringComparison.Ordinal);
        Assert.Contains("<nSeqEvento>2</nSeqEvento>", consulta.Retorno.Eventos[1], StringComparison.Ordinal);
        Assert.All(consulta.Retorno.Eventos, e => Assert.Contains("110110", e, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ASequenciaRepetidaE573()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);
        var carta = new CartaCorrecao(nota.Chave!, 1, Texto) { Momento = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3)) };

        var primeira = await Enviar(dfe, carta);
        var segunda = await Enviar(dfe, carta);

        Assert.Equal(135, primeira.Status);
        Assert.Equal((573, false), (segunda.Status, segunda.Registrado));
        Assert.Null(segunda.XmlProcessado);
    }

    [Fact]
    public async Task ANotaCanceladaNaoRecebeCartaE580()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);
        Assert.Equal(135, (await Enviar(dfe, new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, "Cancelamento antes da carta"))).Status);

        var evento = await Enviar(dfe, Carta(nota));

        Assert.Equal((580, false), (evento.Status, evento.Registrado));
    }

    [Fact]
    public async Task ODhEventoAntesDaEmissaoE577()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);
        var antes = DateTimeOffset.UtcNow.AddHours(-2).ToOffset(TimeSpan.FromHours(-3));

        var evento = await Enviar(dfe, new CartaCorrecao(nota.Chave!, 1, Texto) { Momento = antes });

        Assert.Equal((577, false), (evento.Status, evento.Registrado));
    }

    [Fact]
    public async Task UmLoteComCartaECancelamentoDaMesmaNotaRegistraOsDoisEACartaDepoisDoCancelamentoE580()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var primeira = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);
        var segunda = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 2);

        var cartaPrimeiro = await dfe.ExecutarAsync(
            new EnvioEvento(Carta(primeira), new Cancelamento(primeira.Chave!, primeira.Protocolo!.Numero!, "Cancelamento no mesmo lote")), Ct);
        var cancelamentoPrimeiro = await dfe.ExecutarAsync(
            new EnvioEvento(new Cancelamento(segunda.Chave!, segunda.Protocolo!.Numero!, "Cancelamento no mesmo lote"), Carta(segunda)), Ct);

        Assert.Empty(cartaPrimeiro.Problemas);
        Assert.Equal([135, 135], cartaPrimeiro.Retorno!.Eventos.Select(e => e.Status));
        Assert.Equal([TipoEvento.CartaCorrecao, TipoEvento.Cancelamento], cartaPrimeiro.Retorno.Eventos.Select(e => e.Evento!.Tipo));
        Assert.Equal([135, 580], cancelamentoPrimeiro.Retorno!.Eventos.Select(e => e.Status));
    }

    [Fact]
    public async Task ANFCeNaoTemCartaDeCorrecaoEOConstrutorRecusa()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var nota = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);

        // o 784 do emulador (GA03a) é provado nos testes dele, com o evento escrito à mão
        Assert.Throws<ArgumentException>(() => new CartaCorrecao(nota.Chave!, 1, Texto));
    }

    [Fact]
    public async Task AQuedaDepoisDeProcessarSeResolvePelaConsultaQueMostraACarta()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);
        var carta = new CartaCorrecao(nota.Chave!, 1, Texto) { Momento = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3)) };
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", "NFeRecepcaoEvento4", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new EnvioEvento(carta), Ct));
        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);
        var reenvio = await Enviar(dfe, carta);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(100, consulta.CStat);
        Assert.Contains("110110", Assert.Single(consulta.Retorno!.Eventos), StringComparison.Ordinal);
        Assert.Equal(573, reenvio.Status);
    }
}
