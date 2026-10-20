namespace OpenDFe.NFe;

/// <summary>O ICMS isento, não tributado ou suspenso (<c>ICMS40</c>, CST 40, 41 ou 50).</summary>
[Tag("ICMS40", "N06")]
public sealed class Icms40 : Icms
{
    /// <summary>O CST: <c>40</c>, <c>41</c> ou <c>50</c>.</summary>
    [Tag("CST", "N12")]
    public CstIcms? Cst { get; set; }

    /// <summary>O valor do ICMS desonerado; opcional.</summary>
    [Tag("vICMSDeson", "N28a")]
    public decimal? ValorDesonerado { get; set; }

    /// <summary>O motivo da desoneração do ICMS; opcional.</summary>
    [Tag("motDesICMS", "N28")]
    public MotivoDesoneracao? MotivoDesoneracao { get; set; }

    /// <summary>Se o ICMS desonerado deduz do valor do item e do total da nota; opcional.</summary>
    [Tag("indDeduzDeson", "N28b")]
    public IndicadorDeducaoDesoneracao? DeducaoDesoneracao { get; set; }
}
