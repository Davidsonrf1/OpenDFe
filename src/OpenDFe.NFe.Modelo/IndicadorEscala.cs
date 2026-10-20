namespace OpenDFe.NFe;

/// <summary>Se o produto é fabricado em escala relevante (<c>indEscala</c>).</summary>
/// <remarks>O valor do XML não é número: o leiaute traduz o membro (1 e 2) para <c>S</c> e <c>N</c>.</remarks>
public enum IndicadorEscala
{
    /// <summary>Produzido em escala relevante; <c>S</c> no XML.</summary>
    Relevante = 1,

    /// <summary>Produzido em escala não relevante; <c>N</c> no XML.</summary>
    NaoRelevante = 2,
}
