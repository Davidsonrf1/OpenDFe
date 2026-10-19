namespace OpenDFe;

/// <summary>
/// Unidade federativa do Brasil. O valor de cada membro é o código IBGE da UF, o mesmo que vai no XML
/// (<c>cUF</c>); o nome é a sigla.
/// </summary>
/// <remarks>
/// Os ambientes nacionais (AN, SVRS, SVAN, SVC) não são UFs: são destinos de transmissão.
/// Para converter de e para a sigla e o código, use <see cref="UnidadesFederativas"/>.
/// </remarks>
public enum UF
{
    /// <summary>Rondônia (11).</summary>
    RO = 11,

    /// <summary>Acre (12).</summary>
    AC = 12,

    /// <summary>Amazonas (13).</summary>
    AM = 13,

    /// <summary>Roraima (14).</summary>
    RR = 14,

    /// <summary>Pará (15).</summary>
    PA = 15,

    /// <summary>Amapá (16).</summary>
    AP = 16,

    /// <summary>Tocantins (17).</summary>
    TO = 17,

    /// <summary>Maranhão (21).</summary>
    MA = 21,

    /// <summary>Piauí (22).</summary>
    PI = 22,

    /// <summary>Ceará (23).</summary>
    CE = 23,

    /// <summary>Rio Grande do Norte (24).</summary>
    RN = 24,

    /// <summary>Paraíba (25).</summary>
    PB = 25,

    /// <summary>Pernambuco (26).</summary>
    PE = 26,

    /// <summary>Alagoas (27).</summary>
    AL = 27,

    /// <summary>Sergipe (28).</summary>
    SE = 28,

    /// <summary>Bahia (29).</summary>
    BA = 29,

    /// <summary>Minas Gerais (31).</summary>
    MG = 31,

    /// <summary>Espírito Santo (32).</summary>
    ES = 32,

    /// <summary>Rio de Janeiro (33).</summary>
    RJ = 33,

    /// <summary>São Paulo (35).</summary>
    SP = 35,

    /// <summary>Paraná (41).</summary>
    PR = 41,

    /// <summary>Santa Catarina (42).</summary>
    SC = 42,

    /// <summary>Rio Grande do Sul (43).</summary>
    RS = 43,

    /// <summary>Mato Grosso do Sul (50).</summary>
    MS = 50,

    /// <summary>Mato Grosso (51).</summary>
    MT = 51,

    /// <summary>Goiás (52).</summary>
    GO = 52,

    /// <summary>Distrito Federal (53).</summary>
    DF = 53,
}
