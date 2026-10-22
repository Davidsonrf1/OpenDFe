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
// AmbienteHomologacao; os offline rodam sempre. Desde a 4.1, sobre o transporte da lib (ClienteSefaz): a confiança e as
// raízes foram para o núcleo, e os testes offline delas para o Nucleo.Testes (ConfiancaServidorTestes).
public class ProvaRiscoTestes
{
    private const string UrlStatusMG = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeStatusServico4";
    private const string UrlStatusSVRS = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static void Registrar(string texto) => TestContext.Current.TestOutputHelper?.WriteLine(texto);

    // Offline: o envelope e o status

    [Fact]
    public void MontaOEnvelopeDoStatus()
    {
        var envelope = XDocument.Parse(EnvelopeSoap.Montar(StatusServico.Servico, StatusServico.Pedido(UF.MG)));

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

        var retorno = StatusServico.Ler(EnvelopeSoap.Ler(resposta));

        Assert.Equal(new RetornoStatus("2", "MG-4.00", "107", "Servico em Operacao", "31", "2026-10-19T16:00:00-03:00", "1"), retorno);
    }

    [Fact]
    public void AFalhaSoapViraExcecao()
    {
        const string resposta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><soap:Fault>"
            + "<soap:Code><soap:Value>soap:Receiver</soap:Value></soap:Code><soap:Reason><soap:Text xml:lang=\"pt\">Erro interno</soap:Text></soap:Reason>"
            + "</soap:Fault></soap:Body></soap:Envelope>";

        var erro = Assert.Throws<FalhaSoapException>(() => EnvelopeSoap.Ler(resposta));
        Assert.Contains("Erro interno", erro.Message, StringComparison.Ordinal);
    }

    // Rede (OPENDFE_HOMOLOGACAO=1): a cadeia que cada servidor apresenta, sem o certificado do cliente. O
    // servidor que o exige no handshake (MG, PR) aborta antes da avaliação, no Windows e no Linux: o teste
    // pula, e a cadeia dele fica para o status com o PFX.

    [Theory]
    [InlineData("hnfe.fazenda.mg.gov.br", OrigemProva.Sistema)]
    [InlineData("hom1.nfe.fazenda.gov.br", OrigemProva.Sistema)]
    [InlineData("nfe-homologacao.svrs.rs.gov.br", OrigemProva.IcpBrasil)]
    [InlineData("homologacao.nfe.fazenda.sp.gov.br", OrigemProva.IcpBrasil)]
    [InlineData("homologacao.nfe.sefa.pr.gov.br", OrigemProva.IcpBrasil)]
    public async Task OServidorDeHomologacaoTemCadeiaConfiavel(string servidor, OrigemProva esperada)
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
    [InlineData(UF.MG, UrlStatusMG, OrigemProva.Sistema)]
    [InlineData(UF.SC, UrlStatusSVRS, OrigemProva.IcpBrasil)]
    public async Task ConsultaOStatusDoServicoEmHomologacao(UF uf, string url, OrigemProva confianca)
    {
        using var certificado = AmbienteHomologacao.AbrirPfx();
        var intermediarias = AmbienteHomologacao.AbrirIntermediarias();
        try
        {
            using var cliente = new ClienteSefaz(certificado, intermediarias);

            var relogio = Stopwatch.StartNew();
            var resposta = await cliente.EnviarAsync(new Uri(url), StatusServico.Pedido(uf), Ct);
            relogio.Stop();

            Registrar($"{uf} ({url}): {relogio.ElapsedMilliseconds} ms, confiança {cliente.UltimaOrigem}");
            var retorno = StatusServico.Ler(resposta);
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

    // Com a SEFAZ emulada de pé (OPENDFE_EMULADOR; PROJETO.md 13.5): o lado do cliente do TLS mútuo, que a
    // homologação real só aceita com o PFX ICP-Brasil. O autoassinado (13.1) aberto pelo AbrirPfx da 1.3
    // (DefaultKeySet), no handshake, com o emulador exigindo o certificado como MG e PR.

    [Theory]
    [InlineData(UF.MG, "MG")]
    [InlineData(UF.SC, "SVRS")]
    [InlineData(UF.SP, "SP")]
    public async Task ConsultaOStatusNaSefazEmuladaComOCertificadoAutoassinado(UF uf, string destino)
    {
        var url = new Uri(AmbienteHomologacao.Emulador(), destino + "/NFeStatusServico4");
        using var raiz = AmbienteHomologacao.AbrirACEmulador();
        using var certificado = CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("senha"), "senha");
        using var cliente = new ClienteSefaz(certificado, [], raiz);

        var relogio = Stopwatch.StartNew();
        var resposta = await cliente.EnviarAsync(url, StatusServico.Pedido(uf), Ct);
        relogio.Stop();

        Registrar($"{uf} ({url}): {relogio.ElapsedMilliseconds} ms, confiança {cliente.UltimaOrigem}");
        var retorno = StatusServico.Ler(resposta);
        Registrar($"cStat {retorno.Situacao} {retorno.Motivo}; verAplic {retorno.VersaoAplicativo}; cUF {retorno.CodigoUF}");
        Assert.Equal("2", retorno.TipoAmbiente);
        Assert.Equal("107", retorno.Situacao);
        Assert.Equal(OrigemProva.Emulador, cliente.UltimaOrigem);
    }

    [Fact]
    public async Task RecusaASefazEmuladaSemAACDela()
    {
        var url = new Uri(AmbienteHomologacao.Emulador(), "MG/NFeStatusServico4");
        using var certificado = CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("senha"), "senha");
        using var cliente = new ClienteSefaz(certificado, []);

        var falha = await Assert.ThrowsAsync<FalhaTransporte>(() => cliente.EnviarAsync(url, StatusServico.Pedido(UF.MG), Ct));
        Assert.Equal(TipoFalhaTransmissao.Tls, falha.Tipo);
        Assert.False(falha.PodeTerSidoProcessado);
        Assert.Equal(OrigemProva.Recusado, cliente.UltimaOrigem);
    }

    // A esperada é a do Linux, cujo repositório (o do Mozilla) não traz a ICP-Brasil. O do Windows traz a
    // Raiz v10 (conferido em 2026-10-19): lá, a cadeia ICP-Brasil já passa pelo sistema.
    private static void ConferirOrigem(OrigemProva esperada, OrigemProva origem)
    {
        if (OperatingSystem.IsWindows() && esperada == OrigemProva.IcpBrasil)
            Assert.NotEqual(OrigemProva.Recusado, origem);
        else
            Assert.Equal(esperada, origem);
    }

    // O handshake sem o certificado do cliente: a avaliação do certificado do servidor e como terminou.
    private static async Task<(OrigemProva? Origem, string? Erro)> AvaliarServidorAsync(string servidor)
    {
        OrigemProva? origem = null;
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
                origem = (OrigemProva)(int)ConfiancaServidor.Avaliar(certificado, cadeia, erros);
                return origem != OrigemProva.Recusado;
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
