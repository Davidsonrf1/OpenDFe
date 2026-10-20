namespace OpenDFe.NFe;

/// <summary>A tributação monofásica padrão da CBS ad valorem (<c>gMonoPadrao</c>).</summary>
public sealed class MonofasiaCbsAdValoremPadrao
{
    /// <summary>A base de cálculo da tributação monofásica.</summary>
    [Tag("vBCMono", "UB101a")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota da CBS.</summary>
    [Tag("pAliqMonoCBS", "UB101b")]
    public decimal? Aliquota { get; set; }

    /// <summary>O valor da CBS monofásica.</summary>
    [Tag("vCBSMono", "UB101c")]
    public decimal? Valor { get; set; }
}
