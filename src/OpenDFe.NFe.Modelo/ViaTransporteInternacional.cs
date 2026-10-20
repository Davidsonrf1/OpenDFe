namespace OpenDFe.NFe;

/// <summary>
/// A via de transporte internacional da declaração de importação. O valor de cada membro é o do XML
/// (<c>tpViaTransp</c>).
/// </summary>
public enum ViaTransporteInternacional
{
    /// <summary>Marítima (1).</summary>
    Maritima = 1,

    /// <summary>Fluvial (2).</summary>
    Fluvial = 2,

    /// <summary>Lacustre (3).</summary>
    Lacustre = 3,

    /// <summary>Aérea (4).</summary>
    Aerea = 4,

    /// <summary>Postal (5).</summary>
    Postal = 5,

    /// <summary>Ferroviária (6).</summary>
    Ferroviaria = 6,

    /// <summary>Rodoviária (7).</summary>
    Rodoviaria = 7,

    /// <summary>Conduto ou rede de transmissão (8).</summary>
    Conduto = 8,

    /// <summary>Meios próprios (9).</summary>
    MeiosProprios = 9,

    /// <summary>Entrada ou saída ficta (10).</summary>
    Ficta = 10,

    /// <summary>Courier (11).</summary>
    Courier = 11,

    /// <summary>Em mãos (12).</summary>
    EntregaMaos = 12,

    /// <summary>Por reboque (13).</summary>
    Reboque = 13,
}
