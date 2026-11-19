namespace OpenDFe.NFSe;

/// <summary>O motivo da emissão da DPS pelo tomador ou pelo intermediário (<c>cMotivoEmisTI</c>). O valor de cada membro é o do XML.</summary>
public enum MotivoEmissaoTerceiro
{
    /// <summary>Importação de serviço (1).</summary>
    ImportacaoServico = 1,

    /// <summary>Tomador ou intermediário obrigado a emitir pela legislação municipal (2).</summary>
    ObrigacaoLegal = 2,

    /// <summary>Emissão por recusa do prestador (3).</summary>
    RecusaPrestador = 3,

    /// <summary>Emissão por rejeitar a NFS-e emitida pelo prestador (4).</summary>
    RejeicaoNFSe = 4,
}
