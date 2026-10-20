namespace OpenDFe.NFe;

/// <summary>A exigibilidade do ISS. O valor de cada membro é o do XML (<c>indISS</c>).</summary>
public enum ExigibilidadeIss
{
    /// <summary>Exigível (1).</summary>
    Exigivel = 1,

    /// <summary>Não incidência (2).</summary>
    NaoIncidencia = 2,

    /// <summary>Isenção (3).</summary>
    Isencao = 3,

    /// <summary>Exportação (4).</summary>
    Exportacao = 4,

    /// <summary>Imunidade (5).</summary>
    Imunidade = 5,

    /// <summary>Exigibilidade suspensa por decisão judicial (6).</summary>
    SuspensaJudicial = 6,

    /// <summary>Exigibilidade suspensa por processo administrativo (7).</summary>
    SuspensaAdministrativa = 7,
}
