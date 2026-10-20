namespace OpenDFe.NFe;

/// <summary>O transportador (<c>transporta</c>).</summary>
public sealed class Transportador
{
    /// <summary>O CNPJ; ou o <see cref="Cpf"/> (um dos dois, ou nenhum).</summary>
    [Tag("CNPJ", "X04")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF; ou o <see cref="Cnpj"/> (um dos dois, ou nenhum).</summary>
    [Tag("CPF", "X05")]
    public string? Cpf { get; set; }

    /// <summary>A razão social ou o nome; opcional.</summary>
    [Tag("xNome", "X06")]
    public string? Nome { get; set; }

    /// <summary>A inscrição estadual, ou <c>ISENTO</c>; opcional.</summary>
    [Tag("IE", "X07")]
    public string? InscricaoEstadual { get; set; }

    /// <summary>O endereço completo, em texto; opcional.</summary>
    [Tag("xEnder", "X08")]
    public string? Endereco { get; set; }

    /// <summary>O nome do município; opcional.</summary>
    [Tag("xMun", "X09")]
    public string? Municipio { get; set; }

    /// <summary>A UF; opcional, nula no exterior (<see cref="Exterior"/>).</summary>
    [Tag("UF", "X10")]
    public UF? UF { get; set; }

    /// <summary>
    /// Se a UF do XML é <c>EX</c> (exterior); com ela, <see cref="UF"/> fica nulo. Sem <see cref="TagAttribute"/>: é
    /// outro valor do mesmo campo <c>UF</c>.
    /// </summary>
    public bool Exterior { get; set; }
}
