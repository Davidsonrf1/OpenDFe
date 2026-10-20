namespace OpenDFe.NFe;

/// <summary>O transporte (<c>transp</c>).</summary>
public sealed class Transporte
{
    /// <summary>A modalidade do frete.</summary>
    [Tag("modFrete", "X02")]
    public ModalidadeFrete? Modalidade { get; set; }

    /// <summary>O transportador; opcional.</summary>
    [Tag("transporta", "X03")]
    public Transportador? Transportador { get; set; }

    /// <summary>A retenção do ICMS do transporte; opcional.</summary>
    [Tag("retTransp", "X11")]
    public RetencaoIcmsTransporte? Retencao { get; set; }

    /// <summary>
    /// O veículo de tração; opcional, com os <see cref="Reboques"/>. Exclusivo com o <see cref="Vagao"/> e a
    /// <see cref="Balsa"/>.
    /// </summary>
    [Tag("veicTransp", "X18")]
    public Veiculo? Veiculo { get; set; }

    /// <summary>Os reboques (até 5).</summary>
    [Tag("reboque", "X22")]
    public IList<Veiculo> Reboques { get; } = new List<Veiculo>();

    /// <summary>A identificação do vagão; opcional, exclusivo com o veículo e a balsa.</summary>
    [Tag("vagao", "X25a")]
    public string? Vagao { get; set; }

    /// <summary>A identificação da balsa; opcional, exclusiva com o veículo e o vagão.</summary>
    [Tag("balsa", "X25b")]
    public string? Balsa { get; set; }

    /// <summary>Os volumes (até 5000).</summary>
    [Tag("vol", "X26")]
    public IList<Volume> Volumes { get; } = new List<Volume>();
}
