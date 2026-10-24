using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.NFe;

// O infNFeSupl (docs/CONTRATOS.md 16): a NFC-e e a NF-e com o DANFE Simplificado Tipo 2 (tpImp 6, NT 2026.002) levam o
// QR Code e a URL da consulta pela chave, gerados pela lib a partir da chave, do ambiente e da ConfiguracaoNFCe. O grupo
// fica fora do infNFe, que é o que a assinatura cobre: escrito antes de assinar, não muda o DigestValue. Na contingência
// offline (tpEmis 9, etapa 5.2, CONTRATOS.md 17) o QR Code leva o dia, o valor, o resumo da assinatura (versão 2) ou o
// destinatário e uma assinatura (versão 3): sai depois do infNFe pronto, inserido no texto sem mexer num byte dele.
internal static partial class EscritorNFe
{
    // A NFC-e, ou a NF-e com o DANFE Simplificado Tipo 2: as que levam o QR Code.
    private static bool TemQrCode(NotaFiscal nota, ValoresNucleo nucleo) =>
        nucleo.Documento == Documento.NFCe
        || (nucleo.Documento == Documento.NFe && nota.Identificacao?.FormatoImpressao == FormatoImpressao.DanfeSimplificadoTipo2);

    // O grupo que vai no XML: o gerado, nas notas com QR Code; nas outras, o que a nota traz (a NF-e sem tpImp 6 com o
    // grupo é a regra 393, do leiaute, depois do schema). Nulo: nenhum grupo (com o problema, se for o caso). Na offline,
    // o grupo precisa do infNFe escrito: sem o texto (xmlPronto nulo, durante a escrita) devolve nulo, e a escrita chama
    // de novo com o XML pronto.
    private static InformacoesSuplementares? Suplementares(EscritaNFe e, NotaFiscal nota, ValoresNucleo nucleo, string? xmlPronto = null)
    {
        var temQrCode = TemQrCode(nota, nucleo);
        if (!temQrCode)
            return nota.Suplementares;
        // Sem a chave, a escrita já tem o erro e não há QR Code a gerar.
        if (e.Chave is not { } chave)
            return null;

        var offline = nucleo.Emissao == TipoEmissao.ContingenciaOffline;
        if (offline && xmlPronto is null)
            return null;

        var configuracao = nucleo.NFCe;
        var tabela = UrlsConsultaNFCe.Procurar(nucleo.UF, nucleo.Ambiente);
        var urlQrCode = configuracao?.UrlQrCode?.OriginalString ?? tabela?.QrCode.OriginalString;
        var urlChave = configuracao?.UrlChave?.OriginalString ?? tabela?.Chave.OriginalString;
        if (urlQrCode is null || urlChave is null)
        {
            e.RegistrarSuplementares(CodigosEscrita.QrCode,
                $"A tabela de URLs de consulta da NFC-e não tem a UF {nucleo.UF} no ambiente {nucleo.Ambiente}: informe "
                + (urlQrCode is null ? "a UrlQrCode" : "a UrlChave") + " na ConfiguracaoNFCe.");
            return null;
        }

        var csc = nucleo.Documento == Documento.NFCe && configuracao is { QrCode: VersaoQrCode.V2 } ? configuracao.Csc : null;
        string qrCode;
        if (!offline)
            qrCode = csc is null
                ? QrCodeNFCe.Versao3(urlQrCode, chave, nucleo.Ambiente)
                : QrCodeNFCe.Versao2(urlQrCode, chave, nucleo.Ambiente, csc);
        else if (QrCodeOfflineGerar(e, nucleo, xmlPronto!, chave, urlQrCode, csc) is { } texto)
            qrCode = texto;
        else
            return null;
        var gerado = new InformacoesSuplementares { QrCode = qrCode, UrlChave = urlChave };

        // O que quem usa preencheu e difere do gerado (decisão 7): o QR Code de outra chave ou de outro CSC daria 397 ou 464.
        if (nota.Suplementares is { } informado)
        {
            if (!string.IsNullOrEmpty(informado.QrCode) && !string.Equals(informado.QrCode, gerado.QrCode, StringComparison.Ordinal))
                e.RegistrarSuplementares(CodigosEscrita.Nucleo,
                    "O qrCode difere do gerado pela lib: deixe o grupo nulo para usar o gerado.", "qrCode", "QrCode");
            if (!string.IsNullOrEmpty(informado.UrlChave) && !string.Equals(informado.UrlChave, gerado.UrlChave, StringComparison.Ordinal))
                e.RegistrarSuplementares(CodigosEscrita.Nucleo,
                    "O urlChave difere do gerado pela lib: deixe o grupo nulo para usar o gerado.", "urlChave", "UrlChave");
        }
        return gerado;
    }

    // O QR Code da offline (CONTRATOS.md 17): o dia, o valor e o destinatário lidos do XML escrito; na versão 2, o digVal
    // da assinatura que o núcleo vai fazer; na versão 3, a assinatura dos sete parâmetros com o certificado do núcleo.
    // Nulo, com o problema, quando não dá para gerar (a escrita sem a chave não chega aqui).
    private static string? QrCodeOfflineGerar(EscritaNFe e, ValoresNucleo nucleo, string xml, ChaveAcesso chave, string urlQrCode,
        CodigoSeguranca? csc)
    {
        var dados = QrCodeOffline.Ler(xml);
        if (csc is not null)
            return QrCodeNFCe.Versao2Offline(urlQrCode, chave, nucleo.Ambiente, dados, QrCodeOffline.DigVal(xml, "NFe" + chave), csc);

        using var rsa = nucleo.Certificado?.GetRSAPrivateKey();
        if (rsa is null)
        {
            e.RegistrarSuplementares(CodigosEscrita.QrCode,
                "O QR Code da contingência offline na versão 3 é assinado com o certificado da configuração do núcleo, que não está disponível ou não tem a chave privada RSA.");
            return null;
        }
        var parametros = QrCodeNFCe.ParametrosVersao3Offline(chave, nucleo.Ambiente, dados);
        return QrCodeNFCe.Versao3Offline(urlQrCode, parametros, QrCodeOffline.Assinar(parametros, rsa));
    }
}
