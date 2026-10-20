namespace OpenDFe.NFe;

/// <summary>
/// O motivo da desoneração do ICMS ou do ICMS ST (<c>motDesICMS</c>, <c>motDesICMSST</c>). O valor de cada membro é o
/// do XML.
/// </summary>
/// <remarks>
/// Uma enumeração para todos os grupos, com a união dos valores: cada grupo aceita só uma parte, e o schema aponta o
/// valor que o grupo não aceita.
/// </remarks>
public enum MotivoDesoneracao
{
    /// <summary>Táxi (1).</summary>
    Taxi = 1,

    /// <summary>Uso na agropecuária (no <c>ICMS40</c>, produtor agropecuário).</summary>
    Agropecuaria = 3,

    /// <summary>Frotista ou locadora (4).</summary>
    FrotistaLocadora = 4,

    /// <summary>Diplomático ou consular (5).</summary>
    DiplomaticoConsular = 5,

    /// <summary>Utilitários e motocicletas da Amazônia Ocidental e das áreas de livre comércio (6).</summary>
    UtilitariosMotocicletasAmazonia = 6,

    /// <summary>SUFRAMA (7).</summary>
    Suframa = 7,

    /// <summary>Venda a órgão público (8).</summary>
    OrgaoPublico = 8,

    /// <summary>Outros (9).</summary>
    Outros = 9,

    /// <summary>Deficiente condutor (10).</summary>
    DeficienteCondutor = 10,

    /// <summary>Deficiente não condutor (11).</summary>
    DeficienteNaoCondutor = 11,

    /// <summary>Órgão de fomento e desenvolvimento agropecuário (12).</summary>
    FomentoAgropecuario = 12,

    /// <summary>Olimpíadas Rio 2016 (16).</summary>
    OlimpiadasRio2016 = 16,

    /// <summary>Solicitado pelo fisco (90).</summary>
    SolicitadoFisco = 90,
}
