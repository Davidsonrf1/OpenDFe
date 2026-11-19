namespace OpenDFe;

/// <summary>
/// A transmissão a um autorizador (o passo 8 do <see cref="DFe.ExecutarAsync{TRetorno}"/>). Interno: o
/// <see cref="TransporteSoap"/> é o real; os testes do núcleo injetam um falso.
/// </summary>
/// <remarks>
/// Qualquer exceção é falha de transmissão (a <see cref="FalhaTransporte"/> já classificada), exceto a
/// <see cref="OperationCanceledException"/> do token de quem chama. Antes de o pedido sair inteiro, o transporte não chama
/// <see cref="EnvioTransporte.MarcarEnviado"/>; depois, chama: a falha a partir daí pode ter sido processada.
/// </remarks>
internal interface ITransporteSefaz
{
    /// <summary>O SOAP: recebe a mensagem (o conteúdo do <c>nfeDadosMsg</c>) e devolve a do <c>nfeResultMsg</c>.</summary>
    Task<string> EnviarAsync(EnvioSefaz envio, CancellationToken cancellationToken);

    /// <summary>
    /// O REST (docs/NFSE.md, D4 e D5): devolve a resposta que o leiaute lê (2xx, e 4xx com JSON ou o 404 do HEAD); o resto
    /// é <see cref="FalhaTransporte"/>.
    /// </summary>
    Task<RespostaRest> EnviarRestAsync(EnvioRest envio, CancellationToken cancellationToken);
}
