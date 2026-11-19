namespace OpenDFe.NFSe;

/// <summary>A solicitação de análise fiscal para cancelamento (<c>e101103</c>), fora do prazo do cancelamento.</summary>
[Tag("e101103")]
public sealed class AnaliseFiscalCancelamento : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"101103"</c>.</summary>
    public override string Tipo => "101103";

    /// <summary>O motivo da solicitação.</summary>
    [Tag("cMotivo")]
    public MotivoAnaliseFiscal? Motivo { get; set; }

    /// <summary>A descrição do motivo.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
