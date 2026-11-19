using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe;

/// <summary>
/// O que todo envio tem para o pool (o certificado, as cifras, a URL) e se o pedido já saiu inteiro: a base do envio SOAP
/// e do REST.
/// </summary>
internal abstract class EnvioTransporte
{
    private int _enviado;

    protected EnvioTransporte(X509Certificate2 certificado, Destino autorizador, Uri url, IReadOnlyList<TlsCipherSuite>? cifras)
    {
        Certificado = certificado;
        Autorizador = autorizador;
        Url = url;
        Cifras = cifras;
    }

    public X509Certificate2 Certificado { get; }

    public Destino Autorizador { get; }

    public Uri Url { get; }

    /// <summary>As cifras que o leiaute declara para o autorizador; <see langword="null"/> = o padrão da máquina.</summary>
    public IReadOnlyList<TlsCipherSuite>? Cifras { get; }

    /// <summary>O pedido saiu inteiro: o autorizador pode tê-lo processado.</summary>
    public bool Enviado => Volatile.Read(ref _enviado) != 0;

    public void MarcarEnviado() => Volatile.Write(ref _enviado, 1);
}

/// <summary>Uma mensagem SOAP a transmitir, e se ela já saiu inteira.</summary>
internal sealed class EnvioSefaz : EnvioTransporte
{
    public EnvioSefaz(
        X509Certificate2 certificado, Destino autorizador, Uri url, ServicoSoap servico, string xml,
        IReadOnlyList<TlsCipherSuite>? cifras = null)
        : base(certificado, autorizador, url, cifras)
    {
        Soap = servico;
        Xml = xml;
    }

    /// <summary>O namespace do WSDL e a ação do serviço.</summary>
    public ServicoSoap Soap { get; }

    public string Servico => Soap.Nome;

    public string Xml { get; }
}

/// <summary>
/// Um pedido REST a transmitir (docs/NFSE.md, D4): o método, a URL inteira (a base e o caminho) e o JSON do corpo, já com o
/// documento compactado. O pedido sem corpo (GET, HEAD) nunca é marcado como enviado: a consulta não processa nada.
/// </summary>
internal sealed class EnvioRest : EnvioTransporte
{
    public EnvioRest(
        X509Certificate2 certificado, Destino autorizador, Uri url, string servico, MetodoRest metodo, string? corpo,
        IReadOnlyList<TlsCipherSuite>? cifras = null)
        : base(certificado, autorizador, url, cifras)
    {
        Servico = servico;
        Metodo = metodo;
        Corpo = corpo;
    }

    /// <summary>O nome da base na tabela de URLs (<c>"SefinNacional"</c>).</summary>
    public string Servico { get; }

    public MetodoRest Metodo { get; }

    /// <summary>O JSON do corpo; <see langword="null"/> sem corpo.</summary>
    public string? Corpo { get; }
}
