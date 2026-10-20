namespace OpenDFe.NFe;

/// <summary>O destinatário (<c>dest</c>).</summary>
public sealed class Destinatario
{
    /// <summary>O CNPJ; ou o <see cref="Cpf"/> ou o <see cref="IdEstrangeiro"/> (um dos três).</summary>
    [Tag("CNPJ", "E02")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF; ou o <see cref="Cnpj"/> ou o <see cref="IdEstrangeiro"/> (um dos três).</summary>
    [Tag("CPF", "E03")]
    public string? Cpf { get; set; }

    /// <summary>
    /// O documento do estrangeiro; ou o <see cref="Cnpj"/> ou o <see cref="Cpf"/> (um dos três). Texto vazio escreve a
    /// tag vazia (o estrangeiro sem documento).
    /// </summary>
    [Tag("idEstrangeiro", "E03a")]
    public string? IdEstrangeiro { get; set; }

    /// <summary>A razão social ou o nome; opcional.</summary>
    [Tag("xNome", "E04")]
    public string? Nome { get; set; }

    /// <summary>O endereço; opcional.</summary>
    [Tag("enderDest", "E05")]
    public EnderecoDestinatario? Endereco { get; set; }

    /// <summary>O indicador da inscrição estadual.</summary>
    [Tag("indIEDest", "E16a")]
    public IndicadorIEDestinatario? IndicadorIE { get; set; }

    /// <summary>A inscrição estadual; opcional.</summary>
    [Tag("IE", "E17")]
    public string? InscricaoEstadual { get; set; }

    /// <summary>A inscrição na SUFRAMA; opcional.</summary>
    [Tag("ISUF", "E18")]
    public string? InscricaoSuframa { get; set; }

    /// <summary>A inscrição municipal do tomador do serviço; opcional.</summary>
    [Tag("IM", "E18a")]
    public string? InscricaoMunicipal { get; set; }

    /// <summary>O e-mail; opcional.</summary>
    [Tag("email", "E19")]
    public string? Email { get; set; }
}
