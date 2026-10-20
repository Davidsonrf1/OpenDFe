namespace OpenDFe.NFe;

/// <summary>O diferimento de um tributo da reforma (<c>gDif</c>).</summary>
/// <remarks>
/// Uma classe para os três lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no IBS da UF, no do município e
/// na CBS.
/// </remarks>
public sealed class Diferimento
{
    /// <summary>O percentual do diferimento.</summary>
    [Tag("pDif")]
    public decimal? Percentual { get; set; }

    /// <summary>O valor do diferimento.</summary>
    [Tag("vDif")]
    public decimal? Valor { get; set; }
}
