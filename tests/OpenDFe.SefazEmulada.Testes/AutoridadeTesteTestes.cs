using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// A AC de teste e o TLS mútuo (PROJETO.md 13.5): o certificado do servidor emitido por ela, a confiança pelo
// ac.crt exportado, o certificado do cliente exigido e aceito sem cadeia.
public sealed class AutoridadeTesteTestes(EmuladorFixture fixture) : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly List<string> _pastas = [];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ConfiaPeloArquivoExportado()
    {
        var arquivo = Path.Combine(fixture.Pasta, AutoridadeTeste.ArquivoCertificado);
        Assert.Equal(arquivo, fixture.Emulador.ArquivoAutoridade);
        using var autoridade = X509Certificate2.CreateFromPem(await File.ReadAllTextAsync(arquivo, Ct));
        using var http = Clientes.Criar(autoridade, Clientes.CertificadoCliente);

        var retorno = await http.ConsultarStatusAsync(fixture.Url("MG"), Ct);

        Assert.Equal("107", retorno.Campo("cStat"));
        Assert.False(autoridade.HasPrivateKey);
        Assert.Equal(fixture.Emulador.Autoridade.Thumbprint, autoridade.Thumbprint);
    }

    [Fact]
    public async Task OServidorNaoEConfiavelSemAAutoridade()
    {
        using var http = Clientes.Criar(autoridade: null, Clientes.CertificadoCliente);

        await Assert.ThrowsAsync<HttpRequestException>(() => http.ConsultarStatusAsync(fixture.Url("MG"), Ct));
    }

    [Fact]
    public async Task ExigeOCertificadoDoCliente()
    {
        using var http = Clientes.Criar(fixture.Emulador.Autoridade, certificado: null);

        await Assert.ThrowsAsync<HttpRequestException>(() => http.ConsultarStatusAsync(fixture.Url("MG"), Ct));
    }

    [Fact]
    public async Task AceitaOutroCertificadoAutoassinado()
    {
        using var outro = X509CertificateLoader.LoadPkcs12(
            new CertificadoTeste { Cpf = "52998224725", Nome = "PESSOA DE TESTE", NaoDepois = DateTimeOffset.UtcNow.AddDays(-1), NaoAntes = DateTimeOffset.UtcNow.AddDays(-30) }.GerarPfx("teste"), "teste");
        using var http = Clientes.Criar(fixture.Emulador.Autoridade, outro);

        var retorno = await http.ConsultarStatusAsync(fixture.Url("MG"), Ct);

        Assert.Equal("107", retorno.Campo("cStat"));
    }

    [Fact]
    public void GeraUmaAutoridadeDeTeste()
    {
        using var autoridade = AutoridadeTeste.Abrir(PastaNova(), TimeProvider.System);

        Assert.True(autoridade.HasPrivateKey);
        Assert.Equal(autoridade.SubjectName.Name, autoridade.IssuerName.Name);
        var restricoes = Assert.Single(autoridade.Extensions.OfType<X509BasicConstraintsExtension>());
        Assert.True(restricoes.CertificateAuthority);
        Assert.True(autoridade.NotAfter > DateTime.Now.AddYears(9));
    }

    [Fact]
    public void ReaproveitaAAutoridadeDaPasta()
    {
        var pasta = PastaNova();
        using var primeira = AutoridadeTeste.Abrir(pasta, TimeProvider.System);
        File.Delete(Path.Combine(pasta, AutoridadeTeste.ArquivoCertificado));

        using var segunda = AutoridadeTeste.Abrir(pasta, TimeProvider.System);

        Assert.Equal(primeira.Thumbprint, segunda.Thumbprint);
        Assert.True(File.Exists(Path.Combine(pasta, AutoridadeTeste.ArquivoCertificado)));
    }

    [Fact]
    public void GeraOutraAutoridadeQuandoAGuardadaVence()
    {
        var pasta = PastaNova();
        using var primeira = AutoridadeTeste.Abrir(pasta, TimeProvider.System);

        using var segunda = AutoridadeTeste.Abrir(pasta, new RelogioFixo(DateTimeOffset.UtcNow.AddYears(10)));

        Assert.NotEqual(primeira.Thumbprint, segunda.Thumbprint);
    }

    [Fact]
    public void EmiteOServidorPelaAutoridadeComOsNomesLocais()
    {
        using var autoridade = AutoridadeTeste.Abrir(PastaNova(), TimeProvider.System);

        using var servidor = AutoridadeTeste.EmitirServidor(autoridade, ["sefaz.teste", "10.0.0.5"], TimeProvider.System);

        Assert.True(servidor.HasPrivateKey);
        Assert.Equal(autoridade.SubjectName.Name, servidor.IssuerName.Name);
        Assert.Equal("CN=10.0.0.5", servidor.Subject);
        Assert.True(servidor.MatchesHostname("localhost"));
        Assert.True(servidor.MatchesHostname("sefaz.teste"));
        Assert.True(servidor.MatchesHostname("127.0.0.1"));
        Assert.True(servidor.MatchesHostname("10.0.0.5"));
        Assert.True(servidor.MatchesHostname("[::1]"));
        Assert.False(servidor.MatchesHostname("outro.teste"));
        using var cadeia = new X509Chain();
        cadeia.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        cadeia.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        cadeia.ChainPolicy.CustomTrustStore.Add(autoridade);
        cadeia.ChainPolicy.ApplicationPolicy.Add(new System.Security.Cryptography.Oid("1.3.6.1.5.5.7.3.1"));
        Assert.True(cadeia.Build(servidor), string.Join("; ", cadeia.ChainStatus.Select(s => s.StatusInformation)));
    }

    public void Dispose()
    {
        foreach (var pasta in _pastas.Where(Directory.Exists))
            Directory.Delete(pasta, recursive: true);
    }

    private string PastaNova()
    {
        var pasta = Path.Combine(Path.GetTempPath(), "OpenDFe.SefazEmulada.Testes", Guid.NewGuid().ToString("N"));
        _pastas.Add(pasta);
        return pasta;
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }
}

// As opções da linha de comando.
public class OpcoesEmuladorTestes
{
    [Fact]
    public void UsaOPadraoSemArgumentos()
    {
        var opcoes = OpcoesEmulador.Ler([]);

        Assert.Equal(OpcoesEmulador.PortaPadrao, opcoes.Porta);
        Assert.Equal(IPAddress.Loopback, opcoes.Endereco);
        Assert.Equal("127.0.0.1", opcoes.Nome);
        Assert.EndsWith("OpenDFe.SefazEmulada", opcoes.Pasta, StringComparison.Ordinal);
        Assert.False(opcoes.Registrar);
    }

    [Fact]
    public void LeAsOpcoes()
    {
        var opcoes = OpcoesEmulador.Ler(["--porta", "6000", "--pasta", "ac", "--endereco", "0.0.0.0", "--nome", "sefaz.teste"]);

        Assert.Equal(6000, opcoes.Porta);
        Assert.Equal(Path.GetFullPath("ac"), opcoes.Pasta);
        Assert.Equal(IPAddress.Any, opcoes.Endereco);
        Assert.Equal("sefaz.teste", opcoes.Nome);
    }

    [Theory]
    [InlineData("--porta")]
    [InlineData("--desconhecida", "1")]
    public void RecusaAOpcaoErrada(params string[] argumentos)
    {
        Assert.Throws<ArgumentException>(() => OpcoesEmulador.Ler(argumentos));
    }
}
