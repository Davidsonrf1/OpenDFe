namespace OpenDFe;

/// <summary>
/// Um documento inteiro (cada nota do lote), entregue ao callback <see cref="ValidacaoCallback.Documento"/>.
/// Criado pelo núcleo.
/// </summary>
public sealed class DocumentoVerificado
{
    internal DocumentoVerificado(Documento documento, object? modelo, string xml, ChaveAcesso? chave)
    {
        Documento = documento;
        Modelo = modelo;
        Xml = xml;
        Chave = chave;
    }

    /// <summary>O documento (NF-e, NFC-e).</summary>
    public Documento Documento { get; }

    /// <summary>
    /// O modelo do leiaute (no OpenDFe.NFe, a nota), para quem usa converter para o tipo que conhece;
    /// <see langword="null"/> quando o documento veio só como XML.
    /// </summary>
    public object? Modelo { get; }

    /// <summary>O XML do documento. Tem dados pessoais.</summary>
    public string Xml { get; }

    /// <summary>A chave de acesso, quando o documento tem uma.</summary>
    public ChaveAcesso? Chave { get; }
}
