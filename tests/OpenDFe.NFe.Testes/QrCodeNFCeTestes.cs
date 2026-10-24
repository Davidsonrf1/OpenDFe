using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O texto do QR Code online (etapa 5.1, CONTRATOS.md 16.3): o hash contra os dois exemplos do Manual 6.0 e as duas
// versões.
public class QrCodeNFCeTestes
{
    // O CSC dos exemplos do manual tem 41 caracteres (o CodigoSeguranca o recusaria): o cálculo interno aceita.
    private const string CscManual = "SEU-CODIGO-CSC-CONTRIBUINTE-36-CARACTERES";

    private const string Url = "https://uf.exemplo.gov.br/nfce/qrcode";

    // A chave do exemplo do manual tem o DV errado (é ilustrativa); o texto do QR Code usa uma chave válida.
    private static readonly ChaveAcesso Chave = ChaveAcesso.Montar(UF.MG, new DateTimeOffset(2026, 10, 20, 14, 30, 0, TimeSpan.FromHours(-3)),
        NotasTeste.CnpjEmitente, Documento.NFCe, 1, 123, TipoEmissao.Normal, 12345678);

    private static string ChaveValida => Chave.ToString();

    private const string ChaveExemplo = "28170800156225000131650110000151341562040824";

    [Fact]
    public void OHashDoPrimeiroExemploDoManualConfere()
    {
        var hash = QrCodeNFCe.Hash($"{ChaveExemplo}|2|1|1", CscManual);

        Assert.Equal("DC6AE2C2B9A992BE59679AC365E29922DE6B7511", hash);
    }

    [Fact]
    public void OHashDoSegundoExemploDoManualConfere()
    {
        // A NFC-e offline da versão 2 (4.3.6.2): a mesma regra do hash, com os 6 parâmetros antes do idCSC.
        var parametros = "28170800156225000131650110000151349562040824|2|1|02|60.90|"
            + "797a4759685578312f5859597a6b7357422b6650523351633530633d|1";

        Assert.Equal("4615A93BB0D7C4E780F8D30EE77EDD5BA55C7D66", QrCodeNFCe.Hash(parametros, CscManual));
    }

    [Fact]
    public void AVersao3SoTemAChaveEOAmbiente()
    {
        Assert.Equal($"{Url}?p={ChaveValida}|3|2", QrCodeNFCe.Versao3(Url, Chave, Ambiente.Homologacao));
        Assert.Equal($"{Url}?p={ChaveValida}|3|1", QrCodeNFCe.Versao3(Url, Chave, Ambiente.Producao));
    }

    [Fact]
    public void AVersao2LevaOIdSemZerosEOHashEmMaiusculas()
    {
        var csc = new CodigoSeguranca(1, "OPENDFE-CSC-DE-TESTE-0001");

        var texto = QrCodeNFCe.Versao2(Url, Chave, Ambiente.Producao, csc);

        var hash = QrCodeNFCe.Hash($"{ChaveValida}|2|1|1", csc.Codigo);
        Assert.Equal($"{Url}?p={ChaveValida}|2|1|1|{hash}", texto);
        Assert.Matches("^[0-9A-F]{40}$", hash);
    }

    [Fact]
    public void OIdentificadorVaiSemZerosEOHashMudaComOCsc()
    {
        var a = QrCodeNFCe.Versao2(Url, Chave, Ambiente.Homologacao, new CodigoSeguranca(12, "OPENDFE-CSC-DE-TESTE-0001"));
        var b = QrCodeNFCe.Versao2(Url, Chave, Ambiente.Homologacao, new CodigoSeguranca(12, "OPENDFE-CSC-DE-TESTE-0002"));

        Assert.Contains("|2|2|12|", a, StringComparison.Ordinal);
        Assert.NotEqual(a, b);
    }
}
