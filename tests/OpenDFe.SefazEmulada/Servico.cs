using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.SefazEmulada;

// Um Web Service da NF-e: o nome no caminho (/MG/NFeStatusServico4), o namespace do WSDL (o do
// nfeDadosMsg e do nfeResultMsg) e o processamento da área de dados.
internal abstract class Servico
{
    public const string NomeStatus = "NFeStatusServico4";
    public const string NomeAutorizacao = "NFeAutorizacao4";
    public const string NomeRetAutorizacao = "NFeRetAutorizacao4";

    public static readonly XNamespace NamespaceNFe = "http://www.portalfiscal.inf.br/nfe";

    public abstract string Nome { get; }

    public XNamespace NamespaceWsdl => "http://www.portalfiscal.inf.br/nfe/wsdl/" + Nome;

    // O cStat do nfeDadosMsg sem a área de dados: 516 (D01b); no lote da autorização, 565.
    public virtual string RaizAusente => Motivos.RaizInexistente;

    // A área de dados (o elemento dentro do nfeDadosMsg) e o retorno, já com o cStat.
    public abstract XElement Processar(XElement dados, Pedido pedido);

    // O retorno do serviço com uma rejeição das validações gerais (Anexo I, 4.1: grupos B e D), antes de
    // ler a área de dados.
    public abstract XElement Rejeitar(string codigo, Pedido pedido);

    // A versão do aplicativo: a sigla do autorizador (FR04: "iniciada com a sigla da UF ... ou SVAN ou SVRS").
    protected static string VersaoAplicativo(Pedido pedido) => pedido.Autorizador.Sigla + "-EMULADA";

    // AAAA-MM-DDThh:mm:ssTZD, no horário de Brasília (os autorizadores respondem com -03:00).
    protected static string DataHora(DateTimeOffset instante) =>
        instante.ToOffset(TimeSpan.FromHours(-3)).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

    protected static XElement Elemento(string nome, object? conteudo) => new(NamespaceNFe + nome, conteudo);

    // As validações do grupo D (Anexo I, 4.1.3) sobre a raiz da mensagem: a tag raiz (516; 565 no lote da
    // autorização), o prefixo de namespace (404), o atributo versao (517; 568 no lote) e a versão suportada
    // (239, B06). Nulo: a raiz está certa.
    protected static string? ConferirRaiz(XElement dados, Pedido pedido, string raiz, string versao,
        string raizAusente = Motivos.RaizInexistente, string versaoAusente = Motivos.VersaoInexistente)
    {
        if (dados.Name != NamespaceNFe + raiz)
            return raizAusente;
        if (pedido.Prefixo)
            return Motivos.PrefixoNamespace;
        var atributo = dados.Attribute("versao");
        if (atributo is null)
            return versaoAusente;
        return atributo.Value == versao ? null : Motivos.VersaoNaoSuportada;
    }

    // O texto da área de dados para o schema e a assinatura: o elemento sozinho, sem as declarações do
    // envelope (que o C14N levaria para dentro do que é assinado), com o espaço em branco como veio.
    protected static string Texto(XElement elemento) => elemento.ToString(SaveOptions.DisableFormatting);

    // Os filhos da raiz, na ordem, no namespace da NF-e e sem filhos; nulo se a estrutura não é essa
    // (215, falha no schema) ou se a raiz tem atributo além da versao e das declarações de namespace.
    protected static string[]? Campos(XElement dados, params string[] nomes)
    {
        if (dados.Attributes().Any(a => !a.IsNamespaceDeclaration && a.Name != "versao"))
            return null;
        var filhos = dados.Elements().ToList();
        if (filhos.Count != nomes.Length)
            return null;
        var valores = new string[nomes.Length];
        for (var i = 0; i < nomes.Length; i++)
        {
            if (filhos[i].Name != NamespaceNFe + nomes[i] || filhos[i].HasElements || filhos[i].HasAttributes)
                return null;
            valores[i] = filhos[i].Value;
        }
        return valores;
    }
}

// O que o atendimento sabe do pedido, além da área de dados. Prefixo: algum elemento da área de dados veio
// com prefixo de namespace (o LINQ to XML não guarda o prefixo; quem lê o envelope confere). Transmissor: o
// CNPJ ou o CPF do certificado do TLS (ou a impressão digital, sem os campos da ICP-Brasil), que o recibo
// guarda (Visão Geral, 4.3.3). Processando: o lote consultado responde 105 (a falha comandada).
// Pendentes: as consultas que o lote novo responde com 105 (a marca no infCpl).
internal sealed record Pedido(
    Autorizador Autorizador,
    DateTimeOffset Recebimento,
    Paralisacao? Paralisacao,
    bool Prefixo = false,
    string Transmissor = "",
    Repositorio? Repositorio = null,
    bool Processando = false,
    int Pendentes = 0);

// O serviço paralisado (Anexo I, B03 e B04): 108 (momentaneamente) ou 109 (sem previsão).
internal sealed record Paralisacao(string Codigo, string? Observacao);
