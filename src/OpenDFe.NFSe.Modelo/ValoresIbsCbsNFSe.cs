namespace OpenDFe.NFSe;

/// <summary>A base de cálculo e as alíquotas do IBS e da CBS (<c>IBSCBS/valores</c> da NFS-e).</summary>
public sealed class ValoresIbsCbsNFSe
{
    /// <summary>A base de cálculo.</summary>
    [Tag("vBC")]
    public decimal? BaseCalculo { get; set; }

    /// <summary>O total de reembolsos, repasses e ressarcimentos; opcional.</summary>
    [Tag("vCalcReeRepRes")]
    public decimal? ReembolsoRepasse { get; set; }

    /// <summary>O IBS estadual.</summary>
    [Tag("uf")]
    public AliquotaIbsUF UF { get; set; } = new();

    /// <summary>O IBS municipal.</summary>
    [Tag("mun")]
    public AliquotaIbsMunicipio Municipio { get; set; } = new();

    /// <summary>A CBS.</summary>
    [Tag("fed")]
    public AliquotaCbs Cbs { get; set; } = new();
}
