using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;
using ConsultaProtocolo = OpenDFe.NFe.ConsultaProtocolo;

namespace OpenDFe.Testes;

/// <summary>
/// A manifestação do destinatário pela lib inteira contra a SEFAZ emulada (etapa 8.2), com núcleos de CNPJ diferente, cada um
/// com o seu certificado autoassinado: o emitente A autoriza a NF-e para o destinatário B, que manifesta no Ambiente Nacional.
/// Cobre os quatro eventos, a liberação do <c>nfeProc</c> na distribuição (8.1), o <c>procEventoNFe</c> que chega ao
/// emitente, o cancelamento depois da manifestação (221), a retificação, as rejeições (575, 494, 650, 651, 655, 596), a consulta
/// da nota, a conexão derrubada depois de processar e o lote. O 594 da ciência com sequência 2 não sai da lib (o construtor
/// não deixa): o emulador o prova no <c>SefazEmulada.Testes</c>. O repositório do emulador é limpo a cada teste.
/// </summary>
public sealed class ManifestacaoEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private const string CnpjA = NotasTeste.CnpjEmitente; // 11222333000181: o emitente
    private const string CnpjB = "11444777000161"; // o destinatário da nota de teste
    private const string CnpjC = "45723174000110"; // um terceiro
    private const string CpfD = "52998224725";
    private const string Justificativa = "Cancelamento por erro de digitacao";
    private const string NaoRealizada = "Mercadoria devolvida ao transportador";

    private static readonly Dictionary<string, X509Certificate2> Certificados = [];

    private readonly Emulador _emulador;

    public ManifestacaoEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static X509Certificate2 Certificado(string documento)
    {
        lock (Certificados)
        {
            if (!Certificados.TryGetValue(documento, out var certificado))
            {
                var teste = documento.Length == 11 ? new CertificadoTeste { Cpf = documento } : new CertificadoTeste { Cnpj = documento };
                Certificados[documento] = certificado = CertificadoDigital.AbrirPfx(teste.GerarPfx("teste"), "teste");
            }
            return certificado;
        }
    }

    private static DFe Criar(Emulador emulador, ConexoesSefaz conexoes, string documento, TimeProvider? relogio = null) =>
        DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado(documento),
            Conexoes = conexoes,
            Relogio = relogio ?? TimeProvider.System,
            Urls = ApoioEmulado.Urls(emulador, Documento.NFe),
        });

    private DFe Criar(ConexoesSefaz conexoes, string documento) => Criar(_emulador, conexoes, documento);

    private static Task<ResultadoNota> Autorizar(DFe emitente, int numero, TimeProvider? relogio = null, Action<NotaFiscal>? ajustar = null) =>
        ApoioEmulado.Autorizar(emitente, Documento.NFe, numero, relogio, ajustar);

    private static ResultadoEvento Unico(ResultadoWS<RetornoEvento> resultado)
    {
        Assert.True(resultado.Processado);
        Assert.Equal(128, resultado.CStat);
        return Assert.Single(resultado.Retorno!.Eventos);
    }

    // A manifestação vai ao Ambiente Nacional; o cancelamento, ao autorizador da UF da chave.
    private static async Task<ResultadoEvento> Enviar(DFe dfe, Evento evento)
    {
        var resultado = await dfe.ExecutarAsync(new EnvioEvento(evento), Ct);
        Assert.Equal(evento is Manifestacao ? Destino.AN : Destino.MG, resultado.Rastro.Autorizador);
        return Unico(resultado);
    }

    private static async Task<RetornoDistribuicao> Distribuir(DFe dfe, DistribuicaoDFe consulta)
    {
        var resultado = await dfe.ExecutarAsync(consulta, Ct);
        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        return resultado.Retorno;
    }

    private static Cancelamento Cancelar(ResultadoNota nota) => new(nota.Chave!, nota.Protocolo!.Numero!, Justificativa);

    // A ciência libera o download

    [Fact]
    public async Task AAscienciaLiberaONfeProcNaDistribuicaoDoDestinatario()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);
        var antes = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));
        Assert.Equal(TipoDocumentoDistribuido.ResumoNFe, Assert.Single(antes.Documentos).Tipo);

        var ciencia = await Enviar(destinatario, new CienciaOperacao(nota.Chave!, CnpjB));

        Assert.Equal((135, true, nota.Chave), (ciencia.Status, ciencia.Registrado, ciencia.Chave));
        Assert.Equal("Ciencia da Operacao registrada", ciencia.Descricao);
        Assert.StartsWith("291", ciencia.Protocolo, StringComparison.Ordinal);
        Assert.NotNull(ciencia.Registro);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(ciencia.XmlProcessado!));
        Assert.Empty(AssinaturaDigital.Verificar(ciencia.XmlProcessado!, $"ID210210{nota.Chave}01", CnpjB));

        var depois = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, antes.UltimoNsu!.Value));
        var liberado = Assert.Single(depois.Documentos);
        Assert.Equal((TipoDocumentoDistribuido.NFeProcessada, nota.Chave), (liberado.Tipo, liberado.Chave));
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(liberado.Xml));
        var pontual = await Distribuir(destinatario, DistribuicaoDFe.Chave(CnpjB, nota.Chave!));
        Assert.Equal(TipoDocumentoDistribuido.NFeProcessada, Assert.Single(pontual.Documentos).Tipo);
    }

    [Fact]
    public async Task OEmitenteRecebeAManifestacaoComoProcEventoNFeNaDistribuicaoDele()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);
        await Enviar(destinatario, new CienciaOperacao(nota.Chave!, CnpjB));

        var confirmacao = await Enviar(destinatario, new ConfirmacaoOperacao(nota.Chave!, CnpjB));
        var recebido = await Distribuir(emitente, DistribuicaoDFe.UltimoNsu(CnpjA, 0));

        Assert.Equal((135, "Confirmacao de Operacao registrada"), (confirmacao.Status, confirmacao.Descricao));
        Assert.Equal(138, recebido.Status);
        Assert.Equal(2, recebido.Documentos.Count);
        Assert.All(recebido.Documentos, d =>
        {
            Assert.Equal((TipoDocumentoDistribuido.EventoProcessado, nota.Chave), (d.Tipo, d.Chave));
            Assert.Empty(SchemasNFe400.EventoProcessado.Validar(d.Xml));
        });
        Assert.Contains("<tpEvento>210210</tpEvento>", recebido.Documentos[0].Xml, StringComparison.Ordinal);
        Assert.Contains("<tpEvento>210200</tpEvento>", recebido.Documentos[1].Xml, StringComparison.Ordinal);
        Assert.Contains($"<CNPJ>{CnpjB}</CNPJ>", recebido.Documentos[1].Xml, StringComparison.Ordinal);
    }

    // A confirmação impede o cancelamento, e a retificação

    [Fact]
    public async Task ADepoisDaConfirmacaoOEmitenteNaoCancelaE221MesmoDepoisDaRetificacao()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);
        Assert.Equal(135, (await Enviar(destinatario, new ConfirmacaoOperacao(nota.Chave!, CnpjB))).Status);

        var cancelamento = await Enviar(emitente, Cancelar(nota));
        var retificacao = await Enviar(destinatario, new ConfirmacaoOperacao(nota.Chave!, CnpjB, 2));
        var outraVez = await Enviar(emitente, Cancelar(nota));

        Assert.Equal((221, false), (cancelamento.Status, cancelamento.Registrado));
        Assert.Equal((135, true, 2), (retificacao.Status, retificacao.Registrado, retificacao.Evento!.Sequencia));
        Assert.Empty(AssinaturaDigital.Verificar(retificacao.XmlProcessado!, $"ID210200{nota.Chave}02", CnpjB));
        Assert.Equal(221, outraVez.Status);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConfirmacaoOperacao(nota.Chave!, CnpjB, 3));
        Assert.Equal(100, (await emitente.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct)).CStat);
    }

    [Fact]
    public async Task ODesconhecimentoDeixaOEmitenteCancelarEODestinatarioVeOCancelamento()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);

        var desconhecimento = await Enviar(destinatario, new DesconhecimentoOperacao(nota.Chave!, CnpjB));
        var cancelamento = await Enviar(emitente, Cancelar(nota));
        var recebido = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal((135, "Desconhecimento da Operacao registrada"), (desconhecimento.Status, desconhecimento.Descricao));
        Assert.Equal((135, true), (cancelamento.Status, cancelamento.Registrado));
        // o desconhecimento não libera o download: o destinatário recebeu o resumo da nota e o resumo do cancelamento
        Assert.Equal([TipoDocumentoDistribuido.ResumoNFe, TipoDocumentoDistribuido.ResumoEvento], recebido.Documentos.Select(d => d.Tipo));
        Assert.Equal(101, (await emitente.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct)).CStat);
    }

    [Fact]
    public async Task AOperacaoNaoRealizadaComJustificativaLiberaODownloadEOEmitentePodeCancelar()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);

        var evento = await Enviar(destinatario, new OperacaoNaoRealizada(nota.Chave!, CnpjB, NaoRealizada));
        var cancelamento = await Enviar(emitente, Cancelar(nota));
        var recebido = await Distribuir(destinatario, DistribuicaoDFe.UltimoNsu(CnpjB, 0));

        Assert.Equal((135, "Operacao nao Realizada registrada"), (evento.Status, evento.Descricao));
        Assert.Contains(NaoRealizada, evento.XmlEvento, StringComparison.Ordinal);
        Assert.Equal(135, cancelamento.Status);
        Assert.Equal(TipoDocumentoDistribuido.ResumoNFe, recebido.Documentos[0].Tipo);
        Assert.Contains(recebido.Documentos, d => d.Tipo == TipoDocumentoDistribuido.NFeProcessada);
        Assert.Contains(recebido.Documentos, d => d is { Tipo: TipoDocumentoDistribuido.EventoProcessado } && d.Xml.Contains("<tpEvento>110111</tpEvento>", StringComparison.Ordinal));
    }

    [Fact]
    public async Task OAutorCpfManifestaComOCertificadoDoCpf()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var nota = await Autorizar(emitente, 1, ajustar: n =>
        {
            n.Destinatario!.Cnpj = null;
            n.Destinatario.Cpf = CpfD;
            n.Destinatario.IndicadorIE = IndicadorIEDestinatario.NaoContribuinte;
            n.Destinatario.InscricaoEstadual = null;
            n.Identificacao.ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal;
            n.Transporte.Transportador!.Cnpj = CnpjC;
            n.AutorizadosXml.Clear();
        });

        var ciencia = await Enviar(Criar(conexoes, CpfD), new CienciaOperacao(nota.Chave!, "529.982.247-25"));

        Assert.Equal(135, ciencia.Status);
        Assert.Contains($"<CPF>{CpfD}</CPF>", ciencia.XmlEvento, StringComparison.Ordinal);
    }

    // As rejeições

    [Fact]
    public async Task AAscienciaDepoisDeUmaManifestacaoConclusivaE655()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(Criar(conexoes, CnpjA), 1);
        Assert.Equal(135, (await Enviar(destinatario, new ConfirmacaoOperacao(nota.Chave!, CnpjB))).Status);

        var ciencia = await Enviar(destinatario, new CienciaOperacao(nota.Chave!, CnpjB));

        Assert.Equal((655, false), (ciencia.Status, ciencia.Registrado));
        Assert.Null(ciencia.XmlProcessado);
    }

    [Fact]
    public async Task OAutorQueNaoEODestinatarioDaNotaE575()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var nota = await Autorizar(Criar(conexoes, CnpjA), 1);

        var evento = await Enviar(Criar(conexoes, CnpjC), new CienciaOperacao(nota.Chave!, CnpjC));

        Assert.Equal((575, false), (evento.Status, evento.Registrado));
    }

    [Fact]
    public async Task ANotaQueNaoExisteE494()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var preparada = emitente.Preparar(ApoioEmulado.Nota(Documento.NFe, 9, DateTimeOffset.UtcNow));

        var evento = await Enviar(Criar(conexoes, CnpjB), new CienciaOperacao(preparada.Chave!, CnpjB));

        Assert.Equal((494, false), (evento.Status, evento.Registrado));
    }

    [Fact]
    public async Task ANotaCanceladaRecusaACienciaE650EODesconhecimentoE651MasAceitaAConfirmacaoEANaoRealizada()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);
        Assert.Equal(135, (await Enviar(emitente, Cancelar(nota))).Status);

        var ciencia = await Enviar(destinatario, new CienciaOperacao(nota.Chave!, CnpjB));
        var desconhecimento = await Enviar(destinatario, new DesconhecimentoOperacao(nota.Chave!, CnpjB));
        var confirmacao = await Enviar(destinatario, new ConfirmacaoOperacao(nota.Chave!, CnpjB));
        var naoRealizada = await Enviar(destinatario, new OperacaoNaoRealizada(nota.Chave!, CnpjB, NaoRealizada));

        Assert.Equal(650, ciencia.Status);
        Assert.Equal(651, desconhecimento.Status);
        Assert.Equal((135, 135), (confirmacao.Status, naoRealizada.Status));
    }

    [Fact]
    public async Task OsPrazosDe10DiasDaCienciaEDe90DiasDasConclusivasSaoDoRelogioDoEmulador()
    {
        var relogio = new ApoioEmulado.RelogioAvancavel();
        await using var emulador = await Emulador.IniciarAsync(new OpcoesEmulador
        {
            Porta = 0,
            Pasta = Path.Combine(Path.GetTempPath(), "opendfe-emulada-" + Guid.NewGuid().ToString("N")),
            Relogio = relogio,
        }, Ct);
        using var conexoes = ApoioEmulado.Conexoes(emulador);
        var emitente = Criar(emulador, conexoes, CnpjA, relogio);
        var destinatario = Criar(emulador, conexoes, CnpjB, relogio);
        var nota = await Autorizar(emitente, 1, relogio);

        relogio.Avancar(TimeSpan.FromDays(11));
        var ciencia = await Enviar(destinatario, new CienciaOperacao(nota.Chave!, CnpjB));
        var confirmacao = await Enviar(destinatario, new ConfirmacaoOperacao(nota.Chave!, CnpjB));
        relogio.Avancar(TimeSpan.FromDays(78));
        var noPrazo = await Enviar(destinatario, new ConfirmacaoOperacao(nota.Chave!, CnpjB, 2));
        relogio.Avancar(TimeSpan.FromDays(2));
        var tarde = await Enviar(destinatario, new DesconhecimentoOperacao(nota.Chave!, CnpjB));

        Assert.Equal((596, false), (ciencia.Status, ciencia.Registrado));
        Assert.Contains("10 dias", ciencia.Motivo, StringComparison.Ordinal);
        Assert.Equal(135, confirmacao.Status);
        Assert.Equal(135, noPrazo.Status);
        Assert.Equal(596, tarde.Status);
        Assert.Contains("90 dias", tarde.Motivo, StringComparison.Ordinal);
    }

    // A consulta, o lote e a queda

    [Fact]
    public async Task AConsultaDaNotaTrazOProcEventoNFeDaManifestacao()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var nota = await Autorizar(emitente, 1);
        var ciencia = await Enviar(Criar(conexoes, CnpjB), new CienciaOperacao(nota.Chave!, CnpjB));

        var consulta = await emitente.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);

        Assert.Equal(100, consulta.CStat);
        var evento = Assert.Single(consulta.Retorno!.Eventos);
        Assert.Contains("<tpEvento>210210</tpEvento>", evento, StringComparison.Ordinal);
        Assert.Contains(ciencia.Protocolo!, evento, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UmLoteComDuasManifestacoesDeNotasDiferentesRegistraAsDuas()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var primeira = await Autorizar(emitente, 1);
        var segunda = await Autorizar(emitente, 2);

        var resultado = await destinatario.ExecutarAsync(new EnvioEvento(
            new CienciaOperacao(primeira.Chave!, CnpjB), new ConfirmacaoOperacao(segunda.Chave!, CnpjB)), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(128, resultado.CStat);
        Assert.Equal(91, resultado.Retorno!.Orgao);
        Assert.Equal([135, 135], resultado.Retorno.Eventos.Select(e => e.Status));
        Assert.Equal([primeira.Chave, segunda.Chave], resultado.Retorno.Eventos.Select(e => e.Chave));
    }

    [Fact]
    public async Task AQuedaDepoisDeProcessarSeResolvePelaConsultaEPeloReenvioDoMesmoEventoE573()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var emitente = Criar(conexoes, CnpjA);
        var destinatario = Criar(conexoes, CnpjB);
        var nota = await Autorizar(emitente, 1);
        var evento = new CienciaOperacao(nota.Chave!, CnpjB) { Momento = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3)) };
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "AN", "NFeRecepcaoEvento4", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => destinatario.ExecutarAsync(new EnvioEvento(evento), Ct));
        var consulta = await emitente.ExecutarAsync(new ConsultaProtocolo(nota.Chave!), Ct);
        var reenvio = await Enviar(destinatario, evento);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Contains("210210", Assert.Single(consulta.Retorno!.Eventos), StringComparison.Ordinal);
        Assert.Equal((573, false), (reenvio.Status, reenvio.Registrado));
        Assert.Equal(OperacaoEventoApoio.Enviados(falha.Rastro.XmlPedido!)[0], reenvio.XmlEvento);
    }
}
