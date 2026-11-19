namespace OpenDFe.NFSe;

/// <summary>O cancelamento por substituição (<c>e105102</c>), gerado pela Sefin Nacional na emissão da DPS com a substituição: a lib só o lê.</summary>
[Tag("e105102")]
public sealed class CancelamentoSubstituicao : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"105102"</c>.</summary>
    public override string Tipo => "105102";

    /// <summary>O motivo da substituição.</summary>
    [Tag("cMotivo")]
    public MotivoSubstituicao? Motivo { get; set; }

    /// <summary>A descrição do motivo; opcional.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }

    /// <summary>A chave da NFS-e substituta.</summary>
    [Tag("chSubstituta")]
    public string? ChaveSubstituta { get; set; }
}
