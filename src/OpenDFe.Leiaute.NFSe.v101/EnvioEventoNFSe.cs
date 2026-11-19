namespace OpenDFe.NFSe;

/// <summary>
/// O registro de um evento de quem emite na Sefin Nacional (<c>POST /nfse/{chave}/eventos</c>; docs/NFSE.md 3.3 e D9): o
/// cancelamento (<see cref="CancelamentoNFSe"/>, 101101) ou a solicitação de análise fiscal para cancelamento
/// (<see cref="AnaliseFiscalCancelamento"/>, 101103). O pedido (<c>pedRegEvento</c>) é escrito, assinado, validado pelo
/// schema, compactado no JSON e enviado; a Sefin devolve o evento registrado ou os erros.
/// </summary>
/// <remarks>
/// <para>
/// A substituição não é evento de quem emite: é a <see cref="EmissaoNFSe"/> com a <see cref="Dps.Substituicao"/>, e a Sefin
/// gera o 105102 sobre a substituída. A manifestação vai ao ADN e fica fora (D9); os eventos do município e do fisco a lib só
/// lê, pela <see cref="ConsultaEventosNFSe"/>.
/// </para>
/// <para>
/// O <c>dhEvento</c> nulo vale o relógio da configuração, no horário de Brasília, como no evento da NF-e; o autor (o
/// <see cref="EventoNFSe.CnpjAutor"/> ou o <see cref="EventoNFSe.CpfAutor"/>) é de quem usa, e o schema aponta a falta. Se
/// a resposta se perder, a <see cref="ConsultaEventosNFSe"/> com o tipo diz se o evento foi registrado; reenviar o mesmo
/// pedido depois do registro é rejeitado pela matriz de eventos (o cancelamento sobre o cancelamento).
/// </para>
/// </remarks>
public sealed class EnvioEventoNFSe : ParametroWS<RetornoEventoNFSe>
{
    /// <summary>O registro do evento.</summary>
    /// <param name="evento">O cancelamento ou a análise fiscal, sem assinatura. O <c>tpAmb</c> nulo vale o do núcleo.</param>
    /// <exception cref="ArgumentNullException">O evento é nulo.</exception>
    /// <exception cref="ArgumentException">O evento não é de quem emite (só o 101101 e o 101103 vão à Sefin).</exception>
    public EnvioEventoNFSe(EventoNFSe evento)
    {
        ArgumentNullException.ThrowIfNull(evento);
        if (!MapasNFSe.Eventos.Any(t => t.Classe == evento.GetType() && t.Emitente))
            throw new ArgumentException(
                $"O evento {evento.Tipo} não é de quem emite: a Sefin Nacional só recebe o cancelamento (101101) e a análise fiscal (101103).",
                nameof(evento));
        Evento = evento;
    }

    /// <summary>O evento.</summary>
    public EventoNFSe Evento { get; }
}

/// <summary>
/// O retorno do registro do evento: o evento registrado (<see cref="Registrado"/>) ou os erros da Sefin. O
/// <see cref="ResultadoWS{TRetorno}.CStat"/> é o status HTTP (201 registrado, 400 rejeitado).
/// </summary>
public sealed class RetornoEventoNFSe
{
    internal RetornoEventoNFSe(EventoNFSeRetornado? evento, IReadOnlyList<MensagemSefin> erros, IReadOnlyList<MensagemSefin> alertas)
    {
        Evento = evento;
        Erros = erros;
        Alertas = alertas;
    }

    /// <summary>A Sefin registrou o evento: há o <see cref="Evento"/>.</summary>
    public bool Registrado => Evento is not null;

    /// <summary>O evento registrado, com o XML que se guarda; <see langword="null"/> na rejeição.</summary>
    public EventoNFSeRetornado? Evento { get; }

    /// <summary>Os erros da Sefin (a rejeição), na ordem; vazio no evento registrado.</summary>
    public IReadOnlyList<MensagemSefin> Erros { get; }

    /// <summary>Os alertas da Sefin, na ordem.</summary>
    public IReadOnlyList<MensagemSefin> Alertas { get; }
}

/// <summary>
/// Um evento que a Sefin devolveu, no registro (<see cref="RetornoEventoNFSe"/>) ou na consulta
/// (<see cref="RetornoEventosNFSe"/>): o tipo, o número sequencial, o processamento, o XML do evento e ele lido.
/// </summary>
/// <remarks>
/// O tipo, o número e o processamento saem do XML lido (o que a Sefin assinou) e, sem ele, do JSON. Os problemas da leitura
/// do XML vão nos problemas do resultado.
/// </remarks>
public sealed class EventoNFSeRetornado
{
    internal EventoNFSeRetornado(string tipo, int? sequencia, DateTimeOffset? processamento, string xml, EventoNFSeRegistrado? registrado)
    {
        Tipo = tipo;
        Sequencia = sequencia;
        Processamento = processamento;
        Xml = xml;
        Registrado = registrado;
    }

    /// <summary>O código do tipo (<c>"101101"</c>); vazio quando nem o XML nem o JSON o trazem.</summary>
    public string Tipo { get; }

    /// <summary>O número sequencial do evento (<c>nSeqEvento</c>).</summary>
    public int? Sequencia { get; }

    /// <summary>O processamento do evento (<c>dhProc</c>).</summary>
    public DateTimeOffset? Processamento { get; }

    /// <summary>
    /// O XML do evento registrado (<c>evento</c>, com o pedido dentro), descompactado, como veio: o que se guarda.
    /// </summary>
    public string Xml { get; }

    /// <summary>
    /// O evento lido do <see cref="Xml"/> pelo leiaute (<see cref="LeiauteNFSe101.LerEvento"/>), de qualquer autor, com o
    /// pedido na subclasse do tipo (<see cref="EventoNFSeRegistrado.Pedido"/>); <see langword="null"/> quando o XML não é o
    /// evento registrado.
    /// </summary>
    public EventoNFSeRegistrado? Registrado { get; }

    // O evento da resposta: o XML lido, com os problemas da leitura acrescentados em problemas.
    internal static EventoNFSeRetornado Ler(EventoSefin evento, List<Problema> problemas)
    {
        var lido = LeitorNFSe.LerEvento(evento.Xml!);
        problemas.AddRange(lido.Problemas);
        var registrado = lido.Registrado;
        DateTimeOffset? processamento = registrado?.Processamento;
        if (processamento is null && evento.Processamento is { } texto
            && DateTimeOffset.TryParse(texto, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var data))
            processamento = data;
        return new EventoNFSeRetornado(registrado?.Pedido?.Tipo ?? evento.Tipo ?? "", registrado?.Sequencia ?? evento.Sequencia,
            processamento, evento.Xml!, registrado);
    }
}

/// <summary>O handler do registro do evento: o pedido no POST, a resposta lida (docs/NFSE.md, D4 e D5).</summary>
internal sealed class OperacaoEnvioEventoNFSe : IOperacaoRest<EnvioEventoNFSe, RetornoEventoNFSe>
{
    private static readonly TimeSpan Brasilia = TimeSpan.FromHours(-3);

    public PedidoRest Montar(EnvioEventoNFSe parametro, ContextoOperacao contexto)
    {
        var agora = contexto.Configuracao.Relogio.GetUtcNow().ToOffset(Brasilia);
        var escrito = EscritorNFSe.EscreverPedido(parametro.Evento, contexto.Configuracao.Ambiente, agora);
        // Sem a chave válida, o pedido não tem Id e a montagem já é erro (NFSE-ID): o caminho não chega a ser usado.
        var caminho = ChaveNFSe.TentarLer(parametro.Evento.Chave, out var chave) ? $"nfse/{chave.Valor}/eventos" : "nfse/eventos";
        var id = escrito.Id;
        return new PedidoRest(SefinNacional.Servico, MetodoRest.Post, caminho)
        {
            Xml = escrito.Xml,
            Schema = SchemasNFSe101.Pedido,
            CampoXml = SefinNacional.CampoEvento,
            IdsAssinatura = id is null ? [] : [id],
            Destino = Destino.SefinNacional,
            Problemas = escrito.Problemas,
            Documentos = id is null ? [] : [new DocumentoPedido(id) { Modelo = parametro.Evento, Propriedade = CaminhoModeloNFSe.Propriedade }],
        };
    }

    public LeituraWS<RetornoEventoNFSe> Ler(EnvioEventoNFSe parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var lida = RespostaSefin.Ler(resposta);
        var problemas = new List<Problema>();
        EventoNFSeRetornado? evento = null;
        if (lida.Sucesso)
        {
            if (lida.Evento?.Xml is null)
                throw new FormatException($"A resposta do registro do evento (HTTP {lida.Status}) não traz o evento.");
            evento = EventoNFSeRetornado.Ler(lida.Evento, problemas);
        }
        var retorno = new RetornoEventoNFSe(evento, lida.Erros, lida.Alertas);
        return new LeituraWS<RetornoEventoNFSe>(retorno, resposta.Status, lida.Motivo("Evento registrado")) { Problemas = problemas };
    }
}
