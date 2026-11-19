namespace OpenDFe.NFSe;

/// <summary>
/// A recuperação da emissão cuja resposta se perdeu (a queda no meio, como a PROJETO.md 8.6 da NF-e; docs/NFSE.md 3.3): consulta a
/// DPS (<see cref="ConsultaDps"/>, o GET) e, gerada a NFS-e, a NFS-e pela chave (<see cref="ConsultaNFSe"/>). Duas chamadas
/// ao mesmo núcleo; não reenvia nada.
/// </summary>
/// <remarks>
/// <para>
/// Quando usar: a <see cref="EmissaoNFSe"/> lançou <see cref="FalhaTransmissaoException"/> com
/// <see cref="FalhaTransmissaoException.PodeTerSidoProcessado"/>. O identificador é o da DPS enviada
/// (<see cref="IdentificadorDps.Montar"/> com o município, o documento, a série e o número dela, ou o <c>Id</c> do
/// <c>infDPS</c> no rastro).
/// </para>
/// <para>
/// O resultado diz o que fazer: <see cref="RecuperacaoNFSe.Gerada"/>, guardar a NFS-e da segunda consulta;
/// <see cref="RecuperacaoNFSe.NaoGerada"/>, reenviar a mesma DPS (o mesmo número); nenhum dos dois (a consulta rejeitada, a
/// NFS-e que a chave não acha), olhar os resultados. A falha de transmissão de uma das consultas sobe como veio: tentar de
/// novo depois.
/// </para>
/// </remarks>
public static class RecuperacaoEmissao
{
    /// <summary>Consulta a DPS e, gerada a NFS-e, a NFS-e.</summary>
    /// <param name="dfe">O núcleo da NFS-e (o mesmo da emissão).</param>
    /// <param name="id">O identificador da DPS enviada.</param>
    /// <param name="cancellationToken">O cancelamento.</param>
    /// <returns>Os dois resultados, com o que eles dizem.</returns>
    /// <exception cref="ArgumentNullException">O núcleo é nulo.</exception>
    /// <exception cref="ArgumentException">O identificador é o valor padrão (vazio).</exception>
    /// <exception cref="FalhaTransmissaoException">Uma das consultas não chegou à Sefin, ou a resposta não se lê.</exception>
    public static async Task<RecuperacaoNFSe> ExecutarAsync(DFe dfe, IdentificadorDps id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dfe);
        var consulta = new ConsultaDps(id);
        var dps = await dfe.ExecutarAsync(consulta, cancellationToken).ConfigureAwait(false);
        if (!dps.Processado || !dps.Retorno.Existe || dps.Retorno.Chave is not { } chave)
            return new RecuperacaoNFSe(dps, null);
        var nota = await dfe.ExecutarAsync(new ConsultaNFSe(chave), cancellationToken).ConfigureAwait(false);
        return new RecuperacaoNFSe(dps, nota);
    }
}

/// <summary>O resultado da <see cref="RecuperacaoEmissao"/>: a consulta da DPS e, se ela gerou a NFS-e, a consulta da NFS-e.</summary>
public sealed class RecuperacaoNFSe
{
    internal RecuperacaoNFSe(ResultadoWS<RetornoConsultaDps> dps, ResultadoWS<RetornoConsultaNFSe>? nota)
    {
        Dps = dps;
        Nota = nota;
    }

    /// <summary>A consulta da DPS (o GET).</summary>
    public ResultadoWS<RetornoConsultaDps> Dps { get; }

    /// <summary>A consulta da NFS-e pela chave; <see langword="null"/> se a DPS não gerou a NFS-e.</summary>
    public ResultadoWS<RetornoConsultaNFSe>? Nota { get; }

    /// <summary>A DPS gerou a NFS-e, e ela está em <see cref="Nota"/> (o XML e a nota lida).</summary>
    public bool Gerada => Nota is { Processado: true, Retorno.Encontrada: true };

    /// <summary>
    /// A Sefin respondeu que a DPS não gerou a NFS-e (o 404): a emissão não foi processada, e a mesma DPS pode ser reenviada.
    /// </summary>
    public bool NaoGerada => Dps is { Processado: true, CStat: 404 } && !Dps.Retorno.Existe;
}
