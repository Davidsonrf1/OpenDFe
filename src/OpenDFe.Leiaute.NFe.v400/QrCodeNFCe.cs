using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace OpenDFe.NFe;

/// <summary>
/// O texto do QR Code da NFC-e emitida online (Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0, 4.3 e 4.4): a
/// URL de consulta da UF, o <c>?p=</c> e os parâmetros separados por <c>|</c>. As formas offline entram na etapa 5.2.
/// Sem estado.
/// </summary>
internal static class QrCodeNFCe
{
    /// <summary>
    /// Versão 3, online: <c>chave|3|tpAmb</c>. Sem CSC, sem hash.
    /// </summary>
    internal static string Versao3(string urlConsulta, ChaveAcesso chave, Ambiente ambiente) =>
        Texto(urlConsulta, string.Create(CultureInfo.InvariantCulture, $"{chave}|3|{(int)ambiente}"));

    /// <summary>
    /// Versão 2, online: <c>chave|2|tpAmb|idCSC|hash</c>, com o <c>idCSC</c> sem zeros à esquerda e o hash em
    /// maiúsculas (Manual, 4.3.4 e 4.3.5).
    /// </summary>
    internal static string Versao2(string urlConsulta, ChaveAcesso chave, Ambiente ambiente, CodigoSeguranca csc)
    {
        var parametros = string.Create(CultureInfo.InvariantCulture, $"{chave}|2|{(int)ambiente}|{csc.Identificador}");
        return Texto(urlConsulta, parametros + "|" + Hash(parametros, csc.Codigo));
    }

    /// <summary>
    /// O hash da versão 2: o SHA-1 dos parâmetros (separados por <c>|</c>, sem o hash) com o CSC colado no fim, em
    /// hexadecimal maiúsculo (40 caracteres).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms",
        Justification = "O SHA-1 é o hash que o Manual do DANFE NFC-e e QR Code manda (4.3.4); não protege nada, a SEFAZ o recalcula.")]
    internal static string Hash(string parametros, string csc) =>
        Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(parametros + csc)));

    private static string Texto(string urlConsulta, string parametros) => urlConsulta + "?p=" + parametros;
}
