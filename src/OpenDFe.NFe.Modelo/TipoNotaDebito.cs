namespace OpenDFe.NFe;

/// <summary>O tipo da nota de débito (NT 2025.002). O valor de cada membro é o do XML (<c>tpNFDebito</c>).</summary>
public enum TipoNotaDebito
{
    /// <summary>Transferência de créditos para cooperativas (1).</summary>
    TransferenciaCreditoCooperativa = 1,

    /// <summary>Anulação de crédito por saídas imunes ou isentas (2).</summary>
    AnulacaoCreditoSaidaImune = 2,

    /// <summary>Débitos de notas fiscais não processadas na apuração (3).</summary>
    DebitoNaoProcessado = 3,

    /// <summary>Multa e juros (4).</summary>
    MultaJuros = 4,

    /// <summary>Transferência de crédito na sucessão (5).</summary>
    TransferenciaCreditoSucessao = 5,

    /// <summary>Pagamento antecipado (6).</summary>
    PagamentoAntecipado = 6,

    /// <summary>Perda em estoque (7).</summary>
    PerdaEstoque = 7,

    /// <summary>Desenquadramento do Simples Nacional (8).</summary>
    DesenquadramentoSimples = 8,
}
