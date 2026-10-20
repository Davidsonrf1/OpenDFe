namespace OpenDFe.NFe;

/// <summary>O IPI tributado (<c>IPITrib</c>).</summary>
/// <remarks>
/// A base e a alíquota (<see cref="BaseCalculo"/> e <see cref="Aliquota"/>) ou a quantidade e o valor por unidade
/// (<see cref="Quantidade"/> e <see cref="ValorUnidade"/>): escolha do XSD.
/// </remarks>
public sealed class IpiTributado
{
    /// <summary>O CST: <c>00</c>, <c>49</c>, <c>50</c> ou <c>99</c>.</summary>
    [Tag("CST", "O09")]
    public CstIpi? Cst { get; set; }

    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBC", "O10")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota; opcional.</summary>
    [Tag("pIPI", "O13")]
    public decimal? Aliquota { get; set; }

    /// <summary>A quantidade total na unidade padrão de tributação; opcional.</summary>
    [Tag("qUnid", "O11")]
    public decimal? Quantidade { get; set; }

    /// <summary>O valor por unidade tributável; opcional.</summary>
    [Tag("vUnid", "O12")]
    public decimal? ValorUnidade { get; set; }

    /// <summary>O valor do IPI.</summary>
    [Tag("vIPI", "O14")]
    public decimal? Valor { get; set; }
}
