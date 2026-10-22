namespace OpenDFe;

/// <summary>
/// Um documento dentro da mensagem do pedido (<see cref="PedidoWS.Documentos"/>): a nota do lote. O núcleo o acha no
/// XML assinado pelo <see cref="Id"/> e o entrega às regras do leiaute (<see cref="DeclaracaoLeiaute.DeclararRegras"/>)
/// e aos callbacks de quem usa (<see cref="ConfiguracaoDFe.Validacao"/>), como um <see cref="DocumentoVerificado"/>.
/// </summary>
public sealed class DocumentoPedido
{
    /// <summary>Cria o documento.</summary>
    /// <param name="id">
    /// O <c>Id</c> do elemento assinado (<c>"NFe"</c> + a chave, do <c>infNFe</c>). O documento é o elemento pai dele (o
    /// <c>NFe</c>), como na assinatura enveloped de todo DF-e.
    /// </param>
    /// <exception cref="ArgumentException">O <c>Id</c> é nulo ou vazio.</exception>
    public DocumentoPedido(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        Id = id;
    }

    /// <summary>O <c>Id</c> do elemento assinado; o documento é o pai dele.</summary>
    public string Id { get; }

    /// <summary>
    /// O modelo do leiaute (no OpenDFe.NFe, a nota); <see langword="null"/> quando o documento veio só como XML.
    /// </summary>
    public object? Modelo { get; init; }

    /// <summary>A chave de acesso, quando o documento tem uma.</summary>
    public ChaveAcesso? Chave { get; init; }

    /// <summary>
    /// A conversão do caminho de um campo do documento (<c>"NFe/infNFe/det[3]/prod/xProd"</c>) na propriedade do
    /// modelo (<c>"Itens[2].Produto.Descricao"</c>), ou <see langword="null"/> quando o caminho não tem propriedade;
    /// a mesma do <see cref="SchemaXml"/>. Nula: os campos saem sem propriedade.
    /// </summary>
    public Func<string, string?>? Propriedade { get; init; }
}
