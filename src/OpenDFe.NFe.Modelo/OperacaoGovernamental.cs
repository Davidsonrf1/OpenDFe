namespace OpenDFe.NFe;

/// <summary>
/// O tipo da operação com o ente governamental (NT 2025.002). O valor de cada membro é o do XML (<c>tpOperGov</c>).
/// </summary>
public enum OperacaoGovernamental
{
    /// <summary>Fornecimento com pagamento posterior (1).</summary>
    FornecimentoPagamentoPosterior = 1,

    /// <summary>Recebimento do pagamento com fornecimento já realizado (2).</summary>
    RecebimentoFornecimentoRealizado = 2,

    /// <summary>Fornecimento com pagamento já realizado (3).</summary>
    FornecimentoPagamentoRealizado = 3,

    /// <summary>Recebimento do pagamento com fornecimento posterior (4).</summary>
    RecebimentoFornecimentoPosterior = 4,
}
