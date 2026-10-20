using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace OpenDFe.NFe;

/// <summary>
/// O aviso do campo que o leiaute não escreve (PROJETO.md 4.4): o modelo vale para todas as versões do leiaute, e um
/// campo de uma versão que este leiaute não conhece, preenchido, fica fora do XML. Nunca some calado: vira aviso, com a
/// propriedade.
/// </summary>
/// <remarks>
/// O levantamento é o teste do leiaute (<c>EscritorNFeTestes</c>): toda propriedade com <see cref="TagAttribute"/> do
/// modelo ou é escrita pelo <see cref="EscritorNFe"/>, ou está em <see cref="Lista"/>. No leiaute 4.00 o modelo inteiro
/// é escrito, e a lista é vazia; a propriedade nova do modelo que este leiaute não escrever entra nela.
/// </remarks>
internal static class CamposNaoSuportados
{
    /// <summary>As propriedades do modelo que o leiaute 4.00 não escreve.</summary>
    internal static readonly IReadOnlyCollection<PropertyInfo> Lista = [];

    private static readonly Assembly Modelo = typeof(NotaFiscal).Assembly;

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Propriedades = new();

    /// <summary>Um aviso para cada campo de <paramref name="naoSuportados"/> preenchido na nota.</summary>
    internal static void Conferir(NotaFiscal nota, IReadOnlyCollection<PropertyInfo> naoSuportados, List<Problema> problemas)
    {
        if (naoSuportados.Count > 0)
            Percorrer(nota, null, naoSuportados, problemas);
    }

    private static void Percorrer(object grupo, string? caminho, IReadOnlyCollection<PropertyInfo> naoSuportados, List<Problema> problemas)
    {
        foreach (var propriedade in Propriedades.GetOrAdd(grupo.GetType(), Levantar))
        {
            var valor = propriedade.GetValue(grupo);
            if (valor is null)
                continue;
            var nome = caminho is null ? propriedade.Name : caminho + "." + propriedade.Name;
            if (naoSuportados.Any(n => n.DeclaringType == propriedade.DeclaringType && n.Name == propriedade.Name))
            {
                if (Preenchido(valor))
                    problemas.Add(new Problema(CodigosEscrita.NaoSuportado, Severidade.Aviso,
                        $"O campo {propriedade.GetCustomAttribute<TagAttribute>()!.Tag} não existe no leiaute 4.00 e ficou fora do XML.")
                    {
                        Tag = propriedade.GetCustomAttribute<TagAttribute>()!.Tag,
                        Propriedade = nome,
                    });
                continue;
            }
            if (valor is IList lista)
            {
                for (var i = 0; i < lista.Count; i++)
                    if (lista[i] is { } item && item.GetType().Assembly == Modelo)
                        Percorrer(item, string.Create(CultureInfo.InvariantCulture, $"{nome}[{i}]"), naoSuportados, problemas);
            }
            else if (valor.GetType().Assembly == Modelo && valor.GetType().IsClass)
                Percorrer(valor, nome, naoSuportados, problemas);
        }
    }

    private static bool Preenchido(object valor) => valor switch
    {
        string texto => texto.Length > 0,
        ICollection colecao => colecao.Count > 0,
        _ => true,
    };

    private static PropertyInfo[] Levantar(Type tipo) =>
        [.. tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<TagAttribute>() is not null)];
}
