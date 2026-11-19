namespace OpenDFe;

/// <summary>
/// O que foi enviado e recebido numa operação, para quem quiser guardar (PROJETO.md 8.7): no
/// <see cref="ResultadoWS{TRetorno}.Rastro"/> e na <see cref="FalhaTransmissaoException.Rastro"/>. Criado pelo
/// núcleo.
/// </summary>
/// <remarks>
/// Os XMLs têm dados pessoais: guarde-os com o cuidado do documento, nunca em log nem telemetria.
/// </remarks>
public sealed class RastroWS
{
    internal RastroWS(
        string? xmlPedido, string? xmlRetorno, Destino? autorizador, Uri? url,
        DateTimeOffset inicio, TimeSpan duracao, TimeSpan? duracaoTransmissao, int? statusHttp = null)
    {
        StatusHttp = statusHttp;
        XmlPedido = xmlPedido;
        XmlRetorno = xmlRetorno;
        Autorizador = autorizador;
        Url = url;
        Inicio = inicio;
        Duracao = duracao;
        DuracaoTransmissao = duracaoTransmissao;
    }

    /// <summary>
    /// A mensagem (o conteúdo do <c>nfeDadosMsg</c>) como foi, ou seria, enviada, assinada; na recusa da
    /// montagem, como o leiaute a montou; <see langword="null"/> se a montagem não a produziu. No REST, o XML assinado e
    /// legível (a DPS), e não o JSON compactado; <see langword="null"/> no pedido sem corpo (docs/NFSE.md, D12).
    /// </summary>
    public string? XmlPedido { get; }

    /// <summary>
    /// A resposta (o conteúdo do <c>nfeResultMsg</c>); <see langword="null"/> se não houve resposta. No REST, o corpo como
    /// veio (o JSON), com o documento ainda compactado; <see langword="null"/> no HEAD.
    /// </summary>
    public string? XmlRetorno { get; }

    /// <summary>O status HTTP da resposta REST; <see langword="null"/> no SOAP e sem resposta (docs/NFSE.md, D12).</summary>
    public int? StatusHttp { get; }

    /// <summary>O autorizador escolhido; <see langword="null"/> se a operação parou antes da escolha.</summary>
    public Destino? Autorizador { get; }

    /// <summary>A URL escolhida; <see langword="null"/> se a operação parou antes da escolha.</summary>
    public Uri? Url { get; }

    /// <summary>O início da operação, pelo <see cref="ConfiguracaoDFe.Relogio"/>.</summary>
    public DateTimeOffset Inicio { get; }

    /// <summary>Do início da operação ao resultado (ou à falha).</summary>
    public TimeSpan Duracao { get; }

    /// <summary>Só a ida e volta ao autorizador; <see langword="null"/> se não chegou a transmitir.</summary>
    public TimeSpan? DuracaoTransmissao { get; }
}
