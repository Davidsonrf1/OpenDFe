namespace OpenDFe.NFe;

/// <summary>A CBS monofásica ad valorem (<c>gCBSMonoAdValorem</c>).</summary>
public sealed class MonofasiaCbsAdValorem
{
    /// <summary>A tributação monofásica padrão; opcional.</summary>
    [Tag("gMonoPadrao", "UB101")]
    public MonofasiaCbsAdValoremPadrao? Padrao { get; set; }

    /// <summary>A tributação monofásica sujeita à retenção; opcional.</summary>
    [Tag("gMonoReten", "UB102")]
    public MonofasiaCbsAdValoremRetencao? Retencao { get; set; }

    /// <summary>A tributação monofásica retida anteriormente; opcional.</summary>
    [Tag("gMonoRet", "UB103")]
    public MonofasiaCbsRetido? Retido { get; set; }

    /// <summary>A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional.</summary>
    [Tag("gpBioDiferenca", "UB104")]
    public MonofasiaCbsBiocombustivel? Biocombustivel { get; set; }
}
