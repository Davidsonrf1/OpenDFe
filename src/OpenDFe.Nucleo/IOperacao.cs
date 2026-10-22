namespace OpenDFe;

/// <summary>
/// O handler de uma operação, no leiaute: monta o pedido e lê o retorno. Registrado por
/// <see cref="DeclaracaoLeiaute.RegistrarOperacao{TParametro, TRetorno}"/>.
/// </summary>
/// <typeparam name="TParametro">O parâmetro da operação (o tipo exato, selado).</typeparam>
/// <typeparam name="TRetorno">O retorno da operação.</typeparam>
/// <remarks>
/// Sem estado e seguro para uso concorrente. Não faz I/O: o núcleo assina, valida pelo schema, escolhe a URL e
/// transmite (o caminho de <see cref="DFe.ExecutarAsync{TRetorno}"/>).
/// </remarks>
public interface IOperacao<TParametro, TRetorno>
    where TParametro : ParametroWS<TRetorno>
    where TRetorno : class
{
    /// <summary>
    /// Monta a mensagem (o conteúdo do <c>nfeDadosMsg</c>), sem assinar. Os problemas da montagem vão no
    /// <see cref="PedidoWS.Problemas"/>; exceção aqui é erro de programação e sobe como veio.
    /// </summary>
    /// <param name="parametro">O parâmetro recebido pelo núcleo.</param>
    /// <param name="contexto">O documento e a configuração do núcleo.</param>
    /// <returns>O pedido.</returns>
    PedidoWS Montar(TParametro parametro, ContextoOperacao contexto);

    /// <summary>
    /// Lê o retorno (o conteúdo do <c>nfeResultMsg</c>). O XML que não se lê é
    /// <see cref="System.Xml.XmlException"/> ou <see cref="FormatException"/>: o núcleo a embrulha numa
    /// <see cref="FalhaTransmissaoException"/> com o rastro. Outra exceção é erro de programação e sobe como veio.
    /// </summary>
    /// <param name="parametro">O parâmetro recebido pelo núcleo.</param>
    /// <param name="xmlPedido">
    /// A mensagem como foi enviada, assinada: os bytes dos documentos que o retorno completa (a nota no <c>nfeProc</c>).
    /// </param>
    /// <param name="xmlRetorno">A resposta do autorizador.</param>
    /// <param name="contexto">O documento e a configuração do núcleo.</param>
    /// <returns>O retorno lido.</returns>
    LeituraWS<TRetorno> Ler(TParametro parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto);
}
