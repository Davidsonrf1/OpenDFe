namespace OpenDFe.NFSe;

/// <summary>
/// O resultado da leitura de uma NFS-e ou de uma DPS (<see cref="LeiauteNFSe101.Ler"/>), nas versões 1.00 e 1.01: a nota, a
/// DPS, a chave, o identificador da DPS, a versão, o XML recebido e os problemas.
/// </summary>
/// <remarks>
/// Imutável. Os objetos dentro dele são do modelo, mutáveis, e passam a ser de quem leu: a lib não guarda referência.
/// </remarks>
public sealed class NFSeLida
{
    internal NFSeLida(NotaFiscalServico? nota, Dps? dps, ChaveNFSe? chave, IdentificadorDps? identificadorDps, string? versao,
        string xml, IReadOnlyList<Problema> problemas)
    {
        Nota = nota;
        Dps = dps;
        Chave = chave;
        IdentificadorDps = identificadorDps;
        Versao = versao;
        Xml = xml;
        Problemas = problemas;
    }

    /// <summary>A NFS-e; <see langword="null"/> quando o XML é uma DPS sozinha ou não é do leiaute.</summary>
    public NotaFiscalServico? Nota { get; }

    /// <summary>A DPS: a de dentro da NFS-e (a mesma instância de <c>Nota.Dps</c>) ou a DPS sozinha.</summary>
    public Dps? Dps { get; }

    /// <summary>A chave do Id do <c>infNFSe</c>; <see langword="null"/> na DPS sozinha, ausente ou inválida.</summary>
    public ChaveNFSe? Chave { get; }

    /// <summary>O identificador do Id do <c>infDPS</c>; <see langword="null"/> quando ausente ou inválido.</summary>
    public IdentificadorDps? IdentificadorDps { get; }

    /// <summary>A versão do XML (<c>1.00</c> ou <c>1.01</c>); <see langword="null"/> quando ele não é do leiaute.</summary>
    public string? Versao { get; }

    /// <summary>
    /// O XML recebido, a mesma instância passada à leitura: é o que se guarda (PROJETO.md 5.3), nunca o regerado, porque a
    /// escrita de novo não reproduz os bytes da assinatura.
    /// </summary>
    public string Xml { get; }

    /// <summary>Os problemas da leitura, na ordem do XML.</summary>
    public IReadOnlyList<Problema> Problemas { get; }
}
