namespace OpenDFe.NFe;

/// <summary>A tributação do IBS e da CBS (<c>gIBSCBS</c>).</summary>
public sealed class TributacaoIbsCbs
{
    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "UB16")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O IBS da UF.</summary>
    [Tag("gIBSUF", "UB17")]
    public IbsUF IbsUF { get; set; } = new();

    /// <summary>O IBS do município.</summary>
    [Tag("gIBSMun", "UB36")]
    public IbsMunicipio IbsMunicipio { get; set; } = new();

    /// <summary>O valor do IBS (o da UF mais o do município).</summary>
    [Tag("vIBS", "UB54a")]
    public decimal? ValorIbs { get; set; }

    /// <summary>A CBS.</summary>
    [Tag("gCBS", "UB55")]
    public Cbs Cbs { get; set; } = new();

    /// <summary>A tributação regular, se não cumprida a condição resolutória ou suspensiva; opcional.</summary>
    [Tag("gTribRegular", "UB68")]
    public TributacaoRegular? TributacaoRegular { get; set; }

    /// <summary>A composição do IBS e da CBS na compra governamental; opcional.</summary>
    [Tag("gTribCompraGov", "UB82a")]
    public TributacaoCompraGovernamental? CompraGovernamental { get; set; }
}
