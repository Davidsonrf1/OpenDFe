namespace OpenDFe.NFSe;

/// <summary>A situação e a classificação tributária do IBS e da CBS (<c>gIBSCBS</c>).</summary>
public sealed class ClassificacaoIbsCbs
{
    /// <summary>O código de situação tributária (3 dígitos).</summary>
    [Tag("CST")]
    public string? Cst { get; set; }

    /// <summary>O código de classificação tributária (6 dígitos).</summary>
    [Tag("cClassTrib")]
    public string? ClassificacaoTributaria { get; set; }

    /// <summary>O código do crédito presumido (2 dígitos); opcional.</summary>
    [Tag("cCredPres")]
    public string? CreditoPresumido { get; set; }

    /// <summary>A tributação regular; opcional.</summary>
    [Tag("gTribRegular")]
    public TributacaoRegularIbsCbs? TributacaoRegular { get; set; }

    /// <summary>O diferimento; opcional.</summary>
    [Tag("gDif")]
    public DiferimentoIbsCbs? Diferimento { get; set; }
}
