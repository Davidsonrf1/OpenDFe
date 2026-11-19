namespace OpenDFe.NFSe;

/// <summary>O endereço nacional do emitente da NFS-e (<c>enderNac</c>).</summary>
public sealed class EnderecoEmitente
{
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

    /// <summary>O código IBGE do município.</summary>
    [Tag("cMun")]
    public int? Municipio { get; set; }

    /// <summary>A UF.</summary>
    [Tag("UF")]
    public UF? UF { get; set; }

    /// <summary>O CEP, só os dígitos.</summary>
    [Tag("CEP")]
    public string? Cep { get; set; }
}
