namespace OpenDFe;

/// <summary>
/// Um problema encontrado na validação (schema, regras do leiaute ou de quem usa) ou na leitura de um
/// documento. Problema não é exceção: volta como resultado.
/// </summary>
public sealed class Problema
{
    /// <summary>Cria um problema.</summary>
    /// <param name="codigo">O código do problema; nas regras do MOC, o da rejeição que ela antecipa.</param>
    /// <param name="severidade">A severidade.</param>
    /// <param name="mensagem">A mensagem, em português.</param>
    /// <exception cref="ArgumentException">O código ou a mensagem estão vazios.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A severidade não é um valor de <see cref="OpenDFe.Severidade"/>.</exception>
    public Problema(string codigo, Severidade severidade, string mensagem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(mensagem);
        if (!Enum.IsDefined(severidade))
            throw new ArgumentOutOfRangeException(nameof(severidade), severidade, "Severidade desconhecida.");

        Codigo = codigo;
        Severidade = severidade;
        Mensagem = mensagem;
    }

    /// <summary>O código do problema; nas regras do MOC, o da rejeição que ela antecipa (<c>"564"</c>).</summary>
    public string Codigo { get; }

    /// <summary>A severidade.</summary>
    public Severidade Severidade { get; }

    /// <summary>A mensagem, em português.</summary>
    public string Mensagem { get; }

    /// <summary>A tag do XML (<c>"vProd"</c>), quando o problema é de um campo.</summary>
    public string? Tag { get; init; }

    /// <summary>O caminho do elemento no XML (<c>"det[3]/prod/vProd"</c>), quando conhecido.</summary>
    public string? Caminho { get; init; }

    /// <summary>A propriedade do modelo (<c>"Itens[2].Produto.Valor"</c>), quando conhecida.</summary>
    public string? Propriedade { get; init; }

    /// <summary>O problema em uma linha: severidade, código, mensagem e, se houver, onde.</summary>
    /// <returns>O texto do problema.</returns>
    public override string ToString()
    {
        var onde = Propriedade ?? Caminho ?? Tag;
        return onde is null
            ? $"{Severidade} {Codigo}: {Mensagem}"
            : $"{Severidade} {Codigo}: {Mensagem} ({onde})";
    }
}
