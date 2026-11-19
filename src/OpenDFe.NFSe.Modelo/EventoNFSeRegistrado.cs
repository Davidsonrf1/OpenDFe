namespace OpenDFe.NFSe;

/// <summary>
/// O evento registrado (<c>evento</c>, o <c>infEvento</c>): o que a Sefin Nacional, o município ou o ADN devolvem, com o
/// pedido dentro (docs/NFSE.md 7.5, M11). O Id (<c>EVT</c> + a chave + o tipo + a sequência) e a assinatura ficam fora.
/// </summary>
public sealed class EventoNFSeRegistrado
{
    /// <summary>A versão do aplicativo que registrou o evento.</summary>
    [Tag("verAplic")]
    public string? VersaoAplicativo { get; set; }

    /// <summary>O ambiente que gerou o evento.</summary>
    [Tag("ambGer")]
    public AmbienteGeradorEvento? AmbienteGerador { get; set; }

    /// <summary>O número sequencial do evento para o mesmo tipo.</summary>
    [Tag("nSeqEvento")]
    public int? Sequencia { get; set; }

    /// <summary>A data e a hora do registro.</summary>
    [Tag("dhProc")]
    public DateTimeOffset? Processamento { get; set; }

    /// <summary>O número do documento no ambiente gerador.</summary>
    [Tag("nDFSe")]
    public long? NumeroDfse { get; set; }

    /// <summary>O pedido de registro do evento, como o autor o enviou.</summary>
    [Tag("pedRegEvento")]
    public EventoNFSe? Pedido { get; set; }
}
