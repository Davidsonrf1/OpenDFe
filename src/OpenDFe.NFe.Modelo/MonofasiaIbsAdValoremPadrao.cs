namespace OpenDFe.NFe;

/// <summary>A tributação monofásica padrão do IBS ad valorem (<c>gMonoPadrao</c>).</summary>
public sealed class MonofasiaIbsAdValoremPadrao
{
    /// <summary>A base de cálculo da tributação monofásica.</summary>
    [Tag("vBCMono", "UB91a")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota do IBS da UF.</summary>
    [Tag("pAliqMonoUF", "UB91b")]
    public decimal? AliquotaUF { get; set; }

    /// <summary>O valor do IBS monofásico da UF.</summary>
    [Tag("vIBSMonoUF", "UB91c")]
    public decimal? ValorUF { get; set; }

    /// <summary>A alíquota do IBS do município.</summary>
    [Tag("pAliqMonoMun", "UB91d")]
    public decimal? AliquotaMunicipio { get; set; }

    /// <summary>O valor do IBS monofásico do município.</summary>
    [Tag("vIBSMonoMun", "UB91e")]
    public decimal? ValorMunicipio { get; set; }

    /// <summary>O valor do IBS monofásico.</summary>
    [Tag("vIBSMono", "UB91f")]
    public decimal? Valor { get; set; }
}
