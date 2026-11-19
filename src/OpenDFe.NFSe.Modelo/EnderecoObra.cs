namespace OpenDFe.NFSe;

/// <summary>O endereço da obra e do imóvel (<c>TCEnderObraEvento</c>).</summary>
public sealed class EnderecoObra
{
    /// <summary>O CEP, só os dígitos; um dos dois (CEP ou exterior).</summary>
    [Tag("CEP")]
    public string? Cep { get; set; }

    /// <summary>O endereço no exterior.</summary>
    [Tag("endExt")]
    public EnderecoExteriorSimples? Exterior { get; set; }

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
