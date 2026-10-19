using System.Collections.Concurrent;

namespace OpenDFe.Testes;

/// <summary>
/// O transporte falso (a costura interna do <see cref="ConexoesSefaz"/>): guarda cada envio, avança o relógio
/// falso pela demora e responde como o autorizador falso, ou como o teste mandar.
/// </summary>
internal sealed class TransporteFalso : ITransporteSefaz, IDisposable
{
    public ConcurrentQueue<EnvioSefaz> Envios { get; } = new();

    public RelogioFalso? Relogio { get; init; }

    public TimeSpan Demora { get; init; }

    public bool Descartado { get; private set; }

    /// <summary>A resposta; o padrão marca o envio e responde como o autorizador falso.</summary>
    public Func<EnvioSefaz, CancellationToken, Task<string>> Responder { get; set; } = (envio, _) =>
    {
        envio.MarcarEnviado();
        return Task.FromResult(Resposta(envio));
    };

    public static string Resposta(EnvioSefaz envio) => envio.Servico == LeiauteFalso.ServicoStatus
        ? $"<retStatus><cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo><host>{envio.Url.Host}</host></retStatus>"
        : OperacaoEcoFalsa.Responder(envio.Xml);

    public Task<string> EnviarAsync(EnvioSefaz envio, CancellationToken cancellationToken)
    {
        Envios.Enqueue(envio);
        Relogio?.Avancar(Demora);
        return Responder(envio, cancellationToken);
    }

    public void Dispose() => Descartado = true;
}

/// <summary>Um relógio parado, que só anda quando o teste manda.</summary>
internal sealed class RelogioFalso : TimeProvider
{
    private readonly Lock _trava = new();
    private DateTimeOffset _agora;
    private long _marca;

    public RelogioFalso(DateTimeOffset agora)
    {
        _agora = agora;
    }

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override DateTimeOffset GetUtcNow()
    {
        lock (_trava)
            return _agora;
    }

    public override long GetTimestamp()
    {
        lock (_trava)
            return _marca;
    }

    public void Avancar(TimeSpan tempo)
    {
        lock (_trava)
        {
            _agora += tempo;
            _marca += tempo.Ticks;
        }
    }
}
