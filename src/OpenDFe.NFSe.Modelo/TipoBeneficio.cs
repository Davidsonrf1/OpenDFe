namespace OpenDFe.NFSe;

/// <summary>O tipo do benefício municipal calculado (<c>tpBM</c> dos valores da NFS-e). O valor de cada membro é o do XML.</summary>
public enum TipoBeneficio
{
    /// <summary>Isenção (1).</summary>
    Isencao = 1,

    /// <summary>Redução da base de cálculo em percentual (2).</summary>
    ReducaoPercentual = 2,

    /// <summary>Redução da base de cálculo em valor (3).</summary>
    ReducaoValor = 3,

    /// <summary>Alíquota diferenciada (4).</summary>
    AliquotaDiferenciada = 4,
}
