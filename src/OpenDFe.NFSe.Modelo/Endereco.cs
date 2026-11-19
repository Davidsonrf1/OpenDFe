namespace OpenDFe.NFSe;

/// <summary>Um endereço da DPS (<c>TCEndereco</c>): o nacional ou o do exterior, e o logradouro.</summary>
public sealed class Endereco
{
    /// <summary>O município e o CEP, no Brasil; um dos dois (nacional ou exterior).</summary>
    [Tag("endNac")]
    public EnderecoNacional? Nacional { get; set; }

    /// <summary>O país, o código postal e a cidade, no exterior.</summary>
    [Tag("endExt")]
    public EnderecoExterior? Exterior { get; set; }

    /// <summary>O tipo e o nome do logradouro.</summary>
    [Tag("xLgr")]
    public string? Logradouro { get; set; }

    /// <summary>O número.</summary>
    [Tag("nro")]
    public string? Numero { get; set; }

    /// <summary>O complemento; opcional.</summary>
    [Tag("xCpl")]
    public string? Complemento { get; set; }

    /// <summary>O bairro.</summary>
    [Tag("xBairro")]
    public string? Bairro { get; set; }
}
