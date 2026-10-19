namespace OpenDFe;

/// <summary>
/// A base de todo parâmetro de operação. Só <see cref="ParametroWS{TRetorno}"/> deriva dela: existe para a
/// restrição de <see cref="DFe.Suporta{TParametro}"/>.
/// </summary>
public abstract class ParametroWS
{
    private readonly Destino? _destino;

    private protected ParametroWS()
    {
    }

    /// <summary>
    /// Para onde a operação vai; <see langword="null"/> = o destino natural dela (PROJETO.md 8.3 e 8.4).
    /// Sobrepõe o destino do pedido e o do núcleo; o tipo de emissão continua o do pedido (ou o do núcleo).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">O valor não é um destino.</exception>
    public Destino? Destino
    {
        get => _destino;
        init
        {
            if (value is { } destino && !Enum.IsDefined(destino))
                throw new ArgumentOutOfRangeException(nameof(value), destino, "O valor não é um destino.");
            _destino = value;
        }
    }
}

/// <summary>
/// O parâmetro de uma operação que devolve <typeparamref name="TRetorno"/>
/// (<c>DFe.ExecutarAsync(new ConsultaStatus())</c> é <c>Task&lt;ResultadoWS&lt;RetornoStatus&gt;&gt;</c>). Os
/// leiautes derivam classes seladas, uma por operação.
/// </summary>
/// <typeparam name="TRetorno">O retorno da operação.</typeparam>
public abstract class ParametroWS<TRetorno> : ParametroWS
    where TRetorno : class
{
    /// <summary>Construtor do parâmetro derivado.</summary>
#pragma warning disable RS0022 // De propósito: a base não genérica só é herdável por esta (CONTRATOS.md 2.6), e não tem membros protegidos.
    protected ParametroWS()
#pragma warning restore RS0022
    {
    }
}
