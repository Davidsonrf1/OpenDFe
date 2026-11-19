namespace OpenDFe.NFSe;

/// <summary>
/// O resultado da leitura de um evento da NFS-e (<see cref="LeiauteNFSe101.LerEvento"/>), nas versões 1.00 e 1.01: o pedido
/// (<c>pedRegEvento</c>) ou o evento registrado (<c>evento</c>, com o pedido dentro), o Id, a versão, o XML recebido e os
/// problemas.
/// </summary>
/// <remarks>Imutável. Os objetos dentro dele são do modelo e passam a ser de quem leu.</remarks>
public sealed class EventoLido
{
    internal EventoLido(EventoNFSe? evento, EventoNFSeRegistrado? registrado, string? id, string? versao, string xml,
        IReadOnlyList<Problema> problemas)
    {
        Evento = evento;
        Registrado = registrado;
        Id = id;
        Versao = versao;
        Xml = xml;
        Problemas = problemas;
    }

    /// <summary>
    /// O pedido do evento, com o detalhe na subclasse do tipo (no evento registrado, a mesma instância de
    /// <c>Registrado.Pedido</c>); <see langword="null"/> quando o tipo é desconhecido ou o XML não é do leiaute.
    /// </summary>
    public EventoNFSe? Evento { get; }

    /// <summary>O evento registrado; <see langword="null"/> quando o XML é só o pedido.</summary>
    public EventoNFSeRegistrado? Registrado { get; }

    /// <summary>O Id do XML como veio: <c>PRE...</c> no pedido, <c>EVT...</c> no evento registrado.</summary>
    public string? Id { get; }

    /// <summary>A versão do XML (<c>1.00</c> ou <c>1.01</c>); <see langword="null"/> quando ele não é do leiaute.</summary>
    public string? Versao { get; }

    /// <summary>O XML recebido, a mesma instância passada à leitura: é o que se guarda.</summary>
    public string Xml { get; }

    /// <summary>Os problemas da leitura, na ordem do XML.</summary>
    public IReadOnlyList<Problema> Problemas { get; }
}
