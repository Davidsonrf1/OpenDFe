namespace OpenDFe.NFe;

/// <summary>
/// O indicador da inscrição estadual do destinatário. O valor de cada membro é o do XML (<c>indIEDest</c>).
/// </summary>
public enum IndicadorIEDestinatario
{
    /// <summary>Contribuinte do ICMS, com a inscrição estadual (1).</summary>
    Contribuinte = 1,

    /// <summary>Contribuinte isento de inscrição estadual (2).</summary>
    ContribuinteIsento = 2,

    /// <summary>Não contribuinte, que pode ou não ter inscrição estadual (9).</summary>
    NaoContribuinte = 9,
}
