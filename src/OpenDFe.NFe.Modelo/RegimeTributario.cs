namespace OpenDFe.NFe;

/// <summary>O código do regime tributário do emitente. O valor de cada membro é o do XML (<c>CRT</c>).</summary>
public enum RegimeTributario
{
    /// <summary>Simples Nacional (1).</summary>
    SimplesNacional = 1,

    /// <summary>Simples Nacional, excesso de sublimite da receita bruta (2).</summary>
    SimplesExcessoSublimite = 2,

    /// <summary>Regime normal (3).</summary>
    Normal = 3,

    /// <summary>Simples Nacional, microempreendedor individual (MEI) (4).</summary>
    Mei = 4,
}
