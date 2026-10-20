namespace OpenDFe.NFe;

/// <summary>As informações de compra (<c>compra</c>).</summary>
public sealed class Compra
{
    /// <summary>A nota de empenho (compras públicas); opcional.</summary>
    [Tag("xNEmp", "ZB02")]
    public string? NotaEmpenho { get; set; }

    /// <summary>O pedido; opcional.</summary>
    [Tag("xPed", "ZB03")]
    public string? Pedido { get; set; }

    /// <summary>O contrato; opcional.</summary>
    [Tag("xCont", "ZB04")]
    public string? Contrato { get; set; }
}
