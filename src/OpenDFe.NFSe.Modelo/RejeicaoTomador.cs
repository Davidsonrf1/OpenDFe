namespace OpenDFe.NFSe;

/// <summary>A rejeição do tomador (<c>e203206</c>), manifestação ao ADN: a lib só a lê.</summary>
[Tag("e203206")]
public sealed class RejeicaoTomador : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"203206"</c>.</summary>
    public override string Tipo => "203206";

    /// <summary>O motivo da rejeição.</summary>
    [Tag("cMotivo")]
    public MotivoRejeicao? Motivo { get; set; }

    /// <summary>A descrição do motivo; opcional.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
