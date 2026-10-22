namespace OpenDFe.NFe;

/// <summary>
/// A consulta da situação de uma nota pela chave (<c>NFeConsultaProtocolo4</c>, docs/CONTRATOS.md 15): o
/// <c>consSitNFe</c> e o <c>retConsSitNFe</c>, com o protocolo e os eventos da nota.
/// </summary>
/// <remarks>
/// A consulta vai ao autorizador da UF e do tipo de emissão da chave (PROJETO.md 8.4): a nota emitida em SVC-AN é
/// consultada na SVC-AN, que só acha as que ela autorizou; o <c>Destino</c> do parâmetro sobrepõe, para
/// consultar no autorizador normal, que acha também as da SVC. A chave de outro modelo que o do núcleo (a NFC-e num
/// núcleo de NF-e) é recusada antes de transmitir (<c>NFE-CHAVE</c>, decisão 3).
/// </remarks>
public sealed class ConsultaProtocolo : ParametroWS<RetornoProtocolo>
{
    /// <summary>A consulta da nota da chave.</summary>
    /// <param name="chave">A chave de acesso da nota.</param>
    /// <exception cref="ArgumentNullException">A chave é nula.</exception>
    public ConsultaProtocolo(ChaveAcesso chave)
    {
        ArgumentNullException.ThrowIfNull(chave);
        Chave = chave;
    }

    /// <summary>A chave de acesso consultada (<c>chNFe</c>).</summary>
    public ChaveAcesso Chave { get; }
}
