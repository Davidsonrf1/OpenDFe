namespace OpenDFe.NFe;

/// <summary>O meio de pagamento. O valor de cada membro é o do XML (<c>tPag</c>).</summary>
/// <remarks>
/// A lista é a da Tabela de Meios de Pagamento do Portal Nacional da NF-e (IT 2024.002 v1.11, publicada em 04/03/2026),
/// que muda por informe técnico: a leitura de um código que não está aqui devolve o número (<c>(MeioPagamento)25</c>),
/// e a escrita escreve o número que estiver.
/// </remarks>
public enum MeioPagamento
{
    /// <summary>Dinheiro (1).</summary>
    Dinheiro = 1,

    /// <summary>Cheque (2).</summary>
    Cheque = 2,

    /// <summary>Cartão de crédito (3).</summary>
    CartaoCredito = 3,

    /// <summary>Cartão de débito (4).</summary>
    CartaoDebito = 4,

    /// <summary>Cartão da loja (private label), crediário digital e outros crediários (5).</summary>
    CartaoLoja = 5,

    /// <summary>Vale alimentação (10).</summary>
    ValeAlimentacao = 10,

    /// <summary>Vale refeição (11).</summary>
    ValeRefeicao = 11,

    /// <summary>Vale presente (12).</summary>
    ValePresente = 12,

    /// <summary>Vale combustível (13).</summary>
    ValeCombustivel = 13,

    /// <summary>Duplicata mercantil (14).</summary>
    DuplicataMercantil = 14,

    /// <summary>Boleto bancário (15).</summary>
    BoletoBancario = 15,

    /// <summary>Depósito bancário (16).</summary>
    DepositoBancario = 16,

    /// <summary>Pagamento instantâneo (PIX) com QR Code ou URL dinâmicos (17).</summary>
    PixDinamico = 17,

    /// <summary>TED (transferência eletrônica disponível), entre contas de bancos diferentes (18).</summary>
    Ted = 18,

    /// <summary>Programa de fidelidade, cashback, crédito virtual (19).</summary>
    ProgramaFidelidade = 19,

    /// <summary>Pagamento instantâneo (PIX) com QR Code estático, chave PIX ou agência e conta (20).</summary>
    PixEstatico = 20,

    /// <summary>Crédito em loja (valor pago antes, devolução de mercadoria) (21).</summary>
    CreditoLoja = 21,

    /// <summary>Pagamento eletrônico não informado, por falha de hardware do sistema emissor (22).</summary>
    PagamentoEletronicoNaoInformado = 22,

    /// <summary>Pagamento instantâneo (PIX) automático, recorrente (23).</summary>
    PixAutomatico = 23,

    /// <summary>TEF "book transfer", entre contas do mesmo banco (24).</summary>
    TefBookTransfer = 24,

    /// <summary>Sem pagamento (90).</summary>
    Nenhum = 90,

    /// <summary>Pagamento posterior à emissão (91).</summary>
    PagamentoPosterior = 91,

    /// <summary>Outros (99).</summary>
    Outros = 99,
}
