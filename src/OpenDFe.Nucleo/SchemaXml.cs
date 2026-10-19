using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace OpenDFe;

/// <summary>
/// O schema XSD de uma mensagem (a nota, o lote, o pedido de evento), carregado dos recursos embutidos de um
/// assembly e compilado uma vez, e a validação de um XML por ele (PROJETO.md 6.1 e 6.4).
/// </summary>
/// <remarks>
/// <para>
/// A instância é imutável e segura para validar em paralelo: carregue uma vez (o leiaute a guarda) e valide
/// quantas vezes quiser. O <see cref="XmlSchemaSet"/> compilado fica dentro dela, sem ser exposto, para
/// ninguém o alterar.
/// </para>
/// <para>
/// A carga nunca toca o disco nem a rede: o schema principal e os que ele inclui ou importa
/// (<c>xs:include</c>, <c>xs:import</c>) vêm só dos recursos do assembly, e DTD é proibido. A validação não
/// segue o <c>xsi:schemaLocation</c> do XML validado.
/// </para>
/// </remarks>
public sealed class SchemaXml
{
    private readonly XmlSchemaSet _schemas;
    private readonly string _codigo;

    private SchemaXml(XmlSchemaSet schemas, string codigo)
    {
        _schemas = schemas;
        _codigo = codigo;
    }

    /// <summary>Carrega e compila o schema de uma mensagem a partir dos recursos embutidos de um assembly.</summary>
    /// <param name="assembly">O assembly que embute os XSDs (o do leiaute).</param>
    /// <param name="recurso">
    /// O nome do recurso do schema principal, como caminho separado por <c>/</c>
    /// (<c>"Schemas/PL_010/nfe_v4.00.xsd"</c>): os <c>schemaLocation</c> relativos dos <c>include</c> e
    /// <c>import</c> são resolvidos a partir da pasta dele. No projeto, o nome do recurso é dado pelo
    /// <c>LogicalName</c>
    /// (<c>&lt;EmbeddedResource Include="Schemas/**/*.xsd" LogicalName="Schemas/%(RecursiveDir)%(Filename)%(Extension)" /&gt;</c>);
    /// a <c>\</c> do Windows vale como <c>/</c>.
    /// </param>
    /// <param name="codigo">
    /// O código dos problemas da validação: o da rejeição da SEFAZ que a falha no schema da mensagem antecipa
    /// (o leiaute sabe qual é; na NF-e, <c>"225"</c>, "Falha no Schema XML da NFe").
    /// </param>
    /// <returns>O schema compilado.</returns>
    /// <exception cref="ArgumentNullException">Um dos argumentos é nulo.</exception>
    /// <exception cref="ArgumentException">O recurso ou o código estão vazios, ou o recurso não existe no assembly.</exception>
    /// <exception cref="XmlSchemaException">
    /// Um schema é inválido, não compila, inclui ou importa um recurso que não existe ou um endereço externo.
    /// </exception>
    /// <exception cref="XmlException">Um schema não é XML bem formado ou tem DTD.</exception>
    public static SchemaXml Carregar(Assembly assembly, string recurso, string codigo)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(recurso);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        var resolvedor = new ResolvedorDeRecursos(assembly);
        var endereco = resolvedor.Endereco(recurso)
            ?? throw new ArgumentException($"O assembly {assembly.GetName().Name} não tem o recurso \"{recurso}\".", nameof(recurso));

        var schemas = new XmlSchemaSet { XmlResolver = resolvedor };
        var falhas = new List<string>();
        // Os avisos da carga também são falha: o include que não resolve é aviso no XmlSchemaSet, e um schema
        // pela metade validaria menos do que devia.
        schemas.ValidationEventHandler += (_, e) => falhas.Add(e.Exception?.InnerException is { } causa ? $"{e.Message} ({causa.Message})" : e.Message);
        using (var conteudo = resolvedor.Abrir(endereco))
        using (var leitor = XmlReader.Create(conteudo, ResolvedorDeRecursos.ConfiguracaoDeLeitura(), endereco.AbsoluteUri))
        {
            schemas.Add(null, leitor);
        }
        if (falhas.Count == 0)
            schemas.Compile();
        if (falhas.Count > 0)
            throw new XmlSchemaException($"O schema \"{recurso}\" não carregou: {string.Join(" | ", falhas)}");

        return new SchemaXml(schemas, codigo);
    }

    /// <summary>Valida um XML pelo schema, juntando todos os problemas (não para no primeiro).</summary>
    /// <param name="xml">O XML da mensagem.</param>
    /// <returns>
    /// Os problemas, na ordem do XML; vazio se o XML é válido. Cada um tem o código informado na carga, a
    /// severidade (<see cref="Severidade.Erro"/>, ou <see cref="Severidade.Aviso"/> nos avisos do validador), a
    /// mensagem em português, a <see cref="Problema.Tag"/> (o nome local do elemento ou atributo), o
    /// <see cref="Problema.Caminho"/> e o detalhe em <see cref="Problema.Schema"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// O caminho vai da raiz ao elemento, pelos nomes locais separados por <c>/</c>, com o índice a partir de 1
    /// (como no XPath) só no elemento que se repete entre os irmãos: <c>NFe/infNFe/det[3]/prod/vProd</c>; sem
    /// índice, o elemento é o único com aquele nome ali. O atributo vem com <c>@</c>
    /// (<c>NFe/infNFe/@versao</c>). No elemento obrigatório ausente, o caminho é o que ele teria
    /// (<c>NFe/infNFe/det[2]/prod/vProd</c>) quando a lib sabe qual falta; senão, o do elemento pai.
    /// </para>
    /// <para>
    /// Depois de um elemento inesperado, o validador do .NET não confere o resto do conteúdo do mesmo pai: a
    /// falha seguinte ali só aparece depois de corrigida a primeira.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">O XML é nulo.</exception>
    /// <exception cref="XmlException">O XML não é bem formado ou tem DTD.</exception>
    public IReadOnlyList<Problema> Validar(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        return new ValidacaoSchema(_schemas, _codigo).Executar(xml);
    }
}
