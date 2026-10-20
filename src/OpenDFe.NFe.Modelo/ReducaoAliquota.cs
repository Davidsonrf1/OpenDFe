namespace OpenDFe.NFe;

/// <summary>A redução da alíquota de um tributo da reforma (<c>gRed</c>).</summary>
/// <remarks>
/// Uma classe para os três lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no IBS da UF, no do município e
/// na CBS.
/// </remarks>
public sealed class ReducaoAliquota
{
    /// <summary>O percentual de redução da alíquota.</summary>
    [Tag("pRedAliq")]
    public decimal? Percentual { get; set; }

    /// <summary>A alíquota efetiva, depois da redução.</summary>
    [Tag("pAliqEfet")]
    public decimal? AliquotaEfetiva { get; set; }
}
