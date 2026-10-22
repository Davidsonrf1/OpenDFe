namespace OpenDFe.NFe;

/// <summary>
/// O retorno da <see cref="Autorizacao"/> e da <see cref="ConsultaRecibo"/> (o <c>retEnviNFe</c> e o
/// <c>retConsReciNFe</c>, docs/CONTRATOS.md 14): os campos do lote e o resultado de cada nota. Imutável.
/// </summary>
/// <remarks>
/// O <see cref="Status"/> é o do lote (103 recebido, 104 processado, 105 em processamento, 106 não encontrado, ou a
/// rejeição do lote); o de cada nota está no <see cref="ResultadoNota.Protocolo"/>.
/// </remarks>
public sealed class RetornoAutorizacao
{
    internal RetornoAutorizacao(int status, string motivo, IReadOnlyList<ResultadoNota> notas)
    {
        Status = status;
        Motivo = motivo;
        Notas = notas;
    }

    /// <summary>O <c>cStat</c> do lote.</summary>
    public int Status { get; }

    /// <summary>O <c>xMotivo</c> do lote.</summary>
    public string Motivo { get; }

    /// <summary>O <c>tpAmb</c>; nulo quando não veio ou não se leu.</summary>
    public Ambiente? Ambiente { get; internal init; }

    /// <summary>O <c>verAplic</c>.</summary>
    public string? VersaoAplicativo { get; internal init; }

    /// <summary>O <c>cUF</c>: a UF do autorizador que atendeu.</summary>
    public UF? UF { get; internal init; }

    /// <summary>O <c>dhRecbto</c>.</summary>
    public DateTimeOffset? Recebimento { get; internal init; }

    /// <summary>
    /// O <c>nRec</c>: o recibo do lote (103), que se consulta com <see cref="ConsultaRecibo"/>; também quando a síncrona
    /// devolve o recibo, e na consulta.
    /// </summary>
    public string? Recibo { get; internal init; }

    /// <summary>O <c>tMed</c>: o tempo médio de resposta, a esperar antes de consultar o recibo.</summary>
    public TimeSpan? TempoMedio { get; internal init; }

    /// <summary>A mensagem da SEFAZ ao emissor sobre o lote (<c>cMsg</c> e <c>xMsg</c> da consulta do recibo).</summary>
    public MensagemSefaz? Mensagem { get; internal init; }

    /// <summary>
    /// Uma por nota enviada, na ordem do parâmetro (ou dos <see cref="ConsultaRecibo.Xmls"/>); na consulta sem os XMLs,
    /// uma por <c>protNFe</c>, na ordem da resposta. Vazia na rejeição do lote.
    /// </summary>
    public IReadOnlyList<ResultadoNota> Notas { get; }
}
