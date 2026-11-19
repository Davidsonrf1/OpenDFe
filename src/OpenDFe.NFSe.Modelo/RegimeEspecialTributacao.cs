namespace OpenDFe.NFSe;

/// <summary>O regime especial de tributação (<c>regEspTrib</c>). O valor de cada membro é o do XML.</summary>
public enum RegimeEspecialTributacao
{
    /// <summary>Nenhum (0).</summary>
    Nenhum = 0,

    /// <summary>Ato cooperado (cooperativa) (1).</summary>
    AtoCooperado = 1,

    /// <summary>Estimativa (2).</summary>
    Estimativa = 2,

    /// <summary>Microempresa municipal (3).</summary>
    MicroempresaMunicipal = 3,

    /// <summary>Notário ou registrador (4).</summary>
    NotarioRegistrador = 4,

    /// <summary>Profissional autônomo (5).</summary>
    ProfissionalAutonomo = 5,

    /// <summary>Sociedade de profissionais (6).</summary>
    SociedadeProfissionais = 6,

    /// <summary>Outros (9).</summary>
    Outros = 9,
}
