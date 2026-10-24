using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O QR Code da contingência offline que a lib escreve (etapa 5.2, CONTRATOS.md 17): as duas formas (versão 2 com o digVal e
// o hash, versão 3 com a assinatura), o dia, o vNF e o destinatário; a prova de que o digVal é o DigestValue da assinatura
// que o núcleo faz e de que a assinatura da versão 3 confere com a chave pública; o grupo válido no schema depois de
// assinado. Os valores esperados são calculados pelo caminho direto (System.Security.Cryptography, o XML assinado), sem a
// classe que gera o QR Code.
public class EscritaOfflineTestes
{
    private const string UrlMGQrCode = "https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml";
    private const string CodigoCsc = "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO";
    private static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";
    private static readonly XNamespace Assinatura = "http://www.w3.org/2000/09/xmldsig#";

    private static readonly ConfiguracaoNFCe Versao2 = new(new CodigoSeguranca(1, CodigoCsc));

    private static ValoresNucleo Nucleo(Documento documento = Documento.NFCe, Ambiente ambiente = Ambiente.Homologacao,
        ConfiguracaoNFCe? configuracao = null, UF uf = UF.MG, bool comCertificado = true) => new()
    {
        Documento = documento,
        UF = uf,
        Ambiente = ambiente,
        Emissao = TipoEmissao.ContingenciaOffline,
        Contingencia = new Contingencia(NotasTeste.Emissao, "Teste do QR Code em contingencia"),
        NFCe = configuracao,
        Certificado = comCertificado ? NotasTeste.CertificadoEmitente : null,
    };

    private static NotaEscrita Escrever(NotaFiscal? nota = null, ValoresNucleo? nucleo = null) =>
        NotasTeste.Escrever(nota ?? NotasTeste.NFCe(), nucleo ?? Nucleo());

    private static string QrCode(string xml) => XElement.Parse(xml).Element(Nfe + "infNFeSupl")!.Element(Nfe + "qrCode")!.Value;

    private static string[] Parametros(string xml)
    {
        var qr = QrCode(xml);
        return qr[(qr.IndexOf("?p=", StringComparison.Ordinal) + 3)..].Split('|');
    }

    // O DigestValue da assinatura que o núcleo faz sobre a nota, no texto de hexadecimal que o manual manda (4.3.5, passo 1).
    private static string DigValAssinatura(NotaEscrita escrita)
    {
        var assinada = NotasTeste.Assinar(escrita);
        var resumo = XElement.Parse(assinada).Element(Assinatura + "Signature")!.Element(Assinatura + "SignedInfo")!
            .Element(Assinatura + "Reference")!.Element(Assinatura + "DigestValue")!.Value;
        return Convert.ToHexStringLower(Encoding.ASCII.GetBytes(resumo));
    }

    private static bool Confere(string[] parametros)
    {
        using var chave = NotasTeste.CertificadoEmitente.GetRSAPublicKey()!;
        return chave.VerifyData(Encoding.UTF8.GetBytes(string.Join('|', parametros[..7])), Convert.FromBase64String(parametros[7]),
            HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
    }

    [Theory]
    [InlineData(Ambiente.Homologacao, "2")]
    [InlineData(Ambiente.Producao, "1")]
    public void AVersao3OfflineTemOsOitoParametrosEAAssinaturaConfereComAChavePublica(Ambiente ambiente, string tpAmb)
    {
        var escrita = Escrever(nucleo: Nucleo(ambiente: ambiente));

        Assert.Empty(escrita.Problemas);
        Assert.StartsWith(UrlMGQrCode + "?p=", QrCode(escrita.Xml), StringComparison.Ordinal);
        var p = Parametros(escrita.Xml);
        Assert.Equal(8, p.Length);
        Assert.Equal([escrita.Chave!.ToString(), "3", tpAmb, "20", "50.00", "", ""], p[..7]);
        Assert.True(Confere(p));
        Assert.Contains("</infNFe><infNFeSupl>", escrita.Xml, StringComparison.Ordinal);
        Assert.EndsWith("</infNFeSupl></NFe>", escrita.Xml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(Ambiente.Homologacao, "2")]
    [InlineData(Ambiente.Producao, "1")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5350", Justification = "O hash do QR Code da versão 2 é SHA-1, como o manual manda.")]
    public void AVersao2OfflineTemODigValAssinaturaEOHashDoCsc(Ambiente ambiente, string tpAmb)
    {
        var escrita = Escrever(nucleo: Nucleo(ambiente: ambiente, configuracao: Versao2));

        Assert.Empty(escrita.Problemas);
        var p = Parametros(escrita.Xml);
        Assert.Equal(8, p.Length);
        Assert.Equal([escrita.Chave!.ToString(), "2", tpAmb, "20", "50.00", DigValAssinatura(escrita), "1"], p[..7]);
        var esperado = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(string.Join('|', p[..7]) + CodigoCsc)));
        Assert.Equal(esperado, p[7]);
        Assert.Matches("^[0-9A-F]{40}$", p[7]);
        Assert.Matches("^[0-9a-f]{56}$", p[5]);
    }

    [Fact]
    public void ODigValDoQrCodeEOResumoDaAssinaturaDaNotaEOGrupoFicaForaDela()
    {
        // O resumo é do infNFe no contexto do documento (o xmlns da raiz entra na canonicalização): assinando o texto
        // escrito, o DigestValue tem de ser o que o QR Code já trazia.
        var escrita = Escrever(nucleo: Nucleo(configuracao: Versao2));
        var assinada = NotasTeste.Assinar(escrita);

        Assert.Equal(Parametros(escrita.Xml)[5], DigValAssinatura(escrita));
        Assert.Equal(QrCode(escrita.Xml), QrCode(assinada));
        Assert.Contains("</infNFe><infNFeSupl>", assinada, StringComparison.Ordinal);
        Assert.Contains("</infNFeSupl><Signature", assinada, StringComparison.Ordinal);
    }

    [Fact]
    public void ODigValMudaQuandoANotaMuda()
    {
        var a = Escrever(nucleo: Nucleo(configuracao: Versao2));
        var nota = NotasTeste.NFCe();
        nota.Itens[0].Produto.Descricao = "OUTRA DESCRICAO";
        var b = Escrever(nota, Nucleo(configuracao: Versao2));

        Assert.NotEqual(Parametros(a.Xml)[5], Parametros(b.Xml)[5]);
        Assert.Equal(Parametros(b.Xml)[5], DigValAssinatura(b));
    }

    [Theory]
    [InlineData(2026, 10, 31, 23, 59, -3, "31")]
    [InlineData(2026, 11, 1, 0, 0, -3, "01")]
    [InlineData(2026, 10, 31, 22, 0, -3, "31")]   // 01/11 01:00 em UTC: o dia é o do fuso da nota
    [InlineData(2026, 11, 1, 1, 0, 5, "01")]      // 31/10 20:00 em UTC: o dia é o do fuso da nota
    [InlineData(2026, 2, 9, 12, 0, -3, "09")]
    public void ODiaEODoMesDaEmissaoNoFusoDaNota(int ano, int mes, int dia, int hora, int minuto, int fuso, string esperado)
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.Emissao = new DateTimeOffset(ano, mes, dia, hora, minuto, 0, TimeSpan.FromHours(fuso));

        var p = Parametros(Escrever(nota).Xml);

        Assert.Equal(esperado, p[3]);
        Assert.True(Confere(p));
    }

    [Theory]
    [InlineData("50.00", "50.00")]
    [InlineData("1234.56", "1234.56")]
    [InlineData("0.01", "0.01")]
    [InlineData("100", "100.00")]
    public void OVNFSaiComoNaNota(string valor, string esperado)
    {
        var nota = NotasTeste.NFCe();
        nota.Total.Icms.ValorNota = decimal.Parse(valor, CultureInfo.InvariantCulture);

        var escrita = Escrever(nota);

        Assert.Equal(esperado, Parametros(escrita.Xml)[4]);
        Assert.Contains($"<vNF>{esperado}</vNF>", escrita.Xml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("11444777000161", null, null, "1", "11444777000161")]
    [InlineData(null, "52998224725", null, "2", "52998224725")]
    [InlineData(null, null, "ABC123456", "3", "")]
    public void OTipoEODocumentoDoDestinatario(string? cnpj, string? cpf, string? estrangeiro, string tipo, string documento)
    {
        var nota = NotasTeste.NFCe();
        nota.Destinatario = new Destinatario { Cnpj = cnpj, Cpf = cpf, IdEstrangeiro = estrangeiro };

        var p = Parametros(Escrever(nota).Xml);

        Assert.Equal([tipo, documento], p[5..7]);
        Assert.True(Confere(p));
    }

    [Fact]
    public void SemDestinatarioOsDoisParametrosSaemVazios()
    {
        var escrita = Escrever();
        var p = Parametros(escrita.Xml);

        Assert.Equal(["", ""], p[5..7]);
        Assert.Contains("|50.00|||", QrCode(escrita.Xml), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OGrupoOfflineEValidoNoSchemaDepoisDeAssinado(bool versao2)
    {
        var escrita = Escrever(nucleo: Nucleo(configuracao: versao2 ? Versao2 : null));

        Assert.Empty(NotasTeste.Validar(escrita));
    }

    [Fact]
    public void ANFeComTpImp6OfflineSaiNaVersao3EValidaNoSchema()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
        var escrita = Escrever(nota, Nucleo(Documento.NFe));

        Assert.Empty(NotasTeste.Validar(escrita));
        var p = Parametros(escrita.Xml);
        Assert.Equal("3", p[1]);
        Assert.True(Confere(p));
    }

    [Fact]
    public void ANFeComTpImp6OfflineIgnoraAVersao2Configurada()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;

        var p = Parametros(Escrever(nota, Nucleo(Documento.NFe, configuracao: Versao2)).Xml);

        Assert.Equal("3", p[1]);
    }

    [Fact]
    public void ANotaOfflineNaUFSemUrlEErroNFEQrCode()
    {
        var escrita = Escrever(nucleo: Nucleo(uf: UF.SP));

        var problema = Assert.Single(escrita.Problemas);
        Assert.Equal(("NFE-QRCODE", Severidade.Erro), (problema.Codigo, problema.Severidade));
        Assert.DoesNotContain("infNFeSupl", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void AVersao3OfflineSemOCertificadoEErroNFEQrCodeEAVersao2NaoPrecisaDele()
    {
        var semCertificado = Escrever(nucleo: Nucleo(comCertificado: false));
        var versao2 = Escrever(nucleo: Nucleo(configuracao: Versao2, comCertificado: false));

        var problema = Assert.Single(semCertificado.Problemas);
        Assert.Equal(("NFE-QRCODE", Severidade.Erro), (problema.Codigo, problema.Severidade));
        Assert.Contains("certificado", problema.Mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("infNFeSupl", semCertificado.Xml, StringComparison.Ordinal);
        Assert.Empty(versao2.Problemas);
    }

    [Fact]
    public void OSuplementaresDiferenteDoGeradoEErroNFENucleoNaOffline()
    {
        var nota = NotasTeste.NFCe();
        nota.Suplementares = new InformacoesSuplementares { QrCode = "https://qr?p=1" };

        var escrita = Escrever(nota);

        Assert.Equal("NFE-NUCLEO", Assert.Single(escrita.Problemas).Codigo);
    }

    [Fact]
    public void OSuplementaresIgualAoGeradoPassaNaOffline()
    {
        var gerada = Escrever();
        var nota = NotasTeste.NFCe();
        var grupo = XElement.Parse(gerada.Xml).Element(Nfe + "infNFeSupl")!;
        // A assinatura da versão 3 é determinística (PKCS#1 v1.5): a nota igual dá o mesmo texto.
        nota.Suplementares = new InformacoesSuplementares
        {
            QrCode = grupo.Element(Nfe + "qrCode")!.Value,
            UrlChave = grupo.Element(Nfe + "urlChave")!.Value,
        };

        var escrita = Escrever(nota);

        Assert.Empty(escrita.Problemas);
        Assert.Equal(gerada.Xml, escrita.Xml);
    }

    [Fact]
    public void OCscNaoApareceNaNotaNemNosProblemasDaOffline()
    {
        var escrita = Escrever(nucleo: Nucleo(configuracao: Versao2));

        Assert.DoesNotContain(CodigoCsc, escrita.Xml, StringComparison.Ordinal);
        var ruim = Escrever(nucleo: Nucleo(configuracao: Versao2, uf: UF.SP));
        Assert.All(ruim.Problemas, p => Assert.DoesNotContain(CodigoCsc, p.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public void ANotaOnlineNaoMudou()
    {
        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), NotasTeste.Nucleo());

        Assert.EndsWith("|3|2</qrCode><urlChave>https://hportalsped.fazenda.mg.gov.br/portalnfce</urlChave></infNFeSupl></NFe>",
            escrita.Xml, StringComparison.Ordinal);
    }
}
