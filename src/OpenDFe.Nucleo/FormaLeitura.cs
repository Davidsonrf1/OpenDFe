namespace OpenDFe;

/// <summary>
/// Uma forma de XML que o leiaute lê, para um documento (PROJETO.md 3.3): a raiz e onde estão a versão e o modelo. O
/// leiaute a declara em <see cref="DeclaracaoLeiaute.DeclararLeitura"/>, e <see cref="DFe.Detectar"/> a confere.
/// </summary>
/// <remarks>
/// Os caminhos são os nomes locais dos elementos separados por <c>/</c>, a partir da raiz (sem ela), no namespace da
/// raiz; o último passo pode ser um atributo sem namespace (<c>@versao</c>). O caminho da versão termina sempre num
/// atributo (<c>infNFe/@versao</c>; <c>@versao</c>, o da própria raiz); o do modelo, num elemento ou num atributo
/// (<c>infNFe/ide/mod</c>), cujo texto é o número do documento (<c>55</c>).
/// </remarks>
public sealed class FormaLeitura
{
    /// <summary>Cria a forma.</summary>
    /// <param name="espacoNomes">O namespace da raiz (<c>http://www.portalfiscal.inf.br/nfe</c>).</param>
    /// <param name="raiz">O nome local da raiz (<c>NFe</c>).</param>
    /// <param name="caminhoVersao">O caminho do atributo da versão (<c>infNFe/@versao</c>).</param>
    /// <param name="caminhoModelo">O caminho do modelo (<c>infNFe/ide/mod</c>).</param>
    /// <exception cref="ArgumentNullException">Um dos argumentos é nulo.</exception>
    /// <exception cref="ArgumentException">
    /// Um texto vazio; um caminho com passo vazio ou com atributo antes do fim; o caminho da versão sem atributo no fim.
    /// </exception>
    public FormaLeitura(string espacoNomes, string raiz, string caminhoVersao, string caminhoModelo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(espacoNomes);
        ArgumentException.ThrowIfNullOrWhiteSpace(raiz);
        ArgumentException.ThrowIfNullOrWhiteSpace(caminhoVersao);
        ArgumentException.ThrowIfNullOrWhiteSpace(caminhoModelo);
        Passos(caminhoVersao, nameof(caminhoVersao));
        if (!caminhoVersao.Split('/')[^1].StartsWith('@'))
            throw new ArgumentException("O caminho da versão termina num atributo (@versao).", nameof(caminhoVersao));
        Passos(caminhoModelo, nameof(caminhoModelo));
        EspacoNomes = espacoNomes;
        Raiz = raiz;
        CaminhoVersao = caminhoVersao;
        CaminhoModelo = caminhoModelo;
    }

    /// <summary>O namespace da raiz.</summary>
    public string EspacoNomes { get; }

    /// <summary>O nome local da raiz.</summary>
    public string Raiz { get; }

    /// <summary>O caminho do atributo da versão.</summary>
    public string CaminhoVersao { get; }

    /// <summary>O caminho do modelo.</summary>
    public string CaminhoModelo { get; }

    // Os passos do caminho: nenhum vazio, e o atributo só no último.
    internal static string[] Passos(string caminho, string parametro)
    {
        var passos = caminho.Split('/');
        for (var i = 0; i < passos.Length; i++)
        {
            var passo = passos[i];
            var atributo = passo.StartsWith('@');
            if (passo.Length == 0 || (atributo && passo.Length == 1))
                throw new ArgumentException($"O caminho {caminho} tem um passo vazio.", parametro);
            if (atributo && i < passos.Length - 1)
                throw new ArgumentException($"O caminho {caminho} tem um atributo antes do fim.", parametro);
        }
        return passos;
    }
}
