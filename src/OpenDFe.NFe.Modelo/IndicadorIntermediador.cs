namespace OpenDFe.NFe;

/// <summary>
/// Se a operação usa plataforma própria ou de terceiros. O valor de cada membro é o do XML (<c>indIntermed</c>).
/// </summary>
public enum IndicadorIntermediador
{
    /// <summary>Operação sem intermediador (no site ou plataforma própria) (0).</summary>
    PlataformaPropria = 0,

    /// <summary>Operação em site ou plataforma de terceiros (intermediador) (1).</summary>
    PlataformaTerceiros = 1,
}
