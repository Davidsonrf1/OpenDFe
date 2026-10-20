namespace OpenDFe.NFe;

/// <summary>Uma forma de pagamento (<c>detPag</c>).</summary>
public sealed class FormaPagamento
{
    /// <summary>À vista ou a prazo; opcional.</summary>
    [Tag("indPag", "YA01b")]
    public IndicadorPagamento? Indicador { get; set; }

    /// <summary>O meio de pagamento.</summary>
    [Tag("tPag", "YA02")]
    public MeioPagamento? Meio { get; set; }

    /// <summary>A descrição do meio de pagamento, no <see cref="MeioPagamento.Outros"/>; opcional.</summary>
    [Tag("xPag", "YA02a")]
    public string? Descricao { get; set; }

    /// <summary>O valor do pagamento.</summary>
    [Tag("vPag", "YA03")]
    public decimal? Valor { get; set; }

    /// <summary>A data do pagamento; opcional.</summary>
    [Tag("dPag", "YA03a")]
    public DateOnly? Data { get; set; }

    /// <summary>
    /// O CNPJ do estabelecimento onde o pagamento foi processado, quando diferente do emitente; com a
    /// <see cref="UFTransacao"/>.
    /// </summary>
    [Tag("CNPJPag", "YA03c")]
    public string? CnpjTransacao { get; set; }

    /// <summary>A UF do <see cref="CnpjTransacao"/>.</summary>
    [Tag("UFPag", "YA03d")]
    public UF? UFTransacao { get; set; }

    /// <summary>Os dados do pagamento eletrônico (cartões, PIX, boletos); opcional.</summary>
    [Tag("card", "YA04")]
    public TransacaoEletronica? Transacao { get; set; }
}
