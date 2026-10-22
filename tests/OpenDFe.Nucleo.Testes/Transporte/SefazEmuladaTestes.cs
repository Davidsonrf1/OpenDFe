using System.Xml.Linq;
using OpenDFe.SefazEmulada;

namespace OpenDFe.Testes;

/// <summary>Uma SEFAZ emulada por classe de teste, numa porta livre e com a AC numa pasta temporária.</summary>
public sealed class SefazEmuladaFixture : IAsyncLifetime
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N"));

    internal Emulador Emulador { get; private set; } = null!;

    public async ValueTask InitializeAsync() =>
        Emulador = await Emulador.IniciarAsync(new OpcoesEmulador { Porta = 0, Pasta = _pasta });

    public async ValueTask DisposeAsync()
    {
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

/// <summary>
/// O encontro da 4.1 com a trilha paralela (PLANO.md, "Encontros"): o transporte da lib contra a SEFAZ emulada, com o
/// certificado autoassinado (13.1) no TLS mútuo e a AC do emulador pela fábrica de teste. O status de três
/// autorizadores e cada falha comandada pelo <c>/_controle</c>, com a espécie e se pode ter sido processada.
/// </summary>
public sealed class SefazEmuladaTestes : IClassFixture<SefazEmuladaFixture>
{
    private const string NamespaceNFe = "http://www.portalfiscal.inf.br/nfe";
    private const string NamespaceWsdl = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4";

    private static readonly ServicoSoap Status = new("NFeStatusServico4", NamespaceWsdl, NamespaceWsdl + "/nfeStatusServicoNF");

    private static readonly System.Security.Cryptography.X509Certificates.X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("teste"), "teste");

    private readonly Emulador _emulador;

    public SefazEmuladaTestes(SefazEmuladaFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ConexoesSefaz Conexoes(TimeSpan? tempoResposta = null) =>
        new(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = tempoResposta ?? TimeSpan.FromSeconds(30) };

    private Task<string> EnviarAsync(ConexoesSefaz conexoes, string destino, UF uf, ServicoSoap? servico = null) =>
        conexoes.Transporte().EnviarAsync(
            new EnvioSefaz(Certificado, Destino.MG, new Uri(_emulador.Endereco, $"{destino}/NFeStatusServico4"), servico ?? Status, Pedido(uf)), Ct);

    private static string Pedido(UF uf) =>
        $"<consStatServ xmlns=\"{NamespaceNFe}\" versao=\"4.00\"><tpAmb>2</tpAmb><cUF>{uf.Codigo()}</cUF><xServ>STATUS</xServ></consStatServ>";

    private static string CStat(string retorno)
    {
        var raiz = XElement.Parse(retorno);
        Assert.Equal(XName.Get("retConsStatServ", NamespaceNFe), raiz.Name);
        return raiz.Element(XName.Get("cStat", NamespaceNFe))!.Value;
    }

    [Theory]
    [InlineData("MG", UF.MG)]
    [InlineData("SVRS", UF.SC)]
    [InlineData("SP", UF.SP)]
    public async Task ConsultaOStatusComOCertificadoAutoassinado(string destino, UF uf)
    {
        using var conexoes = Conexoes();

        Assert.Equal("107", CStat(await EnviarAsync(conexoes, destino, uf)));
    }

    [Fact]
    public async Task OServicoParalisadoEResposta()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Vezes: 1));
        using var conexoes = Conexoes();

        Assert.Equal("108", CStat(await EnviarAsync(conexoes, "MG", UF.MG)));
    }

    [Fact]
    public async Task OAtrasoDentroDoTempoResponde()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Atraso, "MG", Milissegundos: 300, Vezes: 1));
        using var conexoes = Conexoes();

        Assert.Equal("107", CStat(await EnviarAsync(conexoes, "MG", UF.MG)));
    }

    public static TheoryData<string, TipoFalhaTransmissao, bool> Falhas => new()
    {
        { "tempo esgotado", TipoFalhaTransmissao.TempoEsgotado, true },
        { "atraso além do tempo", TipoFalhaTransmissao.TempoEsgotado, true },
        { "conexão derrubada antes", TipoFalhaTransmissao.Conexao, true },
        { "conexão derrubada depois", TipoFalhaTransmissao.Conexao, true },
        { "resposta ilegível", TipoFalhaTransmissao.RespostaIlegivel, true },
        { "falha SOAP", TipoFalhaTransmissao.FalhaSoap, true },
        { "namespace errado", TipoFalhaTransmissao.FalhaSoap, false },
    };

    [Theory]
    [MemberData(nameof(Falhas))]
    public async Task CadaFalhaComandadaTemAEspecieCerta(string caso, TipoFalhaTransmissao tipo, bool podeTerSidoProcessado)
    {
        ServicoSoap? servico = null;
        switch (caso)
        {
            case "tempo esgotado":
                _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.TempoEsgotado, "MG", Vezes: 1));
                break;
            case "atraso além do tempo":
                _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Atraso, "MG", Milissegundos: 5000, Vezes: 1));
                break;
            case "conexão derrubada antes":
                _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1));
                break;
            case "conexão derrubada depois":
                _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1, Depois: true));
                break;
            case "resposta ilegível":
                _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.RespostaIlegivel, "MG", Vezes: 1));
                break;
            case "falha SOAP":
                _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, "MG", Vezes: 1));
                break;
            case "namespace errado":
                servico = new ServicoSoap(Status.Nome, "http://www.portalfiscal.inf.br/nfe/wsdl/Outro", Status.Acao);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(caso));
        }
        using var conexoes = Conexoes(TimeSpan.FromSeconds(1));

        var falha = await Assert.ThrowsAsync<FalhaTransporte>(() => EnviarAsync(conexoes, "MG", UF.MG, servico));

        Assert.Equal(tipo, falha.Tipo);
        Assert.Equal(podeTerSidoProcessado, falha.PodeTerSidoProcessado);

        // A falha foi uma vez só: o pedido seguinte, no mesmo pool, é respondido.
        Assert.Equal("107", CStat(await EnviarAsync(conexoes, "MG", UF.MG)));
    }
}
