namespace OpenDFe.NFSe;

/// <summary>O tipo de emissão da NFS-e (<c>tpEmis</c>). O valor de cada membro é o do XML.</summary>
public enum TipoEmissaoNFSe
{
    /// <summary>Emissão normal no modelo nacional (1).</summary>
    Normal = 1,

    /// <summary>Emissão no leiaute próprio do município, transcrita para o nacional (2).</summary>
    Transcricao = 2,
}
