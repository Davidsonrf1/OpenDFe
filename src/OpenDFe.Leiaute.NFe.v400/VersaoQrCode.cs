namespace OpenDFe.NFe;

/// <summary>
/// A versão do QR Code que a lib escreve na NFC-e (docs/CONTRATOS.md 16). A versão 100, a primeira, só se lê: a SEFAZ
/// a recusa (903).
/// </summary>
public enum VersaoQrCode
{
    /// <summary>A versão 2, com o CSC da UF: <c>chave|2|tpAmb|idCSC|hash</c>.</summary>
    V2 = 2,

    /// <summary>A versão 3, sem CSC: <c>chave|3|tpAmb</c>.</summary>
    V3 = 3,
}
