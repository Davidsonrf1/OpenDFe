namespace OpenDFe.NFSe;

/// <summary>Os itens do pedido (<c>gItemPed</c>).</summary>
public sealed class ItensPedido
{
    /// <summary>Os números dos itens do pedido (1 a 99).</summary>
    [Tag("xItemPed")]
    public IList<string> Itens { get; } = new List<string>();
}
