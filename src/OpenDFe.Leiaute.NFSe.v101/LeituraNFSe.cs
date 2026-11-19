using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace OpenDFe.NFSe;

/// <summary>
/// O estado de uma leitura da NFS-e, da DPS ou do evento (<see cref="LeitorNFSe"/>): a versão do XML, o caminho em que a
/// leitura está (no XML e no modelo) e os problemas. Espelho da <see cref="EscritaNFSe"/>.
/// </summary>
/// <remarks>
/// O grupo é lido pelo nome de cada filho, não pela ordem (docs/LEITURA.md 3). O valor que não converte fica nulo, com
/// erro; o valor encontrado não vai na mensagem, porque pode ser dado pessoal.
/// </remarks>
internal sealed class LeituraNFSe
{
    /// <summary>O namespace da assinatura (a <c>Signature</c> fica fora do modelo).</summary>
    internal const string NamespaceAssinatura = "http://www.w3.org/2000/09/xmldsig#";

    private readonly List<Aberto> _passos = [];

    /// <summary>A versão do leiaute do XML (<c>1.00</c> ou <c>1.01</c>).</summary>
    internal string? Versao { get; set; }

    /// <summary>O <c>valores/xOutInf</c> de uma NFS-e 1.00, que na 1.01 subiu para o <c>infNFSe</c> (M10).</summary>
    internal string? InformacoesFisco100 { get; set; }

    /// <summary>Os problemas da leitura, na ordem do XML.</summary>
    internal List<Problema> Problemas { get; } = [];

    /// <summary>Entra num grupo; <paramref name="propriedade"/> nulo: o passo não existe no modelo.</summary>
    internal void Entrar(string tag, string? propriedade, int indice = -1) => _passos.Add(new(tag, propriedade, indice));

    /// <summary>Sai do grupo em que entrou por último.</summary>
    internal void Sair() => _passos.RemoveAt(_passos.Count - 1);

    /// <summary>
    /// O texto de um elemento simples; nulo, com erro, quando ele tem elementos dentro. O texto vazio entra como vazio.
    /// </summary>
    internal string? TextoSimples(XElement elemento, string? propriedade, int indice = -1)
    {
        if (elemento.HasElements)
        {
            Registrar(CodigosNFSe.Valor, Severidade.Erro, $"O campo {elemento.Name.LocalName} tem elementos dentro: não é um valor.",
                elemento.Name.LocalName, propriedade, indice);
            return null;
        }
        return elemento.Value;
    }

    /// <summary>Um problema num campo do grupo em que a leitura está (sem tag: no próprio grupo).</summary>
    internal void Registrar(string codigo, Severidade severidade, string mensagem, string? tag, string? propriedade, int indice = -1) =>
        Problemas.Add(new Problema(codigo, severidade, mensagem)
        {
            Tag = tag,
            Caminho = Caminho(tag, indice),
            Propriedade = Propriedade(propriedade, indice),
        });

    private string Caminho(string? tag, int indice)
    {
        var caminho = new StringBuilder();
        foreach (var passo in _passos)
            caminho.Append(caminho.Length == 0 ? "" : "/").Append(Passo(passo.Tag, passo.Indice, xml: true));
        if (tag is not null)
            caminho.Append(caminho.Length == 0 ? "" : "/").Append(Passo(tag, indice, xml: true));
        return caminho.ToString();
    }

    private string? Propriedade(string? propriedade, int indice)
    {
        var partes = new List<string>();
        foreach (var passo in _passos)
            if (passo.Propriedade is not null)
                partes.Add(Passo(passo.Propriedade, passo.Indice, xml: false));
        if (!string.IsNullOrEmpty(propriedade))
            partes.Add(Passo(propriedade, indice, xml: false));
        return partes.Count == 0 ? null : string.Join('.', partes);
    }

    private static string Passo(string nome, int indice, bool xml) =>
        indice < 0 ? nome : string.Create(CultureInfo.InvariantCulture, $"{nome}[{(xml ? indice + 1 : indice)}]");

    private sealed record Aberto(string Tag, string? Propriedade, int Indice);
}

/// <summary>A leitura de um elemento do XML num objeto do modelo, pelo mapa da classe.</summary>
internal static class LeituraMapa
{
    /// <summary>
    /// Lê os filhos de <paramref name="elemento"/> para o <paramref name="grupo"/>: o filho conhecido entra no objeto (os da
    /// 1.00 também); o desconhecido é aviso; o campo que não se repete e veio repetido vale o primeiro, com aviso; a
    /// <c>Signature</c> é pulada, e o filho para o qual <paramref name="ignorar"/> devolve <see langword="true"/> é de quem
    /// chamou (o detalhe do evento, o <c>xDesc</c>).
    /// </summary>
    internal static void Ler(LeituraNFSe l, MapaGrupo mapa, object grupo, XElement elemento, Func<XElement, bool>? ignorar = null)
    {
        var lidos = new HashSet<CampoMapa>();
        foreach (var filho in elemento.Elements())
        {
            var nome = filho.Name.LocalName;
            if ((filho.Name.NamespaceName == LeituraNFSe.NamespaceAssinatura && nome == "Signature") || ignorar?.Invoke(filho) == true)
                continue;
            var campo = filho.Name.NamespaceName == EscritaNFSe.Namespace ? mapa.Campo(nome) : null;
            if (campo is null)
            {
                l.Registrar(CodigosNFSe.Desconhecido, Severidade.Aviso, $"O campo {nome} não é do leiaute: ficou de fora do objeto, e está no XML.", nome, null);
                continue;
            }
            if (!campo.Repete && !lidos.Add(campo))
            {
                l.Registrar(CodigosNFSe.Repetido, Severidade.Aviso, $"O campo {nome} veio repetido: vale o primeiro.", nome, campo.Propriedade);
                continue;
            }
            campo.Ler(l, grupo, filho);
        }
    }
}
