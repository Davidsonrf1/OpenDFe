namespace OpenDFe;

/// <summary>
/// A espécie de uma <see cref="FalhaTransmissaoException"/>: para quem chama decidir se repete (a conexão e o tempo
/// esgotado) ou se tem algo a corrigir (o certificado recusado).
/// </summary>
public enum TipoFalhaTransmissao
{
    /// <summary>
    /// A rede: o nome que não resolve, a conexão recusada ou derrubada, a resposta que parou no meio, ou outra falha do
    /// transporte.
    /// </summary>
    Conexao = 1,

    /// <summary>
    /// O TLS: o handshake falhou ou o certificado do servidor não é confiável. Inclui o autorizador que aborta o
    /// handshake por não aceitar o certificado do cliente (MG, PR).
    /// </summary>
    Tls = 2,

    /// <summary>
    /// O autorizador respondeu HTTP 403 sem o envelope SOAP: recusou o certificado do cliente depois do handshake
    /// (o IIS da SVRS e de SP). Nada foi processado.
    /// </summary>
    CertificadoRecusado = 3,

    /// <summary>
    /// O tempo esgotou: o de conexão (<see cref="ConexoesSefaz.TempoConexao"/>) ou o de resposta
    /// (<see cref="ConexoesSefaz.TempoResposta"/>).
    /// </summary>
    TempoEsgotado = 4,

    /// <summary>Uma resposta HTTP diferente de 200, sem o envelope SOAP.</summary>
    Http = 5,

    /// <summary>A falha SOAP (<c>Fault</c>); o motivo vem na mensagem da exceção.</summary>
    FalhaSoap = 6,

    /// <summary>
    /// A resposta veio e não se leu: não é um envelope SOAP 1.2, não tem o <c>nfeResultMsg</c> ou o leiaute não a
    /// entendeu.
    /// </summary>
    RespostaIlegivel = 7,
}
