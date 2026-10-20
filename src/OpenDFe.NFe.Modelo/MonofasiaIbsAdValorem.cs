namespace OpenDFe.NFe;

/// <summary>O IBS monofásico ad valorem (<c>gIBSMonoAdValorem</c>).</summary>
public sealed class MonofasiaIbsAdValorem
{
    /// <summary>A tributação monofásica padrão; opcional.</summary>
    [Tag("gMonoPadrao", "UB91")]
    public MonofasiaIbsAdValoremPadrao? Padrao { get; set; }

    /// <summary>A tributação monofásica sujeita à retenção; opcional.</summary>
    [Tag("gMonoReten", "UB92")]
    public MonofasiaIbsAdValoremRetencao? Retencao { get; set; }

    /// <summary>A tributação monofásica retida anteriormente; opcional.</summary>
    [Tag("gMonoRet", "UB93")]
    public MonofasiaIbsRetido? Retido { get; set; }

    /// <summary>A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional.</summary>
    [Tag("gpBioDiferenca", "UB94")]
    public MonofasiaIbsBiocombustivel? Biocombustivel { get; set; }
}
