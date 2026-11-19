namespace OpenDFe.NFSe;

/// <summary>O tipo do documento fiscal eletrônico do repositório nacional (<c>tipoChaveDFe</c>). O valor de cada membro é o do XML.</summary>
public enum TipoDFe
{
    /// <summary>NFS-e (1).</summary>
    NFSe = 1,

    /// <summary>NF-e (2).</summary>
    NFe = 2,

    /// <summary>CT-e (3).</summary>
    CTe = 3,

    /// <summary>Outro (9).</summary>
    Outro = 9,
}
