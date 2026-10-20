namespace OpenDFe.NFe;

/// <summary>Uma pessoa autorizada a obter o XML da nota (<c>autXML</c>).</summary>
public sealed class PessoaAutorizada
{
    /// <summary>O CNPJ; ou o <see cref="Cpf"/> (um dos dois).</summary>
    [Tag("CNPJ", "GA02")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF; ou o <see cref="Cnpj"/> (um dos dois).</summary>
    [Tag("CPF", "GA03")]
    public string? Cpf { get; set; }
}
