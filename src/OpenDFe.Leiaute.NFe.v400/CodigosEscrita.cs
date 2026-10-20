namespace OpenDFe.NFe;

/// <summary>
/// Os códigos dos problemas que a própria escrita aponta. Não são rejeições da SEFAZ (essas levam o número do MOC, nas
/// regras da fase 3): são da montagem do XML, com o prefixo <c>NFE-</c>.
/// </summary>
internal static class CodigosEscrita
{
    /// <summary>Mais de uma alternativa de uma escolha do XSD preenchida (MODELO.md, decisões 10 e 25 a 27).</summary>
    internal const string Exclusivo = "NFE-EXCLUSIVO";

    /// <summary>Um campo que o núcleo já sabe preenchido com outro valor (MODELO.md, decisão 6).</summary>
    internal const string Nucleo = "NFE-NUCLEO";

    /// <summary>Falta, ou não serve, um dado da chave de acesso: a nota não tem como sair.</summary>
    internal const string Chave = "NFE-CHAVE";

    /// <summary>Um caractere que o XML 1.0 não aceita.</summary>
    internal const string Caractere = "NFE-CARACTERE";

    /// <summary>Um campo preenchido que o leiaute não escreve (PROJETO.md 4.4): aviso.</summary>
    internal const string NaoSuportado = "NFE-NAO-SUPORTADO";
}
