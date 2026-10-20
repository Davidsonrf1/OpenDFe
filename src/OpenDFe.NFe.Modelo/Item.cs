namespace OpenDFe.NFe;

/// <summary>Um item da nota (<c>det</c>): o produto ou serviço, os tributos e o que vem com eles.</summary>
/// <remarks>
/// O <c>nItem</c> do XML não tem propriedade: é a posição do item em <see cref="NotaFiscal.Itens"/>, a partir de 1 (a
/// escrita numera de 1 a n, a leitura confere a sequência).
/// </remarks>
public sealed class Item
{
    /// <summary>O produto ou serviço.</summary>
    [Tag("prod", "I01")]
    public Produto Produto { get; set; } = new();

    /// <summary>Os tributos do item.</summary>
    [Tag("imposto", "M01")]
    public Imposto Imposto { get; set; } = new();

    /// <summary>O imposto devolvido; opcional.</summary>
    [Tag("impostoDevol", "UA01")]
    public ImpostoDevolvido? ImpostoDevolvido { get; set; }

    /// <summary>As informações adicionais do produto; opcional.</summary>
    [Tag("infAdProd", "V01")]
    public string? InformacoesAdicionais { get; set; }

    /// <summary>As observações do item; opcional.</summary>
    [Tag("obsItem", "VA01")]
    public ObservacoesItem? Observacoes { get; set; }

    /// <summary>O valor total do item, a participação dele no total da nota; opcional.</summary>
    [Tag("vItem", "VB01")]
    public decimal? ValorTotal { get; set; }

    /// <summary>O item de outro documento a que este se refere; opcional.</summary>
    [Tag("DFeReferenciado", "VC01")]
    public ItemReferenciado? Referencia { get; set; }
}
