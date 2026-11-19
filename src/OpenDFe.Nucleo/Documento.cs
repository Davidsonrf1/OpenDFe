namespace OpenDFe;

/// <summary>
/// Documento fiscal eletrônico. O valor de cada membro é o modelo, o que vai no XML (<c>mod</c>), salvo na NFS-e, que não
/// tem modelo.
/// </summary>
public enum Documento
{
    /// <summary>Nota fiscal eletrônica, NF-e (modelo 55).</summary>
    NFe = 55,

    /// <summary>Nota fiscal de consumidor eletrônica, NFC-e (modelo 65).</summary>
    NFCe = 65,

    /// <summary>
    /// Nota fiscal de serviço eletrônica do padrão nacional, NFS-e. Não é modelo de documento: o 99 não é usado por nenhum
    /// DF-e e nunca é escrito no XML (docs/NFSE.md, D11).
    /// </summary>
    NFSe = 99,
}
