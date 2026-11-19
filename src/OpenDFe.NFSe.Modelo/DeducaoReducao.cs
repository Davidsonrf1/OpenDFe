namespace OpenDFe.NFSe;

/// <summary>A dedução ou a redução da base de cálculo (<c>vDedRed</c>): o percentual, o valor ou os documentos.</summary>
public sealed class DeducaoReducao
{
    /// <summary>O percentual de dedução ou redução; um dos três (percentual, valor ou documentos).</summary>
    [Tag("pDR")]
    public decimal? Percentual { get; set; }

    /// <summary>O valor de dedução ou redução.</summary>
    [Tag("vDR")]
    public decimal? Valor { get; set; }

    /// <summary>Os documentos da dedução ou da redução.</summary>
    [Tag("documentos")]
    public DocumentosDeducao? Documentos { get; set; }
}
