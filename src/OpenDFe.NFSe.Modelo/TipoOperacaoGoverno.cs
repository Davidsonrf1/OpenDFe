namespace OpenDFe.NFSe;

/// <summary>O tipo de operação com ente governamental ou sobre bens imóveis (<c>tpOper</c>). O valor de cada membro é o do XML.</summary>
public enum TipoOperacaoGoverno
{
    /// <summary>Fornecimento com pagamento posterior (1).</summary>
    FornecimentoPagamentoPosterior = 1,

    /// <summary>Recebimento do pagamento com fornecimento já realizado (2).</summary>
    PagamentoFornecimentoRealizado = 2,

    /// <summary>Fornecimento com pagamento já realizado (3).</summary>
    FornecimentoPagamentoRealizado = 3,

    /// <summary>Recebimento do pagamento com fornecimento posterior (4).</summary>
    PagamentoFornecimentoPosterior = 4,

    /// <summary>Fornecimento e recebimento do pagamento concomitantes (5).</summary>
    Concomitantes = 5,
}
