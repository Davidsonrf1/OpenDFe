namespace OpenDFe;

/// <summary>
/// Uma operação que o leiaute registrou (<see cref="DFe.Operacoes"/>): o tipo do parâmetro e o do retorno.
/// Criada pelo núcleo.
/// </summary>
public sealed class OperacaoSuportada
{
    internal OperacaoSuportada(Type parametro, Type retorno)
    {
        Parametro = parametro;
        Retorno = retorno;
    }

    /// <summary>O tipo do parâmetro (<c>typeof(ConsultaStatus)</c>).</summary>
    public Type Parametro { get; }

    /// <summary>O tipo do retorno (<c>typeof(RetornoStatus)</c>).</summary>
    public Type Retorno { get; }
}
