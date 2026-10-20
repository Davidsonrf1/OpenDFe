namespace OpenDFe.NFe;

/// <summary>A assinatura do provedor de assinatura e autorização (<c>PAASignature</c>).</summary>
public sealed class AssinaturaProvedor
{
    /// <summary>A assinatura, em base64.</summary>
    [Tag("SignatureValue", "ZG04")]
    public string? Valor { get; set; }

    /// <summary>A chave pública RSA do provedor.</summary>
    [Tag("RSAKeyValue", "ZG05")]
    public ChavePublicaRsa ChavePublica { get; set; } = new();
}
