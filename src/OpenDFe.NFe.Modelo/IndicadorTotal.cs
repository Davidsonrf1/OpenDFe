namespace OpenDFe.NFe;

/// <summary>
/// Se o valor compõe o total da nota. O valor de cada membro é o do XML (<c>indTot, indSomaPISST, indSomaCOFINSST</c>).
/// </summary>
public enum IndicadorTotal
{
    /// <summary>Não compõe o total (0).</summary>
    NaoCompoe = 0,

    /// <summary>Compõe o total (1).</summary>
    Compoe = 1,
}
