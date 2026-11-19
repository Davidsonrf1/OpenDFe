namespace OpenDFe.NFSe;

/// <summary>O benefício municipal do ISSQN (<c>BM</c>).</summary>
public sealed class BeneficioMunicipal
{
    /// <summary>O tipo do benefício; só na versão 1.00.</summary>
    [Tag("tpBM")]
    public TipoBeneficioLei? Tipo { get; set; }

    /// <summary>O identificador do benefício parametrizado pelo município (14 dígitos).</summary>
    [Tag("nBM")]
    public string? Identificador { get; set; }

    /// <summary>O valor de redução da base de cálculo; um dos dois (valor ou percentual), opcional.</summary>
    [Tag("vRedBCBM")]
    public decimal? ValorReducao { get; set; }

    /// <summary>O percentual de redução da base de cálculo.</summary>
    [Tag("pRedBCBM")]
    public decimal? PercentualReducao { get; set; }
}
