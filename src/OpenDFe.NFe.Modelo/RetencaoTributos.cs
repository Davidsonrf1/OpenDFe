namespace OpenDFe.NFe;

/// <summary>As retenções de tributos (<c>retTrib</c>).</summary>
public sealed class RetencaoTributos
{
    /// <summary>O valor retido do PIS; opcional.</summary>
    [Tag("vRetPIS", "W24")]
    public decimal? Pis { get; set; }

    /// <summary>O valor retido da COFINS; opcional.</summary>
    [Tag("vRetCOFINS", "W25")]
    public decimal? Cofins { get; set; }

    /// <summary>O valor retido da CSLL; opcional.</summary>
    [Tag("vRetCSLL", "W26")]
    public decimal? Csll { get; set; }

    /// <summary>A base de cálculo do IRRF; opcional.</summary>
    [Tag("vBCIRRF", "W27")]
    public decimal? BaseCalculoIrrf { get; set; }

    /// <summary>O valor retido do IRRF; opcional.</summary>
    [Tag("vIRRF", "W28")]
    public decimal? Irrf { get; set; }

    /// <summary>A base de cálculo da retenção da previdência social; opcional.</summary>
    [Tag("vBCRetPrev", "W29")]
    public decimal? BaseCalculoPrevidencia { get; set; }

    /// <summary>O valor retido da previdência social; opcional.</summary>
    [Tag("vRetPrev", "W30")]
    public decimal? Previdencia { get; set; }
}
