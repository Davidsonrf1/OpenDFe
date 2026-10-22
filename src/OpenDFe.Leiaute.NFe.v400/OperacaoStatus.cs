using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

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
/// A leitura é branda, como a da nota (docs/LEITURA.md), sem o schema do retorno: sem o <c>cStat</c> ou o
/// <c>xMotivo</c>, ou com outra raiz, a resposta é ilegível (<see cref="FormatException"/>); o campo que não se lê fica
/// nulo, com <c>NFE-VALOR</c>, e o que o leiaute não conhece é <c>NFE-DESCONHECIDO</c>, os dois como aviso: a resposta
/// do autorizador já foi processada, e o aviso não pode virar recusa.
/// </para>
/// </remarks>
internal sealed class OperacaoStatus : IOperacao<ConsultaStatus, RetornoStatus>
{
    private const string Versao = "4.00";
    private const string Raiz = "retConsStatServ";

    private static readonly XmlWriterSettings ConfiguracaoEscrita = new()
    {
        OmitXmlDeclaration = true,
        Indent = false,
        NewLineHandling = NewLineHandling.None,
    };

    private static readonly XmlReaderSettings ConfiguracaoLeitura = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
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

    public LeituraWS<RetornoStatus> Ler(ConsultaStatus parametro, string xmlRetorno, ContextoOperacao contexto)
    {
        var raiz = Carregar(xmlRetorno);
        if (raiz.Name != XName.Get(Raiz, EscritaNFe.Namespace))
            throw new FormatException($"A resposta não é o {Raiz} da NF-e: veio {raiz.Name.LocalName}.");

        var problemas = new List<Problema>();
        var campos = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var filho in raiz.Elements())
        {
            var tag = filho.Name.LocalName;
            if (filho.Name.Namespace != EscritaNFe.Namespace || !Conhecido(tag) || filho.HasElements)
                problemas.Add(Aviso(CodigosLeitura.Desconhecido, tag, $"O campo {tag} do retorno não é conhecido do leiaute: ignorado."));
            else if (!campos.TryAdd(tag, filho.Value))
                problemas.Add(Aviso(CodigosLeitura.Repetido, tag, $"O campo {tag} do retorno veio repetido: vale o primeiro."));
        }

        if (!campos.TryGetValue("cStat", out var textoStatus)
            || !int.TryParse(textoStatus, NumberStyles.None, CultureInfo.InvariantCulture, out var status))
            throw new FormatException("A resposta não tem o cStat, ou ele não é um número.");
        if (!campos.TryGetValue("xMotivo", out var motivo))
            throw new FormatException("A resposta não tem o xMotivo.");

        var retorno = new RetornoStatus(status, motivo)
        {
            Ambiente = Campo(campos, "tpAmb", problemas, "não é 1 nem 2", LerAmbiente),
            VersaoAplicativo = campos.GetValueOrDefault("verAplic"),
            UF = Campo(campos, "cUF", problemas, "não é o código de uma UF", LerUF),
            Recebimento = Campo(campos, "dhRecbto", problemas, "não é uma data e hora com o fuso", LerDataHora),
            TempoMedio = Campo(campos, "tMed", problemas, "não é um número de segundos", LerSegundos),
            PrevisaoRetorno = Campo(campos, "dhRetorno", problemas, "não é uma data e hora com o fuso", LerDataHora),
            Observacao = campos.GetValueOrDefault("xObs"),
        };
        return new LeituraWS<RetornoStatus>(retorno, status, motivo) { Problemas = problemas };
    }

    private static XElement Carregar(string xml)
    {
        using var texto = new StringReader(xml);
        using var leitor = XmlReader.Create(texto, ConfiguracaoLeitura);
        return XElement.Load(leitor);
    }

    private static bool Conhecido(string tag) =>
        tag is "tpAmb" or "verAplic" or "cStat" or "xMotivo" or "cUF" or "dhRecbto" or "tMed" or "dhRetorno" or "xObs";

    // O campo opcional: ausente, nulo; ilegível, nulo com o aviso.
    private static T? Campo<T>(Dictionary<string, string> campos, string tag, List<Problema> problemas, string porque, Func<string, T?> ler)
        where T : struct
    {
        if (!campos.TryGetValue(tag, out var texto))
            return null;
        if (ler(texto) is { } valor)
            return valor;
        problemas.Add(Aviso(CodigosLeitura.Valor, tag, $"O campo {tag} do retorno {porque}: fica nulo."));
        return null;
    }

    private static Ambiente? LerAmbiente(string texto) => texto switch
    {
        "1" => Ambiente.Producao,
        "2" => Ambiente.Homologacao,
        _ => null,
    };

    private static UF? LerUF(string texto) =>
        int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var codigo) && UnidadesFederativas.TentarLerCodigo(codigo, out var uf)
            ? uf
            : null;

    private static DateTimeOffset? LerDataHora(string texto) =>
        LeituraNFe.TentarDataHora(texto, out var valor) ? valor : null;

    private static TimeSpan? LerSegundos(string texto) =>
        texto.Length <= 9 && int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var segundos)
            ? TimeSpan.FromSeconds(segundos)
            : null;

    private static Problema Aviso(string codigo, string tag, string mensagem) =>
        new(codigo, Severidade.Aviso, mensagem) { Tag = tag, Caminho = Raiz + "/" + tag };
}
