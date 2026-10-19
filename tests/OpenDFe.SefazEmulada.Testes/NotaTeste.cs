using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace OpenDFe.Testes;

// Uma NF-e (ou NFC-e) mínima que passa no XSD do pacote de liberação, escrita em texto e assinada com o
// SignedXml do .NET no padrão da SEFAZ (MOC 7.0, Visão Geral, 4.2.4), sem a lib: a verificação do emulador
// (que usa a da lib) é conferida contra uma assinatura independente. Cada nota sai com um número novo, para
// a duplicidade não cruzar os testes que dividem o emulador.
internal sealed record NotaTeste
{
    private static int _ultimoNumero;

    public string Uf { get; init; } = "31";

    public string? Cnpj { get; init; } = "11222333000181";

    public string? Cpf { get; init; }

    public string Modelo { get; init; } = "55";

    public string Serie { get; init; } = "1";

    public int Numero { get; init; } = Interlocked.Increment(ref _ultimoNumero);

    public string TipoEmissao { get; init; } = "1";

    public string Codigo { get; init; } = "10293847";

    public DateTimeOffset Emissao { get; init; } = Agora();

    public string Ambiente { get; init; } = "2";

    public string? Observacao { get; init; }

    // O Id trocado (502) e o DV trocado na chave e no cDV (253).
    public string? Id { get; init; }

    public int? Digito { get; init; }

    // Quebra de linha e recuo entre as tags, assinados como vieram.
    public bool Recuar { get; init; }

    public X509Certificate2 Certificado { get; init; } = Clientes.CertificadoCliente;

    // O horário de Brasília, sem os milissegundos (o dhEmi não os tem).
    public static DateTimeOffset Agora()
    {
        var agora = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3));
        return agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerSecond));
    }

    public string Chave
    {
        get
        {
            var emitente = (Cnpj ?? Cpf!).PadLeft(14, '0');
            var parte = Uf + Emissao.ToString("yyMM", CultureInfo.InvariantCulture) + emitente + Modelo
                + Serie.PadLeft(3, '0') + Numero.ToString("D9", CultureInfo.InvariantCulture) + TipoEmissao + Codigo;
            return parte + (Digito ?? Modulo11(parte));
        }
    }

    // O XML assinado, como sai do emitente.
    public string Gerar()
    {
        var chave = Chave;
        var id = Id ?? "NFe" + chave;
        var emitente = Cnpj is not null ? $"<CNPJ>{Cnpj}</CNPJ>" : $"<CPF>{Cpf}</CPF>";
        var informacoes = Observacao is null ? "" : $"<infAdic><infCpl>{Observacao}</infCpl></infAdic>";
        string[] partes =
        [
            $"<NFe xmlns=\"{Mensagens.NamespaceNFe}\">",
            $"<infNFe versao=\"4.00\" Id=\"{id}\">",
            $"<ide><cUF>{Uf}</cUF><cNF>{Codigo}</cNF><natOp>VENDA</natOp><mod>{Modelo}</mod><serie>{Serie}</serie><nNF>{Numero}</nNF>"
                + $"<dhEmi>{Emissao.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)}</dhEmi><tpNF>1</tpNF><idDest>1</idDest>"
                + $"<cMunFG>3106200</cMunFG><tpImp>{(Modelo == "65" ? "4" : "1")}</tpImp><tpEmis>{TipoEmissao}</tpEmis><cDV>{chave[43]}</cDV>"
                + $"<tpAmb>{Ambiente}</tpAmb><finNFe>1</finNFe><indFinal>1</indFinal><indPres>1</indPres><procEmi>0</procEmi><verProc>TESTE 1.0</verProc></ide>",
            $"<emit>{emitente}<xNome>EMPRESA DE TESTE LTDA</xNome><enderEmit><xLgr>RUA DE TESTE</xLgr><nro>1</nro><xBairro>CENTRO</xBairro>"
                + "<cMun>3106200</cMun><xMun>BELO HORIZONTE</xMun><UF>MG</UF><CEP>30110000</CEP></enderEmit><IE>0623079040081</IE><CRT>3</CRT></emit>",
            "<det nItem=\"1\"><prod><cProd>1</cProd><cEAN>SEM GTIN</cEAN><xProd>PRODUTO DE TESTE</xProd><NCM>61091000</NCM><CFOP>5102</CFOP>"
                + "<uCom>UN</uCom><qCom>1.0000</qCom><vUnCom>10.00</vUnCom><vProd>10.00</vProd><cEANTrib>SEM GTIN</cEANTrib><uTrib>UN</uTrib>"
                + "<qTrib>1.0000</qTrib><vUnTrib>10.00</vUnTrib><indTot>1</indTot></prod><imposto><ICMS><ICMS00><orig>0</orig><CST>00</CST>"
                + "<modBC>3</modBC><vBC>10.00</vBC><pICMS>18.00</pICMS><vICMS>1.80</vICMS></ICMS00></ICMS><PIS><PISNT><CST>07</CST></PISNT></PIS>"
                + "<COFINS><COFINSNT><CST>07</CST></COFINSNT></COFINS></imposto></det>",
            "<total><ICMSTot><vBC>10.00</vBC><vICMS>1.80</vICMS><vICMSDeson>0.00</vICMSDeson><vFCP>0.00</vFCP><vBCST>0.00</vBCST><vST>0.00</vST>"
                + "<vFCPST>0.00</vFCPST><vFCPSTRet>0.00</vFCPSTRet><vProd>10.00</vProd><vFrete>0.00</vFrete><vSeg>0.00</vSeg><vDesc>0.00</vDesc>"
                + "<vII>0.00</vII><vIPI>0.00</vIPI><vIPIDevol>0.00</vIPIDevol><vPIS>0.00</vPIS><vCOFINS>0.00</vCOFINS><vOutro>0.00</vOutro>"
                + "<vNF>10.00</vNF></ICMSTot></total>",
            "<transp><modFrete>9</modFrete></transp><pag><detPag><tPag>01</tPag><vPag>10.00</vPag></detPag></pag>" + informacoes,
            "</infNFe>",
            "</NFe>",
        ];
        var xml = string.Join(Recuar ? "\n  " : "", partes);
        return Assinar(xml, id);
    }

    // Enveloped, C14N, RSA-SHA1, a Reference para o #Id e o certificado no KeyInfo; a Signature no fim da NFe.
    private string Assinar(string xml, string id)
    {
        var documento = new XmlDocument { PreserveWhitespace = true };
        documento.LoadXml(xml);
        using var chave = Certificado.GetRSAPrivateKey()!;
        var assinatura = new SignedXml(documento) { SigningKey = chave };
        assinatura.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
        assinatura.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
        var referencia = new Reference("#" + id) { DigestMethod = SignedXml.XmlDsigSHA1Url };
        referencia.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        referencia.AddTransform(new XmlDsigC14NTransform());
        assinatura.AddReference(referencia);
        var informacao = new KeyInfo();
        informacao.AddClause(new KeyInfoX509Data(Certificado));
        assinatura.KeyInfo = informacao;
        assinatura.ComputeSignature();
        documento.DocumentElement!.AppendChild(documento.ImportNode(assinatura.GetXml(), true));
        var texto = new StringBuilder();
        using (var escritor = XmlWriter.Create(texto, new XmlWriterSettings { OmitXmlDeclaration = true }))
            documento.Save(escritor);
        return texto.ToString();
    }

    private static int Modulo11(string parte)
    {
        var soma = 0;
        var peso = 2;
        for (var i = parte.Length - 1; i >= 0; i--)
        {
            soma += (parte[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
