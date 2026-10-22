using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

/// <summary>
/// A confiança no servidor (PROJETO.md 8.5, o achado da 1.7): o sistema, ou a cadeia montada de novo com as raízes
/// ICP-Brasil embutidas. Trazidos da prova da 1.7; a cadeia real de cada autorizador é dos testes de homologação.
/// </summary>
public class ConfiancaServidorTestes
{
    [Fact]
    public void CarregaAsRaizesIcpBrasilVigentes()
    {
        var raizes = RaizesIcpBrasil.Todas;

        Assert.Equal(7, raizes.Count);
        Assert.Equal(
            ["v10", "v11", "v12", "v13", "v5", "v6", "v7"],
            raizes.Select(r => r.GetNameInfo(X509NameType.SimpleName, false)["Autoridade Certificadora Raiz Brasileira ".Length..]));
        foreach (var raiz in raizes)
        {
            Assert.Equal(raiz.SubjectName.Name, raiz.IssuerName.Name);
            Assert.True(raiz.NotAfter > DateTime.Now, $"{raiz.Subject} venceu em {raiz.NotAfter:yyyy-MM-dd}.");
            var restricoes = Assert.Single(raiz.Extensions.OfType<X509BasicConstraintsExtension>());
            Assert.True(restricoes.CertificateAuthority);
        }
    }

    [Fact]
    public void AceitaOQueOSistemaAceita()
    {
        using var servidor = Autoassinado("CN=servidor.teste");

        Assert.Equal(OrigemConfianca.Sistema, ConfiancaServidor.Avaliar(servidor, null, SslPolicyErrors.None));
        Assert.True(ConfiancaServidor.Validar(this, servidor, null, SslPolicyErrors.None));
    }

    [Fact]
    public void RecusaOServidorQueNaoSobeAUmaRaiz()
    {
        using var servidor = Autoassinado("CN=servidor.teste");
        using var cadeia = new X509Chain();
        cadeia.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        Assert.False(cadeia.Build(servidor));

        Assert.Equal(OrigemConfianca.Recusado, ConfiancaServidor.Avaliar(servidor, cadeia, SslPolicyErrors.RemoteCertificateChainErrors));
        Assert.False(ConfiancaServidor.Validar(this, servidor, cadeia, SslPolicyErrors.RemoteCertificateChainErrors));
    }

    [Fact]
    public void RecusaOServidorQueSeDizRaizIcpBrasilSemSer()
    {
        // Mesmo nome da Raiz v10, outra chave: a cadeia não fecha na raiz embutida.
        using var falsa = Autoassinado("CN=Autoridade Certificadora Raiz Brasileira v10, OU=Instituto Nacional de Tecnologia da Informacao - ITI, O=ICP-Brasil, C=BR");

        Assert.Equal(OrigemConfianca.Recusado, ConfiancaServidor.Avaliar(falsa, null, SslPolicyErrors.RemoteCertificateChainErrors));
    }

    [Theory]
    [InlineData(SslPolicyErrors.RemoteCertificateNameMismatch)]
    [InlineData(SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors)]
    [InlineData(SslPolicyErrors.RemoteCertificateNotAvailable)]
    public void RecusaONomeDivergenteEOCertificadoAusente(SslPolicyErrors erros)
    {
        using var servidor = Autoassinado("CN=servidor.teste");

        Assert.Equal(OrigemConfianca.Recusado, ConfiancaServidor.Avaliar(servidor, null, erros));
        Assert.Equal(OrigemConfianca.Recusado, ConfiancaServidor.Avaliar(null, null, SslPolicyErrors.None));
    }

    private static X509Certificate2 Autoassinado(string nome)
    {
        using var chave = RSA.Create(2048);
        var pedido = new CertificateRequest(nome, chave, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        pedido.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], false));
        var agora = DateTimeOffset.UtcNow;
        using var autoassinado = pedido.CreateSelfSigned(agora.AddDays(-1), agora.AddDays(1));
        return X509CertificateLoader.LoadCertificate(autoassinado.RawData);
    }
}
