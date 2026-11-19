namespace OpenDFe.NFSe;

/// <summary>A finalidade da emissão da NFS-e (<c>finNFSe</c>). O XSD em produção só aceita o 0; a NT 009 acrescenta as notas de ajuste (docs/NFSE.md 7, M6). O valor de cada membro é o do XML.</summary>
public enum FinalidadeNFSe
{
    /// <summary>NFS-e regular (0).</summary>
    Regular = 0,
}
