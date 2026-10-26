#:property TreatWarningsAsErrors=false
#:property EnforceCodeStyleInBuild=false
#:property AnalysisLevel=none

// Gera a referência da API pública (docs/referencia/*.md) a partir das DLLs compiladas e dos comentários XML.
//
//   dotnet build OpenDFe.slnx -c Release
//   dotnet run eng/GerarReferencia.cs
//
// Só os tipos e membros públicos entram (os mesmos dos PublicAPI.*.txt). A descrição é o <summary> de cada membro;
// a do tipo leva também o <remarks>. No modelo da NF-e, cada propriedade traz a tag do XML e o identificador do MOC
// (o atributo [Tag]). As páginas são reescritas inteiras a cada execução: não edite à mão.

using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// O aviso do rodapé, o mesmo das páginas de guia (docs/guia).
const string Rodape = """

---

> **Aviso:** esta documentação foi criada com assistência de inteligência artificial e pode conter imprecisões. Em caso de dúvida, consulte sempre o [código-fonte](https://github.com/Davidsonrf1/OpenDFe), que é a referência definitiva.

""";

var raiz = Directory.GetCurrentDirectory();
while (!File.Exists(Path.Combine(raiz, "OpenDFe.slnx")))
    raiz = Path.GetDirectoryName(raiz) ?? throw new InvalidOperationException("Rode dentro do repositório da OpenDFe.");

var pastaLeiaute = Path.Combine(raiz, "src", "OpenDFe.Leiaute.NFe.v400", "bin", "Release", "net10.0");
var pastaPdf = Path.Combine(raiz, "src", "OpenDFe.Impressao.Pdf", "bin", "Release", "net10.0");
var saida = Path.Combine(raiz, "docs", "referencia");
foreach (var pasta in new[] { pastaLeiaute, pastaPdf })
{
    if (!Directory.Exists(pasta))
        throw new InvalidOperationException($"Pasta não encontrada: {pasta}. Rode antes: dotnet build OpenDFe.slnx -c Release");
}

AssemblyLoadContext.Default.Resolving += (contexto, nome) =>
{
    foreach (var pasta in new[] { pastaLeiaute, pastaPdf })
    {
        var arquivo = Path.Combine(pasta, nome.Name + ".dll");
        if (File.Exists(arquivo))
            return contexto.LoadFromAssemblyPath(arquivo);
    }
    var pacotes = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", nome.Name!.ToLowerInvariant());
    if (Directory.Exists(pacotes))
    {
        var dll = Directory.EnumerateFiles(pacotes, nome.Name + ".dll", SearchOption.AllDirectories).OrderByDescending(f => f).FirstOrDefault();
        if (dll is not null)
            return contexto.LoadFromAssemblyPath(dll);
    }
    return null;
};

Assembly Carregar(string pasta, string nome) => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(pasta, nome + ".dll"));

var nucleo = Carregar(pastaLeiaute, "OpenDFe.Nucleo");
var modelo = Carregar(pastaLeiaute, "OpenDFe.NFe.Modelo");
var leiaute = Carregar(pastaLeiaute, "OpenDFe.Leiaute.NFe.v400");
var pdf = Carregar(pastaPdf, "OpenDFe.Impressao.Pdf");

var documentacao = new Dictionary<string, XElement>(StringComparer.Ordinal);
foreach (var (pasta, nome) in new[] { (pastaLeiaute, "OpenDFe.Nucleo"), (pastaLeiaute, "OpenDFe.NFe.Modelo"), (pastaLeiaute, "OpenDFe.Leiaute.NFe.v400"), (pastaPdf, "OpenDFe.Impressao.Pdf") })
{
    foreach (var membro in XDocument.Load(Path.Combine(pasta, nome + ".xml")).Descendants("member"))
        documentacao[membro.Attribute("name")!.Value] = membro;
}

// As páginas: o arquivo, o título, o pacote e os tipos.
var paginas = new List<Pagina>
{
    new("opendfe.md", "Namespace OpenDFe", "OpenDFe.Nucleo", "Os contratos do núcleo: o `DFe`, a configuração, as conexões, o certificado, a assinatura, os resultados, os problemas, a chave de acesso e os documentos.",
        Publicos(nucleo).Where(t => t.Namespace == "OpenDFe").ToList(), false),
    new("opendfe-impressao.md", "Namespace OpenDFe.Impressao", "OpenDFe.Nucleo", "O modelo de impressão neutro: o documento, o papel e os elementos que o DANFE e o DANFCE usam.",
        Publicos(nucleo).Where(t => t.Namespace == "OpenDFe.Impressao").ToList(), false),
    new("opendfe-nfe.md", "Namespace OpenDFe.NFe: leiaute e operações", "OpenDFe.NFe", "O leiaute 4.00 da NF-e e da NFC-e: as operações (parâmetros e retornos), a leitura, a NFC-e, os eventos, a distribuição, o cadastro e a impressão.",
        Publicos(leiaute).ToList(), false),
    new("opendfe-nfe-modelo.md", "Namespace OpenDFe.NFe: modelo (classes)", "OpenDFe.NFe", "As classes do modelo da NF-e e da NFC-e, com a tag do XML e o identificador do MOC de cada propriedade.",
        Publicos(modelo).Where(t => !t.IsEnum).ToList(), true),
    new("opendfe-nfe-modelo-enumeracoes.md", "Namespace OpenDFe.NFe: modelo (enumerações)", "OpenDFe.NFe", "As enumerações do modelo da NF-e e da NFC-e. O valor de cada membro é o do XML.",
        Publicos(modelo).Where(t => t.IsEnum).ToList(), true),
    new("opendfe-impressao-pdf.md", "Namespace OpenDFe.Impressao.Pdf", "OpenDFe.Impressao.Pdf", "O renderizador PDF do modelo de impressão.",
        Publicos(pdf).ToList(), false),
};

var ondeTipo = new Dictionary<Type, (string Arquivo, string Ancora)>();
foreach (var pagina in paginas)
{
    foreach (var tipo in pagina.Tipos)
        ondeTipo[tipo] = (pagina.Arquivo, Ancora(NomeCurto(tipo)));
}
var tipoNome = ondeTipo.Keys.ToDictionary(t => t.FullName!.Replace('+', '.'), t => t, StringComparer.Ordinal);

Directory.CreateDirectory(saida);
foreach (var pagina in paginas)
{
    var texto = new StringBuilder();
    texto.AppendLine($"# {pagina.Titulo}");
    texto.AppendLine();
    texto.AppendLine($"{pagina.Descricao} Pacote NuGet: `{pagina.Pacote}`.");
    texto.AppendLine();
    texto.AppendLine("> **Observação**");
    texto.AppendLine("> Página gerada por `eng/GerarReferencia.cs` a partir dos comentários XML do código. Não edite à mão.");
    texto.AppendLine();
    texto.AppendLine("[Voltar à referência da API](index.md)");
    texto.AppendLine();

    texto.AppendLine("## Tipos");
    texto.AppendLine();
    texto.AppendLine("| Tipo | Descrição |");
    texto.AppendLine("|---|---|");
    foreach (var tipo in pagina.Tipos)
        texto.AppendLine($"| [`{Escapar(NomeCurto(tipo))}`](#{Ancora(NomeCurto(tipo))}) | {Celula(Resumo(IdTipo(tipo), pagina.Arquivo))} |");
    texto.AppendLine();

    foreach (var tipo in pagina.Tipos)
        EscreverTipo(texto, tipo, pagina);
    texto.Append(Rodape);

    File.WriteAllText(Path.Combine(saida, pagina.Arquivo), texto.ToString().Replace("\r\n", "\n"), new UTF8Encoding(false));
    Console.WriteLine($"{pagina.Arquivo}: {pagina.Tipos.Count} tipos");
}

// O índice da referência.
var indice = new StringBuilder();
indice.AppendLine("# Referência da API");
indice.AppendLine();
indice.AppendLine("A referência lista todos os tipos e membros públicos da OpenDFe, organizados por namespace. Para aprender a usar a biblioteca, comece pelos [guias](../index.md); volte aqui para consultar a assinatura e o comportamento exato de um membro.");
indice.AppendLine();
indice.AppendLine("> **Observação**");
indice.AppendLine("> As páginas desta seção são geradas por `eng/GerarReferencia.cs` a partir dos comentários XML do código-fonte. Para atualizá-las, compile em Release (`dotnet build OpenDFe.slnx -c Release`) e rode `dotnet run eng/GerarReferencia.cs` na raiz do repositório.");
indice.AppendLine();
indice.AppendLine("| Página | Pacote | Conteúdo | Tipos |");
indice.AppendLine("|---|---|---|---|");
foreach (var pagina in paginas)
    indice.AppendLine($"| [{pagina.Titulo}]({pagina.Arquivo}) | `{pagina.Pacote}` | {pagina.Descricao} | {pagina.Tipos.Count} |");
indice.AppendLine();
indice.AppendLine("## Índice alfabético");
indice.AppendLine();
foreach (var grupo in ondeTipo.OrderBy(t => NomeCurto(t.Key), StringComparer.Ordinal).GroupBy(t => char.ToUpperInvariant(NomeCurto(t.Key)[0])))
{
    indice.AppendLine($"**{grupo.Key}**: " + string.Join(" · ", grupo.Select(t => $"[`{NomeCurto(t.Key)}`]({t.Value.Arquivo}#{t.Value.Ancora})")));
    indice.AppendLine();
}
indice.Append(Rodape);
File.WriteAllText(Path.Combine(saida, "index.md"), indice.ToString().Replace("\r\n", "\n"), new UTF8Encoding(false));
Console.WriteLine("index.md");

// ---------------------------------------------------------------------------------------------------------------------


static List<Type> Publicos(Assembly assembly) =>
    [.. assembly.GetExportedTypes().Where(t => !t.IsNested || t.IsNestedPublic).OrderBy(NomeCurto, StringComparer.Ordinal)];

void EscreverTipo(StringBuilder texto, Type tipo, Pagina pagina)
{
    var nome = NomeCurto(tipo);
    texto.AppendLine(nome.Contains('<') ? $"## `{nome}`" : $"## {nome}");
    texto.AppendLine();
    var natureza = Natureza(tipo);
    if (pagina.Modelo && tipo.GetCustomAttributesData().FirstOrDefault(a => a.AttributeType.Name == "TagAttribute") is { } tagTipo)
        natureza += $" · Tag `{tagTipo.ConstructorArguments[0].Value}`" + (tagTipo.ConstructorArguments.Count > 1 ? $" ({tagTipo.ConstructorArguments[1].Value})" : "");
    var heranca = new List<string>();
    if (tipo.BaseType is { } baseTipo && baseTipo != typeof(object) && baseTipo != typeof(Enum) && baseTipo != typeof(ValueType))
        heranca.Add(baseTipo.Name == "Attribute" ? "`Attribute`" : LinkTipo(baseTipo, pagina.Arquivo));
    foreach (var interfaceTipo in tipo.GetInterfaces().Where(i => i.IsPublic && !(tipo.BaseType?.GetInterfaces().Contains(i) ?? false)))
        heranca.Add(LinkTipo(interfaceTipo, pagina.Arquivo));
    texto.AppendLine($"{natureza} · Namespace `{tipo.Namespace}`" + (heranca.Count > 0 ? " · Deriva de " + string.Join(", ", heranca) : ""));
    texto.AppendLine();

    var id = IdTipo(tipo);
    var resumo = Resumo(id, pagina.Arquivo);
    if (resumo.Length > 0)
    {
        texto.AppendLine(resumo);
        texto.AppendLine();
    }
    var comentario = Observacoes(id, pagina.Arquivo);
    if (comentario.Length > 0)
    {
        texto.AppendLine(comentario);
        texto.AppendLine();
    }

    if (tipo.IsEnum)
    {
        texto.AppendLine("| Membro | Valor | Descrição |");
        texto.AppendLine("|---|---|---|");
        foreach (var campo in tipo.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var valor = Convert.ToInt64(campo.GetRawConstantValue(), System.Globalization.CultureInfo.InvariantCulture);
            texto.AppendLine($"| `{campo.Name}` | {valor} | {Celula(Resumo("F:" + IdNome(tipo) + "." + campo.Name, pagina.Arquivo))} |");
        }
        texto.AppendLine();
        return;
    }

    const BindingFlags Declarados = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    var contexto = new NullabilityInfoContext();

    var construtores = tipo.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    if (construtores.Length > 0)
    {
        texto.AppendLine("### Construtores");
        texto.AppendLine();
        texto.AppendLine("| Construtor | Descrição |");
        texto.AppendLine("|---|---|");
        foreach (var construtor in construtores)
            texto.AppendLine($"| `{Escapar(NomeCurto(tipo).Split('<')[0] + Parametros(construtor, contexto))}` | {Celula(DescricaoMetodo(IdMetodo(construtor), pagina.Arquivo))} |");
        texto.AppendLine();
    }

    var constantes = tipo.GetFields(Declarados).Where(f => !f.IsSpecialName).ToList();
    if (constantes.Count > 0)
    {
        texto.AppendLine("### Campos");
        texto.AppendLine();
        texto.AppendLine("| Campo | Tipo | Valor | Descrição |");
        texto.AppendLine("|---|---|---|---|");
        foreach (var campo in constantes)
        {
            var valor = campo.IsLiteral ? Convert.ToString(campo.GetRawConstantValue(), System.Globalization.CultureInfo.InvariantCulture) : "";
            texto.AppendLine($"| `{campo.Name}` | `{Escapar(NomeTipo(campo.FieldType, contexto.Create(campo)))}` | {valor} | {Celula(Resumo("F:" + IdNome(tipo) + "." + campo.Name, pagina.Arquivo))} |");
        }
        texto.AppendLine();
    }

    var propriedades = tipo.GetProperties(Declarados).Where(p => p.GetMethod?.IsPublic == true).ToList();
    if (propriedades.Count > 0)
    {
        texto.AppendLine("### Propriedades");
        texto.AppendLine();
        texto.AppendLine(pagina.Modelo
            ? "| Propriedade | Tipo | Tag | MOC | Descrição |\n|---|---|---|---|---|"
            : "| Propriedade | Tipo | Acesso | Descrição |\n|---|---|---|---|");
        foreach (var propriedade in propriedades)
        {
            var tipoTexto = Escapar(NomeTipo(propriedade.PropertyType, contexto.Create(propriedade)));
            var descricao = Celula(Resumo("P:" + IdNome(tipo) + "." + propriedade.Name, pagina.Arquivo));
            var estatico = propriedade.GetMethod!.IsStatic ? "static " : "";
            if (pagina.Modelo)
            {
                var tag = propriedade.GetCustomAttributesData().FirstOrDefault(a => a.AttributeType.Name == "TagAttribute");
                var nomeTag = tag?.ConstructorArguments.ElementAtOrDefault(0).Value as string;
                var identificador = tag?.ConstructorArguments.Count > 1 ? tag.ConstructorArguments[1].Value as string : null;
                texto.AppendLine($"| `{estatico}{propriedade.Name}` | `{tipoTexto}` | {(nomeTag is null ? "" : $"`{nomeTag}`")} | {identificador} | {descricao} |");
            }
            else
            {
                texto.AppendLine($"| `{estatico}{propriedade.Name}` | `{tipoTexto}` | {Acesso(propriedade)} | {descricao} |");
            }
        }
        texto.AppendLine();
    }

    var metodos = tipo.GetMethods(Declarados).Where(m => !m.IsSpecialName || m.Name.StartsWith("op_", StringComparison.Ordinal)).ToList();
    if (metodos.Count > 0)
    {
        texto.AppendLine("### Métodos");
        texto.AppendLine();
        texto.AppendLine("| Método | Retorno | Descrição |");
        texto.AppendLine("|---|---|---|");
        foreach (var metodo in metodos.OrderBy(m => m.Name, StringComparer.Ordinal))
        {
            var nomeMetodo = (metodo.IsStatic ? "static " : "") + NomeMetodo(metodo) + Parametros(metodo, contexto);
            var retorno = Escapar(NomeTipo(metodo.ReturnType, contexto.Create(metodo.ReturnParameter)));
            texto.AppendLine($"| `{Escapar(nomeMetodo)}` | `{retorno}` | {Celula(DescricaoMetodo(IdMetodo(metodo), pagina.Arquivo))} |");
        }
        texto.AppendLine();
    }
}

static string Natureza(Type tipo)
{
    if (tipo.IsEnum) return "Enumeração";
    if (tipo.IsInterface) return "Interface";
    if (tipo.IsValueType) return "Estrutura";
    if (typeof(Exception).IsAssignableFrom(tipo)) return "Exceção";
    if (tipo.IsAbstract && tipo.IsSealed) return "Classe estática";
    if (tipo.IsAbstract) return "Classe abstrata";
    if (tipo.IsSealed) return "Classe selada";
    return "Classe";
}

static string Acesso(PropertyInfo propriedade)
{
    var partes = new List<string> { "get" };
    if (propriedade.SetMethod is { IsPublic: true } definir)
    {
        var init = definir.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.FullName == "System.Runtime.CompilerServices.IsExternalInit");
        partes.Add(init ? "init" : "set");
    }
    var obrigatoria = propriedade.GetCustomAttributesData().Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute");
    return string.Join("; ", partes) + (obrigatoria ? " (obrigatória)" : "");
}

static string NomeMetodo(MethodInfo metodo)
{
    var nome = metodo.Name switch
    {
        "op_Equality" => "operator ==",
        "op_Inequality" => "operator !=",
        "op_Implicit" => "implicit operator",
        "op_Explicit" => "explicit operator",
        _ => metodo.Name,
    };
    return metodo.IsGenericMethodDefinition ? nome + "<" + string.Join(", ", metodo.GetGenericArguments().Select(a => a.Name)) + ">" : nome;
}

string Parametros(MethodBase metodo, NullabilityInfoContext contexto)
{
    var partes = new List<string>();
    var extensao = metodo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
    foreach (var parametro in metodo.GetParameters())
    {
        var prefixo = parametro.Position == 0 && extensao ? "this " : "";
        if (parametro.IsDefined(typeof(ParamArrayAttribute), false) || parametro.GetCustomAttributesData().Any(a => a.AttributeType.Name == "ParamCollectionAttribute"))
            prefixo += "params ";
        var tipoParametro = parametro.ParameterType;
        if (tipoParametro.IsByRef)
        {
            prefixo += parametro.IsOut ? "out " : "ref ";
            tipoParametro = tipoParametro.GetElementType()!;
        }
        var texto = prefixo + NomeTipo(tipoParametro, contexto.Create(parametro)) + " " + parametro.Name;
        if (parametro.HasDefaultValue)
            texto += " = " + ValorPadrao(parametro);
        partes.Add(texto);
    }
    return "(" + string.Join(", ", partes) + ")";
}

static string ValorPadrao(ParameterInfo parametro)
{
    var valor = parametro.RawDefaultValue;
    var tipo = Nullable.GetUnderlyingType(parametro.ParameterType) ?? parametro.ParameterType;
    if (valor is null || valor is DBNull)
        return tipo.IsValueType && Nullable.GetUnderlyingType(parametro.ParameterType) is null ? "default" : "null";
    if (tipo.IsEnum)
        return tipo.Name + "." + (Enum.GetName(tipo, valor) ?? valor.ToString());
    return valor switch
    {
        bool logico => logico ? "true" : "false",
        string texto => "\"" + texto + "\"",
        IFormattable numero => numero.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
        _ => valor.ToString() ?? "",
    };
}

static string NomeCurto(Type tipo)
{
    var nome = tipo.Name;
    var crase = nome.IndexOf('`');
    if (crase >= 0)
        nome = nome[..crase] + "<" + string.Join(", ", tipo.GetGenericArguments().Select(a => a.Name)) + ">";
    return tipo.IsNested ? NomeCurto(tipo.DeclaringType!) + "." + nome : nome;
}

static string NomeTipo(Type tipo, NullabilityInfo? anulavel)
{
    if (Nullable.GetUnderlyingType(tipo) is { } subjacente)
        return NomeTipo(subjacente, null) + "?";
    string nome;
    if (tipo.IsArray)
        nome = NomeTipo(tipo.GetElementType()!, anulavel?.ElementType) + "[]";
    else if (tipo.IsGenericParameter)
        nome = tipo.Name;
    else if (tipo.IsGenericType)
    {
        var argumentos = tipo.GetGenericArguments();
        var anulaveis = anulavel?.GenericTypeArguments;
        var nomes = argumentos.Select((a, i) => NomeTipo(a, anulaveis is not null && i < anulaveis.Length ? anulaveis[i] : null));
        var baseNome = tipo.Name[..tipo.Name.IndexOf('`')];
        nome = baseNome + "<" + string.Join(", ", nomes) + ">";
    }
    else
    {
        nome = tipo.FullName switch
        {
            "System.String" => "string",
            "System.Int32" => "int",
            "System.Int64" => "long",
            "System.Boolean" => "bool",
            "System.Decimal" => "decimal",
            "System.Double" => "double",
            "System.Byte" => "byte",
            "System.Object" => "object",
            "System.Void" => "void",
            _ => tipo.Name,
        };
    }
    if (!tipo.IsValueType && anulavel?.ReadState == NullabilityState.Nullable)
        nome += "?";
    return nome;
}

string LinkTipo(Type tipo, string arquivoAtual)
{
    var definicao = tipo.IsGenericType ? tipo.GetGenericTypeDefinition() : tipo;
    var nome = NomeTipo(tipo, null);
    if (ondeTipo.TryGetValue(definicao, out var onde))
        return $"[`{nome}`]({(onde.Arquivo == arquivoAtual ? "" : onde.Arquivo)}#{onde.Ancora})";
    return $"`{nome}`";
}

// Os identificadores de documentação do compilador (ECMA-334, anexo D).
static string IdNome(Type tipo) => (tipo.IsGenericType ? tipo.GetGenericTypeDefinition() : tipo).FullName!.Replace('+', '.');

static string IdTipo(Type tipo) => "T:" + IdNome(tipo);

static string IdMetodo(MethodBase metodo)
{
    var nome = metodo is ConstructorInfo ? "#ctor" : metodo.Name;
    if (metodo is MethodInfo { IsGenericMethodDefinition: true } generico)
        nome += "``" + generico.GetGenericArguments().Length;
    var parametros = metodo.GetParameters();
    var texto = "M:" + IdNome(metodo.DeclaringType!) + "." + nome;
    if (parametros.Length > 0)
        texto += "(" + string.Join(",", parametros.Select(p => IdParametro(p.ParameterType))) + ")";
    return texto;
}

static string IdParametro(Type tipo)
{
    if (tipo.IsByRef)
        return IdParametro(tipo.GetElementType()!) + "@";
    if (tipo.IsArray)
        return IdParametro(tipo.GetElementType()!) + "[]";
    if (tipo.IsGenericParameter)
        return (tipo.DeclaringMethod is not null ? "``" : "`") + tipo.GenericParameterPosition;
    if (tipo.IsGenericType)
    {
        var definicao = tipo.GetGenericTypeDefinition().FullName!;
        return definicao[..definicao.IndexOf('`')].Replace('+', '.') + "{" + string.Join(",", tipo.GetGenericArguments().Select(IdParametro)) + "}";
    }
    return tipo.FullName!.Replace('+', '.');
}

string Resumo(string id, string arquivoAtual) =>
    documentacao.TryGetValue(id, out var membro) && membro.Element("summary") is { } resumo ? Texto(resumo, arquivoAtual) : "";

string Observacoes(string id, string arquivoAtual) =>
    documentacao.TryGetValue(id, out var membro) && membro.Element("remarks") is { } observacao ? Texto(observacao, arquivoAtual) : "";

string DescricaoMetodo(string id, string arquivoAtual)
{
    if (!documentacao.TryGetValue(id, out var membro))
        return "";
    var partes = new List<string>();
    if (membro.Element("summary") is { } resumo)
        partes.Add(Texto(resumo, arquivoAtual));
    if (membro.Element("returns") is { } retorno)
        partes.Add("**Retorna:** " + Texto(retorno, arquivoAtual));
    foreach (var excecao in membro.Elements("exception"))
    {
        var cref = excecao.Attribute("cref")?.Value ?? "";
        var nome = cref.Contains('.') ? cref[(cref.LastIndexOf('.') + 1)..] : cref.Replace("T:", "");
        partes.Add($"**`{nome}`:** " + Texto(excecao, arquivoAtual));
    }
    return string.Join(" ", partes);
}

// O XML da documentação em Markdown, numa linha (as tabelas não aceitam quebra).
string Texto(XElement elemento, string arquivoAtual)
{
    var texto = new StringBuilder();
    foreach (var no in elemento.Nodes())
        EscreverNo(texto, no, arquivoAtual);
    return Regex.Replace(texto.ToString(), @"\s+", " ").Trim();
}

void EscreverNo(StringBuilder texto, XNode no, string arquivoAtual)
{
    switch (no)
    {
        case XText parte:
            texto.Append(EscaparTexto(parte.Value));
            break;
        case XElement e when e.Name == "c":
            texto.Append('`').Append(e.Value.Replace("`", "'")).Append('`');
            break;
        case XElement e when e.Name == "code":
            texto.Append(" `").Append(Regex.Replace(e.Value, @"\s+", " ").Trim()).Append("` ");
            break;
        case XElement e when e.Name == "see" || e.Name == "seealso":
            if (e.Attribute("langword") is { } palavra)
                texto.Append('`').Append(palavra.Value).Append('`');
            else if (e.Attribute("href") is { } endereco)
                texto.Append($"[{(e.Value.Length > 0 ? e.Value : endereco.Value)}]({endereco.Value})");
            else if (e.Attribute("cref") is { } referencia)
                texto.Append(Referencia(referencia.Value, e.Value, arquivoAtual));
            break;
        case XElement e when e.Name == "paramref" || e.Name == "typeparamref":
            texto.Append('`').Append(e.Attribute("name")?.Value).Append('`');
            break;
        case XElement e when e.Name == "para":
            texto.Append(' ');
            foreach (var filho in e.Nodes())
                EscreverNo(texto, filho, arquivoAtual);
            texto.Append(' ');
            break;
        case XElement e when e.Name == "b":
            texto.Append("**").Append(EscaparTexto(e.Value)).Append("**");
            break;
        case XElement e when e.Name == "i":
            texto.Append('*').Append(EscaparTexto(e.Value)).Append('*');
            break;
        case XElement e when e.Name == "list":
            foreach (var item in e.Elements("item"))
            {
                texto.Append(" • ");
                foreach (var filho in item.Nodes())
                    EscreverNo(texto, filho, arquivoAtual);
            }
            break;
        case XElement e:
            foreach (var filho in e.Nodes())
                EscreverNo(texto, filho, arquivoAtual);
            break;
    }
}

string Referencia(string cref, string rotulo, string arquivoAtual)
{
    var semPrefixo = cref.Length > 2 && cref[1] == ':' ? cref[2..] : cref;
    var semParametros = semPrefixo.Split('(')[0];
    var membro = "";
    var nomeTipo = semParametros;
    if (cref.StartsWith("T:", StringComparison.Ordinal) == false && semParametros.Contains('.'))
    {
        membro = semParametros[(semParametros.LastIndexOf('.') + 1)..];
        nomeTipo = semParametros[..semParametros.LastIndexOf('.')];
    }
    var exibido = rotulo.Length > 0 ? rotulo : (membro.Length > 0 ? membro.Replace("#ctor", "construtor") : nomeTipo[(nomeTipo.LastIndexOf('.') + 1)..]);
    exibido = Regex.Replace(exibido, "``?\\d+", "");
    if (tipoNome.TryGetValue(nomeTipo, out var tipo) && ondeTipo.TryGetValue(tipo, out var onde))
        return $"[`{exibido}`]({(onde.Arquivo == arquivoAtual ? "" : onde.Arquivo)}#{onde.Ancora})";
    return $"`{exibido}`";
}

// Dentro do código (entre crases): só a barra vertical, que quebraria a tabela.
static string Escapar(string texto) => texto.Replace("|", "\\|");

// No texto corrido: também os sinais que o Markdown leria como HTML ou formatação.
static string EscaparTexto(string texto) =>
    texto.Replace("|", "\\|").Replace("<", "&lt;").Replace(">", "&gt;").Replace("*", "\\*").Replace("_", "\\_");

static string Celula(string texto) => texto.Length == 0 ? "" : texto;

// O identificador do título, como o GitHub e o kramdown (do GitHub Pages) o geram.
static string Ancora(string titulo)
{
    var texto = new StringBuilder();
    foreach (var letra in titulo.ToLowerInvariant())
    {
        if (char.IsLetterOrDigit(letra) || letra == '-' || letra == '_')
            texto.Append(letra);
        else if (letra == ' ')
            texto.Append('-');
    }
    return texto.ToString();
}

internal sealed record Pagina(string Arquivo, string Titulo, string Pacote, string Descricao, List<Type> Tipos, bool Modelo);
