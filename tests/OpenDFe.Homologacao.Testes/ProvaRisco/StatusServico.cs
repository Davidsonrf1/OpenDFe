using System.Xml.Linq;

namespace OpenDFe.Testes;

// O NFeStatusServico4: o pedido consStatServ e a leitura do retConsStatServ (MOC 7.0, Visão Geral, 5.6).
internal static class StatusServico
{
    public const string NamespaceNFe = "http://www.portalfiscal.inf.br/nfe";
    public const string NamespaceWsdl = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4";
    public const string Acao = NamespaceWsdl + "/nfeStatusServicoNF";

    public static string Pedido(UF uf) =>
        $"<consStatServ xmlns=\"{NamespaceNFe}\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>{uf.Codigo()}</cUF><xServ>STATUS</xServ></consStatServ>";

    public static RetornoStatus Ler(XElement retorno)
    {
        if (retorno.Name != XName.Get("retConsStatServ", NamespaceNFe))
            throw new FormatException($"Esperado o retConsStatServ; veio o {retorno.Name.LocalName}.");
        string? Campo(string nome) => retorno.Element(XName.Get(nome, NamespaceNFe))?.Value;
        return new RetornoStatus(
            Campo("tpAmb"),
            Campo("verAplic"),
            Campo("cStat") ?? throw new FormatException("O retConsStatServ não tem o cStat."),
            Campo("xMotivo"),
            Campo("cUF"),
            Campo("dhRecbto"),
            Campo("tMed"));
    }
}

internal sealed record RetornoStatus(
    string? TipoAmbiente, string? VersaoAplicativo, string Situacao, string? Motivo, string? CodigoUF, string? Recebimento, string? TempoMedio);
