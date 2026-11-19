namespace OpenDFe.NFSe;

/// <summary>A exportação ou a importação do serviço (<c>comExt</c>).</summary>
public sealed class ComercioExterior
{
    /// <summary>O modo de prestação.</summary>
    [Tag("mdPrestacao")]
    public ModoPrestacao? ModoPrestacao { get; set; }

    /// <summary>O vínculo entre as partes.</summary>
    [Tag("vincPrest")]
    public VinculoPartes? Vinculo { get; set; }

    /// <summary>O código da moeda da transação (3 dígitos).</summary>
    [Tag("tpMoeda")]
    public string? Moeda { get; set; }

    /// <summary>O valor do serviço na moeda estrangeira.</summary>
    [Tag("vServMoeda")]
    public decimal? ValorMoeda { get; set; }

    /// <summary>O mecanismo de apoio ou fomento ao comércio exterior do prestador.</summary>
    [Tag("mecAFComexP")]
    public MecanismoFomentoPrestador? MecanismoPrestador { get; set; }

    /// <summary>O mecanismo de apoio ou fomento ao comércio exterior do tomador.</summary>
    [Tag("mecAFComexT")]
    public MecanismoFomentoTomador? MecanismoTomador { get; set; }

    /// <summary>O vínculo com a movimentação temporária de bens.</summary>
    [Tag("movTempBens")]
    public MovimentacaoTemporariaBens? MovimentacaoBens { get; set; }

    /// <summary>O número da declaração de importação; opcional.</summary>
    [Tag("nDI")]
    public string? DeclaracaoImportacao { get; set; }

    /// <summary>O número do registro de exportação; opcional.</summary>
    [Tag("nRE")]
    public string? RegistroExportacao { get; set; }

    /// <summary>Se a NFS-e é compartilhada com a Secretaria de Comércio Exterior.</summary>
    [Tag("mdic")]
    public CompartilhamentoMdic? CompartilharMdic { get; set; }
}
