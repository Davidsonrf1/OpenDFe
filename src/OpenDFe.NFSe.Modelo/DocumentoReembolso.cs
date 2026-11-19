namespace OpenDFe.NFSe;

/// <summary>Um documento de reembolso, repasse ou ressarcimento (<c>gReeRepRes/documentos</c>).</summary>
public sealed class DocumentoReembolso
{
    /// <summary>O documento fiscal eletrônico do repositório nacional; um dos três documentos.</summary>
    [Tag("dFeNacional")]
    public DFeNacional? DFeNacional { get; set; }

    /// <summary>O documento fiscal fora do repositório nacional.</summary>
    [Tag("docFiscalOutro")]
    public DocumentoFiscalOutro? DocumentoFiscal { get; set; }

    /// <summary>O documento não fiscal.</summary>
    [Tag("docOutro")]
    public DocumentoOutro? DocumentoNaoFiscal { get; set; }

    /// <summary>O fornecedor; opcional.</summary>
    [Tag("fornec")]
    public FornecedorDocumento? Fornecedor { get; set; }

    /// <summary>A data da emissão do documento.</summary>
    [Tag("dtEmiDoc")]
    public DateOnly? Emissao { get; set; }

    /// <summary>A data da competência do documento.</summary>
    [Tag("dtCompDoc")]
    public DateOnly? Competencia { get; set; }

    /// <summary>O tipo do valor.</summary>
    [Tag("tpReeRepRes")]
    public TipoReembolso? Tipo { get; set; }

    /// <summary>A descrição, quando o tipo é "outros"; opcional.</summary>
    [Tag("xTpReeRepRes")]
    public string? DescricaoTipo { get; set; }

    /// <summary>O valor fora da base de cálculo.</summary>
    [Tag("vlrReeRepRes")]
    public decimal? Valor { get; set; }
}
