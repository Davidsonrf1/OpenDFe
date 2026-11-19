namespace OpenDFe.NFSe;

/// <summary>Os valores e os tributos do IBS e da CBS declarados (<c>IBSCBS/valores</c>).</summary>
public sealed class ValoresIbsCbs
{
    /// <summary>Os reembolsos, repasses e ressarcimentos fora da base de cálculo; opcional.</summary>
    [Tag("gReeRepRes")]
    public ReembolsoRepasse? ReembolsoRepasse { get; set; }

    /// <summary>Os tributos.</summary>
    [Tag("trib")]
    public TributosIbsCbs Tributos { get; set; } = new();
}
