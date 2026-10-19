namespace OpenDFe;

/// <summary>
/// Um campo, como foi escrito no XML, entregue ao callback <see cref="ValidacaoCallback.Campo"/>. Criado pelo
/// núcleo.
/// </summary>
public sealed class CampoVerificado
{
    internal CampoVerificado(Documento documento, string tag, string caminho, string? propriedade, string texto)
    {
        Documento = documento;
        Tag = tag;
        Caminho = caminho;
        Propriedade = propriedade;
        Texto = texto;
    }

    /// <summary>O documento (NF-e, NFC-e).</summary>
    public Documento Documento { get; }

    /// <summary>A tag do campo (<c>"xProd"</c>).</summary>
    public string Tag { get; }

    /// <summary>
    /// O caminho do campo no XML (<c>"NFe/infNFe/det[3]/prod/xProd"</c>), como o <see cref="Problema.Caminho"/>
    /// da validação pelo schema.
    /// </summary>
    public string Caminho { get; }

    /// <summary>
    /// A propriedade do modelo (<c>"Itens[2].Produto.Descricao"</c>), quando o leiaute a conhece.
    /// </summary>
    public string? Propriedade { get; }

    /// <summary>O valor como está no XML. Pode ser dado pessoal.</summary>
    public string Texto { get; }
}
