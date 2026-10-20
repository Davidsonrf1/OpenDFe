namespace OpenDFe.NFe;

/// <summary>Os tributos do item (<c>imposto</c>).</summary>
/// <remarks>
/// O ICMS (com o IPI e o II) ou o ISSQN (com o IPI) são uma escolha do XSD: o <see cref="Icms"/> e o
/// <see cref="Issqn"/> juntos, ou o <see cref="ImpostoImportacao"/> com o <see cref="Issqn"/>, são erro na montagem; o
/// <see cref="Ipi"/> é um só nos dois ramos.
/// </remarks>
public sealed class Imposto
{
    /// <summary>O valor aproximado total dos tributos (Lei 12.741/2012); opcional.</summary>
    [Tag("vTotTrib", "M02")]
    public decimal? ValorTotalTributos { get; set; }

    /// <summary>O ICMS; opcional.</summary>
    [Tag("ICMS", "N01")]
    public Icms? Icms { get; set; }

    /// <summary>O IPI; opcional.</summary>
    [Tag("IPI", "O01")]
    public Ipi? Ipi { get; set; }

    /// <summary>O imposto de importação; opcional.</summary>
    [Tag("II", "P01")]
    public ImpostoImportacao? ImpostoImportacao { get; set; }

    /// <summary>O ISSQN; opcional.</summary>
    [Tag("ISSQN", "U01")]
    public Issqn? Issqn { get; set; }

    /// <summary>O PIS; opcional.</summary>
    [Tag("PIS", "Q01")]
    public Pis? Pis { get; set; }

    /// <summary>O PIS por substituição tributária; opcional.</summary>
    [Tag("PISST", "R01")]
    public PisSt? PisSt { get; set; }

    /// <summary>A COFINS; opcional.</summary>
    [Tag("COFINS", "S01")]
    public Cofins? Cofins { get; set; }

    /// <summary>A COFINS por substituição tributária; opcional.</summary>
    [Tag("COFINSST", "T01")]
    public CofinsSt? CofinsSt { get; set; }

    /// <summary>O ICMS de partilha para a UF do destinatário; opcional.</summary>
    [Tag("ICMSUFDest", "NA01")]
    public IcmsUFDestino? IcmsUFDestino { get; set; }

    /// <summary>O imposto seletivo; opcional.</summary>
    [Tag("IS", "UB01")]
    public ImpostoSeletivo? ImpostoSeletivo { get; set; }

    /// <summary>O IBS e a CBS; opcional.</summary>
    [Tag("IBSCBS", "UB12")]
    public IbsCbs? IbsCbs { get; set; }
}
