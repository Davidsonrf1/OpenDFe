namespace OpenDFe.NFe;

/// <summary>O ICMS do item (<c>ICMS</c>): exatamente um dos grupos do XML, uma subclasse para cada.</summary>
/// <remarks>
/// Hierarquia fechada: só a lib deriva. Um grupo novo entra como subclasse nova em versão menor; quem faz <c>switch</c>
/// pelo tipo precisa de um caso padrão. A <see cref="Origem"/> está em todos os grupos; o CST e o CSOSN ficam nas
/// subclasses.
/// </remarks>
public abstract class Icms
{
    private protected Icms()
    {
    }

    /// <summary>A origem da mercadoria.</summary>
    [Tag("orig", "N11")]
    public OrigemMercadoria? Origem { get; set; }
}
