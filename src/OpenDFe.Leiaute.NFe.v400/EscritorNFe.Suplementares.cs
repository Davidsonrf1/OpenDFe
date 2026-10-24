namespace OpenDFe.NFe;

// O infNFeSupl (docs/CONTRATOS.md 16): a NFC-e e a NF-e com o DANFE Simplificado Tipo 2 (tpImp 6, NT 2026.002) levam o
// QR Code e a URL da consulta pela chave, gerados pela lib a partir da chave, do ambiente e da ConfiguracaoNFCe. O grupo
// fica fora do infNFe, que é o que a assinatura cobre: escrito antes de assinar, não muda o DigestValue.
internal static partial class EscritorNFe
{
    // O grupo que vai no XML: o gerado, nas notas com QR Code; nas outras, o que a nota traz (a NF-e sem tpImp 6 com o
    // grupo é a regra 393, do leiaute, depois do schema). Nulo: nenhum grupo (com o problema, se for o caso).
    private static InformacoesSuplementares? Suplementares(EscritaNFe e, NotaFiscal nota, ValoresNucleo nucleo)
    {
        var temQrCode = nucleo.Documento == Documento.NFCe
            || (nucleo.Documento == Documento.NFe && nota.Identificacao?.FormatoImpressao == FormatoImpressao.DanfeSimplificadoTipo2);
        if (!temQrCode)
            return nota.Suplementares;
        // Sem a chave, a escrita já tem o erro e não há QR Code a gerar.
        if (e.Chave is not { } chave)
            return null;

        if (nucleo.Emissao == TipoEmissao.ContingenciaOffline)
        {
            e.RegistrarSuplementares(CodigosEscrita.QrCode,
                "O QR Code da contingência offline (tpEmis 9) ainda não é gerado pela lib: ele entra na etapa 5.2.");
            return null;
        }

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
        var gerado = new InformacoesSuplementares
        {
            QrCode = csc is null
                ? QrCodeNFCe.Versao3(urlQrCode, chave, nucleo.Ambiente)
                : QrCodeNFCe.Versao2(urlQrCode, chave, nucleo.Ambiente, csc),
            UrlChave = urlChave,
        };

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
}
