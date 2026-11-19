namespace OpenDFe.NFSe;

/// <summary>O cancelamento da NFS-e por quem a emitiu (<c>e101101</c>).</summary>
[Tag("e101101")]
public sealed class CancelamentoNFSe : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"101101"</c>.</summary>
    public override string Tipo => "101101";

    /// <summary>O motivo do cancelamento.</summary>
    [Tag("cMotivo")]
    public MotivoCancelamento? Motivo { get; set; }

    /// <summary>A descrição do motivo.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
