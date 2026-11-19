namespace OpenDFe.NFSe;

/// <summary>O destinatário do serviço (<c>dest</c>, <c>TCRTCInfoDest</c>).</summary>
public sealed class Destinatario
{
    /// <summary>O CNPJ do destinatário, só os dígitos; um dos documentos (CNPJ, CPF, NIF ou o motivo da ausência do NIF).</summary>
    [Tag("CNPJ")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF do destinatário, só os dígitos.</summary>
    [Tag("CPF")]
    public string? Cpf { get; set; }

    /// <summary>O número de identificação fiscal do destinatário no exterior.</summary>
    [Tag("NIF")]
    public string? Nif { get; set; }

    /// <summary>O motivo de não informar o NIF do destinatário.</summary>
    [Tag("cNaoNIF")]
    public MotivoAusenciaNif? MotivoAusenciaNif { get; set; }

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
