namespace OpenDFe.NFe;

/// <summary>Uma nota fiscal modelo 1 ou 2 referenciada (<c>refNF</c>).</summary>
[Tag("refNF", "BA03")]
public sealed class NotaModelo1Referenciada : DocumentoReferenciado
{
    /// <summary>A UF do emitente.</summary>
    [Tag("cUF", "BA04")]
    public UF? UF { get; set; }

    /// <summary>O ano e o mês da emissão, como no XML (<c>AAMM</c>).</summary>
    [Tag("AAMM", "BA05")]
    public string? AnoMes { get; set; }

    /// <summary>O CNPJ do emitente.</summary>
    [Tag("CNPJ", "BA06")]
    public string? Cnpj { get; set; }

    /// <summary>O modelo (01 ou 02).</summary>
    [Tag("mod", "BA07")]
    public ModeloNotaReferenciada? Modelo { get; set; }

    /// <summary>A série (0 se não houver).</summary>
    [Tag("serie", "BA08")]
    public int? Serie { get; set; }

    /// <summary>O número.</summary>
    [Tag("nNF", "BA09")]
    public int? Numero { get; set; }
}
