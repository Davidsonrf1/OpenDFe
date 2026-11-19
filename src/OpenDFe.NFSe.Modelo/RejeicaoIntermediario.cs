namespace OpenDFe.NFSe;

/// <summary>A rejeição do intermediário (<c>e204207</c>), manifestação ao ADN: a lib só a lê.</summary>
[Tag("e204207")]
public sealed class RejeicaoIntermediario : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"204207"</c>.</summary>
    public override string Tipo => "204207";

    /// <summary>O motivo da rejeição.</summary>
    [Tag("cMotivo")]
    public MotivoRejeicao? Motivo { get; set; }

    /// <summary>A descrição do motivo; opcional.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
