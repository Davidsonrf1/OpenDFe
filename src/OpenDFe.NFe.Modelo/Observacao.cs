namespace OpenDFe.NFe;

/// <summary>
/// Uma observação de uso livre: do contribuinte (<c>obsCont</c>) ou do fisco (<c>obsFisco</c>), na nota ou no item.
/// </summary>
/// <remarks>
/// Uma classe para os quatro lugares, com o <see cref="TagAttribute"/> só com a tag: na nota, Z05 e Z06 no contribuinte,
/// Z08 e Z09 no fisco; no item (<c>obsItem</c>), VA03 e VA04 no contribuinte, VA06 e VA07 no fisco.
/// </remarks>
public sealed class Observacao
{
    /// <summary>A identificação do campo (o atributo <c>xCampo</c>).</summary>
    [Tag("xCampo")]
    public string? Campo { get; set; }

    /// <summary>O conteúdo.</summary>
    [Tag("xTexto")]
    public string? Texto { get; set; }
}
