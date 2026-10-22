using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// O envelope SOAP 1.2, lido e escrito à mão (MOC 7.0, Visão Geral, 4.2.2 e 4.4.1): o pedido traz a área de
// dados dentro do nfeDadosMsg, no namespace do WSDL do serviço, sem o cabeçalho (o nfeCabecMsg saiu no
// leiaute 4.00 e, se vier, é ignorado); a resposta vai dentro do nfeResultMsg. No serviço com a operação no
// Body (o NFeDistribuicaoDFe, NT 2014.002, 5 e 6), o nfeDadosMsg vem dentro dela (nfeDistDFeInteresse), e a
// resposta vai em <operação>Response/<operação>Result.
internal static class EnvelopeSoap
{
    public const string TipoConteudo = "application/soap+xml";

    public static readonly XNamespace NamespaceSoap = "http://www.w3.org/2003/05/soap-envelope";
    private static readonly XNamespace NamespaceSoap11 = "http://schemas.xmlsoap.org/soap/envelope/";

    private static readonly XmlReaderSettings Leitura = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
    };

    // A área de dados do pedido, com o espaço em branco como veio (ele faz parte do que a nota assina).
    // Envelope que não serve vira FalhaSoapException (o cliente errou).
    public static XElement LerDados(byte[] corpo, XNamespace namespaceWsdl, string? operacao = null)
    {
        XDocument documento;
        try
        {
            using var fluxo = new MemoryStream(corpo);
            using var leitor = XmlReader.Create(fluxo, Leitura);
            documento = XDocument.Load(leitor, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException erro)
        {
            throw new FalhaSoapException(CodigoFalha.Remetente, "XML mal formado: " + erro.Message);
        }

        var raiz = documento.Root!;
        if (raiz.Name == NamespaceSoap11 + "Envelope")
            throw new FalhaSoapException(CodigoFalha.VersaoDivergente, "Envelope SOAP 1.1; o serviço é SOAP 1.2.");
        if (raiz.Name != NamespaceSoap + "Envelope")
            throw new FalhaSoapException(CodigoFalha.Remetente, "A mensagem não é um envelope SOAP 1.2.");
        var corpoSoap = raiz.Element(NamespaceSoap + "Body")
            ?? throw new FalhaSoapException(CodigoFalha.Remetente, "O envelope não tem o Body.");
        if (operacao is not null)
        {
            corpoSoap = corpoSoap.Element(namespaceWsdl + operacao)
                ?? throw new FalhaSoapException(CodigoFalha.Remetente, $"O Body não tem o {operacao} no namespace {namespaceWsdl}.");
        }
        var mensagem = corpoSoap.Element(namespaceWsdl + "nfeDadosMsg")
            ?? throw new FalhaSoapException(CodigoFalha.Remetente, $"O {(operacao ?? "Body")} não tem o nfeDadosMsg no namespace {namespaceWsdl}.");
        return mensagem;
    }

    // Se algum elemento dentro do nfeDadosMsg usa prefixo de namespace (Anexo I, D02, 404). Lido de novo com o
    // XmlReader, que mostra o prefixo; chamado depois do LerDados, com o XML já conferido.
    public static bool UsaPrefixo(byte[] corpo, XNamespace namespaceWsdl)
    {
        using var fluxo = new MemoryStream(corpo);
        using var leitor = XmlReader.Create(fluxo, Leitura);
        var profundidade = -1;
        while (leitor.Read())
        {
            if (leitor.NodeType != XmlNodeType.Element)
                continue;
            if (profundidade < 0)
            {
                if (leitor.LocalName == "nfeDadosMsg" && leitor.NamespaceURI == namespaceWsdl.NamespaceName)
                    profundidade = leitor.Depth;
            }
            else if (leitor.Depth <= profundidade)
            {
                return false;
            }
            else if (leitor.Prefix.Length > 0)
            {
                return true;
            }
        }
        return false;
    }

    public static byte[] Resultado(XNamespace namespaceWsdl, string? operacao, XElement retorno) =>
        Escrever(new XElement(NamespaceSoap + "Body", operacao is null
            ? new XElement(namespaceWsdl + "nfeResultMsg", retorno)
            : new XElement(namespaceWsdl + (operacao + "Response"), new XElement(namespaceWsdl + (operacao + "Result"), retorno))));

    // A falha SOAP 1.2 (env:Fault, com o Code e o Reason).
    public static byte[] Falha(CodigoFalha codigo, string motivo) =>
        Escrever(new XElement(NamespaceSoap + "Body",
            new XElement(NamespaceSoap + "Fault",
                new XElement(NamespaceSoap + "Code", new XElement(NamespaceSoap + "Value", "soap:" + Valor(codigo))),
                new XElement(NamespaceSoap + "Reason",
                    new XElement(NamespaceSoap + "Text", new XAttribute(XNamespace.Xml + "lang", "pt-BR"), motivo)))));

    private static string Valor(CodigoFalha codigo) => codigo switch
    {
        CodigoFalha.VersaoDivergente => "VersionMismatch",
        CodigoFalha.Remetente => "Sender",
        _ => "Receiver",
    };

    private static byte[] Escrever(XElement corpo)
    {
        var envelope = new XElement(NamespaceSoap + "Envelope", new XAttribute(XNamespace.Xmlns + "soap", NamespaceSoap.NamespaceName), corpo);
        var configuracao = new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false };
        using var fluxo = new MemoryStream();
        using (var escritor = XmlWriter.Create(fluxo, configuracao))
            new XDocument(new XDeclaration("1.0", "utf-8", null), envelope).Save(escritor);
        return fluxo.ToArray();
    }
}

// O Code da falha SOAP 1.2. A ligação HTTP (SOAP 1.2, parte 2, 7.5.1): Sender é 400; os outros, 500.
internal enum CodigoFalha
{
    VersaoDivergente = 1,
    Remetente = 2,
    Receptor = 3,
}

internal sealed class FalhaSoapException(CodigoFalha codigo, string motivo) : Exception(motivo)
{
    public CodigoFalha Codigo { get; } = codigo;
}
