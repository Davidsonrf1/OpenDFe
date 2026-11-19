namespace OpenDFe.NFSe;

/// <summary>O PIS e a COFINS (<c>piscofins</c>).</summary>
public sealed class PisCofins
{
    /// <summary>O código de situação tributária.</summary>
    [Tag("CST")]
    public CstPisCofinsServico? Cst { get; set; }

    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBCPisCofins")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota do PIS (%); opcional.</summary>
    [Tag("pAliqPis")]
    public decimal? AliquotaPis { get; set; }

    /// <summary>A alíquota da COFINS (%); opcional.</summary>
    [Tag("pAliqCofins")]
    public decimal? AliquotaCofins { get; set; }

    /// <summary>O valor do PIS; opcional.</summary>
    [Tag("vPis")]
    public decimal? ValorPis { get; set; }

    /// <summary>O valor da COFINS; opcional.</summary>
    [Tag("vCofins")]
    public decimal? ValorCofins { get; set; }

    /// <summary>A retenção do PIS, da COFINS e da CSLL; opcional.</summary>
    [Tag("tpRetPisCofins")]
    public RetencaoPisCofins? Retencao { get; set; }
}
