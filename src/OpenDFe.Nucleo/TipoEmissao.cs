namespace OpenDFe;

/// <summary>
/// Tipo (forma) de emissão do documento. O valor de cada membro é o que vai no XML (<c>tpEmis</c>).
/// </summary>
/// <remarks>
/// É independente do <see cref="Ambiente"/>: a contingência também se testa em homologação.
/// </remarks>
public enum TipoEmissao
{
    /// <summary>Emissão normal (1).</summary>
    Normal = 1,

    /// <summary>Contingência em formulário de segurança, FS-IA (2).</summary>
    ContingenciaFSIA = 2,

    /// <summary>Contingência pelo evento prévio de emissão, EPEC (4).</summary>
    ContingenciaEpec = 4,

    /// <summary>Contingência em formulário de segurança para impressão de documento auxiliar, FS-DA (5).</summary>
    ContingenciaFSDA = 5,

    /// <summary>Contingência pela SEFAZ Virtual do Ambiente Nacional, SVC-AN (6).</summary>
    ContingenciaSvcAN = 6,

    /// <summary>Contingência pela SEFAZ Virtual do Rio Grande do Sul, SVC-RS (7).</summary>
    ContingenciaSvcRS = 7,

    /// <summary>Contingência off-line da NFC-e (9).</summary>
    ContingenciaOffline = 9,
}
