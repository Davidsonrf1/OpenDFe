namespace OpenDFe.NFSe;

/// <summary>O documento fiscal eletrônico do repositório nacional (<c>dFeNacional</c>).</summary>
public sealed class DFeNacional
{
    /// <summary>O tipo do documento.</summary>
    [Tag("tipoChaveDFe")]
    public TipoDFe? Tipo { get; set; }

    /// <summary>A descrição do documento, quando o tipo é "outro"; opcional.</summary>
    [Tag("xTipoChaveDFe")]
    public string? DescricaoTipo { get; set; }

    /// <summary>A chave do documento.</summary>
    [Tag("chaveDFe")]
    public string? Chave { get; set; }
}
