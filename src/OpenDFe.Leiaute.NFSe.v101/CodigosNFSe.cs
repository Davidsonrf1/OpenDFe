namespace OpenDFe.NFSe;

/// <summary>
/// Os códigos dos problemas da escrita e da leitura da NFS-e, que não são rejeição da Sefin (as regras, em
/// <see cref="RegrasNFSe101"/>, têm os seus). Como os <c>NFE-...</c> do leiaute da NF-e.
/// </summary>
internal static class CodigosNFSe
{
    /// <summary>Mais de uma alternativa de uma escolha do XSD preenchida.</summary>
    internal const string Exclusivo = "NFSE-EXCLUSIVO";

    /// <summary>Campo que o núcleo já sabe (o ambiente) preenchido com outro valor.</summary>
    internal const string Nucleo = "NFSE-NUCLEO";

    /// <summary>Dado que falta para montar o Id da DPS (ou do evento).</summary>
    internal const string Id = "NFSE-ID";

    /// <summary>Caractere que o XML não aceita.</summary>
    internal const string Caractere = "NFSE-CARACTERE";

    /// <summary>Campo preenchido que a versão escrita não tem (os da 1.00, M10).</summary>
    internal const string NaoSuportado = "NFSE-NAO-SUPORTADO";

    /// <summary>Texto que não é XML bem formado.</summary>
    internal const string Xml = "NFSE-XML";

    /// <summary>Raiz que o leiaute não lê.</summary>
    internal const string Raiz = "NFSE-RAIZ";

    /// <summary>Versão que o leiaute não lê.</summary>
    internal const string Versao = "NFSE-VERSAO";

    /// <summary>Id (a chave da NFS-e, o identificador da DPS) ausente ou inválido.</summary>
    internal const string Chave = "NFSE-CHAVE-ID";

    /// <summary>Valor que não converte.</summary>
    internal const string Valor = "NFSE-VALOR";

    /// <summary>Código de enumeração fora da lista.</summary>
    internal const string Enumeracao = "NFSE-ENUMERACAO";

    /// <summary>Campo que o leiaute não conhece.</summary>
    internal const string Desconhecido = "NFSE-DESCONHECIDO";

    /// <summary>Campo que não se repete e veio repetido.</summary>
    internal const string Repetido = "NFSE-REPETIDO";

    /// <summary>Evento de tipo desconhecido ou com o texto fixo de outro tipo.</summary>
    internal const string Evento = "NFSE-EVENTO";

    /// <summary>Campo da resposta da parametrização municipal que não se lê (docs/NFSE.md 11, P2): fica nulo, com o JSON ao lado.</summary>
    internal const string Parametro = "NFSE-PARAMETRO";
}
