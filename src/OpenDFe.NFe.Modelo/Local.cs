namespace OpenDFe.NFe;

/// <summary>Um local de retirada (<c>retirada</c>) ou de entrega (<c>entrega</c>), o tipo <c>TLocal</c>.</summary>
/// <remarks>
/// Uma classe para os dois lugares: o <see cref="TagAttribute"/> dos campos leva só a tag, sem o identificador do MOC,
/// que muda com o lugar (<c>xLgr</c> é F03 na retirada e G03 na entrega); o identificador do grupo fica na propriedade
/// da <see cref="NotaFiscal"/>.
/// </remarks>
public sealed class Local
{
    /// <summary>O CNPJ; ou o <see cref="Cpf"/> (um dos dois).</summary>
    [Tag("CNPJ")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF; ou o <see cref="Cnpj"/> (um dos dois).</summary>
    [Tag("CPF")]
    public string? Cpf { get; set; }

    /// <summary>A razão social ou o nome do expedidor ou do recebedor; opcional.</summary>
    [Tag("xNome")]
    public string? Nome { get; set; }

    /// <summary>O logradouro.</summary>
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

    /// <summary>O código do município (IBGE); 9999999 no exterior.</summary>
    [Tag("cMun")]
    public int? CodigoMunicipio { get; set; }

    /// <summary>O nome do município; <c>EXTERIOR</c> no exterior.</summary>
    [Tag("xMun")]
    public string? Municipio { get; set; }

    /// <summary>A UF; nula no exterior (<see cref="Exterior"/>).</summary>
    [Tag("UF")]
    public UF? UF { get; set; }

    /// <summary>
    /// Se a UF do XML é <c>EX</c> (exterior); com ela, <see cref="UF"/> fica nulo. Sem <see cref="TagAttribute"/>: é
    /// outro valor do mesmo campo <c>UF</c>.
    /// </summary>
    public bool Exterior { get; set; }

    /// <summary>O CEP; opcional.</summary>
    [Tag("CEP")]
    public string? Cep { get; set; }

    /// <summary>O código do país; opcional.</summary>
    [Tag("cPais")]
    public int? CodigoPais { get; set; }

    /// <summary>O nome do país; opcional.</summary>
    [Tag("xPais")]
    public string? Pais { get; set; }

    /// <summary>O telefone; opcional.</summary>
    [Tag("fone")]
    public string? Telefone { get; set; }

    /// <summary>O e-mail; opcional.</summary>
    [Tag("email")]
    public string? Email { get; set; }

    /// <summary>A inscrição estadual; opcional.</summary>
    [Tag("IE")]
    public string? InscricaoEstadual { get; set; }
}
