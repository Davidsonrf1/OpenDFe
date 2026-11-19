namespace OpenDFe.NFSe;

/// <summary>
/// A DPS escrita sem transmitir (<see cref="LeiauteNFSe101.Escrever"/>): o XML sem a assinatura, o identificador e os
/// problemas da montagem. A escrita não altera a DPS: o identificador fica aqui.
/// </summary>
/// <remarks>Imutável.</remarks>
public sealed class DpsEscrita
{
    internal DpsEscrita(string xml, IdentificadorDps? id, IReadOnlyList<Problema> problemas)
    {
        Xml = xml;
        Id = id;
        Problemas = problemas;
    }

    /// <summary>
    /// O <c>DPS</c> 1.01, com o namespace sem prefixo, sem declaração XML, sem espaço entre as tags e sem a
    /// <c>Signature</c>, que se acrescenta ao assinar o <c>infDPS</c> pelo <see cref="Id"/>.
    /// </summary>
    public string Xml { get; }

    /// <summary>O identificador da DPS, o Id do <c>infDPS</c>; nulo quando faltou um dado dele (com o problema).</summary>
    public IdentificadorDps? Id { get; }

    /// <summary>
    /// Os problemas da montagem: os erros que impedem o envio (escolha com mais de uma alternativa, ambiente diferente do
    /// núcleo, dado do identificador, caractere que o XML não aceita) e os avisos dos campos só da versão 1.00. O schema e
    /// as regras não são conferidos aqui.
    /// </summary>
    public IReadOnlyList<Problema> Problemas { get; }
}
