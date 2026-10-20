namespace OpenDFe.NFe;

/// <summary>A CBS com alíquota zero em área incentivada, ALC ou ZFM (<c>gALCZFMCBS</c>).</summary>
public sealed class CbsAreaIncentivada
{
    /// <summary>O tipo de aplicação da alíquota zero da CBS.</summary>
    [Tag("tpALCZFMCBS", "UB66b")]
    public AplicacaoAliquotaZeroCbs? Tipo { get; set; }

    /// <summary>O número do processo na SUFRAMA para o item; opcional.</summary>
    [Tag("nProcSuframa", "UB66c")]
    public string? ProcessoSuframa { get; set; }

    /// <summary>A alíquota efetiva de referência da CBS fora das áreas incentivadas.</summary>
    [Tag("pAliqEfetRegCBS", "UB66d")]
    public decimal? AliquotaEfetiva { get; set; }

    /// <summary>O valor da CBS calculado como fora das áreas incentivadas.</summary>
    [Tag("vTribRegCBS", "UB66e")]
    public decimal? Valor { get; set; }
}
