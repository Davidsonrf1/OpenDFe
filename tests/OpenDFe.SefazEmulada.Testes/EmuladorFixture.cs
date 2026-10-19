using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// Um emulador por classe de teste, numa porta livre e com a AC numa pasta temporária, e o HttpClient que
// confia nela e apresenta o certificado autoassinado dos testes (13.1). As falhas comandadas são limpas
// por cada classe que as usa.
public sealed class EmuladorFixture : IAsyncLifetime
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "OpenDFe.SefazEmulada.Testes", Guid.NewGuid().ToString("N"));

    internal Emulador Emulador { get; private set; } = null!;

    public HttpClient Http { get; private set; } = null!;

    public string Pasta => _pasta;

    public Uri Url(string destino, string servico = Servico.NomeStatus) => new(Emulador.Endereco, $"{destino}/{servico}");

    public async ValueTask InitializeAsync()
    {
        Emulador = await Emulador.IniciarAsync(new OpcoesEmulador { Porta = 0, Pasta = _pasta });
        Http = Clientes.Criar(Emulador.Autoridade, Clientes.CertificadoCliente);
    }

    public async ValueTask DisposeAsync()
    {
        Http.Dispose();
        await Emulador.DisposeAsync();
        try
        {
            Directory.Delete(_pasta, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}

// O cliente HTTP dos testes, sem a lib: TLS 1.2 ou 1.3, o certificado do cliente e a confiança só na AC dada.
internal static class Clientes
{
    // e-CNPJ autoassinado; recarregado do PKCS#12 porque o SChannel não usa a chave efêmera no TLS.
    public static X509Certificate2 CertificadoCliente { get; } =
        X509CertificateLoader.LoadPkcs12(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("teste"), "teste");

    // Autoridade nula: a confiança do sistema (que não conhece a AC de teste).
    public static HttpClient Criar(X509Certificate2? autoridade, X509Certificate2? certificado)
    {
        var opcoes = new SslClientAuthenticationOptions
        {
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
        };
        if (certificado is not null)
            opcoes.ClientCertificates = [certificado];
        if (autoridade is not null)
        {
            var politica = new X509ChainPolicy
            {
                TrustMode = X509ChainTrustMode.CustomRootTrust,
                RevocationMode = X509RevocationMode.NoCheck,
            };
            politica.CustomTrustStore.Add(autoridade);
            opcoes.CertificateChainPolicy = politica;
        }
        var handler = new SocketsHttpHandler { SslOptions = opcoes, ConnectTimeout = TimeSpan.FromSeconds(10) };
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
    }

    public static async Task<HttpResponseMessage> EnviarAsync(
        this HttpClient http, Uri url, string corpo, CancellationToken cancellationToken, string tipo = "application/soap+xml")
    {
        using var conteudo = new StringContent(corpo, Encoding.UTF8);
        conteudo.Headers.ContentType = MediaTypeHeaderValue.Parse($"{tipo}; charset=utf-8");
        return await http.PostAsync(url, conteudo, cancellationToken);
    }

    // O status: POST, HTTP 200 e o retConsStatServ.
    public static async Task<XElement> ConsultarStatusAsync(
        this HttpClient http, Uri url, CancellationToken cancellationToken, string ambiente = "2", string uf = "31")
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(Mensagens.Status(ambiente, uf)), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }
}

// As mensagens como os manuais as mostram (MOC 7.0, Visão Geral, 4.2.2 e o exemplo do consStatServ),
// escritas em texto, sem o LINQ to XML do emulador.
internal static class Mensagens
{
    public const string NamespaceNFe = "http://www.portalfiscal.inf.br/nfe";
    public const string NamespaceSoap = "http://www.w3.org/2003/05/soap-envelope";

    public static readonly XNamespace NFe = NamespaceNFe;
    public static readonly XNamespace Soap = NamespaceSoap;

    public static string Status(string ambiente = "2", string uf = "31", string versao = "4.00", string servico = "STATUS") =>
        $"<consStatServ xmlns=\"{NamespaceNFe}\" versao=\"{versao}\"><tpAmb>{ambiente}</tpAmb><cUF>{uf}</cUF><xServ>{servico}</xServ></consStatServ>";

    public static string Envelope(string dados, string servico = Servico.NomeStatus) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        + "<soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\""
        + $" xmlns:soap12=\"{NamespaceSoap}\"><soap12:Body>"
        + $"<nfeDadosMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/{servico}\">{dados}</nfeDadosMsg>"
        + "</soap12:Body></soap12:Envelope>";

    // A área de dados do nfeResultMsg.
    public static XElement Retorno(string envelope)
    {
        var corpo = XDocument.Parse(envelope).Root!.Element(Soap + "Body")!;
        var resultado = Assert.Single(corpo.Elements());
        Assert.Equal("nfeResultMsg", resultado.Name.LocalName);
        return Assert.Single(resultado.Elements());
    }

    // O Code/Value da falha SOAP 1.2 (soap:Sender, soap:Receiver, soap:VersionMismatch).
    public static string Falha(string envelope)
    {
        var falha = XDocument.Parse(envelope).Root!.Element(Soap + "Body")!.Element(Soap + "Fault")!;
        return falha.Element(Soap + "Code")!.Element(Soap + "Value")!.Value;
    }

    public static string Campo(this XElement retorno, string nome) => retorno.Element(NFe + nome)?.Value!;
}
