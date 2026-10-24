namespace OpenDFe.Impressao;

/// <summary>Uma célula de uma <see cref="LinhaImpressao"/>: o texto e o peso da largura.</summary>
/// <remarks>Imutável.</remarks>
public sealed class CelulaImpressao
{
    private readonly double _peso;

    /// <summary>Cria a célula.</summary>
    /// <param name="texto">O texto, já formatado.</param>
    /// <param name="peso">O peso da largura em relação às outras células da linha; maior que zero.</param>
    /// <exception cref="ArgumentNullException">O texto é nulo.</exception>
    /// <exception cref="ArgumentOutOfRangeException">O peso não é um número maior que zero.</exception>
    public CelulaImpressao(string texto, double peso = 1)
    {
        ArgumentNullException.ThrowIfNull(texto);
        _peso = ValidarPeso(peso);
        Texto = texto;
    }

    /// <summary>O texto.</summary>
    public string Texto { get; }

    /// <summary>O peso da largura em relação às outras células da linha.</summary>
    public double Peso => _peso;

    /// <summary>O alinhamento do texto dentro da célula; à esquerda por padrão.</summary>
    public AlinhamentoImpressao Alinhamento { get; init; }

    /// <summary>Se o texto está em negrito.</summary>
    public bool Negrito { get; init; }

    /// <summary>Quebra o texto da célula como <see cref="TextoImpressao.Quebrar"/>.</summary>
    /// <param name="colunas">As colunas da célula, 1 ou mais.</param>
    /// <returns>As linhas do texto na largura.</returns>
    /// <exception cref="ArgumentOutOfRangeException">As colunas são menores que 1.</exception>
    public IReadOnlyList<string> Quebrar(int colunas) => QuebraTexto.Quebrar(Texto, colunas);

    internal static double ValidarPeso(double peso)
    {
        if (!(peso > 0) || double.IsInfinity(peso))
            throw new ArgumentOutOfRangeException(nameof(peso), peso, "O peso deve ser um número maior que zero.");
        return peso;
    }
}

/// <summary>
/// Uma linha de células com larguras proporcionais (os itens da NFC-e: código, descrição, quantidade, valores).
/// </summary>
/// <remarks>Imutável: a lista passada é copiada.</remarks>
public sealed class LinhaImpressao : ElementoImpressao
{
    /// <summary>Cria a linha.</summary>
    /// <param name="celulas">As células, uma ou mais (a lista é copiada).</param>
    /// <exception cref="ArgumentNullException">A lista ou uma célula é nula.</exception>
    /// <exception cref="ArgumentException">A lista é vazia.</exception>
    public LinhaImpressao(IReadOnlyList<CelulaImpressao> celulas)
    {
        ArgumentNullException.ThrowIfNull(celulas);
        if (celulas.Count == 0)
            throw new ArgumentException("A linha precisa de ao menos uma célula.", nameof(celulas));
        var copia = new CelulaImpressao[celulas.Count];
        for (var i = 0; i < copia.Length; i++)
            copia[i] = celulas[i] ?? throw new ArgumentNullException(nameof(celulas), "A lista tem uma célula nula.");
        Celulas = Array.AsReadOnly(copia);
    }

    /// <summary>As células, da esquerda para a direita.</summary>
    public IReadOnlyList<CelulaImpressao> Celulas { get; }

    /// <summary>
    /// As larguras inteiras de cada célula, em colunas, que somam exatamente <paramref name="colunas"/> (o método do maior
    /// resto: cada célula recebe a parte inteira da sua cota e as colunas que sobram vão às maiores frações, a primeira
    /// célula nos empates). Com <paramref name="colunas"/> maior ou igual ao número de células, nenhuma fica com largura
    /// zero (a coluna que falta sai da célula mais larga); com menos, as de menor peso podem ficar com zero.
    /// </summary>
    /// <param name="colunas">As colunas da linha, 1 ou mais.</param>
    /// <returns>As larguras, uma por célula, na ordem das células.</returns>
    /// <exception cref="ArgumentOutOfRangeException">As colunas são menores que 1.</exception>
    public IReadOnlyList<int> Larguras(int colunas)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(colunas, 1);
        var n = Celulas.Count;
        var soma = 0d;
        foreach (var celula in Celulas)
            soma += celula.Peso;

        var larguras = new int[n];
        var fracoes = new double[n];
        var usadas = 0;
        for (var i = 0; i < n; i++)
        {
            var cota = Celulas[i].Peso / soma * colunas;
            larguras[i] = (int)Math.Floor(cota);
            fracoes[i] = cota - larguras[i];
            usadas += larguras[i];
        }

        // As colunas que sobram vão às maiores frações; o empate fica com a célula da esquerda.
        var ordem = Enumerable.Range(0, n).OrderByDescending(i => fracoes[i]).ThenBy(i => i).ToArray();
        for (var k = 0; usadas < colunas; k++, usadas++)
            larguras[ordem[k % n]]++;

        if (colunas >= n)
        {
            for (var i = 0; i < n; i++)
            {
                if (larguras[i] != 0)
                    continue;
                var maior = 0;
                for (var j = 1; j < n; j++)
                {
                    if (larguras[j] > larguras[maior])
                        maior = j;
                }

                larguras[maior]--;
                larguras[i] = 1;
            }
        }

        return Array.AsReadOnly(larguras);
    }
}
