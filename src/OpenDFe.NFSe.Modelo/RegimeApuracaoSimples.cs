namespace OpenDFe.NFSe;

/// <summary>O regime de apuração do optante ME/EPP que ultrapassou um sublimite (<c>regApTribSN</c>). O valor de cada membro é o do XML.</summary>
public enum RegimeApuracaoSimples
{
    /// <summary>Tributos federais e municipal pelo Simples Nacional (1).</summary>
    Simples = 1,

    /// <summary>Federais pelo Simples Nacional e o ISSQN por fora (2).</summary>
    IssqnFora = 2,

    /// <summary>Federais e municipal por fora do Simples Nacional (3).</summary>
    TodosFora = 3,
}
