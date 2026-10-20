namespace OpenDFe.NFe;

/// <summary>O PIS do item (<c>PIS</c>): exatamente um dos grupos do XML, uma subclasse para cada.</summary>
/// <remarks>
/// Hierarquia fechada: só a lib deriva; quem faz <c>switch</c> pelo tipo precisa de um caso padrão. O CST está em todos
/// os grupos, com a mesma tag e o mesmo identificador, e fica aqui.
/// </remarks>
public abstract class Pis
{
    private protected Pis()
    {
    }

    /// <summary>
    /// O CST, o mesmo campo em todos os grupos. Nulo na escrita, no <see cref="PisQuantidade"/>: o único valor dele
    /// (<c>03</c>).
    /// </summary>
    [Tag("CST", "Q06")]
    public CstPisCofins? Cst { get; set; }
}
