namespace OpenDFe;

/// <summary>
/// Para onde vai uma operação: uma UF (o autorizador sai do <see cref="Atendimento"/> que o leiaute declara) ou
/// um autorizador (o Ambiente Nacional e os autorizadores virtuais).
/// </summary>
/// <remarks>
/// As UFs têm o código IBGE, o mesmo valor de <see cref="UF"/>; o <see cref="AN"/>, o 91 do <c>cOrgao</c>. Os
/// virtuais (<see cref="SVRS"/>, <see cref="SVAN"/>, <see cref="SvcAN"/>, <see cref="SvcRS"/>) não têm código
/// oficial: o valor deles nunca é escrito no XML. Conversões e siglas: <see cref="Destinos"/>.
/// </remarks>
public enum Destino
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

    /// <summary>Ambiente Nacional (91, o <c>cOrgao</c>): distribuição DF-e, manifestação, EPEC.</summary>
    AN = 91,

    /// <summary>SEFAZ Virtual do Rio Grande do Sul, SVRS (sem código oficial).</summary>
    SVRS = 101,

    /// <summary>SEFAZ Virtual do Ambiente Nacional, SVAN (sem código oficial).</summary>
    SVAN = 102,

    /// <summary>SEFAZ Virtual de Contingência do Ambiente Nacional, SVC-AN (sem código oficial).</summary>
    SvcAN = 103,

    /// <summary>SEFAZ Virtual de Contingência do Rio Grande do Sul, SVC-RS (sem código oficial).</summary>
    SvcRS = 104,
}
