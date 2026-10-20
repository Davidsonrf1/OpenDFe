namespace OpenDFe.NFe;

/// <summary>O encerrante do abastecimento (<c>encerrante</c>).</summary>
public sealed class Encerrante
{
    /// <summary>O número do bico.</summary>
    [Tag("nBico", "LA12")]
    public int? Bico { get; set; }

    /// <summary>O número da bomba; opcional.</summary>
    [Tag("nBomba", "LA13")]
    public int? Bomba { get; set; }

    /// <summary>O número do tanque.</summary>
    [Tag("nTanque", "LA14")]
    public int? Tanque { get; set; }

    /// <summary>O encerrante no início do abastecimento.</summary>
    [Tag("vEncIni", "LA15")]
    public decimal? Inicial { get; set; }

    /// <summary>O encerrante no fim do abastecimento.</summary>
    [Tag("vEncFin", "LA16")]
    public decimal? Final { get; set; }
}
