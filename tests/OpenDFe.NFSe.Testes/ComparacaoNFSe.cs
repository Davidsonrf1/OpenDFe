using System.Collections;
using System.Reflection;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// A comparação do modelo da NFS-e por reflexão, para a ida e volta (PROJETO.md 13.2): cada propriedade com [Tag], grupo a
// grupo, com o tipo da subclasse do evento. Junta as propriedades comparadas com valor, para a cobertura.
internal static class ComparacaoNFSe
{
    private static readonly Assembly Modelo = typeof(Dps).Assembly;

    internal static List<string> Diferencas(object esperado, object lido, HashSet<(Type, string)>? comparadas = null)
    {
        var diferencas = new List<string>();
        Comparar(esperado, lido, "", diferencas, comparadas);
        return diferencas;
    }

    private static void Comparar(object esperado, object lido, string caminho, List<string> diferencas, HashSet<(Type, string)>? comparadas)
    {
        if (esperado.GetType() != lido.GetType())
        {
            diferencas.Add($"{caminho}: {esperado.GetType().Name} e {lido.GetType().Name}.");
            return;
        }
        foreach (var propriedade in esperado.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (propriedade.GetCustomAttribute<TagAttribute>() is null)
                continue;
            var onde = caminho.Length == 0 ? propriedade.Name : $"{caminho}.{propriedade.Name}";
            var a = propriedade.GetValue(esperado);
            var b = propriedade.GetValue(lido);
            if (a is null || b is null)
            {
                if (a is not null || b is not null)
                    diferencas.Add($"{onde}: {a ?? "nulo"} e {b ?? "nulo"}.");
                continue;
            }
            if (a is IList listaA && b is IList listaB)
            {
                if (listaA.Count > 0)
                    comparadas?.Add((propriedade.DeclaringType!, propriedade.Name));
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
                        Comparar(listaA[i]!, listaB[i]!, $"{onde}[{i}]", diferencas, comparadas);
                    }
                }
                continue;
            }
            comparadas?.Add((propriedade.DeclaringType!, propriedade.Name));
            if (a.GetType().Assembly == Modelo && a.GetType().IsClass)
            {
                Comparar(a, b, onde, diferencas, comparadas);
                continue;
            }
            if (!Equals(a, b))
                diferencas.Add($"{onde}: {a} e {b}.");
        }
    }
}
