namespace OpenDFe;

/// <summary>
/// O retorno lido por <see cref="IOperacao{TParametro, TRetorno}.Ler"/>: o objeto do leiaute, o <c>cStat</c>,
/// o motivo e os problemas da leitura.
/// </summary>
/// <typeparam name="TRetorno">O retorno da operação.</typeparam>
public sealed class LeituraWS<TRetorno>
    where TRetorno : class
{
    private readonly IReadOnlyList<Problema> _problemas = [];

    /// <summary>Cria o retorno lido.</summary>
    /// <param name="retorno">O retorno.</param>
    /// <param name="cStat">
    /// O <c>cStat</c> da mensagem (no lote, o do lote; o de cada nota fica no <paramref name="retorno"/>).
    /// </param>
    /// <param name="motivo">O <c>xMotivo</c> da mensagem.</param>
    /// <exception cref="ArgumentNullException">O retorno ou o motivo são nulos.</exception>
    public LeituraWS(TRetorno retorno, int cStat, string motivo)
    {
        ArgumentNullException.ThrowIfNull(retorno);
        ArgumentNullException.ThrowIfNull(motivo);
        Retorno = retorno;
        CStat = cStat;
        Motivo = motivo;
    }

    /// <summary>O retorno.</summary>
    public TRetorno Retorno { get; }

    /// <summary>O <c>cStat</c> da mensagem.</summary>
    public int CStat { get; }

    /// <summary>O <c>xMotivo</c> da mensagem.</summary>
    public string Motivo { get; }

    /// <summary>Os problemas da leitura (um campo que o leiaute não reconhece, por exemplo).</summary>
    /// <exception cref="ArgumentNullException">A lista ou um dos problemas são nulos.</exception>
    public IReadOnlyList<Problema> Problemas
    {
        get => _problemas;
        init => _problemas = PedidoWS.Copiar(value);
    }
}
