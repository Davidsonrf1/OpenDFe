using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O Preparar com a contingência offline (etapa 5.2, CONTRATOS.md 17, decisões 2, 4 e 7): o núcleo com tpEmis 9 e a
// Contingencia devolve a NFC-e assinada com o QR Code offline nas duas versões, e sem a Contingencia o núcleo nem nasce.
public class PreparoOfflineTestes
{
    private static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";
    private static readonly Contingencia Entrada = new(NotasTeste.Emissao, "Sem comunicacao com a SEFAZ");

    private static DFe Criar(ConexoesSefaz conexoes, Documento documento = Documento.NFCe, ConfiguracaoNFCe? configuracao = null) =>
        DFe.Criar(new LeiauteNFe400(), documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = NotasTeste.CertificadoEmitente,
            Conexoes = conexoes,
            Emissao = TipoEmissao.ContingenciaOffline,
            Contingencia = Entrada,
            Documento = configuracao,
        });

    private static string QrCode(string xml) => XElement.Parse(xml).Element(Nfe + "infNFeSupl")!.Element(Nfe + "qrCode")!.Value;

    [Fact]
    public void ANFCeOfflineSaiPreparadaNaVersao3ComAAssinaturaDoQrCodeEADaNota()
    {
        using var conexoes = new ConexoesSefaz();
        var dfe = Criar(conexoes);

        var preparado = dfe.Preparar(NotasTeste.NFCe());

        Assert.True(preparado.Preparado);
        Assert.Empty(preparado.Problemas);
        Assert.Empty(SchemasNFe400.Nota.Validar(preparado.Xml!));
        Assert.Empty(AssinaturaDigital.Verificar(preparado.Xml!, "NFe" + preparado.Chave, NotasTeste.CnpjEmitente));
        var nota = XElement.Parse(preparado.Xml!).Element(Nfe + "infNFe")!;
        Assert.Equal("9", nota.Element(Nfe + "ide")!.Element(Nfe + "tpEmis")!.Value);
        Assert.Equal("Sem comunicacao com a SEFAZ", nota.Element(Nfe + "ide")!.Element(Nfe + "xJust")!.Value);
        Assert.Equal(Documento.NFCe, preparado.Chave!.Modelo);
        Assert.Equal(TipoEmissao.ContingenciaOffline, preparado.Chave.TipoEmissao);
        var p = QrCode(preparado.Xml!).Split("?p=")[1].Split('|');
        Assert.Equal([preparado.Chave.ToString(), "3", "2"], p[..3]);
        using var chave = NotasTeste.CertificadoEmitente.GetRSAPublicKey()!;
        Assert.True(chave.VerifyData(Encoding.UTF8.GetBytes(string.Join('|', p[..7])), Convert.FromBase64String(p[7]),
            HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void ANFCeOfflineComOCscSaiPreparadaNaVersao2ComODigValDaAssinaturaPreparada()
    {
        using var conexoes = new ConexoesSefaz();
        var dfe = Criar(conexoes, configuracao: new ConfiguracaoNFCe(new CodigoSeguranca(1, "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO")));

        var preparado = dfe.Preparar(NotasTeste.NFCe());

        Assert.True(preparado.Preparado);
        var p = QrCode(preparado.Xml!).Split("?p=")[1].Split('|');
        Assert.Equal("2", p[1]);
        var resumo = XElement.Parse(preparado.Xml!).Element(XNamespace.Get("http://www.w3.org/2000/09/xmldsig#") + "Signature")!
            .Descendants(XNamespace.Get("http://www.w3.org/2000/09/xmldsig#") + "DigestValue").Single().Value;
        Assert.Equal(Convert.ToHexStringLower(Encoding.ASCII.GetBytes(resumo)), p[5]);
    }

    [Fact]
    public void ANFeTpImp6OfflineSaiPreparadaNaVersao3()
    {
        using var conexoes = new ConexoesSefaz();
        var nota = NotasTeste.NFCe();
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;

        var preparado = Criar(conexoes, Documento.NFe).Preparar(nota);

        Assert.True(preparado.Preparado);
        Assert.Empty(preparado.Problemas);
        Assert.Equal("3", QrCode(preparado.Xml!).Split("?p=")[1].Split('|')[1]);
    }

    [Fact]
    public void ANFeOfflineSemOTpImp6EARegra711()
    {
        using var conexoes = new ConexoesSefaz();

        var preparado = Criar(conexoes, Documento.NFe).Preparar(NotasTeste.NFe());

        Assert.False(preparado.Preparado);
        Assert.Contains(preparado.Problemas, p => p.Codigo == "711" && p.Severidade == Severidade.Erro);
    }

    [Fact]
    public void SemAContingenciaONucleoOfflineNemNasce()
    {
        using var conexoes = new ConexoesSefaz();

        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = NotasTeste.CertificadoEmitente,
            Conexoes = conexoes,
            Emissao = TipoEmissao.ContingenciaOffline,
        }));
    }

    [Fact]
    public void ADataEAJustificativaDaNotaDiferentesDasDoNucleoSaoErro()
    {
        using var conexoes = new ConexoesSefaz();
        var nota = NotasTeste.NFCe();
        nota.Identificacao.JustificativaContingencia = "Outra justificativa qualquer";

        var preparado = Criar(conexoes).Preparar(nota);

        Assert.False(preparado.Preparado);
        Assert.Contains(preparado.Problemas, p => p.Codigo == "NFE-NUCLEO");
    }
}
