namespace OpenDFe.NFe;

/// <summary>
/// Uma NF-e (modelo 55) ou NFC-e (modelo 65): tudo o que vai dentro do <c>infNFe</c> e do <c>infNFeSupl</c>.
/// </summary>
/// <remarks>
/// Preenchida por quem emite e devolvida pela leitura (docs/MODELO.md). Mutável e sem conferência: o que está errado
/// aparece como <see cref="Problema"/> na montagem (schema, regras), nunca como exceção do modelo. Não é segura para
/// alteração concorrente; a lib nunca altera a nota que recebe. Ficam fora do modelo o <c>Id</c> e a <c>versao</c> do
/// <c>infNFe</c> (a chave e a versão do leiaute), o <c>cDV</c> e a assinatura.
/// </remarks>
public sealed class NotaFiscal
{
    /// <summary>A identificação da nota.</summary>
    [Tag("ide", "B01")]
    public Identificacao Identificacao { get; set; } = new();

    /// <summary>O emitente.</summary>
    [Tag("emit", "C01")]
    public Emitente Emitente { get; set; } = new();

    /// <summary>O fisco que emitiu a nota avulsa (uso exclusivo do fisco); opcional.</summary>
    [Tag("avulsa", "D01")]
    public FiscoEmitente? Avulsa { get; set; }

    /// <summary>O destinatário; opcional (na NFC-e, o consumidor, quando identificado).</summary>
    [Tag("dest", "E01")]
    public Destinatario? Destinatario { get; set; }

    /// <summary>O local de retirada, quando diferente do endereço do emitente; opcional.</summary>
    [Tag("retirada", "F01")]
    public Local? Retirada { get; set; }

    /// <summary>O local de entrega, quando diferente do endereço do destinatário; opcional.</summary>
    [Tag("entrega", "G01")]
    public Local? Entrega { get; set; }

    /// <summary>As pessoas autorizadas a obter o XML (até 10).</summary>
    [Tag("autXML", "GA01")]
    public IList<PessoaAutorizada> AutorizadosXml { get; } = new List<PessoaAutorizada>();

    /// <summary>Os itens da nota (de 1 a 990); o <c>nItem</c> é a posição na lista, a partir de 1.</summary>
    [Tag("det", "H01")]
    public IList<Item> Itens { get; } = new List<Item>();

    /// <summary>Os totais da nota.</summary>
    [Tag("total", "W01")]
    public Total Total { get; set; } = new();

    /// <summary>O transporte.</summary>
    [Tag("transp", "X01")]
    public Transporte Transporte { get; set; } = new();

    /// <summary>A cobrança (fatura e duplicatas); opcional.</summary>
    [Tag("cobr", "Y01")]
    public Cobranca? Cobranca { get; set; }

    /// <summary>O pagamento.</summary>
    [Tag("pag", "YA01")]
    public Pagamento Pagamento { get; set; } = new();

    /// <summary>O intermediador da transação; opcional.</summary>
    [Tag("infIntermed", "YB01")]
    public Intermediador? Intermediador { get; set; }

    /// <summary>As informações adicionais; opcional.</summary>
    [Tag("infAdic", "Z01")]
    public InformacoesAdicionais? InformacoesAdicionais { get; set; }

    /// <summary>A exportação; opcional.</summary>
    [Tag("exporta", "ZA01")]
    public Exportacao? Exportacao { get; set; }

    /// <summary>A compra (nota de empenho, pedido, contrato); opcional.</summary>
    [Tag("compra", "ZB01")]
    public Compra? Compra { get; set; }

    /// <summary>A aquisição de cana; opcional.</summary>
    [Tag("cana", "ZC01")]
    public AquisicaoCana? Cana { get; set; }

    /// <summary>O responsável técnico pelo sistema emissor; opcional.</summary>
    [Tag("infRespTec", "ZD01")]
    public ResponsavelTecnico? ResponsavelTecnico { get; set; }

    /// <summary>A solicitação da NFF (nota fiscal fácil); opcional.</summary>
    [Tag("infSolicNFF", "ZE01")]
    public SolicitacaoNff? SolicitacaoNff { get; set; }

    /// <summary>Os produtos agropecuários (defensivos ou guia de trânsito); opcional.</summary>
    [Tag("agropecuario", "ZF01")]
    public Agropecuario? Agropecuario { get; set; }

    /// <summary>O provedor de assinatura e autorização (PAA); opcional.</summary>
    [Tag("infPAA", "ZG01")]
    public ProvedorAssinatura? ProvedorAssinatura { get; set; }

    /// <summary>
    /// As informações suplementares da NFC-e (QR Code e URL da consulta). No XML, irmão do <c>infNFe</c>, e não filho.
    /// </summary>
    [Tag("infNFeSupl", "ZX01")]
    public InformacoesSuplementares? Suplementares { get; set; }
}
