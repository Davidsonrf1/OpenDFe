using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace OpenDFe.NFe;

/// <summary>
/// A conversão do caminho de um problema no XML na propriedade do modelo (docs/MODELO.md 4, 11.10 e 14):
/// <c>NFe/infNFe/det[3]/prod/vProd</c> → <c>Itens[2].Produto.Valor</c>. É a que o <see cref="SchemasNFe400"/> passa
/// ao <see cref="SchemaXml"/>, para o problema do schema apontar a propriedade.
/// </summary>
/// <remarks>
/// <para>
/// Só olha os tipos, pelo <see cref="TagAttribute"/>, sem a instância; o levantamento de cada tipo é feito uma vez e
/// guardado. A raiz é o <c>NFe</c> (a <see cref="NotaFiscal"/>, com o <c>infNFe</c> transparente) ou o <c>nfeProc</c>
/// (o <c>NFe</c> igual, e o <c>protNFe/infProt</c> como <c>Protocolo</c>, como a leitura aponta).
/// </para>
/// <para>
/// Cada passo acha, no tipo atual, a propriedade com aquela tag (o atributo pela tag sem <c>@</c>); o que não é
/// propriedade do tipo procura entre as subclasses dele (a escolha: o <c>[Tag]</c> na classe troca o tipo sem gerar
/// passo; a propriedade da subclasse troca o tipo e gera o passo) e, por fim, na lista sem <c>[Tag]</c> de uma
/// sequência repetida sem elemento (o <c>cMsg</c> e o <c>xMsg</c> do protocolo). Na lista, o índice do XML (a partir
/// de 1) vira o do modelo (a partir de 0), e sem índice é o primeiro; fora da lista, só o índice 1 é aceito. O
/// caminho converte inteiro ou dá nulo (decisão 3 da 14.3).
/// </para>
/// </remarks>
internal static class CaminhoModeloNFe
{
    private static readonly ConcurrentDictionary<Type, Levantamento> Tipos = new();

    /// <summary>A propriedade do modelo do caminho no XML; nula quando o caminho não tem propriedade.</summary>
    internal static string? Propriedade(string caminho)
    {
        var passos = caminho.Split('/');
        int inicio;
        Type tipo;
        var partes = new List<string>();
        if (Raiz(passos, 0, "nfeProc") && passos.Length > 1)
        {
            if (Raiz(passos, 1, "NFe"))
            {
                (tipo, inicio) = (typeof(NotaFiscal), 2);
            }
            else if (Raiz(passos, 1, "protNFe"))
            {
                partes.Add("Protocolo");
                if (passos.Length == 2)
                    return "Protocolo";
                if (!Raiz(passos, 2, "infProt"))
                    return null;
                (tipo, inicio) = (typeof(Protocolo), 3);
            }
            else
            {
                return null;
            }
        }
        else if (Raiz(passos, 0, "NFe"))
        {
            (tipo, inicio) = (typeof(NotaFiscal), 1);
        }
        else
        {
            return null;
        }

        if (tipo == typeof(NotaFiscal) && inicio < passos.Length && Raiz(passos, inicio, "infNFe"))
            inicio++;

        for (var i = inicio; i < passos.Length; i++)
        {
            if (!Ler(passos[i], out var nome, out var indice, out var atributo))
                return null;
            var levantamento = Tipos.GetOrAdd(tipo, Levantar);

            if (levantamento.Propriedades.TryGetValue(nome, out var propriedade))
            {
                if (!Passo(propriedade, indice, atributo, partes, out tipo))
                    return null;
                continue;
            }
            if (atributo)
                return null;

            if (levantamento.Escolhas.TryGetValue(nome, out var subclasse))
            {
                if (indice is not (null or 1))
                    return null;
                tipo = subclasse;
                continue;
            }
            if (levantamento.ValoresEscolha.TryGetValue(nome, out var valor))
            {
                if (!Passo(valor, indice, atributo: false, partes, out tipo))
                    return null;
                continue;
            }
            if (levantamento.Sequencias.TryGetValue(nome, out var sequencia))
            {
                // cMsg[n] e xMsg[n]: a n-ésima mensagem; o índice é o do elemento, que se repete com a sequência
                partes.Add(Indexado(sequencia.Lista.Name, (indice ?? 1) - 1));
                if (!Passo(sequencia.Campo, null, atributo: false, partes, out tipo))
                    return null;
                continue;
            }
            return null;
        }

        return partes.Count == 0 ? null : string.Join('.', partes);
    }

    private static bool Raiz(string[] passos, int posicao, string nome) =>
        passos[posicao] == nome || passos[posicao] == nome + "[1]";

    // Um passo do caminho: o nome, o índice (nulo sem índice) e se é atributo.
    private static bool Ler(string passo, out string nome, out int? indice, out bool atributo)
    {
        atributo = passo.StartsWith('@');
        nome = atributo ? passo[1..] : passo;
        indice = null;
        var abre = nome.IndexOf('[', StringComparison.Ordinal);
        if (abre < 0)
            return nome.Length > 0;
        if (atributo || !nome.EndsWith(']')
            || !int.TryParse(nome.AsSpan(abre + 1, nome.Length - abre - 2), NumberStyles.None, CultureInfo.InvariantCulture, out var numero)
            || numero < 1)
            return false;
        indice = numero;
        nome = nome[..abre];
        return nome.Length > 0;
    }

    private static bool Passo(PropertyInfo propriedade, int? indice, bool atributo, List<string> partes, out Type tipo)
    {
        if (Elemento(propriedade.PropertyType) is { } elemento && !atributo)
        {
            partes.Add(Indexado(propriedade.Name, (indice ?? 1) - 1));
            tipo = elemento;
            return true;
        }
        tipo = Nullable.GetUnderlyingType(propriedade.PropertyType) ?? propriedade.PropertyType;
        if (indice is not (null or 1))
            return false;
        partes.Add(propriedade.Name);
        return true;
    }

    private static string Indexado(string nome, int indice) =>
        string.Create(CultureInfo.InvariantCulture, $"{nome}[{indice}]");

    private static Type? Elemento(Type tipo) =>
        tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(IList<>) ? tipo.GetGenericArguments()[0] : null;

    private static Levantamento Levantar(Type tipo)
    {
        var publicas = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propriedades = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
        foreach (var propriedade in publicas)
            if (propriedade.GetCustomAttribute<TagAttribute>() is { } tag)
                propriedades.Add(tag.Tag, propriedade);

        var escolhas = new Dictionary<string, Type>(StringComparer.Ordinal);
        var valoresEscolha = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
        if (tipo.IsAbstract)
        {
            foreach (var subclasse in tipo.Assembly.GetTypes().Where(t => t.IsSubclassOf(tipo) && !t.IsAbstract))
            {
                if (subclasse.GetCustomAttribute<TagAttribute>(inherit: false) is { } tagClasse)
                {
                    escolhas.Add(tagClasse.Tag, subclasse);
                    continue;
                }
                // a escolha de um valor só (refNFe): a propriedade própria da subclasse
                foreach (var propriedade in subclasse.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    if (propriedade.GetCustomAttribute<TagAttribute>() is { } tag)
                        valoresEscolha.Add(tag.Tag, propriedade);
            }
        }

        var sequencias = new Dictionary<string, (PropertyInfo, PropertyInfo)>(StringComparer.Ordinal);
        foreach (var lista in publicas)
        {
            if (lista.GetCustomAttribute<TagAttribute>() is not null || Elemento(lista.PropertyType) is not { } elemento)
                continue;
            foreach (var campo in elemento.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (campo.GetCustomAttribute<TagAttribute>() is { } tag)
                    sequencias.Add(tag.Tag, (lista, campo));
        }

        return new Levantamento(propriedades, escolhas, valoresEscolha, sequencias);
    }

    private sealed record Levantamento(
        Dictionary<string, PropertyInfo> Propriedades,
        Dictionary<string, Type> Escolhas,
        Dictionary<string, PropertyInfo> ValoresEscolha,
        Dictionary<string, (PropertyInfo Lista, PropertyInfo Campo)> Sequencias);
}
