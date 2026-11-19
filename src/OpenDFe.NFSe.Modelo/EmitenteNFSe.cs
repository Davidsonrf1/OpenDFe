namespace OpenDFe.NFSe;

/// <summary>O emitente da NFS-e (<c>emit</c>).</summary>
public sealed class EmitenteNFSe
{
    /// <summary>O CNPJ; um dos dois (CNPJ ou CPF).</summary>
    [Tag("CNPJ")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF.</summary>
    [Tag("CPF")]
    public string? Cpf { get; set; }

    /// <summary>A inscrição municipal; opcional.</summary>
    [Tag("IM")]
    public string? InscricaoMunicipal { get; set; }

    /// <summary>O nome ou a razão social.</summary>
    [Tag("xNome")]
    public string? Nome { get; set; }

    /// <summary>O nome fantasia; opcional.</summary>
    [Tag("xFant")]
    public string? NomeFantasia { get; set; }

    /// <summary>O endereço nacional.</summary>
    [Tag("enderNac")]
    public EnderecoEmitente Endereco { get; set; } = new();

    /// <summary>O telefone; opcional.</summary>
    [Tag("fone")]
    public string? Telefone { get; set; }

    /// <summary>O e-mail; opcional.</summary>
    [Tag("email")]
    public string? Email { get; set; }
}
