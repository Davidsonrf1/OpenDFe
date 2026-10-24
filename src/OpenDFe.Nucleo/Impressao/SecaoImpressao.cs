namespace OpenDFe.Impressao;

/// <summary>Um campo rotulado de uma <see cref="LinhaCamposImpressao"/> (a grade do DANFE).</summary>
/// <remarks>Imutável.</remarks>
public sealed class CampoImpressao
{
    private readonly double _peso;

    /// <summary>Cria o campo.</summary>
    /// <param name="rotulo">O rótulo, já formatado.</param>
    /// <param name="valor">O valor, já formatado.</param>
    /// <param name="peso">O peso da largura em relação aos outros campos da linha; maior que zero.</param>
    /// <exception cref="ArgumentNullException">O rótulo ou o valor é nulo.</exception>
    /// <exception cref="ArgumentOutOfRangeException">O peso não é um número maior que zero.</exception>
    public CampoImpressao(string rotulo, string valor, double peso = 1)
    {
        ArgumentNullException.ThrowIfNull(rotulo);
        ArgumentNullException.ThrowIfNull(valor);
        _peso = CelulaImpressao.ValidarPeso(peso);
        Rotulo = rotulo;
        Valor = valor;
    }

    /// <summary>O rótulo.</summary>
    public string Rotulo { get; }

    /// <summary>O valor.</summary>
    public string Valor { get; }

    /// <summary>O peso da largura em relação aos outros campos da linha.</summary>
    public double Peso => _peso;

    /// <summary>O alinhamento do valor; à esquerda por padrão.</summary>
    public AlinhamentoImpressao Alinhamento { get; init; }

    /// <summary>Se o valor está em negrito.</summary>
    public bool Negrito { get; init; }
}

/// <summary>Uma linha de campos rotulados, lado a lado, com larguras proporcionais aos pesos.</summary>
/// <remarks>Imutável: a lista passada é copiada.</remarks>
public sealed class LinhaCamposImpressao
{
    /// <summary>Cria a linha.</summary>
    /// <param name="campos">Os campos, um ou mais (a lista é copiada).</param>
    /// <exception cref="ArgumentNullException">A lista ou um campo é nulo.</exception>
    /// <exception cref="ArgumentException">A lista é vazia.</exception>
    public LinhaCamposImpressao(IReadOnlyList<CampoImpressao> campos)
    {
        ArgumentNullException.ThrowIfNull(campos);
        if (campos.Count == 0)
            throw new ArgumentException("A linha precisa de ao menos um campo.", nameof(campos));
        var copia = new CampoImpressao[campos.Count];
        for (var i = 0; i < copia.Length; i++)
            copia[i] = campos[i] ?? throw new ArgumentNullException(nameof(campos), "A lista tem um campo nulo.");
        Campos = Array.AsReadOnly(copia);
    }

    /// <summary>Os campos, da esquerda para a direita.</summary>
    public IReadOnlyList<CampoImpressao> Campos { get; }
}

/// <summary>Uma seção com título e linhas de campos rotulados, em grade (o DANFE, etapa 7.3).</summary>
/// <remarks>Imutável: a lista passada é copiada.</remarks>
public sealed class SecaoImpressao : ElementoImpressao
{
    /// <summary>Cria a seção.</summary>
    /// <param name="titulo">O título; <see langword="null"/> para a seção sem título.</param>
    /// <param name="linhas">As linhas de campos, uma ou mais (a lista é copiada).</param>
    /// <exception cref="ArgumentNullException">A lista ou uma linha é nula.</exception>
    /// <exception cref="ArgumentException">A lista é vazia.</exception>
    public SecaoImpressao(string? titulo, IReadOnlyList<LinhaCamposImpressao> linhas)
    {
        ArgumentNullException.ThrowIfNull(linhas);
        if (linhas.Count == 0)
            throw new ArgumentException("A seção precisa de ao menos uma linha.", nameof(linhas));
        var copia = new LinhaCamposImpressao[linhas.Count];
        for (var i = 0; i < copia.Length; i++)
            copia[i] = linhas[i] ?? throw new ArgumentNullException(nameof(linhas), "A lista tem uma linha nula.");
        Titulo = titulo;
        Linhas = Array.AsReadOnly(copia);
    }

    /// <summary>O título; <see langword="null"/> na seção sem título.</summary>
    public string? Titulo { get; }

    /// <summary>As linhas de campos, de cima para baixo.</summary>
    public IReadOnlyList<LinhaCamposImpressao> Linhas { get; }
}

/// <summary>Uma coluna de uma <see cref="TabelaImpressao"/>: o título, o peso da largura e o alinhamento.</summary>
/// <remarks>Imutável.</remarks>
public sealed class ColunaImpressao
{
    private readonly double _peso;

    /// <summary>Cria a coluna.</summary>
    /// <param name="titulo">O título do cabeçalho.</param>
    /// <param name="peso">O peso da largura em relação às outras colunas; maior que zero.</param>
    /// <param name="alinhamento">O alinhamento das células da coluna.</param>
    /// <exception cref="ArgumentNullException">O título é nulo.</exception>
    /// <exception cref="ArgumentOutOfRangeException">O peso não é um número maior que zero.</exception>
    public ColunaImpressao(string titulo, double peso, AlinhamentoImpressao alinhamento = AlinhamentoImpressao.Esquerda)
    {
        ArgumentNullException.ThrowIfNull(titulo);
        _peso = CelulaImpressao.ValidarPeso(peso);
        Titulo = titulo;
        Alinhamento = alinhamento;
    }

    /// <summary>O título do cabeçalho.</summary>
    public string Titulo { get; }

    /// <summary>O peso da largura em relação às outras colunas.</summary>
    public double Peso => _peso;

    /// <summary>O alinhamento das células da coluna.</summary>
    public AlinhamentoImpressao Alinhamento { get; }
}

/// <summary>Uma tabela com o cabeçalho repetido a cada página (os itens do DANFE, etapa 7.3).</summary>
/// <remarks>Imutável: as listas passadas são copiadas, linha por linha.</remarks>
public sealed class TabelaImpressao : ElementoImpressao
{
    /// <summary>Cria a tabela.</summary>
    /// <param name="titulo">O título; <see langword="null"/> sem título.</param>
    /// <param name="colunas">As colunas, uma ou mais (a lista é copiada).</param>
    /// <param name="linhas">As linhas, cada uma com um texto por coluna (as listas são copiadas); pode não ter nenhuma.</param>
    /// <exception cref="ArgumentNullException">Uma lista, uma coluna, uma linha ou um texto é nulo.</exception>
    /// <exception cref="ArgumentException">Não há colunas, ou uma linha não tem um texto por coluna.</exception>
    public TabelaImpressao(string? titulo, IReadOnlyList<ColunaImpressao> colunas, IReadOnlyList<IReadOnlyList<string>> linhas)
    {
        ArgumentNullException.ThrowIfNull(colunas);
        ArgumentNullException.ThrowIfNull(linhas);
        if (colunas.Count == 0)
            throw new ArgumentException("A tabela precisa de ao menos uma coluna.", nameof(colunas));

        var colunasCopia = new ColunaImpressao[colunas.Count];
        for (var i = 0; i < colunasCopia.Length; i++)
            colunasCopia[i] = colunas[i] ?? throw new ArgumentNullException(nameof(colunas), "A lista tem uma coluna nula.");

        var linhasCopia = new IReadOnlyList<string>[linhas.Count];
        for (var i = 0; i < linhasCopia.Length; i++)
        {
            var linha = linhas[i] ?? throw new ArgumentNullException(nameof(linhas), "A lista tem uma linha nula.");
            if (linha.Count != colunasCopia.Length)
                throw new ArgumentException($"A linha {i} tem {linha.Count} textos, e a tabela tem {colunasCopia.Length} colunas.", nameof(linhas));
            var textos = new string[linha.Count];
            for (var j = 0; j < textos.Length; j++)
                textos[j] = linha[j] ?? throw new ArgumentNullException(nameof(linhas), "A linha tem um texto nulo.");
            linhasCopia[i] = Array.AsReadOnly(textos);
        }

        Titulo = titulo;
        Colunas = Array.AsReadOnly(colunasCopia);
        Linhas = Array.AsReadOnly(linhasCopia);
    }

    /// <summary>O título; <see langword="null"/> sem título.</summary>
    public string? Titulo { get; }

    /// <summary>As colunas.</summary>
    public IReadOnlyList<ColunaImpressao> Colunas { get; }

    /// <summary>As linhas, cada uma com um texto por coluna.</summary>
    public IReadOnlyList<IReadOnlyList<string>> Linhas { get; }

    /// <summary>
    /// Se uma linha da tabela nunca é dividida entre duas páginas (o item do DANFE com a descrição e as informações
    /// adicionais em várias linhas, Anexo II 3.1.7); falso por padrão. A quebra de linha (o caractere
    /// de nova linha) no texto de uma célula é uma quebra de linha dentro da célula.
    /// </summary>
    public bool LinhasIndivisiveis { get; init; }

    /// <summary>
    /// Se há um divisor entre uma linha e a seguinte (o "destaque divisório" que distingue um item do outro, Anexo II 3.1.7:
    /// linha, espaço duplo ou sombreamento, a escolha é de quem renderiza); falso por padrão.
    /// </summary>
    public bool DivisorLinhas { get; init; }
}
