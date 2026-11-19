using System.Reflection;
using System.Xml.Linq;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// A escrita da DPS (etapa 9.1, docs/NFSE.md 7): cada DPS de teste sai válida no XSD 1.01 e volta igual pela leitura; o Id,
// os valores do núcleo (M4), as escolhas (M7), os campos da 1.00 (M10) e os problemas da montagem.
public class EscritaDpsTestes
{
    private static readonly XNamespace Ns = "http://www.sped.fazenda.gov.br/nfse";

    public static TheoryData<string> Variantes => new(DpsTeste.Todas().Select(v => v.Nome));

    private static Dps Criar(string nome) => DpsTeste.Todas().First(v => v.Nome == nome).Criar();

    [Theory]
    [MemberData(nameof(Variantes))]
    public void ADPSDeTesteSaiValidaNoXSDESemProblema(string variante)
    {
        var escrita = LeiauteNFSe101.Instancia.Escrever(Criar(variante));

        Assert.Empty(escrita.Problemas);
        Assert.Empty(SchemasNFSe101.Dps.Validar(escrita.Xml));
    }

    [Theory]
    [MemberData(nameof(Variantes))]
    public void ADPSEscritaVoltaIgualPelaLeitura(string variante)
    {
        var dps = Criar(variante);
        var escrita = LeiauteNFSe101.Instancia.Escrever(dps);

        var lida = LeiauteNFSe101.Instancia.Ler(escrita.Xml);

        Assert.Empty(lida.Problemas);
        Assert.Equal("1.01", lida.Versao);
        Assert.Null(lida.Nota);
        Assert.Equal(escrita.Id, lida.IdentificadorDps);
        Assert.Empty(ComparacaoNFSe.Diferencas(dps, lida.Dps!));
    }

    // Juntas, as variantes preenchem cada propriedade do modelo da DPS (as da 1.00 e as da NFS-e ficam de fora).
    [Fact]
    public void AsVariantesCobremTodasAsPropriedadesDaDPS()
    {
        var comparadas = new HashSet<(Type, string)>();
        foreach (var (_, criar) in DpsTeste.Todas())
        {
            var dps = criar();
            var lida = LeiauteNFSe101.Instancia.Ler(LeiauteNFSe101.Instancia.Escrever(dps).Xml).Dps!;
            ComparacaoNFSe.Diferencas(dps, lida, comparadas);
        }

        var faltam = PropriedadesDps().Where(p => !comparadas.Contains(p)).Select(p => $"{p.Item1.Name}.{p.Item2}").ToList();

        Assert.Empty(faltam);
    }

    [Fact]
    public void OIdSaiDosCamposDaDPSComQuemEmite()
    {
        var a = LeiauteNFSe101.Instancia.Escrever(DpsTeste.CompletaA());
        var b = LeiauteNFSe101.Instancia.Escrever(DpsTeste.CompletaB());
        var c = LeiauteNFSe101.Instancia.Escrever(DpsTeste.CompletaC());

        Assert.Equal("DPS" + "3106200" + "2" + DpsTeste.CnpjPrestador + "00001" + "000000000000042", a.Id!.Value.Valor);
        Assert.Equal("DPS" + "3106200" + "2" + DpsTeste.CnpjTomador + "00001" + "000000000000043", b.Id!.Value.Valor);
        Assert.Equal("DPS" + "3106200" + "1" + "000" + DpsTeste.Cpf + "00001" + "000000000000044", c.Id!.Value.Valor);
        Assert.Equal(a.Id.Value.Valor, (string?)XElement.Parse(a.Xml).Element(Ns + "infDPS")!.Attribute("Id"));
    }

    [Fact]
    public void OXMLSaiSemPrefixoSemDeclaracaoSemEspacosEComAVersao()
    {
        var xml = LeiauteNFSe101.Instancia.Escrever(DpsTeste.Minima()).Xml;

        Assert.StartsWith("<DPS versao=\"1.01\" xmlns=\"http://www.sped.fazenda.gov.br/nfse\"><infDPS Id=\"DPS", xml, StringComparison.Ordinal);
        Assert.DoesNotContain("> <", xml, StringComparison.Ordinal);
        Assert.DoesNotContain("<?xml", xml, StringComparison.Ordinal);
        Assert.Contains("<vServ>1000.00</vServ>", xml, StringComparison.Ordinal);
        Assert.Contains("<dhEmi>2026-11-19T10:30:00-03:00</dhEmi>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OsCodigosComZerosSaemComOsDigitosDoCampo()
    {
        var xml = XElement.Parse(LeiauteNFSe101.Instancia.Escrever(DpsTeste.CompletaA()).Xml);

        Assert.Equal("99", xml.Descendants(Ns + "subst").Single().Element(Ns + "cMotivo")!.Value);
        Assert.Equal("01", xml.Descendants(Ns + "mecAFComexP").Single().Value);
        Assert.Equal("26", xml.Descendants(Ns + "mecAFComexT").Single().Value);
        Assert.Equal("01", xml.Descendants(Ns + "piscofins").Single().Element(Ns + "CST")!.Value);
        Assert.Equal("02", xml.Descendants(Ns + "tpReeRepRes").Single().Value);
    }

    [Fact]
    public void AEscritaNaoAlteraADPS()
    {
        var dps = DpsTeste.Minima();
        dps.Ambiente = null;
        dps.VersaoAplicativo = null;

        LeiauteNFSe101.Instancia.Escrever(dps);

        Assert.Null(dps.Ambiente);
        Assert.Null(dps.VersaoAplicativo);
    }

    [Fact]
    public void AVersaoDoAplicativoNulaValeAPadrao()
    {
        var dps = DpsTeste.Minima();
        dps.VersaoAplicativo = null;

        var xml = XElement.Parse(LeiauteNFSe101.Instancia.Escrever(dps).Xml);

        Assert.StartsWith("OpenDFe ", xml.Descendants(Ns + "verAplic").Single().Value, StringComparison.Ordinal);
        Assert.True(xml.Descendants(Ns + "verAplic").Single().Value.Length <= 20);
    }

    [Fact]
    public void OAmbienteDoNucleoValeQuandoADPSNaoOTrazEODiferenteEErro()
    {
        var dps = DpsTeste.Minima();
        dps.Ambiente = null;
        var doNucleo = EscritorNFSe.Escrever(dps, Ambiente.Producao);
        dps.Ambiente = Ambiente.Homologacao;
        var diferente = EscritorNFSe.Escrever(dps, Ambiente.Producao);

        Assert.Empty(doNucleo.Problemas);
        Assert.Equal("1", XElement.Parse(doNucleo.Xml).Descendants(Ns + "tpAmb").Single().Value);
        var problema = Assert.Single(diferente.Problemas);
        Assert.Equal("NFSE-NUCLEO", problema.Codigo);
        Assert.Equal("Ambiente", problema.Propriedade);
        Assert.Equal("1", XElement.Parse(diferente.Xml).Descendants(Ns + "tpAmb").Single().Value);
    }

    [Fact]
    public void SemNucleoOAmbienteNuloNaoSaiEOSchemaAponta()
    {
        var dps = DpsTeste.Minima();
        dps.Ambiente = null;

        var escrita = LeiauteNFSe101.Instancia.Escrever(dps);

        Assert.DoesNotContain("<tpAmb>", escrita.Xml, StringComparison.Ordinal);
        Assert.NotEmpty(SchemasNFSe101.Dps.Validar(escrita.Xml));
    }

    [Fact]
    public void DuasAlternativasDeUmaEscolhaSaoErroComAPropriedadeDaSegunda()
    {
        var dps = DpsTeste.CompletaA();
        dps.Tomador!.Cnpj = DpsTeste.CnpjTomador;

        var problema = Assert.Single(LeiauteNFSe101.Instancia.Escrever(dps).Problemas);

        Assert.Equal("NFSE-EXCLUSIVO", problema.Codigo);
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.Equal("Tomador.Cpf", problema.Propriedade);
        Assert.Equal("DPS/infDPS/toma", problema.Caminho);
    }

    [Fact]
    public void OCampoSoDaVersao100PreenchidoEAvisoENaoSai()
    {
        var dps = DpsTeste.Minima();
        dps.Servico.ExploracaoRodoviaria = new ExploracaoRodoviaria { Placa = "ABC1234" };
        dps.Valores.Tributacao.Municipal.Beneficio = new BeneficioMunicipal { Identificador = "12345678901234", Tipo = TipoBeneficioLei.Isencao };

        var escrita = LeiauteNFSe101.Instancia.Escrever(dps);

        Assert.All(escrita.Problemas, p => Assert.Equal(("NFSE-NAO-SUPORTADO", Severidade.Aviso), (p.Codigo, p.Severidade)));
        Assert.Equal(["Servico.ExploracaoRodoviaria", "Valores.Tributacao.Municipal.Beneficio.Tipo"], escrita.Problemas.Select(p => p.Propriedade));
        Assert.DoesNotContain("explRod", escrita.Xml, StringComparison.Ordinal);
        Assert.DoesNotContain("tpBM", escrita.Xml, StringComparison.Ordinal);
        Assert.Empty(SchemasNFSe101.Dps.Validar(escrita.Xml));
    }

    [Theory]
    [InlineData("MunicipioEmissor")]
    [InlineData("Serie")]
    [InlineData("Numero")]
    [InlineData("Prestador")]
    public void SemUmDadoDoIdADPSSaiSemIdComOProblema(string falta)
    {
        var dps = DpsTeste.Minima();
        switch (falta)
        {
            case "MunicipioEmissor": dps.MunicipioEmissor = null; break;
            case "Serie": dps.Serie = null; break;
            case "Numero": dps.Numero = null; break;
            default: dps.Prestador.Cpf = null; break;
        }

        var escrita = LeiauteNFSe101.Instancia.Escrever(dps);

        Assert.Null(escrita.Id);
        var problema = Assert.Single(escrita.Problemas, p => p.Codigo == "NFSE-ID");
        Assert.StartsWith(falta, problema.Propriedade, StringComparison.Ordinal);
        Assert.DoesNotContain(" Id=", escrita.Xml, StringComparison.Ordinal);
    }

    [Fact]
    public void ODadoDoIdForaDaFaixaEProblemaComOCampo()
    {
        var dps = DpsTeste.Minima();
        dps.Serie = "123456";

        var problema = Assert.Single(LeiauteNFSe101.Instancia.Escrever(dps).Problemas);

        Assert.Equal(("NFSE-ID", "Serie", "DPS/infDPS/serie"), (problema.Codigo, problema.Propriedade, problema.Caminho));
    }

    [Fact]
    public void OCaractereQueOXMLNaoAceitaNaoSaiEEProblemaSemOValor()
    {
        var dps = DpsTeste.Minima();
        dps.Servico.Codigo.Descricao = "SERVI\u0001CO";

        var problema = Assert.Single(LeiauteNFSe101.Instancia.Escrever(dps).Problemas);

        Assert.Equal("NFSE-CARACTERE", problema.Codigo);
        Assert.Equal("Servico.Codigo.Descricao", problema.Propriedade);
        Assert.Equal("DPS/infDPS/serv/cServ/xDescServ", problema.Caminho);
        Assert.DoesNotContain("SERVI", problema.Mensagem, StringComparison.Ordinal);
    }

    // As propriedades [Tag] das classes que a DPS alcança, sem as da 1.00.
    internal static IEnumerable<(Type, string)> PropriedadesDps()
    {
        var vistos = new HashSet<Type>();
        var fila = new Queue<Type>([typeof(Dps)]);
        while (fila.Count > 0)
        {
            var tipo = fila.Dequeue();
            if (!vistos.Add(tipo))
                continue;
            foreach (var propriedade in tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (propriedade.GetCustomAttribute<TagAttribute>() is null
                    || propriedade.GetCustomAttribute<TagAttribute>()!.Tag is "lsadppu" or "explRod" or "tpBM")
                    continue;
                yield return (tipo, propriedade.Name);
                var alvo = propriedade.PropertyType.IsGenericType && propriedade.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)
                    ? propriedade.PropertyType.GetGenericArguments()[0]
                    : Nullable.GetUnderlyingType(propriedade.PropertyType) ?? propriedade.PropertyType;
                if (alvo.Assembly == typeof(Dps).Assembly && alvo.IsClass)
                    fila.Enqueue(alvo);
            }
        }
    }
}
