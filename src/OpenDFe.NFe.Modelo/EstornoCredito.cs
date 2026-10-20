namespace OpenDFe.NFe;

/// <summary>O estorno de crédito do IBS e da CBS (<c>gEstornoCred</c>).</summary>
/// <remarks>
/// Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no item e no total.
/// </remarks>
public sealed class EstornoCredito
{
    /// <summary>O valor do IBS estornado.</summary>
    [Tag("vIBSEstCred")]
    public decimal? ValorIbs { get; set; }

    /// <summary>O valor da CBS estornada.</summary>
    [Tag("vCBSEstCred")]
    public decimal? ValorCbs { get; set; }
}
