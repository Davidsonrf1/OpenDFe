namespace OpenDFe.NFSe;

/// <summary>O mecanismo de apoio ou fomento ao comércio exterior do prestador (<c>mecAFComexP</c>). O XML leva dois dígitos. O valor de cada membro é o do XML.</summary>
public enum MecanismoFomentoPrestador
{
    /// <summary>Desconhecido (não informado na nota de origem) (0).</summary>
    Desconhecido = 0,

    /// <summary>Nenhum (1).</summary>
    Nenhum = 1,

    /// <summary>ACC, adiantamento sobre contrato de câmbio (2).</summary>
    Acc = 2,

    /// <summary>ACE, adiantamento sobre cambiais entregues (3).</summary>
    Ace = 3,

    /// <summary>BNDES-Exim pós-embarque, serviços (4).</summary>
    BndesEximPosEmbarque = 4,

    /// <summary>BNDES-Exim pré-embarque, serviços (5).</summary>
    BndesEximPreEmbarque = 5,

    /// <summary>FGE, Fundo de Garantia à Exportação (6).</summary>
    Fge = 6,

    /// <summary>PROEX, equalização (7).</summary>
    ProexEqualizacao = 7,

    /// <summary>PROEX, financiamento (8).</summary>
    ProexFinanciamento = 8,
}
