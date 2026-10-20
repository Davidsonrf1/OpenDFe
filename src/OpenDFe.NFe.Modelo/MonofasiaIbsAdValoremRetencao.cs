namespace OpenDFe.NFe;

/// <summary>A tributação monofásica sujeita à retenção do IBS ad valorem (<c>gMonoReten</c>).</summary>
public sealed class MonofasiaIbsAdValoremRetencao
{
    /// <summary>A base de cálculo da tributação monofásica sujeita à retenção.</summary>
    [Tag("vBCMonoReten", "UB92a")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota da tributação monofásica sujeita à retenção.</summary>
    [Tag("pAliqMonoReten", "UB92b")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor do IBS monofásico sujeito à retenção.</summary>
    [Tag("vIBSMonoReten", "UB92c")]
    public decimal? Valor { get; set; }
}
