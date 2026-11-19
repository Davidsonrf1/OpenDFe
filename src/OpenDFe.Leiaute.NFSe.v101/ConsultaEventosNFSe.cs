using System.Globalization;

namespace OpenDFe.NFSe;

/// <summary>
/// A consulta dos eventos de uma NFS-e na Sefin Nacional (<c>GET /nfse/{chave}/eventos[/{tipo}[/{seq}]]</c>; docs/NFSE.md
/// 3.3 e D9): todos os eventos, os de um tipo, ou o de um tipo e número sequencial. Lê os de qualquer autor: os de quem
/// emite, o 105102 da substituição, os do município (o deferimento, o indeferimento) e os do fisco (o ofício).
/// </summary>
public sealed class ConsultaEventosNFSe : ParametroWS<RetornoEventosNFSe>
{
    private readonly string? _tipo;
    private readonly int? _sequencia;

    /// <summary>A consulta dos eventos da chave.</summary>
    /// <param name="chave">A chave de acesso (50 dígitos).</param>
    /// <exception cref="ArgumentException">A chave é o valor padrão (vazia).</exception>
    public ConsultaEventosNFSe(ChaveNFSe chave)
    {
        if (chave.Valor.Length == 0)
            throw new ArgumentException("A chave da NFS-e é vazia: use ChaveNFSe.Ler.", nameof(chave));
        Chave = chave;
    }

    /// <summary>A chave consultada.</summary>
    public ChaveNFSe Chave { get; }

    /// <summary>O tipo do evento (<c>"101101"</c>); <see langword="null"/>, todos.</summary>
    /// <exception cref="ArgumentException">O tipo não tem 6 dígitos.</exception>
    public string? Tipo
    {
        get => _tipo;
        init
        {
            if (value is not null && (value.Length != 6 || value.AsSpan().IndexOfAnyExceptInRange('0', '9') >= 0))
                throw new ArgumentException($"O tipo do evento tem 6 dígitos: {value}.", nameof(value));
            _tipo = value;
        }
    }

    /// <summary>
    /// O número sequencial do evento (1 a 999), só com o <see cref="Tipo"/> (sem ele, o <c>DFe.ExecutarAsync</c>
    /// lança <see cref="ArgumentException"/>); <see langword="null"/>, todos do tipo.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">O número não vai de 1 a 999.</exception>
    public int? Sequencia
    {
        get => _sequencia;
        init
        {
            if (value is < 1 or > 999)
                throw new ArgumentOutOfRangeException(nameof(value), value, "O número sequencial do evento vai de 1 a 999.");
            _sequencia = value;
        }
    }
}

/// <summary>
/// O retorno da consulta dos eventos: os eventos da NFS-e (<see cref="Encontrada"/>, a lista vazia quando ela não tem
/// nenhum) ou os erros (o 404 da NFS-e que não existe). O <see cref="ResultadoWS{TRetorno}.CStat"/> é o status HTTP.
/// </summary>
public sealed class RetornoEventosNFSe
{
    internal RetornoEventosNFSe(bool encontrada, IReadOnlyList<EventoNFSeRetornado> eventos, IReadOnlyList<MensagemSefin> erros,
        IReadOnlyList<MensagemSefin> alertas)
    {
        Encontrada = encontrada;
        Eventos = eventos;
        Erros = erros;
        Alertas = alertas;
    }

    /// <summary>A Sefin respondeu com a lista (2xx), mesmo vazia.</summary>
    public bool Encontrada { get; }

    /// <summary>Os eventos, na ordem da resposta; vazio sem eventos e quando não foi encontrada.</summary>
    public IReadOnlyList<EventoNFSeRetornado> Eventos { get; }

    /// <summary>Os erros da Sefin (o E0044 da NFS-e que não existe, o evento que não há); vazio se foi encontrada.</summary>
    public IReadOnlyList<MensagemSefin> Erros { get; }

    /// <summary>Os alertas da Sefin.</summary>
    public IReadOnlyList<MensagemSefin> Alertas { get; }
}

internal sealed class OperacaoConsultaEventosNFSe : IOperacaoRest<ConsultaEventosNFSe, RetornoEventosNFSe>
{
    public PedidoRest Montar(ConsultaEventosNFSe parametro, ContextoOperacao contexto)
    {
        if (parametro.Sequencia is not null && parametro.Tipo is null)
            throw new ArgumentException("O número sequencial do evento só se consulta com o tipo.", nameof(parametro));
        var caminho = "nfse/" + parametro.Chave.Valor + "/eventos";
        if (parametro.Tipo is { } tipo)
            caminho += "/" + tipo;
        if (parametro.Sequencia is { } sequencia)
            caminho += "/" + sequencia.ToString(CultureInfo.InvariantCulture);
        return new PedidoRest(SefinNacional.Servico, MetodoRest.Get, caminho) { Destino = Destino.SefinNacional };
    }

    public LeituraWS<RetornoEventosNFSe> Ler(ConsultaEventosNFSe parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var lida = RespostaSefin.Ler(resposta);
        var problemas = new List<Problema>();
        var eventos = new List<EventoNFSeRetornado>();
        if (lida.Sucesso)
        {
            // A lista; sem ela, o evento único no próprio objeto (a consulta pelo número sequencial pode vir assim).
            IReadOnlyList<EventoSefin>? unico = lida.Evento is { } evento0 ? [evento0] : null;
            var recebidos = lida.Eventos ?? unico
                ?? throw new FormatException($"A resposta da consulta dos eventos (HTTP {lida.Status}) não traz a lista de eventos.");
            foreach (var evento in recebidos)
            {
                if (evento.Xml is null)
                    throw new FormatException("Um evento da resposta da consulta dos eventos não traz o XML.");
                eventos.Add(EventoNFSeRetornado.Ler(evento, problemas));
            }
        }
        var retorno = new RetornoEventosNFSe(lida.Sucesso, eventos, lida.Erros, lida.Alertas);
        return new LeituraWS<RetornoEventosNFSe>(retorno, resposta.Status, lida.Motivo("Eventos encontrados")) { Problemas = problemas };
    }
}
