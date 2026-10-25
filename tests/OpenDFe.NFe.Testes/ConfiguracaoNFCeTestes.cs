using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A configuração da NFC-e (etapa 5.1, CONTRATOS.md 16.1): os dois construtores, o CSC e as URLs trocadas.
public class ConfiguracaoNFCeTestes
{
    private const string Codigo = "OPENDFE-CSC-DE-TESTE-0001";

    [Fact]
    public void OConstrutorVazioEAVersao3SemCsc()
    {
        var configuracao = new ConfiguracaoNFCe();

        Assert.Equal(VersaoQrCode.V3, configuracao.QrCode);
        Assert.Null(configuracao.Csc);
        Assert.Null(configuracao.UrlQrCode);
        Assert.Null(configuracao.UrlChave);
        Assert.IsAssignableFrom<ConfiguracaoDocumento>(configuracao);
    }

    [Fact]
    public void ComOCscEAVersao2()
    {
        var csc = new CodigoSeguranca(1, Codigo);

        var configuracao = new ConfiguracaoNFCe(csc);

        Assert.Equal(VersaoQrCode.V2, configuracao.QrCode);
        Assert.Same(csc, configuracao.Csc);
        Assert.Throws<ArgumentNullException>(() => new ConfiguracaoNFCe(null!));
    }

    [Fact]
    public void AsUrlsTrocadasSaemComoForamDadas()
    {
        var configuracao = new ConfiguracaoNFCe
        {
            UrlQrCode = new Uri("https://nova.exemplo.gov.br/nfce/qrcode"),
            UrlChave = new Uri("http://nova.exemplo.gov.br/nfce/consulta"),
        };

        Assert.Equal("https://nova.exemplo.gov.br/nfce/qrcode", configuracao.UrlQrCode!.OriginalString);
        Assert.Equal("http://nova.exemplo.gov.br/nfce/consulta", configuracao.UrlChave!.OriginalString);
    }

    [Theory]
    [InlineData("/nfce/qrcode")]
    [InlineData("ftp://exemplo.gov.br/nfce/qrcode")]
    [InlineData("https://exemplo.gov.br/nfce/qrcode?p=")]
    [InlineData("https://exemplo.gov.br/nfce/qrcode#topo")]
    public void AUrlDoQrCodeRecusaARelativaOutroEsquemaEAQuery(string url)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);

        Assert.Throws<ArgumentException>(() => new ConfiguracaoNFCe { UrlQrCode = uri });
    }

    [Theory]
    [InlineData("/nfce/consulta")]
    [InlineData("ftp://exemplo.gov.br/nfce/consulta")]
    public void AUrlDaChaveRecusaARelativaEOutroEsquema(string url)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);

        Assert.Throws<ArgumentException>(() => new ConfiguracaoNFCe { UrlChave = uri });
    }
}

// O CSC (Manual 6.0, 4.6): cada recusa, e o ToString que não mostra o código.
public class CodigoSegurancaTestes
{
    [Theory]
    [InlineData(1, "OPENDFE-CSC-DE-TESTE")]
    [InlineData(999_999, "0123456789ABCDEF")]
    [InlineData(2, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    [InlineData(3, "ab!#$%&()*+,-./:;<=>?@[]^_`{}~")]
    public void AceitaOQueOManualDeixa(int identificador, string codigo)
    {
        var csc = new CodigoSeguranca(identificador, codigo);

        Assert.Equal(identificador, csc.Identificador);
        Assert.Equal(codigo, csc.Codigo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1_000_000)]
    public void RecusaOIdentificadorForaDe1A999999(int identificador)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CodigoSeguranca(identificador, "OPENDFE-CSC-DE-TESTE"));
    }

    [Theory]
    [InlineData("0123456789ABCDE")]                      // 15 caracteres
    [InlineData("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0")] // 37 caracteres
    [InlineData("OPENDFE CSC DE TESTE")]                 // espaço
    [InlineData("OPENDFE|CSC|DE|TESTE")]                 // a barra vertical separa os parâmetros
    [InlineData("OPENDFE-CSC-DE-TESTÉ")]                 // acento
    [InlineData("OPENDFE-CSC-DE-TESTE\n")]               // controle
    public void RecusaOCodigoQueQuebraOQrCode(string codigo)
    {
        Assert.Throws<ArgumentException>(() => new CodigoSeguranca(1, codigo));
    }

    [Fact]
    public void RecusaOCodigoNulo()
    {
        Assert.Throws<ArgumentNullException>(() => new CodigoSeguranca(1, null!));
    }

    [Fact]
    public void OToStringNaoMostraOCodigo()
    {
        var csc = new CodigoSeguranca(12, "SEGREDO-DO-CONTRIBUINTE-123");

        Assert.Equal("CSC 12", csc.ToString());
        Assert.DoesNotContain("SEGREDO", csc.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("SEGREDO", $"{csc}", StringComparison.Ordinal);
    }

    [Fact]
    public void AMensagemDaRecusaNaoTrazOCodigo()
    {
        var erro = Assert.Throws<ArgumentException>(() => new CodigoSeguranca(1, "SEGREDO CURTO"));

        Assert.DoesNotContain("SEGREDO", erro.Message, StringComparison.Ordinal);
    }
}

// A tabela das URLs de consulta (docs/NTS.md 4): só as linhas conferidas em fonte primária, com a forma certa; as UFs
// que faltam estão na lista abaixo, para quebrar quando alguém completar a tabela sem atualizar a lista.
public class UrlsConsultaTestes
{
    // As UFs sem URL na tabela (nenhuma fonte primária conferida em 2026-10-24: docs/NTS.md 4.3).
    private static readonly UF[] UFsPendentes =
    [
        UF.AC, UF.AL, UF.AM, UF.AP, UF.BA, UF.CE, UF.DF, UF.ES, UF.GO, UF.MA, UF.MT, UF.PA, UF.PB, UF.PE, UF.PI,
        UF.PR, UF.RJ, UF.RN, UF.RO, UF.RR, UF.RS, UF.SE, UF.SP, UF.TO,
    ];

    private static IReadOnlyList<UrlConsultaNFCe> Tabela => new LeiauteNFe400().UrlsConsulta;

    [Fact]
    public void ATabelaTemAsUFsConferidasNosDoisAmbientesEAListaDasQueFaltamEstaCerta()
    {
        var conferidas = Enum.GetValues<UF>().Except(UFsPendentes).ToList();

        Assert.NotEmpty(conferidas);
        foreach (var uf in conferidas)
        {
            Assert.Contains(Tabela, l => l.UF == uf && l.Ambiente == Ambiente.Producao);
            Assert.Contains(Tabela, l => l.UF == uf && l.Ambiente == Ambiente.Homologacao);
        }
        Assert.Empty(Tabela.Select(l => l.UF).Intersect(UFsPendentes));
        Assert.Equal(Tabela.Count, Tabela.Select(l => (l.UF, l.Ambiente)).Distinct().Count());
    }

    [Fact]
    public void CadaLinhaTemUrlsAbsolutasOQrCodeSemQueryEOUrlChaveDe21A85Caracteres()
    {
        Assert.All(Tabela, l =>
        {
            Assert.True(l.QrCode.IsAbsoluteUri && l.QrCode.Scheme is "http" or "https", $"{l.UF} {l.Ambiente}: {l.QrCode}");
            Assert.True(l.Chave.IsAbsoluteUri && l.Chave.Scheme is "http" or "https", $"{l.UF} {l.Ambiente}: {l.Chave}");
            Assert.Empty(l.QrCode.Query);
            Assert.InRange(l.Chave.OriginalString.Length, 21, 85);
        });
    }

    [Fact]
    public void MGTemAUrlDaSefMG()
    {
        var producao = Tabela.Single(l => l.UF == UF.MG && l.Ambiente == Ambiente.Producao);
        var homologacao = Tabela.Single(l => l.UF == UF.MG && l.Ambiente == Ambiente.Homologacao);

        Assert.Equal("https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml", producao.QrCode.OriginalString);
        Assert.Equal("https://portalsped.fazenda.mg.gov.br/portalnfce", producao.Chave.OriginalString);
        Assert.Equal("https://hportalsped.fazenda.mg.gov.br/portalnfce", homologacao.Chave.OriginalString);
    }

    [Fact]
    public void ATabelaEhSomenteLeitura()
    {
        var lista = Assert.IsAssignableFrom<IList<UrlConsultaNFCe>>(Tabela);

        Assert.Throws<NotSupportedException>(() => lista.Clear());
    }
}
