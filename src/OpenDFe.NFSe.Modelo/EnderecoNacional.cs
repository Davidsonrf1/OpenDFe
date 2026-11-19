namespace OpenDFe.NFSe;

/// <summary>O município e o CEP do endereço no Brasil (<c>endNac</c>).</summary>
public sealed class EnderecoNacional
{
    /// <summary>O código IBGE do município.</summary>
    [Tag("cMun")]
    public int? Municipio { get; set; }

    /// <summary>O CEP, só os dígitos.</summary>
    [Tag("CEP")]
    public string? Cep { get; set; }
}
