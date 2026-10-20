namespace OpenDFe.NFe;

/// <summary>Um volume transportado (<c>vol</c>).</summary>
public sealed class Volume
{
    /// <summary>A quantidade de volumes; opcional.</summary>
    [Tag("qVol", "X27")]
    public long? Quantidade { get; set; }

    /// <summary>A espécie; opcional.</summary>
    [Tag("esp", "X28")]
    public string? Especie { get; set; }

    /// <summary>A marca; opcional.</summary>
    [Tag("marca", "X29")]
    public string? Marca { get; set; }

    /// <summary>A numeração; opcional.</summary>
    [Tag("nVol", "X30")]
    public string? Numeracao { get; set; }

    /// <summary>O peso líquido, em kg; opcional.</summary>
    [Tag("pesoL", "X31")]
    public decimal? PesoLiquido { get; set; }

    /// <summary>O peso bruto, em kg; opcional.</summary>
    [Tag("pesoB", "X32")]
    public decimal? PesoBruto { get; set; }

    /// <summary>Os lacres.</summary>
    [Tag("lacres", "X33")]
    public IList<Lacre> Lacres { get; } = new List<Lacre>();
}
