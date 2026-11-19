namespace OpenDFe.NFSe;

/// <summary>Os tributos federais (<c>tribFed</c>).</summary>
public sealed class TributacaoFederal
{
    /// <summary>O PIS e a COFINS; opcional.</summary>
    [Tag("piscofins")]
    public PisCofins? PisCofins { get; set; }

    /// <summary>A contribuição previdenciária retida; opcional.</summary>
    [Tag("vRetCP")]
    public decimal? RetencaoCp { get; set; }

    /// <summary>O IRRF retido; opcional.</summary>
    [Tag("vRetIRRF")]
    public decimal? RetencaoIrrf { get; set; }

    /// <summary>A CSLL retida; opcional.</summary>
    [Tag("vRetCSLL")]
    public decimal? RetencaoCsll { get; set; }
}
