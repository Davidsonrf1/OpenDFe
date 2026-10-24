namespace OpenDFe.Impressao;

/// <summary>O documento de impressão: o papel e a lista de elementos, de cima para baixo.</summary>
/// <remarks>
/// Imutável: a lista passada é copiada. O modelo é neutro (não conhece NF-e) e estrutural, sem fonte nem coordenada: quem
/// monta o documento formata os textos (data, dinheiro, CNPJ) e quem renderiza só desenha (docs/CONTRATOS.md 20).
/// </remarks>
public sealed class DocumentoImpressao
{
    /// <summary>Cria o documento.</summary>
    /// <param name="papel">O papel.</param>
    /// <param name="elementos">Os elementos, de cima para baixo (a lista é copiada).</param>
    /// <exception cref="ArgumentNullException">O papel, a lista ou um elemento é nulo.</exception>
    public DocumentoImpressao(PapelImpressao papel, IReadOnlyList<ElementoImpressao> elementos)
    {
        ArgumentNullException.ThrowIfNull(papel);
        ArgumentNullException.ThrowIfNull(elementos);
        var copia = new ElementoImpressao[elementos.Count];
        for (var i = 0; i < copia.Length; i++)
            copia[i] = elementos[i] ?? throw new ArgumentNullException(nameof(elementos), "A lista tem um elemento nulo.");

        Papel = papel;
        Elementos = Array.AsReadOnly(copia);
    }

    /// <summary>O papel.</summary>
    public PapelImpressao Papel { get; }

    /// <summary>Os elementos, de cima para baixo.</summary>
    public IReadOnlyList<ElementoImpressao> Elementos { get; }

    /// <summary>Metadado: o título do documento (o título do PDF); <see langword="null"/> sem título.</summary>
    public string? Titulo { get; init; }
}
