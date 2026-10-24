using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;
using ConsultaProtocolo = OpenDFe.NFe.ConsultaProtocolo;

namespace OpenDFe.Testes;

/// <summary>
/// O cancelamento pela lib inteira contra a SEFAZ emulada (etapa 6.1): a nota autorizada e cancelada (135) com o
/// <c>procEventoNFe</c>, a consulta depois (101), o segundo cancelamento (573), o protocolo errado (222), o prazo de 24 horas
/// (501, com o relógio avançado), a nota que não existe (494), a NFC-e, a nota da SVC-AN, o lote de 3 e a queda depois de
/// processar.
/// </summary>
public sealed class EventoEmuladoTestes : IClassFixture<EmuladorFixture>
{
    private const string Justificativa = "Cancelamento por erro de digitacao";

    private readonly Emulador _emulador;

    public EventoEmuladoTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<ResultadoNota> Autorizar(DFe dfe, Documento documento, int numero, TimeProvider? relogio = null)
    {
        var agora = (relogio ?? TimeProvider.System).GetUtcNow();
        var resultado = await dfe.ExecutarAsync(new Autorizacao(ApoioEmulado.Nota(documento, numero, agora)), Ct);
        var nota = Assert.Single(resultado.Retorno!.Notas);
        Assert.True(nota.Autorizada, resultado.Retorno.Motivo + " / " + nota.Protocolo?.Motivo);
        return nota;
    }

    private static Cancelamento Cancelar(ResultadoNota nota, string justificativa = Justificativa) =>
        new(nota.Chave!, nota.Protocolo!.Numero!, justificativa);

    private static ResultadoEvento Unico(ResultadoWS<RetornoEvento> resultado)
    {
        Assert.True(resultado.Processado);
        Assert.Equal(128, resultado.CStat);
        return Assert.Single(resultado.Retorno!.Eventos);
    }

    [Fact]
    public async Task ANFeAutorizadaECanceladaTrazOProcEventoNFeEAConsultaMostraOEvento()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await Autorizar(dfe, Documento.NFe, 1);

        var resultado = await dfe.ExecutarAsync(new EnvioEvento(Cancelar(nota)), Ct);

        Assert.Empty(resultado.Problemas);
        var evento = Unico(resultado);
        Assert.True(evento.Registrado);
        Assert.Equal(135, evento.Status);
        Assert.Equal("Cancelamento homologado", evento.Descricao);
        Assert.Equal(nota.Chave, evento.Chave);
        Assert.NotNull(evento.Protocolo);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(evento.XmlProcessado!));
        Assert.Contains(evento.XmlEvento!, evento.XmlProcessado, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(evento.XmlProcessado!, $"ID110111{nota.Chave}01", NotasTeste.CnpjEmitente));

        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);
        Assert.Equal(101, consulta.CStat);
        Assert.True(consulta.Retorno!.Cancelada);
        Assert.Contains("110111", Assert.Single(consulta.Retorno.Eventos), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OSegundoCancelamentoDaMesmaNotaE573()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await Autorizar(dfe, Documento.NFe, 1);
        var momento = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3));
        var evento = new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, Justificativa) { Momento = momento };

        var primeiro = Unico(await dfe.ExecutarAsync(new EnvioEvento(evento), Ct));
        var segundo = Unico(await dfe.ExecutarAsync(new EnvioEvento(evento), Ct));

        Assert.Equal(135, primeiro.Status);
        Assert.Equal((573, false), (segundo.Status, segundo.Registrado));
        Assert.Null(segundo.XmlProcessado);
        Assert.Equal(primeiro.XmlEvento, segundo.XmlEvento);
    }

    [Fact]
    public async Task OProtocoloErradoE222()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await Autorizar(dfe, Documento.NFe, 1);

        var evento = Unico(await dfe.ExecutarAsync(new EnvioEvento(new Cancelamento(nota.Chave!, "131269999999999", Justificativa)), Ct));

        Assert.Equal(222, evento.Status);
        Assert.False(evento.Registrado);
    }

    [Fact]
    public async Task ONotaQueNaoExisteE494()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var preparada = dfe.Preparar(ApoioEmulado.Nota(Documento.NFe, 9, DateTimeOffset.UtcNow));

        var evento = Unico(await dfe.ExecutarAsync(new EnvioEvento(new Cancelamento(preparada.Chave!, "131260000000001", Justificativa)), Ct));

        Assert.Equal(494, evento.Status);
    }

    [Fact]
    public async Task OCancelamentoDepoisDe24HorasE501()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Emulador.IniciarAsync(new OpcoesEmulador
        {
            Porta = 0,
            Pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N")),
            Relogio = relogio,
        }, Ct);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        var dfe = ApoioEmulado.Criar(emulador, conexoes, relogio: relogio);
        var nota = await Autorizar(dfe, Documento.NFe, 1, relogio);

        relogio.Avancar(TimeSpan.FromHours(25));
        var evento = Unico(await dfe.ExecutarAsync(new EnvioEvento(Cancelar(nota)), Ct));

        Assert.Equal(501, evento.Status);
        Assert.False(evento.Registrado);
    }

    [Fact]
    public async Task OCertificadoDeOutroEmitenteERejeitadoPelaAssinatura()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var chave = ChaveAcesso.Montar(UF.MG, DateTimeOffset.UtcNow, "45723174000110", Documento.NFe, 1, 1, TipoEmissao.Normal, 12345678);

        var evento = Unico(await dfe.ExecutarAsync(new EnvioEvento(new Cancelamento(chave, "131260000000001", Justificativa)), Ct));

        // O autor é o emitente da chave, e o certificado é de outra raiz: a assinatura é a primeira a recusar (213);
        // o 574 só chega com o certificado certo e o autor errado, que a lib não monta.
        Assert.Equal(213, evento.Status);
    }

    [Fact]
    public async Task ANFCeAutorizadaECancelada()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var nota = await Autorizar(dfe, Documento.NFCe, 1);

        var resultado = await dfe.ExecutarAsync(new EnvioEvento(Cancelar(nota)), Ct);

        var evento = Unico(resultado);
        Assert.Equal(135, evento.Status);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(evento.XmlProcessado!));
        Assert.Equal(101, (await dfe.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct)).CStat);
    }

    [Fact]
    public async Task ANotaDaSvcAnECanceladaNaSvcEPeloDestinoNoNormal()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Codigo: "109"));
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var svc = ApoioEmulado.Criar(_emulador, conexoes, emissao: TipoEmissao.ContingenciaSvcAN);
        var normal = ApoioEmulado.Criar(_emulador, conexoes);
        var primeira = await Autorizar(svc, Documento.NFe, 1);
        var segunda = await Autorizar(svc, Documento.NFe, 2);
        _emulador.Controle.Limpar();

        var naSvc = await normal.ExecutarAsync(new EnvioEvento(Cancelar(primeira)), Ct);
        var noNormal = await normal.ExecutarAsync(new EnvioEvento(Cancelar(segunda)) { Destino = Destino.MG }, Ct);

        Assert.Equal((Destino.SvcAN, 135), (naSvc.Rastro.Autorizador, Unico(naSvc).Status));
        Assert.Equal((Destino.MG, 135), (noNormal.Rastro.Autorizador, Unico(noNormal).Status));
        Assert.Equal(101, (await normal.ExecutarAsync(new ConsultaProtocolo(segunda.Chave!), Ct)).CStat);
    }

    [Fact]
    public async Task OLoteDeTresCancelamentosTemUmRetEventoParaCada()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var notas = new List<ResultadoNota>();
        for (var numero = 1; numero <= 3; numero++)
            notas.Add(await Autorizar(dfe, Documento.NFe, numero));
        var eventos = notas.Select(n => Cancelar(n)).ToArray();

        var resultado = await dfe.ExecutarAsync(new EnvioEvento(eventos), Ct);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        var retorno = resultado.Retorno!;
        Assert.Equal(128, retorno.Status);
        Assert.Equal(3, retorno.Eventos.Count);
        for (var k = 0; k < 3; k++)
        {
            Assert.Same(eventos[k], retorno.Eventos[k].Evento);
            Assert.Equal(135, retorno.Eventos[k].Status);
            Assert.Empty(SchemasNFe400.EventoProcessado.Validar(retorno.Eventos[k].XmlProcessado!));
        }
    }

    [Fact]
    public async Task ALoteComUmCancelamentoBomEUmRejeitadoTrazOsDois()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var bom = await Autorizar(dfe, Documento.NFe, 1);
        var outro = await Autorizar(dfe, Documento.NFe, 2);

        var resultado = await dfe.ExecutarAsync(new EnvioEvento(Cancelar(bom), new Cancelamento(outro.Chave!, "131269999999999", Justificativa)), Ct);

        Assert.Equal([135, 222], resultado.Retorno!.Eventos.Select(e => e.Status));
    }

    [Fact]
    public async Task AQuedaDepoisDeProcessarSeResolvePelaConsultaEPeloReenvioDoMesmoEvento()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = await Autorizar(dfe, Documento.NFe, 1);
        var evento = new Cancelamento(nota.Chave!, nota.Protocolo!.Numero!, Justificativa) { Momento = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3)) };
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", "NFeRecepcaoEvento4", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new EnvioEvento(evento), Ct));
        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);
        var reenvio = await dfe.ExecutarAsync(new EnvioEvento(evento), Ct);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(101, consulta.CStat);
        Assert.Contains("110111", Assert.Single(consulta.Retorno!.Eventos), StringComparison.Ordinal);
        var repetido = Unico(reenvio);
        Assert.Equal(573, repetido.Status);
        Assert.Equal(OperacaoEventoApoio.Enviados(falha.Rastro.XmlPedido!)[0], repetido.XmlEvento);
    }
}
