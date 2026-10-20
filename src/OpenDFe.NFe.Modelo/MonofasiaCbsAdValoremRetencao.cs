namespace OpenDFe.NFe;

/// <summary>A tributação monofásica sujeita à retenção da CBS ad valorem (<c>gMonoReten</c>).</summary>
public sealed class MonofasiaCbsAdValoremRetencao
{
    /// <summary>A base de cálculo da tributação monofásica sujeita à retenção.</summary>
    [Tag("vBCMonoReten", "UB102a")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota da tributação monofásica sujeita à retenção.</summary>
    [Tag("pAliqMonoReten", "UB102b")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor da CBS monofásica sujeita à retenção.</summary>
    [Tag("vCBSMonoReten", "UB102c")]
    public decimal? Valor { get; set; }
}
