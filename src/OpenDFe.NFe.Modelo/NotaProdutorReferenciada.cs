namespace OpenDFe.NFe;

/// <summary>Uma nota fiscal de produtor rural referenciada (<c>refNFP</c>).</summary>
[Tag("refNFP", "BA10")]
public sealed class NotaProdutorReferenciada : DocumentoReferenciado
{
    /// <summary>A UF do emitente.</summary>
    [Tag("cUF", "BA11")]
    public UF? UF { get; set; }

    /// <summary>O ano e o mês da emissão, como no XML (<c>AAMM</c>).</summary>
    [Tag("AAMM", "BA12")]
    public string? AnoMes { get; set; }

    /// <summary>O CNPJ do emitente; ou o <see cref="Cpf"/> (um dos dois).</summary>
    [Tag("CNPJ", "BA13")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF do emitente; ou o <see cref="Cnpj"/> (um dos dois).</summary>
    [Tag("CPF", "BA14")]
    public string? Cpf { get; set; }

    /// <summary>A inscrição estadual do emitente, ou <c>ISENTO</c>.</summary>
    [Tag("IE", "BA15")]
    public string? InscricaoEstadual { get; set; }

    /// <summary>O modelo (01 ou 04).</summary>
    [Tag("mod", "BA16")]
    public ModeloNotaProdutor? Modelo { get; set; }

    /// <summary>A série (0 se não houver).</summary>
    [Tag("serie", "BA17")]
    public int? Serie { get; set; }

    /// <summary>O número.</summary>
    [Tag("nNF", "BA18")]
    public int? Numero { get; set; }
}
