namespace OpenDFe.NFe;

/// <summary>O IBS monofásico retido anteriormente (<c>gMonoRet</c>).</summary>
/// <remarks>
/// Uma classe para os dois lugares em que o XSD tem o mesmo conteúdo, com o <see cref="TagAttribute"/> só com a tag: o
/// identificador do MOC muda com o lugar e fica na propriedade que guarda o grupo. Está no ad rem e no ad valorem do
/// IBS.
/// </remarks>
public sealed class MonofasiaIbsRetido
{
    /// <summary>O valor do IBS retido anteriormente.</summary>
    [Tag("vIBSMonoRet")]
    public decimal? Valor { get; set; }
}
