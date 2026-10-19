namespace OpenDFe;

/// <summary>
/// O que o handler de uma operação (<see cref="IOperacao{TParametro, TRetorno}"/>) enxerga do núcleo. Criado
/// pelo núcleo.
/// </summary>
public sealed class ContextoOperacao
{
    internal ContextoOperacao(Documento documento, ConfiguracaoDFe configuracao)
    {
        Documento = documento;
        Configuracao = configuracao;
    }

    /// <summary>O documento do núcleo.</summary>
    public Documento Documento { get; }

    /// <summary>A configuração do núcleo.</summary>
    public ConfiguracaoDFe Configuracao { get; }
}
