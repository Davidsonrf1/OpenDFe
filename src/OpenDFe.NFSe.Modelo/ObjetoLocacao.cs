namespace OpenDFe.NFSe;

/// <summary>O objeto da locação (<c>objeto</c>, só na versão 1.00). O valor de cada membro é o do XML.</summary>
public enum ObjetoLocacao
{
    /// <summary>Ferrovia (1).</summary>
    Ferrovia = 1,

    /// <summary>Rodovia (2).</summary>
    Rodovia = 2,

    /// <summary>Postes (3).</summary>
    Postes = 3,

    /// <summary>Cabos (4).</summary>
    Cabos = 4,

    /// <summary>Dutos (5).</summary>
    Dutos = 5,

    /// <summary>Condutos de qualquer natureza (6).</summary>
    Condutos = 6,
}
