namespace OpenDFe.NFe;

/// <summary>O ajuste de competência do IBS e da CBS (<c>gAjusteCompet</c>, CST 811).</summary>
public sealed class AjusteCompetencia
{
    /// <summary>O ano e o mês da competência de apuração, como no XML (<c>AAAA-MM</c>).</summary>
    [Tag("competApur", "UB113")]
    public string? Competencia { get; set; }

    /// <summary>O valor do IBS do ajuste.</summary>
    [Tag("vIBS", "UB114")]
    public decimal? ValorIbs { get; set; }

    /// <summary>O valor da CBS do ajuste.</summary>
    [Tag("vCBS", "UB115")]
    public decimal? ValorCbs { get; set; }
}
