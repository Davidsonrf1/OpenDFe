namespace OpenDFe.NFSe;

/// <summary>A suspensão da exigibilidade do ISSQN (<c>exigSusp</c>).</summary>
public sealed class ExigibilidadeSuspensa
{
    /// <summary>O tipo da suspensão.</summary>
    [Tag("tpSusp")]
    public TipoSuspensao? Tipo { get; set; }

    /// <summary>O número do processo (30 dígitos).</summary>
    [Tag("nProcesso")]
    public string? Processo { get; set; }
}
