using System.Globalization;
using System.Xml;

namespace OpenDFe;

/// <summary>
/// Os passos 5 e 6 do <see cref="DFe.ExecutarAsync{TRetorno}"/> (REGRAS.md 3): as regras do leiaute e os callbacks de
/// quem usa, sobre os documentos do pedido achados no XML assinado. Sem estado.
/// </summary>
internal static class MotorRegras
{
    private const string EspacoAssinatura = "http://www.w3.org/2000/09/xmldsig#";
    private const string EspacoXmlns = "http://www.w3.org/2000/xmlns/";

    /// <summary>Um documento do pedido, achado na mensagem assinada.</summary>
    internal sealed record Achado(DocumentoPedido Pedido, XmlElement Elemento, DocumentoVerificado Verificado);

    /// <summary>
    /// Acha cada documento no XML assinado: o pai do elemento com o <c>Id</c>. O <c>Id</c> que não está na mensagem, que
    /// está mais de uma vez ou que é da raiz é erro de programação do leiaute.
    /// </summary>
    internal static Achado[] Achar(string xml, IReadOnlyList<DocumentoPedido> documentos, Documento documento)
    {
        var mensagem = Carregar(xml);
        var porId = new Dictionary<string, List<XmlElement>>(StringComparer.Ordinal);
        foreach (XmlElement elemento in mensagem.GetElementsByTagName("*"))
        {
            var id = elemento.GetAttribute("Id");
            if (id.Length > 0)
            {
                if (!porId.TryGetValue(id, out var lista))
                    porId[id] = lista = [];
                lista.Add(elemento);
            }
        }

        var achados = new Achado[documentos.Count];
        for (var i = 0; i < documentos.Count; i++)
        {
            var pedido = documentos[i];
            if (!porId.TryGetValue(pedido.Id, out var elementos))
                throw new InvalidOperationException($"O documento de Id {pedido.Id} não está na mensagem do pedido.");
            if (elementos.Count > 1)
                throw new InvalidOperationException($"O Id {pedido.Id} aparece mais de uma vez na mensagem do pedido.");
            if (elementos[0].ParentNode is not XmlElement pai)
                throw new InvalidOperationException($"O elemento de Id {pedido.Id} é a raiz da mensagem: o documento é o pai dele.");
            achados[i] = new Achado(pedido, pai, new DocumentoVerificado(documento, pedido.Modelo, Texto(pai), pedido.Chave));
        }
        return achados;
    }

    /// <summary>
    /// As regras do leiaute, em todos os documentos; o código em <paramref name="comoAviso"/> sai como aviso.
    /// </summary>
    internal static List<Problema> Regras(
        Achado[] achados,
        IReadOnlyList<Func<DocumentoVerificado, ContextoOperacao, IEnumerable<Problema>>> regras,
        ContextoOperacao contexto,
        IReadOnlySet<string> comoAviso)
    {
        var problemas = new List<Problema>();
        foreach (var achado in achados)
        {
            foreach (var regra in regras)
            {
                foreach (var problema in Conferir(regra(achado.Verificado, contexto), "Uma função de regras do leiaute"))
                {
                    problemas.Add(Prefixar(achado, problema.Severidade == Severidade.Erro && comoAviso.Contains(problema.Codigo)
                        ? Rebaixar(problema)
                        : problema));
                }
            }
        }
        return problemas;
    }

    /// <summary>
    /// O schema de cada documento que o declara (<see cref="DocumentoPedido.Schema"/>), sobre o documento sozinho: o
    /// caminho é o do documento, e a propriedade, a do schema ou, sem ela, a da conversão do documento.
    /// </summary>
    internal static List<Problema> Schemas(Achado[] achados)
    {
        var problemas = new List<Problema>();
        foreach (var achado in achados)
        {
            if (achado.Pedido.Schema is not { } schema)
                continue;
            var conversao = achado.Pedido.Propriedade;
            foreach (var problema in schema.Validar(achado.Verificado.Xml))
            {
                var propriedade = problema.Propriedade
                    ?? (conversao is not null && problema.Caminho is not null ? conversao(problema.Caminho) : null);
                problemas.Add(Prefixar(achado, propriedade == problema.Propriedade
                    ? problema
                    : Copiar(problema, problema.Severidade, propriedade)));
            }
        }
        return problemas;
    }

    /// <summary>Os callbacks de quem usa: o de campo em cada campo de cada documento, depois o de documento.</summary>
    internal static List<Problema> Callbacks(Achado[] achados, ValidacaoCallback? validacao)
    {
        var problemas = new List<Problema>();
        if (validacao is null)
            return problemas;
        foreach (var achado in achados)
        {
            if (validacao.Campo is { } campo)
            {
                foreach (var verificado in Campos(achado))
                    problemas.AddRange(Conferir(campo(verificado), "O callback de campo").Select(p => Prefixar(achado, p)));
            }
            if (validacao.Documento is { } documento)
                problemas.AddRange(Conferir(documento(achado.Verificado), "O callback de documento").Select(p => Prefixar(achado, p)));
        }
        return problemas;
    }

    /// <summary>
    /// Os campos do documento, na ordem do XML: os atributos de cada elemento (fora as declarações de namespace) e cada
    /// elemento sem filhos elementos, fora da <c>Signature</c>; o caminho como o do schema (índice só no que se repete).
    /// </summary>
    internal static IEnumerable<CampoVerificado> Campos(Achado achado)
    {
        var documento = achado.Verificado.Documento;
        var propriedade = achado.Pedido.Propriedade;
        var pilha = new Stack<(XmlElement Elemento, string Caminho)>();
        pilha.Push((achado.Elemento, achado.Elemento.LocalName));
        while (pilha.Count > 0)
        {
            var (elemento, caminho) = pilha.Pop();
            foreach (XmlAttribute atributo in elemento.Attributes)
            {
                if (atributo.NamespaceURI == EspacoXmlns)
                    continue;
                var caminhoAtributo = caminho + "/@" + atributo.LocalName;
                yield return new CampoVerificado(documento, atributo.LocalName, caminhoAtributo, propriedade?.Invoke(caminhoAtributo), atributo.Value);
            }

            var filhos = elemento.ChildNodes.OfType<XmlElement>()
                .Where(e => !(e.LocalName == "Signature" && e.NamespaceURI == EspacoAssinatura))
                .ToList();
            if (filhos.Count == 0)
            {
                // Só a Signature dentro: não é campo.
                if (elemento.ChildNodes.OfType<XmlElement>().Any())
                    continue;
                yield return new CampoVerificado(documento, elemento.LocalName, caminho, propriedade?.Invoke(caminho), elemento.InnerText);
                continue;
            }

            var contagem = filhos.GroupBy(e => e.LocalName, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
            var vistos = new Dictionary<string, int>(StringComparer.Ordinal);
            var passos = new List<(XmlElement, string)>(filhos.Count);
            foreach (var filho in filhos)
            {
                var indice = vistos[filho.LocalName] = vistos.GetValueOrDefault(filho.LocalName) + 1;
                var passo = contagem[filho.LocalName] > 1
                    ? string.Create(CultureInfo.InvariantCulture, $"{filho.LocalName}[{indice}]")
                    : filho.LocalName;
                passos.Add((filho, caminho + "/" + passo));
            }
            for (var i = passos.Count - 1; i >= 0; i--)
                pilha.Push(passos[i]);
        }
    }

    private static Problema[] Conferir(IEnumerable<Problema>? problemas, string origem)
    {
        if (problemas is null)
            throw new InvalidOperationException($"{origem} devolveu uma lista nula.");
        var copia = problemas.ToArray();
        if (Array.Exists(copia, p => p is null))
            throw new InvalidOperationException($"{origem} devolveu um problema nulo.");
        return copia;
    }

    // O prefixo do documento na propriedade (DocumentoPedido.PrefixoPropriedade), quando há os dois.
    private static Problema Prefixar(Achado achado, Problema problema) =>
        achado.Pedido.PrefixoPropriedade is { } prefixo && problema.Propriedade is { } propriedade
            ? Copiar(problema, problema.Severidade, prefixo + propriedade)
            : problema;

    private static Problema Rebaixar(Problema problema) =>
        Copiar(problema, Severidade.Aviso, problema.Propriedade);

    private static Problema Copiar(Problema problema, Severidade severidade, string? propriedade) =>
        new(problema.Codigo, severidade, problema.Mensagem)
        {
            Tag = problema.Tag,
            Caminho = problema.Caminho,
            Propriedade = propriedade,
            Schema = problema.Schema,
        };

    // O documento sozinho, com o namespace declarado nele (o pai pode trazer a declaração na mensagem).
    private static string Texto(XmlElement elemento)
    {
        var copia = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        copia.AppendChild(copia.ImportNode(elemento, deep: true));
        return copia.OuterXml;
    }

    private static XmlDocument Carregar(string xml)
    {
        var configuracao = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };
        using var texto = new StringReader(xml);
        using var leitor = XmlReader.Create(texto, configuracao);
        var documento = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
        documento.Load(leitor);
        return documento;
    }
}
