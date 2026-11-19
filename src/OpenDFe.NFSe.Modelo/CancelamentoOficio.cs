namespace OpenDFe.NFSe;

/// <summary>O cancelamento por ofício (<c>e305101</c>), do município: a lib só o lê.</summary>
[Tag("e305101")]
public sealed class CancelamentoOficio : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"305101"</c>.</summary>
    public override string Tipo => "305101";

    /// <summary>O CPF do agente da administração tributária.</summary>
    [Tag("CPFAgTrib")]
    public string? CpfAgente { get; set; }

    /// <summary>O número do processo administrativo.</summary>
    [Tag("nProcAdm")]
    public string? ProcessoAdministrativo { get; set; }

    /// <summary>A descrição do processo.</summary>
    [Tag("xProcAdm")]
    public string? DescricaoProcesso { get; set; }
}
