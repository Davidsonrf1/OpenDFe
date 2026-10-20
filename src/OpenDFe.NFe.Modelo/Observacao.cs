namespace OpenDFe.NFe;

/// <summary>Uma observação de uso livre: do contribuinte (<c>obsCont</c>) ou do fisco (<c>obsFisco</c>).</summary>
/// <remarks>
/// Uma classe para os dois lugares, com o <see cref="TagAttribute"/> só com a tag (Z05 e Z06 no contribuinte, Z08 e Z09
/// no fisco).
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
