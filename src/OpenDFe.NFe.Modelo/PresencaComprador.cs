namespace OpenDFe.NFe;

/// <summary>
/// A presença do comprador no estabelecimento no momento da operação. O valor de cada membro é o do XML
/// (<c>indPres</c>).
/// </summary>
public enum PresencaComprador
{
    /// <summary>Não se aplica (nota complementar ou de ajuste, por exemplo) (0).</summary>
    NaoAplicavel = 0,

    /// <summary>Operação presencial (1).</summary>
    Presencial = 1,

    /// <summary>Operação não presencial, pela internet (2).</summary>
    NaoPresencialInternet = 2,

    /// <summary>Operação não presencial, teleatendimento (3).</summary>
    NaoPresencialTeleatendimento = 3,

    /// <summary>NFC-e em operação com entrega a domicílio (4).</summary>
    NaoPresencialEntrega = 4,

    /// <summary>Operação presencial, fora do estabelecimento (5).</summary>
    PresencialExterno = 5,

    /// <summary>Operação não presencial, outros (9).</summary>
    NaoPresencialOutros = 9,
}
