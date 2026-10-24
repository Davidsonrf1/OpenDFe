namespace OpenDFe.Impressao;

/// <summary>O estilo do separador.</summary>
public enum EstiloSeparador
{
    /// <summary>A linha tracejada.</summary>
    Tracejado,

    /// <summary>A linha cheia.</summary>
    Cheio,
}

/// <summary>Uma linha horizontal na largura do papel que separa as partes do documento.</summary>
/// <remarks>Imutável.</remarks>
public sealed class SeparadorImpressao : ElementoImpressao
{
    /// <summary>O estilo; tracejado por padrão.</summary>
    public EstiloSeparador Estilo { get; init; }
}

/// <summary>Um espaço em branco de algumas linhas de texto.</summary>
/// <remarks>Imutável.</remarks>
public sealed class EspacoImpressao : ElementoImpressao
{
    /// <summary>Cria o espaço.</summary>
    /// <param name="linhas">A altura em linhas de texto normal, 1 ou mais.</param>
    /// <exception cref="ArgumentOutOfRangeException">As linhas são menores que 1.</exception>
    public EspacoImpressao(int linhas)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(linhas, 1);
        Linhas = linhas;
    }

    /// <summary>A altura em linhas de texto normal.</summary>
    public int Linhas { get; }
}

/// <summary>A quebra de página (só na folha; na bobina não tem efeito).</summary>
/// <remarks>Imutável.</remarks>
public sealed class QuebraPaginaImpressao : ElementoImpressao
{
}
