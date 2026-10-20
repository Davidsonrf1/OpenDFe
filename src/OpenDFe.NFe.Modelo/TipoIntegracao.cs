namespace OpenDFe.NFe;

/// <summary>
/// Se o pagamento é integrado ao sistema de automação da empresa. O valor de cada membro é o do XML (<c>tpIntegra</c>).
/// </summary>
public enum TipoIntegracao
{
    /// <summary>Pagamento integrado (TEF, comércio eletrônico, POS integrado) (1).</summary>
    Integrado = 1,

    /// <summary>Pagamento não integrado (POS simples) (2).</summary>
    NaoIntegrado = 2,
}
