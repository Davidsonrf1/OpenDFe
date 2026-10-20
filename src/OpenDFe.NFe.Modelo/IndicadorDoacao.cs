namespace OpenDFe.NFe;

/// <summary>A operação de doação. O valor de cada membro é o do XML (<c>indDoacao</c>).</summary>
/// <remarks>
/// O XSD aceita só o 1; sem o indicador, a propriedade fica nula. Enumeração, e não <see cref="bool"/>, para ganhar
/// valor em versão menor.
/// </remarks>
public enum IndicadorDoacao
{
    /// <summary>Doação (1).</summary>
    Doacao = 1,
}
