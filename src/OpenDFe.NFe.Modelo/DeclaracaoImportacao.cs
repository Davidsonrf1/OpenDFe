namespace OpenDFe.NFe;

/// <summary>Uma declaração de importação do produto (<c>DI</c>).</summary>
public sealed class DeclaracaoImportacao
{
    /// <summary>O número da declaração de importação (DI, DSI, DIRE, DUImp).</summary>
    [Tag("nDI", "I19")]
    public string? Numero { get; set; }

    /// <summary>A data do registro.</summary>
    [Tag("dDI", "I20")]
    public DateOnly? Data { get; set; }

    /// <summary>O local do desembaraço aduaneiro.</summary>
    [Tag("xLocDesemb", "I21")]
    public string? LocalDesembaraco { get; set; }

    /// <summary>A UF do desembaraço aduaneiro.</summary>
    [Tag("UFDesemb", "I22")]
    public UF? UFDesembaraco { get; set; }

    /// <summary>A data do desembaraço aduaneiro.</summary>
    [Tag("dDesemb", "I23")]
    public DateOnly? DataDesembaraco { get; set; }

    /// <summary>A via de transporte internacional.</summary>
    [Tag("tpViaTransp", "I23a")]
    public ViaTransporteInternacional? ViaTransporte { get; set; }

    /// <summary>O valor do adicional ao frete para renovação da marinha mercante (AFRMM); opcional.</summary>
    [Tag("vAFRMM", "I23b")]
    public decimal? ValorAfrmm { get; set; }

    /// <summary>A forma de importação quanto à intermediação.</summary>
    [Tag("tpIntermedio", "I23c")]
    public FormaImportacao? FormaImportacao { get; set; }

    /// <summary>O CNPJ do adquirente ou do encomendante; ou o <see cref="Cpf"/> (um dos dois).</summary>
    [Tag("CNPJ", "I23d")]
    public string? Cnpj { get; set; }

    /// <summary>O CPF do adquirente ou do encomendante; ou o <see cref="Cnpj"/> (um dos dois).</summary>
    [Tag("CPF", "I23d1")]
    public string? Cpf { get; set; }

    /// <summary>A UF do adquirente ou do encomendante; opcional.</summary>
    [Tag("UFTerceiro", "I23e")]
    public UF? UFTerceiro { get; set; }

    /// <summary>O código do exportador.</summary>
    [Tag("cExportador", "I24")]
    public string? CodigoExportador { get; set; }

    /// <summary>As adições (de 1 a 999).</summary>
    [Tag("adi", "I25")]
    public IList<Adicao> Adicoes { get; } = new List<Adicao>();
}
