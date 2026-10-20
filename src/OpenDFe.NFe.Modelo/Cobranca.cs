namespace OpenDFe.NFe;

/// <summary>A cobrança (<c>cobr</c>).</summary>
public sealed class Cobranca
{
    /// <summary>A fatura; opcional.</summary>
    [Tag("fat", "Y02")]
    public Fatura? Fatura { get; set; }

    /// <summary>As duplicatas (até 120).</summary>
    [Tag("dup", "Y07")]
    public IList<Duplicata> Duplicatas { get; } = new List<Duplicata>();
}
