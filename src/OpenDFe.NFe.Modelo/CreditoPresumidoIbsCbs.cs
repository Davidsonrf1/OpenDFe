namespace OpenDFe.NFe;

/// <summary>O crédito presumido de um tributo da reforma (<c>gIBSCredPres</c> ou <c>gCBSCredPres</c>).</summary>
/// <remarks>
/// Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. O <see cref="Valor"/> e o
/// <see cref="ValorCondicaoSuspensiva"/> são uma escolha do XSD: um dos dois.
/// </remarks>
public sealed class CreditoPresumidoIbsCbs
{
    /// <summary>O percentual do crédito presumido.</summary>
    [Tag("pCredPres")]
    public decimal? Percentual { get; set; }

    /// <summary>O valor do crédito presumido; ou o <see cref="ValorCondicaoSuspensiva"/> (um dos dois).</summary>
    [Tag("vCredPres")]
    public decimal? Valor { get; set; }

    /// <summary>O valor do crédito presumido em condição suspensiva; ou o <see cref="Valor"/> (um dos dois).</summary>
    [Tag("vCredPresCondSus")]
    public decimal? ValorCondicaoSuspensiva { get; set; }
}
