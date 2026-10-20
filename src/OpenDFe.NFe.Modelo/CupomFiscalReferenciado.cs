namespace OpenDFe.NFe;

/// <summary>Um cupom fiscal referenciado (<c>refECF</c>).</summary>
[Tag("refECF", "BA20")]
public sealed class CupomFiscalReferenciado : DocumentoReferenciado
{
    /// <summary>O modelo do cupom (<c>2B</c>, <c>2C</c> ou <c>2D</c>).</summary>
    [Tag("mod", "BA21")]
    public ModeloCupomFiscal? Modelo { get; set; }

    /// <summary>O número de ordem sequencial do ECF.</summary>
    [Tag("nECF", "BA22")]
    public int? NumeroEcf { get; set; }

    /// <summary>O contador de ordem de operação (COO).</summary>
    [Tag("nCOO", "BA23")]
    public int? Coo { get; set; }
}
