namespace OpenDFe;

/// <summary>
/// A resposta HTTP que o núcleo entrega a <see cref="IOperacaoRest{TParametro, TRetorno}.Ler"/> (docs/NFSE.md, D5): o
/// status e o corpo como vieram. Só chega ao leiaute o que é resposta (2xx, e 4xx com JSON ou sem corpo); o resto é
/// <see cref="FalhaTransmissaoException"/>.
/// </summary>
public sealed class RespostaRest
{
    /// <summary>Cria a resposta (o núcleo cria; público para os testes do leiaute).</summary>
    /// <param name="status">O status HTTP.</param>
    /// <param name="corpo">O corpo como texto; <see langword="null"/> no HEAD e na resposta vazia.</param>
    /// <exception cref="ArgumentOutOfRangeException">O status não está entre 100 e 599.</exception>
    public RespostaRest(int status, string? corpo)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(status, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(status, 599);
        Status = status;
        Corpo = string.IsNullOrEmpty(corpo) ? null : corpo;
    }

    /// <summary>O status HTTP (201, 200, 400, 404).</summary>
    public int Status { get; }

    /// <summary>O corpo como texto (o JSON); <see langword="null"/> no HEAD e na resposta vazia.</summary>
    public string? Corpo { get; }
}
