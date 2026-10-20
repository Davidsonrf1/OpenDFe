namespace OpenDFe.NFe;

/// <summary>O crédito presumido da operação no IBS e na CBS (<c>gCredPresOper</c>).</summary>
public sealed class CreditoPresumidoOperacao
{
    /// <summary>A base de cálculo do crédito presumido.</summary>
    [Tag("vBCCredPres", "UB121")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O código de classificação do crédito presumido.</summary>
    [Tag("cCredPres", "UB122")]
    public string? Codigo { get; set; }

    /// <summary>O crédito presumido do IBS; opcional.</summary>
    [Tag("gIBSCredPres", "UB123")]
    public CreditoPresumidoIbsCbs? Ibs { get; set; }

    /// <summary>O crédito presumido da CBS; opcional.</summary>
    [Tag("gCBSCredPres", "UB127")]
    public CreditoPresumidoIbsCbs? Cbs { get; set; }
}
