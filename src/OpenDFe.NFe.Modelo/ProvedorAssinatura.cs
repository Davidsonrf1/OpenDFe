namespace OpenDFe.NFe;

/// <summary>
/// O provedor de assinatura e autorização, PAA (<c>infPAA</c>): a nota gerada por ele
/// (<see cref="ProcessoEmissao.ProvedorAssinatura"/>).
/// </summary>
public sealed class ProvedorAssinatura
{
    /// <summary>O CNPJ do provedor.</summary>
    [Tag("CNPJPAA", "ZG02")]
    public string? Cnpj { get; set; }

    /// <summary>A assinatura do provedor.</summary>
    [Tag("PAASignature", "ZG03")]
    public AssinaturaProvedor Assinatura { get; set; } = new();
}
