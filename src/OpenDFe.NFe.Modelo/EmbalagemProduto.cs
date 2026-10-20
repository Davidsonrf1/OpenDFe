namespace OpenDFe.NFe;

/// <summary>A embalagem do produto (<c>infProdEmb</c>).</summary>
public sealed class EmbalagemProduto
{
    /// <summary>A embalagem.</summary>
    [Tag("xEmb", "I87a")]
    public string? Descricao { get; set; }

    /// <summary>O volume do produto na embalagem.</summary>
    [Tag("qVolEmb", "I87b")]
    public decimal? Volume { get; set; }

    /// <summary>A unidade de medida da embalagem.</summary>
    [Tag("uEmb", "I87c")]
    public string? Unidade { get; set; }
}
