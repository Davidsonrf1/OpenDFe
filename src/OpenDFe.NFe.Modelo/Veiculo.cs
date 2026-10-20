namespace OpenDFe.NFe;

/// <summary>
/// Um veículo (o tipo <c>TVeiculo</c>): o de tração (<c>veicTransp</c>) ou um reboque (<c>reboque</c>).
/// </summary>
/// <remarks>
/// Uma classe para os dois lugares, com o <see cref="TagAttribute"/> só com a tag (X19 a X21 no de tração, X23 a X25 no
/// reboque).
/// </remarks>
public sealed class Veiculo
{
    /// <summary>A placa.</summary>
    [Tag("placa")]
    public string? Placa { get; set; }

    /// <summary>A UF; opcional, nula no exterior (<see cref="Exterior"/>).</summary>
    [Tag("UF")]
    public UF? UF { get; set; }

    /// <summary>
    /// Se a UF do XML é <c>EX</c> (exterior); com ela, <see cref="UF"/> fica nulo. Sem <see cref="TagAttribute"/>: é
    /// outro valor do mesmo campo <c>UF</c>.
    /// </summary>
    public bool Exterior { get; set; }

    /// <summary>O registro nacional de transportador de carga (ANTT); opcional.</summary>
    [Tag("RNTC")]
    public string? Rntc { get; set; }
}
