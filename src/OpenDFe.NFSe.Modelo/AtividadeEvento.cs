namespace OpenDFe.NFSe;

/// <summary>A atividade de evento (<c>atvEvento</c>).</summary>
public sealed class AtividadeEvento
{
    /// <summary>A descrição do evento (na versão 1.00, a tag <c>desc</c>).</summary>
    [Tag("xNome")]
    public string? Nome { get; set; }

    /// <summary>A data de início.</summary>
    [Tag("dtIni")]
    public DateOnly? Inicio { get; set; }

    /// <summary>A data de fim.</summary>
    [Tag("dtFim")]
    public DateOnly? Fim { get; set; }

    /// <summary>A identificação do evento pela administração tributária (na versão 1.00, a tag <c>id</c>); um dos dois (identificação ou endereço).</summary>
    [Tag("idAtvEvt")]
    public string? Identificacao { get; set; }

    /// <summary>O endereço do evento.</summary>
    [Tag("end")]
    public EnderecoEvento? Endereco { get; set; }
}
