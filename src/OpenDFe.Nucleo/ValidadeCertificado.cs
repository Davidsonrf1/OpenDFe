namespace OpenDFe;

/// <summary>A situação de um certificado num instante, pelo período de validade (<c>notBefore</c> e <c>notAfter</c>).</summary>
public enum ValidadeCertificado
{
    /// <summary>O instante está dentro do período de validade, com os dois extremos incluídos.</summary>
    Vigente = 1,

    /// <summary>O instante é anterior ao início da validade (<c>notBefore</c>).</summary>
    AindaNaoVigente = 2,

    /// <summary>O instante é posterior ao fim da validade (<c>notAfter</c>).</summary>
    Vencido = 3,
}
