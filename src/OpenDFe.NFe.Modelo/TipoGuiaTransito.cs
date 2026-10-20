namespace OpenDFe.NFe;

/// <summary>
/// O tipo da guia de trânsito de produtos agropecuários (NT 2024.003). O valor de cada membro é o do XML
/// (<c>tpGuia</c>).
/// </summary>
public enum TipoGuiaTransito
{
    /// <summary>GTA, guia de trânsito animal (1).</summary>
    Gta = 1,

    /// <summary>TTA, termo de trânsito animal (2).</summary>
    Tta = 2,

    /// <summary>DTA, documento de transferência animal (3).</summary>
    Dta = 3,

    /// <summary>ATV, autorização de trânsito vegetal (4).</summary>
    Atv = 4,

    /// <summary>PTV, permissão de trânsito vegetal (5).</summary>
    Ptv = 5,

    /// <summary>GTV, guia de trânsito vegetal (6).</summary>
    Gtv = 6,

    /// <summary>Guia florestal (DOF, SisFlora PA e MT, SIAM MG) (7).</summary>
    GuiaFlorestal = 7,
}
