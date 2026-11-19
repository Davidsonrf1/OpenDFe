namespace OpenDFe.NFSe;

/// <summary>O fornecedor do documento de reembolso (<c>fornec</c>, <c>TCRTCListaDocFornec</c>).</summary>
public sealed class FornecedorDocumento
{
    /// <summary>O CNPJ do fornecedor, só os dígitos; um dos documentos (CNPJ, CPF, NIF ou o motivo da ausência do NIF).</summary>
    [Tag("CNPJ")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF do fornecedor, só os dígitos.</summary>
    [Tag("CPF")]
    public string? Cpf { get; set; }

    /// <summary>O número de identificação fiscal do fornecedor no exterior.</summary>
    [Tag("NIF")]
    public string? Nif { get; set; }

    /// <summary>O motivo de não informar o NIF do fornecedor.</summary>
    [Tag("cNaoNIF")]
    public MotivoAusenciaNif? MotivoAusenciaNif { get; set; }

    /// <summary>O nome ou a razão social.</summary>
    [Tag("xNome")]
    public string? Nome { get; set; }
}
