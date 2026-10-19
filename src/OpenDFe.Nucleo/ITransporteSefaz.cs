namespace OpenDFe;

/// <summary>
/// A transmissão de uma mensagem a um autorizador (o passo 6 do <see cref="DFe.ExecutarAsync{TRetorno}"/>):
/// recebe a mensagem (o conteúdo do <c>nfeDadosMsg</c>) e devolve a resposta (o conteúdo do
/// <c>nfeResultMsg</c>). Interno: a costura do <see cref="ConexoesSefaz"/> até a etapa 4.1, que traz o SOAP
/// real.
/// </summary>
/// <remarks>
/// Qualquer exceção é falha de transmissão, exceto a <see cref="OperationCanceledException"/> do token de quem
/// chama. Antes de a mensagem sair inteira, o transporte não chama <see cref="EnvioSefaz.MarcarEnviado"/>; depois,
/// chama: a falha a partir daí pode ter sido processada pela SEFAZ.
/// </remarks>
internal interface ITransporteSefaz
{
    Task<string> EnviarAsync(EnvioSefaz envio, CancellationToken cancellationToken);
}
