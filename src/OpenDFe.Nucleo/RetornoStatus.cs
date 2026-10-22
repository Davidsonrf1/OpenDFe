namespace OpenDFe;

/// <summary>
/// O retorno do status do serviço (<see cref="ConsultaStatus"/>): o <c>cStat</c>, o motivo e os outros campos da
/// resposta, comuns aos documentos.
/// </summary>
/// <remarks>
/// <para>
/// O leiaute cria. A leitura é branda: só o <c>cStat</c> e o <c>xMotivo</c> são exigidos (sem eles, a resposta é
/// ilegível); o campo ausente fica nulo, e o que não se lê também, com um aviso no resultado.
/// </para>
/// <para>
/// O <see cref="Status"/> e o <see cref="Motivo"/> são os mesmos do <see cref="ResultadoWS{TRetorno}"/>: repetidos aqui
/// para o retorno valer sozinho.
/// </para>
/// </remarks>
public sealed class RetornoStatus
{
    /// <summary>O código do status do serviço na NF-e, no CT-e e no MDF-e: "Serviço em Operação".</summary>
    private const int CodigoOperando = 107;

    /// <summary>Cria o retorno.</summary>
    /// <param name="status">O <c>cStat</c>.</param>
    /// <param name="motivo">O <c>xMotivo</c>.</param>
    /// <exception cref="ArgumentNullException">O motivo é nulo.</exception>
    public RetornoStatus(int status, string motivo)
    {
        ArgumentNullException.ThrowIfNull(motivo);
        Status = status;
        Motivo = motivo;
    }

    /// <summary>
    /// O código do status (<c>cStat</c>): 107 em operação; 108 paralisado momentaneamente; 109 paralisado sem previsão;
    /// ou o da rejeição do pedido.
    /// </summary>
    public int Status { get; }

    /// <summary>A descrição do status (<c>xMotivo</c>).</summary>
    public string Motivo { get; }

    /// <summary>Se o serviço está em operação (<see cref="Status"/> 107).</summary>
    public bool Operando => Status == CodigoOperando;

    /// <summary>O ambiente que respondeu (<c>tpAmb</c>).</summary>
    public Ambiente? Ambiente { get; init; }

    /// <summary>A versão do aplicativo do autorizador (<c>verAplic</c>).</summary>
    public string? VersaoAplicativo { get; init; }

    /// <summary>A UF responsável pelo serviço (<c>cUF</c>).</summary>
    public UF? UF { get; init; }

    /// <summary>A data e a hora do recebimento da consulta (<c>dhRecbto</c>).</summary>
    public DateTimeOffset? Recebimento { get; init; }

    /// <summary>O tempo médio de resposta do serviço nos últimos 5 minutos (<c>tMed</c>, em segundos).</summary>
    public TimeSpan? TempoMedio { get; init; }

    /// <summary>A data e a hora previstas para a volta do serviço paralisado (<c>dhRetorno</c>).</summary>
    public DateTimeOffset? PrevisaoRetorno { get; init; }

    /// <summary>A observação do autorizador ao contribuinte (<c>xObs</c>).</summary>
    public string? Observacao { get; init; }
}
