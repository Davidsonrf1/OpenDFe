namespace OpenDFe;

/// <summary>
/// O preparo de um documento, no leiaute: monta o XML que o <see cref="DFe.Preparar{TDocumento}"/> assina e valida
/// sem transmitir (a nota preparada, a NFC-e offline). Registrado por
/// <see cref="DeclaracaoLeiaute.RegistrarPreparo{TDocumento}"/>.
/// </summary>
/// <typeparam name="TDocumento">O modelo do documento (a nota).</typeparam>
/// <remarks>Sem estado e seguro para uso concorrente. Não faz I/O.</remarks>
public interface IPreparo<TDocumento>
    where TDocumento : class
{
    /// <summary>
    /// Monta o documento, sem assinar. Os problemas da montagem vão no <see cref="DocumentoMontado.Problemas"/>;
    /// exceção aqui é erro de programação e sobe como veio.
    /// </summary>
    /// <param name="documento">O documento recebido pelo núcleo.</param>
    /// <param name="contexto">O documento e a configuração do núcleo.</param>
    /// <returns>O documento montado.</returns>
    DocumentoMontado Montar(TDocumento documento, ContextoOperacao contexto);
}
