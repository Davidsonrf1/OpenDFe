namespace OpenDFe.NFe;

/// <summary>Um defensivo agrícola (<c>defensivo</c>).</summary>
public sealed class Defensivo
{
    /// <summary>O número do receituário ou da receita.</summary>
    [Tag("nReceituario", "ZF03")]
    public string? Receituario { get; set; }

    /// <summary>O CPF do responsável técnico pelo receituário.</summary>
    [Tag("CPFRespTec", "ZF03a")]
    public string? CpfResponsavel { get; set; }
}
