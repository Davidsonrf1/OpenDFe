namespace OpenDFe.NFe;

/// <summary>O local de destino da operação. O valor de cada membro é o do XML (<c>idDest</c>).</summary>
public enum LocalDestino
{
    /// <summary>Operação interna (1).</summary>
    Interna = 1,

    /// <summary>Operação interestadual (2).</summary>
    Interestadual = 2,

    /// <summary>Operação com o exterior (3).</summary>
    Exterior = 3,
}
