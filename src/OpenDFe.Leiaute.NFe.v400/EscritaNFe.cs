using System.Globalization;
using System.Text;
using System.Xml;

namespace OpenDFe.NFe;

/// <summary>
/// O estado de uma escrita da nota (<see cref="EscritorNFe"/>): o <see cref="XmlWriter"/>, o caminho em que a escrita
/// está (no XML e no modelo, para o problema apontar os dois, 6.4) e os problemas da montagem.
/// </summary>
/// <remarks>
/// Cada campo é escrito por um método que recebe a tag e o nome da propriedade e omite o valor nulo (e o texto vazio):
/// a tag vazia nunca sai (PROJETO.md 5.1). Os grupos são abertos e fechados pelo pai, porque a mesma classe aparece com
/// tags diferentes (o <see cref="Local"/> na retirada e na entrega).
/// </remarks>
internal sealed class EscritaNFe
{
    /// <summary>O namespace da NF-e, sem prefixo.</summary>
    internal const string Namespace = "http://www.portalfiscal.inf.br/nfe";

    private readonly XmlWriter _xml;
    private readonly List<Aberto> _passos = [];

    internal EscritaNFe(XmlWriter xml, ValoresNucleo nucleo)
    {
        _xml = xml;
        Nucleo = nucleo;
    }

    /// <summary>Os valores que vêm do núcleo (a configuração).</summary>
    internal ValoresNucleo Nucleo { get; }

    /// <summary>A chave montada, ou nulo quando faltou um dado dela (com o problema).</summary>
    internal ChaveAcesso? Chave { get; set; }

    /// <summary>Os problemas da montagem, na ordem em que apareceram.</summary>
    internal List<Problema> Problemas { get; } = [];

    /// <summary>
    /// Abre um grupo (<c>&lt;tag&gt;</c>). <paramref name="propriedade"/> nulo: o passo não existe no modelo (o
    /// <c>infNFe</c>, o grupo de uma subclasse como o <c>ICMS00</c>). Com <paramref name="indice"/>, um elemento de
    /// lista: o modelo conta a partir de 0, o XML a partir de 1, e o índice do XML só aparece quando o elemento se
    /// repete, como no caminho da validação pelo schema.
    /// </summary>
    internal void Abrir(string tag, string? propriedade, int indice = -1, int quantidade = 0)
    {
        _xml.WriteStartElement(tag, Namespace);
        _passos.Add(new(tag, propriedade, indice, quantidade));
    }

    /// <summary>Fecha o grupo aberto por último.</summary>
    internal void Fechar()
    {
        _xml.WriteFullEndElement();   // o grupo vazio sai <x></x>, nunca <x />
        _passos.RemoveAt(_passos.Count - 1);
    }

    /// <summary>O texto como está; nulo e vazio omitem a tag.</summary>
    internal void Texto(string tag, string propriedade, string? valor, int indice = -1, int quantidade = 0)
    {
        if (string.IsNullOrEmpty(valor) || !Conferir(tag, propriedade, valor, indice, quantidade))
            return;
        _xml.WriteElementString(tag, Namespace, valor);
    }

    /// <summary>O texto como está, inclusive vazio (a tag vazia); só o nulo omite a tag.</summary>
    internal void TextoVazio(string tag, string propriedade, string? valor)
    {
        if (valor is null || !Conferir(tag, propriedade, valor, -1, 0))
            return;
        _xml.WriteStartElement(tag, Namespace);
        _xml.WriteString(valor);
        _xml.WriteFullEndElement();
    }

    /// <summary>O texto que o leiaute traduziu (os valores de enumeração que não são número).</summary>
    internal void Literal(string tag, string propriedade, string? valor)
    {
        if (valor is not null)
            _xml.WriteElementString(tag, Namespace, valor);
    }

    /// <summary>O decimal com as casas do campo, sem arredondar (<see cref="FormatadorInvariavel.Numero"/>).</summary>
    internal void Decimal(string tag, string propriedade, decimal? valor, int casas, int indice = -1, int quantidade = 0)
    {
        if (valor is { } v)
            _xml.WriteElementString(tag, Namespace, FormatadorInvariavel.Numero(v, casas));
    }

    /// <summary>
    /// O número inteiro (também o valor de uma enumeração), com zeros à esquerda até <paramref name="digitos"/> quando o
    /// campo tem tamanho fixo (<c>cNF</c>, <c>CST</c>); 0 = sem zeros.
    /// </summary>
    internal void Inteiro(string tag, string? propriedade, long? valor, int digitos, int indice = -1, int quantidade = 0)
    {
        if (valor is { } v)
            _xml.WriteElementString(tag, Namespace, Numero(v, digitos));
    }

    /// <summary>A data e a hora, com o deslocamento do próprio valor.</summary>
    internal void DataHora(string tag, string propriedade, DateTimeOffset? valor)
    {
        if (valor is { } v)
            _xml.WriteElementString(tag, Namespace, FormatadorInvariavel.DataHora(v));
    }

    /// <summary>A data, <c>AAAA-MM-DD</c>.</summary>
    internal void Data(string tag, string propriedade, DateOnly? valor)
    {
        if (valor is { } v)
            _xml.WriteElementString(tag, Namespace, FormatadorInvariavel.Data(v));
    }

    /// <summary>
    /// A sigla da UF; com <paramref name="exterior"/>, <c>EX</c>. A UF preenchida com o exterior é problema (são o
    /// mesmo campo), e sai o <c>EX</c>.
    /// </summary>
    internal void Sigla(string tag, string propriedade, UF? uf, bool exterior)
    {
        if (exterior)
        {
            if (uf is not null)
                Registrar(CodigosEscrita.Exclusivo, $"A UF e o exterior são o mesmo campo ({tag}): preencha só um dos dois.",
                    null, Propriedade(propriedade, -1));
            _xml.WriteElementString(tag, Namespace, "EX");
            return;
        }
        if (uf is { } u)
            _xml.WriteElementString(tag, Namespace, Enum.IsDefined(u) ? u.ToString() : Numero((int)u, 0));
    }

    /// <summary>O atributo de texto do grupo aberto; nulo e vazio omitem o atributo.</summary>
    internal void TextoAtributo(string tag, string propriedade, string? valor)
    {
        if (string.IsNullOrEmpty(valor) || !Conferir(tag, propriedade, valor, -1, 0))
            return;
        _xml.WriteAttributeString(tag, valor);
    }

    /// <summary>O atributo numérico do grupo aberto.</summary>
    internal void InteiroAtributo(string tag, string? propriedade, long? valor, int digitos)
    {
        if (valor is { } v)
            _xml.WriteAttributeString(tag, Numero(v, digitos));
    }

    /// <summary>
    /// Confere uma escolha do XSD (MODELO.md, decisão 10): só uma das alternativas pode estar preenchida. Mais de uma é
    /// problema, com a propriedade da segunda; a escrita segue com todas, e o problema impede o envio.
    /// </summary>
    internal void Escolha(params ReadOnlySpan<(string Propriedades, bool Preenchida)> alternativas)
    {
        var preenchidas = new List<string>();
        foreach (var (propriedades, preenchida) in alternativas)
            if (preenchida)
                preenchidas.Add(propriedades);
        if (preenchidas.Count < 2)
            return;
        var segunda = preenchidas[1].Split(',')[0].Trim();
        Registrar(CodigosEscrita.Exclusivo,
            $"Só uma destas alternativas pode ser preenchida: {string.Join("; ", preenchidas)}.",
            null, Propriedade(segunda, -1));
    }

    /// <summary>
    /// O valor de um campo que o núcleo já sabe (MODELO.md, decisão 6): nulo na nota, vale o do núcleo; preenchido e
    /// diferente, é problema, e sai o do núcleo.
    /// </summary>
    internal T? Configurado<T>(string tag, string propriedade, T? nota, T? nucleo, string descricao) where T : struct
    {
        if (nota is { } n && !EqualityComparer<T?>.Default.Equals(n, nucleo))
            Divergente(tag, propriedade, descricao);
        return nucleo;
    }

    /// <summary>O texto de um campo que o núcleo já sabe (a justificativa da contingência).</summary>
    internal string? Configurado(string tag, string propriedade, string? nota, string? nucleo, string descricao)
    {
        if (nota is not null && !string.Equals(nota, nucleo, StringComparison.Ordinal))
            Divergente(tag, propriedade, descricao);
        return nucleo;
    }

    /// <summary>
    /// Um problema num campo qualquer da nota, antes de abrir os grupos: o caminho a partir do <c>infNFe</c>
    /// (<c>"ide/dhEmi"</c>) e a propriedade a partir da raiz (<c>"Identificacao.Emissao"</c>).
    /// </summary>
    internal void RegistrarInfNFe(string codigo, string mensagem, string caminho, string propriedade) =>
        Registrar(codigo, mensagem, "NFe/infNFe/" + caminho, propriedade, caminho[(caminho.LastIndexOf('/') + 1)..]);

    /// <summary>
    /// Um problema no <c>infNFeSupl</c>, que fica ao lado do <c>infNFe</c>: <paramref name="tag"/> é o campo do grupo
    /// (<c>qrCode</c>, <c>urlChave</c>) e <paramref name="propriedade"/> o dele no modelo; sem campo, o grupo.
    /// </summary>
    internal void RegistrarSuplementares(string codigo, string mensagem, string? tag = null, string? propriedade = null) =>
        Problemas.Add(new Problema(codigo, Severidade.Erro, mensagem)
        {
            Tag = tag ?? "infNFeSupl",
            Caminho = tag is null ? "NFe/infNFeSupl" : "NFe/infNFeSupl/" + tag,
            Propriedade = propriedade is null ? "Suplementares" : "Suplementares." + propriedade,
        });

    private void Divergente(string tag, string propriedade, string descricao) =>
        Registrar(CodigosEscrita.Nucleo,
            $"O campo {tag} difere da configuração do núcleo ({descricao}): deixe-o nulo para usar o valor do núcleo.",
            Caminho(tag, -1, 0), Propriedade(propriedade, -1), tag);

    private void Registrar(string codigo, string mensagem, string? caminho, string? propriedade, string? tag = null) =>
        Problemas.Add(new Problema(codigo, Severidade.Erro, mensagem)
        {
            Tag = tag,
            Caminho = caminho ?? Caminho(null, -1, 0),
            Propriedade = propriedade,
        });

    // O texto com caractere que o XML 1.0 não aceita (controles, substituto sem par) não é escrito: o XmlWriter
    // lançaria. Vira problema, com o campo.
    private bool Conferir(string tag, string propriedade, string valor, int indice, int quantidade)
    {
        for (var i = 0; i < valor.Length; i++)
        {
            var c = valor[i];
            if (XmlConvert.IsXmlChar(c))
                continue;
            if (char.IsHighSurrogate(c) && i + 1 < valor.Length && char.IsLowSurrogate(valor[i + 1]))
            {
                i++;
                continue;
            }
            Registrar(CodigosEscrita.Caractere,
                string.Create(CultureInfo.InvariantCulture,
                    $"O campo {tag} tem um caractere que o XML não aceita (U+{(int)c:X4}, na posição {i + 1})."),
                Caminho(tag, indice, quantidade), Propriedade(propriedade, indice), tag);
            return false;
        }
        return true;
    }

    private string Caminho(string? tag, int indice, int quantidade)
    {
        var caminho = new StringBuilder("NFe");
        foreach (var passo in _passos)
            caminho.Append('/').Append(Passo(passo.Tag, passo.Indice, passo.Quantidade, xml: true));
        if (tag is not null)
            caminho.Append('/').Append(Passo(tag, indice, quantidade, xml: true));
        return caminho.ToString();
    }

    private string? Propriedade(string? propriedade, int indice)
    {
        var partes = new List<string>();
        foreach (var passo in _passos)
            if (passo.Propriedade is not null)
                partes.Add(Passo(passo.Propriedade, passo.Indice, int.MaxValue, xml: false));
        if (propriedade is not null)
            partes.Add(Passo(propriedade, indice, int.MaxValue, xml: false));
        return partes.Count == 0 ? null : string.Join('.', partes);
    }

    private static string Passo(string nome, int indice, int quantidade, bool xml)
    {
        if (indice < 0 || (xml && quantidade < 2))
            return nome;
        return string.Create(CultureInfo.InvariantCulture, $"{nome}[{(xml ? indice + 1 : indice)}]");
    }

    private static string Numero(long valor, int digitos) =>
        digitos > 0 && valor >= 0
            ? valor.ToString("D" + digitos.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
            : valor.ToString(CultureInfo.InvariantCulture);

    private sealed record Aberto(string Tag, string? Propriedade, int Indice, int Quantidade);
}
