namespace OpenDFe.NFSe;

/// <summary>A situação da NFS-e (<c>cStat</c>). O valor de cada membro é o do XML.</summary>
public enum SituacaoNFSe
{
    /// <summary>NFS-e gerada (100).</summary>
    Gerada = 100,

    /// <summary>NFS-e de decisão judicial (102).</summary>
    DecisaoJudicial = 102,

    /// <summary>NFS-e avulsa (103).</summary>
    Avulsa = 103,

    /// <summary>NFS-e do MEI (107).</summary>
    Mei = 107,
}
