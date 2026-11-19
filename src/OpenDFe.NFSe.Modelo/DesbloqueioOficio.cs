namespace OpenDFe.NFSe;

/// <summary>O desbloqueio por ofício (<c>e305103</c>), do município: a lib só o lê.</summary>
[Tag("e305103")]
public sealed class DesbloqueioOficio : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"305103"</c>.</summary>
    public override string Tipo => "305103";

    /// <summary>O CPF do agente da administração tributária.</summary>
    [Tag("CPFAgTrib")]
    public string? CpfAgente { get; set; }

    /// <summary>O identificador do bloqueio desfeito (59 dígitos).</summary>
    [Tag("idBloqOfic")]
    public string? Bloqueio { get; set; }
}
