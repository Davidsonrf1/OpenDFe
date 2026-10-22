namespace OpenDFe.NFe;

/// <summary>
/// Uma mensagem da SEFAZ ao emissor, no protocolo (<c>cMsg</c> e <c>xMsg</c>). Sem grupo no XML: a sequência dos dois
/// campos se repete dentro do <c>infProt</c>.
/// </summary>
public sealed class MensagemSefaz
{
    /// <summary>O código da mensagem.</summary>
    [Tag("cMsg", "PR14")]
    public int? Codigo { get; set; }

    /// <summary>O texto da mensagem.</summary>
    [Tag("xMsg", "PR15")]
    public string? Texto { get; set; }
}
