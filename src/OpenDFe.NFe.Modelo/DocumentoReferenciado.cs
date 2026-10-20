namespace OpenDFe.NFe;

/// <summary>
/// Um documento referenciado (<c>NFref</c>): exatamente uma das escolhas do XML, uma subclasse para cada.
/// </summary>
/// <remarks>
/// Hierarquia fechada: só a lib deriva. Uma escolha nova entra como subclasse nova em versão menor; quem faz
/// <c>switch</c> pelo tipo precisa de um caso padrão.
/// </remarks>
public abstract class DocumentoReferenciado
{
    private protected DocumentoReferenciado()
    {
    }
}
