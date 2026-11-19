namespace OpenDFe.NFSe;

/// <summary>A tributação regular do IBS e da CBS (<c>gTribRegular</c> da DPS).</summary>
public sealed class TributacaoRegularIbsCbs
{
    /// <summary>O código de situação tributária da tributação regular.</summary>
    [Tag("CSTReg")]
    public string? Cst { get; set; }

    /// <summary>O código de classificação tributária da tributação regular.</summary>
    [Tag("cClassTribReg")]
    public string? ClassificacaoTributaria { get; set; }
}
