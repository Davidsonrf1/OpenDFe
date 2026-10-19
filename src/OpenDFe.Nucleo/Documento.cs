namespace OpenDFe;

/// <summary>
/// Documento fiscal eletrônico. O valor de cada membro é o modelo, o que vai no XML (<c>mod</c>).
/// </summary>
public enum Documento
{
    /// <summary>Nota fiscal eletrônica, NF-e (modelo 55).</summary>
    NFe = 55,

    /// <summary>Nota fiscal de consumidor eletrônica, NFC-e (modelo 65).</summary>
    NFCe = 65,
}
