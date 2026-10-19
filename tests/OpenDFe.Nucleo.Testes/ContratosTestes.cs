namespace OpenDFe.Testes;

// Os tipos que o leiaute e quem usa criam: as recusas dos construtores e a cópia das listas.
public class ContratosTestes
{
    private static readonly Uri Url = new("https://mg.exemplo/status");

    [Fact]
    public void OEnderecoExigeAUrlAbsolutaEHttps()
    {
        var endereco = new EnderecoServico(Destino.SVRS, Ambiente.Producao, "NFeStatusServico4", Url);
        Assert.Equal((Destino.SVRS, Ambiente.Producao, "NFeStatusServico4", Url), (endereco.Autorizador, endereco.Ambiente, endereco.Servico, endereco.Url));

        Assert.Throws<ArgumentException>(() => new EnderecoServico(Destino.MG, Ambiente.Producao, "S", new Uri("http://mg.exemplo/status")));
        Assert.Throws<ArgumentException>(() => new EnderecoServico(Destino.MG, Ambiente.Producao, "S", new Uri("/status", UriKind.Relative)));
        Assert.Throws<ArgumentException>(() => new EnderecoServico(Destino.MG, Ambiente.Producao, " ", Url));
        Assert.Throws<ArgumentNullException>(() => new EnderecoServico(Destino.MG, Ambiente.Producao, "S", null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnderecoServico((Destino)99, Ambiente.Producao, "S", Url));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnderecoServico(Destino.MG, (Ambiente)3, "S", Url));
    }

    [Fact]
    public void OPedidoCopiaAsListas()
    {
        var ids = new List<string> { "A" };
        var problemas = new List<Problema> { new("X", Severidade.Aviso, "aviso") };
        var pedido = new PedidoWS("S", "<a/>", LeiauteFalso.Schema) { IdsAssinatura = ids, Problemas = problemas };

        ids.Add("B");
        problemas.Clear();

        Assert.Equal(["A"], pedido.IdsAssinatura);
        Assert.Single(pedido.Problemas);
        Assert.Empty(new PedidoWS("S", "", LeiauteFalso.Schema).IdsAssinatura);
    }

    [Fact]
    public void OPedidoRecusaOValorInvalido()
    {
        var schema = LeiauteFalso.Schema;
        Assert.Throws<ArgumentException>(() => new PedidoWS("", "<a/>", schema));
        Assert.Throws<ArgumentNullException>(() => new PedidoWS("S", null!, schema));
        Assert.Throws<ArgumentNullException>(() => new PedidoWS("S", "<a/>", null!));
        Assert.Throws<ArgumentException>(() => new PedidoWS("S", "<a/>", schema) { IdsAssinatura = ["A", ""] });
        Assert.Throws<ArgumentNullException>(() => new PedidoWS("S", "<a/>", schema) { IdsAssinatura = null! });
        Assert.Throws<ArgumentNullException>(() => new PedidoWS("S", "<a/>", schema) { Problemas = [null!] });
        Assert.Throws<ArgumentOutOfRangeException>(() => new PedidoWS("S", "<a/>", schema) { Destino = (Destino)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new PedidoWS("S", "<a/>", schema) { Emissao = (TipoEmissao)3 });
    }

    [Fact]
    public void ALeituraRecusaONulo()
    {
        var leitura = new LeituraWS<RetornoEcoFalso>(new RetornoEcoFalso { Textos = [] }, 100, "ok");
        Assert.Equal((100, "ok"), (leitura.CStat, leitura.Motivo));
        Assert.Empty(leitura.Problemas);

        Assert.Throws<ArgumentNullException>(() => new LeituraWS<RetornoEcoFalso>(null!, 100, "ok"));
        Assert.Throws<ArgumentNullException>(() => new LeituraWS<RetornoEcoFalso>(new RetornoEcoFalso { Textos = [] }, 100, null!));
        Assert.Throws<ArgumentNullException>(() => new LeituraWS<RetornoEcoFalso>(new RetornoEcoFalso { Textos = [] }, 100, "ok") { Problemas = null! });
    }

    [Fact]
    public void OParametroRecusaODestinoDesconhecido()
    {
        Assert.Null(new StatusFalso().Destino);
        Assert.Equal(Destino.AN, new StatusFalso { Destino = Destino.AN }.Destino);
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatusFalso { Destino = (Destino)99 });
    }

    [Fact]
    public void AContingenciaExigeAJustificativa()
    {
        Assert.Throws<ArgumentException>(() => new Contingencia(DateTimeOffset.UnixEpoch, " "));
        Assert.Throws<ArgumentNullException>(() => new Contingencia(DateTimeOffset.UnixEpoch, null!));
    }

    [Fact]
    public void AConfiguracaoTemOsPadroes()
    {
        using var conexoes = new ConexoesSefaz();
        using var certificado = new CertificadoTeste { Cnpj = "11222333000181" }.Gerar();
        var configuracao = new ConfiguracaoDFe { Ambiente = Ambiente.Homologacao, UF = UF.MG, Certificado = certificado, Conexoes = conexoes };

        Assert.Equal(TipoEmissao.Normal, configuracao.Emissao);
        Assert.Null(configuracao.Contingencia);
        Assert.Empty(configuracao.Urls);
        Assert.Null(configuracao.Documento);
        Assert.Null(configuracao.Validacao);
        Assert.Same(TimeProvider.System, configuracao.Relogio);
    }
}
