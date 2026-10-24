using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O infNFeSupl que a lib escreve (etapa 5.1, CONTRATOS.md 16): o QR Code versão 3 e 2 online, as URLs da tabela e as
// trocadas, o grupo válido no schema depois de assinado, a assinatura que não muda com ele, a NF-e com tpImp 6, o
// Suplementares preenchido, a UF sem URL. A contingência offline (5.2) tem os seus testes em EscritaOfflineTestes.
public partial class EscritaQrCodeTestes
{
    private const string UrlMGQrCode = "https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml";
    private const string UrlMGChaveProducao = "https://portalsped.fazenda.mg.gov.br/portalnfce";
    private const string UrlMGChaveHomologacao = "https://hportalsped.fazenda.mg.gov.br/portalnfce";
    private const string CodigoCsc = "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO";

    [GeneratedRegex("<infNFeSupl>.*?</infNFeSupl>", RegexOptions.Singleline)]
    private static partial Regex GrupoSuplementar();

    private static ValoresNucleo Nucleo(Documento documento = Documento.NFCe, Ambiente ambiente = Ambiente.Homologacao,
        UF uf = UF.MG, ConfiguracaoNFCe? configuracao = null, TipoEmissao emissao = TipoEmissao.Normal) => new()
    {
        Documento = documento,
        UF = uf,
        Ambiente = ambiente,
        Emissao = emissao,
        Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(NotasTeste.Emissao, "Teste do QR Code em contingencia"),
        NFCe = configuracao,
    };

    private static NotaFiscal NFeSimplificada()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
        return nota;
    }

    private static (string QrCode, string UrlChave) Grupo(NotaEscrita escrita)
    {
        var supl = XElement.Parse(escrita.Xml).Element(XName.Get("infNFeSupl", "http://www.portalfiscal.inf.br/nfe"))!;
        return (supl.Element(XName.Get("qrCode", "http://www.portalfiscal.inf.br/nfe"))!.Value,
            supl.Element(XName.Get("urlChave", "http://www.portalfiscal.inf.br/nfe"))!.Value);
    }

    [Theory]
    [InlineData(Ambiente.Homologacao, 2, UrlMGChaveHomologacao)]
    [InlineData(Ambiente.Producao, 1, UrlMGChaveProducao)]
    public void ANFCeSaiComOQrCodeDaVersao3EAUrlDaTabela(Ambiente ambiente, int tpAmb, string urlChave)
    {
        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(ambiente: ambiente));

        Assert.Empty(escrita.Problemas);
        var (qrCode, chave) = Grupo(escrita);
        Assert.Equal($"{UrlMGQrCode}?p={escrita.Chave}|3|{tpAmb}", qrCode);
        Assert.Equal(urlChave, chave);
        Assert.Contains("</infNFe><infNFeSupl>", escrita.Xml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(Ambiente.Homologacao, 2)]
    [InlineData(Ambiente.Producao, 1)]
    public void ComOCscASaidaEAVersao2ComOHash(Ambiente ambiente, int tpAmb)
    {
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(1, CodigoCsc));

        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(ambiente: ambiente, configuracao: configuracao));

        Assert.Empty(escrita.Problemas);
        var parametros = $"{escrita.Chave}|2|{tpAmb}|1";
        Assert.Equal($"{UrlMGQrCode}?p={parametros}|{QrCodeNFCe.Hash(parametros, CodigoCsc)}", Grupo(escrita).QrCode);
    }

    [Fact]
    public void AsUrlsTrocadasValemNoLugarDasDaTabela()
    {
        var configuracao = new ConfiguracaoNFCe
        {
            UrlQrCode = new Uri("https://nova.exemplo.gov.br/nfce/qrcode"),
            UrlChave = new Uri("http://nova.exemplo.gov.br/nfce/consulta"),
        };

        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(configuracao: configuracao));

        var (qrCode, chave) = Grupo(escrita);
        Assert.Equal($"https://nova.exemplo.gov.br/nfce/qrcode?p={escrita.Chave}|3|2", qrCode);
        Assert.Equal("http://nova.exemplo.gov.br/nfce/consulta", chave);
    }

    [Fact]
    public void SoUmaDasUrlsTrocadasCompletaComATabela()
    {
        var configuracao = new ConfiguracaoNFCe { UrlChave = new Uri("https://nova.exemplo.gov.br/nfce/consulta") };

        var (qrCode, chave) = Grupo(NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(configuracao: configuracao)));

        Assert.StartsWith(UrlMGQrCode + "?p=", qrCode, StringComparison.Ordinal);
        Assert.Equal("https://nova.exemplo.gov.br/nfce/consulta", chave);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OInfNFeSuplEValidoNoSchemaDepoisDeAssinado(bool versao2)
    {
        var configuracao = versao2 ? new ConfiguracaoNFCe(new CodigoSeguranca(1, CodigoCsc)) : null;
        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(configuracao: configuracao));

        var assinado = NotasTeste.Assinar(escrita);

        Assert.Empty(SchemasNFe400.Nota.Validar(assinado));
        // O schema manda infNFe, infNFeSupl, Signature, nesta ordem.
        var nota = XElement.Parse(assinado);
        Assert.Equal(["infNFe", "infNFeSupl", "Signature"], nota.Elements().Select(e => e.Name.LocalName));
    }

    [Fact]
    public void AAssinaturaNaoMudaComOGrupo()
    {
        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo());
        Assert.Matches(GrupoSuplementar(), escrita.Xml);
        var reduzida = new NotaEscrita(GrupoSuplementar().Replace(escrita.Xml, ""), escrita.Chave, []);

        var completa = XElement.Parse(NotasTeste.Assinar(escrita));
        var original = XElement.Parse(NotasTeste.Assinar(reduzida));

        Assert.Equal(Digest(original), Digest(completa));
    }

    private static string Digest(XElement nota)
    {
        var assinatura = XNamespace.Get("http://www.w3.org/2000/09/xmldsig#");
        return nota.Element(assinatura + "Signature")!.Element(assinatura + "SignedInfo")!.Element(assinatura + "Reference")!
            .Element(assinatura + "DigestValue")!.Value;
    }

    [Fact]
    public void ANFeComODanfeSimplificadoTipo2LevaOQrCodeDaVersao3()
    {
        var nota = NFeSimplificada();

        var escrita = NotasTeste.Escrever(nota, Nucleo(Documento.NFe));

        Assert.Empty(escrita.Problemas);
        Assert.Equal($"{UrlMGQrCode}?p={escrita.Chave}|3|2", Grupo(escrita).QrCode);
        Assert.Empty(NotasTeste.Validar(escrita));
    }

    [Fact]
    public void ANFeSimplificadaComOCscNoNucleoSaiNaVersao3EAVersao2EDaRegra672()
    {
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(1, CodigoCsc));

        var escrita = NotasTeste.Escrever(NFeSimplificada(), Nucleo(Documento.NFe, configuracao: configuracao));

        Assert.Empty(escrita.Problemas);
        Assert.EndsWith("|3|2", Grupo(escrita).QrCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ANFeSemOTpImp6NaoLevaOGrupo()
    {
        var escrita = NotasTeste.Escrever(NotasTeste.NFe(), Nucleo(Documento.NFe));

        Assert.Empty(escrita.Problemas);
        Assert.DoesNotContain("infNFeSupl", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void ANFeSemOTpImp6ComOGrupoDeQuemUsaEscreveComoEstaEARegra393AcusaDepois()
    {
        var nota = NotasTeste.NFe();
        nota.Suplementares = new InformacoesSuplementares { QrCode = "https://qr?p=1", UrlChave = "https://consulta" };

        var escrita = NotasTeste.Escrever(nota, Nucleo(Documento.NFe));

        Assert.Empty(escrita.Problemas);
        Assert.Contains("<infNFeSupl><qrCode>https://qr?p=1</qrCode><urlChave>https://consulta</urlChave></infNFeSupl>", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OSuplementaresIgualAoGeradoPassa()
    {
        var gerado = Grupo(NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo()));
        var nota = NotasTeste.NFCe();
        nota.Suplementares = new InformacoesSuplementares { QrCode = gerado.QrCode, UrlChave = gerado.UrlChave };

        var escrita = NotasTeste.Escrever(nota, Nucleo());

        Assert.Empty(escrita.Problemas);
        Assert.Equal(gerado, Grupo(escrita));
    }

    [Theory]
    [InlineData("QrCode", "qrCode")]
    [InlineData("UrlChave", "urlChave")]
    public void OSuplementaresDiferenteDoGeradoEErroNFENucleoESaiOGerado(string campo, string tag)
    {
        var gerado = Grupo(NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo()));
        var nota = NotasTeste.NFCe();
        nota.Suplementares = new InformacoesSuplementares { QrCode = gerado.QrCode, UrlChave = gerado.UrlChave };
        if (campo == "QrCode")
            nota.Suplementares.QrCode += "0";
        else
            nota.Suplementares.UrlChave += "/outra";

        var escrita = NotasTeste.Escrever(nota, Nucleo());

        var problema = Assert.Single(escrita.Problemas);
        Assert.Equal(("NFE-NUCLEO", Severidade.Erro), (problema.Codigo, problema.Severidade));
        Assert.Equal(tag, problema.Tag);
        Assert.Equal("Suplementares." + campo, problema.Propriedade);
        Assert.Equal("NFe/infNFeSupl/" + tag, problema.Caminho);
        Assert.Equal(gerado, Grupo(escrita));
    }

    [Fact]
    public void AUFSemUrlNaTabelaESemATrocaEErroNFEQrCode()
    {
        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(uf: UF.SP));

        var problema = Assert.Single(escrita.Problemas);
        Assert.Equal(("NFE-QRCODE", Severidade.Erro), (problema.Codigo, problema.Severidade));
        Assert.Contains("SP", problema.Mensagem, StringComparison.Ordinal);
        Assert.Contains("UrlQrCode", problema.Mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("infNFeSupl", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void AUFSemUrlComAsDuasTrocadasSai()
    {
        var configuracao = new ConfiguracaoNFCe
        {
            UrlQrCode = new Uri("https://sp.exemplo.gov.br/nfce/qrcode"),
            UrlChave = new Uri("https://sp.exemplo.gov.br/nfce/consulta"),
        };

        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(uf: UF.SP, configuracao: configuracao));

        Assert.Empty(escrita.Problemas);
        Assert.StartsWith("https://sp.exemplo.gov.br/nfce/qrcode?p=", Grupo(escrita).QrCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AUFSemUrlComSoUmaTrocadaPedeAOutra()
    {
        var configuracao = new ConfiguracaoNFCe { UrlQrCode = new Uri("https://sp.exemplo.gov.br/nfce/qrcode") };

        var escrita = NotasTeste.Escrever(NotasTeste.NFCe(), Nucleo(uf: UF.SP, configuracao: configuracao));

        var problema = Assert.Single(escrita.Problemas);
        Assert.Equal("NFE-QRCODE", problema.Codigo);
        Assert.Contains("UrlChave", problema.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public void ANFeSimplificadaNaUFSemUrlTambemEErro()
    {
        var escrita = NotasTeste.Escrever(NFeSimplificada(), Nucleo(Documento.NFe, uf: UF.SP));

        Assert.Equal("NFE-QRCODE", Assert.Single(escrita.Problemas).Codigo);
    }

    [Fact]
    public void OCscNaoApareceNaMensagemDeNenhumProblema()
    {
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(1, CodigoCsc));
        var nota = NotasTeste.NFCe();
        nota.Suplementares = new InformacoesSuplementares { QrCode = "https://qr?p=1" };

        var escrita = NotasTeste.Escrever(nota, Nucleo(configuracao: configuracao));

        Assert.NotEmpty(escrita.Problemas);
        Assert.All(escrita.Problemas, p => Assert.DoesNotContain(CodigoCsc, p.ToString(), StringComparison.Ordinal));
        Assert.DoesNotContain(CodigoCsc, escrita.Xml, StringComparison.Ordinal);
    }
}
