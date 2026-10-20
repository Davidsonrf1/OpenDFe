namespace OpenDFe.NFe;

/// <summary>O IBS e a CBS do item (<c>IBSCBS</c>, NT 2025.002).</summary>
/// <remarks>
/// O CST e a classificação tributária são códigos de tabela publicada à parte e ficam como texto, com os zeros à
/// esquerda. A <see cref="Tributacao"/>, a <see cref="Monofasia"/>, a <see cref="TransferenciaCredito"/> e o
/// <see cref="AjusteCompetencia"/> são uma escolha do XSD, e o <see cref="CreditoPresumido"/> e o
/// <see cref="CreditoPresumidoZfm"/> outra: em cada uma, no máximo um vai preenchido.
/// </remarks>
public sealed class IbsCbs
{
    /// <summary>O CST do IBS e da CBS, como no XML (com os zeros à esquerda).</summary>
    [Tag("CST", "UB13")]
    public string? Cst { get; set; }

    /// <summary>A classificação tributária, como no XML (com os zeros à esquerda).</summary>
    [Tag("cClassTrib", "UB14")]
    public string? ClassificacaoTributaria { get; set; }

    /// <summary>Se a operação é de doação; opcional.</summary>
    [Tag("indDoacao", "UB14a")]
    public IndicadorDoacao? Doacao { get; set; }

    /// <summary>
    /// A tributação do IBS e da CBS; ou a <see cref="Monofasia"/>, a <see cref="TransferenciaCredito"/> ou o
    /// <see cref="AjusteCompetencia"/> (no máximo um).
    /// </summary>
    [Tag("gIBSCBS", "UB15")]
    public TributacaoIbsCbs? Tributacao { get; set; }

    /// <summary>
    /// A tributação monofásica de combustíveis; ou a <see cref="Tributacao"/>, a <see cref="TransferenciaCredito"/> ou
    /// o <see cref="AjusteCompetencia"/> (no máximo um).
    /// </summary>
    [Tag("gIBSCBSMono", "UB84")]
    public MonofasiaIbsCbs? Monofasia { get; set; }

    /// <summary>
    /// A transferência de crédito (CST 800); ou a <see cref="Tributacao"/>, a <see cref="Monofasia"/> ou o
    /// <see cref="AjusteCompetencia"/> (no máximo um).
    /// </summary>
    [Tag("gTransfCred", "UB106")]
    public TransferenciaCredito? TransferenciaCredito { get; set; }

    /// <summary>
    /// O ajuste de competência (CST 811); ou a <see cref="Tributacao"/>, a <see cref="Monofasia"/> ou a
    /// <see cref="TransferenciaCredito"/> (no máximo um).
    /// </summary>
    [Tag("gAjusteCompet", "UB112")]
    public AjusteCompetencia? AjusteCompetencia { get; set; }

    /// <summary>O estorno de crédito; opcional.</summary>
    [Tag("gEstornoCred", "UB116")]
    public EstornoCredito? EstornoCredito { get; set; }

    /// <summary>O crédito presumido da operação; ou o <see cref="CreditoPresumidoZfm"/> (no máximo um).</summary>
    [Tag("gCredPresOper", "UB120")]
    public CreditoPresumidoOperacao? CreditoPresumido { get; set; }

    /// <summary>
    /// O crédito presumido do IBS na Zona Franca de Manaus; ou o <see cref="CreditoPresumido"/> (no máximo um).
    /// </summary>
    [Tag("gCredPresIBSZFM", "UB131")]
    public CreditoPresumidoZfm? CreditoPresumidoZfm { get; set; }
}
