namespace OpenDFe.NFe;

/// <summary>As NF-e de antecipação de pagamento que esta abate (<c>gPagAntecipado</c>, NT 2025.002).</summary>
public sealed class PagamentoAntecipado
{
    /// <summary>As chaves de acesso (de 1 a 99).</summary>
    [Tag("refNFe", "BC02")]
    public IList<string> Chaves { get; } = new List<string>();
}
