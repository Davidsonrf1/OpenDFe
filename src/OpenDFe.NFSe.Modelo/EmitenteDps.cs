namespace OpenDFe.NFSe;

/// <summary>Quem emite a DPS (<c>tpEmit</c>). O valor de cada membro é o do XML.</summary>
public enum EmitenteDps
{
    /// <summary>O prestador (1).</summary>
    Prestador = 1,

    /// <summary>O tomador (2).</summary>
    Tomador = 2,

    /// <summary>O intermediário (3).</summary>
    Intermediario = 3,
}
