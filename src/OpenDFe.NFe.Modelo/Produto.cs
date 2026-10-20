namespace OpenDFe.NFe;

/// <summary>O produto ou serviço do item (<c>prod</c>).</summary>
/// <remarks>
/// Os grupos específicos (<see cref="VeiculoNovo"/>, <see cref="Medicamento"/>, <see cref="Armas"/>,
/// <see cref="Combustivel"/>, <see cref="Recopi"/>) são uma escolha do XSD: no máximo um vai preenchido (a lista de
/// armas não vazia conta como escolhida); mais de um é erro na montagem.
/// </remarks>
public sealed class Produto
{
    /// <summary>O código do produto ou serviço (ou <c>CFOP9999</c>, sem codificação própria).</summary>
    [Tag("cProd", "I02")]
    public string? Codigo { get; set; }

    /// <summary>O GTIN do produto (o antigo código EAN), ou <c>SEM GTIN</c>.</summary>
    [Tag("cEAN", "I03")]
    public string? Gtin { get; set; }

    /// <summary>O código de barras diferente do padrão GTIN; opcional.</summary>
    [Tag("cBarra", "I03a")]
    public string? CodigoBarras { get; set; }

    /// <summary>A descrição do produto ou serviço.</summary>
    [Tag("xProd", "I04")]
    public string? Descricao { get; set; }

    /// <summary>O código NCM (8 dígitos, ou o capítulo com 2).</summary>
    [Tag("NCM", "I05")]
    public string? Ncm { get; set; }

    /// <summary>As nomenclaturas de valor aduaneiro e estatístico (até 8).</summary>
    [Tag("NVE", "I05a")]
    public IList<string> Nve { get; } = new List<string>();

    /// <summary>O código especificador da substituição tributária (CEST); opcional.</summary>
    [Tag("CEST", "I05c")]
    public string? Cest { get; set; }

    /// <summary>Se o produto é fabricado em escala relevante; opcional.</summary>
    [Tag("indEscala", "I05d")]
    public IndicadorEscala? Escala { get; set; }

    /// <summary>O CNPJ do fabricante da mercadoria, quando produzida em escala não relevante; opcional.</summary>
    [Tag("CNPJFab", "I05e")]
    public string? CnpjFabricante { get; set; }

    /// <summary>O código de benefício fiscal na UF; opcional.</summary>
    [Tag("cBenef", "I05f")]
    public string? CodigoBeneficio { get; set; }

    /// <summary>Os créditos presumidos do ICMS (até 4).</summary>
    [Tag("gCred", "I05g")]
    public IList<CreditoPresumidoIcms> CreditosPresumidos { get; } = new List<CreditoPresumidoIcms>();

    /// <summary>A classificação para o crédito presumido do IBS na Zona Franca de Manaus; opcional.</summary>
    [Tag("tpCredPresIBSZFM", "I05k")]
    public ClassificacaoCreditoPresumidoZfm? ClassificacaoZfm { get; set; }

    /// <summary>O código EX da TIPI; opcional.</summary>
    [Tag("EXTIPI", "I06")]
    public string? ExTipi { get; set; }

    /// <summary>O CFOP.</summary>
    [Tag("CFOP", "I08")]
    public int? Cfop { get; set; }

    /// <summary>A unidade comercial.</summary>
    [Tag("uCom", "I09")]
    public string? UnidadeComercial { get; set; }

    /// <summary>A quantidade comercial.</summary>
    [Tag("qCom", "I10")]
    public decimal? QuantidadeComercial { get; set; }

    /// <summary>O valor unitário de comercialização.</summary>
    [Tag("vUnCom", "I10a")]
    public decimal? ValorUnitarioComercial { get; set; }

    /// <summary>O valor bruto do produto ou serviço.</summary>
    [Tag("vProd", "I11")]
    public decimal? Valor { get; set; }

    /// <summary>O GTIN da unidade tributável, ou <c>SEM GTIN</c>.</summary>
    [Tag("cEANTrib", "I12")]
    public string? GtinTributavel { get; set; }

    /// <summary>O código de barras da unidade tributável diferente do padrão GTIN; opcional.</summary>
    [Tag("cBarraTrib", "I12a")]
    public string? CodigoBarrasTributavel { get; set; }

    /// <summary>A unidade tributável.</summary>
    [Tag("uTrib", "I13")]
    public string? UnidadeTributavel { get; set; }

    /// <summary>A quantidade tributável.</summary>
    [Tag("qTrib", "I14")]
    public decimal? QuantidadeTributavel { get; set; }

    /// <summary>O valor unitário de tributação.</summary>
    [Tag("vUnTrib", "I14a")]
    public decimal? ValorUnitarioTributavel { get; set; }

    /// <summary>O valor do frete; opcional.</summary>
    [Tag("vFrete", "I15")]
    public decimal? Frete { get; set; }

    /// <summary>O valor do seguro; opcional.</summary>
    [Tag("vSeg", "I16")]
    public decimal? Seguro { get; set; }

    /// <summary>O valor do desconto; opcional.</summary>
    [Tag("vDesc", "I17")]
    public decimal? Desconto { get; set; }

    /// <summary>As outras despesas acessórias; opcional.</summary>
    [Tag("vOutro", "I17a")]
    public decimal? OutrasDespesas { get; set; }

    /// <summary>Se o valor do item compõe o total da nota (<c>vProd</c>).</summary>
    [Tag("indTot", "I17b")]
    public IndicadorTotal? Totalizacao { get; set; }

    /// <summary>Se o item é fornecimento de bem móvel usado; opcional.</summary>
    [Tag("indBemMovelUsado", "I17c")]
    public IndicadorBemMovelUsado? BemMovelUsado { get; set; }

    /// <summary>As declarações de importação (até 100).</summary>
    [Tag("DI", "I18")]
    public IList<DeclaracaoImportacao> Importacoes { get; } = new List<DeclaracaoImportacao>();

    /// <summary>Os detalhes da exportação (até 500).</summary>
    [Tag("detExport", "I50")]
    public IList<DetalheExportacao> Exportacoes { get; } = new List<DetalheExportacao>();

    /// <summary>O número do pedido de compra; opcional.</summary>
    [Tag("xPed", "I60")]
    public string? Pedido { get; set; }

    /// <summary>O item do pedido de compra; opcional.</summary>
    [Tag("nItemPed", "I61")]
    public string? ItemPedido { get; set; }

    /// <summary>
    /// O número de controle da FCI (ficha de conteúdo de importação), o GUID em maiúsculas; opcional.
    /// </summary>
    [Tag("nFCI", "I70")]
    public string? NumeroFci { get; set; }

    /// <summary>Os lotes do produto, para a rastreabilidade (até 500).</summary>
    [Tag("rastro", "I80")]
    public IList<Rastro> Rastros { get; } = new List<Rastro>();

    /// <summary>As informações do produto na NFF; opcional.</summary>
    [Tag("infProdNFF", "I86")]
    public ProdutoNff? Nff { get; set; }

    /// <summary>A embalagem do produto; opcional.</summary>
    [Tag("infProdEmb", "I87")]
    public EmbalagemProduto? Embalagem { get; set; }

    /// <summary>O veículo novo; opcional.</summary>
    [Tag("veicProd", "J01")]
    public VeiculoNovo? VeiculoNovo { get; set; }

    /// <summary>O medicamento ou a matéria-prima farmacêutica; opcional.</summary>
    [Tag("med", "K01")]
    public Medicamento? Medicamento { get; set; }

    /// <summary>As armas de fogo (até 500).</summary>
    [Tag("arma", "L01")]
    public IList<Arma> Armas { get; } = new List<Arma>();

    /// <summary>O combustível; opcional.</summary>
    [Tag("comb", "LA01")]
    public Combustivel? Combustivel { get; set; }

    /// <summary>O número do RECOPI (papel imune); opcional.</summary>
    [Tag("nRECOPI", "LB01")]
    public string? Recopi { get; set; }
}
