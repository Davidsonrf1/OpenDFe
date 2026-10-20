namespace OpenDFe.NFe;

/// <summary>
/// O resultado da escrita da nota: o XML do <c>NFe</c>, sem a assinatura, a chave e os problemas da montagem. A escrita
/// não altera a nota: o <c>cNF</c> sorteado e a chave ficam aqui (MODELO.md 6).
/// </summary>
internal sealed class NotaEscrita
{
    internal NotaEscrita(string xml, ChaveAcesso? chave, IReadOnlyList<Problema> problemas)
    {
        Xml = xml;
        Chave = chave;
        Problemas = problemas;
    }

    /// <summary>
    /// O <c>NFe</c>, com o namespace sem prefixo, sem declaração XML, sem espaço entre as tags e sem a
    /// <c>Signature</c>, que o núcleo acrescenta ao assinar o <c>infNFe</c> pelo <see cref="Id"/>.
    /// </summary>
    internal string Xml { get; }

    /// <summary>A chave de acesso; nula quando faltou um dado dela (com o problema).</summary>
    internal ChaveAcesso? Chave { get; }

    /// <summary>O <c>Id</c> do <c>infNFe</c> (<c>NFe</c> + a chave), o que o núcleo assina.</summary>
    internal string? Id => Chave is null ? null : "NFe" + Chave;

    /// <summary>
    /// Os problemas da montagem: os erros que impedem o envio (escolha com duas alternativas, campo diferente do núcleo,
    /// dado da chave, caractere que o XML não aceita) e os avisos do campo não suportado (PROJETO.md 4.4).
    /// </summary>
    internal IReadOnlyList<Problema> Problemas { get; }
}
