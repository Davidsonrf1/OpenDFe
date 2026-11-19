using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFSe;
using Emulador = OpenDFe.SefazEmulada.Emulador;
using EmissorMunicipio = OpenDFe.SefazEmulada.EmissorMunicipio;
using Municipio = OpenDFe.SefazEmulada.Municipio;
using OpcoesEmulador = OpenDFe.SefazEmulada.OpcoesEmulador;
using PedidoFalha = OpenDFe.SefazEmulada.PedidoFalha;
using TipoFalha = OpenDFe.SefazEmulada.TipoFalha;

namespace OpenDFe.Testes;

/// <summary>Uma Sefin emulada por classe de teste (E.7), numa pasta temporária.</summary>
public sealed class SefinEmuladaFixture : IAsyncLifetime
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "opendfe-sefin-" + Guid.NewGuid().ToString("N"));

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
/// A emissão, as consultas e a recuperação da 9.2 pela lib inteira contra a Sefin emulada (E.7; PLANO.md, "Encontros"): o
/// leiaute de verdade, a base do emulador trocada na configuração, o certificado autoassinado no TLS mútuo e a AC do
/// emulador pela fábrica de teste. A produção restrita fica para o PFX (PROJETO.md 17).
/// </summary>
public sealed class EmissaoEmuladaTestes : IClassFixture<SefinEmuladaFixture>, IDisposable
{
    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = DpsTeste.CnpjPrestador }.GerarPfx("teste"), "teste");

    private static readonly TimeSpan Brasilia = TimeSpan.FromHours(-3);

    // A numeração de cada teste: a DPS repetida é a E0014 (a mesma série, número, município e inscrição).
    private static int _numero = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 100_000) * 100;

    private readonly Emulador _emulador;
    private readonly ConexoesSefaz _conexoes;
    private readonly DFe _dfe;

    public EmissaoEmuladaTestes(SefinEmuladaFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Municipios.Limpar();
        _conexoes = new ConexoesSefaz(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };
        _dfe = Criar(_conexoes);
    }

    public void Dispose() => _conexoes.Dispose();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DFe Criar(ConexoesSefaz conexoes) =>
        DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Urls = [new EnderecoServico(Destino.SefinNacional, Ambiente.Homologacao, "SefinNacional", _emulador.SefinNacional)],
        });

    // A DPS mínima do prestador CNPJ (o do certificado), emitida um minuto antes, com um número só dela.
    private static Dps Dps(Action<Dps>? ajustar = null)
    {
        var agora = DateTimeOffset.UtcNow.ToOffset(Brasilia).AddMinutes(-1);
        var dps = DpsTeste.Minima();
        dps.Numero = Interlocked.Increment(ref _numero);
        dps.Emissao = agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerSecond));
        dps.Competencia = DateOnly.FromDateTime(dps.Emissao.Value.DateTime);
        dps.Prestador.Cpf = null;
        dps.Prestador.Cnpj = DpsTeste.CnpjPrestador;
        ajustar?.Invoke(dps);
        return dps;
    }

    private static IdentificadorDps IdDe(Dps dps) => IdentificadorDps.Montar(dps.MunicipioEmissor!.Value, DpsTeste.CnpjPrestador, dps.Serie!, dps.Numero!.Value);

    // A emissão

    [Fact]
    public async Task EmiteANFSeEConsultaPelaChaveEPelaDps()
    {
        var dps = Dps();

        var emissao = await _dfe.ExecutarAsync(new EmissaoNFSe(dps), Ct);

        Assert.True(emissao.Processado, emissao.Motivo);
        Assert.Equal(201, emissao.CStat);
        Assert.Equal("NFS-e gerada", emissao.Motivo);
        var retorno = emissao.Retorno;
        Assert.True(retorno.Emitida, string.Join("; ", retorno.Erros));
        var chave = retorno.Chave!.Value;
        Assert.Equal(DpsTeste.Municipio, chave.Municipio);
        Assert.Equal(DpsTeste.CnpjPrestador, chave.Inscricao);
        Assert.NotNull(retorno.Nota);
        Assert.Equal(dps.Numero, retorno.Nota!.Dps!.Numero);
        Assert.Empty(emissao.Problemas);
        Assert.Equal(201, emissao.Rastro.StatusHttp);
        Assert.Equal(new Uri(_emulador.SefinNacional, "nfse"), emissao.Rastro.Url);
        Assert.Contains("<Signature", emissao.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Contains("chaveAcesso", emissao.Rastro.XmlRetorno, StringComparison.Ordinal);

        // A NFS-e gerada lida pelo leiaute: a DPS de dentro é a que a lib assinou.
        var lida = LeiauteNFSe101.Instancia.Ler(retorno.Xml!);
        Assert.Equal(chave, lida.Chave);
        Assert.Equal(IdDe(dps), lida.IdentificadorDps);
        Assert.Contains(emissao.Rastro.XmlPedido!.Replace("<DPS ", "", StringComparison.Ordinal)[..60], retorno.Xml, StringComparison.Ordinal);

        var consulta = await _dfe.ExecutarAsync(new ConsultaNFSe(chave), Ct);
        Assert.True(consulta.Retorno!.Encontrada);
        Assert.Equal(retorno.Xml, consulta.Retorno.Xml);
        Assert.Null(consulta.Rastro.XmlPedido);

        var porDps = await _dfe.ExecutarAsync(new ConsultaDps(IdDe(dps)), Ct);
        Assert.True(porDps.Retorno!.Existe);
        Assert.Equal(chave, porDps.Retorno.Chave);

        var existe = await _dfe.ExecutarAsync(new ConsultaDps(IdDe(dps)) { ApenasExistencia = true }, Ct);
        Assert.True(existe.Retorno!.Existe);
        Assert.Null(existe.Retorno.Chave);
        Assert.Null(existe.Rastro.XmlRetorno);
    }

    [Fact]
    public async Task ADpsRepetidaERejeitadaComAChaveDaPrimeira()
    {
        var dps = Dps();
        var primeira = await _dfe.ExecutarAsync(new EmissaoNFSe(dps), Ct);

        var repetida = await _dfe.ExecutarAsync(new EmissaoNFSe(dps), Ct);

        Assert.True(repetida.Processado);
        Assert.Equal(400, repetida.CStat);
        Assert.False(repetida.Retorno.Emitida);
        var erro = Assert.Single(repetida.Retorno.Erros);
        Assert.Equal("E0014", erro.Codigo);
        Assert.Contains(primeira.Retorno!.Chave!.Value.Valor, erro.Complemento, StringComparison.Ordinal);
        Assert.StartsWith("E0014: ", repetida.Motivo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OMunicipioComEmissorProprioERejeitado()
    {
        const int Codigo = 3550308;
        _emulador.Municipios.Cadastrar(new Municipio(Codigo, Emissor: EmissorMunicipio.Proprio));

        var resultado = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps(d =>
        {
            d.MunicipioEmissor = Codigo;
            d.Servico.Local.Municipio = Codigo;
        })), Ct);

        Assert.Equal(400, resultado.CStat);
        Assert.Equal("E0039", Assert.Single(resultado.Retorno!.Erros).Codigo);
    }

    [Fact]
    public async Task ASubstituicaoVaiComoEmissaoComOGrupoSubst()
    {
        var original = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps()), Ct);

        var substituta = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps(d => d.Substituicao = new Substituicao
        {
            Chave = original.Retorno!.Chave!.Value.Valor,
            Motivo = MotivoSubstituicao.Outros,
            Descricao = "Correção do valor do serviço",
        })), Ct);

        // A substituição chega à Sefin; o emulador só a gera na E.8.
        Assert.True(substituta.Processado);
        Assert.Contains("<subst>", substituta.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Equal("EMULADA-SUBST", Assert.Single(substituta.Retorno.Erros).Codigo);
    }

    // A recusa local

    [Fact]
    public async Task ARegraForaDoSchemaRecusaSemTransmitir()
    {
        var antes = _emulador.NFSe.Listar().Count;

        var resultado = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps(d => d.Prestador.Nome = "NOME QUE A SEFIN TIRA DO CADASTRO")), Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal("NFSE-PRESTADOR", Assert.Single(resultado.Problemas).Codigo);
        Assert.Contains("<Signature", resultado.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.Null(resultado.Rastro.Url);
        Assert.Equal(antes, _emulador.NFSe.Listar().Count);
    }

    [Fact]
    public async Task OSchemaEAMontagemRecusamSemTransmitir()
    {
        var schema = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps(d => d.Servico.Codigo.TributacaoNacional = "1")), Ct);
        var montagem = await _dfe.ExecutarAsync(new EmissaoNFSe(Dps(d => d.Numero = null)), Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, schema.Situacao);
        Assert.Contains(schema.Problemas, p => p.Codigo == "E1235");
        Assert.Equal(SituacaoResultado.RecusadoLocalmente, montagem.Situacao);
        Assert.DoesNotContain("<Signature", montagem.Rastro.XmlPedido, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AConsultaDoQueNaoExisteDevolve404()
    {
        var dps = Dps();

        var porDps = await _dfe.ExecutarAsync(new ConsultaDps(IdDe(dps)), Ct);
        var head = await _dfe.ExecutarAsync(new ConsultaDps(IdDe(dps)) { ApenasExistencia = true }, Ct);
        var porChave = await _dfe.ExecutarAsync(new ConsultaNFSe(ChaveNFSe.Ler(NFSeTeste.Chave)), Ct);

        Assert.Equal(404, porDps.CStat);
        Assert.False(porDps.Retorno!.Existe);
        Assert.Equal(404, head.CStat);
        Assert.False(head.Retorno!.Existe);
        Assert.Equal(404, porChave.CStat);
        Assert.False(porChave.Retorno!.Encontrada);
        Assert.Equal("E0044", Assert.Single(porChave.Retorno.Erros).Codigo);
    }

    // A queda no meio e a recuperação

    [Fact]
    public async Task AQuedaDepoisDeGerarSeRecuperaPelaDps()
    {
        var dps = Dps();
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "SefinNacional", "EmissaoNFSe", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new EmissaoNFSe(dps), Ct));
        var recuperacao = await RecuperacaoEmissao.ExecutarAsync(_dfe, IdDe(dps), Ct);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Contains("<Signature", falha.Rastro.XmlPedido, StringComparison.Ordinal);
        Assert.True(recuperacao.Gerada);
        Assert.False(recuperacao.NaoGerada);
        Assert.Equal(200, recuperacao.Dps.CStat);
        var nota = recuperacao.Nota!.Retorno!;
        Assert.Equal(recuperacao.Dps.Retorno!.Chave, nota.Chave);
        Assert.Equal(IdDe(dps), LeiauteNFSe101.Instancia.Ler(nota.Xml!).IdentificadorDps);
    }

    [Fact]
    public async Task AQuedaAntesDeGerarPedeOReenvioDaMesmaDps()
    {
        var dps = Dps();
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "SefinNacional", "EmissaoNFSe", Vezes: 1));

        await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new EmissaoNFSe(dps), Ct));
        var recuperacao = await RecuperacaoEmissao.ExecutarAsync(_dfe, IdDe(dps), Ct);
        var reenvio = await _dfe.ExecutarAsync(new EmissaoNFSe(dps), Ct);

        Assert.False(recuperacao.Gerada);
        Assert.True(recuperacao.NaoGerada);
        Assert.Null(recuperacao.Nota);
        Assert.True(reenvio.Retorno!.Emitida);
    }

    [Fact]
    public async Task ARecuperacaoRecusaOsArgumentosVazios()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => RecuperacaoEmissao.ExecutarAsync(null!, IdDe(Dps()), Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => RecuperacaoEmissao.ExecutarAsync(_dfe, default, Ct));
    }

    // As falhas da Sefin

    [Fact]
    public async Task AParalisacaoEO503SaoFalhaHttp()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "SefinNacional", "EmissaoNFSe", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new EmissaoNFSe(Dps()), Ct));

        Assert.Equal(TipoFalhaTransmissao.Http, falha.Tipo);
        Assert.Equal(503, falha.Rastro.StatusHttp);
        Assert.True(falha.PodeTerSidoProcessado);
    }

    [Fact]
    public async Task AFalhaInternaEO500()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.FalhaSoap, "SefinNacional", "ConsultaDps", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new ConsultaDps(IdDe(Dps())), Ct));

        Assert.Equal(TipoFalhaTransmissao.Http, falha.Tipo);
        Assert.Equal(500, falha.Rastro.StatusHttp);
        Assert.False(falha.PodeTerSidoProcessado);
    }

    [Fact]
    public async Task ARespostaCortadaEIlegivel()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.RespostaIlegivel, "SefinNacional", "EmissaoNFSe", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new EmissaoNFSe(Dps()), Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
        Assert.True(falha.PodeTerSidoProcessado);
    }

    [Fact]
    public async Task AMarcaNoTextoLivreComandaAParalisacao()
    {
        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() =>
            _dfe.ExecutarAsync(new EmissaoNFSe(Dps(d => d.Servico.InformacoesComplementares = new InformacoesComplementares { Texto = "Pedido 7 EMULADA:paralisacao" })), Ct));

        Assert.Equal(503, falha.Rastro.StatusHttp);
    }
}
