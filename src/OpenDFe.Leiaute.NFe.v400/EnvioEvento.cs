namespace OpenDFe.NFe;

/// <summary>
/// O envio de 1 a 20 eventos (<c>NFeRecepcaoEvento4</c>, docs/CONTRATOS.md 18): o <c>envEvento</c>, com cada evento
/// assinado à parte, e o <c>retEnvEvento</c>. Na NF-e e na NFC-e.
/// </summary>
/// <remarks>
/// O destino e o tipo de emissão saem da chave do primeiro evento (PROJETO.md 8.4); só o cancelamento de nota de SVC vai à
/// SVC. O <c>Destino</c> do parâmetro sobrepõe, no autorizador normal. Os eventos de um lote vão ao mesmo destino
/// (<c>NFE-LOTE</c>), e a chave de outro modelo que o do núcleo é <c>NFE-CHAVE</c>.
/// </remarks>
public sealed class EnvioEvento : ParametroWS<RetornoEvento>
{
    /// <summary>O máximo de eventos num envio.</summary>
    public const int MaximoEventos = 20;

    private const long MaiorLote = 999_999_999_999_999;

    private readonly long? _lote;

    /// <summary>O envio dos eventos.</summary>
    /// <param name="eventos">De 1 a 20 eventos.</param>
    /// <exception cref="ArgumentNullException">A lista é nula.</exception>
    /// <exception cref="ArgumentException">Nenhum evento, mais de 20, ou um nulo.</exception>
    public EnvioEvento(params IEnumerable<Evento> eventos)
    {
        ArgumentNullException.ThrowIfNull(eventos);
        Evento[] lista = [.. eventos];
        if (lista.Length == 0)
            throw new ArgumentException("O envio precisa de pelo menos um evento.", nameof(eventos));
        if (lista.Length > MaximoEventos)
            throw new ArgumentException($"O envio vai até {MaximoEventos} eventos: vieram {lista.Length}.", nameof(eventos));
        if (Array.Exists(lista, e => e is null))
            throw new ArgumentException("Um dos eventos é nulo.", nameof(eventos));
        Eventos = lista.AsReadOnly();
    }

    /// <summary>Os eventos, na ordem do envio.</summary>
    public IReadOnlyList<Evento> Eventos { get; }

    /// <summary>O <c>idLote</c> (1 a 15 dígitos); nulo: do relógio da configuração, em UTC (<c>AAMMDDhhmmssmmm</c>).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Menor que 1 ou com mais de 15 dígitos.</exception>
    public long? Lote
    {
        get => _lote;
        init
        {
            if (value is { } lote && lote is < 1 or > MaiorLote)
                throw new ArgumentOutOfRangeException(nameof(value), value, "O lote vai de 1 a 15 dígitos.");
            _lote = value;
        }
    }
}
