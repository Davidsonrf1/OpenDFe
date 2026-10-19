using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace OpenDFe;

// Resolve os include e import dos schemas só nos recursos embutidos de um assembly, pelo esquema
// "recurso:///<nome do recurso>". Qualquer outro endereço (arquivo, http) é recusado: a carga nunca toca o
// disco nem a rede.
internal sealed class ResolvedorDeRecursos : XmlResolver
{
    private const string Esquema = "recurso";

    private readonly Assembly _assembly;
    private readonly Dictionary<string, string> _recursos = new(StringComparer.Ordinal);

    public ResolvedorDeRecursos(Assembly assembly)
    {
        _assembly = assembly;
        foreach (var nome in assembly.GetManifestResourceNames())
            _recursos.TryAdd(Normalizar(nome), nome);
    }

    public static XmlReaderSettings ConfiguracaoDeLeitura() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
    };

    // O endereço do recurso, ou null se ele não existe.
    public Uri? Endereco(string recurso)
    {
        var nome = Normalizar(recurso);
        return _recursos.ContainsKey(nome) ? Montar(nome) : null;
    }

    public Stream Abrir(Uri endereco)
    {
        var nome = Nome(endereco);
        if (nome is null || !_recursos.TryGetValue(nome, out var real))
            throw new XmlSchemaException($"O schema \"{endereco.OriginalString}\" não está nos recursos do assembly {_assembly.GetName().Name}.");
        return _assembly.GetManifestResourceStream(real)
            ?? throw new XmlSchemaException($"O recurso \"{real}\" não pôde ser aberto.");
    }

    public override Uri ResolveUri(Uri? baseUri, string? relativeUri)
    {
        ArgumentNullException.ThrowIfNull(relativeUri);
        if (Uri.TryCreate(relativeUri, UriKind.Absolute, out var absoluto) && absoluto.Scheme.Length > 1)
            return absoluto;   // externo: recusado no GetEntity

        var pasta = "";
        if (baseUri is not null && Nome(baseUri) is { } nomeBase)
        {
            var barra = nomeBase.LastIndexOf('/');
            pasta = barra < 0 ? "" : nomeBase[..(barra + 1)];
        }

        var partes = new List<string>();
        foreach (var parte in (pasta + Normalizar(relativeUri)).Split('/'))
        {
            if (parte is "" or ".")
                continue;
            if (parte == "..")
            {
                if (partes.Count > 0)
                    partes.RemoveAt(partes.Count - 1);
                continue;
            }
            partes.Add(parte);
        }
        return Montar(string.Join('/', partes));
    }

    public override object? GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
    {
        ArgumentNullException.ThrowIfNull(absoluteUri);
        if (ofObjectToReturn is not null && ofObjectToReturn != typeof(Stream) && ofObjectToReturn != typeof(object))
            throw new XmlSchemaException($"Tipo de entidade não suportado: {ofObjectToReturn}.");
        if (Nome(absoluteUri) is null)
            throw new XmlSchemaException($"O schema \"{absoluteUri.OriginalString}\" está fora dos recursos embutidos: endereço externo não é carregado.");
        return Abrir(absoluteUri);
    }

    private static string Normalizar(string nome) => nome.Replace('\\', '/').TrimStart('/');

    private static Uri Montar(string nome) => new(Esquema + ":///" + Uri.EscapeDataString(nome).Replace("%2F", "/", StringComparison.Ordinal));

    private static string? Nome(Uri endereco) =>
        endereco.IsAbsoluteUri && endereco.Scheme == Esquema
            ? Uri.UnescapeDataString(endereco.AbsolutePath).TrimStart('/')
            : null;
}
