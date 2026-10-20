namespace OpenDFe.NFe;

/// <summary>O IBS monofásico ad rem (<c>gIBSMonoAdRem</c>).</summary>
public sealed class MonofasiaIbsAdRem
{
    /// <summary>A tributação monofásica padrão; opcional.</summary>
    [Tag("gMonoPadrao", "UB86")]
    public MonofasiaIbsAdRemPadrao? Padrao { get; set; }

    /// <summary>A tributação monofásica sujeita à retenção; opcional.</summary>
    [Tag("gMonoReten", "UB87")]
    public MonofasiaIbsAdRemRetencao? Retencao { get; set; }

    /// <summary>A tributação monofásica retida anteriormente; opcional.</summary>
    [Tag("gMonoRet", "UB88")]
    public MonofasiaIbsRetido? Retido { get; set; }

    /// <summary>A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional.</summary>
    [Tag("gpBioDiferenca", "UB89")]
    public MonofasiaIbsBiocombustivel? Biocombustivel { get; set; }
}
