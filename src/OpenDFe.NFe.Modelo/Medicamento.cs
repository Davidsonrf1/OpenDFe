namespace OpenDFe.NFe;

/// <summary>O medicamento ou a matéria-prima farmacêutica (<c>med</c>).</summary>
public sealed class Medicamento
{
    /// <summary>O número do registro na ANVISA, ou <c>ISENTO</c>.</summary>
    [Tag("cProdANVISA", "K01a")]
    public string? CodigoAnvisa { get; set; }

    /// <summary>O motivo da isenção do registro na ANVISA; opcional.</summary>
    [Tag("xMotivoIsencao", "K01b")]
    public string? MotivoIsencao { get; set; }

    /// <summary>O preço máximo ao consumidor.</summary>
    [Tag("vPMC", "K06")]
    public decimal? PrecoMaximoConsumidor { get; set; }
}
