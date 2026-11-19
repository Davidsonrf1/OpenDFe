namespace OpenDFe.NFSe;

/// <summary>A NFS-e do padrão municipal anterior (<c>NFSeMun</c>).</summary>
public sealed class NFSeMunicipal
{
    /// <summary>O código IBGE do município emissor.</summary>
    [Tag("cMunNFSeMun")]
    public int? Municipio { get; set; }

    /// <summary>O número da nota (15 dígitos).</summary>
    [Tag("nNFSeMun")]
    public string? Numero { get; set; }

    /// <summary>O código de verificação.</summary>
    [Tag("cVerifNFSeMun")]
    public string? CodigoVerificacao { get; set; }
}
