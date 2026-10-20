namespace OpenDFe.NFe;

/// <summary>O modelo do cupom fiscal referenciado (<c>refECF/mod</c>).</summary>
/// <remarks>
/// O único domínio com valor do XML que não é número: o leiaute traduz o membro (1 a 3) para <c>2B</c>, <c>2C</c> e
/// <c>2D</c>.
/// </remarks>
public enum ModeloCupomFiscal
{
    /// <summary>Cupom fiscal emitido por máquina registradora, não ECF; <c>2B</c> no XML.</summary>
    MaquinaRegistradora = 1,

    /// <summary>Cupom fiscal PDV; <c>2C</c> no XML.</summary>
    Pdv = 2,

    /// <summary>Cupom fiscal emitido por ECF; <c>2D</c> no XML.</summary>
    Ecf = 3,
}
