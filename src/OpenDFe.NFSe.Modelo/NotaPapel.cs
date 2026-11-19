namespace OpenDFe.NFSe;

/// <summary>A NF ou a NFS em papel (<c>NFNFS</c>).</summary>
public sealed class NotaPapel
{
    /// <summary>O número da nota (7 dígitos).</summary>
    [Tag("nNFS")]
    public string? Numero { get; set; }

    /// <summary>O modelo da nota.</summary>
    [Tag("modNFS")]
    public string? Modelo { get; set; }

    /// <summary>A série da nota.</summary>
    [Tag("serieNFS")]
    public string? Serie { get; set; }
}
