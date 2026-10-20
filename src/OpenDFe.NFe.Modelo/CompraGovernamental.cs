namespace OpenDFe.NFe;

/// <summary>A compra governamental (<c>gCompraGov</c>, NT 2025.002).</summary>
public sealed class CompraGovernamental
{
    /// <summary>O tipo do ente governamental.</summary>
    [Tag("tpEnteGov", "BB02")]
    public EnteGovernamental? Ente { get; set; }

    /// <summary>O percentual de redução da alíquota.</summary>
    [Tag("pRedutor", "BB03")]
    public decimal? PercentualReducao { get; set; }

    /// <summary>O tipo da operação com o ente governamental.</summary>
    [Tag("tpOperGov", "BB04")]
    public OperacaoGovernamental? Operacao { get; set; }

    /// <summary>As chaves dos documentos anteriores (até 99).</summary>
    [Tag("refDFeAnt", "BB05")]
    public IList<string> DocumentosAnteriores { get; } = new List<string>();
}
