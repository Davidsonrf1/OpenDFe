using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.NFe;

/// <summary>
/// A leitura branda dos retornos dos serviços (CONTRATOS.md 13, decisão 4): o status, a autorização e a consulta do
/// recibo. Sem o schema do retorno: os campos do lote pelo nome, o desconhecido e o repetido como aviso, o opcional que
/// não se lê nulo com aviso. Sem estado.
/// </summary>
internal static class LeituraRetorno
{
    private static readonly XmlReaderSettings Configuracao = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
    };

    /// <summary>
    /// A raiz do retorno, que tem de ser <paramref name="raiz"/> no namespace da NF-e (senão,
    /// <see cref="FormatException"/>). Sem DTD.
    /// </summary>
    internal static XElement Carregar(string xml, string raiz)
    {
        XElement elemento;
        using (var texto = new StringReader(xml))
        using (var leitor = XmlReader.Create(texto, Configuracao))
            elemento = XElement.Load(leitor);
        if (elemento.Name != XName.Get(raiz, EscritaNFe.Namespace))
            throw new FormatException($"A resposta não é o {raiz} da NF-e: veio {elemento.Name.LocalName}.");
        return elemento;
    }

    /// <summary>
    /// Os campos simples da raiz que <paramref name="conhecido"/> reconhece, pelo nome (vale o primeiro). Os grupos de
    /// <paramref name="grupos"/> ficam de fora sem aviso, para quem lê; o resto é <c>NFE-DESCONHECIDO</c>.
    /// </summary>
    internal static Dictionary<string, string> Campos(XElement raiz, Func<string, bool> conhecido, IReadOnlySet<string> grupos,
        List<Problema> problemas)
    {
        var campos = new Dictionary<string, string>(StringComparer.Ordinal);
        var nomeRaiz = raiz.Name.LocalName;
        foreach (var filho in raiz.Elements())
        {
            var tag = filho.Name.LocalName;
            if (filho.Name.Namespace == EscritaNFe.Namespace && grupos.Contains(tag))
                continue;
            if (filho.Name.Namespace != EscritaNFe.Namespace || !conhecido(tag) || filho.HasElements)
                problemas.Add(Aviso(CodigosLeitura.Desconhecido, nomeRaiz, tag, $"O campo {tag} do retorno não é conhecido do leiaute: ignorado."));
            else if (!campos.TryAdd(tag, filho.Value))
                problemas.Add(Aviso(CodigosLeitura.Repetido, nomeRaiz, tag, $"O campo {tag} do retorno veio repetido: vale o primeiro."));
        }
        return campos;
    }

    /// <summary>O <c>cStat</c> e o <c>xMotivo</c>, obrigatórios: sem eles, a resposta é ilegível.</summary>
    internal static (int Status, string Motivo) Situacao(Dictionary<string, string> campos)
    {
        if (!campos.TryGetValue("cStat", out var textoStatus)
            || !int.TryParse(textoStatus, NumberStyles.None, CultureInfo.InvariantCulture, out var status))
            throw new FormatException("A resposta não tem o cStat, ou ele não é um número.");
        if (!campos.TryGetValue("xMotivo", out var motivo))
            throw new FormatException("A resposta não tem o xMotivo.");
        return (status, motivo);
    }

    /// <summary>O campo opcional: ausente, nulo; ilegível, nulo com o aviso <c>NFE-VALOR</c>.</summary>
    internal static T? Campo<T>(Dictionary<string, string> campos, string raiz, string tag, List<Problema> problemas, string porque,
        Func<string, T?> ler)
        where T : struct
    {
        if (!campos.TryGetValue(tag, out var texto))
            return null;
        if (ler(texto) is { } valor)
            return valor;
        problemas.Add(Aviso(CodigosLeitura.Valor, raiz, tag, $"O campo {tag} do retorno {porque}: fica nulo."));
        return null;
    }

    internal static Ambiente? LerAmbiente(string texto) => texto switch
    {
        "1" => Ambiente.Producao,
        "2" => Ambiente.Homologacao,
        _ => null,
    };

    internal static UF? LerUF(string texto) =>
        int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var codigo) && UnidadesFederativas.TentarLerCodigo(codigo, out var uf)
            ? uf
            : null;

    internal static DateTimeOffset? LerDataHora(string texto) =>
        LeituraNFe.TentarDataHora(texto, out var valor) ? valor : null;

    internal static TimeSpan? LerSegundos(string texto) =>
        texto.Length <= 9 && int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var segundos)
            ? TimeSpan.FromSeconds(segundos)
            : null;

    /// <summary>Um aviso no campo <paramref name="tag"/> da raiz (ou do caminho) <paramref name="raiz"/>.</summary>
    internal static Problema Aviso(string codigo, string raiz, string tag, string mensagem) =>
        new(codigo, Severidade.Aviso, mensagem) { Tag = tag, Caminho = raiz + "/" + tag };
}
