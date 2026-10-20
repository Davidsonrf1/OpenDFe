namespace OpenDFe.NFe;

/// <summary>As observações do item (<c>obsItem</c>).</summary>
public sealed class ObservacoesItem
{
    /// <summary>A observação do contribuinte; opcional.</summary>
    [Tag("obsCont", "VA02")]
    public Observacao? Contribuinte { get; set; }

    /// <summary>A observação do fisco; opcional.</summary>
    [Tag("obsFisco", "VA05")]
    public Observacao? Fisco { get; set; }
}
