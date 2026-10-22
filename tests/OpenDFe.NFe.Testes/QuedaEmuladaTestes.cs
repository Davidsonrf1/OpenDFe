using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;
using ConsultaProtocolo = OpenDFe.NFe.ConsultaProtocolo;

namespace OpenDFe.Testes;

/// <summary>
/// A queda no meio pela lib inteira contra a SEFAZ emulada (etapa 4.5): a consulta do protocolo, a recuperação depois
/// da conexão derrubada (antes e depois de autorizar), o reenvio do mesmo XML e a duplicidade com a chave da outra nota.
/// As notas são emitidas agora, e o repositório do emulador é limpo a cada teste.
/// </summary>
public sealed class QuedaEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = NotasTeste.CnpjEmitente }.GerarPfx("teste"), "teste");

    private readonly Emulador _emulador;

    public QuedaEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task AConsultaAchaAAutorizadaENaoAQueNaoFoi()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var autorizada = Assert.Single((await dfe.ExecutarAsync(new Autorizacao(Nota(1)), Ct)).Retorno!.Notas);
        var nunca = dfe.Preparar(Nota(2));

        var achada = await dfe.ExecutarAsync(new ConsultaProtocolo(autorizada.Chave!), Ct);
        var ausente = await dfe.ExecutarAsync(new ConsultaProtocolo(nunca.Chave!), Ct);

        Assert.True(achada.Processado);
        Assert.Empty(achada.Problemas);
        Assert.True(achada.Retorno.Autorizada);
        Assert.Equal(autorizada.Chave, achada.Retorno.Chave);
        Assert.Equal(autorizada.Protocolo!.Numero, achada.Retorno.Protocolo!.Numero);
        Assert.Empty(achada.Retorno.Eventos);
        Assert.Equal(217, ausente.CStat);
        Assert.Null(ausente.Retorno!.Protocolo);
    }

    [Fact]
    public async Task OCodigoNumericoDiferenteTrazAChaveDaBase()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var autorizada = Assert.Single((await dfe.ExecutarAsync(new Autorizacao(Nota(1)), Ct)).Retorno!.Notas);
        var outra = Nota(1);
        outra.Identificacao.CodigoNumerico = 87654321;

        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(dfe.Preparar(outra).Chave!), Ct);

        Assert.Equal(562, consulta.CStat);
        Assert.Equal(autorizada.Chave, consulta.Retorno!.ChaveRegistrada);
    }

    [Fact]
    public async Task ANotaDaSvcEConsultadaNaSvcENoNormal()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Codigo: "109"));
        using var conexoes = Conexoes();
        var svc = Criar(conexoes, TipoEmissao.ContingenciaSvcAN);
        var normal = Criar(conexoes);
        var nota = Assert.Single((await svc.ExecutarAsync(new Autorizacao(Nota(1)), Ct)).Retorno!.Notas);

        var consultaSvc = await normal.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);
        var paralisado = await normal.ExecutarAsync(new ConsultaProtocolo(nota.Chave!) { Destino = Destino.MG }, Ct);
        _emulador.Controle.Limpar();
        var consultaNormal = await normal.ExecutarAsync(new ConsultaProtocolo(nota.Chave!) { Destino = Destino.MG }, Ct);

        Assert.Equal((Destino.SvcAN, 100), (consultaSvc.Rastro.Autorizador, consultaSvc.CStat));
        Assert.Equal((Destino.MG, 109), (paralisado.Rastro.Autorizador, paralisado.CStat));
        Assert.Equal((Destino.MG, 100), (consultaNormal.Rastro.Autorizador, consultaNormal.CStat));
    }

    [Fact]
    public async Task AQuedaDepoisDeAutorizarSeRecuperaEOReenvioEDuplicidade()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new Autorizacao(Nota(1)), Ct));
        Assert.True(falha.PodeTerSidoProcessado);
        var xml = Assert.Single(Autorizacao.NotasEnviadas(falha.Rastro.XmlPedido!));
        var recuperada = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml), Ct);
        var reenvio = await dfe.ExecutarAsync(new Autorizacao(xml), Ct);

        Assert.True(recuperada.Processado);
        Assert.Empty(recuperada.Problemas);
        Assert.Equal(100, recuperada.CStat);
        var nota = Assert.Single(recuperada.Retorno.Notas);
        Assert.True(nota.Autorizada);
        Assert.Equal(xml, nota.Xml);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        Assert.Empty(AssinaturaDigital.Verificar(nota.XmlProcessado!, "NFe" + nota.Chave, NotasTeste.CnpjEmitente));

        var duplicada = Assert.Single(reenvio.Retorno!.Notas);
        Assert.Equal(204, duplicada.Protocolo!.Status);
        Assert.True(duplicada.Duplicidade!.MesmaChave);
        Assert.Equal(nota.Chave, duplicada.Duplicidade.Chave);
        var outraVez = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(duplicada.Xml!), Ct);
        Assert.Equal(nota.XmlProcessado, outraVez.Retorno!.Notas[0].XmlProcessado);
    }

    [Fact]
    public async Task AQuedaAntesDeProcessarNaoConstaEOReenvioAutoriza()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new Autorizacao(Nota(1)), Ct));
        var xml = Assert.Single(Autorizacao.NotasEnviadas(falha.Rastro.XmlPedido!));
        var recuperada = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml), Ct);
        var reenvio = await dfe.ExecutarAsync(new Autorizacao(xml), Ct);

        Assert.Equal(217, recuperada.CStat);
        Assert.Null(Assert.Single(recuperada.Retorno!.Notas).Protocolo);
        var nota = Assert.Single(reenvio.Retorno!.Notas);
        Assert.True(nota.Autorizada);
        Assert.Equal(xml, nota.Xml);
    }

    [Fact]
    public async Task AMesmaNumeracaoComOutraChaveApontaAOutraNota()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var outra = Nota(1);
        outra.Identificacao.CodigoNumerico = 87654321;
        var primeira = Assert.Single((await dfe.ExecutarAsync(new Autorizacao(Nota(1)), Ct)).Retorno!.Notas);

        var rejeitada = Assert.Single((await dfe.ExecutarAsync(new Autorizacao(outra), Ct)).Retorno!.Notas);
        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(rejeitada.Duplicidade!.Chave!), Ct);

        Assert.Equal(539, rejeitada.Protocolo!.Status);
        Assert.False(rejeitada.Duplicidade.MesmaChave);
        Assert.Equal(primeira.Chave, rejeitada.Duplicidade.Chave);
        Assert.True(consulta.Retorno!.Autorizada);
    }

    // A NF-e de teste emitida agora, com o número do caso e o cNF fixo.
    private static NotaFiscal Nota(int numero)
    {
        var nota = NotasTeste.NFe();
        var agora = DateTimeOffset.UtcNow.AddMinutes(-1).ToOffset(TimeSpan.FromHours(-3));
        agora = agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerSecond));
        nota.Identificacao.Numero = numero;
        nota.Identificacao.CodigoNumerico = 12345678;
        nota.Identificacao.Emissao = agora;
        nota.Identificacao.SaidaEntrada = agora;
        return nota;
    }

    private ConexoesSefaz Conexoes() =>
        new(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };

    private DFe Criar(ConexoesSefaz conexoes, TipoEmissao emissao = TipoEmissao.Normal) =>
        DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(DateTimeOffset.UtcNow.AddMinutes(-10), "Teste da queda na SEFAZ emulada"),
            Urls = Urls(),
        });

    // As URLs do emulador para os serviços que o leiaute declara.
    private List<EnderecoServico> Urls()
    {
        var servicos = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), Documento.NFe).Servicos.Select(s => s.Nome).ToHashSet(StringComparer.Ordinal);
        var urls = new List<EnderecoServico>();
        foreach (var url in _emulador.Urls)
        {
            if (servicos.Contains(url.Servico) && Destinos.TentarLerSigla(url.Destino, out var destino))
                urls.Add(new EnderecoServico(destino, Ambiente.Homologacao, url.Servico, url.Url));
        }
        return urls;
    }
}
