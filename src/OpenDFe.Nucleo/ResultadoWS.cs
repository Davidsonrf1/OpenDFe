using System.Diagnostics.CodeAnalysis;

namespace OpenDFe;

/// <summary>
/// O resultado de uma operação (<see cref="DFe.ExecutarAsync{TRetorno}"/>): processada pelo autorizador (com o
/// <c>cStat</c>, inclusive a rejeição) ou recusada localmente, com os problemas e o rastro. Criado pelo núcleo.
/// </summary>
/// <typeparam name="TRetorno">O retorno da operação.</typeparam>
/// <remarks>
/// A falha de infraestrutura (rede, TLS, resposta ilegível) não é resultado: é
/// <see cref="FalhaTransmissaoException"/> (PROJETO.md 2.4).
/// </remarks>
public sealed class ResultadoWS<TRetorno>
    where TRetorno : class
{
    private ResultadoWS(
        SituacaoResultado situacao, int? cStat, string? motivo, TRetorno? retorno,
        IReadOnlyList<Problema> problemas, RastroWS rastro)
    {
        Situacao = situacao;
        Processado = situacao == SituacaoResultado.Processado;
        CStat = cStat;
        Motivo = motivo;
        Retorno = retorno;
        Problemas = problemas;
        Rastro = rastro;
    }

    /// <summary>O que aconteceu.</summary>
    public SituacaoResultado Situacao { get; }

    /// <summary>
    /// <see cref="Situacao"/> é <see cref="SituacaoResultado.Processado"/>; com ela, <see cref="Retorno"/>,
    /// <see cref="CStat"/> e <see cref="Motivo"/> não são nulos.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Retorno), nameof(Motivo))]
    public bool Processado { get; }

    /// <summary>O <c>cStat</c> da resposta; <see langword="null"/> na recusa local.</summary>
    public int? CStat { get; }

    /// <summary>O <c>xMotivo</c> da resposta; <see langword="null"/> na recusa local.</summary>
    public string? Motivo { get; }

    /// <summary>O retorno lido pelo leiaute; <see langword="null"/> na recusa local.</summary>
    public TRetorno? Retorno { get; }

    /// <summary>
    /// Todos os problemas: os da montagem, os do schema e os da leitura, na ordem do caminho; no processado, só
    /// avisos.
    /// </summary>
    public IReadOnlyList<Problema> Problemas { get; }

    /// <summary>O que foi enviado e recebido, a URL e os tempos.</summary>
    public RastroWS Rastro { get; }

    internal static ResultadoWS<TRetorno> CriarProcessado(LeituraWS<TRetorno> leitura, IReadOnlyList<Problema> problemas, RastroWS rastro) =>
        new(SituacaoResultado.Processado, leitura.CStat, leitura.Motivo, leitura.Retorno, problemas, rastro);

    internal static ResultadoWS<TRetorno> CriarRecusado(IReadOnlyList<Problema> problemas, RastroWS rastro) =>
        new(SituacaoResultado.RecusadoLocalmente, null, null, null, problemas, rastro);
}
