namespace OpenDFe.NFe;

/// <summary>O combustível líquido ou gasoso (<c>comb</c>).</summary>
public sealed class Combustivel
{
    /// <summary>O código do produto na ANP.</summary>
    [Tag("cProdANP", "LA02")]
    public string? CodigoAnp { get; set; }

    /// <summary>A descrição do produto na ANP.</summary>
    [Tag("descANP", "LA03")]
    public string? DescricaoAnp { get; set; }

    /// <summary>O percentual do GLP derivado do petróleo; opcional.</summary>
    [Tag("pGLP", "LA03a")]
    public decimal? PercentualGlp { get; set; }

    /// <summary>O percentual de gás natural nacional; opcional.</summary>
    [Tag("pGNn", "LA03b")]
    public decimal? PercentualGasNacional { get; set; }

    /// <summary>O percentual de gás natural importado; opcional.</summary>
    [Tag("pGNi", "LA03c")]
    public decimal? PercentualGasImportado { get; set; }

    /// <summary>O valor de partida, por quilograma, sem ICMS; opcional.</summary>
    [Tag("vPart", "LA03d")]
    public decimal? ValorPartida { get; set; }

    /// <summary>O código de autorização ou registro do CODIF; opcional.</summary>
    [Tag("CODIF", "LA04")]
    public string? Codif { get; set; }

    /// <summary>A quantidade faturada à temperatura ambiente; opcional.</summary>
    [Tag("qTemp", "LA05")]
    public decimal? QuantidadeTemperaturaAmbiente { get; set; }

    /// <summary>A UF de consumo.</summary>
    [Tag("UFCons", "LA06")]
    public UF? UFConsumo { get; set; }

    /// <summary>
    /// Se a UF de consumo do XML é <c>EX</c> (exterior); com ela, <see cref="UFConsumo"/> fica nulo. Sem
    /// <see cref="TagAttribute"/>: é outro valor do mesmo campo <c>UFCons</c>.
    /// </summary>
    public bool Exterior { get; set; }

    /// <summary>A CIDE; opcional.</summary>
    [Tag("CIDE", "LA07")]
    public Cide? Cide { get; set; }

    /// <summary>O encerrante; opcional.</summary>
    [Tag("encerrante", "LA11")]
    public Encerrante? Encerrante { get; set; }

    /// <summary>O percentual do índice de mistura do biodiesel; opcional.</summary>
    [Tag("pBio", "LA17")]
    public decimal? PercentualBiodiesel { get; set; }

    /// <summary>As origens do combustível (até 30).</summary>
    [Tag("origComb", "LA18")]
    public IList<OrigemCombustivel> Origens { get; } = new List<OrigemCombustivel>();
}
