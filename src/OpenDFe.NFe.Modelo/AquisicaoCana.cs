namespace OpenDFe.NFe;

/// <summary>A aquisição de cana (<c>cana</c>).</summary>
public sealed class AquisicaoCana
{
    /// <summary>A safra, como no XML (<c>AAAA</c> ou <c>AAAA/AAAA</c>).</summary>
    [Tag("safra", "ZC02")]
    public string? Safra { get; set; }

    /// <summary>O mês e o ano de referência, como no XML (<c>MM/AAAA</c>).</summary>
    [Tag("ref", "ZC03")]
    public string? Referencia { get; set; }

    /// <summary>O fornecimento diário (de 1 a 31).</summary>
    [Tag("forDia", "ZC04")]
    public IList<FornecimentoDiario> Fornecimentos { get; } = new List<FornecimentoDiario>();

    /// <summary>A quantidade total do mês.</summary>
    [Tag("qTotMes", "ZC07")]
    public decimal? QuantidadeMes { get; set; }

    /// <summary>A quantidade total anterior.</summary>
    [Tag("qTotAnt", "ZC08")]
    public decimal? QuantidadeAnterior { get; set; }

    /// <summary>A quantidade total geral.</summary>
    [Tag("qTotGer", "ZC09")]
    public decimal? QuantidadeGeral { get; set; }

    /// <summary>As deduções (até 10).</summary>
    [Tag("deduc", "ZC10")]
    public IList<Deducao> Deducoes { get; } = new List<Deducao>();

    /// <summary>O valor dos fornecimentos.</summary>
    [Tag("vFor", "ZC13")]
    public decimal? ValorFornecimentos { get; set; }

    /// <summary>O valor total das deduções.</summary>
    [Tag("vTotDed", "ZC14")]
    public decimal? ValorDeducoes { get; set; }

    /// <summary>O valor líquido dos fornecimentos.</summary>
    [Tag("vLiqFor", "ZC15")]
    public decimal? ValorLiquido { get; set; }
}
