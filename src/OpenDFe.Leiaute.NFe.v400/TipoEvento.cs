namespace OpenDFe.NFe;

/// <summary>O tipo de um evento (<c>tpEvento</c>, MOC 7.0, Visão Geral, 3.1): o código de 6 dígitos.</summary>
public enum TipoEvento
{
    /// <summary>Carta de correção (110110).</summary>
    CartaCorrecao = 110110,

    /// <summary>Cancelamento (110111).</summary>
    Cancelamento = 110111,

    /// <summary>Cancelamento por substituição, da NFC-e (110112).</summary>
    CancelamentoSubstituicao = 110112,

    /// <summary>Confirmação da operação, do destinatário (210200).</summary>
    ConfirmacaoOperacao = 210200,

    /// <summary>Ciência da operação, do destinatário (210210).</summary>
    CienciaOperacao = 210210,

    /// <summary>Desconhecimento da operação, do destinatário (210220).</summary>
    DesconhecimentoOperacao = 210220,

    /// <summary>Operação não realizada, do destinatário (210240).</summary>
    OperacaoNaoRealizada = 210240,
}
