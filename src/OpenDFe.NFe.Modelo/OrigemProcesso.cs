namespace OpenDFe.NFe;

/// <summary>A origem de um processo referenciado. O valor de cada membro é o do XML (<c>indProc</c>).</summary>
public enum OrigemProcesso
{
    /// <summary>SEFAZ (0).</summary>
    Sefaz = 0,

    /// <summary>Justiça Federal (1).</summary>
    JusticaFederal = 1,

    /// <summary>Justiça Estadual (2).</summary>
    JusticaEstadual = 2,

    /// <summary>SECEX/RFB (3).</summary>
    SecexRfb = 3,

    /// <summary>CONFAZ (4).</summary>
    Confaz = 4,

    /// <summary>Outros (9).</summary>
    Outros = 9,
}
