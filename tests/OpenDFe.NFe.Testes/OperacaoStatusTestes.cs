using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O status do serviço (etapa 4.3) pelo ExecutarAsync com o leiaute de verdade e um handler que guarda o pedido e
// responde o retorno de cada caso: a montagem (o cUF, o tpAmb, o autorizador e o schema) e a leitura (os campos, os
// avisos e a resposta ilegível). Contra a SEFAZ emulada, em StatusEmuladoTestes.
public class OperacaoStatusTestes
{
    private const string NamespaceNFe = "http://www.portalfiscal.inf.br/nfe";

    private const string Operando =
        "<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><verAplic>MG-4.00</verAplic>"
        + "<cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo><cUF>31</cUF><dhRecbto>2026-10-22T10:00:00-03:00</dhRecbto>"
        + "<tMed>2</tMed></retConsStatServ>";

    private static readonly Lazy<X509Certificate2> Certificado = new(() => new CertificadoTeste { Cnpj = "11222333000181" }.Gerar());

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(Documento.NFe)]
    [InlineData(Documento.NFCe)]
    public void OLeiauteSuportaOStatusNosDoisDocumentos(Documento documento)
    {
        using var conexoes = new ConexoesSefaz();

        Assert.True(DFe.Criar(new LeiauteNFe400(), documento, Configuracao(conexoes, UF.MG)).Suporta<ConsultaStatus>());
    }

    public static TheoryData<Documento, UF, TipoEmissao, Destino?, Destino, int> Montagens => new()
    {
        // documento, UF do núcleo, tipo de emissão, destino do parâmetro, autorizador, cUF
        { Documento.NFe, UF.MG, TipoEmissao.Normal, null, Destino.MG, 31 },
        { Documento.NFe, UF.SC, TipoEmissao.Normal, null, Destino.SVRS, 42 },
        { Documento.NFe, UF.MA, TipoEmissao.Normal, null, Destino.SVAN, 21 },
        { Documento.NFe, UF.MG, TipoEmissao.ContingenciaSvcAN, null, Destino.SvcAN, 31 },
        { Documento.NFe, UF.PI, TipoEmissao.ContingenciaSvcRS, null, Destino.SvcRS, 22 },
        { Documento.NFe, UF.MG, TipoEmissao.Normal, Destino.SvcAN, Destino.SvcAN, 31 },
        { Documento.NFe, UF.MG, TipoEmissao.Normal, Destino.SP, Destino.SP, 35 },
        { Documento.NFe, UF.MG, TipoEmissao.Normal, Destino.SC, Destino.SVRS, 42 },
        { Documento.NFe, UF.MG, TipoEmissao.Normal, Destino.SVRS, Destino.SVRS, 31 },
        { Documento.NFCe, UF.MG, TipoEmissao.Normal, null, Destino.MG, 31 },
        { Documento.NFCe, UF.BA, TipoEmissao.Normal, null, Destino.SVRS, 29 },
        { Documento.NFCe, UF.MG, TipoEmissao.ContingenciaOffline, null, Destino.MG, 31 },
    };

    [Theory]
    [MemberData(nameof(Montagens))]
    public async Task OPedidoVaiAoAutorizadorComAUFConsultada(Documento documento, UF uf, TipoEmissao emissao, Destino? destino,
        Destino autorizador, int codigoUF)
    {
        var (resultado, url) = await Executar(Operando, documento, uf, emissao, new ConsultaStatus { Destino = destino });

        Assert.True(resultado.Processado);
        Assert.Equal(autorizador, resultado.Rastro.Autorizador);
        Assert.Equal(resultado.Rastro.Url, url);
        Assert.Equal(Assert.Single(DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento).Enderecos,
            e => e.Autorizador == autorizador && e.Ambiente == Ambiente.Homologacao && e.Servico == "NFeStatusServico4").Url, url);
        var pedido = XElement.Parse(resultado.Rastro.XmlPedido!);
        Assert.Equal(XName.Get("consStatServ", NamespaceNFe), pedido.Name);
        Assert.Equal("4.00", (string?)pedido.Attribute("versao"));
        Assert.Equal(["2", codigoUF.ToString(System.Globalization.CultureInfo.InvariantCulture), "STATUS"],
            pedido.Elements().Select(e => e.Value));
        Assert.Equal(["tpAmb", "cUF", "xServ"], pedido.Elements().Select(e => e.Name.LocalName));
    }

    [Fact]
    public async Task OPedidoPassaNoSchemaSemAssinaturaNemDeclaracao()
    {
        var (resultado, _) = await Executar(Operando);

        var pedido = resultado.Rastro.XmlPedido!;
        Assert.Empty(SchemasNFe400.Status.Validar(pedido));
        Assert.DoesNotContain("Signature", pedido, StringComparison.Ordinal);
        Assert.StartsWith("<consStatServ ", pedido, StringComparison.Ordinal);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task NaProducaoOTpAmbE1()
    {
        var (resultado, url) = await Executar(Operando, ambiente: Ambiente.Producao);

        Assert.Equal("1", XElement.Parse(resultado.Rastro.XmlPedido!).Element(XName.Get("tpAmb", NamespaceNFe))!.Value);
        Assert.Equal("nfe.fazenda.mg.gov.br", url.Host);
    }

    [Fact]
    public async Task OAmbienteNacionalNaoTemOStatus()
    {
        var enviados = new List<Uri>();
        using var conexoes = Conexoes(Operando, enviados);
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes, UF.MG));

        await Assert.ThrowsAsync<NotSupportedException>(() => dfe.ExecutarAsync(new ConsultaStatus { Destino = Destino.AN }, Ct));
        Assert.Empty(enviados);
    }

    [Fact]
    public async Task LeTodosOsCamposDoServicoEmOperacao()
    {
        var (resultado, _) = await Executar(Operando);

        Assert.True(resultado.Processado);
        Assert.Equal(107, resultado.CStat);
        Assert.Equal("Servico em Operacao", resultado.Motivo);
        Assert.Empty(resultado.Problemas);
        var retorno = resultado.Retorno;
        Assert.Equal(107, retorno.Status);
        Assert.Equal("Servico em Operacao", retorno.Motivo);
        Assert.True(retorno.Operando);
        Assert.Equal(Ambiente.Homologacao, retorno.Ambiente);
        Assert.Equal("MG-4.00", retorno.VersaoAplicativo);
        Assert.Equal(UF.MG, retorno.UF);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.FromHours(-3)), retorno.Recebimento);
        Assert.Equal(TimeSpan.FromSeconds(2), retorno.TempoMedio);
        Assert.Null(retorno.PrevisaoRetorno);
        Assert.Null(retorno.Observacao);
    }

    [Fact]
    public async Task LeOServicoParalisadoComAPrevisaoEAObservacao()
    {
        const string Retorno =
            "<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>1</tpAmb><verAplic>SVRS202610</verAplic>"
            + "<cStat>108</cStat><xMotivo>Servico Paralisado Momentaneamente (curto prazo)</xMotivo><cUF>42</cUF>"
            + "<dhRecbto>2026-10-22T13:00:00Z</dhRecbto><dhRetorno>2026-10-22T15:30:00-03:00</dhRetorno>"
            + "<xObs>Manutencao programada</xObs></retConsStatServ>";

        var (resultado, _) = await Executar(Retorno);

        Assert.True(resultado.Processado);
        Assert.Equal(108, resultado.CStat);
        var retorno = resultado.Retorno;
        Assert.False(retorno.Operando);
        Assert.Equal(Ambiente.Producao, retorno.Ambiente);
        Assert.Equal(UF.SC, retorno.UF);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 13, 0, 0, TimeSpan.Zero), retorno.Recebimento);
        Assert.Null(retorno.TempoMedio);
        Assert.Equal(new DateTimeOffset(2026, 10, 22, 15, 30, 0, TimeSpan.FromHours(-3)), retorno.PrevisaoRetorno);
        Assert.Equal("Manutencao programada", retorno.Observacao);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task ARejeicaoEProcessadaComOCStat()
    {
        const string Retorno =
            "<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><verAplic>MG</verAplic>"
            + "<cStat>289</cStat><xMotivo>Rejeicao: Codigo da UF informada diverge da UF solicitada</xMotivo><cUF>31</cUF>"
            + "<dhRecbto>2026-10-22T10:00:00-03:00</dhRecbto></retConsStatServ>";

        var (resultado, _) = await Executar(Retorno);

        Assert.Equal(SituacaoResultado.Processado, resultado.Situacao);
        Assert.Equal(289, resultado.CStat);
        Assert.False(resultado.Retorno!.Operando);
    }

    [Fact]
    public async Task SoOCStatEOMotivoBastam()
    {
        const string Retorno =
            "<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo></retConsStatServ>";

        var (resultado, _) = await Executar(Retorno);

        var retorno = resultado.Retorno!;
        Assert.True(retorno.Operando);
        Assert.Null(retorno.Ambiente);
        Assert.Null(retorno.VersaoAplicativo);
        Assert.Null(retorno.UF);
        Assert.Null(retorno.Recebimento);
        Assert.Null(retorno.TempoMedio);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task OValorQueNaoSeLeFicaNuloComAviso()
    {
        const string Retorno =
            "<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>3</tpAmb><verAplic>MG</verAplic>"
            + "<cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo><cUF>91</cUF><dhRecbto>22/10/2026 10:00</dhRecbto>"
            + "<tMed>dois</tMed><dhRetorno>2026-10-22T10:00:00</dhRetorno></retConsStatServ>";

        var (resultado, _) = await Executar(Retorno);

        Assert.True(resultado.Processado);
        var retorno = resultado.Retorno;
        Assert.Null(retorno.Ambiente);
        Assert.Null(retorno.UF);
        Assert.Null(retorno.Recebimento);
        Assert.Null(retorno.TempoMedio);
        Assert.Null(retorno.PrevisaoRetorno);
        Assert.Equal(["tpAmb", "cUF", "dhRecbto", "tMed", "dhRetorno"], resultado.Problemas.Select(p => p.Tag));
        Assert.All(resultado.Problemas, p =>
        {
            Assert.Equal(("NFE-VALOR", Severidade.Aviso), (p.Codigo, p.Severidade));
            Assert.Equal("retConsStatServ/" + p.Tag, p.Caminho);
        });
    }

    [Fact]
    public async Task OCampoDesconhecidoEORepetidoSaoAvisos()
    {
        const string Retorno =
            "<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><verAplic>MG</verAplic>"
            + "<cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo><cStat>108</cStat><cUF>31</cUF><novo>1</novo>"
            + "<tMed xmlns=\"urn:outro\">5</tMed></retConsStatServ>";

        var (resultado, _) = await Executar(Retorno);

        Assert.Equal(107, resultado.CStat);
        Assert.Null(resultado.Retorno!.TempoMedio);
        Assert.Equal([("NFE-REPETIDO", "cStat"), ("NFE-DESCONHECIDO", "novo"), ("NFE-DESCONHECIDO", "tMed")],
            resultado.Problemas.Select(p => (p.Codigo, p.Tag!)));
        Assert.All(resultado.Problemas, p => Assert.Equal(Severidade.Aviso, p.Severidade));
    }

    [Theory]
    [InlineData("<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><xMotivo>Servico em Operacao</xMotivo></retConsStatServ>")]
    [InlineData("<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><cStat>cento e sete</cStat><xMotivo>Servico em Operacao</xMotivo></retConsStatServ>")]
    [InlineData("<retConsStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><cStat>107</cStat></retConsStatServ>")]
    [InlineData("<retConsSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo></retConsSitNFe>")]
    [InlineData("<retConsStatServ versao=\"4.00\"><cStat>107</cStat><xMotivo>Servico em Operacao</xMotivo></retConsStatServ>")]
    public async Task ARespostaSemOCStatOuOMotivoEIlegivel(string retorno)
    {
        var enviados = new List<Uri>();
        using var conexoes = Conexoes(retorno, enviados);
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes, UF.MG));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new ConsultaStatus(), Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.IsType<FormatException>(falha.InnerException);
        Assert.NotNull(falha.Rastro.XmlRetorno);
        Assert.Single(enviados);
    }

    private static async Task<(ResultadoWS<RetornoStatus> Resultado, Uri Url)> Executar(string retorno, Documento documento = Documento.NFe,
        UF uf = UF.MG, TipoEmissao emissao = TipoEmissao.Normal, ConsultaStatus? parametro = null, Ambiente ambiente = Ambiente.Homologacao)
    {
        var enviados = new List<Uri>();
        using var conexoes = Conexoes(retorno, enviados);
        var dfe = DFe.Criar(new LeiauteNFe400(), documento, Configuracao(conexoes, uf, emissao, ambiente));

        var resultado = await dfe.ExecutarAsync(parametro ?? new ConsultaStatus(), Ct);

        return (resultado, Assert.Single(enviados));
    }

    private static ConfiguracaoDFe Configuracao(ConexoesSefaz conexoes, UF uf, TipoEmissao emissao = TipoEmissao.Normal,
        Ambiente ambiente = Ambiente.Homologacao) => new()
        {
            Ambiente = ambiente,
            UF = uf,
            Certificado = Certificado.Value,
            Conexoes = conexoes,
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(DateTimeOffset.UnixEpoch, "Teste do status do servico na contingencia"),
        };

    // As conexões com um handler que guarda a URL e responde o retorno dentro do nfeResultMsg.
    private static ConexoesSefaz Conexoes(string retorno, List<Uri> enviados) => new(_ => new HandlerRetorno(retorno, enviados));

    private sealed class HandlerRetorno(string retorno, List<Uri> enviados) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (enviados)
                enviados.Add(request.RequestUri!);
            var envelope = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>"
                + "<nfeResultMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4\">" + retorno
                + "</nfeResultMsg></soap:Body></soap:Envelope>";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(envelope, Encoding.UTF8, "application/soap+xml"),
            });
        }
    }
}
