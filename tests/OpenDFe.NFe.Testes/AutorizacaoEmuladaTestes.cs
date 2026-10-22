using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;

namespace OpenDFe.Testes;

/// <summary>
/// A autorização da NF-e pela lib inteira contra a SEFAZ emulada (etapa 4.4): a síncrona, o lote com a consulta do
/// recibo, a nota preparada, a duplicidade, a contingência SVC-AN e a queda depois de autorizar. As notas são emitidas
/// agora (o prazo do emulador é o do relógio dele), e o repositório do emulador é limpo a cada teste.
/// </summary>
public sealed class AutorizacaoEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = NotasTeste.CnpjEmitente }.GerarPfx("teste"), "teste");

    private readonly Emulador _emulador;

    public AutorizacaoEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task UmaNotaEAutorizadaNaSincrona()
    {
        using var conexoes = Conexoes();

        var resultado = await Criar(conexoes).ExecutarAsync(new Autorizacao(Nota(1)), Ct);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(104, resultado.CStat);
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.True(nota.Autorizada);
        Assert.Equal(100, nota.Protocolo!.Status);
        Assert.Equal(nota.Chave!.ToString(), nota.Protocolo.Chave);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        Assert.Empty(AssinaturaDigital.Verificar(nota.XmlProcessado!, "NFe" + nota.Chave, NotasTeste.CnpjEmitente));
        Assert.Contains(_emulador.Repositorio.Notas(), n => n.Chave == nota.Chave.ToString());
    }

    [Fact]
    public async Task OLoteDeDuasEAutorizadoPelaConsultaDoRecibo()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);

        var lote = await dfe.ExecutarAsync(new Autorizacao(Nota(1), Nota(2)), Ct);
        Assert.Equal(103, lote.CStat);
        var recibo = lote.Retorno!.Recibo!;
        Assert.Equal(2, lote.Retorno.Notas.Count);

        var consulta = await dfe.ExecutarAsync(new ConsultaRecibo(recibo) { Xmls = [.. lote.Retorno.Notas.Select(n => n.Xml!)] }, Ct);

        Assert.Equal(104, consulta.CStat);
        Assert.Empty(consulta.Problemas);
        Assert.Equal(lote.Retorno.Notas.Select(n => n.Chave), consulta.Retorno!.Notas.Select(n => n.Chave));
        Assert.All(consulta.Retorno.Notas, n =>
        {
            Assert.True(n.Autorizada);
            Assert.Empty(SchemasNFe400.NotaProcessada.Validar(n.XmlProcessado!));
        });
    }

    [Fact]
    public async Task OLoteAindaNaFilaRespondeOs105EDepoisOs104()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.LoteProcessando, "MG", Vezes: 1));
        var lote = await dfe.ExecutarAsync(new Autorizacao(Nota(1), Nota(2)), Ct);
        var consulta = new ConsultaRecibo(lote.Retorno!.Recibo!) { Xmls = [.. lote.Retorno.Notas.Select(n => n.Xml!)] };

        var fila = await dfe.ExecutarAsync(consulta, Ct);
        var processado = await dfe.ExecutarAsync(consulta, Ct);

        Assert.Equal(105, fila.CStat);
        Assert.Empty(fila.Problemas);
        Assert.All(fila.Retorno!.Notas, n => Assert.Null(n.Protocolo));
        Assert.Equal(104, processado.CStat);
        Assert.All(processado.Retorno!.Notas, n => Assert.True(n.Autorizada));
    }

    [Fact]
    public async Task ANotaPreparadaEAutorizadaEOReenvioEDuplicidade()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var preparado = dfe.Preparar(Nota(1));
        Assert.True(preparado.Preparado);

        var primeira = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);
        var reenvio = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        var autorizada = Assert.Single(primeira.Retorno!.Notas);
        Assert.True(autorizada.Autorizada);
        Assert.Equal(preparado.Xml, autorizada.Xml);
        Assert.Contains(preparado.Xml!, autorizada.XmlProcessado, StringComparison.Ordinal);
        var duplicada = Assert.Single(reenvio.Retorno!.Notas);
        Assert.Equal(204, duplicada.Protocolo!.Status);
        Assert.False(duplicada.Autorizada);
        Assert.Null(duplicada.XmlProcessado);
    }

    [Fact]
    public async Task AMesmaNumeracaoComOutraChaveERejeitada()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var outra = Nota(1);
        outra.Identificacao.CodigoNumerico = 87654321;

        await dfe.ExecutarAsync(new Autorizacao(Nota(1)), Ct);
        var rejeitada = await dfe.ExecutarAsync(new Autorizacao(outra), Ct);

        Assert.True(rejeitada.Processado);
        var nota = Assert.Single(rejeitada.Retorno.Notas);
        Assert.Equal(539, nota.Protocolo!.Status);
        Assert.Null(nota.XmlProcessado);
    }

    [Fact]
    public async Task ComMGParalisadoASvcANAutoriza()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Codigo: "109"));
        using var conexoes = Conexoes();
        var normal = Criar(conexoes);
        var svc = Criar(conexoes, TipoEmissao.ContingenciaSvcAN);

        var paralisado = await normal.ExecutarAsync(new Autorizacao(Nota(1)), Ct);
        var contingencia = await svc.ExecutarAsync(new Autorizacao(Nota(2)), Ct);

        Assert.Equal(109, paralisado.CStat);
        Assert.Empty(paralisado.Retorno!.Notas);
        Assert.Equal(Destino.SvcAN, contingencia.Rastro.Autorizador);
        var nota = Assert.Single(contingencia.Retorno!.Notas);
        Assert.True(nota.Autorizada);
        Assert.Equal(TipoEmissao.ContingenciaSvcAN, nota.Chave!.TipoEmissao);
    }

    [Fact]
    public async Task AQuedaDepoisDeAutorizarDeixaOPedidoNoRastroEOReenvioEDuplicidade()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var preparado = dfe.Preparar(Nota(1));
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct));
        var reenvio = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Contains(preparado.Xml!, falha.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Equal(204, Assert.Single(reenvio.Retorno!.Notas).Protocolo!.Status);
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
            Contingencia = emissao == TipoEmissao.Normal ? null : new Contingencia(DateTimeOffset.UtcNow.AddMinutes(-10), "Teste da autorizacao na SEFAZ emulada"),
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
