namespace OpenDFe.NFe;

/// <summary>A CBS monofásica retida anteriormente (<c>gMonoRet</c>).</summary>
/// <remarks>
/// Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no ad rem e no ad valorem da
/// CBS.
/// </remarks>
public sealed class MonofasiaCbsRetido
{
    /// <summary>O valor da CBS retida anteriormente.</summary>
    [Tag("vCBSMonoRet")]
    public decimal? Valor { get; set; }
}
