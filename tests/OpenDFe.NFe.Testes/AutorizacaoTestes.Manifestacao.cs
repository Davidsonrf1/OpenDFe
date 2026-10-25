using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A manifestação do destinatário (etapa 8.2, CONTRATOS.md 24), pelo ExecutarAsync com o handler que responde conforme o pedido:
// os quatro eventos, a montagem do envEvento (assinado, cOrgao 91, autor CNPJ e CPF, Id, descEvento, xJust), o destino sempre
// no AN, o lote, as recusas antes de transmitir e a leitura do retorno. Contra a SEFAZ emulada, em ManifestacaoEmuladaTestes.
public partial class AutorizacaoTestes
{
    private const string Destinatario = "11444777000161";
    private const string JustificativaNaoRealizada = "Mercadoria devolvida ao transportador";

    private static IEnumerable<Manifestacao> Manifestacoes(ChaveAcesso chave, string autor = Destinatario, int sequencia = 1) =>
    [
        new CienciaOperacao(chave, autor),
        new ConfirmacaoOperacao(chave, autor, sequencia),
        new DesconhecimentoOperacao(chave, autor, sequencia),
        new OperacaoNaoRealizada(chave, autor, JustificativaNaoRealizada, sequencia),
    ];

    [Fact]
    public void OsParametrosDaManifestacaoRecusamOQueASefazRejeitaria()
    {
        var chave = ChaveNota();
        var nfce = ChaveAcesso.Ler(Chave(NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()))));

        Assert.Throws<ArgumentNullException>(() => new CienciaOperacao(null!, Destinatario));
        Assert.Throws<ArgumentNullException>(() => new ConfirmacaoOperacao(null!, Destinatario));
        Assert.Throws<ArgumentNullException>(() => new DesconhecimentoOperacao(null!, Destinatario));
        Assert.Throws<ArgumentNullException>(() => new OperacaoNaoRealizada(null!, Destinatario, JustificativaNaoRealizada));
        // a manifestação só existe para a NF-e (modelo 55)
        Assert.Throws<ArgumentException>(() => new CienciaOperacao(nfce, Destinatario));
        Assert.Throws<ArgumentException>(() => new ConfirmacaoOperacao(nfce, Destinatario));
        Assert.Throws<ArgumentException>(() => new DesconhecimentoOperacao(nfce, Destinatario));
        Assert.Throws<ArgumentException>(() => new OperacaoNaoRealizada(nfce, Destinatario, JustificativaNaoRealizada));
        // o autor: nulo, vazio, DV errado, tamanho que não é de CNPJ nem de CPF
        foreach (var autor in new string?[] { null, "", "11444777000162", "5299822472", "529.982.247-26", "abc" })
        {
            Assert.Throws<ArgumentException>(() => new CienciaOperacao(chave, autor!));
            Assert.Throws<ArgumentException>(() => new ConfirmacaoOperacao(chave, autor!));
            Assert.Throws<ArgumentException>(() => new DesconhecimentoOperacao(chave, autor!));
            Assert.Throws<ArgumentException>(() => new OperacaoNaoRealizada(chave, autor!, JustificativaNaoRealizada));
        }
        Assert.Throws<ArgumentException>(() => new OperacaoNaoRealizada(chave, Destinatario, null!));
        Assert.Throws<ArgumentException>(() => new OperacaoNaoRealizada(chave, Destinatario, ""));
        // a sequência vai de 1 a 2 (a segunda é a retificação)
        foreach (var sequencia in new[] { 0, 3, -1 })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ConfirmacaoOperacao(chave, Destinatario, sequencia));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DesconhecimentoOperacao(chave, Destinatario, sequencia));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OperacaoNaoRealizada(chave, Destinatario, JustificativaNaoRealizada, sequencia));
        }
    }

    [Fact]
    public void ASequenciaETodosOsDadosDaManifestacaoFicamNasPropriedades()
    {
        var chave = ChaveNota();

        var ciencia = new CienciaOperacao(chave, "11.444.777/0001-61");
        var confirmacao = new ConfirmacaoOperacao(chave, "529.982.247-25", 2);
        var naoRealizada = new OperacaoNaoRealizada(chave, Destinatario, JustificativaNaoRealizada);

        Assert.Equal((TipoEvento.CienciaOperacao, 1, Destinatario), (ciencia.Tipo, ciencia.Sequencia, ciencia.Autor));
        Assert.Equal((TipoEvento.ConfirmacaoOperacao, 2, "52998224725"), (confirmacao.Tipo, confirmacao.Sequencia, confirmacao.Autor));
        Assert.Equal((TipoEvento.OperacaoNaoRealizada, 1, JustificativaNaoRealizada), (naoRealizada.Tipo, naoRealizada.Sequencia, naoRealizada.Justificativa));
        Assert.Equal((TipoEvento.DesconhecimentoOperacao, 1), (new DesconhecimentoOperacao(chave, Destinatario).Tipo, new DesconhecimentoOperacao(chave, Destinatario).Sequencia));
        Assert.All<Evento>([ciencia, confirmacao, naoRealizada], e => Assert.Same(chave, e.Chave));
    }

    [Fact]
    public void AManifestacaoValeSoNaNFe()
    {
        using var conexoes = new ConexoesSefaz();
        var nfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));

        Assert.True(nfe.Suporta<EnvioEvento>());
    }

    [Theory]
    [InlineData(TipoEvento.CienciaOperacao, "210210", "Ciencia da Operacao", false)]
    [InlineData(TipoEvento.ConfirmacaoOperacao, "210200", "Confirmacao da Operacao", false)]
    [InlineData(TipoEvento.DesconhecimentoOperacao, "210220", "Desconhecimento da Operacao", false)]
    [InlineData(TipoEvento.OperacaoNaoRealizada, "210240", "Operacao nao Realizada", true)]
    public async Task CadaManifestacaoSaiNoSchemaAssinadaComOrgao91OAutorEOId(TipoEvento tipo, string codigo, string descricao, bool justificativa)
    {
        var chave = ChaveNota();
        var evento = Manifestacoes(chave).Single(e => e.Tipo == tipo);

        var (resultado, envio) = await Executar(new EnvioEvento(evento) { Lote = 9 }, RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(Destino.AN, resultado.Rastro.Autorizador);
        Assert.Contains("NFeRecepcaoEvento4", envio!.Url.AbsolutePath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("hom1.nfe.fazenda.gov.br", envio.Url.Host);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio.Mensagem));
        var raiz = XElement.Parse(envio.Mensagem);
        XNamespace ns = NamespaceNFe;
        var informacao = raiz.Element(ns + "evento")!.Element(ns + "infEvento")!;
        Assert.Equal($"ID{codigo}{chave}01", (string?)informacao.Attribute("Id"));
        Assert.Equal(["91", "2", Destinatario, chave.ToString(), "2026-10-20T15:30:00-03:00", codigo, "1", "1.00"],
            informacao.Elements().Where(e => e.Name != ns + "detEvento").Select(e => e.Value));
        Assert.Equal("CNPJ", informacao.Elements().ElementAt(2).Name.LocalName);
        var detalhe = informacao.Element(ns + "detEvento")!;
        Assert.Equal("1.00", (string?)detalhe.Attribute("versao"));
        Assert.Equal(justificativa ? new[] { descricao, JustificativaNaoRealizada } : new[] { descricao }, detalhe.Elements().Select(e => e.Value));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID{codigo}{chave}01", NotasTeste.CnpjEmitente));
        Assert.Equal("Signature", raiz.Element(ns + "evento")!.Elements().Last().Name.LocalName);
    }

    [Fact]
    public async Task OAutorCpfVaiComoCpfEOCnpjMascaradoSaiSemMascara()
    {
        var chave = ChaveNota();

        var (cpf, envioCpf) = await Executar(new EnvioEvento(new CienciaOperacao(chave, "529.982.247-25")), RetornoEventos());
        var (cnpj, envioCnpj) = await Executar(new EnvioEvento(new CienciaOperacao(chave, "11.444.777/0001-61")), RetornoEventos());

        Assert.True(cpf.Processado && cnpj.Processado);
        Assert.Contains("<CPF>52998224725</CPF>", envioCpf!.Mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("<CNPJ>", envioCpf.Mensagem, StringComparison.Ordinal);
        Assert.Empty(SchemasNFe400.Evento.Validar(envioCpf.Mensagem));
        Assert.Contains($"<CNPJ>{Destinatario}</CNPJ>", envioCnpj!.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ARetificacaoVaiComSequencia2NoIdENoNSeqEvento()
    {
        var chave = ChaveNota();

        var (resultado, envio) = await Executar(new EnvioEvento(new ConfirmacaoOperacao(chave, Destinatario, 2)), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Contains($"Id=\"ID210200{chave}02\"", envio!.Mensagem, StringComparison.Ordinal);
        Assert.Contains("<nSeqEvento>2</nSeqEvento>", envio.Mensagem, StringComparison.Ordinal);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio.Mensagem));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID210200{chave}02", NotasTeste.CnpjEmitente));
        Assert.Equal(2, Assert.Single(resultado.Retorno!.Eventos).Evento!.Sequencia);
    }

    [Fact]
    public async Task OMomentoInformadoVaiNoDhEvento()
    {
        var momento = new DateTimeOffset(2026, 10, 20, 15, 0, 7, TimeSpan.FromHours(-3));

        var (_, envio) = await Executar(new EnvioEvento(new CienciaOperacao(ChaveNota(), Destinatario) { Momento = momento }), RetornoEventos());

        Assert.Contains("<dhEvento>2026-10-20T15:00:07-03:00</dhEvento>", envio!.Mensagem, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(14)]
    [InlineData(256)]
    public async Task AJustificativaDaOperacaoNaoRealizadaForaDoSchemaRecusaCom493(int tamanho)
    {
        var evento = new OperacaoNaoRealizada(ChaveNota(), Destinatario, new string('a', tamanho));

        var (resultado, envio) = await Executar(new EnvioEvento(evento), RetornoEventos());

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("493", Severidade.Erro, "xJust", "Eventos[0].Justificativa"), (problema.Codigo, problema.Severidade, problema.Tag, problema.Propriedade));
        Assert.Equal("envEvento/evento/infEvento/detEvento/xJust", problema.Caminho);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(255)]
    public async Task AJustificativaNosLimitesDoSchemaPassa(int tamanho)
    {
        var evento = new OperacaoNaoRealizada(ChaveNota(), Destinatario, new string('a', tamanho));

        var (resultado, envio) = await Executar(new EnvioEvento(evento), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.NotNull(envio);
    }

    [Fact]
    public async Task OCaractereQueOXmlNaoAceitaNaJustificativaERecusadoSemExcecao()
    {
        var evento = new OperacaoNaoRealizada(ChaveNota(), Destinatario, "Mercadoria \u0001 devolvida ao transportador");

        var (resultado, envio) = await Executar(new EnvioEvento(evento), RetornoEventos());

        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-CARACTERE", "Eventos[0].Justificativa"), (problema.Codigo, problema.Propriedade));
        Assert.DoesNotContain("devolvida", problema.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AManifestacaoVaiAoAnMesmoComAChaveDeSvcENaoAceitaODestinoDeOutroLugar()
    {
        var svc = ChaveNota(1, TipoEmissao.ContingenciaSvcAN);
        var svcRs = ChaveNota(2, TipoEmissao.ContingenciaSvcRS);

        var (daSvc, envioSvc) = await Executar(new EnvioEvento(new CienciaOperacao(svc, Destinatario)), RetornoEventos());
        var (daSvcRs, _) = await Executar(new EnvioEvento(new ConfirmacaoOperacao(svcRs, Destinatario)), RetornoEventos());

        Assert.Equal((Destino.AN, Destino.AN), (daSvc.Rastro.Autorizador, daSvcRs.Rastro.Autorizador));
        Assert.Contains("<cOrgao>91</cOrgao>", envioSvc!.Mensagem, StringComparison.Ordinal);
        Assert.Equal("hom1.nfe.fazenda.gov.br", daSvc.Rastro.Url!.Host);
    }

    [Fact]
    public async Task UmLoteDeDuasManifestacoesDeNotasDiferentesVaiJuntoCadaUmaAssinada()
    {
        var eventos = new Evento[] { new CienciaOperacao(ChaveNota(1), Destinatario), new ConfirmacaoOperacao(ChaveNota(2), Destinatario) };

        var (resultado, envio) = await Executar(new EnvioEvento(eventos), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(2, resultado.Retorno!.Eventos.Count);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Count(envio!.Mensagem, "<Signature "));
        Assert.Empty(SchemasNFe400.Evento.Validar(envio.Mensagem));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID210210{eventos[0].Chave}01", NotasTeste.CnpjEmitente));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID210200{eventos[1].Chave}01", NotasTeste.CnpjEmitente));
    }

    [Fact]
    public async Task AManifestacaoMisturadaComOCancelamentoNoMesmoLoteENFeLote()
    {
        var eventos = new Evento[] { new CienciaOperacao(ChaveNota(1), Destinatario), Cancelar(ChaveNota(2)) };
        var contrario = new Evento[] { Cancelar(ChaveNota(2)), new CienciaOperacao(ChaveNota(1), Destinatario) };

        var (mistura, envio) = await Executar(new EnvioEvento(eventos), RetornoEventos());
        var (inversa, _) = await Executar(new EnvioEvento(contrario), RetornoEventos());

        Assert.Null(envio);
        var problema = Assert.Single(mistura.Problemas);
        Assert.Equal(("NFE-LOTE", "Eventos[1].Chave"), (problema.Codigo, problema.Propriedade));
        Assert.Equal("NFE-LOTE", Assert.Single(inversa.Problemas).Codigo);
    }

    [Fact]
    public async Task OMesmoEventoDuasVezesNoLoteENFeLote()
    {
        var eventos = new Evento[] { new CienciaOperacao(ChaveNota(1), Destinatario), new CienciaOperacao(ChaveNota(1), Destinatario) };

        var (resultado, envio) = await Executar(new EnvioEvento(eventos), RetornoEventos());

        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-LOTE", "Eventos[1]"), (problema.Codigo, problema.Propriedade));
    }

    [Fact]
    public async Task ANFeNumNucleoDeNFCeERecusadaComNFeChave()
    {
        var (resultado, envio) = await Executar(new EnvioEvento(new CienciaOperacao(ChaveNota(), Destinatario)), RetornoEventos(), Documento.NFCe);

        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-CHAVE", "envEvento/evento/infEvento/chNFe", "Eventos[0].Chave"), (problema.Codigo, problema.Caminho, problema.Propriedade));
    }

    [Fact]
    public async Task OProcEventoDaManifestacaoRegistradaPassaNoSchemaECarregaAAssinatura()
    {
        var chave = ChaveNota();
        var evento = new CienciaOperacao(chave, Destinatario);

        var (resultado, envio) = await Executar(new EnvioEvento(evento), RetornoEventos());

        var registrado = Assert.Single(resultado.Retorno!.Eventos);
        Assert.Equal((135, true, evento.Chave), (registrado.Status, registrado.Registrado, registrado.Chave));
        Assert.Same(evento, registrado.Evento);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(registrado.XmlProcessado!));
        Assert.Contains(registrado.XmlEvento!, registrado.XmlProcessado, StringComparison.Ordinal);
        Assert.Equal(OperacaoEventoApoio.Enviados(envio!.Mensagem)[0], registrado.XmlEvento);
        Assert.Empty(AssinaturaDigital.Verificar(registrado.XmlProcessado!, $"ID210210{chave}01", NotasTeste.CnpjEmitente));
    }

    [Theory]
    [InlineData(136, true)]
    [InlineData(575, false)]
    [InlineData(596, false)]
    [InlineData(655, false)]
    [InlineData(650, false)]
    [InlineData(494, false)]
    public async Task ORetornoDaManifestacaoTrazOStatusEAindicacaoDeRegistro(int status, bool registrado)
    {
        var (resultado, _) = await Executar(new EnvioEvento(new ConfirmacaoOperacao(ChaveNota(), Destinatario)), RetornoEventos(status));

        var e = Assert.Single(resultado.Retorno!.Eventos);
        Assert.Equal((status, registrado), (e.Status, e.Registrado));
        Assert.Equal(registrado, e.XmlProcessado is not null);
        Assert.NotNull(e.XmlEvento);
    }

    [Theory]
    [InlineData(215)]
    [InlineData(516)]
    [InlineData(108)]
    public async Task OLoteDeManifestacaoRejeitadoNaoTemEventos(int status)
    {
        var (resultado, _) = await Executar(new EnvioEvento(new CienciaOperacao(ChaveNota(), Destinatario)), RetornoEventos(lote: status));

        Assert.Equal(status, resultado.CStat);
        Assert.Empty(resultado.Retorno!.Eventos);
    }
}
