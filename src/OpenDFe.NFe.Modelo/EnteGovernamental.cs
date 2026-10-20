namespace OpenDFe.NFe;

/// <summary>
/// O tipo do ente governamental da compra governamental (NT 2025.002). O valor de cada membro é o do XML
/// (<c>tpEnteGov</c>).
/// </summary>
public enum EnteGovernamental
{
    /// <summary>União (1).</summary>
    Uniao = 1,

    /// <summary>Estado (2).</summary>
    Estado = 2,

    /// <summary>Distrito Federal (3).</summary>
    DistritoFederal = 3,

    /// <summary>Município (4).</summary>
    Municipio = 4,

    /// <summary>Consórcio público (5).</summary>
    ConsorcioPublico = 5,

    /// <summary>Comitê Gestor do IBS (6).</summary>
    ComiteGestorIbs = 6,
}
