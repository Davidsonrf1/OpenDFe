namespace OpenDFe.NFe;

/// <summary>A transferência de crédito do IBS e da CBS (<c>gTransfCred</c>, CST 800).</summary>
public sealed class TransferenciaCredito
{
    /// <summary>O valor do IBS a transferir.</summary>
    [Tag("vIBS", "UB107")]
    public decimal? ValorIbs { get; set; }

    /// <summary>O valor da CBS a transferir.</summary>
    [Tag("vCBS", "UB108")]
    public decimal? ValorCbs { get; set; }
}
