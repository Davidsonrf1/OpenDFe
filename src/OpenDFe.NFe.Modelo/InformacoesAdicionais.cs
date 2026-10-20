namespace OpenDFe.NFe;

/// <summary>As informações adicionais (<c>infAdic</c>).</summary>
public sealed class InformacoesAdicionais
{
    /// <summary>As informações de interesse do fisco; opcional.</summary>
    [Tag("infAdFisco", "Z02")]
    public string? Fisco { get; set; }

    /// <summary>As informações complementares de interesse do contribuinte; opcional.</summary>
    [Tag("infCpl", "Z03")]
    public string? Complementares { get; set; }

    /// <summary>As observações de uso livre do contribuinte (até 10).</summary>
    [Tag("obsCont", "Z04")]
    public IList<Observacao> ObservacoesContribuinte { get; } = new List<Observacao>();

    /// <summary>As observações de uso livre do fisco (até 10).</summary>
    [Tag("obsFisco", "Z07")]
    public IList<Observacao> ObservacoesFisco { get; } = new List<Observacao>();

    /// <summary>Os processos referenciados (até 100).</summary>
    [Tag("procRef", "Z10")]
    public IList<ProcessoReferenciado> Processos { get; } = new List<ProcessoReferenciado>();
}
