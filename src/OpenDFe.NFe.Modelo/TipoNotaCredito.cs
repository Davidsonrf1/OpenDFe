namespace OpenDFe.NFe;

/// <summary>O tipo da nota de crédito (NT 2025.002). O valor de cada membro é o do XML (<c>tpNFCredito</c>).</summary>
public enum TipoNotaCredito
{
    /// <summary>Multa e juros (1).</summary>
    MultaJuros = 1,

    /// <summary>Apropriação de crédito presumido de IBS sobre o saldo devedor na ZFM (2).</summary>
    CreditoPresumidoZfm = 2,

    /// <summary>Retorno por recusa na entrega ou por não localização do destinatário (3).</summary>
    RetornoRecusaEntrega = 3,

    /// <summary>Redução de valores (4).</summary>
    ReducaoValores = 4,

    /// <summary>Transferência de crédito na sucessão (5).</summary>
    TransferenciaCreditoSucessao = 5,

    /// <summary>Retorno de mercadoria por recusa parcial (6).</summary>
    RetornoRecusaParcial = 6,
}
