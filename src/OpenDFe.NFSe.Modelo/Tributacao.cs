namespace OpenDFe.NFSe;

/// <summary>Os tributos do serviço (<c>trib</c>).</summary>
public sealed class Tributacao
{
    /// <summary>O ISSQN.</summary>
    [Tag("tribMun")]
    public TributacaoMunicipal Municipal { get; set; } = new();

    /// <summary>Os tributos federais; opcional.</summary>
    [Tag("tribFed")]
    public TributacaoFederal? Federal { get; set; }

    /// <summary>O total aproximado dos tributos (Lei 12.741/2012).</summary>
    [Tag("totTrib")]
    public TotalTributos Total { get; set; } = new();
}
