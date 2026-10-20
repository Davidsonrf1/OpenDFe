namespace OpenDFe.NFe;

/// <summary>As informações do produto na NFF, nota fiscal fácil (<c>infProdNFF</c>).</summary>
public sealed class ProdutoNff
{
    /// <summary>O código fiscal do produto.</summary>
    [Tag("cProdFisco", "I86a")]
    public string? CodigoFisco { get; set; }

    /// <summary>O código da operação escolhida na NFF para o item.</summary>
    [Tag("cOperNFF", "I86b")]
    public int? CodigoOperacao { get; set; }
}
