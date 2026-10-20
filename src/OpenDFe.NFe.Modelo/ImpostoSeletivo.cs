namespace OpenDFe.NFe;

/// <summary>O imposto seletivo do item (<c>IS</c>, NT 2025.002).</summary>
/// <remarks>
/// O CST e a classificação tributária são códigos de tabela publicada à parte e ficam como texto, com os zeros à
/// esquerda.
/// </remarks>
public sealed class ImpostoSeletivo
{
    /// <summary>O CST do imposto seletivo, como no XML (com os zeros à esquerda).</summary>
    [Tag("CSTIS", "UB02")]
    public string? Cst { get; set; }

    /// <summary>A classificação tributária do imposto seletivo, como no XML.</summary>
    [Tag("cClassTribIS", "UB03")]
    public string? ClassificacaoTributaria { get; set; }

    /// <summary>A base de cálculo; opcional.</summary>
    [Tag("vBCIS", "UB05")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>A alíquota, em percentual; opcional.</summary>
    [Tag("pIS", "UB06")]
    public decimal? Aliquota { get; set; }

    /// <summary>A alíquota específica, por unidade de medida; opcional.</summary>
    [Tag("adRemIS", "UB07")]
    public decimal? AliquotaEspecifica { get; set; }

    /// <summary>A unidade de medida tributável do imposto seletivo; opcional.</summary>
    [Tag("uTrib", "UB09")]
    public string? UnidadeTributavel { get; set; }

    /// <summary>A quantidade na unidade tributável do imposto seletivo; opcional.</summary>
    [Tag("qTrib", "UB10")]
    public decimal? QuantidadeTributavel { get; set; }

    /// <summary>O valor do imposto seletivo; opcional.</summary>
    [Tag("vIS", "UB11")]
    public decimal? Valor { get; set; }
}
