namespace OpenDFe;

/// <summary>
/// Falha de infraestrutura depois de montado e validado o pedido: rede, TLS, tempo esgotado, HTTP diferente de
/// 200, falha SOAP, resposta ilegível (PROJETO.md 2.4). A espécie está em <see cref="Tipo"/>; a causa, em
/// <see cref="Exception.InnerException"/>. Criada pelo núcleo.
/// </summary>
/// <remarks>
/// A mensagem não traz o XML; o <see cref="Rastro"/> traz, com os dados pessoais do documento: não o registre
/// em log.
/// </remarks>
public sealed class FalhaTransmissaoException : Exception
{
    // Sem os construtores padrão de exceção (CA1032): o rastro é obrigatório, e quem cria a falha é o núcleo.
    internal FalhaTransmissaoException(
        string mensagem, RastroWS rastro, TipoFalhaTransmissao tipo, bool podeTerSidoProcessado, Exception causa)
        : base(mensagem, causa)
    {
        Rastro = rastro;
        Tipo = tipo;
        PodeTerSidoProcessado = podeTerSidoProcessado;
    }

    /// <summary>
    /// O que foi enviado (e recebido, se a resposta veio e não se leu: na falha SOAP e no HTTP sem envelope, o corpo
    /// como veio), a URL e os tempos.
    /// </summary>
    public RastroWS Rastro { get; }

    /// <summary>A espécie da falha.</summary>
    public TipoFalhaTransmissao Tipo { get; }

    /// <summary>
    /// O pedido saiu inteiro e a resposta não veio (ou não se leu): o autorizador pode tê-lo processado. Na
    /// autorização, é o caso da recuperação pela chave (PROJETO.md 8.6). <see langword="false"/>: com certeza não
    /// foi processado (DNS, conexão recusada, TLS, certificado recusado, falha SOAP do remetente, HTTP 4xx).
    /// </summary>
    public bool PodeTerSidoProcessado { get; }
}
