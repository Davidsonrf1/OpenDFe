namespace OpenDFe.Impressao;

/// <summary>
/// As fichas de paginação que o texto de um elemento pode levar (<c>FOLHA {pagina}/{paginas}</c> no DANFE) e a troca delas
/// pelos números de cada página.
/// </summary>
/// <remarks>
/// <para>
/// O modelo é estrutural e não sabe em quantas páginas o documento será cortado: quem monta o documento escreve a ficha
/// <see cref="Pagina"/> onde vai o número da página e <see cref="Paginas"/> onde vai o total, e quem renderiza chama
/// <see cref="Resolver"/> com os números de cada página, depois de cortar o documento. Vale para o texto de um
/// <see cref="TextoImpressao"/>, o valor de um <see cref="CampoImpressao"/> e o texto de uma célula, de uma linha ou de
/// uma tabela, incluindo o <see cref="DocumentoImpressao.CabecalhoContinuacao"/>. Um renderizador que não pagina deixa o
/// texto como está ou resolve com a página 1 de 1.
/// </para>
/// <para>Sem estado, seguro em paralelo (docs/CONTRATOS.md 21).</para>
/// </remarks>
public static class TextoPaginado
{
    /// <summary>A ficha do número da página: <c>{pagina}</c>.</summary>
    public const string Pagina = "{pagina}";

    /// <summary>A ficha do total de páginas: <c>{paginas}</c>.</summary>
    public const string Paginas = "{paginas}";

    /// <summary>Troca as fichas pelo número da página e pelo total, em algarismos sem zeros à esquerda e sem cultura.</summary>
    /// <param name="texto">O texto, com ou sem as fichas.</param>
    /// <param name="pagina">O número da página, a partir de 1.</param>
    /// <param name="paginas">O total de páginas, ao menos o número da página.</param>
    /// <returns>O texto com cada ficha trocada; o texto sem fichas volta igual.</returns>
    /// <exception cref="ArgumentNullException">O texto é nulo.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A página é menor que 1, ou o total é menor que a página.</exception>
    public static string Resolver(string texto, int pagina, int paginas)
    {
        ArgumentNullException.ThrowIfNull(texto);
        ArgumentOutOfRangeException.ThrowIfLessThan(pagina, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(paginas, pagina);
        return texto
            .Replace(Pagina, pagina.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace(Paginas, paginas.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}
