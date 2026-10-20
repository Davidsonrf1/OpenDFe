namespace OpenDFe.NFe;

/// <summary>A finalidade da emissão. O valor de cada membro é o do XML (<c>finNFe</c>).</summary>
public enum FinalidadeEmissao
{
    /// <summary>Nota normal (1).</summary>
    Normal = 1,

    /// <summary>Nota complementar (2).</summary>
    Complementar = 2,

    /// <summary>Nota de ajuste (3).</summary>
    Ajuste = 3,

    /// <summary>Devolução de mercadoria (4).</summary>
    Devolucao = 4,

    /// <summary>Nota de crédito (5).</summary>
    NotaCredito = 5,

    /// <summary>Nota de débito (6).</summary>
    NotaDebito = 6,
}
