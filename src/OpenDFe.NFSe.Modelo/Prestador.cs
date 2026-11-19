namespace OpenDFe.NFSe;

/// <summary>O prestador do serviço (<c>prest</c>, <c>TCInfoPrestador</c>). Com a DPS emitida pelo prestador, o nome, o endereço, o NIF e o motivo de ausência do NIF ficam de fora: o ambiente nacional os tira do cadastro (regra NFSE-PRESTADOR).</summary>
public sealed class Prestador
{
    /// <summary>O CNPJ do prestador, só os dígitos; um dos documentos (CNPJ, CPF, NIF ou o motivo da ausência do NIF).</summary>
    [Tag("CNPJ")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF do prestador, só os dígitos.</summary>
    [Tag("CPF")]
    public string? Cpf { get; set; }

    /// <summary>O número de identificação fiscal do prestador no exterior.</summary>
    [Tag("NIF")]
    public string? Nif { get; set; }

    /// <summary>O motivo de não informar o NIF do prestador.</summary>
    [Tag("cNaoNIF")]
    public MotivoAusenciaNif? MotivoAusenciaNif { get; set; }

    /// <summary>O CAEPF do prestador pessoa física; opcional.</summary>
    [Tag("CAEPF")]
    public string? Caepf { get; set; }

    /// <summary>A inscrição municipal; opcional.</summary>
    [Tag("IM")]
    public string? InscricaoMunicipal { get; set; }

    /// <summary>O nome ou o nome empresarial; opcional.</summary>
    [Tag("xNome")]
    public string? Nome { get; set; }

    /// <summary>O endereço; opcional.</summary>
    [Tag("end")]
    public Endereco? Endereco { get; set; }

    /// <summary>O telefone, com o DDD; opcional.</summary>
    [Tag("fone")]
    public string? Telefone { get; set; }

    /// <summary>O e-mail; opcional.</summary>
    [Tag("email")]
    public string? Email { get; set; }

    /// <summary>Os regimes de tributação do prestador.</summary>
    [Tag("regTrib")]
    public RegimeTributacao RegimeTributacao { get; set; } = new();
}
