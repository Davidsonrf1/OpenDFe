namespace OpenDFe.NFSe;

/// <summary>O ISSQN (<c>tribMun</c>).</summary>
public sealed class TributacaoMunicipal
{
    /// <summary>A tributação do ISSQN; na versão 1.00 o XSD dá outro texto para os códigos 2 a 4, e a leitura guarda o número como veio (docs/NFSE.md 7, M12).</summary>
    [Tag("tribISSQN")]
    public TributacaoIssqn? Tributacao { get; set; }

    /// <summary>O país do resultado da prestação, na exportação; opcional.</summary>
    [Tag("cPaisResult")]
    public string? PaisResultado { get; set; }

    /// <summary>O tipo da imunidade; opcional.</summary>
    [Tag("tpImunidade")]
    public TipoImunidade? Imunidade { get; set; }

    /// <summary>A suspensão da exigibilidade; opcional.</summary>
    [Tag("exigSusp")]
    public ExigibilidadeSuspensa? ExigibilidadeSuspensa { get; set; }

    /// <summary>O benefício municipal; opcional.</summary>
    [Tag("BM")]
    public BeneficioMunicipal? Beneficio { get; set; }

    /// <summary>A retenção do ISSQN.</summary>
    [Tag("tpRetISSQN")]
    public RetencaoIssqn? Retencao { get; set; }

    /// <summary>A alíquota do ISSQN (%); opcional.</summary>
    [Tag("pAliq")]
    public decimal? Aliquota { get; set; }
}
