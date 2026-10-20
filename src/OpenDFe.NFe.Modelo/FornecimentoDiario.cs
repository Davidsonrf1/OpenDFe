namespace OpenDFe.NFe;

/// <summary>O fornecimento de cana de um dia (<c>forDia</c>).</summary>
public sealed class FornecimentoDiario
{
    /// <summary>O dia (o atributo <c>dia</c>).</summary>
    [Tag("dia", "ZC05")]
    public int? Dia { get; set; }

    /// <summary>A quantidade.</summary>
    [Tag("qtde", "ZC06")]
    public decimal? Quantidade { get; set; }
}
