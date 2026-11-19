namespace OpenDFe.NFSe;

/// <summary>A rejeição do prestador (<c>e202205</c>), manifestação ao ADN: a lib só a lê.</summary>
[Tag("e202205")]
public sealed class RejeicaoPrestador : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"202205"</c>.</summary>
    public override string Tipo => "202205";

    /// <summary>O motivo da rejeição.</summary>
    [Tag("cMotivo")]
    public MotivoRejeicao? Motivo { get; set; }

    /// <summary>A descrição do motivo; opcional.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
