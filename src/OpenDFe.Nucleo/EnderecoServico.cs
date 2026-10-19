namespace OpenDFe;

/// <summary>
/// A URL de um serviço (<c>"NFeStatusServico4"</c>) num autorizador e num ambiente. O leiaute declara as
/// padrão (<see cref="DeclaracaoLeiaute.DeclararEndereco"/>); quem usa troca as que mudaram
/// (<see cref="ConfiguracaoDFe.Urls"/>).
/// </summary>
public sealed class EnderecoServico
{
    /// <summary>Cria o endereço de um serviço.</summary>
    /// <param name="autorizador">O autorizador (uma UF, o AN ou um virtual).</param>
    /// <param name="ambiente">O ambiente.</param>
    /// <param name="servico">O nome do serviço, como o leiaute o usa no pedido (<c>"NFeStatusServico4"</c>).</param>
    /// <param name="url">A URL, absoluta e HTTPS.</param>
    /// <exception cref="ArgumentNullException">O serviço ou a URL são nulos.</exception>
    /// <exception cref="ArgumentOutOfRangeException">O autorizador ou o ambiente não são valores conhecidos.</exception>
    /// <exception cref="ArgumentException">O serviço está vazio; a URL não é absoluta ou não é HTTPS.</exception>
    public EnderecoServico(Destino autorizador, Ambiente ambiente, string servico, Uri url)
    {
        if (!Enum.IsDefined(autorizador))
            throw new ArgumentOutOfRangeException(nameof(autorizador), autorizador, "O valor não é um destino.");
        if (!Enum.IsDefined(ambiente))
            throw new ArgumentOutOfRangeException(nameof(ambiente), ambiente, "O valor não é um ambiente.");
        ArgumentException.ThrowIfNullOrWhiteSpace(servico);
        ArgumentNullException.ThrowIfNull(url);
        if (!url.IsAbsoluteUri || url.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException($"A URL do serviço {servico} não é absoluta e HTTPS.", nameof(url));

        Autorizador = autorizador;
        Ambiente = ambiente;
        Servico = servico;
        Url = url;
    }

    /// <summary>O autorizador.</summary>
    public Destino Autorizador { get; }

    /// <summary>O ambiente.</summary>
    public Ambiente Ambiente { get; }

    /// <summary>O nome do serviço (<c>"NFeStatusServico4"</c>).</summary>
    public string Servico { get; }

    /// <summary>A URL.</summary>
    public Uri Url { get; }

    internal (Destino, Ambiente, string) Chave => (Autorizador, Ambiente, Servico);
}
