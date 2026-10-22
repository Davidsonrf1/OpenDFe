using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe;

/// <summary>Uma mensagem a transmitir, e se ela já saiu inteira.</summary>
internal sealed class EnvioSefaz
{
    private int _enviado;

    public EnvioSefaz(
        X509Certificate2 certificado, Destino autorizador, Uri url, ServicoSoap servico, string xml,
        IReadOnlyList<TlsCipherSuite>? cifras = null)
    {
        Certificado = certificado;
        Autorizador = autorizador;
        Url = url;
        Soap = servico;
        Xml = xml;
        Cifras = cifras;
    }

    public X509Certificate2 Certificado { get; }

    public Destino Autorizador { get; }

    public Uri Url { get; }

    /// <summary>O namespace do WSDL e a ação do serviço.</summary>
    public ServicoSoap Soap { get; }

    public string Servico => Soap.Nome;

    public string Xml { get; }

    /// <summary>As cifras que o leiaute declara para o autorizador; <see langword="null"/> = o padrão da máquina.</summary>
    public IReadOnlyList<TlsCipherSuite>? Cifras { get; }

    /// <summary>A mensagem saiu inteira: a SEFAZ pode tê-la processado.</summary>
    public bool Enviado => Volatile.Read(ref _enviado) != 0;

    public void MarcarEnviado() => Volatile.Write(ref _enviado, 1);
}
