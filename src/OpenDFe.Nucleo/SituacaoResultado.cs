namespace OpenDFe;

/// <summary>O que aconteceu com uma operação (<see cref="ResultadoWS{TRetorno}.Situacao"/>).</summary>
public enum SituacaoResultado
{
    /// <summary>
    /// O autorizador respondeu, inclusive com rejeição: o <see cref="ResultadoWS{TRetorno}.CStat"/> diz qual
    /// (PROJETO.md 2.4).
    /// </summary>
    Processado = 1,

    /// <summary>
    /// Um problema de severidade <see cref="Severidade.Erro"/> antes de transmitir (na montagem ou no schema):
    /// nada foi enviado.
    /// </summary>
    RecusadoLocalmente = 2,
}
