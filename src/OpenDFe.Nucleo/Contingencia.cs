namespace OpenDFe;

/// <summary>
/// A entrada em contingência: o início (<c>dhCont</c>) e a justificativa (<c>xJust</c>) de toda nota emitida
/// fora do tipo de emissão <see cref="TipoEmissao.Normal"/> (<see cref="ConfiguracaoDFe.Contingencia"/>).
/// </summary>
public sealed class Contingencia
{
    /// <summary>Cria a entrada em contingência.</summary>
    /// <param name="inicio">O instante da entrada em contingência, com o fuso.</param>
    /// <param name="justificativa">O motivo da contingência, em texto livre.</param>
    /// <exception cref="ArgumentException">A justificativa é nula ou vazia.</exception>
    /// <remarks>O tamanho da justificativa (de 15 a 256 caracteres na NF-e) é conferido pelo schema.</remarks>
    public Contingencia(DateTimeOffset inicio, string justificativa)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(justificativa);
        Inicio = inicio;
        Justificativa = justificativa;
    }

    /// <summary>O instante da entrada em contingência (<c>dhCont</c>).</summary>
    public DateTimeOffset Inicio { get; }

    /// <summary>A justificativa (<c>xJust</c>).</summary>
    public string Justificativa { get; }
}
