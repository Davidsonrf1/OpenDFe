using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O QR Code da NFC-e e da NF-e com DANFE Simplificado Tipo 2 (tpImp 6), no grupo infNFeSupl: as regras do
// grupo ZX (MOC 7.0, Anexo I; NT 2025.001 v1.02 e NT 2026.002 v1.11) e o cálculo do Manual do DANFE NFC-e e
// QR Code 6.0 (4.3 e 4.4), em código próprio. O schema já confere a forma de cada versão (a 100 com os
// parâmetros nomeados, a 2 e a 3 com os separados por "|", online ou offline pelo tpEmis da chave) e, com ela,
// os parâmetros inexistentes ou fora do formato (396, 400) e a assinatura que sobra ou falta na versão 3 (445,
// 474). A URL do site da UF (395, 878) não é conferida.
internal static class QrCode
{
    private static readonly XNamespace NamespaceAssinatura = "http://www.w3.org/2000/09/xmldsig#";

    // As regras, na ordem: o grupo na NF-e comum (393) e a falta dele (394); a versão 100 (903) e, na NF-e com
    // tpImp 6, outra versão que não a 3 (672); a versão 2 do emitente CPF (444); a chave e o ambiente (397); na
    // offline, o dia da emissão e o valor (397); na versão 2, o DigestValue da offline (397), o CSC (462, 463)
    // e o hash (464); na versão 3 offline, o destinatário (397) e a assinatura (583). Nulo: passou.
    public static (string Codigo, string Complemento)? Conferir(XElement nota, string chave, CodigosSeguranca codigos)
    {
        var infNFe = nota.Element(Servico.NamespaceNFe + "infNFe")!;
        var modelo = Ide(infNFe, "mod");
        var simplificada = modelo == "65" || Ide(infNFe, "tpImp") == "6";
        var suplementar = nota.Element(Servico.NamespaceNFe + "infNFeSupl");
        if (!simplificada)
            return suplementar is null ? null : (Motivos.SuplementarNfe, "");
        if (suplementar is null)
            return (Motivos.QrCodeAusente, "");

        var texto = suplementar.Element(Servico.NamespaceNFe + "qrCode")!.Value;
        var inicio = texto.LastIndexOf("?p=", StringComparison.Ordinal);
        if (inicio < 0)
            return (modelo == "65" ? Motivos.VersaoQrCodeAntiga : Motivos.VersaoQrCodeSimplificado, "");
        var parametros = texto[(inicio + 3)..].Split('|');
        var versao = parametros[1];
        if (modelo == "55" && versao != "3")
            return (Motivos.VersaoQrCodeSimplificado, "");
        var emitente = infNFe.Element(Servico.NamespaceNFe + "emit")!;
        var cnpj = emitente.Element(Servico.NamespaceNFe + "CNPJ")?.Value;
        if (versao == "2" && cnpj is null)
            return (Motivos.QrCodePessoaFisica, "");

        // A chave conferida garante, pelo schema, a forma online ou offline que o tpEmis dela pede.
        if (parametros[0] != chave)
            return Divergente("chNFe");
        if (parametros[2] != Ide(infNFe, "tpAmb"))
            return Divergente("tpAmb");
        var offline = Ide(infNFe, "tpEmis") == "9";
        if (offline)
        {
            if (parametros[3] != Ide(infNFe, "dhEmi")[8..10])
                return Divergente("dhEmi");
            var total = infNFe.Element(Servico.NamespaceNFe + "total")!.Element(Servico.NamespaceNFe + "ICMSTot")!.Element(Servico.NamespaceNFe + "vNF")!.Value;
            if (decimal.Parse(parametros[4], CultureInfo.InvariantCulture) != decimal.Parse(total, CultureInfo.InvariantCulture))
                return Divergente("vNF");
        }

        var assinatura = nota.Element(NamespaceAssinatura + "Signature")!;
        if (versao == "2")
        {
            if (offline)
            {
                var resumo = assinatura.Element(NamespaceAssinatura + "SignedInfo")!.Element(NamespaceAssinatura + "Reference")!
                    .Element(NamespaceAssinatura + "DigestValue")!.Value;
                if (!string.Equals(parametros[5], Hexadecimal(resumo), StringComparison.OrdinalIgnoreCase))
                    return Divergente("digVal");
            }
            var csc = codigos.Procurar(UnidadesFederativas.Sigla(Ide(infNFe, "cUF"))!, cnpj![..8], parametros[^2]);
            if (csc is null)
                return (Motivos.CscInexistente, "");
            var emissao = DateTimeOffset.ParseExact(Ide(infNFe, "dhEmi"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
            if (csc.Revogacao <= emissao)
                return (Motivos.CscRevogado, "");
            if (!string.Equals(parametros[^1], Hash(string.Join('|', parametros[..^1]), csc.Csc), StringComparison.OrdinalIgnoreCase))
                return (Motivos.HashQrCodeDivergente, "");
            return null;
        }

        if (!offline)
            return null;
        if ((parametros[5], parametros[6]) != Destinatario(infNFe))
            return Divergente("idDest");
        var certificado = assinatura.Element(NamespaceAssinatura + "KeyInfo")!.Element(NamespaceAssinatura + "X509Data")!
            .Element(NamespaceAssinatura + "X509Certificate")!.Value;
        return Verificar(string.Join('|', parametros[..7]), parametros[7], certificado) ? null : (Motivos.AssinaturaQrCodeDivergente, "");
    }

    // O hash da versão 2 (Manual, 4.3.4 e 4.3.5): o SHA-1 dos parâmetros, separados por "|", com o CSC no fim,
    // em hexadecimal.
#pragma warning disable CA5350 // De propósito: o manual do QR Code define o hash em SHA-1.
    public static string Hash(string parametros, string csc) =>
        Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(parametros + csc)));
#pragma warning restore CA5350

    // O DigestValue da versão 2 offline (Manual, 4.3.5, passo 1): o texto em Base64, cada caractere em
    // hexadecimal.
    public static string Hexadecimal(string resumo) => Convert.ToHexStringLower(Encoding.ASCII.GetBytes(resumo));

    // O 397 com o parâmetro divergente no complemento.
    private static (string, string) Divergente(string parametro) => (Motivos.QrCodeDivergente, $" [Param: {parametro}]");

    // O 6º e o 7º parâmetros da versão 3 offline (Manual, 4.4.2): o tipo (1 CNPJ, 2 CPF, 3 estrangeiro) e o
    // documento do destinatário; vazios sem destinatário, e o documento vazio no estrangeiro.
    private static (string, string) Destinatario(XElement infNFe)
    {
        var destinatario = infNFe.Element(Servico.NamespaceNFe + "dest");
        if (destinatario?.Element(Servico.NamespaceNFe + "CNPJ") is { } cnpj)
            return ("1", cnpj.Value);
        if (destinatario?.Element(Servico.NamespaceNFe + "CPF") is { } cpf)
            return ("2", cpf.Value);
        return destinatario is null ? ("", "") : ("3", "");
    }

    // A assinatura da versão 3 offline (Manual, 4.4.2, 8º parâmetro): RSA-SHA1 dos parâmetros 1 a 7, em Base64,
    // com o certificado que assina a nota (o do KeyInfo, que a verificação da assinatura já aceitou).
    private static bool Verificar(string parametros, string valor, string certificado)
    {
        byte[] assinatura;
        try
        {
            assinatura = Convert.FromBase64String(valor);
        }
        catch (FormatException)
        {
            return false;
        }
        using var x509 = X509CertificateLoader.LoadCertificate(Convert.FromBase64String(certificado));
        using var chave = x509.GetRSAPublicKey();
        return chave is not null && chave.VerifyData(Encoding.UTF8.GetBytes(parametros), assinatura, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
    }

    private static string Ide(XElement infNFe, string campo) =>
        infNFe.Element(Servico.NamespaceNFe + "ide")!.Element(Servico.NamespaceNFe + campo)!.Value;
}
