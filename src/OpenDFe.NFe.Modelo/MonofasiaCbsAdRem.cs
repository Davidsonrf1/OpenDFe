namespace OpenDFe.NFe;

/// <summary>A CBS monofásica ad rem (<c>gCBSMonoAdRem</c>).</summary>
public sealed class MonofasiaCbsAdRem
{
    /// <summary>A tributação monofásica padrão; opcional.</summary>
    [Tag("gMonoPadrao", "UB96")]
    public MonofasiaCbsAdRemPadrao? Padrao { get; set; }

    /// <summary>A tributação monofásica sujeita à retenção; opcional.</summary>
    [Tag("gMonoReten", "UB97")]
    public MonofasiaCbsAdRemRetencao? Retencao { get; set; }

    /// <summary>A tributação monofásica retida anteriormente; opcional.</summary>
    [Tag("gMonoRet", "UB98")]
    public MonofasiaCbsRetido? Retido { get; set; }

    /// <summary>A diferença pela mistura de biocombustível em percentual diferente do obrigatório; opcional.</summary>
    [Tag("gpBioDiferenca", "UB99")]
    public MonofasiaCbsBiocombustivel? Biocombustivel { get; set; }
}
