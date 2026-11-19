namespace OpenDFe.NFSe;

/// <summary>Os regimes de tributação do prestador (<c>regTrib</c>).</summary>
public sealed class RegimeTributacao
{
    /// <summary>A situação perante o Simples Nacional.</summary>
    [Tag("opSimpNac")]
    public SituacaoSimplesNacional? SimplesNacional { get; set; }

    /// <summary>O regime de apuração do optante ME/EPP que ultrapassou um sublimite; opcional.</summary>
    [Tag("regApTribSN")]
    public RegimeApuracaoSimples? ApuracaoSimples { get; set; }

    /// <summary>O regime especial de tributação.</summary>
    [Tag("regEspTrib")]
    public RegimeEspecialTributacao? RegimeEspecial { get; set; }
}
