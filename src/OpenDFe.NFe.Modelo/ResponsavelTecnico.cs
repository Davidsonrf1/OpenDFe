namespace OpenDFe.NFe;

/// <summary>O responsável técnico pelo sistema emissor (<c>infRespTec</c>).</summary>
/// <remarks>
/// O modelo guarda o que está no XML. A conta do hash, que precisa da chave já montada e do CSRT secreto, fica com a
/// configuração do documento, fora do modelo.
/// </remarks>
public sealed class ResponsavelTecnico
{
    /// <summary>O CNPJ.</summary>
    [Tag("CNPJ", "ZD02")]
    public string? Cnpj { get; set; }

    /// <summary>O nome da pessoa de contato.</summary>
    [Tag("xContato", "ZD04")]
    public string? Contato { get; set; }

    /// <summary>O e-mail.</summary>
    [Tag("email", "ZD05")]
    public string? Email { get; set; }

    /// <summary>O telefone.</summary>
    [Tag("fone", "ZD06")]
    public string? Telefone { get; set; }

    /// <summary>O identificador do CSRT; com o <see cref="HashCsrt"/>.</summary>
    [Tag("idCSRT", "ZD08")]
    public int? IdCsrt { get; set; }

    /// <summary>O hash do CSRT, em base64, como no XML; com o <see cref="IdCsrt"/>.</summary>
    [Tag("hashCSRT", "ZD09")]
    public string? HashCsrt { get; set; }
}
