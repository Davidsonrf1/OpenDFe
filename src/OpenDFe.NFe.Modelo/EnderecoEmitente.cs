namespace OpenDFe.NFe;

/// <summary>O endereço do emitente (<c>enderEmit</c>, o tipo <c>TEnderEmi</c>): só UF do Brasil.</summary>
public sealed class EnderecoEmitente
{
    /// <summary>O logradouro.</summary>
    [Tag("xLgr", "C06")]
    public string? Logradouro { get; set; }

    /// <summary>O número.</summary>
    [Tag("nro", "C07")]
    public string? Numero { get; set; }

    /// <summary>O complemento; opcional.</summary>
    [Tag("xCpl", "C08")]
    public string? Complemento { get; set; }

    /// <summary>O bairro.</summary>
    [Tag("xBairro", "C09")]
    public string? Bairro { get; set; }

    /// <summary>O código do município (IBGE).</summary>
    [Tag("cMun", "C10")]
    public int? CodigoMunicipio { get; set; }

    /// <summary>O nome do município.</summary>
    [Tag("xMun", "C11")]
    public string? Municipio { get; set; }

    /// <summary>A UF.</summary>
    [Tag("UF", "C12")]
    public UF? UF { get; set; }

    /// <summary>O CEP.</summary>
    [Tag("CEP", "C13")]
    public string? Cep { get; set; }

    /// <summary>O código do país (1058, Brasil); opcional.</summary>
    [Tag("cPais", "C14")]
    public int? CodigoPais { get; set; }

    /// <summary>O nome do país; opcional.</summary>
    [Tag("xPais", "C15")]
    public string? Pais { get; set; }

    /// <summary>O telefone; opcional.</summary>
    [Tag("fone", "C16")]
    public string? Telefone { get; set; }
}
