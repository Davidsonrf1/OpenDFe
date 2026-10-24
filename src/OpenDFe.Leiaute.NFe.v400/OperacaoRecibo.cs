using System.Globalization;
using System.Text;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// A consulta do recibo (<c>NFeRetAutorizacao4</c>, docs/CONTRATOS.md 14): o <c>consReciNFe</c> e o
/// <c>retConsReciNFe</c> com os protocolos do lote. Vale para a NF-e e para a NFC-e, como a autorização.
/// </summary>
/// <remarks>
/// Com as notas do lote no parâmetro, a consulta vai ao autorizador da chave da primeira (o lote foi para lá) e cada
/// protocolo é ligado à nota, com o <c>nfeProc</c>; sem elas, uma <see cref="ResultadoNota"/> por <c>protNFe</c>, sem
/// o XML. A leitura é branda (<see cref="LeituraRetorno"/>).
/// </remarks>
internal sealed class OperacaoRecibo : IOperacao<ConsultaRecibo, RetornoAutorizacao>
{
    private const string Raiz = "retConsReciNFe";

    private static readonly HashSet<string> Grupos = ["protNFe"];

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    public PedidoWS Montar(ConsultaRecibo parametro, ContextoOperacao contexto)
    {
        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, ConfiguracaoEscrita))
        {
            xml.WriteStartElement("consReciNFe", EscritaNFe.Namespace);
            xml.WriteAttributeString("versao", "4.00");
            xml.WriteElementString("tpAmb", EscritaNFe.Namespace, ((int)contexto.Configuracao.Ambiente).ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("nRec", EscritaNFe.Namespace, parametro.Recibo);
            xml.WriteEndElement();
        }
        var primeira = parametro.Xmls.Select(x => LoteNFe.Chave(LoteNFe.Normalizar(x))).FirstOrDefault(c => c is not null);
        return new PedidoWS(AutorizadoresNFe400.RetAutorizacao, texto.ToString(), SchemasNFe400.Recibo)
        {
            Destino = primeira?.UF.ComoDestino(),
            Emissao = primeira?.TipoEmissao,
        };
    }

    public LeituraWS<RetornoAutorizacao> Ler(ConsultaRecibo parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var problemas = new List<Problema>();
        var campos = LeituraRetorno.Campos(raiz, Conhecido, Grupos, problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        List<(string Xml, ChaveAcesso? Chave)> enviadas =
            [.. parametro.Xmls.Select(LoteNFe.Normalizar).Select(x => (x, LoteNFe.Chave(x)))];
        var temProtocolo = raiz.Elements(System.Xml.Linq.XName.Get("protNFe", EscritaNFe.Namespace)).Any();
        var notas = temProtocolo || OperacaoAutorizacao.StatusLote.Contains(status)
            ? LoteNFe.Resultados(raiz, enviadas, avisarFalta: status == 104, problemas)
            : [];

        MensagemSefaz? mensagem = null;
        if (campos.ContainsKey("cMsg") || campos.ContainsKey("xMsg"))
        {
            mensagem = new MensagemSefaz
            {
                Codigo = LeituraRetorno.Campo(campos, Raiz, "cMsg", problemas, "não é um número", LerCodigo),
                Texto = campos.GetValueOrDefault("xMsg"),
            };
        }

        var retorno = new RetornoAutorizacao(status, motivo, notas.AsReadOnly())
        {
            Ambiente = LeituraRetorno.Campo(campos, Raiz, "tpAmb", problemas, "não é 1 nem 2", LeituraRetorno.LerAmbiente),
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            UF = LeituraRetorno.Campo(campos, Raiz, "cUF", problemas, "não é o código de uma UF", LeituraRetorno.LerUF),
            Recebimento = LeituraRetorno.Campo(campos, Raiz, "dhRecbto", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora),
            Recibo = campos.GetValueOrDefault("nRec"),
            Mensagem = mensagem,
        };
        return new LeituraWS<RetornoAutorizacao>(retorno, status, motivo) { Problemas = problemas };
    }

    private static int? LerCodigo(string texto) =>
        texto.Length <= 4 && int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var codigo) ? codigo : null;

    private static bool Conhecido(string tag) =>
        tag is "tpAmb" or "verAplic" or "nRec" or "cStat" or "xMotivo" or "cUF" or "dhRecbto" or "cMsg" or "xMsg";
}
