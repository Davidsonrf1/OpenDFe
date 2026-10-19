using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O envelope SOAP 1.2 mínimo da SEFAZ: a mensagem dentro do nfeDadosMsg, no namespace do WSDL do serviço;
// a resposta dentro do nfeResultMsg. Sem cabeçalho (o nfeCabecMsg saiu no leiaute 4.00).
internal static class EnvelopeSoap
{
    public const string NamespaceSoap = "http://www.w3.org/2003/05/soap-envelope";

    public static string Montar(string namespaceWsdl, string mensagem) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        + $"<soap12:Envelope xmlns:soap12=\"{NamespaceSoap}\"><soap12:Body>"
        + $"<nfeDadosMsg xmlns=\"{namespaceWsdl}\">{mensagem}</nfeDadosMsg>"
        + "</soap12:Body></soap12:Envelope>";

    // O primeiro elemento dentro do nfeResultMsg; a falha SOAP vira exceção, com o motivo.
    public static XElement LerResultado(string resposta)
    {
        var documento = XDocument.Parse(resposta);
        var corpo = documento.Root?.Element(XName.Get("Body", NamespaceSoap))
            ?? throw new XmlException("A resposta não é um envelope SOAP 1.2.");
        var falha = corpo.Element(XName.Get("Fault", NamespaceSoap));
        if (falha is not null)
            throw new InvalidOperationException("Falha SOAP: " + string.Join(" | ", falha.Descendants().Where(e => !e.HasElements).Select(e => e.Value)));
        var resultado = corpo.Elements().FirstOrDefault(e => e.Name.LocalName == "nfeResultMsg")
            ?? throw new XmlException("A resposta não tem o nfeResultMsg.");
        return resultado.Elements().FirstOrDefault()
            ?? throw new XmlException("O nfeResultMsg está vazio.");
    }
}
