namespace OpenDFe.NFe;

/// <summary>
/// Os códigos dos problemas da leitura (docs/LEITURA.md 4), com o prefixo <c>NFE-</c>, como os da escrita
/// (<see cref="CodigosEscrita"/>). Não são rejeições da SEFAZ.
/// </summary>
internal static class CodigosLeitura
{
    /// <summary>O texto não é XML bem formado: sem nota.</summary>
    internal const string Xml = "NFE-XML";

    /// <summary>A raiz não é <c>NFe</c> nem <c>nfeProc</c> no namespace da NF-e, ou falta a nota dentro dela: sem nota.</summary>
    internal const string Raiz = "NFE-RAIZ";

    /// <summary>A versão não é a 4.00: é outro leiaute, sem nota.</summary>
    internal const string Versao = "NFE-VERSAO";

    /// <summary>O <c>Id</c> falta ou não tem uma chave válida, ou o <c>cDV</c> difere do dígito da chave.</summary>
    internal const string Chave = "NFE-CHAVE";

    /// <summary>A chave do protocolo difere da chave da nota.</summary>
    internal const string Protocolo = "NFE-PROTOCOLO";

    /// <summary>O valor não converte ao tipo da propriedade: fica nulo.</summary>
    internal const string Valor = "NFE-VALOR";

    /// <summary>Um campo de NT anterior que saiu do schema (docs/LEITURA.md 5): aviso.</summary>
    internal const string Retirado = "NFE-RETIRADO";

    /// <summary>Um elemento, atributo ou texto que o leiaute não conhece: aviso.</summary>
    internal const string Desconhecido = "NFE-DESCONHECIDO";

    /// <summary>Um elemento que o leiaute aceita uma vez, repetido: vale o primeiro, aviso.</summary>
    internal const string Repetido = "NFE-REPETIDO";

    /// <summary>O código fora da lista da enumeração: entra como o número, aviso.</summary>
    internal const string Enumeracao = "NFE-ENUMERACAO";

    /// <summary>O <c>nItem</c> fora da sequência 1 a n, repetido ou ausente: aviso.</summary>
    internal const string Item = "NFE-ITEM";
}
