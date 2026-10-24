using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace OpenDFe.NFe;

/// <summary>
/// O texto do QR Code da NFC-e emitida online (Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0, 4.3 e 4.4): a
/// URL de consulta da UF, o <c>?p=</c> e os parâmetros separados por <c>|</c>, nas quatro formas: as duas online e as duas da
/// contingência offline (<c>tpEmis</c> 9, etapa 5.2).
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
    /// Versão 2, offline: <c>chave|2|tpAmb|dia|vNF|digVal|idCSC|hash</c> (Manual, 4.3.2), com o <paramref name="digVal"/>
    /// do <see cref="QrCodeOffline.DigVal"/>.
    /// </summary>
    internal static string Versao2Offline(string urlConsulta, ChaveAcesso chave, Ambiente ambiente, QrCodeOffline.DadosNota nota,
        string digVal, CodigoSeguranca csc)
    {
        var parametros = string.Create(CultureInfo.InvariantCulture,
            $"{chave}|2|{(int)ambiente}|{nota.Dia}|{nota.Valor}|{digVal}|{csc.Identificador}");
        return Texto(urlConsulta, parametros + "|" + Hash(parametros, csc.Codigo));
    }

    /// <summary>
    /// Os sete primeiros parâmetros da versão 3 offline: <c>chave|3|tpAmb|dia|vNF|tipoDestinatario|documento</c>, o que a
    /// assinatura cobre (Manual, 4.4.2).
    /// </summary>
    internal static string ParametrosVersao3Offline(ChaveAcesso chave, Ambiente ambiente, QrCodeOffline.DadosNota nota) =>
        string.Create(CultureInfo.InvariantCulture,
            $"{chave}|3|{(int)ambiente}|{nota.Dia}|{nota.Valor}|{nota.TipoDestinatario}|{nota.Documento}");

    /// <summary>Versão 3, offline: os sete parâmetros e a assinatura deles em Base64 (<see cref="QrCodeOffline.Assinar"/>).</summary>
    internal static string Versao3Offline(string urlConsulta, string parametros, string assinatura) =>
        Texto(urlConsulta, parametros + "|" + assinatura);

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
