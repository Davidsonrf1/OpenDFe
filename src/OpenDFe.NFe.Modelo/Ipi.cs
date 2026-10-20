namespace OpenDFe.NFe;

/// <summary>O IPI do item (<c>IPI</c>).</summary>
public sealed class Ipi
{
    /// <summary>O CNPJ do produtor, quando diferente do emitente (exportação direta ou indireta); opcional.</summary>
    [Tag("CNPJProd", "O03")]
    public string? CnpjProdutor { get; set; }

    /// <summary>O código do selo de controle do IPI; opcional.</summary>
    [Tag("cSelo", "O04")]
    public string? CodigoSelo { get; set; }

    /// <summary>A quantidade de selos de controle; opcional.</summary>
    [Tag("qSelo", "O05")]
    public long? QuantidadeSelo { get; set; }

    /// <summary>O código de enquadramento legal do IPI.</summary>
    [Tag("cEnq", "O06")]
    public string? CodigoEnquadramento { get; set; }

    /// <summary>O IPI tributado; ou o <see cref="NaoTributado"/> (um dos dois).</summary>
    [Tag("IPITrib", "O07")]
    public IpiTributado? Tributado { get; set; }

    /// <summary>O IPI não tributado; ou o <see cref="Tributado"/> (um dos dois).</summary>
    [Tag("IPINT", "O08")]
    public IpiNaoTributado? NaoTributado { get; set; }
}
