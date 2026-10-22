using System.Globalization;
using System.Text;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// O status do serviço (<c>NFeStatusServico4</c>, MOC 7.0, Visão Geral, 5.5): o <c>consStatServ</c> e o
/// <c>retConsStatServ</c>, iguais na NF-e e na NFC-e.
/// </summary>
/// <remarks>
/// <para>
/// O pedido não é assinado. A UF consultada (<c>cUF</c>) é a do <see cref="ParametroWS.Destino"/>, quando ele é uma UF;
/// senão, a do núcleo (a SVRS, a SVAN e a SVC recebem a UF do emitente).
/// </para>
/// <para>
/// A leitura é branda (<see cref="LeituraRetorno"/>), como a da nota (docs/LEITURA.md), sem o schema do retorno: sem o
/// <c>cStat</c> ou o <c>xMotivo</c>, ou com outra raiz, a resposta é ilegível (<see cref="FormatException"/>); o campo
/// que não se lê fica nulo, com <c>NFE-VALOR</c>, e o que o leiaute não conhece é <c>NFE-DESCONHECIDO</c>, os dois como
/// aviso: a resposta do autorizador já foi processada, e o aviso não pode virar recusa.
/// </para>
/// </remarks>
internal sealed class OperacaoStatus : IOperacao<ConsultaStatus, RetornoStatus>
{
    private const string Versao = "4.00";
    private const string Raiz = "retConsStatServ";

    private static readonly HashSet<string> Grupos = [];

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    public PedidoWS Montar(ConsultaStatus parametro, ContextoOperacao contexto)
    {
        var uf = parametro.Destino is { } destino && destino.TentarUF(out var consultada) ? consultada : contexto.Configuracao.UF;
        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, ConfiguracaoEscrita))
        {
            xml.WriteStartElement("consStatServ", EscritaNFe.Namespace);
            xml.WriteAttributeString("versao", Versao);
            xml.WriteElementString("tpAmb", EscritaNFe.Namespace, ((int)contexto.Configuracao.Ambiente).ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("cUF", EscritaNFe.Namespace, uf.Codigo().ToString(CultureInfo.InvariantCulture));
            xml.WriteElementString("xServ", EscritaNFe.Namespace, "STATUS");
            xml.WriteEndElement();
        }
        return new PedidoWS(AutorizadoresNFe400.Status, texto.ToString(), SchemasNFe400.Status);
    }

    public LeituraWS<RetornoStatus> Ler(ConsultaStatus parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = LeituraRetorno.Carregar(xmlRetorno, Raiz);
        var problemas = new List<Problema>();
        var campos = LeituraRetorno.Campos(raiz, Conhecido, Grupos, problemas);
        var (status, motivo) = LeituraRetorno.Situacao(campos);

        var retorno = new RetornoStatus(status, motivo)
        {
            Ambiente = LeituraRetorno.Campo(campos, Raiz, "tpAmb", problemas, "não é 1 nem 2", LeituraRetorno.LerAmbiente),
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            UF = LeituraRetorno.Campo(campos, Raiz, "cUF", problemas, "não é o código de uma UF", LeituraRetorno.LerUF),
            Recebimento = LeituraRetorno.Campo(campos, Raiz, "dhRecbto", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora),
            TempoMedio = LeituraRetorno.Campo(campos, Raiz, "tMed", problemas, "não é um número de segundos", LeituraRetorno.LerSegundos),
            PrevisaoRetorno = LeituraRetorno.Campo(campos, Raiz, "dhRetorno", problemas, "não é uma data e hora com o fuso", LeituraRetorno.LerDataHora),
            Observacao = campos.GetValueOrDefault("xObs"),
        };
        return new LeituraWS<RetornoStatus>(retorno, status, motivo) { Problemas = problemas };
    }

    private static bool Conhecido(string tag) =>
        tag is "tpAmb" or "verAplic" or "cStat" or "xMotivo" or "cUF" or "dhRecbto" or "tMed" or "dhRetorno" or "xObs";
}
