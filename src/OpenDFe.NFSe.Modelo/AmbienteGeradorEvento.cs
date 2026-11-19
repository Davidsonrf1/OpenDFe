namespace OpenDFe.NFSe;

/// <summary>O ambiente que gerou o evento (<c>ambGer</c> do <c>infEvento</c>). O valor de cada membro é o do XML.</summary>
public enum AmbienteGeradorEvento
{
    /// <summary>O sistema próprio do município (1).</summary>
    Prefeitura = 1,

    /// <summary>A Sefin Nacional (2).</summary>
    SefinNacional = 2,

    /// <summary>O Ambiente de Dados Nacional (3).</summary>
    AmbienteNacional = 3,
}
