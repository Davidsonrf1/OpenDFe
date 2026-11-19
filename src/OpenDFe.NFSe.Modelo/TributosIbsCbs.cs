namespace OpenDFe.NFSe;

/// <summary>Os tributos IBS e CBS declarados (<c>IBSCBS/valores/trib</c>).</summary>
public sealed class TributosIbsCbs
{
    /// <summary>A situação e a classificação tributária.</summary>
    [Tag("gIBSCBS")]
    public ClassificacaoIbsCbs Classificacao { get; set; } = new();
}
