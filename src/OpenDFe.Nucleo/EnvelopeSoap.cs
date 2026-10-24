using System.Xml;
using System.Xml.Linq;

namespace OpenDFe;

/// <summary>
/// O envelope SOAP 1.2 dos autorizadores, escrito e lido à mão, sem WSDL (PROJETO.md 8.2): a mensagem dentro do
/// <c>nfeDadosMsg</c>, no namespace do WSDL do serviço, sem cabeçalho (o <c>nfeCabecMsg</c> saiu no leiaute 4.00); a
/// resposta dentro do <c>nfeResultMsg</c>. No serviço com a operação no <c>Body</c> (<see cref="ServicoSoap.Operacao"/>),
/// o <c>nfeDadosMsg</c> vai dentro do elemento da operação, e a resposta vem em <c>&lt;Operacao&gt;Response/&lt;Operacao&gt;Result</c>.
/// </summary>
internal static class EnvelopeSoap
{
    public const string NamespaceSoap = "http://www.w3.org/2003/05/soap-envelope";

    private static readonly XmlReaderSettings Leitura = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreProcessingInstructions = true,
    };

    /// <summary>O envelope com a mensagem, sem a declaração XML dela.</summary>
    public static string Montar(ServicoSoap servico, string mensagem)
    {
        var dados = $"<nfeDadosMsg xmlns=\"{servico.NamespaceWsdl}\">{TirarDeclaracao(mensagem)}</nfeDadosMsg>";
        // Com a operação, o nfeDadosMsg herda o namespace do elemento dela, que é o mesmo do WSDL.
        var corpo = servico.Operacao is { } operacao
            ? $"<{operacao} xmlns=\"{servico.NamespaceWsdl}\">{dados}</{operacao}>"
            : dados;
        return "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
            + $"<soap12:Envelope xmlns:soap12=\"{NamespaceSoap}\"><soap12:Body>{corpo}</soap12:Body></soap12:Envelope>";
    }

    /// <summary>
    /// A mensagem da resposta: o primeiro elemento dentro do <c>nfeResultMsg</c> (ou, no serviço com
    /// <see cref="ServicoSoap.Operacao"/>, do <c>&lt;Operacao&gt;Result</c>), em qualquer namespace (há autorizador que o
    /// põe noutro) e em qualquer profundidade do <c>Body</c>, como texto.
    /// </summary>
    /// <param name="resposta">O envelope da resposta.</param>
    /// <param name="servico">O serviço, ou nulo para o <c>nfeResultMsg</c> direto no <c>Body</c>.</param>
    /// <exception cref="FalhaSoapException">A resposta é uma falha SOAP.</exception>
    /// <exception cref="XmlException">A resposta não é XML bem formado.</exception>
    /// <exception cref="InvalidDataException">Não é um envelope SOAP 1.2, ou não tem o <c>nfeResultMsg</c> com uma mensagem.</exception>
    public static string Ler(string resposta, ServicoSoap? servico = null)
    {
        XDocument documento;
        using (var texto = new StringReader(resposta.TrimStart('﻿')))
        using (var leitor = XmlReader.Create(texto, Leitura))
            documento = XDocument.Load(leitor, LoadOptions.PreserveWhitespace);

        var raiz = documento.Root!;
        if (raiz.Name != XName.Get("Envelope", NamespaceSoap))
            throw new InvalidDataException($"A resposta não é um envelope SOAP 1.2 (a raiz é {raiz.Name.LocalName}, em '{raiz.Name.NamespaceName}').");
        var corpo = raiz.Element(XName.Get("Body", NamespaceSoap))
            ?? throw new InvalidDataException("O envelope da resposta não tem o Body.");
        if (corpo.Element(XName.Get("Fault", NamespaceSoap)) is { } falha)
            throw LerFalha(falha);
        var nomeResultado = servico?.Operacao is { } operacao ? operacao + "Result" : "nfeResultMsg";
        var resultado = corpo.Descendants().FirstOrDefault(e => e.Name.LocalName == nomeResultado)
            ?? throw new InvalidDataException($"A resposta não tem o {nomeResultado}.");
        var mensagem = resultado.Elements().FirstOrDefault()
            ?? throw new InvalidDataException($"O {nomeResultado} da resposta está vazio.");
        return mensagem.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>Se o texto parece um envelope SOAP 1.2 (para separar a resposta HTTP sem envelope).</summary>
    public static bool PareceEnvelope(string resposta) =>
        resposta.Contains(NamespaceSoap, StringComparison.Ordinal);

    private static FalhaSoapException LerFalha(XElement falha)
    {
        XNamespace soap = NamespaceSoap;
        var codigo = falha.Element(soap + "Code")?.Element(soap + "Value")?.Value.Trim() ?? "";
        var local = codigo[(codigo.IndexOf(':', StringComparison.Ordinal) + 1)..];
        var motivo = falha.Element(soap + "Reason")?.Elements(soap + "Text").FirstOrDefault()?.Value.Trim();
        return new FalhaSoapException(local, string.IsNullOrEmpty(motivo) ? "(sem motivo)" : motivo);
    }

    // A mensagem montada pelo leiaute pode vir com a declaração XML (e o BOM), que não vale dentro do envelope.
    private static string TirarDeclaracao(string mensagem)
    {
        var texto = mensagem.TrimStart('﻿');
        if (!texto.StartsWith("<?xml", StringComparison.Ordinal))
            return texto;
        var fim = texto.IndexOf("?>", StringComparison.Ordinal);
        return fim < 0 ? texto : texto[(fim + 2)..].TrimStart();
    }
}

/// <summary>
/// A falha SOAP 1.2 (<c>Fault</c>) recebida. <see cref="Remetente"/>: o autorizador recusou o pedido sem processá-lo
/// (<c>Sender</c>, <c>VersionMismatch</c>, <c>MustUnderstand</c>, <c>DataEncodingUnknown</c>); o resto
/// (<c>Receiver</c>) pode ter sido processado.
/// </summary>
internal sealed class FalhaSoapException(string codigo, string motivo)
    : Exception($"Falha SOAP {(codigo.Length == 0 ? "sem código" : codigo)}: {motivo}")
{
    public string Codigo { get; } = codigo;

    public string Motivo { get; } = motivo;

    public bool Remetente => Codigo is "Sender" or "VersionMismatch" or "MustUnderstand" or "DataEncodingUnknown";
}
