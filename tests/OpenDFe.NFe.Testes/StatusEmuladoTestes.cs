using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;

namespace OpenDFe.Testes;

/// <summary>Uma SEFAZ emulada por classe de teste, numa porta livre e com a AC numa pasta temporária.</summary>
public sealed class EmuladorFixture : IAsyncLifetime
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N"));

    internal Emulador Emulador { get; private set; } = null!;

    public async ValueTask InitializeAsync() =>
        Emulador = await Emulador.IniciarAsync(new OpcoesEmulador { Porta = 0, Pasta = _pasta });

    public async ValueTask DisposeAsync()
    {
        await Emulador.DisposeAsync();
        try
        {
            Directory.Delete(_pasta, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}

/// <summary>
/// O status do serviço pela lib inteira contra a SEFAZ emulada (PLANO.md, "Encontros", da 4.3 em diante): o leiaute de
/// verdade, as URLs do emulador trocadas na configuração, o certificado autoassinado (13.1) no TLS mútuo e a AC do
/// emulador pela fábrica de teste. A homologação real fica para o PFX (PROJETO.md 17).
/// </summary>
public sealed class StatusEmuladoTestes : IClassFixture<EmuladorFixture>
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = "11222333000181" }.GerarPfx("teste"), "teste");

    private readonly Emulador _emulador;

    public StatusEmuladoTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<Documento, UF, TipoEmissao, Destino> Autorizadores => new()
    {
        { Documento.NFe, UF.MG, TipoEmissao.Normal, Destino.MG },
        { Documento.NFe, UF.SP, TipoEmissao.Normal, Destino.SP },
        { Documento.NFe, UF.SC, TipoEmissao.Normal, Destino.SVRS },
        { Documento.NFe, UF.MA, TipoEmissao.Normal, Destino.SVAN },
        { Documento.NFe, UF.MG, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN },
        { Documento.NFe, UF.PR, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS },
        { Documento.NFe, UF.PI, TipoEmissao.ContingenciaSvcAN, Destino.SvcAN },
        { Documento.NFe, UF.PI, TipoEmissao.ContingenciaSvcRS, Destino.SvcRS },
        { Documento.NFCe, UF.MG, TipoEmissao.Normal, Destino.MG },
        { Documento.NFCe, UF.SC, TipoEmissao.Normal, Destino.SVRS },
    };

    [Theory]
    [MemberData(nameof(Autorizadores))]
    public async Task OAutorizadorEmuladoEstaEmOperacao(Documento documento, UF uf, TipoEmissao emissao, Destino autorizador)
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes, documento, uf, emissao);

        var resultado = await dfe.ExecutarAsync(new ConsultaStatus(), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(107, resultado.CStat);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(autorizador, resultado.Rastro.Autorizador);
        var retorno = resultado.Retorno;
        Assert.True(retorno.Operando);
        Assert.Equal(Ambiente.Homologacao, retorno.Ambiente);
        Assert.Equal(uf, retorno.UF);
        Assert.Equal(autorizador.Sigla() + "-EMULADA", retorno.VersaoAplicativo);
        Assert.NotNull(retorno.Recebimento);
        Assert.Equal(TimeSpan.FromSeconds(1), retorno.TempoMedio);
    }

    [Theory]
    [InlineData("108", 108)]
    [InlineData("109", 109)]
    public async Task OAutorizadorParalisadoRespondeComAObservacao(string codigo, int cStat)
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Vezes: 1, Codigo: codigo, Observacao: "Manutencao programada"));
        using var conexoes = Conexoes();

        var resultado = await Criar(conexoes, Documento.NFe, UF.MG).ExecutarAsync(new ConsultaStatus(), Ct);

        Assert.Equal(cStat, resultado.CStat);
        var retorno = resultado.Retorno!;
        Assert.False(retorno.Operando);
        Assert.Equal("Manutencao programada", retorno.Observacao);
        Assert.Null(retorno.TempoMedio);
        Assert.Empty(resultado.Problemas);
    }

    [Fact]
    public async Task ComOAutorizadorParalisadoASvcAtende()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Codigo: "109"));
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes, Documento.NFe, UF.MG);

        var normal = await dfe.ExecutarAsync(new ConsultaStatus(), Ct);
        var svc = await dfe.ExecutarAsync(new ConsultaStatus { Destino = Destino.SvcAN }, Ct);

        Assert.Equal(109, normal.CStat);
        Assert.True(svc.Retorno!.Operando);
        Assert.Equal(Destino.SvcAN, svc.Rastro.Autorizador);
        Assert.Equal(UF.MG, svc.Retorno.UF);
    }

    [Fact]
    public async Task ODestinoDoParametroConsultaOutraUF()
    {
        using var conexoes = Conexoes();

        var resultado = await Criar(conexoes, Documento.NFe, UF.MG).ExecutarAsync(new ConsultaStatus { Destino = Destino.SC }, Ct);

        Assert.True(resultado.Retorno!.Operando);
        Assert.Equal(Destino.SVRS, resultado.Rastro.Autorizador);
        Assert.Equal(UF.SC, resultado.Retorno.UF);
    }

    [Fact]
    public async Task AUFQueOAutorizadorNaoAtendeERejeitada()
    {
        // A SVRS consultada com a UF de MG, que ela não atende: 289, processado.
        using var conexoes = Conexoes();

        var resultado = await Criar(conexoes, Documento.NFe, UF.MG).ExecutarAsync(new ConsultaStatus { Destino = Destino.SVRS }, Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(289, resultado.CStat);
        Assert.False(resultado.Retorno.Operando);
    }

    [Fact]
    public async Task AQuedaDepoisDeProcessarEFalhaQuePodeTerSidoProcessada()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1, Depois: true));
        using var conexoes = Conexoes();

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Criar(conexoes, Documento.NFe, UF.MG).ExecutarAsync(new ConsultaStatus(), Ct));

        Assert.Equal(TipoFalhaTransmissao.Conexao, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(Destino.MG, falha.Rastro.Autorizador);
        Assert.NotNull(falha.Rastro.XmlPedido);
    }

    private ConexoesSefaz Conexoes() =>
        new(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };

    private DFe Criar(ConexoesSefaz conexoes, Documento documento, UF uf, TipoEmissao emissao = TipoEmissao.Normal) =>
        DFe.Criar(new LeiauteNFe400(), documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = uf,
            Certificado = Certificado,
            Conexoes = conexoes,
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(DateTimeOffset.UnixEpoch, "Teste do status na SEFAZ emulada"),
            Urls = Urls(documento),
        });

    // As URLs do emulador para os serviços que o leiaute declara (o Criar recusa a URL de um serviço sem o envelope
    // declarado: a distribuição DF-e fica para a fase 8), como o ErpCore e o PDV as trocam no build de teste.
    private List<EnderecoServico> Urls(Documento documento)
    {
        var servicos = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento).Servicos.Select(s => s.Nome).ToHashSet(StringComparer.Ordinal);
        var urls = new List<EnderecoServico>();
        foreach (var url in _emulador.Urls)
        {
            if (servicos.Contains(url.Servico) && Destinos.TentarLerSigla(url.Destino, out var destino))
                urls.Add(new EnderecoServico(destino, Ambiente.Homologacao, url.Servico, url.Url));
        }
        return urls;
    }
}
