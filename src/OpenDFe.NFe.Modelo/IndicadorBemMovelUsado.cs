namespace OpenDFe.NFe;

/// <summary>O fornecimento de bem móvel usado. O valor de cada membro é o do XML (<c>indBemMovelUsado</c>).</summary>
/// <remarks>
/// O XSD aceita só o 1; sem o indicador, a propriedade fica nula. Enumeração, e não <see cref="bool"/>, para ganhar
/// valor em versão menor.
/// </remarks>
public enum IndicadorBemMovelUsado
{
    /// <summary>Bem móvel usado (1).</summary>
    Usado = 1,
}
