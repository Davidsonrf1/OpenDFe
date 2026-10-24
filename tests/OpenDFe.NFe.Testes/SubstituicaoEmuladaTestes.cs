using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;
using ConsultaProtocolo = OpenDFe.NFe.ConsultaProtocolo;

namespace OpenDFe.Testes;

/// <summary>
/// O cancelamento por substituição da NFC-e pela lib inteira contra a SEFAZ emulada (etapa 6.2): a NFC-e normal e a
/// substituta emitida offline (tpEmis 9, MOC 5P31-44), autorizadas, e a primeira cancelada pela segunda (135, o
/// <c>procEventoNFe</c> com o <c>cOrgaoAutor</c> e a consulta com o evento), e cada rejeição da Tabela 5-38 que a lib
/// pode provocar: a substituta inexistente (912), cancelada (913), emitida mais de 2 horas depois (914), de outro valor
/// (915), de outro destinatário (917), de outro item (919), de outra UF ou de outro emitente (911), da emissão normal
/// (921), a janela de 168 horas (501), a nota em emissão offline (920) e a repetição (573 e 580).
/// </summary>
public sealed class SubstituicaoEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private const string Justificativa = "Substituicao por erro de digitacao";

    private readonly Emulador _emulador;

    public SubstituicaoEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static CancelamentoSubstituicao Substituir(ResultadoNota cancelada, ChaveAcesso substituta) =>
        new(cancelada.Chave!, cancelada.Protocolo!.Numero!, Justificativa, substituta);

    private static ResultadoEvento Unico(ResultadoWS<RetornoEvento> resultado)
    {
        Assert.True(resultado.Processado);
        Assert.Equal(128, resultado.CStat);
        return Assert.Single(resultado.Retorno!.Eventos);
    }

    private static async Task<ResultadoEvento> Enviar(DFe dfe, Evento evento) =>
        Unico(await dfe.ExecutarAsync(new EnvioEvento(evento), Ct));

    private static ChaveAcesso ChaveDe(DFe dfe, int numero, DateTimeOffset agora) => dfe.Preparar(ApoioEmulado.Nota(Documento.NFCe, numero, agora)).Chave!;

    [Fact]
    public async Task ANFCeSubstituidaEDepoisDeAutorizadaASubstitutaE135ComOProcEventoNFeEAConsultaMostraOEvento()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2, offline: offline);
        var evento = Substituir(cancelada, substituta.Chave!);

        var resultado = await dfe.ExecutarAsync(new EnvioEvento(evento), Ct);

        Assert.Empty(resultado.Problemas);
        var registrado = Unico(resultado);
        Assert.True(registrado.Registrado);
        Assert.Equal(135, registrado.Status);
        Assert.Same(evento, registrado.Evento);
        Assert.Equal(cancelada.Chave, registrado.Chave);
        Assert.NotNull(registrado.Protocolo);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(registrado.XmlProcessado!));
        // o cOrgaoAutor do retEvento fica no XML processado (o ResultadoEvento não tem o campo)
        Assert.Contains("<cOrgaoAutor>31</cOrgaoAutor>", registrado.XmlProcessado, StringComparison.Ordinal);
        Assert.Contains(registrado.XmlEvento!, registrado.XmlProcessado, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(registrado.XmlProcessado!, $"ID110112{cancelada.Chave}01", NotasTeste.CnpjEmitente));

        var consulta = await dfe.ExecutarAsync(new ConsultaProtocolo(cancelada.Chave!), Ct);
        Assert.Equal(101, consulta.CStat);
        Assert.True(consulta.Retorno!.Cancelada);
        Assert.Contains("110112", Assert.Single(consulta.Retorno.Eventos), StringComparison.Ordinal);
        // A resposta da consulta não é validada pelo retConsSitNFe do PL_010d: o TProcEvento dele não tem o cOrgaoAutor
        // (SEFAZ_EMULADA.md, "Divergência entre os schemas do Portal"). O procEventoNFe é o que vale, e passou acima.
        var daSubstituta = await dfe.ExecutarAsync(new ConsultaProtocolo(substituta.Chave!), Ct);
        Assert.Equal(100, daSubstituta.CStat);
        Assert.False(daSubstituta.Retorno!.Cancelada);
    }

    [Fact]
    public async Task ASubstitutaDaEmissaoNormalE921()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2);
        Assert.Equal(TipoEmissao.Normal, substituta.Chave!.TipoEmissao);

        var evento = await Enviar(dfe, Substituir(cancelada, substituta.Chave!));

        // MOC 7.0, Tabela 5-38, 5P31-44: a substituta é a NFC-e emitida em contingência (offline)
        Assert.Equal((921, false), (evento.Status, evento.Registrado));
    }

    [Fact]
    public async Task ASubstitutaQueNaoExisteE912()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);

        var evento = await Enviar(dfe, Substituir(cancelada, ChaveDe(dfe, 9, DateTimeOffset.UtcNow)));

        Assert.Equal((912, false), (evento.Status, evento.Registrado));
        Assert.Null(evento.XmlProcessado);
    }

    [Fact]
    public async Task ASubstitutaCanceladaE913()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2, offline: offline);
        Assert.Equal(135, (await Enviar(dfe, new Cancelamento(substituta.Chave!, substituta.Protocolo!.Numero!, "Cancelamento da substituta"))).Status);

        var evento = await Enviar(dfe, Substituir(cancelada, substituta.Chave!));

        Assert.Equal(913, evento.Status);
    }

    [Fact]
    public async Task ASubstitutaEmitidaMaisDe2HorasDepoisE914()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Emulador.IniciarAsync(new OpcoesEmulador
        {
            Porta = 0,
            Pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N")),
            Relogio = relogio,
        }, Ct);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        var dfe = ApoioEmulado.Criar(emulador, conexoes, Documento.NFCe, relogio: relogio);
        var offline = ApoioEmulado.Criar(emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline, relogio);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1, relogio);
        relogio.Avancar(TimeSpan.FromHours(3));
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2, relogio, offline: offline);
        // a virada do mês entre as duas é o 911 (a substituta é do mês da cancelada ou do anterior), não o caso
        Assert.SkipUnless(substituta.Chave!.Mes == cancelada.Chave!.Mes, "A virada do mês entre a cancelada e a substituta muda o caso.");

        var evento = await Enviar(dfe, Substituir(cancelada, substituta.Chave!));

        Assert.Equal(914, evento.Status);
    }

    [Fact]
    public async Task ASubstitutaDeOutroValorE915()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2, offline: offline, ajustar: nota =>
        {
            var item = nota.Itens[0].Produto;
            (item.ValorUnitarioComercial, item.ValorUnitarioTributavel, item.Valor) = (20.00m, 20.00m, 40.00m);
            (nota.Total.Icms.ValorProdutos, nota.Total.Icms.ValorNota) = (40.00m, 40.00m);
            nota.Pagamento.Formas[0].Valor = 40.00m;
        });

        var evento = await Enviar(dfe, Substituir(cancelada, substituta.Chave!));

        Assert.Equal(915, evento.Status);
    }

    [Fact]
    public async Task ASubstitutaDeOutroDestinatarioE917()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2, offline: offline, ajustar: nota =>
            nota.Destinatario = new Destinatario { Cpf = "52998224725", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte });

        var evento = await Enviar(dfe, Substituir(cancelada, substituta.Chave!));

        Assert.Equal(917, evento.Status);
    }

    [Fact]
    public async Task ASubstitutaDeOutroItemE919()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2, offline: offline, ajustar: nota => nota.Itens[0].Produto.Descricao = "CAFE TORRADO 1KG");

        var evento = await Enviar(dfe, Substituir(cancelada, substituta.Chave!));

        Assert.Equal(919, evento.Status);
    }

    [Theory]
    [InlineData("UF", "Código da UF")]
    [InlineData("CNPJ", "CNPJ/CPF")]
    public async Task AChaveDaSubstitutaDeOutraUfOuDeOutroEmitenteE911(string caso, string campo)
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var agora = DateTimeOffset.UtcNow.AddMinutes(-1).ToOffset(TimeSpan.FromHours(-3));
        var substituta = caso == "UF"
            ? ChaveAcesso.Montar(UF.SP, agora, NotasTeste.CnpjEmitente, Documento.NFCe, 1, 9, TipoEmissao.Normal, 12345678)
            : ChaveAcesso.Montar(UF.MG, agora, "11444777000161", Documento.NFCe, 1, 9, TipoEmissao.Normal, 12345678);

        var evento = await Enviar(dfe, Substituir(cancelada, substituta));

        Assert.Equal(911, evento.Status);
        Assert.Contains(campo, evento.Motivo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DepoisDe168HorasASubstituicaoE501()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Emulador.IniciarAsync(new OpcoesEmulador
        {
            Porta = 0,
            Pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N")),
            Relogio = relogio,
        }, Ct);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        var dfe = ApoioEmulado.Criar(emulador, conexoes, Documento.NFCe, relogio: relogio);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1, relogio);
        // a chave da substituta é do mês da cancelada (o 501 vem antes da conferência da substituta na base)
        var substituta = ChaveDe(dfe, 2, relogio.GetUtcNow());

        relogio.Avancar(TimeSpan.FromHours(169));
        var evento = await Enviar(dfe, Substituir(cancelada, substituta));

        Assert.Equal((501, false), (evento.Status, evento.Registrado));
    }

    [Fact]
    public async Task ANotaEmEmissaoOfflineNaoTemSubstituicao920()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline);
        var normal = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var preparada = offline.Preparar(ApoioEmulado.Nota(Documento.NFCe, 1, DateTimeOffset.UtcNow));
        Assert.True(preparada.Preparado);
        var resultado = await normal.ExecutarAsync(new Autorizacao(preparada.Xml!), Ct);
        var cancelada = Assert.Single(resultado.Retorno!.Notas);
        Assert.True(cancelada.Autorizada);
        Assert.Equal(TipoEmissao.ContingenciaOffline, cancelada.Chave!.TipoEmissao);

        var evento = await Enviar(normal, Substituir(cancelada, ChaveDe(normal, 2, DateTimeOffset.UtcNow)));

        Assert.Equal((920, false), (evento.Status, evento.Registrado));
    }

    [Fact]
    public async Task ARepeticaoDaSubstituicaoE573EOCancelamentoDepoisDelaE580()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe);
        var offline = ApoioEmulado.Criar(_emulador, conexoes, Documento.NFCe, TipoEmissao.ContingenciaOffline);
        var cancelada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1);
        var substituta = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 2, offline: offline);
        var momento = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3));
        var evento = new CancelamentoSubstituicao(cancelada.Chave!, cancelada.Protocolo!.Numero!, Justificativa, substituta.Chave!) { Momento = momento };

        var primeiro = await Enviar(dfe, evento);
        var segundo = await Enviar(dfe, evento);
        var comum = await Enviar(dfe, new Cancelamento(cancelada.Chave!, cancelada.Protocolo.Numero!, "Cancelamento depois da substituicao"));

        Assert.Equal(135, primeiro.Status);
        Assert.Equal((573, false), (segundo.Status, segundo.Registrado));
        Assert.Equal(primeiro.XmlEvento, segundo.XmlEvento);
        Assert.Equal((580, false), (comum.Status, comum.Registrado));
    }
}
