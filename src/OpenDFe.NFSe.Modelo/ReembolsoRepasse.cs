namespace OpenDFe.NFSe;

/// <summary>Os reembolsos, repasses e ressarcimentos (<c>gReeRepRes</c>).</summary>
public sealed class ReembolsoRepasse
{
    /// <summary>Os documentos (1 a 1.000).</summary>
    [Tag("documentos")]
    public IList<DocumentoReembolso> Documentos { get; } = new List<DocumentoReembolso>();
}
