namespace OpenDFe.NFSe;

/// <summary>As opções do DANFSe (<see cref="LeiauteNFSe101.Danfse"/>).</summary>
/// <remarks>Imutável (docs/NFSE.md 12, decisões G5 e G7).</remarks>
public sealed class OpcoesDanfse
{
    /// <summary>
    /// Se o canhoto sai (a data de cientificação, a identificação e assinatura e o número e a chave da NFS-e, no pé da folha);
    /// verdadeiro por padrão, como o Anexo I da NT 008/2026. Sem ele, a altura vai às informações complementares (2.3.3).
    /// </summary>
    public bool Canhoto { get; init; } = true;

    /// <summary>
    /// Se a NFS-e foi cancelada: o DANFSe sai com a marca d'água <c>CANCELADA</c> (NT 008/2026 2.5.1). A situação vem de quem
    /// usa (o evento 101101 na <see cref="ConsultaEventosNFSe"/>); a lib não a deduz.
    /// </summary>
    public bool Cancelada { get; init; }

    /// <summary>
    /// Se a NFS-e foi substituída: o DANFSe sai com a marca d'água <c>SUBSTITUÍDA</c> (NT 008/2026 2.5.2). A situação vem de
    /// quem usa (o evento 105102 na <see cref="ConsultaEventosNFSe"/>); a lib não a deduz.
    /// </summary>
    public bool Substituida { get; init; }
}
