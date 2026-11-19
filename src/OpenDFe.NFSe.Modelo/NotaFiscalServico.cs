namespace OpenDFe.NFSe;

/// <summary>A NFS-e do padrão nacional: o <c>NFSe</c> e o <c>infNFSe</c> juntos (docs/NFSE.md 7, M3), com a DPS de dentro. Devolvida pela leitura; quem a gera é o ambiente nacional ou o município. A chave fica no resultado da leitura.</summary>
public sealed class NotaFiscalServico
{
    /// <summary>O nome do município emissor.</summary>
    [Tag("xLocEmi")]
    public string? NomeMunicipioEmissor { get; set; }

    /// <summary>O nome do local da prestação.</summary>
    [Tag("xLocPrestacao")]
    public string? NomeLocalPrestacao { get; set; }

    /// <summary>O número da NFS-e, sequencial por emitente.</summary>
    [Tag("nNFSe")]
    public long? Numero { get; set; }

    /// <summary>O código IBGE do município de incidência do ISSQN; opcional.</summary>
    [Tag("cLocIncid")]
    public int? MunicipioIncidencia { get; set; }

    /// <summary>O nome do município de incidência do ISSQN; opcional.</summary>
    [Tag("xLocIncid")]
    public string? NomeMunicipioIncidencia { get; set; }

    /// <summary>A descrição do código de tributação nacional.</summary>
    [Tag("xTribNac")]
    public string? DescricaoTributacaoNacional { get; set; }

    /// <summary>A descrição do código de tributação municipal; opcional.</summary>
    [Tag("xTribMun")]
    public string? DescricaoTributacaoMunicipal { get; set; }

    /// <summary>A descrição do código NBS; opcional.</summary>
    [Tag("xNBS")]
    public string? DescricaoNbs { get; set; }

    /// <summary>A versão do aplicativo que gerou a NFS-e.</summary>
    [Tag("verAplic")]
    public string? VersaoAplicativo { get; set; }

    /// <summary>O ambiente que gerou a NFS-e.</summary>
    [Tag("ambGer")]
    public AmbienteGerador? AmbienteGerador { get; set; }

    /// <summary>O tipo de emissão.</summary>
    [Tag("tpEmis")]
    public TipoEmissaoNFSe? TipoEmissao { get; set; }

    /// <summary>O processo de emissão da DPS; opcional.</summary>
    [Tag("procEmi")]
    public ProcessoEmissao? ProcessoEmissao { get; set; }

    /// <summary>A situação da NFS-e.</summary>
    [Tag("cStat")]
    public SituacaoNFSe? Situacao { get; set; }

    /// <summary>A data e a hora do processamento (a geração da NFS-e).</summary>
    [Tag("dhProc")]
    public DateTimeOffset? Processamento { get; set; }

    /// <summary>O número do documento no ambiente gerador.</summary>
    [Tag("nDFSe")]
    public long? NumeroDfse { get; set; }

    /// <summary>O emitente da NFS-e.</summary>
    [Tag("emit")]
    public EmitenteNFSe Emitente { get; set; } = new();

    /// <summary>Os valores calculados pelo ambiente gerador.</summary>
    [Tag("valores")]
    public ValoresNFSe Valores { get; set; } = new();

    /// <summary>O texto de uso da administração tributária municipal (na versão 1.00, dentro de <c>valores</c>); opcional.</summary>
    [Tag("xOutInf")]
    public string? InformacoesFisco { get; set; }

    /// <summary>O IBS e a CBS calculados pelo ambiente gerador; opcional.</summary>
    [Tag("IBSCBS")]
    public IbsCbsNFSe? IbsCbs { get; set; }

    /// <summary>A DPS que gerou a NFS-e.</summary>
    [Tag("DPS")]
    public Dps Dps { get; set; } = new();
}
