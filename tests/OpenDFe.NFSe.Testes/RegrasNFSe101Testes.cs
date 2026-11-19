using System.Text.RegularExpressions;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// As regras fora do schema da DPS (docs/NFSE.md 4, REGRAS.md 20): cada uma acusa o seu caso, com o caminho e a
// propriedade, e fica quieta na DPS certa; o catálogo cruza com o código.
public class RegrasNFSe101Testes
{
    private static readonly DateTimeOffset Agora = DpsTeste.Emissao.AddMinutes(1);

    private static List<Problema> Conferir(Dps dps) => RegrasNFSe101.Conferir(dps, Agora);

    [Fact]
    public void ADPSCertaDoPrestadorNaoTemProblema()
    {
        Assert.Empty(Conferir(DpsTeste.Minima()));
    }

    [Theory]
    [InlineData("prest", "Prestador.Cnpj")]
    [InlineData("toma", "Tomador.Cpf")]
    [InlineData("interm", "Intermediario.Cnpj")]
    public void ODocumentoInvalidoENFSEDOCUMENTO(string grupo, string propriedade)
    {
        var dps = DpsTeste.Minima();
        dps.Prestador.Cpf = null;
        dps.Prestador.Cnpj = grupo == "prest" ? "11222333000180" : DpsTeste.CnpjPrestador;
        if (grupo == "toma")
            dps.Tomador = new Pessoa { Cpf = "52998224724", Nome = "X" };
        if (grupo == "interm")
            dps.Intermediario = new Pessoa { Cnpj = "11444777000160", Nome = "X" };

        var problema = Assert.Single(Conferir(dps));

        Assert.Equal(("NFSE-DOCUMENTO", Severidade.Erro, propriedade), (problema.Codigo, problema.Severidade, problema.Propriedade));
        Assert.StartsWith($"DPS/infDPS/{grupo}/", problema.Caminho, StringComparison.Ordinal);
    }

    [Fact]
    public void OCNPJAlfanumericoValidoPassa()
    {
        var dps = DpsTeste.Minima();
        dps.Tomador = new Pessoa { Cnpj = "12ABC34501DE35", Nome = "X" };

        Assert.Empty(Conferir(dps));
    }

    [Fact]
    public void ACompetenciaDepoisDaEmissaoENFSECOMPETENCIA()
    {
        var dps = DpsTeste.Minima();
        dps.Competencia = new DateOnly(2026, 11, 20);

        var problema = Assert.Single(Conferir(dps));

        Assert.Equal(("NFSE-COMPETENCIA", "Competencia", "DPS/infDPS/dCompet"), (problema.Codigo, problema.Propriedade, problema.Caminho));
    }

    [Fact]
    public void ACompetenciaNoMesmoDiaOuAntesPassa()
    {
        var dps = DpsTeste.Minima();
        dps.Competencia = new DateOnly(2026, 10, 1);

        Assert.Empty(Conferir(dps));
    }

    [Fact]
    public void AEmissaoNoFuturoAlemDaToleranciaENFSEEMISSAO()
    {
        var dps = DpsTeste.Minima();
        var dentro = RegrasNFSe101.Conferir(dps, DpsTeste.Emissao - RegrasNFSe101.ToleranciaRelogio);
        var fora = RegrasNFSe101.Conferir(dps, DpsTeste.Emissao - RegrasNFSe101.ToleranciaRelogio - TimeSpan.FromSeconds(1));

        Assert.Empty(dentro);
        Assert.Equal(("NFSE-EMISSAO", "Emissao"), (Assert.Single(fora).Codigo, fora[0].Propriedade));
    }

    [Theory]
    [InlineData(EmitenteDps.Tomador)]
    [InlineData(EmitenteDps.Intermediario)]
    public void QuemNaoEOPrestadorENFSEEMITENTE(EmitenteDps emitente)
    {
        var dps = DpsTeste.Minima();
        dps.Emitente = emitente;

        var problema = Assert.Single(Conferir(dps));

        Assert.Equal(("NFSE-EMITENTE", "Emitente", "DPS/infDPS/tpEmit"), (problema.Codigo, problema.Propriedade, problema.Caminho));
    }

    [Fact]
    public void OPrestadorQueEmiteComNomeEnderecoNIFOuMotivoENFSEPRESTADOR()
    {
        var dps = DpsTeste.Minima();
        dps.Prestador.Nome = "X";
        dps.Prestador.Endereco = new Endereco();
        dps.Prestador.Nif = "1";
        dps.Prestador.MotivoAusenciaNif = MotivoAusenciaNif.Dispensado;

        var problemas = Conferir(dps);

        Assert.All(problemas, p => Assert.Equal("NFSE-PRESTADOR", p.Codigo));
        Assert.Equal(["Prestador.Nome", "Prestador.Endereco", "Prestador.Nif", "Prestador.MotivoAusenciaNif"], problemas.Select(p => p.Propriedade));
    }

    [Fact]
    public void ONomeDoPrestadorNaDPSDoTomadorNaoENFSEPRESTADOR()
    {
        var problemas = Conferir(DpsTeste.CompletaB());

        Assert.DoesNotContain(problemas, p => p.Codigo == "NFSE-PRESTADOR");
    }

    // O caminho do núcleo (a função declarada): o modelo, ou, sem ele, a DPS lida do XML; o relógio da configuração.
    [Fact]
    public void ONucleoChamaAsRegrasPeloModeloEPeloXML()
    {
        var dps = DpsTeste.Minima();
        dps.Emitente = EmitenteDps.Tomador;
        var xml = LeiauteNFSe101.Instancia.Escrever(dps).Xml;
        using var conexoes = new ConexoesSefaz();
        var configuracao = new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = NFSeTeste.Certificado,
            Conexoes = conexoes,
            Relogio = new RelogioFixo(Agora),
        };
        var contexto = Interno<ContextoOperacao>(Documento.NFSe, configuracao);

        var peloModelo = RegrasNFSe101.Conferir(Interno<DocumentoVerificado>(Documento.NFSe, dps, xml, null), contexto);
        var peloXml = RegrasNFSe101.Conferir(Interno<DocumentoVerificado>(Documento.NFSe, null, xml, null), contexto);
        var evento = RegrasNFSe101.Conferir(Interno<DocumentoVerificado>(Documento.NFSe, new CancelamentoNFSe(), xml, null), contexto);

        Assert.Equal(["NFSE-EMITENTE"], peloModelo.Select(p => p.Codigo));
        Assert.Equal(["NFSE-EMITENTE"], peloXml.Select(p => p.Codigo));
        Assert.Empty(evento);
    }

    // Cada regra da lista está no catálogo (REGRAS.md 20), e cada linha do catálogo da NFS-e é uma regra da lista.
    [Fact]
    public void OCatalogoEOCodigoTemAsMesmasRegras()
    {
        var texto = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "docs", "REGRAS.md"));
        var secao = texto[texto.IndexOf("## 20.", StringComparison.Ordinal)..];
        var catalogo = Regex.Matches(secao, @"^\| `(NFSE-[A-Z-]+)` \|", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToList();

        Assert.Equal(RegrasNFSe101.Lista.Select(r => r.Codigo), catalogo.Where(c => c != "NFSE-CHAVE"));
        Assert.Contains("NFSE-CHAVE", catalogo);
    }

    // Os construtores internos do núcleo (o núcleo os cria no caminho do ExecutarAsync).
    private static T Interno<T>(params object?[] argumentos) =>
        (T)Activator.CreateInstance(typeof(T), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, argumentos, null)!;

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora.ToUniversalTime();
    }
}
