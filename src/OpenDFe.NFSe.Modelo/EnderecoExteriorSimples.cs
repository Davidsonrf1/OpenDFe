namespace OpenDFe.NFSe;

/// <summary>O endereço no exterior sem o país (<c>endExt</c>, <c>TCEnderExtSimples</c>), da obra, do imóvel e do evento.</summary>
public sealed class EnderecoExteriorSimples
{
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
