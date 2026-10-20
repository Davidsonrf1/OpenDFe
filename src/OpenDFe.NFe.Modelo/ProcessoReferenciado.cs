namespace OpenDFe.NFe;

/// <summary>Um processo referenciado (<c>procRef</c>).</summary>
public sealed class ProcessoReferenciado
{
    /// <summary>O número do processo, ato concessório ou benefício.</summary>
    [Tag("nProc", "Z11")]
    public string? Numero { get; set; }

    /// <summary>A origem do processo.</summary>
    [Tag("indProc", "Z12")]
    public OrigemProcesso? Origem { get; set; }

    /// <summary>O tipo do ato concessório; opcional.</summary>
    [Tag("tpAto", "Z13")]
    public TipoAtoConcessorio? TipoAto { get; set; }
}
