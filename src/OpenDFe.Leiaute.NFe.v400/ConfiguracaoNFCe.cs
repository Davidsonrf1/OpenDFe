namespace OpenDFe.NFe;

/// <summary>
/// A parte da configuração que é da NFC-e (PROJETO.md 3.2 e 9.1, docs/CONTRATOS.md 16): a versão do QR Code, o CSC e as
/// URLs de consulta. Opcional: sem ela, o QR Code sai na versão 3 com as URLs da tabela do leiaute
/// (<see cref="LeiauteNFe400.UrlsConsulta"/>). Vale também para a NF-e com o DANFE Simplificado Tipo 2 (<c>tpImp</c> 6,
/// NT 2026.002), só pelas URLs: o QR Code dela é sempre a versão 3. Imutável.
/// </summary>
public sealed class ConfiguracaoNFCe : ConfiguracaoDocumento
{
    private readonly Uri? _urlQrCode;
    private readonly Uri? _urlChave;

    /// <summary>O QR Code na versão 3, sem CSC.</summary>
    public ConfiguracaoNFCe()
    {
        QrCode = VersaoQrCode.V3;
    }

    /// <summary>O QR Code na versão 2, com o CSC da UF.</summary>
    /// <param name="csc">O CSC.</param>
    /// <exception cref="ArgumentNullException">O CSC é nulo.</exception>
    public ConfiguracaoNFCe(CodigoSeguranca csc)
    {
        ArgumentNullException.ThrowIfNull(csc);
        QrCode = VersaoQrCode.V2;
        Csc = csc;
    }

    /// <summary>A versão do QR Code: <see cref="VersaoQrCode.V3"/> sem CSC, <see cref="VersaoQrCode.V2"/> com ele.</summary>
    public VersaoQrCode QrCode { get; }

    /// <summary>O CSC; nulo na versão 3.</summary>
    public CodigoSeguranca? Csc { get; }

    /// <summary>
    /// Troca a URL do QR Code da tabela do leiaute, na UF e no ambiente do núcleo (que são os da nota): o endereço sem a
    /// query, a que a lib acrescenta o <c>?p=</c>.
    /// </summary>
    /// <exception cref="ArgumentException">A URL é relativa, não é http nem https, ou tem query ou fragmento.</exception>
    public Uri? UrlQrCode
    {
        get => _urlQrCode;
        init => _urlQrCode = Conferir(value, nameof(UrlQrCode), semQuery: true);
    }

    /// <summary>Troca a URL da consulta pela chave (o <c>urlChave</c>) da tabela do leiaute.</summary>
    /// <exception cref="ArgumentException">A URL é relativa ou não é http nem https.</exception>
    public Uri? UrlChave
    {
        get => _urlChave;
        init => _urlChave = Conferir(value, nameof(UrlChave), semQuery: false);
    }

    private static Uri? Conferir(Uri? url, string nome, bool semQuery)
    {
        if (url is null)
            return null;
        if (!url.IsAbsoluteUri || url.Scheme is not ("http" or "https"))
            throw new ArgumentException("A URL tem de ser absoluta, em http ou https.", nome);
        if (semQuery && (url.Query.Length > 0 || url.Fragment.Length > 0))
            throw new ArgumentException("A URL do QR Code vai sem a query: a lib acrescenta o ?p=.", nome);
        return url;
    }
}
