using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFSe;
using BeneficioMunicipio = OpenDFe.SefazEmulada.BeneficioMunicipio;
using Emulador = OpenDFe.SefazEmulada.Emulador;
using EmissorMunicipio = OpenDFe.SefazEmulada.EmissorMunicipio;
using Municipio = OpenDFe.SefazEmulada.Municipio;
using PedidoFalha = OpenDFe.SefazEmulada.PedidoFalha;
using SituacaoConvenio = OpenDFe.SefazEmulada.SituacaoConvenio;
using TipoFalha = OpenDFe.SefazEmulada.TipoFalha;

namespace OpenDFe.Testes;

/// <summary>
/// A parametrização municipal da 9.4 pela lib inteira contra o ADN emulado (E.8): as cinco consultas com a configuração
/// dos municípios do emulador, o 404 e as falhas. A produção restrita (as rotas e o JSON de verdade) fica para o PFX.
/// </summary>
public sealed class ParametrizacaoEmuladaTestes : IClassFixture<SefinEmuladaFixture>, IDisposable
{
    private const int Bh = 3106200;

    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = DpsTeste.CnpjPrestador }.GerarPfx("teste"), "teste");

    private static readonly DateOnly Competencia = new(2026, 11, 1);

    private readonly Emulador _emulador;
    private readonly ConexoesSefaz _conexoes;
    private readonly DFe _dfe;

    public ParametrizacaoEmuladaTestes(SefinEmuladaFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Municipios.Limpar();
        _emulador.Municipios.Cadastrar(new Municipio(Bh, Nome: "Belo Horizonte", AliquotaPadrao: 5.00m,
            Aliquotas: new Dictionary<string, decimal> { ["010101"] = 2.50m }, RegimesEspeciais: [1, 6], Retencoes: ["52998224725"],
            Beneficios: [new BeneficioMunicipio("31062000000001", 4, 2.00m), new BeneficioMunicipio("31062000000002", 1)]));
        _conexoes = new ConexoesSefaz(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };
        _dfe = DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = _conexoes,
            Urls = [new EnderecoServico(Destino.ADN, Ambiente.Homologacao, "Parametrizacao", _emulador.Parametrizacao)],
        });
    }

    public void Dispose() => _conexoes.Dispose();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task OConvenioDoMunicipioConfiguradoEDoInativoComEmissorProprio()
    {
        _emulador.Municipios.Cadastrar(new Municipio(3550308, Convenio: SituacaoConvenio.Inativo, Emissor: EmissorMunicipio.Proprio));

        var bh = await _dfe.ExecutarAsync(new ConsultaConvenio(Bh), Ct);
        var sp = await _dfe.ExecutarAsync(new ConsultaConvenio(3550308), Ct);

        Assert.True(bh.Processado);
        Assert.Equal((200, "Convênio encontrado"), (bh.CStat, bh.Motivo));
        Assert.Equal((true, (bool?)true, (bool?)true), (bh.Retorno!.Encontrado, bh.Retorno.AderenteAmbienteNacional, bh.Retorno.AderenteEmissorNacional));
        Assert.Empty(bh.Problemas);
        Assert.Equal(new Uri(_emulador.Parametrizacao, "3106200/convenio"), bh.Rastro.Url);
        Assert.Contains("parametrosConvenio", bh.Rastro.XmlRetorno, StringComparison.Ordinal);
        Assert.Equal((false, false), (sp.Retorno!.AderenteAmbienteNacional, sp.Retorno.AderenteEmissorNacional));
    }

    [Fact]
    public async Task OMunicipioSemConvenioEo404ComAMensagem()
    {
        _emulador.Municipios.Cadastrar(new Municipio(3304557, Convenio: SituacaoConvenio.Inexistente));

        var resultado = await _dfe.ExecutarAsync(new ConsultaConvenio(3304557), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(404, resultado.CStat);
        Assert.False(resultado.Retorno!.Encontrado);
        Assert.Contains("convênio", resultado.Retorno.Mensagem, StringComparison.Ordinal);
        Assert.Equal(resultado.Retorno.Mensagem, resultado.Motivo);
    }

    [Fact]
    public async Task AAliquotaDoServicoAPadraoEOHistorico()
    {
        var propria = await _dfe.ExecutarAsync(new ConsultaAliquota(Bh, "010101") { Competencia = Competencia }, Ct);
        var padrao = await _dfe.ExecutarAsync(new ConsultaAliquota(Bh, "171901") { Competencia = Competencia }, Ct);
        var historico = await _dfe.ExecutarAsync(new ConsultaAliquota(Bh, "010101"), Ct);

        var aliquota = Assert.Single(propria.Retorno!.Aliquotas);
        Assert.Equal((2.50m, new DateTime(2022, 7, 1), (DateTime?)null), (aliquota.Aliquota, aliquota.Inicio, aliquota.Fim));
        Assert.Empty(propria.Problemas);
        Assert.Equal(5.00m, Assert.Single(padrao.Retorno!.Aliquotas).Aliquota);
        Assert.Equal(2.50m, Assert.Single(historico.Retorno!.Aliquotas).Aliquota);
        Assert.EndsWith("/historicoaliquotas", historico.Rastro.Url!.AbsolutePath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OsRegimesEspeciaisDoMunicipio()
    {
        var resultado = await _dfe.ExecutarAsync(new ConsultaRegimesEspeciais(Bh, "171901", Competencia), Ct);

        Assert.Empty(resultado.Problemas);
        Assert.Equal([RegimeEspecialTributacao.AtoCooperado, RegimeEspecialTributacao.SociedadeProfissionais],
            resultado.Retorno!.Regimes.Select(r => r.Regime!.Value));
        Assert.All(resultado.Retorno.Regimes, r => Assert.Equal(1, r.Situacao));
    }

    [Fact]
    public async Task AsRetencoesComOsResponsaveisESemNenhum()
    {
        var bh = await _dfe.ExecutarAsync(new ConsultaRetencoes(Bh, Competencia), Ct);
        var outro = await _dfe.ExecutarAsync(new ConsultaRetencoes(3550308, Competencia), Ct);

        Assert.Empty(bh.Problemas);
        Assert.False(bh.Retorno!.Artigo6Habilitado);
        var retencao = Assert.Single(bh.Retorno.Retencoes);
        Assert.Equal([RetencaoIssqn.Tomador, RetencaoIssqn.Intermediario], retencao.Tipos);
        Assert.Equal(["52998224725"], retencao.Responsaveis);
        Assert.Empty(outro.Retorno!.Retencoes);
    }

    [Fact]
    public async Task OBeneficioPeloNumeroEOQueOMunicipioNaoTem()
    {
        var diferenciada = await _dfe.ExecutarAsync(new ConsultaBeneficio(Bh, "31062000000001", Competencia), Ct);
        var isencao = await _dfe.ExecutarAsync(new ConsultaBeneficio(Bh, "31062000000002", Competencia), Ct);
        var ausente = await _dfe.ExecutarAsync(new ConsultaBeneficio(Bh, "31062000000099", Competencia), Ct);

        Assert.Empty(diferenciada.Problemas);
        var beneficio = diferenciada.Retorno!;
        Assert.Equal(("31062000000001", (TipoBeneficio?)TipoBeneficio.AliquotaDiferenciada, (decimal?)2.00m), (beneficio.Numero, beneficio.Tipo, beneficio.AliquotaDiferenciada));
        Assert.Equal((TipoBeneficio.Isencao, (decimal?)null), (isencao.Retorno!.Tipo!.Value, isencao.Retorno.AliquotaDiferenciada));
        Assert.Equal(404, ausente.CStat);
        Assert.False(ausente.Retorno!.Encontrado);
        Assert.NotNull(ausente.Retorno.Mensagem);
    }

    [Fact]
    public async Task AParalisacaoDoAdnEFalhaHttp()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "ADN", "ConsultaConvenio", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new ConsultaConvenio(Bh), Ct));
        var depois = await _dfe.ExecutarAsync(new ConsultaConvenio(Bh), Ct);

        Assert.Equal(TipoFalhaTransmissao.Http, falha.Tipo);
        Assert.Equal(503, falha.Rastro.StatusHttp);
        Assert.False(falha.PodeTerSidoProcessado);
        Assert.True(depois.Retorno!.Encontrado);
    }

    [Fact]
    public async Task ARespostaCortadaEIlegivel()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.RespostaIlegivel, "ADN", "ConsultaRetencoes", Vezes: 1));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => _dfe.ExecutarAsync(new ConsultaRetencoes(Bh, Competencia), Ct));

        Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, falha.Tipo);
    }
}
