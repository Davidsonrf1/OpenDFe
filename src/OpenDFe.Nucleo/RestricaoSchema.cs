namespace OpenDFe;

/// <summary>Uma restrição de um tipo simples do schema que o valor violou (<see cref="FalhaSchema.Restricoes"/>).</summary>
public sealed class RestricaoSchema
{
    internal RestricaoSchema(FacetaSchema faceta, IReadOnlyList<string> valores, string? tipo)
    {
        Faceta = faceta;
        Valores = valores;
        Tipo = tipo;
    }

    /// <summary>A restrição.</summary>
    public FacetaSchema Faceta { get; }

    /// <summary>
    /// O valor da restrição, como está no schema: um item (<c>"60"</c> no tamanho máximo, a expressão no padrão,
    /// o nome do tipo primitivo, <c>"decimal"</c>), ou todos os aceitos na enumeração e todos os padrões
    /// alternativos do mesmo tipo (basta atender um).
    /// </summary>
    public IReadOnlyList<string> Valores { get; }

    /// <summary>
    /// O nome do tipo do schema que declara a restrição (<c>"TString"</c>, quando herdada pelo tipo do campo);
    /// <see langword="null"/> se ela está num tipo anônimo, declarado dentro do elemento.
    /// </summary>
    public string? Tipo { get; }
}
