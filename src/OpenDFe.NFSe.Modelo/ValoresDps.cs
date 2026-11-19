namespace OpenDFe.NFSe;

/// <summary>Os valores do serviço e os tributos da DPS (<c>valores</c>, <c>TCInfoValores</c>).</summary>
public sealed class ValoresDps
{
    /// <summary>O valor do serviço.</summary>
    [Tag("vServPrest")]
    public ValorServico Servico { get; set; } = new();

    /// <summary>Os descontos; opcional.</summary>
    [Tag("vDescCondIncond")]
    public Descontos? Descontos { get; set; }

    /// <summary>A dedução ou a redução da base de cálculo; opcional.</summary>
    [Tag("vDedRed")]
    public DeducaoReducao? DeducaoReducao { get; set; }

    /// <summary>Os tributos.</summary>
    [Tag("trib")]
    public Tributacao Tributacao { get; set; } = new();
}
