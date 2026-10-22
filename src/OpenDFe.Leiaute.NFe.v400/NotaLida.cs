namespace OpenDFe.NFe;

/// <summary>
/// O resultado da leitura de uma NF-e ou NFC-e (<see cref="LeiauteNFe400.Ler"/>): a nota, a chave, o protocolo, o XML
/// recebido e os problemas (docs/LEITURA.md).
/// </summary>
/// <remarks>
/// Imutável. A nota dentro dela é do modelo, mutável, e passa a ser de quem leu: a lib não guarda referência.
/// </remarks>
public sealed class NotaLida
{
    internal NotaLida(NotaFiscal? nota, ChaveAcesso? chave, Protocolo? protocolo, string xml, IReadOnlyList<Problema> problemas)
    {
        Nota = nota;
        Chave = chave;
        Protocolo = protocolo;
        Xml = xml;
        Problemas = problemas;
    }

    /// <summary>
    /// A nota; <see langword="null"/> só quando o XML não é uma NF-e do leiaute 4.00 (<c>NFE-XML</c>, <c>NFE-RAIZ</c>,
    /// <c>NFE-VERSAO</c>).
    /// </summary>
    public NotaFiscal? Nota { get; }

    /// <summary>A chave do <c>Id</c> do <c>infNFe</c>; <see langword="null"/> quando ausente ou inválida (<c>NFE-CHAVE</c>).</summary>
    public ChaveAcesso? Chave { get; }

    /// <summary>O protocolo do <c>nfeProc</c>; <see langword="null"/> no <c>NFe</c> sozinho.</summary>
    public Protocolo? Protocolo { get; }

    /// <summary>
    /// O XML recebido, a mesma instância passada à leitura: é o que se guarda (PROJETO.md 5.3), nunca o regerado, porque a
    /// escrita de novo não reproduz os bytes da assinatura.
    /// </summary>
    public string Xml { get; }

    /// <summary>Os problemas da leitura, na ordem do XML (docs/LEITURA.md 4).</summary>
    public IReadOnlyList<Problema> Problemas { get; }
}
