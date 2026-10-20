namespace OpenDFe.NFe;

/// <summary>A COFINS do item (<c>COFINS</c>): exatamente um dos grupos do XML, uma subclasse para cada.</summary>
/// <remarks>
/// Hierarquia fechada: só a lib deriva; quem faz <c>switch</c> pelo tipo precisa de um caso padrão. O CST está em todos
/// os grupos, com a mesma tag e o mesmo identificador, e fica aqui.
/// </remarks>
public abstract class Cofins
{
    private protected Cofins()
    {
    }

    /// <summary>
    /// O CST, o mesmo campo em todos os grupos. Nulo na escrita, no <see cref="CofinsQuantidade"/>: o único valor dele
    /// (<c>03</c>).
    /// </summary>
    [Tag("CST", "S06")]
    public CstPisCofins? Cst { get; set; }
}
