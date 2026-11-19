namespace OpenDFe;

/// <summary>
/// O handler de uma operação REST, no leiaute (a NFS-e nacional; docs/NFSE.md, D4): monta o pedido e lê a resposta.
/// Registrado por <see cref="DeclaracaoLeiaute.RegistrarOperacao{TParametro, TRetorno}(IOperacaoRest{TParametro, TRetorno})"/>.
/// </summary>
/// <typeparam name="TParametro">O parâmetro da operação (o tipo exato, selado).</typeparam>
/// <typeparam name="TRetorno">O retorno da operação.</typeparam>
/// <remarks>
/// Sem estado, sem I/O e seguro para uso concorrente, como o <see cref="IOperacao{TParametro, TRetorno}"/>: o núcleo
/// assina e valida o XML do pedido, compacta (GZip e Base64) no campo do JSON, escolhe a URL e transmite pelas
/// <see cref="ConfiguracaoDFe.Conexoes"/>.
/// </remarks>
public interface IOperacaoRest<TParametro, TRetorno>
    where TParametro : ParametroWS<TRetorno>
    where TRetorno : class
{
    /// <summary>
    /// Monta o pedido, sem assinar nem compactar. Os problemas da montagem vão no <see cref="PedidoRest.Problemas"/>;
    /// exceção aqui é erro de programação e sobe como veio.
    /// </summary>
    /// <param name="parametro">O parâmetro recebido pelo núcleo.</param>
    /// <param name="contexto">O documento e a configuração do núcleo.</param>
    /// <returns>O pedido.</returns>
    PedidoRest Montar(TParametro parametro, ContextoOperacao contexto);

    /// <summary>
    /// Lê a resposta. O corpo que não se lê é <see cref="System.Xml.XmlException"/>, <see cref="FormatException"/> ou
    /// <see cref="System.Text.Json.JsonException"/>: o núcleo a embrulha numa <see cref="FalhaTransmissaoException"/>
    /// com o rastro. Outra exceção é erro de programação e sobe como veio.
    /// </summary>
    /// <param name="parametro">O parâmetro recebido pelo núcleo.</param>
    /// <param name="xmlPedido">O XML como foi enviado, assinado; <see langword="null"/> no pedido sem corpo.</param>
    /// <param name="resposta">O status e o corpo.</param>
    /// <param name="contexto">O documento e a configuração do núcleo.</param>
    /// <returns>O retorno lido, com o status HTTP no <see cref="LeituraWS{TRetorno}.CStat"/>.</returns>
    LeituraWS<TRetorno> Ler(TParametro parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto);
}
