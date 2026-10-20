namespace OpenDFe.NFe;

/// <summary>O imposto de importação do item (<c>II</c>).</summary>
public sealed class ImpostoImportacao
{
    /// <summary>A base de cálculo.</summary>
    [Tag("vBC", "P02")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>As despesas aduaneiras.</summary>
    [Tag("vDespAdu", "P03")]
    public decimal? DespesasAduaneiras { get; set; }

    /// <summary>O valor do imposto de importação.</summary>
    [Tag("vII", "P04")]
    public decimal? Valor { get; set; }

    /// <summary>O valor do IOF.</summary>
    [Tag("vIOF", "P05")]
    public decimal? Iof { get; set; }
}
