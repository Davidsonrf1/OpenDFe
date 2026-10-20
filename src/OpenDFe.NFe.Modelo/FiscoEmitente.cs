namespace OpenDFe.NFe;

/// <summary>O fisco que emitiu a nota avulsa (<c>avulsa</c>).</summary>
public sealed class FiscoEmitente
{
    /// <summary>O CNPJ do órgão emitente.</summary>
    [Tag("CNPJ", "D02")]
    public string? Cnpj { get; set; }

    /// <summary>O órgão emitente.</summary>
    [Tag("xOrgao", "D03")]
    public string? Orgao { get; set; }

    /// <summary>A matrícula do agente.</summary>
    [Tag("matr", "D04")]
    public string? MatriculaAgente { get; set; }

    /// <summary>O nome do agente.</summary>
    [Tag("xAgente", "D05")]
    public string? NomeAgente { get; set; }

    /// <summary>O telefone; opcional.</summary>
    [Tag("fone", "D06")]
    public string? Telefone { get; set; }

    /// <summary>A UF.</summary>
    [Tag("UF", "D07")]
    public UF? UF { get; set; }

    /// <summary>O número do documento de arrecadação; opcional.</summary>
    [Tag("nDAR", "D08")]
    public string? NumeroDar { get; set; }

    /// <summary>A data da emissão do documento de arrecadação; opcional.</summary>
    [Tag("dEmi", "D09")]
    public DateOnly? EmissaoDar { get; set; }

    /// <summary>O valor do documento de arrecadação; opcional.</summary>
    [Tag("vDAR", "D10")]
    public decimal? ValorDar { get; set; }

    /// <summary>A repartição fiscal emitente.</summary>
    [Tag("repEmi", "D11")]
    public string? Reparticao { get; set; }

    /// <summary>A data do pagamento do documento de arrecadação; opcional.</summary>
    [Tag("dPag", "D12")]
    public DateOnly? PagamentoDar { get; set; }
}
