namespace OpenDFe.NFe;

/// <summary>O pagamento (<c>pag</c>).</summary>
public sealed class Pagamento
{
    /// <summary>As formas de pagamento (de 1 a 100).</summary>
    [Tag("detPag", "YA01a")]
    public IList<FormaPagamento> Formas { get; } = new List<FormaPagamento>();

    /// <summary>O valor do troco; opcional.</summary>
    [Tag("vTroco", "YA09")]
    public decimal? Troco { get; set; }
}
