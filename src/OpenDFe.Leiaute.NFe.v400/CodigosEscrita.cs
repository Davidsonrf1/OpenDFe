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

    /// <summary>
    /// O QR Code não pôde ser gerado (docs/CONTRATOS.md 16, decisões 5 e 9): a UF sem URL de consulta na tabela e sem a
    /// troca na configuração, ou a contingência offline (que entra na etapa 5.2).
    /// </summary>
    internal const string QrCode = "NFE-QRCODE";

    /// <summary>
    /// A composição do lote da autorização (docs/CONTRATOS.md 14, decisão 10): a mesma chave duas vezes, ou tipos de
    /// emissão diferentes.
    /// </summary>
    internal const string Lote = "NFE-LOTE";

    /// <summary>
    /// A UF consultada não tem o serviço de cadastro (docs/CONTRATOS.md 24): só as 10 UFs com autorizador próprio de NF-e e,
    /// na SVRS, AC, ES, RN, PB e SC.
    /// </summary>
    internal const string Cadastro = "NFE-CADASTRO";
}
