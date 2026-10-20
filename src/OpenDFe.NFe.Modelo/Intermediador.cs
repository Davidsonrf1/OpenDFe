namespace OpenDFe.NFe;

/// <summary>O intermediador da transação (<c>infIntermed</c>): a plataforma de terceiros.</summary>
public sealed class Intermediador
{
    /// <summary>O CNPJ do intermediador.</summary>
    [Tag("CNPJ", "YB02")]
    public string? Cnpj { get; set; }

    /// <summary>O identificador do vendedor no cadastro do intermediador.</summary>
    [Tag("idCadIntTran", "YB03")]
    public string? Identificador { get; set; }
}
