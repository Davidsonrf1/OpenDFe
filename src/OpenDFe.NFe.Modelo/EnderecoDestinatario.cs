namespace OpenDFe.NFe;

/// <summary>O endereço do destinatário (<c>enderDest</c>, o tipo <c>TEndereco</c>): aceita o exterior.</summary>
public sealed class EnderecoDestinatario
{
    /// <summary>O logradouro.</summary>
    [Tag("xLgr", "E06")]
    public string? Logradouro { get; set; }

    /// <summary>O número.</summary>
    [Tag("nro", "E07")]
    public string? Numero { get; set; }

    /// <summary>O complemento; opcional.</summary>
    [Tag("xCpl", "E08")]
    public string? Complemento { get; set; }

    /// <summary>O bairro.</summary>
    [Tag("xBairro", "E09")]
    public string? Bairro { get; set; }

    /// <summary>O código do município (IBGE); 9999999 no exterior.</summary>
    [Tag("cMun", "E10")]
    public int? CodigoMunicipio { get; set; }

    /// <summary>O nome do município; <c>EXTERIOR</c> no exterior.</summary>
    [Tag("xMun", "E11")]
    public string? Municipio { get; set; }

    /// <summary>A UF; nula no exterior (<see cref="Exterior"/>).</summary>
    [Tag("UF", "E12")]
    public UF? UF { get; set; }

    /// <summary>
    /// Se a UF do XML é <c>EX</c> (exterior); com ela, <see cref="UF"/> fica nulo. Sem <see cref="TagAttribute"/>: é
    /// outro valor do mesmo campo <c>UF</c>.
    /// </summary>
    public bool Exterior { get; set; }

    /// <summary>O CEP; opcional.</summary>
    [Tag("CEP", "E13")]
    public string? Cep { get; set; }

    /// <summary>O código do país; opcional.</summary>
    [Tag("cPais", "E14")]
    public int? CodigoPais { get; set; }

    /// <summary>O nome do país; opcional.</summary>
    [Tag("xPais", "E15")]
    public string? Pais { get; set; }

    /// <summary>O telefone; opcional.</summary>
    [Tag("fone", "E16")]
    public string? Telefone { get; set; }
}
