using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// O que a forma offline do QR Code da NFC-e precisa da nota (Manual de Padrões Técnicos do DANFE NFC-e e QR Code 6.0,
/// 4.3.2 e 4.4.2; docs/CONTRATOS.md 17): o dia da emissão, o valor total, o resumo da assinatura (versão 2) e o
/// destinatário e a assinatura dos parâmetros (versão 3). Tudo se calcula sobre o XML escrito, antes de o núcleo
/// assinar. Sem estado.
/// </summary>
internal static class QrCodeOffline
{
    /// <summary>Os dados da nota que entram no QR Code offline, lidos do XML escrito.</summary>
    internal sealed record DadosNota(string Dia, string Valor, string TipoDestinatario, string Documento);

    /// <summary>
    /// O <c>dia</c> (o dia do mês de <c>dhEmi</c>, 2 dígitos, no fuso da nota), o <c>vNF</c> como está na nota e o
    /// destinatário: 1 e o CNPJ, 2 e o CPF, 3 e vazio (estrangeiro), os dois vazios sem destinatário.
    /// </summary>
    internal static DadosNota Ler(string xml)
    {
        var documento = Carregar(xml);
        var infNFe = documento.DocumentElement!.SelectSingleNode("*[local-name()='infNFe']")!;
        var emissao = Texto(infNFe, "ide", "dhEmi") ?? throw new InvalidOperationException("A nota escrita não tem a data da emissão.");
        var valor = Texto(infNFe, "total", "ICMSTot", "vNF") ?? throw new InvalidOperationException("A nota escrita não tem o valor total.");
        var destinatario = infNFe.SelectSingleNode("*[local-name()='dest']");
        var (tipo, numero) = destinatario switch
        {
            null => ("", ""),
            _ when Texto(destinatario, "CNPJ") is { } cnpj => ("1", cnpj),
            _ when Texto(destinatario, "CPF") is { } cpf => ("2", cpf),
            _ => ("3", ""),
        };
        return new DadosNota(emissao.Substring(8, 2), valor, tipo, numero);
    }

    /// <summary>
    /// O <c>digVal</c> da versão 2: o <c>DigestValue</c> que a assinatura da nota terá, em Base64, e cada caractere dele
    /// em hexadecimal (Manual, 4.3.5, passo 1), como a SEFAZ confere. O resumo é calculado pelo mesmo caminho da
    /// assinatura (<see cref="SignedXml"/> com a referência <i>enveloped</i> e C14N sobre o <c>Id</c>), com uma chave
    /// HMAC descartável no lugar do certificado, porque o resumo do <c>infNFe</c> não depende da chave.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms",
        Justification = "O resumo SHA-1 é o da assinatura XMLDSig da SEFAZ; o QR Code offline da versão 2 o repete.")]
    internal static string DigVal(string xml, string id)
    {
        var documento = Carregar(xml);
        var assinador = new SignedXml(documento);
        assinador.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
        var referencia = new Reference("#" + id) { DigestMethod = SignedXml.XmlDsigSHA1Url };
        referencia.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        referencia.AddTransform(new XmlDsigC14NTransform());
        assinador.AddReference(referencia);
        using var hmac = new HMACSHA1(RandomNumberGenerator.GetBytes(20));
        assinador.ComputeSignature(hmac);
        return Convert.ToHexStringLower(Encoding.ASCII.GetBytes(Convert.ToBase64String(referencia.DigestValue!)));
    }

    /// <summary>
    /// A assinatura da versão 3: RSA com SHA-1 (PKCS#1 v1.5) dos sete primeiros parâmetros separados por <c>|</c>, em
    /// Base64 (Manual, 4.4.2).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms",
        Justification = "O RSA-SHA1 é a assinatura que o Manual do DANFE NFC-e e QR Code manda para a versão 3 offline.")]
    internal static string Assinar(string parametros, RSA chave) =>
        Convert.ToBase64String(chave.SignData(Encoding.UTF8.GetBytes(parametros), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));

    private static XmlDocument Carregar(string xml)
    {
        var documento = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        using var leitor = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        documento.Load(leitor);
        return documento;
    }

    // O texto do elemento no caminho (filhos diretos, a partir de pai); nulo se não existe ou está vazio.
    private static string? Texto(XmlNode pai, params string[] caminho)
    {
        var passo = pai;
        foreach (var nome in caminho)
        {
            passo = passo.SelectSingleNode($"*[local-name()='{nome}']");
            if (passo is null)
                return null;
        }
        return string.IsNullOrEmpty(passo.InnerText) ? null : passo.InnerText;
    }
}
