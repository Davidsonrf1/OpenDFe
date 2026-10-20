namespace OpenDFe.NFe;

/// <summary>O emitente (<c>emit</c>).</summary>
public sealed class Emitente
{
    /// <summary>O CNPJ; ou o <see cref="Cpf"/> (um dos dois).</summary>
    [Tag("CNPJ", "C02")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF; ou o <see cref="Cnpj"/> (um dos dois).</summary>
    [Tag("CPF", "C02a")]
    public string? Cpf { get; set; }

    /// <summary>A razão social ou o nome.</summary>
    [Tag("xNome", "C03")]
    public string? Nome { get; set; }

    /// <summary>O nome fantasia; opcional.</summary>
    [Tag("xFant", "C04")]
    public string? NomeFantasia { get; set; }

    /// <summary>O endereço.</summary>
    [Tag("enderEmit", "C05")]
    public EnderecoEmitente Endereco { get; set; } = new();

    /// <summary>A inscrição estadual, ou <c>ISENTO</c>; opcional desde a NT 2026.007.</summary>
    [Tag("IE", "C17")]
    public string? InscricaoEstadual { get; set; }

    /// <summary>A inscrição estadual do substituto tributário; opcional.</summary>
    [Tag("IEST", "C18")]
    public string? InscricaoEstadualSubstituto { get; set; }

    /// <summary>A inscrição municipal; opcional, com o <see cref="Cnae"/>.</summary>
    [Tag("IM", "C19")]
    public string? InscricaoMunicipal { get; set; }

    /// <summary>O CNAE fiscal; opcional, só com a <see cref="InscricaoMunicipal"/>.</summary>
    [Tag("CNAE", "C20")]
    public string? Cnae { get; set; }

    /// <summary>O código do regime tributário.</summary>
    [Tag("CRT", "C21")]
    public RegimeTributario? RegimeTributario { get; set; }

    /// <summary>A inscrição na SUFRAMA; opcional.</summary>
    [Tag("ISUFEmit", "C22")]
    public string? InscricaoSuframa { get; set; }
}
