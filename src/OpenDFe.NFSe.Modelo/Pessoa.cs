namespace OpenDFe.NFSe;

/// <summary>Uma pessoa da DPS (<c>TCInfoPessoa</c>): o tomador, o intermediário e o fornecedor da dedução (docs/NFSE.md 7, M8).</summary>
public sealed class Pessoa
{
    /// <summary>O CNPJ da pessoa, só os dígitos; um dos documentos (CNPJ, CPF, NIF ou o motivo da ausência do NIF).</summary>
    [Tag("CNPJ")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF da pessoa, só os dígitos.</summary>
    [Tag("CPF")]
    public string? Cpf { get; set; }

    /// <summary>O número de identificação fiscal da pessoa no exterior.</summary>
    [Tag("NIF")]
    public string? Nif { get; set; }

    /// <summary>O motivo de não informar o NIF da pessoa.</summary>
    [Tag("cNaoNIF")]
    public MotivoAusenciaNif? MotivoAusenciaNif { get; set; }

    /// <summary>O CAEPF da pessoa física; opcional.</summary>
    [Tag("CAEPF")]
    public string? Caepf { get; set; }

    /// <summary>A inscrição municipal; opcional.</summary>
    [Tag("IM")]
    public string? InscricaoMunicipal { get; set; }

    /// <summary>O nome ou o nome empresarial.</summary>
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
}
