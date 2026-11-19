namespace OpenDFe.NFSe;

/// <summary>As informações complementares do serviço (<c>infoCompl</c>).</summary>
public sealed class InformacoesComplementares
{
    /// <summary>O documento de responsabilidade técnica (ART, RRT, DRT); opcional.</summary>
    [Tag("idDocTec")]
    public string? ResponsabilidadeTecnica { get; set; }

    /// <summary>O documento de referência (chave, número da nota, contrato); opcional.</summary>
    [Tag("docRef")]
    public string? DocumentoReferencia { get; set; }

    /// <summary>O número do pedido, da ordem de compra ou de serviço; opcional.</summary>
    [Tag("xPed")]
    public string? Pedido { get; set; }

    /// <summary>Os itens do pedido; opcional.</summary>
    [Tag("gItemPed")]
    public ItensPedido? ItensPedido { get; set; }

    /// <summary>As informações complementares (até 2.000 caracteres); opcional.</summary>
    [Tag("xInfComp")]
    public string? Texto { get; set; }
}
