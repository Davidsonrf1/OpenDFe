namespace OpenDFe.NFe;

/// <summary>A devolução de um tributo da reforma (<c>gDevTrib</c>).</summary>
/// <remarks>
/// Uma classe para os três lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no IBS da UF, no do município e
/// na CBS.
/// </remarks>
public sealed class DevolucaoTributo
{
    /// <summary>O percentual da devolução; opcional.</summary>
    [Tag("pDevTrib")]
    public decimal? Percentual { get; set; }

    /// <summary>O valor da devolução.</summary>
    [Tag("vDevTrib")]
    public decimal? Valor { get; set; }
}
