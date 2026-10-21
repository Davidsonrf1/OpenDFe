using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// Um emulador por classe de teste, numa porta livre e com a AC numa pasta temporária, e o HttpClient que
// confia nela e apresenta o certificado autoassinado dos testes (13.1). As falhas comandadas são limpas
// por cada classe que as usa. O relógio do emulador anda com o do sistema, mais o avanço que o teste der
// (o prazo do cancelamento, a emissão da substituta); quem avança volta a zero no fim.
public class EmuladorFixture : IAsyncLifetime
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "OpenDFe.SefazEmulada.Testes", Guid.NewGuid().ToString("N"));

    internal Emulador Emulador { get; private set; } = null!;

    public HttpClient Http { get; private set; } = null!;

    public string Pasta => _pasta;

    public RelogioAjustavel Relogio { get; } = new();

    public Uri Url(string destino, string servico = Servico.NomeStatus) => new(Emulador.Endereco, $"{destino}/{servico}");

    public async ValueTask InitializeAsync()
    {
        Emulador = await Emulador.IniciarAsync(Configurar(new OpcoesEmulador { Porta = 0, Pasta = _pasta, Relogio = Relogio }));
        Http = Clientes.Criar(Emulador.Autoridade, Clientes.CertificadoCliente);
    }

    // As opções de quem herda (o painel, os pedidos guardados).
    private protected virtual OpcoesEmulador Configurar(OpcoesEmulador opcoes) => opcoes;

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
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

// O relógio do sistema adiantado pelo Avanco; os temporizadores são os do sistema.
public sealed class RelogioAjustavel : TimeProvider
{
    public TimeSpan Avanco { get; set; }

    public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + Avanco;
}

// O cliente HTTP dos testes, sem a lib: TLS 1.2 ou 1.3, o certificado do cliente e a confiança só na AC dada.
internal static class Clientes
{
    // e-CNPJ autoassinado; recarregado do PKCS#12 porque o SChannel não usa a chave efêmera no TLS.
    public static X509Certificate2 CertificadoCliente { get; } =
        X509CertificateLoader.LoadPkcs12(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("teste"), "teste");

    // O e-CNPJ de outra empresa: o transmissor que não enviou o lote (223) e o assinante que não é o emitente (213).
    public static X509Certificate2 OutroCertificado { get; } =
        X509CertificateLoader.LoadPkcs12(new CertificadoTeste { Cnpj = "12345678000195", Nome = "OUTRA EMPRESA LTDA" }.GerarPfx("teste"), "teste");

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

    // A autorização: o enviNFe com as notas, HTTP 200 e o retEnviNFe.
    public static async Task<XElement> AutorizarAsync(
        this HttpClient http, Uri url, bool sincrono, IEnumerable<string> notas, CancellationToken cancellationToken)
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(Mensagens.Lote(sincrono, [.. notas]), Servico.NomeAutorizacao), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }

    // A consulta do recibo: o consReciNFe, HTTP 200 e o retConsReciNFe.
    public static async Task<XElement> ConsultarReciboAsync(
        this HttpClient http, Uri url, string recibo, CancellationToken cancellationToken, string ambiente = "2")
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(Mensagens.Recibo(recibo, ambiente), Servico.NomeRetAutorizacao), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }

    // A consulta do protocolo: o consSitNFe, HTTP 200 e o retConsSitNFe.
    public static async Task<XElement> ConsultarProtocoloAsync(
        this HttpClient http, Uri url, string chave, CancellationToken cancellationToken, string ambiente = "2")
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(Mensagens.Consulta(chave, ambiente), Servico.NomeConsultaProtocolo), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }

    // O status: POST, HTTP 200 e o retConsStatServ.
    public static async Task<XElement> ConsultarStatusAsync(
        this HttpClient http, Uri url, CancellationToken cancellationToken, string ambiente = "2", string uf = "31")
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(Mensagens.Status(ambiente, uf)), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }

    // Os eventos: o envEvento com os eventos assinados, HTTP 200 e o retEnvEvento.
    public static async Task<XElement> EnviarEventosAsync(
        this HttpClient http, Uri url, IEnumerable<string> eventos, CancellationToken cancellationToken)
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(Mensagens.Eventos([.. eventos]), Servico.NomeRecepcaoEvento), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }

    // A distribuição: o distDFeInt dentro do nfeDistDFeInteresse, HTTP 200 e o retDistDFeInt.
    public static async Task<XElement> DistribuirAsync(
        this HttpClient http, Uri url, string pedido, CancellationToken cancellationToken)
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.EnvelopeDistribuicao(pedido), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.RetornoDistribuicao(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }

    // A consulta cadastro: o ConsCad, HTTP 200 e o retConsCad.
    public static async Task<XElement> ConsultarCadastroAsync(
        this HttpClient http, Uri url, string pedido, CancellationToken cancellationToken)
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(pedido, Servico.NomeConsultaCadastro), cancellationToken);
        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
        return Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(cancellationToken));
    }

    // A inutilização: o inutNFe assinado, HTTP 200 e o retInutNFe.
    public static async Task<XElement> InutilizarAsync(
        this HttpClient http, Uri url, string pedido, CancellationToken cancellationToken)
    {
        using var resposta = await http.EnviarAsync(url, Mensagens.Envelope(pedido, Servico.NomeInutilizacao), cancellationToken);
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

    // O lote da autorização (Visão Geral, Tabela 5-1), com as notas já assinadas.
    public static string Lote(bool sincrono, params string[] notas) =>
        $"<enviNFe xmlns=\"{NamespaceNFe}\" versao=\"4.00\"><idLote>1</idLote><indSinc>{(sincrono ? 1 : 0)}</indSinc>{string.Concat(notas)}</enviNFe>";

    // A consulta do recibo (Tabela 5-5).
    public static string Recibo(string recibo, string ambiente = "2") =>
        $"<consReciNFe xmlns=\"{NamespaceNFe}\" versao=\"4.00\"><tpAmb>{ambiente}</tpAmb><nRec>{recibo}</nRec></consReciNFe>";

    // A consulta do protocolo (Visão Geral, Tabela 5-13).
    public static string Consulta(string chave, string ambiente = "2", string servico = "CONSULTAR") =>
        $"<consSitNFe xmlns=\"{NamespaceNFe}\" versao=\"4.00\"><tpAmb>{ambiente}</tpAmb><xServ>{servico}</xServ><chNFe>{chave}</chNFe></consSitNFe>";

    // O lote de eventos (Tabela 5-32), com os eventos já assinados.
    public static string Eventos(params string[] eventos) =>
        $"<envEvento xmlns=\"{NamespaceNFe}\" versao=\"1.00\"><idLote>7</idLote>{string.Concat(eventos)}</envEvento>";

    // O pedido da distribuição (NT 2014.002, 3.1 e os exemplos do item 5): o interessado e uma das consultas
    // (DistribuirNsu, ConsultarNsu, ConsultarChave).
    public static string Distribuicao(string consulta, string? cnpj = "11222333000181", string? cpf = null, string ambiente = "2", string? uf = "31") =>
        $"<distDFeInt xmlns=\"{NamespaceNFe}\" versao=\"1.01\"><tpAmb>{ambiente}</tpAmb>{(uf is null ? "" : $"<cUFAutor>{uf}</cUFAutor>")}"
        + (cnpj is not null ? $"<CNPJ>{cnpj}</CNPJ>" : $"<CPF>{cpf}</CPF>") + consulta + "</distDFeInt>";

    public static string DistribuirNsu(long ultimo) =>
        $"<distNSU><ultNSU>{ultimo.ToString("D15", System.Globalization.CultureInfo.InvariantCulture)}</ultNSU></distNSU>";

    public static string ConsultarNsu(long nsu) =>
        $"<consNSU><NSU>{nsu.ToString("D15", System.Globalization.CultureInfo.InvariantCulture)}</NSU></consNSU>";

    public static string ConsultarChave(string chave) => $"<consChNFe><chNFe>{chave}</chNFe></consChNFe>";

    // O pedido da consulta cadastro (Visão Geral, Tabela 5-21), com o argumento (<CNPJ>, <IE> ou <CPF>) pronto.
    public static string Cadastro(string uf, string argumento, string servico = "CONS-CAD") =>
        $"<ConsCad xmlns=\"{NamespaceNFe}\" versao=\"2.00\"><infCons><xServ>{servico}</xServ><UF>{uf}</UF>{argumento}</infCons></ConsCad>";

    // O envelope da distribuição, com a operação no Body (NT 2014.002, 5), em SOAP 1.2.
    public static string EnvelopeDistribuicao(string dados) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
        + $"<soap12:Envelope xmlns:soap12=\"{NamespaceSoap}\"><soap12:Body>"
        + $"<nfeDistDFeInteresse xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/{Servico.NomeDistribuicao}\">"
        + $"<nfeDadosMsg>{dados}</nfeDadosMsg></nfeDistDFeInteresse>"
        + "</soap12:Body></soap12:Envelope>";

    // A área de dados do nfeDistDFeInteresseResponse/nfeDistDFeInteresseResult.
    public static XElement RetornoDistribuicao(string envelope)
    {
        var corpo = XDocument.Parse(envelope).Root!.Element(Soap + "Body")!;
        var resposta = Assert.Single(corpo.Elements());
        Assert.Equal("nfeDistDFeInteresseResponse", resposta.Name.LocalName);
        var resultado = Assert.Single(resposta.Elements());
        Assert.Equal("nfeDistDFeInteresseResult", resultado.Name.LocalName);
        return Assert.Single(resultado.Elements());
    }

    // O documento de um docZip: o base64 descompactado do gzip, em UTF-8.
    public static XElement Descompactar(XElement documento)
    {
        using var compactado = new MemoryStream(Convert.FromBase64String(documento.Value));
        using var gzip = new System.IO.Compression.GZipStream(compactado, System.IO.Compression.CompressionMode.Decompress);
        return XDocument.Load(gzip).Root!;
    }

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

    // O infProt do protNFe.
    public static XElement Protocolo(this XElement protocolo) => protocolo.Element(NFe + "infProt")!;
}
