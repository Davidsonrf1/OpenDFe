using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O levantamento do aviso do campo não suportado (PROJETO.md 4.4): toda propriedade com [Tag] do modelo é escrita pelo
// leiaute 4.00, no elemento (ou atributo) da tag, dentro do grupo do pai, ou está em CamposNaoSuportados.Lista. As notas
// são preenchidas por reflexão, cada campo com um valor qualquer (não são válidas no schema: o que se confere é o lugar),
// e as escolhas por subclasse giram de nota em nota, até todas as subclasses passarem.
public class CoberturaEscritaTestes
{
    private static readonly XNamespace Ns = EscritaNFe.Namespace;

    private static readonly Assembly Modelo = typeof(NotaFiscal).Assembly;

    private static readonly Lazy<XmlSchemaType> TipoNFe = new(Compilar);

    [Fact]
    public void OLeiauteEscreveTodoCampoDoModelo()
    {
        var vistas = new HashSet<(Type, string)>();
        var rodadas = Modelo.GetTypes().Where(t => t.IsAbstract && t.IsClass && t.Namespace == "OpenDFe.NFe")
            .Max(b => Subclasses(b).Count);
        for (var rodada = 0; rodada < rodadas; rodada++)
        {
            var nota = (NotaFiscal)Preencher(typeof(NotaFiscal), rodada);
            var escrita = EscritorNFe.Escrever(nota, NotasTeste.Nucleo(Documento.NFe, TipoEmissao.ContingenciaSvcAN,
                new Contingencia(NotasTeste.Emissao, "SEFAZ FORA DO AR DESDE AS 14H")));
            var raiz = XElement.Parse(escrita.Xml);
            Assert.Equal(Ns + "NFe", raiz.Name);
            ConferirOrdem(raiz, TipoNFe.Value, "NFe");
            var infNFe = raiz.Element(Ns + "infNFe")!;
            Conferir(nota, infNFe, raiz, "", vistas);
        }

        var faltam = Modelo.GetTypes()
            .Where(t => t.IsClass && t.IsPublic)
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(p => p.GetCustomAttribute<TagAttribute>() is not null)
            .Where(p => !vistas.Contains((p.DeclaringType!, p.Name)))
            .Where(p => !CamposNaoSuportados.Lista.Any(n => n.DeclaringType == p.DeclaringType && n.Name == p.Name))
            .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
            .ToList();
        Assert.True(faltam.Count == 0, "Campos do modelo que o leiaute não escreve: " + string.Join(", ", faltam));
    }

    [Fact]
    public void NoLeiaute400NenhumCampoFicaDeFora() => Assert.Empty(CamposNaoSuportados.Lista);

    // A propriedade, com o valor que tem, aparece no XML do grupo: o escalar como elemento (ou atributo) com a tag; o
    // grupo como elemento, conferido por dentro; a lista com um elemento por item, na ordem.
    private static void Conferir(object grupo, XElement elemento, XElement raiz, string caminho, HashSet<(Type, string)> vistas)
    {
        foreach (var propriedade in grupo.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var tag = propriedade.GetCustomAttribute<TagAttribute>()?.Tag;
            if (tag is null)
                continue;
            var valor = propriedade.GetValue(grupo);
            if (valor is null)
                continue;
            var onde = $"{caminho}.{propriedade.Name}";
            vistas.Add((propriedade.DeclaringType!, propriedade.Name));

            // O infNFeSupl é irmão do infNFe, não filho.
            var pai = grupo is NotaFiscal && tag == "infNFeSupl" ? raiz : elemento;
            if (valor is IList lista)
            {
                var filhos = pai.Elements(Ns + tag).ToList();
                Assert.True(filhos.Count == lista.Count, $"{onde}: {lista.Count} itens, {filhos.Count} elementos {tag}.");
                for (var i = 0; i < lista.Count; i++)
                {
                    if (lista[i] is string texto)
                        Assert.Equal(texto, filhos[i].Value);
                    else
                        ConferirGrupo(lista[i]!, filhos[i], raiz, $"{onde}[{i}]", vistas);
                }
                continue;
            }
            if (valor.GetType().Assembly == Modelo && valor.GetType().IsClass)
            {
                var filho = pai.Element(Ns + tag);
                Assert.True(filho is not null, $"{onde}: falta o elemento {tag}.");
                ConferirGrupo(valor, filho!, raiz, onde, vistas);
                continue;
            }
            var presente = pai.Element(Ns + tag) is { } e ? e.Value : pai.Attribute(tag)?.Value;
            Assert.True(!string.IsNullOrEmpty(presente), $"{onde}: falta a tag {tag} com o valor.");
        }
    }

    // A subclasse com [Tag] na classe é um grupo dentro do elemento da escolha (ICMS/ICMS00); a sem tag (refNFe) tem os
    // campos direto nele.
    private static void ConferirGrupo(object grupo, XElement elemento, XElement raiz, string caminho, HashSet<(Type, string)> vistas)
    {
        var tagClasse = grupo.GetType().BaseType!.IsAbstract ? grupo.GetType().GetCustomAttribute<TagAttribute>(false)?.Tag : null;
        if (tagClasse is not null)
        {
            var dentro = elemento.Element(Ns + tagClasse);
            Assert.True(dentro is not null, $"{caminho}: falta o grupo {tagClasse}.");
            elemento = dentro!;
        }
        Conferir(grupo, elemento, raiz, caminho, vistas);
    }

    // Um valor para cada propriedade: texto, número, data, o primeiro membro da enumeração, a UF; cada grupo
    // instanciado, cada lista com um item; na escolha por subclasse, a da rodada.
    private static object Preencher(Type tipo, int rodada)
    {
        if (tipo.IsAbstract)
        {
            var subclasses = Subclasses(tipo);
            tipo = subclasses[rodada % subclasses.Count];
        }
        var grupo = Activator.CreateInstance(tipo)!;
        foreach (var propriedade in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (propriedade.GetCustomAttribute<TagAttribute>() is null)
                continue;
            var t = propriedade.PropertyType;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var lista = (IList)propriedade.GetValue(grupo)!;
                var item = t.GetGenericArguments()[0];
                lista.Add(item == typeof(string) ? "1" : Preencher(item, rodada));
                continue;
            }
            propriedade.SetValue(grupo, Valor(t, rodada));
        }
        return grupo;
    }

    private static object Valor(Type tipo, int rodada)
    {
        var t = Nullable.GetUnderlyingType(tipo) ?? tipo;
        if (t == typeof(string))
            return "X";
        if (t == typeof(decimal))
            return 1.5m;
        if (t == typeof(int))
            return 7;
        if (t == typeof(long))
            return 7L;
        if (t == typeof(DateTimeOffset))
            return NotasTeste.Emissao;
        if (t == typeof(DateOnly))
            return new DateOnly(2026, 10, 20);
        if (t == typeof(UF))
            return UF.MG;
        if (t.IsEnum)
            return Enum.GetValues(t).GetValue(0)!;
        if (t.Assembly == Modelo && t.IsClass)
            return Preencher(t, rodada);
        throw new InvalidOperationException($"Tipo sem valor de teste: {t}.");
    }

    // A ordem dos filhos de cada elemento segue a do XSD (as notas da cobertura não são válidas, mas a ordem é a do
    // schema; o grupo que nenhuma nota válida usa também fica conferido). Cada filho tem de vir na mesma posição ou
    // depois do anterior, na lista dos elementos do tipo em ordem.
    private static void ConferirOrdem(XElement elemento, XmlSchemaType tipo, string caminho)
    {
        var declarados = new List<XmlSchemaElement>();
        if (tipo is XmlSchemaComplexType complexo)
            Declarados(complexo.ContentTypeParticle, declarados);
        var anterior = -1;
        foreach (var filho in elemento.Elements())
        {
            var posicao = declarados.FindIndex(d => d.QualifiedName.Name == filho.Name.LocalName);
            Assert.True(posicao >= 0, $"{caminho}: {filho.Name.LocalName} não é filho de {elemento.Name.LocalName} no XSD.");
            Assert.True(posicao >= anterior, $"{caminho}: {filho.Name.LocalName} fora da ordem do XSD.");
            anterior = posicao;
            if (filho.Name.LocalName != "Signature")
                ConferirOrdem(filho, declarados[posicao].ElementSchemaType!, caminho + "/" + filho.Name.LocalName);
        }
    }

    private static void Declarados(XmlSchemaParticle? particula, List<XmlSchemaElement> declarados)
    {
        switch (particula)
        {
            case XmlSchemaElement el:
                if (!declarados.Any(d => d.QualifiedName == el.QualifiedName))
                    declarados.Add(el);
                break;
            case XmlSchemaGroupBase grupo:
                foreach (XmlSchemaParticle item in grupo.Items)
                    Declarados(item, declarados);
                break;
        }
    }

    private static XmlSchemaType Compilar()
    {
        var pasta = Directory.CreateTempSubdirectory("OpenDFe.Cobertura.");
        var assembly = typeof(LeiauteNFe400).Assembly;
        foreach (var nome in assembly.GetManifestResourceNames())
        {
            using var origem = assembly.GetManifestResourceStream(nome)!;
            using var destino = File.Create(Path.Combine(pasta.FullName, Path.GetFileName(nome.Replace('\\', '/'))));
            origem.CopyTo(destino);
        }
        var schemas = new XmlSchemaSet { XmlResolver = new XmlUrlResolver() };
        schemas.Add(EscritaNFe.Namespace, Path.Combine(pasta.FullName, "nfe_v4.00.xsd"));
        schemas.Compile();
        return ((XmlSchemaElement)schemas.GlobalElements[new XmlQualifiedName("NFe", EscritaNFe.Namespace)]!).ElementSchemaType!;
    }

    private static List<Type> Subclasses(Type baseTipo) =>
        [.. Modelo.GetTypes().Where(t => t.IsSubclassOf(baseTipo) && !t.IsAbstract).OrderBy(t => t.Name, StringComparer.Ordinal)];
}
