namespace OpenDFe.NFSe;

/// <summary>Os códigos e a descrição do serviço (<c>cServ</c>).</summary>
public sealed class CodigoServico
{
    /// <summary>O código de tributação nacional do ISSQN (6 dígitos, LC 116/2003).</summary>
    [Tag("cTribNac")]
    public string? TributacaoNacional { get; set; }

    /// <summary>O código de tributação municipal do ISSQN (3 dígitos); opcional.</summary>
    [Tag("cTribMun")]
    public string? TributacaoMunicipal { get; set; }

    /// <summary>A descrição completa do serviço.</summary>
    [Tag("xDescServ")]
    public string? Descricao { get; set; }

    /// <summary>O código NBS do serviço (9 dígitos); opcional.</summary>
    [Tag("cNBS")]
    public string? Nbs { get; set; }

    /// <summary>O código interno do contribuinte; opcional.</summary>
    [Tag("cIntContrib")]
    public string? CodigoInterno { get; set; }
}
