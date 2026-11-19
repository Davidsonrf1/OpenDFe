namespace OpenDFe.NFSe;

/// <summary>O bloqueio por ofício (<c>e305102</c>), do município: a lib só o lê.</summary>
[Tag("e305102")]
public sealed class BloqueioOficio : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"305102"</c>.</summary>
    public override string Tipo => "305102";

    /// <summary>O CPF do agente da administração tributária.</summary>
    [Tag("CPFAgTrib")]
    public string? CpfAgente { get; set; }

    /// <summary>O evento bloqueado (<c>e101101</c>, <c>e105102</c>, <c>e105104</c>, <c>e105105</c> ou <c>e305101</c>).</summary>
    [Tag("codEvento")]
    public string? EventoBloqueado { get; set; }

    /// <summary>A descrição do motivo.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
