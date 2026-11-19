namespace OpenDFe.NFSe;

/// <summary>O endereço no exterior (<c>endExt</c>, <c>TCEnderExt</c>).</summary>
public sealed class EnderecoExterior
{
    /// <summary>O código do país (ISO 3166-1, duas letras).</summary>
    [Tag("cPais")]
    public string? Pais { get; set; }

    /// <summary>O código de endereçamento postal.</summary>
    [Tag("cEndPost")]
    public string? CodigoPostal { get; set; }

    /// <summary>A cidade.</summary>
    [Tag("xCidade")]
    public string? Cidade { get; set; }

    /// <summary>O estado, a província ou a região.</summary>
    [Tag("xEstProvReg")]
    public string? EstadoProvincia { get; set; }
}
