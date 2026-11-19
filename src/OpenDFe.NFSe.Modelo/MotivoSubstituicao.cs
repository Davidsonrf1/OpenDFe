namespace OpenDFe.NFSe;

/// <summary>O motivo da substituição da NFS-e (<c>cMotivo</c> do <c>subst</c> e do evento 105102). O XML leva dois dígitos. O valor de cada membro é o do XML.</summary>
public enum MotivoSubstituicao
{
    /// <summary>Desenquadramento do Simples Nacional (1).</summary>
    DesenquadramentoSimples = 1,

    /// <summary>Enquadramento no Simples Nacional (2).</summary>
    EnquadramentoSimples = 2,

    /// <summary>Inclusão retroativa de imunidade ou isenção (3).</summary>
    InclusaoImunidadeIsencao = 3,

    /// <summary>Exclusão retroativa de imunidade ou isenção (4).</summary>
    ExclusaoImunidadeIsencao = 4,

    /// <summary>Rejeição pelo tomador ou pelo intermediário responsável pelo recolhimento (5).</summary>
    RejeicaoTomador = 5,

    /// <summary>Outros (99).</summary>
    Outros = 99,
}
