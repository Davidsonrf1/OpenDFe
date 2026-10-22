using System.Collections;
using System.Reflection;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A comparação do modelo por reflexão, para a ida e volta da leitura (PROJETO.md 13.2): cada propriedade com [Tag] (e a
// lista sem tag, as mensagens do protocolo), grupo a grupo, com o tipo da subclasse. Junta as propriedades comparadas
// com valor, para a cobertura.
internal static class ComparacaoModelo
{
    private static readonly Assembly Modelo = typeof(NotaFiscal).Assembly;

    // As propriedades que a escrita completa (os campos do núcleo, o cNF e o CST do grupo de um valor só): na ida e
    // volta, a nota lida pode tê-las preenchidas onde a original estava nula.
    internal static readonly string[] CompletadasEscrita =
    [
        "Identificacao.UF", "Identificacao.Modelo", "Identificacao.TipoEmissao", "Identificacao.Ambiente",
        "Identificacao.InicioContingencia", "Identificacao.JustificativaContingencia", "Identificacao.CodigoNumerico",
    ];

    internal static List<string> Diferencas(object esperado, object lido, HashSet<(Type, string)>? comparadas = null,
        Func<string, bool>? ignorar = null)
    {
        var diferencas = new List<string>();
        Comparar(esperado, lido, "", diferencas, comparadas, ignorar ?? (_ => false));
        return diferencas;
    }

    private static void Comparar(object esperado, object lido, string caminho, List<string> diferencas,
        HashSet<(Type, string)>? comparadas, Func<string, bool> ignorar)
    {
        if (esperado.GetType() != lido.GetType())
        {
            diferencas.Add($"{caminho}: {esperado.GetType().Name} e {lido.GetType().Name}.");
            return;
        }
        foreach (var propriedade in esperado.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var lista = propriedade.PropertyType.IsGenericType && propriedade.PropertyType.GetGenericTypeDefinition() == typeof(IList<>);
            if (propriedade.GetCustomAttribute<TagAttribute>() is null && !lista && propriedade.Name != "Exterior")
                continue;
            var onde = caminho.Length == 0 ? propriedade.Name : $"{caminho}.{propriedade.Name}";
            var a = propriedade.GetValue(esperado);
            var b = propriedade.GetValue(lido);
            if (ignorar(onde) || (a is null && b is not null && Completada(onde)))
                continue;
            if (a is null || b is null)
            {
                if (a is not null || b is not null)
                    diferencas.Add($"{onde}: {a ?? "nulo"} e {b ?? "nulo"}.");
                continue;
            }
            comparadas?.Add((propriedade.DeclaringType!, propriedade.Name));
            if (a is IList listaA && b is IList listaB)
            {
                if (listaA.Count != listaB.Count)
                {
                    diferencas.Add($"{onde}: {listaA.Count} e {listaB.Count} itens.");
                    continue;
                }
                for (var i = 0; i < listaA.Count; i++)
                {
                    if (listaA[i] is string texto)
                    {
                        if (!Equals(texto, listaB[i]))
                            diferencas.Add($"{onde}[{i}]: {texto} e {listaB[i]}.");
                    }
                    else
                    {
                        Comparar(listaA[i]!, listaB[i]!, $"{onde}[{i}]", diferencas, comparadas, ignorar);
                    }
                }
                continue;
            }
            if (a.GetType().Assembly == Modelo && a.GetType().IsClass)
            {
                Comparar(a, b, onde, diferencas, comparadas, ignorar);
                continue;
            }
            if (!Equals(a, b))
                diferencas.Add($"{onde}: {a} e {b}.");
        }
    }

    private static bool Completada(string caminho) =>
        CompletadasEscrita.Contains(caminho) || caminho.EndsWith(".Cst", StringComparison.Ordinal) || caminho.EndsWith(".Csosn", StringComparison.Ordinal);
}
