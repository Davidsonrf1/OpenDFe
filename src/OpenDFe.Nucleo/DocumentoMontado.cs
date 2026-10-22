namespace OpenDFe;

/// <summary>
/// O documento montado pelo <see cref="IPreparo{TDocumento}"/>, sem assinar: o XML, o schema dele e o documento que o
/// núcleo assina e entrega às regras e aos callbacks.
/// </summary>
public sealed class DocumentoMontado
{
    private readonly IReadOnlyList<Problema> _problemas = [];

    /// <summary>Cria o documento montado.</summary>
    /// <param name="xml">
    /// O XML do documento (o <c>NFe</c>), sem a assinatura; com problema de severidade Erro em <see cref="Problemas"/>,
    /// pode estar incompleto ou vazio.
    /// </param>
    /// <param name="schema">O schema do documento, validado depois de assinar.</param>
    /// <param name="documento">
    /// O <c>Id</c> que o núcleo assina, o modelo, a chave e a conversão do caminho na propriedade. O
    /// <see cref="DocumentoPedido.Schema"/> não é usado: vale o <paramref name="schema"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Um dos argumentos é nulo.</exception>
    public DocumentoMontado(string xml, SchemaXml schema, DocumentoPedido documento)
    {
        ArgumentNullException.ThrowIfNull(xml);
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(documento);
        Xml = xml;
        Schema = schema;
        Documento = documento;
    }

    /// <summary>O XML do documento, sem a assinatura.</summary>
    public string Xml { get; }

    /// <summary>O schema do documento.</summary>
    public SchemaXml Schema { get; }

    /// <summary>O documento: o <c>Id</c> assinado, o modelo, a chave e a conversão.</summary>
    public DocumentoPedido Documento { get; }

    /// <summary>
    /// Os problemas da montagem (regras da escrita, campo não suportado). Com Erro, nada é assinado.
    /// </summary>
    /// <exception cref="ArgumentNullException">A lista ou um dos problemas é nulo.</exception>
    public IReadOnlyList<Problema> Problemas
    {
        get => _problemas;
        init => _problemas = PedidoWS.Copiar(value);
    }
}
