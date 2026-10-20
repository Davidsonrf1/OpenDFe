namespace OpenDFe.NFe;

/// <summary>
/// O tipo do ato concessório de um processo referenciado (NT 2023.004). O valor de cada membro é o do XML
/// (<c>tpAto</c>).
/// </summary>
public enum TipoAtoConcessorio
{
    /// <summary>Termo de acordo (8).</summary>
    TermoAcordo = 8,

    /// <summary>Regime especial (10).</summary>
    RegimeEspecial = 10,

    /// <summary>Autorização específica (12).</summary>
    AutorizacaoEspecifica = 12,

    /// <summary>Ajuste SINIEF (14).</summary>
    AjusteSinief = 14,

    /// <summary>Convênio ICMS (15).</summary>
    ConvenioIcms = 15,
}
