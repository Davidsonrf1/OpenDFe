using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A distribuição DF-e (etapa 8.1) pelo ExecutarAsync com o leiaute de verdade e um handler que guarda o pedido e responde o
// retDistDFeInt de cada caso, com o gzip montado no teste: a montagem (os três modos, o schema, o AN e as URLs, o
// envelope com a operação), as recusas dos argumentos e da chave, e a leitura (os quatro tipos de documento, os resumos
// tipados, a paginação, o uso indevido e o docZip que não se lê). Contra a SEFAZ emulada, em DistribuicaoEmuladaTestes.
public class OperacaoDistribuicaoTestes
{
    private const string NamespaceNFe = "http://www.portalfiscal.inf.br/nfe";
    private const string NamespaceWsdl = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe";
    private const string Cnpj = "11444777000161";
    private const string Cpf = "52998224725";

    private static readonly Lazy<X509Certificate2> Certificado = new(() => new CertificadoTeste { Cnpj = Cnpj }.Gerar());

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ChaveAcesso Chave(int numero, Documento modelo = Documento.NFe) =>
        ChaveAcesso.Montar(UF.MG, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.FromHours(-3)), "11222333000181", modelo, 1, numero,
            TipoEmissao.Normal, 12345678);

    // O que o handler recebeu.
    private sealed record Pedido(Uri Url, string? Tipo, string Corpo)
    {
        public XElement Dados => XDocument.Parse(Corpo).Descendants(XName.Get("distDFeInt", NamespaceNFe)).Single();
    }

    // Os documentos do docZip

    private static string Gzip(string xml) => Gzip(new UTF8Encoding(false).GetBytes(xml));

    private static string Gzip(byte[] bytes)
    {
        using var saida = new MemoryStream();
        using (var gzip = new GZipStream(saida, CompressionLevel.SmallestSize, leaveOpen: true))
            gzip.Write(bytes, 0, bytes.Length);
        return Convert.ToBase64String(saida.ToArray());
    }

    private static string DocZip(long? nsu, string schema, string xml) => DocZipBase64(nsu, schema, Gzip(xml));

    private static string DocZipBase64(long? nsu, string schema, string base64) =>
        $"<docZip{(nsu is { } n ? $" NSU=\"{n:D15}\"" : "")} schema=\"{schema}\">{base64}</docZip>";

    private static string ResNFe(ChaveAcesso chave, string emitente = "11222333000181", string ie = "0623079040081", string situacao = "1",
        string? digest = "AAAAAAAAAAAAAAAAAAAAAAAAAAA=") =>
        $"<resNFe xmlns=\"{NamespaceNFe}\" versao=\"1.01\"><chNFe>{chave}</chNFe>"
        + (emitente.Length == 11 ? $"<CPF>{emitente}</CPF>" : $"<CNPJ>{emitente}</CNPJ>")
        + $"<xNome>EMITENTE DE TESTE LTDA</xNome><IE>{ie}</IE><dhEmi>2026-10-22T09:59:00-03:00</dhEmi><tpNF>1</tpNF><vNF>1500.50</vNF>"
        + (digest is null ? "" : $"<digVal>{digest}</digVal>")
        + $"<dhRecbto>2026-10-22T10:00:05-03:00</dhRecbto><nProt>131260000000001</nProt><cSitNFe>{situacao}</cSitNFe></resNFe>";

    private static string ResEvento(ChaveAcesso chave, string tipo = "110111", string descricao = "Cancelamento homologado") =>
        $"<resEvento xmlns=\"{NamespaceNFe}\" versao=\"1.01\"><cOrgao>31</cOrgao><CNPJ>11222333000181</CNPJ><chNFe>{chave}</chNFe>"
        + $"<dhEvento>2026-10-22T11:00:00-03:00</dhEvento><tpEvento>{tipo}</tpEvento><nSeqEvento>2</nSeqEvento><xEvento>{descricao}</xEvento>"
        + "<dhRecbto>2026-10-22T11:00:03-03:00</dhRecbto><nProt>131260000000002</nProt></resEvento>";

    private static string NfeProc(ChaveAcesso chave) =>
        $"<nfeProc xmlns=\"{NamespaceNFe}\" versao=\"4.00\"><NFe><infNFe Id=\"NFe{chave}\" versao=\"4.00\"/></NFe>"
        + $"<protNFe versao=\"4.00\"><infProt><chNFe>{chave}</chNFe><cStat>100</cStat></infProt></protNFe></nfeProc>";

    private static string ProcEvento(ChaveAcesso chave) =>
        $"<procEventoNFe xmlns=\"{NamespaceNFe}\" versao=\"1.00\"><evento versao=\"1.00\"><infEvento Id=\"ID110111{chave}01\"><cOrgao>31</cOrgao>"
        + $"<chNFe>{chave}</chNFe></infEvento></evento><retEvento versao=\"1.00\"><infEvento><chNFe>{chave}</chNFe></infEvento></retEvento></procEventoNFe>";

    private static string Retorno(int status, string motivo, string? ultimo, string? maximo, params string[] documentos) =>
        $"<retDistDFeInt xmlns=\"{NamespaceNFe}\" versao=\"1.01\"><tpAmb>2</tpAmb><verAplic>1.4.0</verAplic><cStat>{status}</cStat>"
        + $"<xMotivo>{motivo}</xMotivo><dhResp>2026-10-22T12:00:00-03:00</dhResp>"
        + (ultimo is null ? "" : $"<ultNSU>{ultimo}</ultNSU>") + (maximo is null ? "" : $"<maxNSU>{maximo}</maxNSU>")
        + (documentos.Length == 0 ? "" : "<loteDistDFeInt>" + string.Concat(documentos) + "</loteDistDFeInt>") + "</retDistDFeInt>";

    private static string Nsu(long nsu) => nsu.ToString("D15", CultureInfo.InvariantCulture);

    // A montagem

    [Theory]
    [InlineData(Documento.NFe, true)]
    [InlineData(Documento.NFCe, false)]
    public void ASoANFeTemADistribuicao(Documento documento, bool suporta)
    {
        using var conexoes = new ConexoesSefaz();

        var dfe = DFe.Criar(new LeiauteNFe400(), documento, Configuracao(conexoes));

        Assert.Equal(suporta, dfe.Suporta<DistribuicaoDFe>());
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento);
        Assert.Equal(suporta, declaracao.Servicos.Any(s => s.Nome == "NFeDistribuicaoDFe"));
        Assert.Equal(suporta, declaracao.Enderecos.Any(e => e.Servico == "NFeDistribuicaoDFe"));
    }

    [Fact]
    public void OServicoESuasUrlsSaoOsDoAmbienteNacional()
    {
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), Documento.NFe);

        var servico = Assert.Single(declaracao.Servicos, s => s.Nome == "NFeDistribuicaoDFe");
        Assert.Equal(NamespaceWsdl, servico.NamespaceWsdl);
        Assert.Equal(NamespaceWsdl + "/nfeDistDFeInteresse", servico.Acao);
        Assert.Equal("nfeDistDFeInteresse", servico.Operacao);
        Assert.All(declaracao.Servicos.Where(s => s.Nome != "NFeDistribuicaoDFe"), s => Assert.Null(s.Operacao));
        var enderecos = declaracao.Enderecos.Where(e => e.Servico == "NFeDistribuicaoDFe").ToList();
        Assert.Equal(2, enderecos.Count);
        Assert.All(enderecos, e => Assert.Equal(Destino.AN, e.Autorizador));
        Assert.Equal("https://hom1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx",
            Assert.Single(enderecos, e => e.Ambiente == Ambiente.Homologacao).Url.ToString());
        Assert.Equal("https://www1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx",
            Assert.Single(enderecos, e => e.Ambiente == Ambiente.Producao).Url.ToString());
    }

    [Theory]
    [InlineData(Ambiente.Homologacao, "hom1.nfe.fazenda.gov.br", "2")]
    [InlineData(Ambiente.Producao, "www1.nfe.fazenda.gov.br", "1")]
    public async Task OPedidoVaiAoANDosDoisAmbientesQualquerQueSejaAUFDoNucleo(Ambiente ambiente, string host, string tpAmb)
    {
        var (resultado, pedido) = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(0)), DistribuicaoDFe.UltimoNsu(Cnpj),
            ambiente: ambiente, uf: UF.SC);

        Assert.True(resultado.Processado);
        Assert.Equal(Destino.AN, resultado.Rastro.Autorizador);
        Assert.Equal(host, pedido.Url.Host);
        Assert.Equal("/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx", pedido.Url.AbsolutePath);
        Assert.Equal(tpAmb, (string?)pedido.Dados.Element(XName.Get("tpAmb", NamespaceNFe)));
    }

    [Fact]
    public async Task OEnvelopeLevaOPedidoDentroDaOperacaoEAAcaoDoServico()
    {
        var (resultado, pedido) = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(0)), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal("application/soap+xml; charset=utf-8; action=\"" + NamespaceWsdl + "/nfeDistDFeInteresse\"", pedido.Tipo);
        var corpo = XDocument.Parse(pedido.Corpo).Root!.Element(XName.Get("Body", "http://www.w3.org/2003/05/soap-envelope"))!;
        var operacao = Assert.Single(corpo.Elements());
        Assert.Equal(XName.Get("nfeDistDFeInteresse", NamespaceWsdl), operacao.Name);
        var dados = Assert.Single(operacao.Elements());
        Assert.Equal(XName.Get("nfeDadosMsg", NamespaceWsdl), dados.Name);
        Assert.Equal(resultado.Rastro.XmlPedido, Assert.Single(dados.Elements()).ToString(SaveOptions.DisableFormatting));
    }

    [Fact]
    public async Task OUltimoNsuVaiComQuinzeAlgarismosECUFAutorDoNucleo()
    {
        var (resultado, pedido) = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(0)), DistribuicaoDFe.UltimoNsu(Cnpj, 123), uf: UF.BA);

        var xml = resultado.Rastro.XmlPedido!;
        Assert.Empty(SchemasNFe400.DistribuicaoPedido.Validar(xml));
        Assert.Equal(
            $"<distDFeInt versao=\"1.01\" xmlns=\"{NamespaceNFe}\"><tpAmb>2</tpAmb><cUFAutor>29</cUFAutor><CNPJ>{Cnpj}</CNPJ>"
            + "<distNSU><ultNSU>000000000000123</ultNSU></distNSU></distDFeInt>",
            xml);
        Assert.DoesNotContain("Signature", xml, StringComparison.Ordinal);
        Assert.Empty(resultado.Problemas);
        Assert.NotNull(pedido);
    }

    [Fact]
    public async Task OPrimeiroAcessoVaiComUltNsuZeradoEAUFDoAutorInformadaSobrepoeAdoNucleo()
    {
        var consulta = DistribuicaoDFe.UltimoNsu(Cnpj, ufAutor: UF.SP);

        var (resultado, _) = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(0)), consulta, uf: UF.MG);

        var xml = resultado.Rastro.XmlPedido!;
        Assert.Empty(SchemasNFe400.DistribuicaoPedido.Validar(xml));
        Assert.Contains("<cUFAutor>35</cUFAutor>", xml, StringComparison.Ordinal);
        Assert.Contains("<ultNSU>000000000000000</ultNSU>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OConsNsuVaiComONsuDeQuinzeAlgarismos()
    {
        var (resultado, _) = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(7), Nsu(9)), DistribuicaoDFe.Nsu(Cnpj, 7));

        var xml = resultado.Rastro.XmlPedido!;
        Assert.Empty(SchemasNFe400.DistribuicaoPedido.Validar(xml));
        Assert.Contains("<consNSU><NSU>000000000000007</NSU></consNSU>", xml, StringComparison.Ordinal);
        Assert.DoesNotContain("distNSU", xml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OConsChNFeVaiComAChave()
    {
        var chave = Chave(5);

        var (resultado, _) = await Executar(Retorno(217, "Rejeicao: NF-e inexistente para a chave de acesso informada", Nsu(0), Nsu(0)),
            DistribuicaoDFe.Chave(Cnpj, chave));

        var xml = resultado.Rastro.XmlPedido!;
        Assert.Empty(SchemasNFe400.DistribuicaoPedido.Validar(xml));
        Assert.Contains($"<consChNFe><chNFe>{chave}</chNFe></consChNFe>", xml, StringComparison.Ordinal);
        Assert.Equal(217, resultado.CStat);
    }

    public static TheoryData<string, string, string> Interessados
    {
        get
        {
            var alfa = "12ABC34501DE" + OpenDFe.Cnpj.CalcularDigitos("12ABC34501DE");
            return new()
            {
                { Cnpj, "CNPJ", Cnpj },
                { "11.444.777/0001-61", "CNPJ", Cnpj },
                { alfa, "CNPJ", alfa },
                { OpenDFe.Cnpj.Formatar(alfa), "CNPJ", alfa },
                { Cpf, "CPF", Cpf },
                { "529.982.247-25", "CPF", Cpf },
            };
        }
    }

    [Theory]
    [MemberData(nameof(Interessados))]
    public async Task OInteressadoVaiSemMascaraComoCnpjOuCpf(string interessado, string elemento, string esperado)
    {
        var consulta = DistribuicaoDFe.UltimoNsu(interessado);

        var (resultado, _) = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(0)), consulta);

        Assert.Equal(esperado, consulta.Interessado);
        var xml = resultado.Rastro.XmlPedido!;
        Assert.Empty(SchemasNFe400.DistribuicaoPedido.Validar(xml));
        Assert.Contains($"<{elemento}>{esperado}</{elemento}>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void OsFactoriesGuardamOQueFoiPedido()
    {
        var chave = Chave(1);

        var ultimo = DistribuicaoDFe.UltimoNsu(Cnpj, 42);
        var nsu = DistribuicaoDFe.Nsu(Cnpj, 7);
        var porChave = DistribuicaoDFe.Chave(Cnpj, chave);

        Assert.Equal((TipoConsultaDistribuicao.UltimoNsu, (long?)42), (ultimo.Consulta, ultimo.NsuConsultado));
        Assert.Null(ultimo.ChaveConsultada);
        Assert.Equal((TipoConsultaDistribuicao.Nsu, (long?)7), (nsu.Consulta, nsu.NsuConsultado));
        Assert.Null(nsu.ChaveConsultada);
        Assert.Equal(TipoConsultaDistribuicao.Chave, porChave.Consulta);
        Assert.Null(porChave.NsuConsultado);
        Assert.Equal(chave, porChave.ChaveConsultada);
        Assert.Equal(0, DistribuicaoDFe.UltimoNsu(Cnpj).NsuConsultado);
        Assert.Null(ultimo.UFAutor);
        Assert.Equal(999_999_999_999_999, DistribuicaoDFe.UltimoNsu(Cnpj, 999_999_999_999_999).NsuConsultado);
    }

    // As recusas dos argumentos

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("11444777000162")]
    [InlineData("11444777000160")]
    [InlineData("00000000000000")]
    [InlineData("52998224726")]
    [InlineData("00000000000")]
    [InlineData("1144477700016")]
    [InlineData("11444777000161X")]
    [InlineData("12abc34501de35")]
    public void OInteressadoQueNaoECnpjNemCpfEArgumentException(string interessado)
    {
        Assert.Throws<ArgumentException>(() => DistribuicaoDFe.UltimoNsu(interessado));
        Assert.Throws<ArgumentException>(() => DistribuicaoDFe.Nsu(interessado, 1));
        Assert.Throws<ArgumentException>(() => DistribuicaoDFe.Chave(interessado, Chave(1)));
    }

    [Fact]
    public void ONuloEmUmArgumentoEArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DistribuicaoDFe.UltimoNsu(null!));
        Assert.Throws<ArgumentNullException>(() => DistribuicaoDFe.Chave(Cnpj, null!));
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(1_000_000_000_000_000L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void ONsuNegativoOuDeMaisDeQuinzeAlgarismosEArgumentException(long nsu)
    {
        Assert.ThrowsAny<ArgumentException>(() => DistribuicaoDFe.UltimoNsu(Cnpj, nsu));
        Assert.ThrowsAny<ArgumentException>(() => DistribuicaoDFe.Nsu(Cnpj, nsu));
    }

    [Fact]
    public void AMensagemDaRecusaNaoTrazOValorDoInteressado()
    {
        var erro = Assert.Throws<ArgumentException>(() => DistribuicaoDFe.UltimoNsu("12345678901"));

        Assert.DoesNotContain("12345678901", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AUFAutorDesconhecidaEArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DistribuicaoDFe.UltimoNsu(Cnpj, 0, (UF)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => DistribuicaoDFe.Nsu(Cnpj, 1, (UF)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => DistribuicaoDFe.Chave(Cnpj, Chave(1), (UF)99));
    }

    [Fact]
    public async Task AChaveDaNFCeEhRecusadaAntesDeTransmitir()
    {
        var enviados = new List<Pedido>();
        using var conexoes = Conexoes("", enviados);
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));

        var resultado = await dfe.ExecutarAsync(DistribuicaoDFe.Chave(Cnpj, Chave(1, Documento.NFCe)), Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Empty(enviados);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-CHAVE", Severidade.Erro), (problema.Codigo, problema.Severidade));
        Assert.Equal("distDFeInt/consChNFe/chNFe", problema.Caminho);
    }

    // A leitura: os documentos

    [Fact]
    public async Task LeOsQuatroTiposDeDocumentoDescompactadosComSeusNsu()
    {
        var (a, b, c, d) = (Chave(1), Chave(2), Chave(3), Chave(4));
        var retorno = Retorno(138, "Documento(s) localizado(s)", Nsu(4), Nsu(4),
            DocZip(1, "resNFe_v1.01.xsd", ResNFe(a)), DocZip(2, "procNFe_v4.00.xsd", NfeProc(b)),
            DocZip(3, "resEvento_v1.01.xsd", ResEvento(c)), DocZip(4, "procEventoNFe_v1.00.xsd", ProcEvento(d)));

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.True(resultado.Processado);
        Assert.Equal(138, resultado.CStat);
        Assert.Empty(resultado.Problemas);
        var documentos = resultado.Retorno.Documentos;
        Assert.Equal([1L, 2L, 3L, 4L], documentos.Select(x => x.Nsu));
        Assert.Equal([TipoDocumentoDistribuido.ResumoNFe, TipoDocumentoDistribuido.NFeProcessada, TipoDocumentoDistribuido.ResumoEvento,
            TipoDocumentoDistribuido.EventoProcessado], documentos.Select(x => x.Tipo));
        Assert.Equal(["resNFe_v1.01.xsd", "procNFe_v4.00.xsd", "resEvento_v1.01.xsd", "procEventoNFe_v1.00.xsd"], documentos.Select(x => x.Schema));
        Assert.Equal([a, b, c, d], documentos.Select(x => x.Chave));
        Assert.Equal(ResNFe(a), documentos[0].Xml);
        Assert.Equal(NfeProc(b), documentos[1].Xml);
        Assert.Equal(ResEvento(c), documentos[2].Xml);
        Assert.Equal(ProcEvento(d), documentos[3].Xml);
        Assert.NotNull(documentos[0].ResumoNFe);
        Assert.Null(documentos[0].ResumoEvento);
        Assert.NotNull(documentos[2].ResumoEvento);
        Assert.Null(documentos[2].ResumoNFe);
        Assert.Null(documentos[1].ResumoNFe);
        Assert.Null(documentos[3].ResumoEvento);
    }

    [Fact]
    public void OsXmlDeExemploPassamNosSchemasDoAmbienteNacional()
    {
        var chave = Chave(1);
        var resumo = SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso("resNFe_v1.01.xsd"), "215");
        var evento = SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso("resEvento_v1.01.xsd"), "215");
        var retorno = SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso("retDistDFeInt_v1.01.xsd"), "215");

        Assert.Empty(resumo.Validar(ResNFe(chave)));
        Assert.Empty(resumo.Validar(ResNFe(chave, emitente: Cpf, ie: "ISENTO", situacao: "3")));
        Assert.Empty(evento.Validar(ResEvento(chave)));
        Assert.Empty(retorno.Validar(Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1), DocZip(1, "resNFe_v1.01.xsd", ResNFe(chave)))));
    }

    [Fact]
    public async Task LeOResumoDaNFeComTodosOsCamposTipados()
    {
        var chave = Chave(1);

        var (resultado, _) = await Executar(Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1), DocZip(1, "resNFe_v1.01.xsd", ResNFe(chave))),
            DistribuicaoDFe.UltimoNsu(Cnpj));

        var resumo = Assert.Single(resultado.Retorno!.Documentos).ResumoNFe!;
        Assert.Equal(chave, resumo.Chave);
        Assert.Equal("11222333000181", resumo.Emitente);
        Assert.Equal("EMITENTE DE TESTE LTDA", resumo.NomeEmitente);
        Assert.Equal("0623079040081", resumo.InscricaoEstadual);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 9, 59, 0, TimeSpan.FromHours(-3)), resumo.Emissao);
        Assert.Equal(TimeSpan.FromHours(-3), resumo.Emissao.Offset);
        Assert.Equal(TipoNota.Saida, resumo.TipoNota);
        Assert.Equal(1500.50m, resumo.ValorTotal);
        Assert.Equal("AAAAAAAAAAAAAAAAAAAAAAAAAAA=", resumo.Digest);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 10, 0, 5, TimeSpan.FromHours(-3)), resumo.Recebimento);
        Assert.Equal("131260000000001", resumo.Protocolo);
        Assert.Equal(SituacaoResumo.Autorizada, resumo.Situacao);
    }

    [Theory]
    [InlineData("ISENTO", "ISENTO")]
    [InlineData("", "")]
    public async Task AIeIsentoOuVaziaChegaComoVeio(string ie, string esperado)
    {
        var (resultado, _) = await Executar(
            Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1), DocZip(1, "resNFe_v1.01.xsd", ResNFe(Chave(1), ie: ie))), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal(esperado, Assert.Single(resultado.Retorno!.Documentos).ResumoNFe!.InscricaoEstadual);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task OResumoDoEmitenteCpfSemDigestECanceladaELido()
    {
        var (resultado, _) = await Executar(
            Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1),
                DocZip(1, "resNFe_v1.01.xsd", ResNFe(Chave(1), emitente: Cpf, situacao: "3", digest: null)),
                DocZip(2, "resNFe_v1.01.xsd", ResNFe(Chave(2), situacao: "2")),
                DocZip(3, "resNFe_v1.01.xsd", ResNFe(Chave(3), situacao: "9"))),
            DistribuicaoDFe.UltimoNsu(Cnpj));

        var resumos = resultado.Retorno!.Documentos.Select(d => d.ResumoNFe!).ToList();
        Assert.Equal(Cpf, resumos[0].Emitente);
        Assert.Null(resumos[0].Digest);
        Assert.Equal([SituacaoResumo.Cancelada, SituacaoResumo.Denegada, (SituacaoResumo)9], resumos.Select(r => r.Situacao));
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task LeOResumoDoEventoComTodosOsCamposTipados()
    {
        var chave = Chave(3);

        var (resultado, _) = await Executar(
            Retorno(138, "Documento(s) localizado(s)", Nsu(3), Nsu(3),
                DocZip(3, "resEvento_v1.01.xsd", ResEvento(chave)),
                DocZip(4, "resEvento_v1.01.xsd", ResEvento(chave, tipo: "210210", descricao: "Ciencia da Operacao")),
                DocZip(5, "resEvento_v1.01.xsd", ResEvento(chave, tipo: "999999", descricao: "Evento novo"))),
            DistribuicaoDFe.UltimoNsu(Cnpj));

        var resumo = resultado.Retorno!.Documentos[0].ResumoEvento!;
        Assert.Equal(31, resumo.Orgao);
        Assert.Equal("11222333000181", resumo.Autor);
        Assert.Equal(chave, resumo.Chave);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 11, 0, 0, TimeSpan.FromHours(-3)), resumo.Momento);
        Assert.Equal(TipoEvento.Cancelamento, resumo.Tipo);
        Assert.Equal(2, resumo.Sequencia);
        Assert.Equal("Cancelamento homologado", resumo.Descricao);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 11, 0, 3, TimeSpan.FromHours(-3)), resumo.Recebimento);
        Assert.Equal("131260000000002", resumo.Protocolo);
        Assert.Equal(TipoEvento.CienciaOperacao, resultado.Retorno.Documentos[1].ResumoEvento!.Tipo);
        Assert.Equal((TipoEvento)999999, resultado.Retorno.Documentos[2].ResumoEvento!.Tipo);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task OSchemaDesconhecidoViraOutroComOXmlEPassaSemAviso()
    {
        const string Xml = "<resCTe xmlns=\"http://www.portalfiscal.inf.br/cte\"><x>1</x></resCTe>";

        var (resultado, _) = await Executar(Retorno(138, "Documento(s) localizado(s)", Nsu(9), Nsu(9), DocZip(9, "resCTe_v1.00.xsd", Xml)),
            DistribuicaoDFe.UltimoNsu(Cnpj));

        var documento = Assert.Single(resultado.Retorno!.Documentos);
        Assert.Equal(TipoDocumentoDistribuido.Outro, documento.Tipo);
        Assert.Equal("resCTe_v1.00.xsd", documento.Schema);
        Assert.Equal(Xml, documento.Xml);
        Assert.Null(documento.Chave);
        Assert.Null(documento.ResumoNFe);
        Assert.Null(documento.ResumoEvento);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task ADistribuicaoPorChaveVemSemNsuNoDocZip()
    {
        var chave = Chave(1);

        var (resultado, _) = await Executar(Retorno(138, "Documento localizado", Nsu(0), Nsu(8), DocZip(null, "procNFe_v4.00.xsd", NfeProc(chave))),
            DistribuicaoDFe.Chave(Cnpj, chave));

        var documento = Assert.Single(resultado.Retorno!.Documentos);
        Assert.Null(documento.Nsu);
        Assert.Equal(chave, documento.Chave);
        Assert.Equal(TipoDocumentoDistribuido.NFeProcessada, documento.Tipo);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task OXmlComBomEComDeclaracaoVemComoVeioSemOBom()
    {
        var chave = Chave(1);
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + ResNFe(chave);
        var comBom = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(xml)).ToArray();

        var (resultado, _) = await Executar(
            Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1), DocZipBase64(1, "resNFe_v1.01.xsd", Gzip(comBom))), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal(xml, Assert.Single(resultado.Retorno!.Documentos).Xml);
        Assert.NotNull(resultado.Retorno.Documentos[0].ResumoNFe);
    }

    [Fact]
    public async Task OBase64ComQuebrasDeLinhaComoNaNtSeLe()
    {
        var chave = Chave(1);
        var base64 = Gzip(ResNFe(chave));
        var comQuebras = string.Join("\n                        ", Enumerable.Range(0, (base64.Length + 59) / 60).Select(i => base64.Substring(i * 60, Math.Min(60, base64.Length - (i * 60)))));

        var (resultado, _) = await Executar(
            Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1), DocZipBase64(1, "resNFe_v1.01.xsd", "\n" + comQuebras + "\n")), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal(ResNFe(chave), Assert.Single(resultado.Retorno!.Documentos).Xml);
        Assert.Empty(resultado.Problemas);
    }

    // A leitura: o status e o NSU

    [Fact]
    public async Task O137NaoTemDocumentoESugereEsperarUmaHora()
    {
        var (resultado, _) = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(200), Nsu(200)), DistribuicaoDFe.UltimoNsu(Cnpj, 200));

        var retorno = resultado.Retorno!;
        Assert.Equal(137, resultado.CStat);
        Assert.Equal("Nenhum documento localizado", resultado.Motivo);
        Assert.True(retorno.NenhumDocumento);
        Assert.False(retorno.ConsumoIndevido);
        Assert.False(retorno.TemMais);
        Assert.Equal(TimeSpan.FromHours(1), retorno.EsperaSugerida);
        Assert.Empty(retorno.Documentos);
        Assert.Equal((200L, 200L), (retorno.UltimoNsu, retorno.MaximoNsu));
        Assert.Equal(Ambiente.Homologacao, retorno.Ambiente);
        Assert.Equal("1.4.0", retorno.VersaoAplicativo);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 12, 0, 0, TimeSpan.FromHours(-3)), retorno.Resposta);
        Assert.Empty(resultado.Problemas);
    }

    [Theory]
    [InlineData(100L, 150L, false)]
    [InlineData(150L, 150L, true)]
    [InlineData(150L, 100L, true)]
    public async Task O138TemMaisSoQuandoOUltimoNsuEMenorQueOMaximoEEntaoNaoHaMaisAEsperarUmaHora(long ultimo, long maximo, bool semMais)
    {
        var retorno = Retorno(138, "Documento(s) localizado(s)", Nsu(ultimo), Nsu(maximo), DocZip(ultimo, "resNFe_v1.01.xsd", ResNFe(Chave(1))));

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj, 50));

        Assert.Equal(!semMais, resultado.Retorno!.TemMais);
        // NT 3.11.4.1, 2: com o ultNSU igual ao maxNSU não há mais documentos, e a hora de espera evita o bloqueio.
        Assert.Equal(semMais ? TimeSpan.FromHours(1) : null, resultado.Retorno.EsperaSugerida);
        Assert.False(resultado.Retorno.NenhumDocumento);
    }

    [Fact]
    public async Task O138SemOUltNsuOuOMaxNsuLegiveisNaoTemMaisNemEspera()
    {
        var retorno = Retorno(138, "Documento(s) localizado(s)", null, Nsu(5), DocZip(1, "resNFe_v1.01.xsd", ResNFe(Chave(1))));

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.False(resultado.Retorno!.TemMais);
        Assert.Null(resultado.Retorno.EsperaSugerida);
    }

    [Fact]
    public async Task AsConsultasPontuaisNaoTemMaisNemEsperaNoCasoDoUltimoNsuMenorQueOMaximo()
    {
        var porNsu = await Executar(Retorno(138, "Documento localizado", Nsu(7), Nsu(900), DocZip(7, "resNFe_v1.01.xsd", ResNFe(Chave(1)))),
            DistribuicaoDFe.Nsu(Cnpj, 7));
        var naoExiste = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(8), Nsu(900)), DistribuicaoDFe.Nsu(Cnpj, 8));
        var porChave = await Executar(Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(900)), DistribuicaoDFe.Chave(Cnpj, Chave(1)));

        Assert.False(porNsu.Resultado.Retorno!.TemMais);
        Assert.False(naoExiste.Resultado.Retorno!.TemMais);
        Assert.Null(naoExiste.Resultado.Retorno.EsperaSugerida);
        Assert.True(naoExiste.Resultado.Retorno.NenhumDocumento);
        Assert.Null(porChave.Resultado.Retorno!.EsperaSugerida);
    }

    [Fact]
    public async Task O656EOConsumoIndevidoComOUltimoNsuDaUltimaConsultaEUmaHoraDeEspera()
    {
        const string Motivo = "Rejeicao: Consumo Indevido. Deve ser utilizado o ultNSU nas solicitacoes subsequentes. Tente apos 1 hora";

        var (resultado, _) = await Executar(Retorno(656, Motivo, Nsu(150), Nsu(300)), DistribuicaoDFe.UltimoNsu(Cnpj, 10));

        var retorno = resultado.Retorno!;
        Assert.Equal(656, resultado.CStat);
        Assert.Equal(Motivo, resultado.Motivo);
        Assert.True(retorno.ConsumoIndevido);
        Assert.False(retorno.TemMais);
        Assert.False(retorno.NenhumDocumento);
        Assert.Equal(TimeSpan.FromHours(1), retorno.EsperaSugerida);
        Assert.Equal(150L, retorno.UltimoNsu);
        Assert.Empty(retorno.Documentos);
    }

    [Theory]
    [InlineData(589, "Rejeicao: Numero do NSU informado superior ao maior NSU da base de dados do Ambiente Nacional")]
    [InlineData(593, "Rejeicao: CNPJ-Base consultado difere do CNPJ-Base do Certificado Digital")]
    [InlineData(215, "Rejeicao: Falha no schema XML")]
    [InlineData(632, "Rejeicao: Solicitacao fora de prazo, a NF-e nao esta mais disponivel para download")]
    public async Task AsRejeicoesSaoProcessadasSemEsperaNemDocumento(int status, string motivo)
    {
        var (resultado, _) = await Executar(Retorno(status, motivo, Nsu(0), Nsu(0)), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal(SituacaoResultado.Processado, resultado.Situacao);
        Assert.Equal(status, resultado.Retorno!.Status);
        Assert.False(resultado.Retorno.TemMais);
        Assert.Null(resultado.Retorno.EsperaSugerida);
        Assert.False(resultado.Retorno.ConsumoIndevido);
        Assert.Empty(resultado.Retorno.Documentos);
    }

    [Fact]
    public async Task OUltNsuEOMaxNsuQueNaoSeLeemFicamNulosComAviso()
    {
        var (resultado, _) = await Executar(Retorno(137, "Nenhum documento localizado", "abc", "1234567890123456"), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Null(resultado.Retorno!.UltimoNsu);
        Assert.Null(resultado.Retorno.MaximoNsu);
        Assert.False(resultado.Retorno.TemMais);
        Assert.Equal(["ultNSU", "maxNSU"], resultado.Problemas.Select(p => p.Tag));
        Assert.All(resultado.Problemas, p =>
        {
            Assert.Equal(("NFE-VALOR", Severidade.Aviso), (p.Codigo, p.Severidade));
            Assert.Equal("retDistDFeInt/" + p.Tag, p.Caminho);
        });
    }

    [Fact]
    public async Task OUltNsuEOMaxNsuAusentesFicamNulosSemAviso()
    {
        var (resultado, _) = await Executar(Retorno(137, "Nenhum documento localizado", null, null), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Null(resultado.Retorno!.UltimoNsu);
        Assert.Null(resultado.Retorno.MaximoNsu);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task OCampoDesconhecidoEORepetidoSaoAvisos()
    {
        var retorno = $"<retDistDFeInt xmlns=\"{NamespaceNFe}\" versao=\"1.01\"><tpAmb>2</tpAmb><verAplic>1.4.0</verAplic><cStat>137</cStat>"
            + $"<xMotivo>Nenhum documento localizado</xMotivo><cStat>138</cStat><ultNSU>{Nsu(0)}</ultNSU><maxNSU>{Nsu(0)}</maxNSU><novo>1</novo></retDistDFeInt>";

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal(137, resultado.CStat);
        Assert.Equal([("NFE-REPETIDO", "cStat"), ("NFE-DESCONHECIDO", "novo")], resultado.Problemas.Select(p => (p.Codigo, p.Tag!)));
        Assert.All(resultado.Problemas, p => Assert.Equal(Severidade.Aviso, p.Severidade));
    }

    [Fact]
    public async Task ACincoentaEUmDocumentosAceitaOQueVier()
    {
        var documentos = Enumerable.Range(1, 55).Select(i => DocZip(i, "resNFe_v1.01.xsd", ResNFe(Chave(i)))).ToArray();

        var (resultado, _) = await Executar(Retorno(138, "Documento(s) localizado(s)", Nsu(55), Nsu(120), documentos), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal(55, resultado.Retorno!.Documentos.Count);
        Assert.Equal(Enumerable.Range(1, 55).Select(i => (long?)i), resultado.Retorno.Documentos.Select(d => d.Nsu));
        Assert.True(resultado.Retorno.TemMais);
        Assert.Empty(resultado.Problemas);
    }

    // A leitura: o docZip que não se lê

    private static string Aviso(IEnumerable<Problema> problemas, string trecho)
    {
        var aviso = Assert.Single(problemas, p => p.Codigo == "NFE-DISTRIBUICAO");
        Assert.Equal(Severidade.Aviso, aviso.Severidade);
        Assert.Equal("docZip", aviso.Tag);
        Assert.Equal("retDistDFeInt/loteDistDFeInt/docZip", aviso.Caminho);
        Assert.Contains(trecho, aviso.Mensagem, StringComparison.Ordinal);
        return aviso.Mensagem;
    }

    public static TheoryData<string, string> DocumentosIlegiveis => new()
    {
        { "base64 inválido", "o conteúdo não é base64" },
        { "não é gzip", "o conteúdo não é um gzip válido" },
        { "gzip cortado", "" },
        { "gzip corrompido", "o conteúdo não é um gzip válido" },
        { "xml mal formado", "o XML descompactado não é bem formado" },
        { "vazio", "o XML descompactado não é bem formado" },
        { "dtd", "o XML descompactado não é bem formado" },
        { "não é UTF-8", "o conteúdo não é UTF-8" },
    };

    [Theory]
    [MemberData(nameof(DocumentosIlegiveis))]
    public async Task ODocZipQueNaoSeLeSaiDaListaComAvisoEOsOutrosSeguem(string caso, string trecho)
    {
        var bom = Gzip(ResNFe(Chave(2)));
        var base64 = caso switch
        {
            "base64 inválido" => "@@@@ isto nao e base64 @@@@",
            "não é gzip" => Convert.ToBase64String(Encoding.UTF8.GetBytes("<resNFe/>")),
            "gzip cortado" => bom[..(bom.Length / 2 / 4 * 4)],
            "gzip corrompido" => CorromperGzip(Gzip(ResNFe(Chave(1)))),
            "xml mal formado" => Gzip("<resNFe><aberto>"),
            "vazio" => "",
            "dtd" => Gzip("<!DOCTYPE a [<!ENTITY e \"x\">]><a>&e;</a>"),
            "não é UTF-8" => Gzip([0x3C, 0x61, 0x3E, 0xFF, 0xFE, 0x3C, 0x2F, 0x61, 0x3E]),
            _ => throw new ArgumentOutOfRangeException(nameof(caso)),
        };
        var retorno = Retorno(138, "Documento(s) localizado(s)", Nsu(3), Nsu(3),
            DocZipBase64(1, "resNFe_v1.01.xsd", base64), DocZip(2, "resNFe_v1.01.xsd", ResNFe(Chave(2))), DocZip(3, "resNFe_v1.01.xsd", ResNFe(Chave(3))));

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.True(resultado.Processado);
        Assert.Equal(138, resultado.CStat);
        Assert.Equal([2L, 3L], resultado.Retorno!.Documentos.Select(d => d.Nsu));
        var mensagem = Aviso(resultado.Problemas, trecho);
        Assert.Contains("NSU 1", mensagem, StringComparison.Ordinal);
        Assert.Single(resultado.Problemas);
    }

    private static string CorromperGzip(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        for (var i = 12; i < bytes.Length - 8; i++)
            bytes[i] ^= 0xFF;
        return Convert.ToBase64String(bytes);
    }

    [Fact]
    public async Task ABombaDeDescompressaoEOmitidaComAvisoEOutrosSeguem()
    {
        var enorme = "<a>" + new string('x', 17 * 1024 * 1024) + "</a>";
        var retorno = Retorno(138, "Documento(s) localizado(s)", Nsu(2), Nsu(2),
            DocZip(1, "resNFe_v1.01.xsd", enorme), DocZip(2, "resNFe_v1.01.xsd", ResNFe(Chave(2))));

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal([2L], resultado.Retorno!.Documentos.Select(d => d.Nsu));
        Aviso(resultado.Problemas, "o conteúdo descompactado passa de 16 MB");
    }

    [Fact]
    public async Task UmDocumentoGrandeAbaixoDoLimiteSeLe()
    {
        var grande = "<a>" + new string('x', 4 * 1024 * 1024) + "</a>";

        var (resultado, _) = await Executar(
            Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1), DocZip(1, "resXYZ_v1.00.xsd", grande)), DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Equal(grande, Assert.Single(resultado.Retorno!.Documentos).Xml);
        Assert.Empty(resultado.Problemas);
    }

    [Theory]
    [InlineData("<resNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><chNFe>x</chNFe></resNFe>", "resNFe")]
    [InlineData("<resEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><cOrgao>31</cOrgao></resEvento>", "resEvento")]
    public async Task OResumoQueNaoSeLeNosTiposFicaNuloComAvisoESegueNoXml(string xml, string resumo)
    {
        var schema = resumo == "resNFe" ? "resNFe_v1.01.xsd" : "resEvento_v1.01.xsd";

        var (resultado, _) = await Executar(Retorno(138, "Documento(s) localizado(s)", Nsu(1), Nsu(1), DocZip(1, schema, xml)), DistribuicaoDFe.UltimoNsu(Cnpj));

        var documento = Assert.Single(resultado.Retorno!.Documentos);
        Assert.Null(documento.ResumoNFe);
        Assert.Null(documento.ResumoEvento);
        Assert.Equal(xml, documento.Xml);
        Aviso(resultado.Problemas, $"O {resumo} (NSU 1) não se lê nos tipos");
    }

    [Fact]
    public async Task ONsuIlegivelNoDocZipFicaNuloComAvisoEODocumentoSegue()
    {
        var retorno = $"<retDistDFeInt xmlns=\"{NamespaceNFe}\" versao=\"1.01\"><tpAmb>2</tpAmb><verAplic>1</verAplic><cStat>138</cStat><xMotivo>Documento(s) localizado(s)</xMotivo>"
            + $"<dhResp>2026-10-22T12:00:00-03:00</dhResp><ultNSU>{Nsu(1)}</ultNSU><maxNSU>{Nsu(1)}</maxNSU><loteDistDFeInt>"
            + $"<docZip NSU=\"um\" schema=\"resNFe_v1.01.xsd\">{Gzip(ResNFe(Chave(1)))}</docZip>"
            + $"<docZip NSU=\"{Nsu(2)}\">{Gzip(ResNFe(Chave(2)))}</docZip><outro/></loteDistDFeInt></retDistDFeInt>";

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj));

        var documentos = resultado.Retorno!.Documentos;
        Assert.Equal(2, documentos.Count);
        Assert.Null(documentos[0].Nsu);
        Assert.Equal(TipoDocumentoDistribuido.ResumoNFe, documentos[0].Tipo);
        Assert.Equal(2L, documentos[1].Nsu);
        Assert.Equal((TipoDocumentoDistribuido.Outro, ""), (documentos[1].Tipo, documentos[1].Schema));
        Assert.Equal([("NFE-VALOR", "docZip/@NSU"), ("NFE-DISTRIBUICAO", "docZip"), ("NFE-DESCONHECIDO", "outro")],
            resultado.Problemas.Select(p => (p.Codigo, p.Tag!)));
    }

    [Theory]
    [InlineData("<retConsSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><cStat>137</cStat><xMotivo>x</xMotivo></retConsSitNFe>")]
    [InlineData("<retDistDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><xMotivo>x</xMotivo></retDistDFeInt>")]
    [InlineData("<retDistDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><cStat>cento</cStat><xMotivo>x</xMotivo></retDistDFeInt>")]
    [InlineData("<retDistDFeInt xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><cStat>137</cStat></retDistDFeInt>")]
    [InlineData("<retDistDFeInt versao=\"1.01\"><cStat>137</cStat><xMotivo>x</xMotivo></retDistDFeInt>")]
    public async Task ARespostaSemOCStatOuOMotivoEIlegivel(string retorno)
    {
        var enviados = new List<Pedido>();
        using var conexoes = Conexoes(retorno, enviados);
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(DistribuicaoDFe.UltimoNsu(Cnpj), Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.IsType<FormatException>(falha.InnerException);
        Assert.NotNull(falha.Rastro.XmlRetorno);
        Assert.Single(enviados);
    }

    [Fact]
    public async Task ARespostaSemOElementoDaOperacaoEIlegivel()
    {
        var enviados = new List<Pedido>();
        using var conexoes = new ConexoesSefaz(_ => new HandlerRetorno("", enviados, RespostaDireta));
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(DistribuicaoDFe.UltimoNsu(Cnpj), Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
    }

    [Fact]
    public async Task AAmbienteDoRetornoQueNaoSeLeFicaNuloComAviso()
    {
        var retorno = Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(0)).Replace("<tpAmb>2</tpAmb>", "<tpAmb>3</tpAmb>", StringComparison.Ordinal)
            .Replace("2026-10-22T12:00:00-03:00", "ontem", StringComparison.Ordinal);

        var (resultado, _) = await Executar(retorno, DistribuicaoDFe.UltimoNsu(Cnpj));

        Assert.Null(resultado.Retorno!.Ambiente);
        Assert.Null(resultado.Retorno.Resposta);
        Assert.Equal(["tpAmb", "dhResp"], resultado.Problemas.Select(p => p.Tag));
    }

    // O handler

    private const string RespostaDireta = "sem-operacao";

    private static async Task<(ResultadoWS<RetornoDistribuicao> Resultado, Pedido Pedido)> Executar(string retorno, DistribuicaoDFe consulta,
        Ambiente ambiente = Ambiente.Homologacao, UF uf = UF.MG)
    {
        var enviados = new List<Pedido>();
        using var conexoes = Conexoes(retorno, enviados);
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes, ambiente, uf));

        var resultado = await dfe.ExecutarAsync(consulta, Ct);

        return (resultado, Assert.Single(enviados));
    }

    private static ConfiguracaoDFe Configuracao(ConexoesSefaz conexoes, Ambiente ambiente = Ambiente.Homologacao, UF uf = UF.MG) => new()
    {
        Ambiente = ambiente,
        UF = uf,
        Certificado = Certificado.Value,
        Conexoes = conexoes,
    };

    private static ConexoesSefaz Conexoes(string retorno, List<Pedido> enviados) => new(_ => new HandlerRetorno(retorno, enviados));

    // Responde o retorno dentro de nfeDistDFeInteresseResponse/nfeDistDFeInteresseResult (ou, com RespostaDireta, no nfeResultMsg direto).
    private sealed class HandlerRetorno(string retorno, List<Pedido> enviados, string? modo = null) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corpo = await request.Content!.ReadAsStringAsync(cancellationToken);
            lock (enviados)
                enviados.Add(new Pedido(request.RequestUri!, request.Content.Headers.ContentType?.ToString(), corpo));
            var envelope = modo == RespostaDireta
                ? "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><nfeResultMsg xmlns=\"" + NamespaceWsdl + "\">"
                    + Retorno(137, "Nenhum documento localizado", Nsu(0), Nsu(0)) + "</nfeResultMsg></soap:Body></soap:Envelope>"
                : "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>"
                    + "<nfeDistDFeInteresseResponse xmlns=\"" + NamespaceWsdl + "\"><nfeDistDFeInteresseResult>" + retorno
                    + "</nfeDistDFeInteresseResult></nfeDistDFeInteresseResponse></soap:Body></soap:Envelope>";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(envelope, Encoding.UTF8, "application/soap+xml") };
        }
    }
}
