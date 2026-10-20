namespace OpenDFe.NFe;

/// <summary>O regime especial de tributação do ISSQN. O valor de cada membro é o do XML (<c>cRegTrib</c>).</summary>
public enum RegimeEspecialTributacao
{
    /// <summary>Microempresa municipal (1).</summary>
    MicroempresaMunicipal = 1,

    /// <summary>Estimativa (2).</summary>
    Estimativa = 2,

    /// <summary>Sociedade de profissionais (3).</summary>
    SociedadeProfissionais = 3,

    /// <summary>Cooperativa (4).</summary>
    Cooperativa = 4,

    /// <summary>Microempreendedor individual (MEI) (5).</summary>
    Mei = 5,

    /// <summary>Microempresa ou empresa de pequeno porte (ME ou EPP) (6).</summary>
    MicroempresaPequenoPorte = 6,
}
