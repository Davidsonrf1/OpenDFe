namespace OpenDFe.NFe;

/// <summary>A chave pública RSA do provedor de assinatura (<c>RSAKeyValue</c>).</summary>
public sealed class ChavePublicaRsa
{
    /// <summary>O módulo, em base64.</summary>
    [Tag("Modulus", "ZG06")]
    public string? Modulo { get; set; }

    /// <summary>O expoente, em base64 (em geral <c>AQAB</c>).</summary>
    [Tag("Exponent", "ZG07")]
    public string? Expoente { get; set; }
}
