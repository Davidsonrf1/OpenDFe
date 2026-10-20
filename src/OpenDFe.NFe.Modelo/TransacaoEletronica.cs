namespace OpenDFe.NFe;

/// <summary>Os dados de um pagamento eletrônico (<c>card</c>): cartões, PIX, boletos e outros.</summary>
public sealed class TransacaoEletronica
{
    /// <summary>Se o pagamento é integrado ao sistema de automação.</summary>
    [Tag("tpIntegra", "YA04a")]
    public TipoIntegracao? Integracao { get; set; }

    /// <summary>O CNPJ da instituição de pagamento, adquirente ou subadquirente; opcional.</summary>
    [Tag("CNPJ", "YA05")]
    public string? CnpjInstituicao { get; set; }

    /// <summary>A bandeira da operadora do cartão; opcional.</summary>
    [Tag("tBand", "YA06")]
    public BandeiraCartao? Bandeira { get; set; }

    /// <summary>O número da autorização da transação; opcional.</summary>
    [Tag("cAut", "YA07")]
    public string? Autorizacao { get; set; }

    /// <summary>O CNPJ do beneficiário do pagamento; opcional.</summary>
    [Tag("CNPJReceb", "YA07a")]
    public string? CnpjBeneficiario { get; set; }

    /// <summary>O identificador do terminal de pagamento; opcional.</summary>
    [Tag("idTermPag", "YA07b")]
    public string? Terminal { get; set; }
}
