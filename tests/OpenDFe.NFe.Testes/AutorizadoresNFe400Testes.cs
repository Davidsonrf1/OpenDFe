using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// Os autorizadores do leiaute (etapa 4.2): os serviços SOAP, os atendimentos e as URLs declarados, e a escolha da
// URL caso a caso pelo ExecutarAsync, com uma operação de teste sobre o leiaute de verdade e um handler que só
// registra a URL. As URLs são as das fontes de 2026-10-22 (docs/NTS.md 3).
public class AutorizadoresNFe400Testes
{
    private const string Wsdl = "http://www.portalfiscal.inf.br/nfe/wsdl/";

    private static readonly string[] Principais =
        ["NFeAutorizacao4", "NFeRetAutorizacao4", "NFeConsultaProtocolo4", "NFeStatusServico4", "NFeInutilizacao4", "NFeRecepcaoEvento4"];

    private static readonly Lazy<X509Certificate2> Certificado = new(() => new CertificadoTeste { Cnpj = "11222333000181" }.Gerar());

    [Theory]
    [InlineData(Documento.NFe)]
    [InlineData(Documento.NFCe)]
    public void DeclaraOsServicosComONamespaceEAAcaoDoWsdl(Documento documento)
    {
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento);

        Assert.Equal(
            [
                ("NFeAutorizacao4", Wsdl + "NFeAutorizacao4", Wsdl + "NFeAutorizacao4/nfeAutorizacaoLote"),
                ("NFeRetAutorizacao4", Wsdl + "NFeRetAutorizacao4", Wsdl + "NFeRetAutorizacao4/nfeRetAutorizacaoLote"),
                ("NFeConsultaProtocolo4", Wsdl + "NFeConsultaProtocolo4", Wsdl + "NFeConsultaProtocolo4/nfeConsultaNF"),
                ("NFeStatusServico4", Wsdl + "NFeStatusServico4", Wsdl + "NFeStatusServico4/nfeStatusServicoNF"),
                ("NFeInutilizacao4", Wsdl + "NFeInutilizacao4", Wsdl + "NFeInutilizacao4/nfeInutilizacaoNF"),
                ("NFeRecepcaoEvento4", Wsdl + "NFeRecepcaoEvento4", Wsdl + "NFeRecepcaoEvento4/nfeRecepcaoEvento"),
                ("CadConsultaCadastro4", Wsdl + "CadConsultaCadastro4", Wsdl + "CadConsultaCadastro4/consultaCadastro"),
            ],
            declaracao.Servicos.Select(s => (s.Nome, s.NamespaceWsdl, s.Acao)));
    }

    [Theory]
    [InlineData(UF.MG, TipoEmissao.Normal, Destino.MG)]
    [InlineData(UF.BA, TipoEmissao.Normal, Destino.BA)]
    [InlineData(UF.PE, TipoEmissao.Normal, Destino.PE)]
    [InlineData(UF.AC, TipoEmissao.Normal, Destino.SVRS)]
    [InlineData(UF.RJ, TipoEmissao.Normal, Destino.SVRS)]
    [InlineData(UF.PI, TipoEmissao.Normal, Destino.SVRS)]
    [InlineData(UF.MA, TipoEmissao.Normal, Destino.SVAN)]
    [InlineData(UF.SP, TipoEmissao.ContingenciaFSIA, Destino.SP)]
    [InlineData(UF.MG, TipoEmissao.ContingenciaEpec, Destino.MG)]
    [InlineData(UF.AC, TipoEmissao.ContingenciaFSDA, Destino.SVRS)]
    [InlineData(UF.MA, TipoEmissao.ContingenciaOffline, Destino.SVAN)]
    [InlineData(UF.SP, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN)]
    [InlineData(UF.RS, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN)]
    [InlineData(UF.BA, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS)]
    [InlineData(UF.MA, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS)]
    [InlineData(UF.PI, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN)]
    [InlineData(UF.PI, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS)]
    public void OAtendimentoDaNFeEODaRelacaoServicosWeb(UF uf, TipoEmissao emissao, Destino autorizador)
    {
        Assert.Equal(autorizador, Atendimento(Documento.NFe, uf, emissao));
    }

    [Fact]
    public void NaNFeCadaUFTemUmaSvcSoOPITemAsDuas()
    {
        var atendimentos = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), Documento.NFe).Atendimentos;

        foreach (var uf in Enum.GetValues<UF>())
        {
            var svc = atendimentos.Where(a => a.UF == uf && a.Emissao is TipoEmissao.ContingenciaSvcAN or TipoEmissao.ContingenciaSvcRS).ToList();
            Assert.Equal(uf == UF.PI ? 2 : 1, svc.Count);
            Assert.All(svc, a => Assert.Equal(a.Emissao == TipoEmissao.ContingenciaSvcAN ? Destino.SvcAN : Destino.SvcRS, a.Autorizador));
        }
        Assert.Equal(27 * 5 + 28, atendimentos.Count);
    }

    [Theory]
    [InlineData(UF.MG, TipoEmissao.Normal, Destino.MG)]
    [InlineData(UF.SP, TipoEmissao.Normal, Destino.SP)]
    [InlineData(UF.AM, TipoEmissao.ContingenciaOffline, Destino.AM)]
    [InlineData(UF.BA, TipoEmissao.Normal, Destino.SVRS)]
    [InlineData(UF.PE, TipoEmissao.Normal, Destino.SVRS)]
    [InlineData(UF.MA, TipoEmissao.Normal, Destino.SVRS)]
    [InlineData(UF.CE, TipoEmissao.ContingenciaOffline, Destino.SVRS)]
    public void OAtendimentoDaNFCeEODoPortalNFCe(UF uf, TipoEmissao emissao, Destino autorizador)
    {
        Assert.Equal(autorizador, Atendimento(Documento.NFCe, uf, emissao));
    }

    [Fact]
    public void ANFCeSoTemONormalEOOffline()
    {
        var atendimentos = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), Documento.NFCe).Atendimentos;

        Assert.Equal(27 * 2, atendimentos.Count);
        Assert.All(atendimentos, a => Assert.Contains(a.Emissao, (TipoEmissao[])[TipoEmissao.Normal, TipoEmissao.ContingenciaOffline]));
    }

    [Theory]
    [InlineData(Documento.NFe, 190)]
    [InlineData(Documento.NFCe, 114)]
    public void TodoAutorizadorAtendidoTemOsServicosNosDoisAmbientes(Documento documento, int urls)
    {
        var declaracao = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento);

        Assert.Equal(urls, declaracao.Enderecos.Count);
        foreach (var autorizador in declaracao.Atendimentos.Select(a => a.Autorizador).Distinct())
        {
            var servicos = autorizador == Destino.SvcRS ? Principais.Where(s => s != "NFeInutilizacao4") : Principais;
            foreach (var ambiente in (Ambiente[])[Ambiente.Homologacao, Ambiente.Producao])
                foreach (var servico in servicos)
                    Assert.Contains(declaracao.Enderecos, e => e.Autorizador == autorizador && e.Ambiente == ambiente && e.Servico == servico);
        }
    }

    [Theory]
    [InlineData(Documento.NFe)]
    [InlineData(Documento.NFCe)]
    public void AsUrlsNaoTemOWsdlEMudamEntreOsAmbientes(Documento documento)
    {
        var enderecos = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento).Enderecos;

        Assert.All(enderecos, e => Assert.Equal("", e.Url.Query));
        foreach (var homologacao in enderecos.Where(e => e.Ambiente == Ambiente.Homologacao))
        {
            var producao = Assert.Single(enderecos, e => e.Ambiente == Ambiente.Producao && e.Autorizador == homologacao.Autorizador && e.Servico == homologacao.Servico);
            Assert.NotEqual(homologacao.Url.Host, producao.Url.Host);
        }
    }

    [Fact]
    public void AConsultaCadastroEOANSeguemAsListas()
    {
        var nfe = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), Documento.NFe).Enderecos;
        var nfce = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), Documento.NFCe).Enderecos;

        Assert.Equal(
            [Destino.AM, Destino.PE, Destino.BA, Destino.MG, Destino.SP, Destino.PR, Destino.RS, Destino.MS, Destino.MT, Destino.GO, Destino.SVRS],
            Autorizadores(nfe, "CadConsultaCadastro4"));
        Assert.Equal([Destino.PR, Destino.MS, Destino.GO], Autorizadores(nfce, "CadConsultaCadastro4"));
        Assert.Equal(["NFeRecepcaoEvento4"], nfe.Where(e => e.Autorizador == Destino.AN).Select(e => e.Servico).Distinct());
        Assert.DoesNotContain(nfce, e => e.Autorizador == Destino.AN);
        Assert.DoesNotContain(nfe, e => e.Servico == "NFeDistribuicaoDFe");
    }

    [Fact]
    public void ONucleoSeCriaEmTodaUFNosDoisDocumentos()
    {
        using var conexoes = new ConexoesSefaz();
        foreach (var documento in (Documento[])[Documento.NFe, Documento.NFCe])
            foreach (var uf in Enum.GetValues<UF>())
                Assert.Equal(uf, DFe.Criar(new LeiauteNFe400(), documento, Configuracao(conexoes, uf)).Configuracao.UF);
    }

    [Fact]
    public void ANFCeEmSvcNaoTemAtendimento()
    {
        using var conexoes = new ConexoesSefaz();
        var configuracao = Configuracao(conexoes, UF.MG, TipoEmissao.ContingenciaSvcAN);

        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteNFe400(), Documento.NFCe, configuracao));
    }

    // A escolha: sem chave, o destino e o tipo de emissão são os do núcleo.
    [Theory]
    [InlineData(Documento.NFe, UF.MG, Ambiente.Homologacao, Destino.MG, "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeStatusServico4")]
    [InlineData(Documento.NFe, UF.MG, Ambiente.Producao, Destino.MG, "https://nfe.fazenda.mg.gov.br/nfe2/services/NFeStatusServico4")]
    [InlineData(Documento.NFCe, UF.MG, Ambiente.Homologacao, Destino.MG, "https://hnfce.fazenda.mg.gov.br/nfce/services/NFeStatusServico4")]
    [InlineData(Documento.NFe, UF.AC, Ambiente.Homologacao, Destino.SVRS, "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx")]
    [InlineData(Documento.NFCe, UF.AC, Ambiente.Producao, Destino.SVRS, "https://nfce.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx")]
    [InlineData(Documento.NFe, UF.MA, Ambiente.Homologacao, Destino.SVAN, "https://hom.sefazvirtual.fazenda.gov.br/NFeStatusServico4/NFeStatusServico4.asmx")]
    [InlineData(Documento.NFCe, UF.MA, Ambiente.Homologacao, Destino.SVRS, "https://nfce-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx")]
    [InlineData(Documento.NFe, UF.PR, Ambiente.Producao, Destino.PR, "https://nfe.sefa.pr.gov.br/nfe/NFeStatusServico4")]
    public async Task SemChaveVaiAoAutorizadorDaUFDoNucleo(Documento documento, UF uf, Ambiente ambiente, Destino autorizador, string url)
    {
        var (resultado, enviada) = await Executar(documento, Configuracao(uf: uf, ambiente: ambiente), new Eco());

        Assert.Equal((autorizador, url), (resultado.Rastro.Autorizador, resultado.Rastro.Url!.AbsoluteUri));
        Assert.Equal(url, enviada);
    }

    [Theory]
    [InlineData(UF.MG, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN, "https://hom.sefazvirtual.fazenda.gov.br/NFeStatusServico4/NFeStatusServico4.asmx")]
    [InlineData(UF.PR, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS, "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx")]
    [InlineData(UF.SP, TipoEmissao.ContingenciaEpec, Destino.SP, "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfestatusservico4.asmx")]
    public async Task ONucleoEmContingenciaVaiAoAutorizadorDela(UF uf, TipoEmissao emissao, Destino autorizador, string url)
    {
        var (resultado, enviada) = await Executar(Documento.NFe, Configuracao(uf: uf, emissao: emissao), new Eco());

        Assert.Equal(autorizador, resultado.Rastro.Autorizador);
        Assert.Equal(url, enviada);
    }

    // Com chave, o destino e o tipo de emissão saem dela (PROJETO.md 8.4), não do núcleo.
    [Theory]
    [InlineData(Documento.NFe, UF.MG, TipoEmissao.Normal, UF.MG, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN,
        "https://hom.sefazvirtual.fazenda.gov.br/NFeConsultaProtocolo4/NFeConsultaProtocolo4.asmx")]
    [InlineData(Documento.NFe, UF.PI, TipoEmissao.Normal, UF.PI, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS,
        "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx")]
    [InlineData(Documento.NFe, UF.MG, TipoEmissao.ContingenciaSvcAN, UF.MG, TipoEmissao.Normal, Destino.MG,
        "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeConsultaProtocolo4")]
    [InlineData(Documento.NFe, UF.MG, TipoEmissao.Normal, UF.SP, TipoEmissao.ContingenciaEpec, Destino.SP,
        "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeconsultaprotocolo4.asmx")]
    [InlineData(Documento.NFe, UF.MG, TipoEmissao.Normal, UF.RJ, TipoEmissao.ContingenciaFSDA, Destino.SVRS,
        "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx")]
    [InlineData(Documento.NFCe, UF.MG, TipoEmissao.Normal, UF.MG, TipoEmissao.ContingenciaOffline, Destino.MG,
        "https://hnfce.fazenda.mg.gov.br/nfce/services/NFeConsultaProtocolo4")]
    [InlineData(Documento.NFCe, UF.SP, TipoEmissao.Normal, UF.BA, TipoEmissao.ContingenciaOffline, Destino.SVRS,
        "https://nfce-homologacao.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx")]
    public async Task ComChaveODestinoEOTipoEmissaoSaemDela(Documento documento, UF ufNucleo, TipoEmissao emissaoNucleo,
        UF ufChave, TipoEmissao emissaoChave, Destino autorizador, string url)
    {
        var chave = Chave(ufChave, documento, emissaoChave);

        var (resultado, enviada) = await Executar(documento, Configuracao(uf: ufNucleo, emissao: emissaoNucleo),
            new Eco { Servico = "NFeConsultaProtocolo4", Chave = chave });

        Assert.Equal(autorizador, resultado.Rastro.Autorizador);
        Assert.Equal(url, enviada);
    }

    [Fact]
    public async Task ODestinoParametroSobrepoeAChave()
    {
        var eco = new Eco { Servico = "NFeRecepcaoEvento4", Chave = Chave(UF.MG, Documento.NFe, TipoEmissao.Normal), Destino = Destino.AN };

        var (resultado, enviada) = await Executar(Documento.NFe, Configuracao(), eco);

        Assert.Equal(Destino.AN, resultado.Rastro.Autorizador);
        Assert.Equal("https://hom1.nfe.fazenda.gov.br/NFeRecepcaoEvento4/NFeRecepcaoEvento4.asmx", enviada);
    }

    [Fact]
    public async Task AUrlTrocadaVale()
    {
        var nova = new Uri("https://outro.exemplo.gov.br/nfe/NFeStatusServico4");
        var configuracao = Configuracao(urls: [new EnderecoServico(Destino.MG, Ambiente.Homologacao, "NFeStatusServico4", nova)]);

        var (_, enviada) = await Executar(Documento.NFe, configuracao, new Eco());

        Assert.Equal(nova.AbsoluteUri, enviada);
    }

    [Theory]
    [InlineData(Documento.NFe, UF.PR, TipoEmissao.ContingenciaSvcRS, "NFeInutilizacao4")]
    [InlineData(Documento.NFCe, UF.MG, TipoEmissao.Normal, "CadConsultaCadastro4")]
    [InlineData(Documento.NFe, UF.MA, TipoEmissao.Normal, "CadConsultaCadastro4")]
    public async Task SemUrlParaOServicoNaoTransmite(Documento documento, UF uf, TipoEmissao emissao, string servico)
    {
        var enviadas = new List<string>();
        using var conexoes = Conexoes(enviadas);
        var dfe = DFe.Criar(new LeiauteEco(), documento, Configuracao(conexoes, uf, emissao));

        await Assert.ThrowsAsync<NotSupportedException>(() => dfe.ExecutarAsync(new Eco { Servico = servico }, TestContext.Current.CancellationToken));
        Assert.Empty(enviadas);
    }

    private static Destino Atendimento(Documento documento, UF uf, TipoEmissao emissao) =>
        Assert.Single(DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento).Atendimentos, a => a.UF == uf && a.Emissao == emissao).Autorizador;

    private static List<Destino> Autorizadores(IEnumerable<EnderecoServico> enderecos, string servico) =>
        [.. enderecos.Where(e => e.Servico == servico).Select(e => e.Autorizador).Distinct().Order()];

    private static ChaveAcesso Chave(UF uf, Documento documento, TipoEmissao emissao) =>
        ChaveAcesso.Montar(uf, new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.FromHours(-3)), "11222333000181", documento, 1, 1, emissao, 12345678);

    private static async Task<(ResultadoWS<RetornoEco> Resultado, string Enviada)> Executar(Documento documento,
        Func<ConexoesSefaz, ConfiguracaoDFe> configuracao, Eco eco)
    {
        var enviadas = new List<string>();
        using var conexoes = Conexoes(enviadas);
        var dfe = DFe.Criar(new LeiauteEco(), documento, configuracao(conexoes));

        var resultado = await dfe.ExecutarAsync(eco, TestContext.Current.CancellationToken);

        Assert.True(resultado.Processado);
        return (resultado, Assert.Single(enviadas));
    }

    private static Func<ConexoesSefaz, ConfiguracaoDFe> Configuracao(UF uf = UF.MG, Ambiente ambiente = Ambiente.Homologacao,
        TipoEmissao emissao = TipoEmissao.Normal, IReadOnlyList<EnderecoServico>? urls = null) =>
        conexoes => Configuracao(conexoes, uf, emissao, ambiente, urls);

    private static ConfiguracaoDFe Configuracao(ConexoesSefaz conexoes, UF uf, TipoEmissao emissao = TipoEmissao.Normal,
        Ambiente ambiente = Ambiente.Homologacao, IReadOnlyList<EnderecoServico>? urls = null) => new()
        {
            Ambiente = ambiente,
            UF = uf,
            Certificado = Certificado.Value,
            Conexoes = conexoes,
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(DateTimeOffset.UnixEpoch, "Teste da escolha da URL do autorizador"),
            Urls = urls ?? [],
            Documento = null,
        };

    // As conexões com um handler que registra a URL e responde um envelope com o nfeResultMsg.
    private static ConexoesSefaz Conexoes(List<string> enviadas) => new(_ => new HandlerEco(enviadas));

    private sealed class HandlerEco(List<string> enviadas) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (enviadas)
                enviadas.Add(request.RequestUri!.AbsoluteUri);
            const string Envelope = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body>"
                + "<nfeResultMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/eco\"><eco/></nfeResultMsg></soap:Body></soap:Envelope>";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Envelope, Encoding.UTF8, "application/soap+xml"),
            });
        }
    }

    // O leiaute de verdade, com uma operação de teste: monta o pedido do status com o destino e o tipo de emissão
    // da chave, como farão as operações da 4.3 em diante.
    private sealed class LeiauteEco : ILeiaute
    {
        private readonly LeiauteNFe400 _leiaute = new();

        public string Versao => _leiaute.Versao;

        public IReadOnlyList<Documento> Documentos => _leiaute.Documentos;

        public void Declarar(DeclaracaoLeiaute declaracao)
        {
            _leiaute.Declarar(declaracao);
            declaracao.RegistrarOperacao(new OperacaoEco());
        }
    }

    public sealed class Eco : ParametroWS<RetornoEco>
    {
        public string Servico { get; init; } = "NFeStatusServico4";

        public ChaveAcesso? Chave { get; init; }
    }

    public sealed class RetornoEco;

    private sealed class OperacaoEco : IOperacao<Eco, RetornoEco>
    {
        public PedidoWS Montar(Eco parametro, ContextoOperacao contexto)
        {
            var xml = $"<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>{(int)contexto.Configuracao.Ambiente}</tpAmb>"
                + $"<cUF>{(int)contexto.Configuracao.UF}</cUF><xServ>STATUS</xServ></consStatServ>";
            return new PedidoWS(parametro.Servico, xml, SchemasNFe400.Status)
            {
                Destino = parametro.Chave?.UF.ComoDestino(),
                Emissao = parametro.Chave?.TipoEmissao,
            };
        }

        public LeituraWS<RetornoEco> Ler(Eco parametro, string xmlPedido, string xmlRetorno, ContextoOperacao contexto) => new(new RetornoEco(), 107, "Servico em Operacao");
    }
}
