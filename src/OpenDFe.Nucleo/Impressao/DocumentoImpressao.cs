namespace OpenDFe.Impressao;

/// <summary>O documento de impressão: o papel e a lista de elementos, de cima para baixo.</summary>
/// <remarks>
/// Imutável: a lista passada é copiada. O modelo é neutro (não conhece NF-e) e estrutural, sem fonte nem coordenada: quem
/// monta o documento formata os textos (data, dinheiro, CNPJ) e quem renderiza só desenha (docs/CONTRATOS.md 20).
/// O <see cref="CabecalhoContinuacao"/> e a <see cref="MarcaAgua"/> (docs/CONTRATOS.md 21) são opcionais para quem renderiza:
/// um renderizador que os ignora ainda gera um documento válido.
/// </remarks>
public sealed class DocumentoImpressao
{
    private IReadOnlyList<ElementoImpressao> _cabecalhoContinuacao = Array.Empty<ElementoImpressao>();
    private string? _marcaAgua;

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

    /// <summary>
    /// O que se repete no topo das páginas seguintes à primeira, depois do corte do documento em páginas (por exemplo, no
    /// DANFE, o quadro do emitente resumido e a chave, Anexo II 3.5); vazio quando as páginas seguintes não repetem nada. A
    /// lista é copiada. Os textos podem levar as fichas de <see cref="TextoPaginado"/> (<c>FOLHA 2/3</c>).
    /// </summary>
    /// <exception cref="ArgumentNullException">A lista ou um elemento é nulo.</exception>
    public IReadOnlyList<ElementoImpressao> CabecalhoContinuacao
    {
        get => _cabecalhoContinuacao;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            var copia = new ElementoImpressao[value.Count];
            for (var i = 0; i < copia.Length; i++)
                copia[i] = value[i] ?? throw new ArgumentNullException(nameof(value), "A lista tem um elemento nulo.");
            _cabecalhoContinuacao = Array.AsReadOnly(copia);
        }
    }

    /// <summary>
    /// O texto em diagonal, em todas as páginas, atrás do conteúdo (<c>CANCELADA</c>, <c>SEM VALOR FISCAL</c>), desde que não
    /// prejudique a legibilidade; <see langword="null"/> sem marca.
    /// </summary>
    /// <exception cref="ArgumentException">O texto é vazio ou só espaços.</exception>
    public string? MarcaAgua
    {
        get => _marcaAgua;
        init
        {
            if (value is not null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A marca d'água não pode ser vazia.", nameof(value));
            _marcaAgua = value;
        }
    }
}
