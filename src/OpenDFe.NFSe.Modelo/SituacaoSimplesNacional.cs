namespace OpenDFe.NFSe;

/// <summary>A situação perante o Simples Nacional (<c>opSimpNac</c>). O valor de cada membro é o do XML.</summary>
public enum SituacaoSimplesNacional
{
    /// <summary>Não optante (1).</summary>
    NaoOptante = 1,

    /// <summary>Optante, microempreendedor individual (MEI) (2).</summary>
    Mei = 2,

    /// <summary>Optante, microempresa ou empresa de pequeno porte (ME/EPP) (3).</summary>
    MeEpp = 3,
}
