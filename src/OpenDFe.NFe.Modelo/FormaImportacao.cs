namespace OpenDFe.NFe;

/// <summary>
/// A forma de importação quanto à intermediação. O valor de cada membro é o do XML (<c>tpIntermedio</c>).
/// </summary>
public enum FormaImportacao
{
    /// <summary>Por conta própria (1).</summary>
    ContaPropria = 1,

    /// <summary>Por conta e ordem (2).</summary>
    ContaOrdem = 2,

    /// <summary>Por encomenda (3).</summary>
    Encomenda = 3,
}
