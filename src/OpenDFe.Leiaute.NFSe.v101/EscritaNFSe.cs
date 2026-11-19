using System.Globalization;
using System.Text;
using System.Xml;

namespace OpenDFe.NFSe;

/// <summary>
/// O estado de uma escrita da NFS-e (<see cref="EscritorNFSe"/>): o <see cref="XmlWriter"/>, o caminho em que a escrita
/// está (no XML e no modelo, para o problema apontar os dois) e os problemas da montagem. Como a <c>EscritaNFe</c>.
/// </summary>
internal sealed class EscritaNFSe
{
    /// <summary>O namespace da NFS-e, sem prefixo.</summary>
    internal const string Namespace = "http://www.sped.fazenda.gov.br/nfse";

    private readonly XmlWriter _xml;
    private readonly List<Aberto> _passos = [];

    internal EscritaNFSe(XmlWriter xml)
    {
        _xml = xml;
    }

    /// <summary>Os problemas da montagem, na ordem em que apareceram.</summary>
    internal List<Problema> Problemas { get; } = [];

    /// <summary>
    /// Abre um grupo. <paramref name="propriedade"/> nulo: o passo não existe no modelo (o <c>DPS</c> e o <c>infDPS</c>, que a
    /// raiz achata). Com <paramref name="indice"/>, um elemento de lista: o índice do XML só aparece quando ele se repete.
    /// </summary>
    internal void Abrir(string tag, string? propriedade, int indice = -1, int quantidade = 0)
    {
        _xml.WriteStartElement(tag, Namespace);
        _passos.Add(new(tag, propriedade, indice, quantidade));
    }

    /// <summary>Fecha o grupo aberto por último; o grupo vazio sai <c>&lt;x&gt;&lt;/x&gt;</c>.</summary>
    internal void Fechar()
    {
        _xml.WriteFullEndElement();
        _passos.RemoveAt(_passos.Count - 1);
    }

    /// <summary>Um atributo do grupo aberto.</summary>
    internal void Atributo(string nome, string valor) => _xml.WriteAttributeString(nome, valor);

    /// <summary>
    /// O texto como está. O caractere que o XML 1.0 não aceita (controles, substituto sem par) não é escrito: vira
    /// problema, com o campo, sem o valor (pode ser dado pessoal).
    /// </summary>
    internal void Texto(string tag, string propriedade, string valor, int indice = -1, int quantidade = 0)
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
            Registrar(CodigosNFSe.Caractere, Severidade.Erro,
                string.Create(CultureInfo.InvariantCulture, $"O campo {tag} tem um caractere que o XML não aceita (U+{(int)c:X4}, na posição {i + 1})."),
                tag, propriedade, indice, quantidade);
            return;
        }
        _xml.WriteElementString(tag, Namespace, valor);
    }

    /// <summary>O valor já formatado (número, data, código).</summary>
    internal void Valor(string tag, string texto) => _xml.WriteElementString(tag, Namespace, texto);

    /// <summary>Um problema num campo do grupo aberto (sem tag: no próprio grupo).</summary>
    internal void Registrar(string codigo, Severidade severidade, string mensagem, string? tag, string? propriedade,
        int indice = -1, int quantidade = 0) =>
        Problemas.Add(new Problema(codigo, severidade, mensagem)
        {
            Tag = tag ?? (_passos.Count > 0 ? _passos[^1].Tag : null),
            Caminho = Caminho(tag, indice, quantidade),
            Propriedade = Propriedade(propriedade, indice),
        });

    private string Caminho(string? tag, int indice, int quantidade)
    {
        var caminho = new StringBuilder();
        foreach (var passo in _passos)
            caminho.Append(caminho.Length == 0 ? "" : "/").Append(Passo(passo.Tag, passo.Indice, passo.Quantidade, xml: true));
        if (tag is not null)
            caminho.Append(caminho.Length == 0 ? "" : "/").Append(Passo(tag, indice, quantidade, xml: true));
        return caminho.ToString();
    }

    private string? Propriedade(string? propriedade, int indice)
    {
        var partes = new List<string>();
        foreach (var passo in _passos)
            if (passo.Propriedade is not null)
                partes.Add(Passo(passo.Propriedade, passo.Indice, int.MaxValue, xml: false));
        if (!string.IsNullOrEmpty(propriedade))
            partes.Add(Passo(propriedade, indice, int.MaxValue, xml: false));
        return partes.Count == 0 ? null : string.Join('.', partes);
    }

    private static string Passo(string nome, int indice, int quantidade, bool xml) =>
        indice < 0 || (xml && quantidade < 2)
            ? nome
            : string.Create(CultureInfo.InvariantCulture, $"{nome}[{(xml ? indice + 1 : indice)}]");

    private sealed record Aberto(string Tag, string? Propriedade, int Indice, int Quantidade);
}

/// <summary>A escrita de um objeto do modelo pelo mapa da classe, na ordem do XSD.</summary>
internal static class EscritaMapa
{
    /// <summary>
    /// Escreve os campos do <paramref name="grupo"/> dentro do grupo aberto. A escolha com mais de uma alternativa
    /// preenchida é erro (M7), e a escrita segue com todas; o campo só da 1.00 preenchido é aviso e não sai (M10). Com
    /// <paramref name="proprio"/>, o campo para o qual ele devolve <see langword="true"/> já foi escrito por quem chamou (os
    /// valores do núcleo, M4), no lugar dele na ordem.
    /// </summary>
    internal static void Escrever(EscritaNFSe e, MapaGrupo mapa, object grupo, Func<CampoMapa, bool>? proprio = null)
    {
        foreach (var escolha in mapa.Escolhas)
        {
            var preenchidas = escolha.Where(p => mapa.Campos.First(c => c.Propriedade == p).Preenchido(grupo)).ToList();
            if (preenchidas.Count > 1)
                e.Registrar(CodigosNFSe.Exclusivo, Severidade.Erro,
                    $"Só uma destas alternativas pode ser preenchida: {string.Join(", ", preenchidas)}.", null, preenchidas[1]);
        }
        foreach (var campo in mapa.Campos)
        {
            if (campo.Versao100)
            {
                if (campo.Preenchido(grupo))
                    e.Registrar(CodigosNFSe.NaoSuportado, Severidade.Aviso,
                        $"O campo {campo.Tag} é só da versão 1.00 do leiaute: a 1.01 não o escreve.", campo.Tag, campo.Propriedade);
                continue;
            }
            if (proprio?.Invoke(campo) != true)
                campo.Escrever(e, grupo);
        }
    }
}
