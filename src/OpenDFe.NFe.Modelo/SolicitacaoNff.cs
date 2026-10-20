namespace OpenDFe.NFe;

/// <summary>A solicitação da NFF, nota fiscal fácil (<c>infSolicNFF</c>).</summary>
public sealed class SolicitacaoNff
{
    /// <summary>A solicitação do pedido de emissão da NFF.</summary>
    [Tag("xSolic", "ZE02")]
    public string? Texto { get; set; }
}
