namespace OpenDFe;

/// <summary>
/// O status do serviço do autorizador (PROJETO.md 8.3): se ele está operando, paralisado por um tempo ou sem
/// previsão. Operação comum aos documentos (PROJETO.md 3.4): cada leiaute registra o seu handler, e
/// <c>dfe.ExecutarAsync(new ConsultaStatus())</c> devolve o <see cref="RetornoStatus"/>.
/// </summary>
/// <remarks>
/// <para>
/// Vai ao autorizador da UF do núcleo no tipo de emissão do núcleo: o núcleo em contingência SVC consulta a SVC. O
/// <see cref="ParametroWS.Destino"/> sobrepõe: <c>new ConsultaStatus { Destino = Destino.SvcAN }</c> consulta a
/// SVC-AN com o núcleo em emissão normal, e <c>Destino = Destino.SP</c>, o autorizador de SP.
/// </para>
/// <para>
/// A UF consultada (o <c>cUF</c> do pedido) é a do destino, quando ele é uma UF; nos autorizadores virtuais (SVRS,
/// SVAN, SVC), a do núcleo.
/// </para>
/// </remarks>
public sealed class ConsultaStatus : ParametroWS<RetornoStatus>
{
}
