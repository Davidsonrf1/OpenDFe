namespace OpenDFe.NFSe;

/// <summary>O IBS e a CBS declarados por quem emite (<c>IBSCBS</c>, <c>TCRTCInfoIBSCBS</c>).</summary>
public sealed class IbsCbsDps
{
    /// <summary>A finalidade da emissão.</summary>
    [Tag("finNFSe")]
    public FinalidadeNFSe? Finalidade { get; set; }

    /// <summary>Se a operação é de uso ou consumo pessoal; opcional.</summary>
    [Tag("indFinal")]
    public ConsumoPessoal? ConsumoPessoal { get; set; }

    /// <summary>O código indicador da operação (6 dígitos, Anexo C).</summary>
    [Tag("cIndOp")]
    public string? CodigoOperacao { get; set; }

    /// <summary>O tipo de operação com ente governamental ou sobre bens imóveis; opcional.</summary>
    [Tag("tpOper")]
    public TipoOperacaoGoverno? TipoOperacao { get; set; }

    /// <summary>As NFS-e referenciadas; opcional.</summary>
    [Tag("gRefNFSe")]
    public NFSeReferenciadas? Referencias { get; set; }

    /// <summary>O tipo de ente governamental; opcional.</summary>
    [Tag("tpEnteGov")]
    public EnteGovernamental? EnteGovernamental { get; set; }

    /// <summary>Se o destinatário é o próprio tomador.</summary>
    [Tag("indDest")]
    public IndicadorDestinatario? IndicadorDestinatario { get; set; }

    /// <summary>O destinatário, quando não é o tomador; opcional.</summary>
    [Tag("dest")]
    public Destinatario? Destinatario { get; set; }

    /// <summary>A operação com bem imóvel, exceto obra; opcional.</summary>
    [Tag("imovel")]
    public Imovel? Imovel { get; set; }

    /// <summary>Os valores e os tributos do IBS e da CBS.</summary>
    [Tag("valores")]
    public ValoresIbsCbs Valores { get; set; } = new();
}
