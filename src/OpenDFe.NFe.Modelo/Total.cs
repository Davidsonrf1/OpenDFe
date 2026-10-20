namespace OpenDFe.NFe;

/// <summary>Os totais da nota (<c>total</c>).</summary>
/// <remarks>
/// Preenchidos por quem emite (ou pela leitura): o modelo não os calcula, e a conferência contra a soma dos itens é
/// regra (rejeições 531 a 535, 564 e as da reforma).
/// </remarks>
public sealed class Total
{
    /// <summary>Os totais do ICMS e da nota.</summary>
    [Tag("ICMSTot", "W02")]
    public TotalIcms Icms { get; set; } = new();

    /// <summary>Os totais do ISSQN; opcional.</summary>
    [Tag("ISSQNtot", "W17")]
    public TotalIssqn? Issqn { get; set; }

    /// <summary>As retenções de tributos; opcional.</summary>
    [Tag("retTrib", "W23")]
    public RetencaoTributos? Retencao { get; set; }

    /// <summary>O total do imposto seletivo; opcional.</summary>
    [Tag("ISTot", "W31")]
    public TotalImpostoSeletivo? ImpostoSeletivo { get; set; }

    /// <summary>Os totais do IBS e da CBS; opcional.</summary>
    [Tag("IBSCBSTot", "W34")]
    public TotalIbsCbs? IbsCbs { get; set; }

    /// <summary>O valor total da nota com o IBS, a CBS e o IS, que são cobrados por fora; opcional.</summary>
    [Tag("vNFTot", "W60")]
    public decimal? ValorTotalNota { get; set; }
}
