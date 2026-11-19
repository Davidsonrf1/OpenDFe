namespace OpenDFe.NFSe;

/// <summary>A retenção do ISSQN (<c>tpRetISSQN</c>). O valor de cada membro é o do XML.</summary>
public enum RetencaoIssqn
{
    /// <summary>Não retido (1).</summary>
    NaoRetido = 1,

    /// <summary>Retido pelo tomador (2).</summary>
    Tomador = 2,

    /// <summary>Retido pelo intermediário (3).</summary>
    Intermediario = 3,
}
