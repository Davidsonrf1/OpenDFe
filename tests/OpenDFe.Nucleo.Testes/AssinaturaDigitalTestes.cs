using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace OpenDFe.Testes;

// Todos com o certificado autoassinado gerado na hora (CertificadoDeTeste): não há PFX real no repositório.
// Os XMLs são mínimos, só com a forma que a assinatura usa; não são notas válidas no XSD.
public partial class AssinaturaDigitalTestes
{
    private const string CnpjEmitente = "11222333000181";
    private const string CnpjFilial = "11222333000262";      // mesma raiz, outro estabelecimento
    private const string CnpjOutro = "45997418000153";
    private const string CnpjAlfanumerico = "12ABC34501DE35";
    private const string CpfEmitente = "52998224725";
    private const string CpfOutro = "11144477735";
    private const string Ds = "http://www.w3.org/2000/09/xmldsig#";

    private static readonly X509Certificate2 Certificado = new CertificadoDeTeste { Cnpj = CnpjEmitente }.Gerar();
    private static readonly X509Certificate2 CertificadoOutro = new CertificadoDeTeste { Cnpj = CnpjOutro }.Gerar();

    private static readonly ChaveAcesso Chave = ChaveAcesso.Montar(UF.MG,
        new DateTimeOffset(2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3)), CnpjEmitente, Documento.NFe, 1, 123, TipoEmissao.Normal, 12345678);

    private static string IdNota => "NFe" + Chave;

    private static string Nota(string id) =>
        $"<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"{id}\" versao=\"4.00\"><ide><cUF>31</cUF><nNF>123</nNF></ide>"
        + $"<emit><CNPJ>{CnpjEmitente}</CNPJ><xNome>CAFÉ &amp; AÇÚCAR LTDA</xNome></emit><total><vNF>10.50</vNF></total></infNFe></NFe>";

    private static string NotaAssinada() => AssinaturaDigital.Assinar(Nota(IdNota), IdNota, Certificado);

    [GeneratedRegex("<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">.*?</Signature>", RegexOptions.Singleline)]
    private static partial Regex ExpressaoAssinatura();

    private static XmlDocument Ler(string xml)
    {
        var documento = new XmlDocument { PreserveWhitespace = true };
        documento.LoadXml(xml);
        return documento;
    }

    private static XmlNamespaceManager Nomes(XmlDocument documento)
    {
        var nomes = new XmlNamespaceManager(documento.NameTable);
        nomes.AddNamespace("ds", Ds);
        nomes.AddNamespace("n", "http://www.portalfiscal.inf.br/nfe");
        return nomes;
    }

    private static string Codigo(IReadOnlyList<Problema> problemas) => Assert.Single(problemas).Codigo;

    // Assinatura

    [Fact]
    public void AssinaEVerificaANota()
    {
        var assinado = NotaAssinada();

        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente));
    }

    [Fact]
    public void AAssinaturaSegueOPadraoDoMoc()
    {
        var documento = Ler(NotaAssinada());
        var nomes = Nomes(documento);

        var assinatura = (XmlElement)documento.SelectSingleNode("/n:NFe/ds:Signature", nomes)!;
        Assert.Same(assinatura, documento.DocumentElement!.LastChild);
        Assert.Equal("Signature", assinatura.Name);   // sem prefixo, com o namespace declarado nela
        Assert.Equal(Ds, assinatura.GetAttribute("xmlns"));
        Assert.Equal(["SignedInfo", "SignatureValue", "KeyInfo"], assinatura.ChildNodes.Cast<XmlNode>().Select(n => n.LocalName));
        Assert.Equal("http://www.w3.org/TR/2001/REC-xml-c14n-20010315",
            Atributo(documento, "//ds:SignedInfo/ds:CanonicalizationMethod/@Algorithm"));
        Assert.Equal("http://www.w3.org/2000/09/xmldsig#rsa-sha1", Atributo(documento, "//ds:SignatureMethod/@Algorithm"));
        Assert.Equal("#" + IdNota, Atributo(documento, "//ds:Reference/@URI"));
        Assert.Equal(["http://www.w3.org/2000/09/xmldsig#enveloped-signature", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315"],
            documento.SelectNodes("//ds:Transform/@Algorithm", nomes)!.Cast<XmlNode>().Select(n => n.Value));
        Assert.Equal("http://www.w3.org/2000/09/xmldsig#sha1", Atributo(documento, "//ds:DigestMethod/@Algorithm"));
        Assert.Single(documento.SelectNodes("//ds:Reference", nomes)!);
        var certificados = documento.SelectNodes("//ds:KeyInfo/ds:X509Data/ds:X509Certificate", nomes)!;
        Assert.Equal(Convert.ToBase64String(Certificado.RawData), Assert.Single(certificados.Cast<XmlNode>()).InnerText);
        Assert.Single(documento.SelectSingleNode("//ds:KeyInfo", nomes)!.ChildNodes);
    }

    private static string Atributo(XmlDocument documento, string caminho) =>
        documento.SelectSingleNode(caminho, Nomes(documento))!.Value!;

    [Fact]
    public void OTextoRecebidoFicaComoVeioComAAssinaturaInserida()
    {
        // Declaração, aspas simples, quebras de linha e recuo fora e dentro do elemento assinado, entidades.
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<NFe xmlns='http://www.portalfiscal.inf.br/nfe'>\r\n  "
            + $"<infNFe Id='{IdNota}' versao='4.00'>\n    <emit><xNome>A &amp; B &gt; C — São João</xNome></emit>\n  </infNFe>\n</NFe>\n";

        var assinado = AssinaturaDigital.Assinar(xml, IdNota, Certificado);

        var assinatura = ExpressaoAssinatura().Match(assinado);
        Assert.True(assinatura.Success);
        Assert.Equal(xml, assinado.Remove(assinatura.Index, assinatura.Length));
        Assert.EndsWith("</Signature></NFe>\n", assinado, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente));
    }

    [Fact]
    public void OBomNoTextoNaoEXml()
    {
        // O BOM é da codificação dos bytes, não do texto: Encoding.UTF8.GetString o mantém, StreamReader o tira.
        var xml = "﻿" + Nota(IdNota);

        Assert.ThrowsAny<XmlException>(() => AssinaturaDigital.Assinar(xml, IdNota, Certificado));
        using var leitor = new StreamReader(new MemoryStream(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(NotaAssinada())).ToArray()));
        Assert.Empty(AssinaturaDigital.Verificar(leitor.ReadToEnd(), IdNota, CnpjEmitente));
    }

    [Fact]
    public void IdaEVoltaPelosBytesUtf8()
    {
        var bytes = Encoding.UTF8.GetBytes(NotaAssinada());
        var arquivo = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(arquivo, bytes);
            var lido = Encoding.UTF8.GetString(File.ReadAllBytes(arquivo));

            Assert.Empty(AssinaturaDigital.Verificar(lido, IdNota, CnpjEmitente));
        }
        finally
        {
            File.Delete(arquivo);
        }
    }

    [Fact]
    public void NaNfceAAssinaturaVemDepoisDoInfNFeSupl()
    {
        var xml = Nota(IdNota).Replace("</infNFe>", "</infNFe><infNFeSupl><qrCode>x</qrCode></infNFeSupl>", StringComparison.Ordinal);

        var assinado = AssinaturaDigital.Assinar(xml, IdNota, Certificado);

        var documento = Ler(assinado);
        Assert.Equal(["infNFe", "infNFeSupl", "Signature"], documento.DocumentElement!.ChildNodes.Cast<XmlNode>().Select(n => n.LocalName));
        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente));
    }

    [Fact]
    public void AssinaCadaNotaDoLote()
    {
        var outroId = "NFe" + ChaveAcesso.Montar(UF.MG, new DateTimeOffset(2026, 10, 19, 10, 0, 0, TimeSpan.FromHours(-3)),
            CnpjEmitente, Documento.NFe, 1, 124, TipoEmissao.Normal, 87654321);
        var lote = $"<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><idLote>1</idLote>{Nota(IdNota)}{Nota(outroId)}</enviNFe>";

        var assinado = AssinaturaDigital.Assinar(AssinaturaDigital.Assinar(lote, IdNota, Certificado), outroId, Certificado);

        Assert.Equal(2, Ler(assinado).SelectNodes("/n:enviNFe/n:NFe/ds:Signature", Nomes(Ler(assinado)))!.Count);
        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente));
        Assert.Empty(AssinaturaDigital.Verificar(assinado, outroId, CnpjEmitente));
    }

    [Fact]
    public void AssinaOEvento()
    {
        const string Id = "ID1101113126101122233300018155001000000123112345678901";
        var xml = "<envEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\"><idLote>1</idLote>"
            + $"<evento versao=\"1.00\"><infEvento Id=\"{Id}\"><CNPJ>{CnpjEmitente}</CNPJ><tpEvento>110111</tpEvento>"
            + "<detEvento versao=\"1.00\"><descEvento>Cancelamento</descEvento></detEvento></infEvento></evento></envEvento>";

        var assinado = AssinaturaDigital.Assinar(xml, Id, Certificado);

        var documento = Ler(assinado);
        Assert.NotNull(documento.SelectSingleNode("/n:envEvento/n:evento/ds:Signature", Nomes(documento)));
        Assert.Empty(AssinaturaDigital.Verificar(assinado, Id, CnpjEmitente));
    }

    [Fact]
    public void AssinaAInutilizacao()
    {
        const string Id = "ID31261122233300018155001000000100000000110";
        var xml = $"<inutNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><infInut Id=\"{Id}\"><tpAmb>2</tpAmb>"
            + $"<CNPJ>{CnpjEmitente}</CNPJ><xJust>Falha na numeracao do sistema</xJust></infInut></inutNFe>";

        var assinado = AssinaturaDigital.Assinar(xml, Id, Certificado);

        Assert.EndsWith("</infInut><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">", assinado[..(assinado.IndexOf("<SignedInfo", StringComparison.Ordinal))], StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(assinado, Id, CnpjEmitente));
    }

    [Fact]
    public void AssinaComECpf()
    {
        using var certificado = new CertificadoDeTeste { Cpf = CpfEmitente, Nome = "PRODUTOR DE TESTE" }.Gerar();

        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, certificado);

        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CpfEmitente));
    }

    [Fact]
    public void AssinaComOCertificadoAbertoDoPfx()
    {
        var pfx = new CertificadoDeTeste { Cnpj = CnpjEmitente }.GerarPfx("senha");
        using var certificado = CertificadoDigital.AbrirPfx(pfx, "senha");

        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, certificado);

        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente));
    }

    // Recusas da assinatura

    [Fact]
    public void RecusaOIdAusente()
    {
        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Assinar(Nota(IdNota), "NFe000", Certificado));
    }

    [Fact]
    public void RecusaOIdRepetido()
    {
        var xml = $"<lote>{Nota(IdNota)}{Nota(IdNota)}</lote>";

        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Assinar(xml, IdNota, Certificado));
    }

    [Fact]
    public void RecusaOIdRepetidoEmOutraGrafia()
    {
        var xml = Nota(IdNota).Replace("<ide>", $"<ide id=\"{IdNota}\">", StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Assinar(xml, IdNota, Certificado));
    }

    [Fact]
    public void RecusaOElementoRaiz()
    {
        var xml = $"<infNFe Id=\"{IdNota}\"><ide/></infNFe>";

        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Assinar(xml, IdNota, Certificado));
    }

    [Fact]
    public void RecusaOXmlJaAssinado()
    {
        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Assinar(NotaAssinada(), IdNota, Certificado));
    }

    [Fact]
    public void RecusaOCertificadoSemChavePrivada()
    {
        using var publico = X509CertificateLoader.LoadCertificate(Certificado.RawData);

        Assert.Throws<CryptographicException>(() => AssinaturaDigital.Assinar(Nota(IdNota), IdNota, publico));
    }

    [Fact]
    public void RecusaAChaveEcdsa()
    {
        using var certificado = new CertificadoDeTeste { Cnpj = CnpjEmitente, ChaveEcdsa = true }.Gerar();

        Assert.Throws<CryptographicException>(() => AssinaturaDigital.Assinar(Nota(IdNota), IdNota, certificado));
    }

    [Fact]
    public void RecusaOXmlMalformadoEADtd()
    {
        Assert.ThrowsAny<XmlException>(() => AssinaturaDigital.Assinar("<NFe><infNFe Id=\"a\"></NFe>", "a", Certificado));
        var comDtd = $"<!DOCTYPE NFe [<!ENTITY x \"y\">]>{Nota(IdNota)}";
        Assert.ThrowsAny<XmlException>(() => AssinaturaDigital.Assinar(comDtd, IdNota, Certificado));
        Assert.ThrowsAny<XmlException>(() => AssinaturaDigital.Verificar(comDtd, IdNota, CnpjEmitente));
    }

    [Fact]
    public void RecusaArgumentosNulosOuVazios()
    {
        Assert.Throws<ArgumentNullException>(() => AssinaturaDigital.Assinar(null!, IdNota, Certificado));
        Assert.Throws<ArgumentNullException>(() => AssinaturaDigital.Assinar(Nota(IdNota), null!, Certificado));
        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Assinar(Nota(IdNota), "", Certificado));
        Assert.Throws<ArgumentNullException>(() => AssinaturaDigital.Assinar(Nota(IdNota), IdNota, null!));
        Assert.Throws<ArgumentNullException>(() => AssinaturaDigital.Verificar(null!, IdNota, CnpjEmitente));
        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Verificar(Nota(IdNota), "", CnpjEmitente));
        Assert.Throws<ArgumentNullException>(() => AssinaturaDigital.Verificar(Nota(IdNota), IdNota, null!));
    }

    [Theory]
    [InlineData("11222333000182")]
    [InlineData("5299822472")]
    [InlineData("")]
    public void RecusaODocumentoEsperadoInvalido(string documento)
    {
        Assert.Throws<ArgumentException>(() => AssinaturaDigital.Verificar(NotaAssinada(), IdNota, documento));
    }

    // XML alterado depois de assinado

    [Theory]
    [InlineData("<vNF>10.50</vNF>", "<vNF>10.51</vNF>")]              // valor
    [InlineData("versao=\"4.00\"", "versao=\"4.01\"")]                // atributo
    [InlineData("<ide>", "<ide> ")]                                   // espaço em branco dentro do elemento assinado
    [InlineData("<total>", "\n<total>")]                              // quebra de linha dentro do elemento assinado
    [InlineData("CAFÉ &amp; AÇÚCAR", "CAFE &amp; ACUCAR")]           // acentos
    public void RecusaOElementoAlteradoDepoisDeAssinado(string antes, string depois)
    {
        var alterado = NotaAssinada().Replace(antes, depois, StringComparison.Ordinal);

        Assert.Equal("297", Codigo(AssinaturaDigital.Verificar(alterado, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void AceitaOEspacoForaDoElementoAssinado()
    {
        var assinado = NotaAssinada().Replace("</infNFe><Signature", "</infNFe>\n  <Signature", StringComparison.Ordinal);

        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente));
    }

    [Fact]
    public void RecusaODigestEOValorAlterados()
    {
        var documento = Ler(NotaAssinada());
        var resumo = documento.SelectSingleNode("//ds:DigestValue", Nomes(documento))!;
        resumo.InnerText = Convert.ToBase64String(new byte[20]);
        Assert.Equal("297", Codigo(AssinaturaDigital.Verificar(documento.OuterXml, IdNota, CnpjEmitente)));

        documento = Ler(NotaAssinada());
        var valor = documento.SelectSingleNode("//ds:SignatureValue", Nomes(documento))!;
        var bytes = Convert.FromBase64String(valor.InnerText);
        bytes[10] ^= 1;
        valor.InnerText = Convert.ToBase64String(bytes);
        Assert.Equal("297", Codigo(AssinaturaDigital.Verificar(documento.OuterXml, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void RecusaOCertificadoTrocadoNoKeyInfo()
    {
        var documento = Ler(NotaAssinada());
        documento.SelectSingleNode("//ds:X509Certificate", Nomes(documento))!.InnerText =
            Convert.ToBase64String(new CertificadoDeTeste { Cnpj = CnpjEmitente }.Gerar().RawData);

        Assert.Equal("297", Codigo(AssinaturaDigital.Verificar(documento.OuterXml, IdNota, CnpjEmitente)));
    }

    // Signature wrapping e referência

    [Fact]
    public void RecusaAReferenciaParaOutroElemento()
    {
        // O atacante guarda o infNFe assinado com outro Id e põe um infNFe alterado com o Id esperado.
        var assinado = NotaAssinada();
        var alterado = assinado.Replace($"Id=\"{IdNota}\"", "Id=\"original\"", StringComparison.Ordinal)
            .Replace($"URI=\"#{IdNota}\"", "URI=\"#original\"", StringComparison.Ordinal)
            .Replace("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">",
                $"<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"{IdNota}\"><total><vNF>99.00</vNF></total></infNFe>",
                StringComparison.Ordinal);

        var problemas = AssinaturaDigital.Verificar(alterado, IdNota, CnpjEmitente);

        Assert.Equal("298", Codigo(problemas));
        Assert.Equal("Reference", problemas[0].Tag);
    }

    [Fact]
    public void RecusaOIdRepetidoNaVerificacao()
    {
        // O infNFe assinado e uma cópia alterada com o mesmo Id, escondida em outro lugar.
        var alterado = NotaAssinada().Replace("<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\">",
            $"<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"{IdNota}\"><total><vNF>99.00</vNF></total></infNFe>",
            StringComparison.Ordinal);

        var problemas = AssinaturaDigital.Verificar($"<x>{alterado}</x>", IdNota, CnpjEmitente);

        Assert.Equal("298", Codigo(problemas));
        Assert.Equal("Id", problemas[0].Tag);
    }

    [Fact]
    public void RecusaDuasReferencias()
    {
        var documento = Ler(NotaAssinada());
        var referencia = documento.SelectSingleNode("//ds:Reference", Nomes(documento))!;
        referencia.ParentNode!.AppendChild(referencia.Clone());

        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(documento.OuterXml, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void RecusaAAssinaturaForaDoLugar()
    {
        // A Signature dentro do infNFe, e não ao lado dele.
        var assinado = NotaAssinada();
        var assinatura = ExpressaoAssinatura().Match(assinado).Value;
        var movido = assinado.Replace(assinatura, "", StringComparison.Ordinal).Replace("</infNFe>", assinatura + "</infNFe>", StringComparison.Ordinal);

        var problemas = AssinaturaDigital.Verificar(movido, IdNota, CnpjEmitente);

        Assert.Equal("298", Codigo(problemas));
        Assert.Equal("Signature", problemas[0].Tag);
    }

    [Fact]
    public void RecusaONaoAssinadoEADuasAssinaturas()
    {
        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(Nota(IdNota), IdNota, CnpjEmitente)));

        var assinado = NotaAssinada();
        var assinatura = ExpressaoAssinatura().Match(assinado).Value;
        var duas = assinado.Replace("</NFe>", assinatura + "</NFe>", StringComparison.Ordinal);
        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(duas, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void RecusaOIdAusenteNaVerificacao()
    {
        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(NotaAssinada(), "NFe000", CnpjEmitente)));
    }

    [Fact]
    public void VerificaANotaDentroDoNfeProcIgnorandoAAssinaturaDoProtocolo()
    {
        var assinado = NotaAssinada();
        var assinaturaQualquer = ExpressaoAssinatura().Match(AssinaturaDigital.Assinar(Nota("NFeOutra"), "NFeOutra", CertificadoOutro)).Value;
        var proc = "<nfeProc xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\">" + assinado
            + $"<protNFe versao=\"4.00\"><infProt Id=\"ID131260000000001\"><chNFe>{Chave}</chNFe></infProt>{assinaturaQualquer}</protNFe></nfeProc>";

        Assert.Empty(AssinaturaDigital.Verificar(proc, IdNota, CnpjEmitente));
    }

    // Algoritmos fora do padrão

    [Theory]
    [InlineData("http://www.w3.org/2000/09/xmldsig#rsa-sha1", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", "SignatureMethod")]
    [InlineData("<CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"",
        "<CanonicalizationMethod Algorithm=\"http://www.w3.org/2001/10/xml-exc-c14n#\"", "CanonicalizationMethod")]
    [InlineData("http://www.w3.org/2000/09/xmldsig#sha1\"", "http://www.w3.org/2001/04/xmlenc#sha256\"", "DigestMethod")]
    [InlineData("<Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />", "", "Transforms")]
    [InlineData("<Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\" />",
        "<Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" />", "Transforms")]
    public void RecusaOsAlgoritmosForaDoPadrao(string antes, string depois, string tag)
    {
        var assinado = NotaAssinada();
        Assert.Contains(antes, assinado, StringComparison.Ordinal);

        var problemas = AssinaturaDigital.Verificar(assinado.Replace(antes, depois, StringComparison.Ordinal), IdNota, CnpjEmitente);

        Assert.Equal("298", Codigo(problemas));
        Assert.Equal(tag, problemas[0].Tag);
    }

    [Fact]
    public void RecusaOKeyInfoForaDoPadrao()
    {
        var assinado = NotaAssinada();
        var certificado = Regex.Match(assinado, "<X509Certificate>.*?</X509Certificate>").Value;

        var dois = assinado.Replace(certificado, certificado + certificado, StringComparison.Ordinal);
        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(dois, IdNota, CnpjEmitente)));

        var semCertificado = assinado.Replace(certificado, "", StringComparison.Ordinal);
        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(semCertificado, IdNota, CnpjEmitente)));

        var comChave = assinado.Replace("</X509Data>", "</X509Data><KeyName>x</KeyName>", StringComparison.Ordinal);
        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(comChave, IdNota, CnpjEmitente)));

        var comObjeto = assinado.Replace("</KeyInfo>", "</KeyInfo><Object>x</Object>", StringComparison.Ordinal);
        Assert.Equal("298", Codigo(AssinaturaDigital.Verificar(comObjeto, IdNota, CnpjEmitente)));
    }

    // Certificado

    [Fact]
    public void RecusaOCertificadoIlegivel()
    {
        var assinado = Regex.Replace(NotaAssinada(), "<X509Certificate>.*?</X509Certificate>", "<X509Certificate>AAAA</X509Certificate>");

        Assert.Equal("290", Codigo(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void RecusaOCertificadoSemNaoRepudio()
    {
        using var certificado = new CertificadoDeTeste { Cnpj = CnpjEmitente, UsoDaChave = X509KeyUsageFlags.DigitalSignature }.Gerar();
        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, certificado);

        Assert.Equal("290", Codigo(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void RecusaOCertificadoDeOutroCnpj()
    {
        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, CertificadoOutro);

        var problemas = AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente);

        Assert.Equal("213", Codigo(problemas));
        Assert.Equal(Severidade.Erro, problemas[0].Severidade);
    }

    [Fact]
    public void AceitaOCertificadoDeOutroEstabelecimentoDaMesmaEmpresa()
    {
        Assert.Empty(AssinaturaDigital.Verificar(NotaAssinada(), IdNota, CnpjFilial));
        Assert.Empty(AssinaturaDigital.Verificar(NotaAssinada(), IdNota, Cnpj.Formatar(CnpjEmitente)));
    }

    [Fact]
    public void ConfereOCnpjAlfanumerico()
    {
        using var certificado = new CertificadoDeTeste { Cnpj = CnpjAlfanumerico }.Gerar();
        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, certificado);

        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, CnpjAlfanumerico));
        Assert.Equal("213", Codigo(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void ConfereOCpf()
    {
        using var certificado = new CertificadoDeTeste { Cpf = CpfEmitente }.Gerar();
        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, certificado);

        Assert.Empty(AssinaturaDigital.Verificar(assinado, IdNota, Cpf.Formatar(CpfEmitente)));
        Assert.Equal("227", Codigo(AssinaturaDigital.Verificar(assinado, IdNota, CpfOutro)));
        Assert.Equal("213", Codigo(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente)));
        Assert.Equal("227", Codigo(AssinaturaDigital.Verificar(NotaAssinada(), IdNota, CpfEmitente)));
    }

    [Fact]
    public void RecusaOCertificadoSemTitular()
    {
        using var certificado = new CertificadoDeTeste { Cnpj = CnpjEmitente, ComOtherName = false, DocumentoNoNomeComum = false }.Gerar();
        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, certificado);

        Assert.Equal("292", Codigo(AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente)));
    }

    [Fact]
    public void JuntaOsProblemasDoValorEDoTitular()
    {
        var assinado = AssinaturaDigital.Assinar(Nota(IdNota), IdNota, CertificadoOutro).Replace("10.50", "10.60", StringComparison.Ordinal);

        var problemas = AssinaturaDigital.Verificar(assinado, IdNota, CnpjEmitente);

        Assert.Equal(["297", "213"], problemas.Select(p => p.Codigo));
    }
}
