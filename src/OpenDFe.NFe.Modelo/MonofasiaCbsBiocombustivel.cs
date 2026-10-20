namespace OpenDFe.NFe;

/// <summary>
/// A diferença da CBS pela mistura de biocombustível (EAC) com a gasolina A em percentual diferente do obrigatório
/// (<c>gpBioDiferenca</c>).
/// </summary>
/// <remarks>
/// Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no ad rem e no ad valorem da
/// CBS.
/// </remarks>
public sealed class MonofasiaCbsBiocombustivel
{
    /// <summary>A quantidade de biocombustível (EAC) a recolher ou a ressarcir.</summary>
    [Tag("qBCBioComb")]
    public decimal? Quantidade { get; set; }

    /// <summary>O valor da CBS da diferença.</summary>
    [Tag("vCBSDiferenca")]
    public decimal? Valor { get; set; }
}
