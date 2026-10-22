using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// O estado de uma leitura da nota (<see cref="LeitorNFe"/>): o <see cref="XmlReader"/>, o caminho em que a leitura está
/// (no XML e no modelo, para o problema apontar os dois) e os problemas. Espelho da <see cref="EscritaNFe"/>.
/// </summary>
/// <remarks>
/// <para>
/// O grupo é lido pelo nome de cada filho, não pela ordem (docs/LEITURA.md 3): o pai entra no grupo
/// (<see cref="Entrar"/>), o método do grupo percorre os filhos (<see cref="Filho"/>) e o pai sai (<see cref="Sair"/>).
/// Cada leitura de valor consome o elemento inteiro e deixa o leitor no nó seguinte.
/// </para>
/// <para>
/// O campo que o XSD aceita uma vez é lido com o valor atual: repetido, vale o primeiro, com <c>NFE-REPETIDO</c>
/// (<see cref="Unico"/>). O valor que não converte fica nulo, com <c>NFE-VALOR</c>; o valor encontrado não vai na
/// mensagem, porque pode ser dado pessoal (PROJETO.md 6.4).
/// </para>
/// </remarks>
internal sealed class LeituraNFe
{
    /// <summary>O namespace da assinatura (a <c>Signature</c> fica fora do modelo).</summary>
    internal const string NamespaceAssinatura = "http://www.w3.org/2000/09/xmldsig#";

    private const string NamespaceXsi = "http://www.w3.org/2001/XMLSchema-instance";
    private const string NamespaceXmlns = "http://www.w3.org/2000/xmlns/";

    private static readonly string[] FormatosDataHora =
    [
        "yyyy-MM-dd'T'HH:mm:sszzz", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz",
    ];

    private static readonly string[] FormatosDataHoraUtc =
    [
        "yyyy-MM-dd'T'HH:mm:ss'Z'", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'",
    ];

    private readonly XmlReader _xml;
    private readonly List<Aberto> _passos = [];

    internal LeituraNFe(XmlReader xml)
    {
        _xml = xml;
    }

    /// <summary>Os problemas da leitura, na ordem do XML.</summary>
    internal List<Problema> Problemas { get; } = [];

    /// <summary>O <c>cDV</c> lido: fora do modelo, conferido contra a chave (MODELO.md, decisão 6).</summary>
    internal int? DigitoVerificador { get; set; }

    /// <summary>O nome local do filho em que a leitura está; vazio quando ele é de outro namespace.</summary>
    internal string Nome { get; private set; } = "";

    /// <summary>Se o filho em que a leitura está é a <c>Signature</c> da assinatura digital.</summary>
    internal bool Assinatura =>
        _xml.LocalName == "Signature" && _xml.NamespaceURI == NamespaceAssinatura;

    /// <summary>
    /// Vai para o próximo filho do grupo aberto. Falso no fim do grupo. O texto solto entre os filhos é aviso; os
    /// espaços, os comentários e as instruções de processamento são ignorados.
    /// </summary>
    internal bool Filho()
    {
        if (_passos[^1].Vazio)
            return false;
        while (true)
        {
            switch (_xml.NodeType)
            {
                case XmlNodeType.Element:
                    Nome = _xml.NamespaceURI == EscritaNFe.Namespace ? _xml.LocalName : "";
                    return true;
                case XmlNodeType.EndElement:
                case XmlNodeType.None:
                    return false;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    Registrar(CodigosLeitura.Desconhecido, Severidade.Aviso,
                        $"Há texto solto dentro de {_passos[^1].Tag}: fica fora do objeto, no XML original.", null, null);
                    _xml.Read();
                    break;
                default:
                    _xml.Read();
                    break;
            }
        }
    }

    /// <summary>
    /// Entra no grupo em que a leitura está. <paramref name="propriedade"/> nulo: o passo não existe no modelo (o
    /// <c>infNFe</c>, o grupo de uma subclasse como o <c>ICMS00</c>). Com <paramref name="indice"/>, um elemento de
    /// lista: o modelo conta a partir de 0, o XML a partir de 1.
    /// </summary>
    internal void Entrar(string? propriedade, int indice = -1)
    {
        var atributos = new Dictionary<string, string>(StringComparer.Ordinal);
        if (_xml.MoveToFirstAttribute())
        {
            do
            {
                if (_xml.NamespaceURI is not (NamespaceXmlns or NamespaceXsi))
                    atributos[_xml.NamespaceURI.Length == 0 ? _xml.LocalName : _xml.Name] = _xml.Value;
            }
            while (_xml.MoveToNextAttribute());
            _xml.MoveToElement();
        }
        var vazio = _xml.IsEmptyElement;
        _passos.Add(new(_xml.LocalName, propriedade, indice, vazio, atributos, []));
        if (!vazio)
            _xml.Read();
    }

    /// <summary>Sai do grupo aberto por último; o atributo que ninguém leu é aviso.</summary>
    internal void Sair()
    {
        foreach (var atributo in _passos[^1].Atributos.Keys)
            Registrar(CodigosLeitura.Desconhecido, Severidade.Aviso,
                $"O atributo {atributo} não é do leiaute 4.00: fica fora do objeto, no XML original.", atributo, null);
        _xml.Read();   // o fim do grupo, ou o grupo vazio
        _passos.RemoveAt(_passos.Count - 1);
    }

    /// <summary>
    /// Se o filho em que a leitura está aparece pela primeira vez no grupo. Repetido: <c>NFE-REPETIDO</c>, o elemento é
    /// pulado, e vale o primeiro.
    /// </summary>
    internal bool Unico()
    {
        if (_passos[^1].Vistos.Add(_xml.LocalName))
            return true;
        Registrar(CodigosLeitura.Repetido, Severidade.Aviso,
            $"O campo {_xml.LocalName} se repete onde o leiaute aceita um só: vale o primeiro.", _xml.LocalName, null);
        _xml.Skip();
        return false;
    }

    /// <summary>O elemento que o leiaute não conhece: aviso, e é pulado com o que tiver dentro.</summary>
    internal void Desconhecido()
    {
        var nome = _xml.NamespaceURI == EscritaNFe.Namespace ? _xml.LocalName : _xml.Name;
        Registrar(CodigosLeitura.Desconhecido, Severidade.Aviso,
            $"O elemento {nome} não é do leiaute 4.00: fica fora do objeto, no XML original.", nome, null);
        _xml.Skip();
    }

    /// <summary>O campo de uma NT anterior que saiu do schema (docs/LEITURA.md 5): aviso, e é pulado.</summary>
    internal void Retirado(string quando)
    {
        Registrar(CodigosLeitura.Retirado, Severidade.Aviso,
            $"O campo {_xml.LocalName} é de {quando}: fica fora do objeto, no XML original.", _xml.LocalName, null);
        _xml.Skip();
    }

    /// <summary>Pula o elemento sem problema (a <c>Signature</c>).</summary>
    internal void Pular() => _xml.Skip();

    /// <summary>Um grupo: entra, lê com o método do grupo e sai. Repetido, vale o primeiro.</summary>
    internal T Grupo<T>(T? atual, string? propriedade, Action<LeituraNFe, T> ler) where T : class, new()
    {
        if (!Unico())
            return atual!;   // o primeiro foi lido e atribuído
        return Subgrupo(new T(), ler, propriedade);
    }

    /// <summary>O grupo de uma alternativa de escolha (<c>ICMS00</c>): sem passo no modelo.</summary>
    internal T Subgrupo<T>(T grupo, Action<LeituraNFe, T> ler, string? propriedade = null)
    {
        Entrar(propriedade);
        ler(this, grupo);
        Sair();
        return grupo;
    }

    /// <summary>Um item de lista: entra com o índice, lê e acrescenta à lista.</summary>
    internal void Lista<T>(IList<T> lista, string propriedade, Action<LeituraNFe, T> ler) where T : new()
    {
        var item = new T();
        Entrar(propriedade, lista.Count);
        ler(this, item);
        Sair();
        lista.Add(item);
    }

    /// <summary>
    /// Uma escolha por subclasse (<c>ICMS</c>, <c>PIS</c>): o filho diz a subclasse, pela função das alternativas. O filho
    /// que ela não conhece é desconhecido; uma segunda alternativa é repetida.
    /// </summary>
    internal T? Escolha<T>(T? atual, string propriedade, Func<LeituraNFe, string, T?> alternativa) where T : class
    {
        if (!Unico())
            return atual;
        Entrar(propriedade);
        var escolhida = Alternativas(alternativa);
        Sair();
        return escolhida;
    }

    /// <summary>Um item de lista que é uma escolha por subclasse (o <c>NFref</c>); vazio, não entra na lista.</summary>
    internal void ListaEscolha<T>(IList<T> lista, string propriedade, Func<LeituraNFe, string, T?> alternativa) where T : class
    {
        Entrar(propriedade, lista.Count);
        var escolhida = Alternativas(alternativa);
        Sair();
        if (escolhida is not null)
            lista.Add(escolhida);
    }

    private T? Alternativas<T>(Func<LeituraNFe, string, T?> alternativa) where T : class
    {
        T? escolhida = null;
        while (Filho())
        {
            if (escolhida is not null)
            {
                Registrar(CodigosLeitura.Repetido, Severidade.Aviso,
                    $"O grupo {_passos[^1].Tag} tem mais de uma alternativa ({_xml.LocalName}): vale a primeira.", _xml.LocalName, null);
                _xml.Skip();
                continue;
            }
            escolhida = Nome.Length == 0 ? null : alternativa(this, Nome);
            if (escolhida is null)
                Desconhecido();
        }
        return escolhida;
    }

    /// <summary>O texto como está, inclusive vazio.</summary>
    internal string? Texto(string? atual, string? propriedade) => Unico() ? Texto(propriedade) : atual;

    /// <summary>O texto do elemento em que a leitura está, sem conferir a repetição.</summary>
    internal string? Texto(string? propriedade, int indice = -1)
    {
        var tag = _xml.LocalName;
        if (_xml.IsEmptyElement)
        {
            _xml.Read();
            return "";
        }
        _xml.Read();
        var texto = new StringBuilder();
        var grupo = false;
        while (_xml.NodeType != XmlNodeType.EndElement)
        {
            switch (_xml.NodeType)
            {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    texto.Append(_xml.Value);
                    _xml.Read();
                    break;
                case XmlNodeType.Element:
                    grupo = true;
                    _xml.Skip();
                    break;
                default:
                    _xml.Read();
                    break;
            }
        }
        _xml.Read();
        if (!grupo)
            return texto.ToString();
        Invalido(tag, propriedade, indice, "tem elementos dentro, e não um valor");
        return null;
    }

    /// <summary>Os itens de uma lista de textos (o <c>NVE</c>).</summary>
    internal void Textos(IList<string> lista, string propriedade)
    {
        if (Texto(propriedade, lista.Count) is { } texto)
            lista.Add(texto);
    }

    /// <summary>O decimal, com as casas que vierem (<c>.</c> como separador, sem milhar).</summary>
    internal decimal? Decimal(decimal? atual, string propriedade)
    {
        if (!Unico())
            return atual;
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return null;
        if (decimal.TryParse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var valor))
            return valor;
        Invalido(tag, propriedade, -1, "não é um número decimal");
        return null;
    }

    /// <summary>O número inteiro, só com algarismos.</summary>
    internal int? Inteiro(int? atual, string? propriedade) => Unico() ? Inteiro(propriedade) : atual;

    /// <summary>O número inteiro do elemento em que a leitura está, sem conferir a repetição.</summary>
    internal int? Inteiro(string? propriedade)
    {
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return null;
        if (int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor))
            return valor;
        Invalido(tag, propriedade, -1, "não é um número inteiro");
        return null;
    }

    /// <summary>O número inteiro longo, só com algarismos.</summary>
    internal long? Inteiro(long? atual, string propriedade)
    {
        if (!Unico())
            return atual;
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return null;
        if (long.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor))
            return valor;
        Invalido(tag, propriedade, -1, "não é um número inteiro");
        return null;
    }

    /// <summary>
    /// A enumeração pelo valor do XML (MODELO.md, decisão 8): o número fora da lista entra como está, com
    /// <c>NFE-ENUMERACAO</c>.
    /// </summary>
    internal T? Enumeracao<T>(T? atual, string? propriedade) where T : struct, Enum
    {
        if (!Unico())
            return atual;
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return null;
        if (!int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
        {
            Invalido(tag, propriedade, -1, "não é um código numérico");
            return null;
        }
        return Membro<T>(numero, tag, propriedade);
    }

    /// <summary>
    /// O valor que não é número (MODELO.md, decisões 8 e 31): o texto do leiaute pela tradução; fora dela, o número
    /// entra como está, com <c>NFE-ENUMERACAO</c>.
    /// </summary>
    internal T? Literal<T>(T? atual, string propriedade, Func<string, T?> traduzir) where T : struct, Enum
    {
        if (!Unico())
            return atual;
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return null;
        if (traduzir(texto) is { } valor)
            return valor;
        if (int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
            return Membro<T>(numero, tag, propriedade);
        Invalido(tag, propriedade, -1, "não é um dos valores do leiaute");
        return null;
    }

    /// <summary>A data e a hora, só com o deslocamento (o <c>TDateTimeUTC</c>).</summary>
    internal DateTimeOffset? DataHora(DateTimeOffset? atual, string propriedade)
    {
        if (!Unico())
            return atual;
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return null;
        if (TentarDataHora(texto, out var valor))
            return valor;
        Invalido(tag, propriedade, -1, "não é uma data e hora com o fuso (AAAA-MM-DDThh:mm:ss-03:00)");
        return null;
    }

    /// <summary>A data e a hora com o deslocamento ou em UTC (<c>Z</c>), com ou sem a fração do segundo.</summary>
    internal static bool TentarDataHora(string texto, out DateTimeOffset valor) =>
        DateTimeOffset.TryParseExact(texto, FormatosDataHora, CultureInfo.InvariantCulture, DateTimeStyles.None, out valor)
        || DateTimeOffset.TryParseExact(texto, FormatosDataHoraUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out valor);

    /// <summary>A data, <c>AAAA-MM-DD</c>.</summary>
    internal DateOnly? Data(DateOnly? atual, string propriedade)
    {
        if (!Unico())
            return atual;
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return null;
        if (DateOnly.TryParseExact(texto, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var valor))
            return valor;
        Invalido(tag, propriedade, -1, "não é uma data (AAAA-MM-DD)");
        return null;
    }

    /// <summary>A sigla da UF, onde o leiaute não aceita o exterior.</summary>
    internal UF? Sigla(UF? atual, string propriedade)
    {
        if (!Unico())
            return atual;
        return LerSigla(propriedade, false).UF;
    }

    /// <summary>A sigla da UF, ou <c>EX</c> (MODELO.md, decisão 9).</summary>
    internal (UF? UF, bool Exterior) SiglaExterior(UF? atual, bool exterior, string propriedade)
    {
        if (!Unico())
            return (atual, exterior);
        return LerSigla(propriedade, true);
    }

    private (UF? UF, bool Exterior) LerSigla(string propriedade, bool aceitaExterior)
    {
        var tag = _xml.LocalName;
        if (Texto(propriedade) is not { } texto)
            return (null, false);
        if (aceitaExterior && texto == "EX")
            return (null, true);
        if (texto.Length == 2 && char.IsAsciiLetterUpper(texto[0]) && char.IsAsciiLetterUpper(texto[1])
            && UnidadesFederativas.TentarLerSigla(texto, out var uf))
            return (uf, false);
        if (int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var numero))
            return (Membro<UF>(numero, tag, propriedade), false);   // a escrita põe o número da UF fora da lista
        Invalido(tag, propriedade, -1, "não é a sigla de uma UF");
        return (null, false);
    }

    /// <summary>O atributo de texto do grupo aberto; nulo se não veio.</summary>
    internal string? TextoAtributo(string nome, string? propriedade)
    {
        _ = propriedade;
        var atributos = _passos[^1].Atributos;
        if (!atributos.Remove(nome, out var valor))
            return null;
        return valor;
    }

    /// <summary>O atributo numérico do grupo aberto; nulo se não veio ou não é número (com <c>NFE-VALOR</c>).</summary>
    internal int? InteiroAtributo(string nome, string? propriedade)
    {
        if (TextoAtributo(nome, propriedade) is not { } texto)
            return null;
        if (int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var valor))
            return valor;
        Registrar(CodigosLeitura.Valor, Severidade.Erro, $"O atributo {nome} não é um número inteiro: fica nulo.", nome, propriedade);
        return null;
    }

    /// <summary>
    /// Um problema no grupo aberto: o caminho no XML com a <paramref name="tag"/> no fim (quando há) e a propriedade a
    /// partir da raiz do modelo.
    /// </summary>
    internal void Registrar(string codigo, Severidade severidade, string mensagem, string? tag, string? propriedade, int indice = -1) =>
        Problemas.Add(new Problema(codigo, severidade, mensagem)
        {
            Tag = tag,
            Caminho = Caminho(tag, indice),
            Propriedade = Propriedade(propriedade, indice),
        });

    private T Membro<T>(int numero, string tag, string? propriedade) where T : struct, Enum
    {
        var valor = Unsafe.As<int, T>(ref numero);
        if (!Enum.IsDefined(valor))
            Registrar(CodigosLeitura.Enumeracao, Severidade.Aviso,
                string.Create(CultureInfo.InvariantCulture, $"O código {numero} do campo {tag} não está na lista do leiaute: entra como o número."),
                tag, propriedade);
        return valor;
    }

    private void Invalido(string tag, string? propriedade, int indice, string motivo) =>
        Registrar(CodigosLeitura.Valor, Severidade.Erro, $"O campo {tag} {motivo}: fica nulo.", tag, propriedade, indice);

    private string Caminho(string? tag, int indice)
    {
        var caminho = new StringBuilder();
        foreach (var passo in _passos)
            caminho.Append(caminho.Length == 0 ? "" : "/").Append(Passo(passo.Tag, passo.Indice, xml: true));
        if (tag is not null)
            caminho.Append('/').Append(Passo(tag, indice, xml: true));
        return caminho.ToString();
    }

    private string? Propriedade(string? propriedade, int indice)
    {
        var partes = new List<string>();
        foreach (var passo in _passos)
            if (passo.Propriedade is not null)
                partes.Add(Passo(passo.Propriedade, passo.Indice, xml: false));
        if (propriedade is not null)
            partes.Add(Passo(propriedade, indice, xml: false));
        return partes.Count == 0 ? null : string.Join('.', partes);
    }

    private static string Passo(string nome, int indice, bool xml) =>
        indice < 0 ? nome : string.Create(CultureInfo.InvariantCulture, $"{nome}[{(xml ? indice + 1 : indice)}]");

    private sealed record Aberto(string Tag, string? Propriedade, int Indice, bool Vazio,
        Dictionary<string, string> Atributos, HashSet<string> Vistos);
}
