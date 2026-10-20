namespace OpenDFe.NFe;

/// <summary>A identificação da nota (<c>ide</c>).</summary>
/// <remarks>
/// Os campos que o núcleo já sabe (<see cref="UF"/>, <see cref="Modelo"/>, <see cref="TipoEmissao"/>,
/// <see cref="Ambiente"/>, a contingência) e o <see cref="CodigoNumerico"/>, nulos na escrita, vêm do núcleo;
/// preenchidos e diferentes dele, são erro na montagem. A leitura sempre os preenche. O <c>cDV</c> fica fora: é
/// calculado na escrita e conferido na leitura.
/// </remarks>
public sealed class Identificacao
{
    /// <summary>A UF do emitente. Nulo na escrita: o valor do núcleo (a configuração).</summary>
    [Tag("cUF", "B02")]
    public UF? UF { get; set; }

    /// <summary>O código numérico que compõe a chave (8 dígitos). Nulo na escrita: sorteado na montagem.</summary>
    [Tag("cNF", "B03")]
    public int? CodigoNumerico { get; set; }

    /// <summary>A natureza da operação.</summary>
    [Tag("natOp", "B04")]
    public string? NaturezaOperacao { get; set; }

    /// <summary>O modelo do documento (55 ou 65). Nulo na escrita: o valor do núcleo (a configuração).</summary>
    [Tag("mod", "B06")]
    public Documento? Modelo { get; set; }

    /// <summary>A série.</summary>
    [Tag("serie", "B07")]
    public int? Serie { get; set; }

    /// <summary>O número da nota.</summary>
    [Tag("nNF", "B08")]
    public int? Numero { get; set; }

    /// <summary>A data e a hora da emissão, com o fuso.</summary>
    [Tag("dhEmi", "B09")]
    public DateTimeOffset? Emissao { get; set; }

    /// <summary>A data e a hora da saída ou da entrada, com o fuso; opcional.</summary>
    [Tag("dhSaiEnt", "B10")]
    public DateTimeOffset? SaidaEntrada { get; set; }

    /// <summary>A data prevista da entrega; opcional.</summary>
    [Tag("dPrevEntrega", "B10a")]
    public DateOnly? PrevisaoEntrega { get; set; }

    /// <summary>Entrada ou saída.</summary>
    [Tag("tpNF", "B11")]
    public TipoNota? Tipo { get; set; }

    /// <summary>O local de destino da operação.</summary>
    [Tag("idDest", "B11a")]
    public LocalDestino? LocalDestino { get; set; }

    /// <summary>O município do fato gerador do ICMS (código do IBGE).</summary>
    [Tag("cMunFG", "B12")]
    public int? MunicipioFatoGerador { get; set; }

    /// <summary>O município do fato gerador do IBS e da CBS (código do IBGE); opcional.</summary>
    [Tag("cMunFGIBS", "B12a")]
    public int? MunicipioFatoGeradorIbsCbs { get; set; }

    /// <summary>O formato de impressão do DANFE.</summary>
    [Tag("tpImp", "B21")]
    public FormatoImpressao? FormatoImpressao { get; set; }

    /// <summary>O tipo de emissão. Nulo na escrita: o valor do núcleo (a configuração).</summary>
    [Tag("tpEmis", "B22")]
    public TipoEmissao? TipoEmissao { get; set; }

    /// <summary>O ambiente. Nulo na escrita: o valor do núcleo (a configuração).</summary>
    [Tag("tpAmb", "B24")]
    public Ambiente? Ambiente { get; set; }

    /// <summary>A finalidade da emissão.</summary>
    [Tag("finNFe", "B25")]
    public FinalidadeEmissao? Finalidade { get; set; }

    /// <summary>O tipo da nota de débito; opcional.</summary>
    [Tag("tpNFDebito", "B25.1")]
    public TipoNotaDebito? TipoDebito { get; set; }

    /// <summary>O tipo da nota de crédito; opcional.</summary>
    [Tag("tpNFCredito", "B25.2")]
    public TipoNotaCredito? TipoCredito { get; set; }

    /// <summary>Se a operação é com consumidor final.</summary>
    [Tag("indFinal", "B25a")]
    public IndicadorConsumidorFinal? ConsumidorFinal { get; set; }

    /// <summary>A presença do comprador no estabelecimento.</summary>
    [Tag("indPres", "B25b")]
    public PresencaComprador? Presenca { get; set; }

    /// <summary>Se a operação usa plataforma de terceiros (intermediador); opcional.</summary>
    [Tag("indIntermed", "B25c")]
    public IndicadorIntermediador? Intermediacao { get; set; }

    /// <summary>O código do indicador da operação (6 dígitos); opcional.</summary>
    [Tag("cIndOp", "B25d")]
    public string? CodigoLocalOperacao { get; set; }

    /// <summary>O processo de emissão.</summary>
    [Tag("procEmi", "B26")]
    public ProcessoEmissao? Processo { get; set; }

    /// <summary>A versão do aplicativo emissor.</summary>
    [Tag("verProc", "B27")]
    public string? VersaoAplicativo { get; set; }

    /// <summary>
    /// A data e a hora da entrada em contingência; com a <see cref="JustificativaContingencia"/>. Nulo na escrita: o
    /// valor do núcleo (a configuração).
    /// </summary>
    [Tag("dhCont", "B28")]
    public DateTimeOffset? InicioContingencia { get; set; }

    /// <summary>
    /// A justificativa da entrada em contingência; com o <see cref="InicioContingencia"/>. Nulo na escrita: o valor do
    /// núcleo (a configuração).
    /// </summary>
    [Tag("xJust", "B29")]
    public string? JustificativaContingencia { get; set; }

    /// <summary>Os documentos referenciados (até 999).</summary>
    [Tag("NFref", "BA01")]
    public IList<DocumentoReferenciado> Referencias { get; } = new List<DocumentoReferenciado>();

    /// <summary>A compra governamental; opcional.</summary>
    [Tag("gCompraGov", "BB01")]
    public CompraGovernamental? CompraGovernamental { get; set; }

    /// <summary>As notas de antecipação de pagamento que esta abate; opcional.</summary>
    [Tag("gPagAntecipado", "BC01")]
    public PagamentoAntecipado? PagamentoAntecipado { get; set; }
}
