using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// Prova de risco (PLANO.md 1.7): o RSA-SHA1 com o certificado real (risco 1) e o TLS mútuo com a SEFAZ
// (risco 2), no Windows e no Linux. Os que falam com a SEFAZ são pulados sem as variáveis de
// AmbienteHomologacao; os offline rodam sempre. O código é descartável: a 4.1 o refaz no núcleo.
public class ProvaRiscoTestes
{
    private const string UrlStatusMG = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeStatusServico4";
    private const string UrlStatusSVRS = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static void Registrar(string texto) => TestContext.Current.TestOutputHelper?.WriteLine(texto);

    // Offline: as raízes e a confiança

    [Fact]
    public void CarregaAsRaizesIcpBrasilVigentes()
    {
        var raizes = RaizesIcpBrasil.Todas;

        Assert.Equal(7, raizes.Count);
        foreach (var raiz in raizes)
        {
            Assert.Equal(raiz.SubjectName.Name, raiz.IssuerName.Name);
            Assert.Contains("Autoridade Certificadora Raiz Brasileira", raiz.Subject, StringComparison.Ordinal);
            Assert.True(raiz.NotAfter > DateTime.Now, $"{raiz.Subject} venceu em {raiz.NotAfter:yyyy-MM-dd}.");
            var restricoes = Assert.Single(raiz.Extensions.OfType<X509BasicConstraintsExtension>());
            Assert.True(restricoes.CertificateAuthority);
        }
    }

    [Fact]
    public void AceitaOQueOSistemaAceita()
    {
        using var servidor = ServidorAutoassinado();

        Assert.Equal(OrigemConfianca.Sistema, ConfiancaServidor.Avaliar(servidor, null, SslPolicyErrors.None));
    }

    [Fact]
    public void RecusaOServidorQueNaoSobeAUmaRaiz()
    {
        using var servidor = ServidorAutoassinado();
        using var cadeia = new X509Chain();
        cadeia.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        Assert.False(cadeia.Build(servidor));

        Assert.Equal(OrigemConfianca.Recusado, ConfiancaServidor.Avaliar(servidor, cadeia, SslPolicyErrors.RemoteCertificateChainErrors));
    }

    [Theory]
    [InlineData(SslPolicyErrors.RemoteCertificateNameMismatch)]
    [InlineData(SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors)]
    [InlineData(SslPolicyErrors.RemoteCertificateNotAvailable)]
    public void RecusaONomeDivergenteEOCertificadoAusente(SslPolicyErrors erros)
    {
        using var servidor = ServidorAutoassinado();

        Assert.Equal(OrigemConfianca.Recusado, ConfiancaServidor.Avaliar(servidor, null, erros));
        Assert.Equal(OrigemConfianca.Recusado, ConfiancaServidor.Avaliar(null, null, SslPolicyErrors.None));
    }

    // Offline: o envelope e o status

    [Fact]
    public void MontaOEnvelopeDoStatus()
    {
        var envelope = XDocument.Parse(EnvelopeSoap.Montar(StatusServico.NamespaceWsdl, StatusServico.Pedido(UF.MG)));

        var corpo = Assert.Single(envelope.Root!.Elements());
        Assert.Equal(XName.Get("Body", EnvelopeSoap.NamespaceSoap), corpo.Name);
        var dados = Assert.Single(corpo.Elements());
        Assert.Equal(XName.Get("nfeDadosMsg", StatusServico.NamespaceWsdl), dados.Name);
        var pedido = Assert.Single(dados.Elements());
        Assert.Equal(XName.Get("consStatServ", StatusServico.NamespaceNFe), pedido.Name);
        Assert.Equal("4.00", pedido.Attribute("versao")?.Value);
        Assert.Equal(["2", "31", "STATUS"], pedido.Elements().Select(e => e.Value));
    }

    [Fact]
    public void LeORetornoDoStatus()
    {
        const string resposta = "<?xml version=\"1.0\" encoding=\"utf-8\"?><soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">"
            + "<soap:Body><nfeResultMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\">"
            + "<retConsStatServ versao=\"4.00\" xmlns=\"http://www.portalfiscal.inf.br/nfe\"><tpAmb>2</tpAmb><verAplic>MG-4.00</verAplic>"
            + "<cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo><cUF>31</cUF><dhRecbto>2026-10-19T16:00:00-03:00</dhRecbto><tMed>1</tMed>"
            + "</retConsStatServ></nfeResultMsg></soap:Body></soap:Envelope>";

        var retorno = StatusServico.Ler(EnvelopeSoap.LerResultado(resposta));

        Assert.Equal(new RetornoStatus("2", "MG-4.00", "107", "Servico em Operacao", "31", "2026-10-19T16:00:00-03:00", "1"), retorno);
    }

    [Fact]
    public void AFalhaSoapViraExcecao()
    {
        const string resposta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><soap:Fault>"
            + "<soap:Code><soap:Value>soap:Receiver</soap:Value></soap:Code><soap:Reason><soap:Text xml:lang=\"pt\">Erro interno</soap:Text></soap:Reason>"
            + "</soap:Fault></soap:Body></soap:Envelope>";

        var erro = Assert.Throws<InvalidOperationException>(() => EnvelopeSoap.LerResultado(resposta));
        Assert.Contains("Erro interno", erro.Message, StringComparison.Ordinal);
    }

    // Rede (OPENDFE_HOMOLOGACAO=1): a cadeia que cada servidor apresenta, sem o certificado do cliente. O
    // servidor que o exige no handshake (MG, PR) aborta antes da avaliação, no Windows e no Linux: o teste
    // pula, e a cadeia dele fica para o status com o PFX.

    [Theory]
    [InlineData("hnfe.fazenda.mg.gov.br", OrigemConfianca.Sistema)]
    [InlineData("hom1.nfe.fazenda.gov.br", OrigemConfianca.Sistema)]
    [InlineData("nfe-homologacao.svrs.rs.gov.br", OrigemConfianca.IcpBrasil)]
    [InlineData("homologacao.nfe.fazenda.sp.gov.br", OrigemConfianca.IcpBrasil)]
    [InlineData("homologacao.nfe.sefa.pr.gov.br", OrigemConfianca.IcpBrasil)]
    public async Task OServidorDeHomologacaoTemCadeiaConfiavel(string servidor, OrigemConfianca esperada)
    {
        AmbienteHomologacao.ExigirRede();

        var (origem, erro) = await AvaliarServidorAsync(servidor);

        Registrar($"{servidor}: {origem?.ToString() ?? "certificado não avaliado"}; handshake: {erro ?? "completo"}");
        Assert.SkipWhen(origem is null, $"{servidor} exige o certificado do cliente no handshake: {erro}");
        ConferirOrigem(esperada, origem.Value);
    }

    // Com o PFX (OPENDFE_PFX_CAMINHO, OPENDFE_PFX_SENHA)

    [Fact]
    public void AbreOCertificadoRealELeOTitular()
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        try
        {
            Assert.Equal(ValidadeCertificado.Vigente, CertificadoDigital.Validade(certificado, DateTimeOffset.Now));
            var titular = CertificadoDigital.LerTitular(certificado);
            Assert.NotNull(titular);

            // Sem o documento no registro: só o perfil e o que a leitura achou.
            var perfil = certificado.SubjectName.EnumerateRelativeDistinguishedNames().Any(r => r.GetSingleElementType().Value == "2.5.4.5")
                ? "serialNumber no titular (DOC-ICP-04 8.2 em diante)"
                : "sem serialNumber no titular (perfil até a 8.1)";
            Registrar($"Titular {(titular.Cnpj is null ? "CPF" : "CNPJ")}; {perfil}; CN com ':': {certificado.GetNameInfo(X509NameType.SimpleName, false).Contains(':', StringComparison.Ordinal)}");
            Registrar($"Emissor: {certificado.Issuer}; chave {certificado.PublicKey.Oid.FriendlyName} {certificado.GetRSAPublicKey()?.KeySize}; vence {certificado.NotAfter:yyyy-MM-dd}");
            Registrar($"Intermediárias no PFX: {intermediarias.Count} ({string.Join("; ", intermediarias.Select(c => c.GetNameInfo(X509NameType.SimpleName, false)))})");
            Registrar($"Sistema: {Environment.OSVersion}; {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        }
        finally
        {
            foreach (var intermediaria in intermediarias)
                intermediaria.Dispose();
        }
    }

    [Fact]
    public void AssinaEmRsaSha1EVerificaComOCertificadoReal()
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var titular = CertificadoDigital.LerTitular(certificado) ?? throw new InvalidOperationException("O certificado não tem CNPJ nem CPF.");
        var emitente = titular.Cnpj ?? titular.Cpf!;
        var chave = ChaveAcesso.Gerar(UF.MG, DateTimeOffset.Now, emitente, Documento.NFe, 1, 1, TipoEmissao.Normal);
        var id = "NFe" + chave;
        var grupo = titular.Cnpj is null ? "CPF" : "CNPJ";
        var nota = $"<NFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infNFe Id=\"{id}\" versao=\"4.00\"><ide><cUF>31</cUF><nNF>1</nNF></ide>"
            + $"<emit><{grupo}>{emitente}</{grupo}><xNome>NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL</xNome></emit></infNFe></NFe>";

        var assinada = AssinaturaDigital.Assinar(nota, id, certificado);

        Assert.Contains("http://www.w3.org/2000/09/xmldsig#rsa-sha1", assinada, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(assinada, id, emitente));
        Assert.Equal("297", Assert.Single(AssinaturaDigital.Verificar(assinada.Replace("<nNF>1</nNF>", "<nNF>2</nNF>", StringComparison.Ordinal), id, emitente)).Codigo);
    }

    [Theory]
    [InlineData(UF.MG, UrlStatusMG, OrigemConfianca.Sistema)]
    [InlineData(UF.SC, UrlStatusSVRS, OrigemConfianca.IcpBrasil)]
    public async Task ConsultaOStatusDoServicoEmHomologacao(UF uf, string url, OrigemConfianca confianca)
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        try
        {
            using var cliente = new ClienteSefaz(certificado, intermediarias);
            var envelope = EnvelopeSoap.Montar(StatusServico.NamespaceWsdl, StatusServico.Pedido(uf));

            var relogio = Stopwatch.StartNew();
            var resposta = await cliente.EnviarAsync(new Uri(url), StatusServico.Acao, envelope, Ct);
            relogio.Stop();

            Registrar($"{uf} ({url}): HTTP {(int)resposta.Status} {resposta.VersaoHttp}, {relogio.ElapsedMilliseconds} ms, confiança {cliente.UltimaOrigem}");
            Assert.True(resposta.Status == HttpStatusCode.OK, $"HTTP {(int)resposta.Status}: {resposta.Texto}");
            var retorno = StatusServico.Ler(EnvelopeSoap.LerResultado(resposta.Texto));
            Registrar($"cStat {retorno.Situacao} {retorno.Motivo}; verAplic {retorno.VersaoAplicativo}; dhRecbto {retorno.Recebimento}; tMed {retorno.TempoMedio}");
            Assert.Equal("2", retorno.TipoAmbiente);
            Assert.Equal("107", retorno.Situacao);       // Serviço em Operação
            ConferirOrigem(confianca, cliente.UltimaOrigem);
        }
        finally
        {
            foreach (var intermediaria in intermediarias)
                intermediaria.Dispose();
        }
    }

    // A esperada é a do Linux, cujo repositório (o do Mozilla) não traz a ICP-Brasil. O do Windows traz a
    // Raiz v10 (conferido em 2026-10-19): lá, a cadeia ICP-Brasil já passa pelo sistema.
    private static void ConferirOrigem(OrigemConfianca esperada, OrigemConfianca origem)
    {
        if (OperatingSystem.IsWindows() && esperada == OrigemConfianca.IcpBrasil)
            Assert.NotEqual(OrigemConfianca.Recusado, origem);
        else
            Assert.Equal(esperada, origem);
    }

    private static X509Certificate2 ServidorAutoassinado()
    {
        using var chave = RSA.Create(2048);
        var pedido = new CertificateRequest("CN=servidor.teste", chave, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        pedido.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], false));
        var agora = DateTimeOffset.UtcNow;
        using var autoassinado = pedido.CreateSelfSigned(agora.AddDays(-1), agora.AddDays(1));
        return X509CertificateLoader.LoadCertificate(autoassinado.RawData);
    }

    // O handshake sem o certificado do cliente: a avaliação do certificado do servidor e como terminou.
    private static async Task<(OrigemConfianca? Origem, string? Erro)> AvaliarServidorAsync(string servidor)
    {
        OrigemConfianca? origem = null;
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(servidor, 443, Ct);
        await using var ssl = new SslStream(tcp.GetStream(), false);
        var opcoes = new SslClientAuthenticationOptions
        {
            TargetHost = servidor,
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
            RemoteCertificateValidationCallback = (_, certificado, cadeia, erros) =>
            {
                origem = ConfiancaServidor.Avaliar(certificado, cadeia, erros);
                return origem != OrigemConfianca.Recusado;
            },
        };
        try
        {
            await ssl.AuthenticateAsClientAsync(opcoes, Ct);
            return (origem, null);
        }
        catch (Exception erro) when (erro is AuthenticationException or IOException)
        {
            return (origem, erro.GetType().Name + ": " + erro.Message);
        }
    }
}
