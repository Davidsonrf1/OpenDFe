namespace OpenDFe.NFe;

/// <summary>
/// O tipo de aplicação da alíquota zero da CBS em área incentivada (ALC ou ZFM). O valor de cada membro é o do XML
/// (<c>tpALCZFMCBS</c>).
/// </summary>
public enum AplicacaoAliquotaZeroCbs
{
    /// <summary>
    /// Fornecedor e destinatário em área incentivada, sem operação industrial com processo aprovado na SUFRAMA para o
    /// item (1).
    /// </summary>
    AreaIncentivada = 1,

    /// <summary>
    /// Fornecedor e destinatário em área incentivada, em operação industrial com processo aprovado na SUFRAMA para o
    /// item (2).
    /// </summary>
    ProcessoSuframa = 2,
}
