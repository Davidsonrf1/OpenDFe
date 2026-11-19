namespace OpenDFe.NFSe;

/// <summary>Os documentos da dedução ou da redução (<c>documentos</c>).</summary>
public sealed class DocumentosDeducao
{
    /// <summary>Os documentos (1 a 1.000).</summary>
    [Tag("docDedRed")]
    public IList<DocumentoDeducao> Itens { get; } = new List<DocumentoDeducao>();
}
