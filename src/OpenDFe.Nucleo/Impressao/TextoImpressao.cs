namespace OpenDFe.Impressao;

/// <summary>O alinhamento do texto na largura disponível.</summary>
public enum AlinhamentoImpressao
{
    /// <summary>À esquerda.</summary>
    Esquerda,

    /// <summary>Centralizado.</summary>
    Centro,

    /// <summary>À direita.</summary>
    Direita,
}

/// <summary>O tamanho do texto.</summary>
public enum TamanhoTexto
{
    /// <summary>Menor que o normal (a fonte B da térmica).</summary>
    Pequeno,

    /// <summary>O normal (a fonte A da térmica).</summary>
    Normal,

    /// <summary>O dobro do normal em largura e altura.</summary>
    Duplo,
}

/// <summary>Um texto, que o renderizador quebra na largura do papel.</summary>
/// <remarks>Imutável.</remarks>
public sealed class TextoImpressao : ElementoImpressao
{
    /// <summary>Cria o texto.</summary>
    /// <param name="texto">O texto, já formatado; sem quebra de linha obrigatória, e <c>\n</c> é quebra explícita.</param>
    /// <exception cref="ArgumentNullException">O texto é nulo.</exception>
    public TextoImpressao(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        Texto = texto;
    }

    /// <summary>O texto (<c>\n</c> é quebra explícita).</summary>
    public string Texto { get; }

    /// <summary>O alinhamento; à esquerda por padrão.</summary>
    public AlinhamentoImpressao Alinhamento { get; init; } = AlinhamentoImpressao.Esquerda;

    /// <summary>Se está em negrito.</summary>
    public bool Negrito { get; init; }

    /// <summary>O tamanho; <see cref="TamanhoTexto.Normal"/> por padrão.</summary>
    public TamanhoTexto Tamanho { get; init; } = TamanhoTexto.Normal;

    /// <summary>Se é texto claro em fundo preto (o destaque de um título).</summary>
    public bool Invertido { get; init; }

    /// <summary>
    /// Quebra o texto em linhas de no máximo <paramref name="colunas"/> caracteres, em espaço, sem cortar a palavra que
    /// cabe numa linha; a palavra maior que a linha é cortada na largura. <c>\n</c> (e <c>\r\n</c>) quebra a linha, e a
    /// linha vazia é mantida. Os espaços do fim de cada linha e do começo das linhas de continuação são descartados.
    /// </summary>
    /// <remarks>
    /// Conta caracteres (unidades UTF-16): as letras acentuadas já compostas valem um. No tamanho
    /// <see cref="TamanhoTexto.Duplo"/> ou <see cref="TamanhoTexto.Pequeno"/>, quem chama passa as colunas dele
    /// (<see cref="PapelImpressao.ColunasTamanho"/>).
    /// </remarks>
    /// <param name="colunas">As colunas de caracteres de uma linha, 1 ou mais.</param>
    /// <returns>As linhas, sem <c>\n</c>; ao menos uma (vazia, para o texto vazio).</returns>
    /// <exception cref="ArgumentOutOfRangeException">As colunas são menores que 1.</exception>
    public IReadOnlyList<string> Quebrar(int colunas) => QuebraTexto.Quebrar(Texto, colunas);
}

/// <summary>A quebra de texto em colunas, compartilhada pelo texto e pela célula.</summary>
internal static class QuebraTexto
{
    internal static IReadOnlyList<string> Quebrar(string texto, int colunas)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(colunas, 1);
        var linhas = new List<string>();
        foreach (var paragrafo in texto.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
            QuebrarParagrafo(paragrafo, colunas, linhas);
        return linhas;
    }

    private static void QuebrarParagrafo(string paragrafo, int colunas, List<string> linhas)
    {
        var restante = paragrafo.TrimEnd(' ');
        if (restante.Length == 0)
        {
            linhas.Add(string.Empty);
            return;
        }

        var primeira = true;
        while (restante.Length > 0)
        {
            if (!primeira)
                restante = restante.TrimStart(' ');
            if (restante.Length <= colunas)
            {
                linhas.Add(restante);
                return;
            }

            // O último espaço dentro da largura (a posição colunas também vale: a palavra termina nela).
            var corte = restante.LastIndexOf(' ', colunas);
            if (corte <= 0)
            {
                linhas.Add(restante[..colunas]);
                restante = restante[colunas..];
            }
            else
            {
                linhas.Add(restante[..corte].TrimEnd(' '));
                restante = restante[corte..];
            }

            primeira = false;
        }
    }
}
