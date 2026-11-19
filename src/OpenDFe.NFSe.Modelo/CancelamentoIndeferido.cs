namespace OpenDFe.NFSe;

/// <summary>O cancelamento indeferido por análise fiscal (<c>e105105</c>), do município: a lib só o lê.</summary>
[Tag("e105105")]
public sealed class CancelamentoIndeferido : EventoNFSe
{
    /// <summary>O tipo do evento: <c>"105105"</c>.</summary>
    public override string Tipo => "105105";

    /// <summary>O CPF do agente da administração tributária.</summary>
    [Tag("CPFAgTrib")]
    public string? CpfAgente { get; set; }

    /// <summary>O número do processo administrativo; opcional.</summary>
    [Tag("nProcAdm")]
    public string? ProcessoAdministrativo { get; set; }

    /// <summary>A resposta da análise.</summary>
    [Tag("cMotivo")]
    public MotivoIndeferimento? Motivo { get; set; }

    /// <summary>A descrição do motivo.</summary>
    [Tag("xMotivo")]
    public string? Descricao { get; set; }
}
