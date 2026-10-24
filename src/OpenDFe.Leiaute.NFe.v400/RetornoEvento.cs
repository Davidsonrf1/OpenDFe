namespace OpenDFe.NFe;

/// <summary>O retorno do <see cref="EnvioEvento"/> (o <c>retEnvEvento</c>): o lote e o resultado de cada evento. Imutável.</summary>
public sealed class RetornoEvento
{
    internal RetornoEvento(int status, string motivo, IReadOnlyList<ResultadoEvento> eventos)
    {
        Status = status;
        Motivo = motivo;
        Eventos = eventos;
    }

    /// <summary>O <c>cStat</c> do lote: 128 (processado) ou a rejeição do lote.</summary>
    public int Status { get; }

    /// <summary>O <c>xMotivo</c> do lote.</summary>
    public string Motivo { get; }

    /// <summary>O <c>tpAmb</c>; nulo quando não veio ou não se leu.</summary>
    public Ambiente? Ambiente { get; internal init; }

    /// <summary>O <c>verAplic</c>.</summary>
    public string? VersaoAplicativo { get; internal init; }

    /// <summary>O <c>cOrgao</c> do retorno.</summary>
    public int? Orgao { get; internal init; }

    /// <summary>O <c>idLote</c>.</summary>
    public long? Lote { get; internal init; }

    /// <summary>Um por evento enviado, na ordem do envio (o que sobrar do retorno, no fim). Vazia na rejeição do lote.</summary>
    public IReadOnlyList<ResultadoEvento> Eventos { get; }
}
