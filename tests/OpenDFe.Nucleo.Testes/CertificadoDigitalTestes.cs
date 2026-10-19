using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OpenDFe.Testes;

// Todos com o certificado autoassinado gerado na hora (CertificadoDeTeste): não há PFX real no repositório.
public class CertificadoDigitalTestes
{
    private const string Senha = "senha-de-teste";
    private const string CnpjNumerico = "11222333000181";
    private const string CnpjAlfanumerico = "12ABC34501DE35";   // exemplo da Receita Federal
    private const string CpfTitular = "52998224725";
    private const string CpfResponsavel = "11144477735";

    private static readonly DateTimeOffset Inicio = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fim = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // Abrir

    [Fact]
    public void AbreOPfxGerado()
    {
        var pfx = new CertificadoDeTeste { Cnpj = CnpjNumerico }.GerarPfx(Senha);

        using var certificado = CertificadoDigital.AbrirPfx(pfx, Senha);

        Assert.True(certificado.HasPrivateKey);
        using var rsa = certificado.GetRSAPrivateKey();
        Assert.NotNull(rsa);
        Assert.Equal(2048, rsa.KeySize);
        var usos = Assert.Single(certificado.Extensions.OfType<X509KeyUsageExtension>());
        Assert.True(usos.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature));
    }

    [Fact]
    public void AChaveAbertaAssinaEVerifica()
    {
        var pfx = new CertificadoDeTeste { Cnpj = CnpjNumerico }.GerarPfx(Senha);
        using var certificado = CertificadoDigital.AbrirPfx(pfx, Senha);
        using var privada = certificado.GetRSAPrivateKey()!;
        using var publica = certificado.GetRSAPublicKey()!;
        byte[] dados = [1, 2, 3, 4];

        var assinatura = privada.SignData(dados, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        Assert.True(publica.VerifyData(dados, assinatura, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void AbreOPfxSemSenha()
    {
        var pfx = new CertificadoDeTeste { Cnpj = CnpjNumerico }.GerarPfx("");

        using var certificado = CertificadoDigital.AbrirPfx(pfx, "");

        Assert.True(certificado.HasPrivateKey);
    }

    [Fact]
    public void AbreOArquivoPfx()
    {
        // Arquivo temporário, fora do repositório, apagado no fim.
        var caminho = Path.Combine(Path.GetTempPath(), $"opendfe-teste-{Guid.NewGuid():N}.pfx");
        File.WriteAllBytes(caminho, new CertificadoDeTeste { Cnpj = CnpjNumerico }.GerarPfx(Senha));
        try
        {
            using var certificado = CertificadoDigital.AbrirArquivoPfx(caminho, Senha);
            Assert.Equal(CnpjNumerico, CertificadoDigital.LerTitular(certificado)?.Cnpj);
        }
        finally
        {
            File.Delete(caminho);
        }
    }

    [Fact]
    public void ArquivoInexistente() =>
        Assert.ThrowsAny<IOException>(() =>
            CertificadoDigital.AbrirArquivoPfx(Path.Combine(Path.GetTempPath(), $"nao-existe-{Guid.NewGuid():N}.pfx"), Senha));

    [Fact]
    public void RecusaASenhaErrada()
    {
        var pfx = new CertificadoDeTeste { Cnpj = CnpjNumerico }.GerarPfx(Senha);

        var erro = Assert.Throws<CryptographicException>(() => CertificadoDigital.AbrirPfx(pfx, "outra senha"));

        Assert.Contains("senha está errada", erro.Message, StringComparison.Ordinal);
        Assert.NotNull(erro.InnerException);
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 1, 2, 3, 4, 5 })]
    [InlineData(new byte[] { 0x30, 0x03, 0x02, 0x01, 0x01 })]   // SEQUENCE { INTEGER 1 }: versão errada
    public void RecusaOQueNaoEPfx(byte[] conteudo)
    {
        var erro = Assert.Throws<CryptographicException>(() => CertificadoDigital.AbrirPfx(conteudo, Senha));
        Assert.Contains("não é um PFX", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RecusaOCertificadoEmDerQueNaoEPfx()
    {
        using var certificado = new CertificadoDeTeste { Cnpj = CnpjNumerico }.Gerar();

        var erro = Assert.Throws<CryptographicException>(() => CertificadoDigital.AbrirPfx(certificado.RawData, Senha));

        Assert.Contains("não é um PFX", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RecusaOPfxSemChavePrivada()
    {
        var pfx = new CertificadoDeTeste { Cnpj = CnpjNumerico }.GerarPfxSemChavePrivada(Senha);

        var erro = Assert.Throws<CryptographicException>(() => CertificadoDigital.AbrirPfx(pfx, Senha));

        Assert.Contains("não tem a chave privada", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RecusaAChaveQueNaoERsa()
    {
        var pfx = new CertificadoDeTeste { Cnpj = CnpjNumerico, ChaveEcdsa = true }.GerarPfx(Senha);

        var erro = Assert.Throws<CryptographicException>(() => CertificadoDigital.AbrirPfx(pfx, Senha));

        Assert.Contains("não é RSA", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AbreOVencidoEOAindaNaoVigente()
    {
        // Abrir não confere a validade: o vencido é aberto para ser lido.
        var vencido = new CertificadoDeTeste { Cnpj = CnpjNumerico, NaoAntes = Inicio, NaoDepois = Fim }.GerarPfx(Senha);
        var futuro = new CertificadoDeTeste
        {
            Cnpj = CnpjNumerico,
            NaoAntes = DateTimeOffset.UtcNow.AddYears(5),
            NaoDepois = DateTimeOffset.UtcNow.AddYears(6),
        }.GerarPfx(Senha);

        using var a = CertificadoDigital.AbrirPfx(vencido, Senha);
        using var b = CertificadoDigital.AbrirPfx(futuro, Senha);

        Assert.True(a.HasPrivateKey);
        Assert.True(b.HasPrivateKey);
    }

    [Fact]
    public void RecusaArgumentosNulos()
    {
        Assert.Throws<ArgumentNullException>(() => CertificadoDigital.AbrirPfx(null!, Senha));
        Assert.Throws<ArgumentNullException>(() => CertificadoDigital.AbrirPfx([], null!));
        Assert.Throws<ArgumentNullException>(() => CertificadoDigital.AbrirArquivoPfx(null!, Senha));
        Assert.Throws<ArgumentException>(() => CertificadoDigital.AbrirArquivoPfx("", Senha));
        Assert.Throws<ArgumentNullException>(() => CertificadoDigital.AbrirArquivoPfx("a.pfx", null!));
        Assert.Throws<ArgumentNullException>(() => CertificadoDigital.LerTitular(null!));
        Assert.Throws<ArgumentNullException>(() => CertificadoDigital.Validade(null!, Inicio));
    }

    // Validade

    public static TheoryData<DateTimeOffset, ValidadeCertificado> Instantes => new()
    {
        { Inicio.AddSeconds(-1), ValidadeCertificado.AindaNaoVigente },
        { Inicio, ValidadeCertificado.Vigente },
        { new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.FromHours(-3)), ValidadeCertificado.Vigente },
        { Fim, ValidadeCertificado.Vigente },
        { Fim.AddSeconds(1), ValidadeCertificado.Vencido },
        // O mesmo instante em outro fuso: 31/12/2026 22:00 em Brasília é 01/01/2027 01:00 UTC, já vencido.
        { new DateTimeOffset(2026, 12, 31, 22, 0, 0, TimeSpan.FromHours(-3)), ValidadeCertificado.Vencido },
        { new DateTimeOffset(2025, 12, 31, 22, 0, 0, TimeSpan.FromHours(-3)), ValidadeCertificado.Vigente },
    };

    [Theory]
    [MemberData(nameof(Instantes))]
    public void ConfereAValidadeNoInstante(DateTimeOffset instante, ValidadeCertificado esperado)
    {
        using var certificado = new CertificadoDeTeste { Cnpj = CnpjNumerico, NaoAntes = Inicio, NaoDepois = Fim }.Gerar();
        Assert.Equal(esperado, CertificadoDigital.Validade(certificado, instante));
    }

    [Fact]
    public void ValidadeDoPfxAberto()
    {
        var pfx = new CertificadoDeTeste { Cpf = CpfTitular, NaoAntes = Inicio, NaoDepois = Fim }.GerarPfx(Senha);
        using var certificado = CertificadoDigital.AbrirPfx(pfx, Senha);

        Assert.Equal(ValidadeCertificado.Vencido, CertificadoDigital.Validade(certificado, Fim.AddDays(1)));
        Assert.Equal(ValidadeCertificado.AindaNaoVigente, CertificadoDigital.Validade(certificado, Inicio.AddDays(-1)));
        Assert.Equal(ValidadeCertificado.Vigente, CertificadoDigital.Validade(certificado, Inicio.AddDays(1)));
    }

    // Titular

    [Theory]
    [InlineData(CnpjNumerico, CodificacaoOtherName.OctetString)]
    [InlineData(CnpjNumerico, CodificacaoOtherName.PrintableString)]
    [InlineData(CnpjAlfanumerico, CodificacaoOtherName.OctetString)]
    [InlineData(CnpjAlfanumerico, CodificacaoOtherName.PrintableString)]
    public void LeOTitularCnpj(string cnpj, CodificacaoOtherName codificacao)
    {
        using var certificado = new CertificadoDeTeste
        {
            Nome = "EMPRESA DE TESTE LTDA",
            Cnpj = cnpj,
            CpfResponsavel = CpfResponsavel,
            Codificacao = codificacao,
        }.Gerar();

        var titular = CertificadoDigital.LerTitular(certificado);

        Assert.NotNull(titular);
        Assert.Equal(cnpj, titular.Cnpj);
        Assert.Null(titular.Cpf);   // o CPF do responsável (2.16.76.1.3.4) não é o titular
        Assert.Equal("EMPRESA DE TESTE LTDA", titular.Nome);
    }

    [Theory]
    [InlineData(CodificacaoOtherName.OctetString)]
    [InlineData(CodificacaoOtherName.PrintableString)]
    public void LeOTitularCpf(CodificacaoOtherName codificacao)
    {
        using var certificado = new CertificadoDeTeste
        {
            Nome = "FULANO DE TAL",
            Cpf = CpfTitular,
            Nascimento = new DateOnly(1975, 12, 31),
            Codificacao = codificacao,
        }.Gerar();

        var titular = CertificadoDigital.LerTitular(certificado);

        Assert.NotNull(titular);
        Assert.Equal(CpfTitular, titular.Cpf);
        Assert.Null(titular.Cnpj);
        Assert.Equal("FULANO DE TAL", titular.Nome);
    }

    [Fact]
    public void LeOTitularDoPfxAberto()
    {
        var pfx = new CertificadoDeTeste { Nome = "LOJA ALFA", Cnpj = CnpjAlfanumerico }.GerarPfx(Senha);
        using var certificado = CertificadoDigital.AbrirPfx(pfx, Senha);

        var titular = CertificadoDigital.LerTitular(certificado);

        Assert.Equal(CnpjAlfanumerico, titular?.Cnpj);
        Assert.Equal("LOJA ALFA", titular?.Nome);
    }

    [Theory]
    [InlineData(CnpjNumerico, null)]
    [InlineData(CnpjAlfanumerico, null)]
    [InlineData(null, CpfTitular)]
    public void LeOTitularPeloNumeroDeSerie(string? cnpj, string? cpf)
    {
        // Perfil da versão 8.2 do DOC-ICP-04 em diante, depois de 2028: sem o otherName, o documento no
        // serialNumber e o CN só com o nome.
        using var certificado = new CertificadoDeTeste
        {
            Nome = "TITULAR NOVO",
            Cnpj = cnpj,
            Cpf = cpf,
            ComOtherName = false,
            DocumentoNoNomeComum = false,
            DocumentoNoNumeroDeSerie = true,
        }.Gerar();

        var titular = CertificadoDigital.LerTitular(certificado);

        Assert.NotNull(titular);
        Assert.Equal(cnpj, titular.Cnpj);
        Assert.Equal(cpf, titular.Cpf);
        Assert.Equal("TITULAR NOVO", titular.Nome);
    }

    [Fact]
    public void ONomeComumSemODocumentoFicaInteiro()
    {
        using var certificado = new CertificadoDeTeste
        {
            Nome = "EMPRESA:FILIAL",
            Cnpj = CnpjNumerico,
            DocumentoNoNomeComum = false,
        }.Gerar();

        Assert.Equal("EMPRESA:FILIAL", CertificadoDigital.LerTitular(certificado)?.Nome);
    }

    [Fact]
    public void SemOsCamposDaIcpBrasilNaoHaTitular()
    {
        // Decidido: sem CNPJ nem CPF, LerTitular devolve null (não é erro: o certificado pode ser lido).
        using var certificado = new CertificadoDeTeste
        {
            Nome = "SERVIDOR QUALQUER",
            Cnpj = CnpjNumerico,
            ComOtherName = false,
            DocumentoNoNomeComum = false,
        }.Gerar();

        Assert.Null(CertificadoDigital.LerTitular(certificado));
    }

    [Theory]
    [InlineData("00000000000000", null)]   // não disponível: preenchido com zeros (DOC-ICP-04 7.1.2.2 b)
    [InlineData("11222333000182", null)]   // dígito errado
    [InlineData("1122233300018", null)]    // 13 posições
    [InlineData("11.222.333/0001-81", null)]  // com máscara: o otherName não aceita
    [InlineData(null, "00000000000")]
    [InlineData(null, "52998224726")]
    public void CampoInvalidoNaoEOTitular(string? cnpj, string? cpf)
    {
        using var certificado = new CertificadoDeTeste
        {
            Cnpj = cnpj,
            Cpf = cpf,
            DocumentoNoNomeComum = false,
        }.Gerar();

        Assert.Null(CertificadoDigital.LerTitular(certificado));
    }

    [Fact]
    public void OOtherNameValeMaisQueONumeroDeSerie()
    {
        // Na transição (até 31/12/2028) os dois vêm juntos e iguais; se divergirem, vale o otherName.
        using var certificado = new CertificadoDeTeste
        {
            Cnpj = CnpjAlfanumerico,
            DocumentoNoNumeroDeSerie = true,
        }.Gerar();

        Assert.Equal(CnpjAlfanumerico, CertificadoDigital.LerTitular(certificado)?.Cnpj);
    }

    [Fact]
    public void LeOTitularDeUmCertificadoSemChavePrivada()
    {
        // O certificado recebido num XML (1.4) não tem chave: a leitura do titular não depende dela.
        using var completo = new CertificadoDeTeste { Cnpj = CnpjNumerico }.Gerar();
        using var publico = X509CertificateLoader.LoadCertificate(completo.RawData);

        Assert.False(publico.HasPrivateKey);
        Assert.Equal(CnpjNumerico, CertificadoDigital.LerTitular(publico)?.Cnpj);
    }
}
