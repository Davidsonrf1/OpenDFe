namespace OpenDFe.NFSe;

/// <summary>O tipo do benefício municipal na DPS (<c>BM/tpBM</c>, só na versão 1.00). O valor de cada membro é o do XML.</summary>
public enum TipoBeneficioLei
{
    /// <summary>Alíquota diferenciada (1).</summary>
    AliquotaDiferenciada = 1,

    /// <summary>Redução da base de cálculo (2).</summary>
    ReducaoBaseCalculo = 2,

    /// <summary>Isenção (3).</summary>
    Isencao = 3,
}
