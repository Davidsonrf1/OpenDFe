using System.Security.Cryptography.X509Certificates;

namespace OpenDFe;

/// <summary>Uma mensagem a transmitir, e se ela já saiu inteira.</summary>
internal sealed class EnvioSefaz
{
    private int _enviado;

    public EnvioSefaz(X509Certificate2 certificado, Destino autorizador, Uri url, string servico, string xml)
    {
        Certificado = certificado;
        Autorizador = autorizador;
        Url = url;
        Servico = servico;
        Xml = xml;
    }

    public X509Certificate2 Certificado { get; }

    public Destino Autorizador { get; }

    public Uri Url { get; }

    public string Servico { get; }

    public string Xml { get; }

    /// <summary>A mensagem saiu inteira: a SEFAZ pode tê-la processado.</summary>
    public bool Enviado => Volatile.Read(ref _enviado) != 0;

    public void MarcarEnviado() => Volatile.Write(ref _enviado, 1);
}
