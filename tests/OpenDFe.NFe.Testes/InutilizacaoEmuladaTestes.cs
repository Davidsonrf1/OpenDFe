using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;
using Inutilizacao = OpenDFe.NFe.Inutilizacao;

namespace OpenDFe.Testes;

/// <summary>
/// A inutilização pela lib inteira contra a SEFAZ emulada (etapa 6.1): a faixa homologada (102) com o <c>ProcInutNFe</c>, a
/// mesma faixa de novo (563), a faixa com número já usado (241) e a que cruza outra inutilizada (256), a série do CPF (266),
/// o ano futuro (453) e o antigo (454), o limite de 10.000 números (201), a NFC-e e o núcleo em SVC, que inutiliza no
/// autorizador normal.
/// </summary>
public sealed class InutilizacaoEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private const string Justificativa = "Inutilizacao por salto de numeracao";

    private readonly Emulador _emulador;

    public InutilizacaoEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Inutilizacao Faixa(int serie, int inicial, int final) =>
        new(NotasTeste.CnpjEmitente, serie, inicial, final, Justificativa);

    [Fact]
    public async Task AFaixaEHomologadaComOProcInutNFeValido()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);

        var resultado = await dfe.ExecutarAsync(Faixa(50, 10, 15), Ct);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        var retorno = resultado.Retorno!;
        Assert.True(retorno.Homologada);
        Assert.Equal((102, Documento.NFe, 50, 10, 15, NotasTeste.CnpjEmitente, UF.MG), (retorno.Status, retorno.Modelo, retorno.Serie, retorno.NumeroInicial, retorno.NumeroFinal, retorno.Cnpj, retorno.UF));
        Assert.Equal(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3)).Year % 100, retorno.Ano);
        Assert.NotNull(retorno.Protocolo);
        Assert.Empty(SchemasNFe400.InutilizacaoProcessada.Validar(retorno.XmlProcessado!));
        Assert.Contains(resultado.Rastro.XmlPedido!, retorno.XmlProcessado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AMesmaFaixaDeNovoE563ComOProtocoloDaAnterior()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var primeira = (await dfe.ExecutarAsync(Faixa(50, 10, 15), Ct)).Retorno!;

        var segunda = (await dfe.ExecutarAsync(Faixa(50, 10, 15), Ct)).Retorno!;

        Assert.Equal(563, segunda.Status);
        Assert.False(segunda.Homologada);
        Assert.Equal((10, 15, primeira.Protocolo), (segunda.NumeroInicial, segunda.NumeroFinal, segunda.Protocolo));
        Assert.Null(segunda.XmlProcessado);
    }

    [Fact]
    public async Task AFaixaQueCobreNumeroJaUsadoE241()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var nota = Assert.Single((await dfe.ExecutarAsync(new Autorizacao(ApoioEmulado.Nota(Documento.NFe, 3, DateTimeOffset.UtcNow)), Ct)).Retorno!.Notas);
        Assert.True(nota.Autorizada);

        var retorno = (await dfe.ExecutarAsync(Faixa(1, 1, 5), Ct)).Retorno!;

        Assert.Equal(241, retorno.Status);
    }

    [Fact]
    public async Task AFaixaQueCruzaUmaInutilizadaE256()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        Assert.Equal(102, (await dfe.ExecutarAsync(Faixa(50, 20, 30), Ct)).CStat);

        var retorno = (await dfe.ExecutarAsync(Faixa(50, 25, 35), Ct)).Retorno!;

        Assert.Equal(256, retorno.Status);
    }

    [Theory]
    [InlineData(920, 0, 266)]
    [InlineData(975, 0, 266)]
    [InlineData(50, 99, 453)]
    [InlineData(50, 5, 454)]
    public async Task ASerieDoCpfOAnoFuturoEOAntigoSaoRejeitados(int serie, int ano, int status)
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);

        var retorno = (await dfe.ExecutarAsync(new Inutilizacao(NotasTeste.CnpjEmitente, serie, 1, 2, Justificativa) { Ano = ano == 0 ? null : ano }, Ct)).Retorno!;

        Assert.Equal(status, retorno.Status);
        Assert.Null(retorno.XmlProcessado);
    }

    [Fact]
    public async Task MaisDe10MilNumerosE201()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);

        var retorno = (await dfe.ExecutarAsync(Faixa(50, 1, 10_001), Ct)).Retorno!;

        Assert.Equal(201, retorno.Status);
    }

    [Fact]
    public async Task ANFCeInutilizaOModelo65()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);

        var retorno = (await dfe.ExecutarAsync(Faixa(50, 10, 15), Ct)).Retorno!;

        Assert.True(retorno.Homologada);
        Assert.Equal(Documento.NFCe, retorno.Modelo);
        Assert.Empty(SchemasNFe400.InutilizacaoProcessada.Validar(retorno.XmlProcessado!));
    }

    [Fact]
    public async Task ONucleoEmSvcInutilizaNoAutorizadorNormal()
    {
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", "NFeStatusServico4", Codigo: "109"));
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var svc = ApoioEmulado.Criar(_emulador, conexoes, emissao: TipoEmissao.ContingenciaSvcAN);

        var resultado = await svc.ExecutarAsync(Faixa(50, 10, 15), Ct);

        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.True(resultado.Retorno!.Homologada);
    }
}
