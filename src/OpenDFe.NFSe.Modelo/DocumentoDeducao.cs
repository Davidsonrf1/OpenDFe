namespace OpenDFe.NFSe;

/// <summary>Um documento da dedução ou da redução (<c>docDedRed</c>).</summary>
public sealed class DocumentoDeducao
{
    /// <summary>A chave da NFS-e nacional; um dos seis documentos.</summary>
    [Tag("chNFSe")]
    public string? ChaveNFSe { get; set; }

    /// <summary>A chave da NF-e.</summary>
    [Tag("chNFe")]
    public string? ChaveNFe { get; set; }

    /// <summary>A NFS-e do padrão municipal anterior.</summary>
    [Tag("NFSeMun")]
    public NFSeMunicipal? NFSeMunicipal { get; set; }

    /// <summary>A NF ou a NFS em papel.</summary>
    [Tag("NFNFS")]
    public NotaPapel? NotaPapel { get; set; }

    /// <summary>O número de outro documento fiscal.</summary>
    [Tag("nDocFisc")]
    public string? NumeroDocumentoFiscal { get; set; }

    /// <summary>O número de um documento não fiscal.</summary>
    [Tag("nDoc")]
    public string? NumeroDocumento { get; set; }

    /// <summary>O tipo da dedução ou da redução.</summary>
    [Tag("tpDedRed")]
    public TipoDeducao? Tipo { get; set; }

    /// <summary>A descrição, quando o tipo é "outras deduções"; opcional.</summary>
    [Tag("xDescOutDed")]
    public string? DescricaoOutra { get; set; }

    /// <summary>A data da emissão do documento.</summary>
    [Tag("dtEmiDoc")]
    public DateOnly? Emissao { get; set; }

    /// <summary>O valor total dedutível ou redutível do documento.</summary>
    [Tag("vDedutivelRedutivel")]
    public decimal? ValorDedutivel { get; set; }

    /// <summary>O valor usado nesta NFS-e.</summary>
    [Tag("vDeducaoReducao")]
    public decimal? ValorUtilizado { get; set; }

    /// <summary>O fornecedor; opcional.</summary>
    [Tag("fornec")]
    public Pessoa? Fornecedor { get; set; }
}
