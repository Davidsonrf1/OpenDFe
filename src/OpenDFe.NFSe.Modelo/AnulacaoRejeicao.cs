namespace OpenDFe.NFSe;

/// <summary>A anulação da rejeição (<c>e205208</c>), do município: a lib só a lê.</summary>
[Tag("e205208")]
public sealed class AnulacaoRejeicao : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"205208"</c>.</summary>
    public override string Tipo => "205208";

    /// <summary>O CPF do agente da administração tributária.</summary>
    [Tag("CPFAgTrib")]
    public string? CpfAgente { get; set; }

    /// <summary>O identificador do evento de rejeição anulado (59 dígitos).</summary>
    [Tag("idEvManifRej")]
    public string? EventoRejeicao { get; set; }

    /// <summary>A descrição do motivo.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
