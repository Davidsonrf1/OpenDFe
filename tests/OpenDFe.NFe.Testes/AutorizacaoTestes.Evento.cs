using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// Os eventos e a inutilização (etapa 6.1, CONTRATOS.md 18), pelo ExecutarAsync com o handler que responde conforme o
// pedido: a montagem do envEvento e do inutNFe, a assinatura de cada evento, as recusas antes de transmitir, o destino, a
// leitura do retEnvEvento e do retInutNFe e os XMLs processados. Contra a SEFAZ emulada, em EventoEmuladoTestes e
// InutilizacaoEmuladaTestes.
public partial class AutorizacaoTestes
{
    private const string ProtocoloNota = "131260000000001";
    private const string Justificativa = "Cancelamento por erro de digitacao";

    private static ChaveAcesso ChaveNota(int numero = 1, TipoEmissao emissao = TipoEmissao.Normal) =>
        ChaveAcesso.Ler(Chave(Assinada(Nota(numero), emissao)));

    private static Cancelamento Cancelar(ChaveAcesso chave, string justificativa = Justificativa) =>
        new(chave, ProtocoloNota, justificativa);

    // O retEnvEvento do pedido: um retEvento por evento, no status do caso (128 no lote).
    private static Func<string, string> RetornoEventos(int status = 135, int lote = 128, bool semChave = false) => pedido =>
    {
        var eventos = Regex.Matches(pedido, "<infEvento Id=\"ID(\\d{6})(\\d{44})(\\d{2})\"", RegexOptions.None)
            .Select(m => (Tipo: m.Groups[1].Value, Chave: m.Groups[2].Value, Sequencia: m.Groups[3].Value.TrimStart('0')));
        var retornos = string.Concat(eventos.Select(e => RetEvento(status, e.Tipo, e.Chave, e.Sequencia, semChave)));
        return RetEnvEvento(lote, lote == 128 ? "Lote de Evento Processado" : "Rejeicao do lote", lote == 128 ? retornos : "");
    };

    private static string RetEnvEvento(int status, string motivo, string retornos) =>
        "<retEnvEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\"><idLote>123</idLote><tpAmb>2</tpAmb>"
        + $"<verAplic>MG-TESTE</verAplic><cOrgao>31</cOrgao><cStat>{status}</cStat><xMotivo>{motivo}</xMotivo>{retornos}</retEnvEvento>";

    private static string RetEvento(int status, string tipo, string chave, string sequencia, bool semChave = false)
    {
        var registrado = status is 135 or 136 or 155;
        return "<retEvento versao=\"1.00\"><infEvento><tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic><cOrgao>31</cOrgao>"
            + $"<cStat>{status}</cStat><xMotivo>Motivo {status}</xMotivo>"
            + (semChave ? "" : $"<chNFe>{chave}</chNFe><tpEvento>{tipo}</tpEvento>")
            + (registrado ? "<xEvento>Cancelamento homologado</xEvento>" : "")
            + (semChave ? "" : $"<nSeqEvento>{sequencia}</nSeqEvento>")
            + $"<dhRegEvento>{Recebimento}</dhRegEvento>" + (registrado ? "<nProt>131260000000099</nProt>" : "")
            + "</infEvento></retEvento>";
    }

    // O suporte e os parâmetros

    [Fact]
    public void OsEventosEAInutilizacaoValemNosDoisDocumentos()
    {
        using var conexoes = new ConexoesSefaz();
        var nfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configuracao(conexoes));
        var nfce = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, Configuracao(conexoes));

        Assert.True(nfe.Suporta<EnvioEvento>());
        Assert.True(nfe.Suporta<Inutilizacao>());
        Assert.True(nfce.Suporta<EnvioEvento>());
        Assert.True(nfce.Suporta<Inutilizacao>());
    }

    [Fact]
    public void OsParametrosDoEventoRecusamOQueASefazRejeitaria()
    {
        var chave = ChaveNota();

        Assert.Throws<ArgumentNullException>(() => new Cancelamento(null!, ProtocoloNota, Justificativa));
        Assert.Throws<ArgumentException>(() => new Cancelamento(chave, "12345678901234", Justificativa));
        Assert.Throws<ArgumentException>(() => new Cancelamento(chave, "1234567890123456", Justificativa));
        Assert.Throws<ArgumentException>(() => new Cancelamento(chave, "13126000000000A", Justificativa));
        Assert.Throws<ArgumentException>(() => new Cancelamento(chave, null!, Justificativa));
        Assert.Throws<ArgumentException>(() => new Cancelamento(chave, ProtocoloNota, ""));
        Assert.Throws<ArgumentException>(() => new Cancelamento(chave, ProtocoloNota, null!));
        var cancelamento = Cancelar(chave);
        Assert.Equal((TipoEvento.Cancelamento, 1, ProtocoloNota, Justificativa), (cancelamento.Tipo, cancelamento.Sequencia, cancelamento.Protocolo, cancelamento.Justificativa));
        Assert.Same(chave, cancelamento.Chave);

        Assert.Throws<ArgumentException>(() => new EnvioEvento(Array.Empty<Evento>()));
        Assert.Throws<ArgumentException>(() => new EnvioEvento(Enumerable.Range(0, 21).Select(_ => Cancelar(chave))));
        Assert.Throws<ArgumentException>(() => new EnvioEvento(cancelamento, null!));
        Assert.Throws<ArgumentNullException>(() => new EnvioEvento((IEnumerable<Evento>)null!));
        Assert.Equal(20, new EnvioEvento(Enumerable.Range(0, 20).Select(_ => Cancelar(chave))).Eventos.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnvioEvento(cancelamento) { Lote = 0 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnvioEvento(cancelamento) { Lote = 1_000_000_000_000_000 });
    }

    [Fact]
    public void OsParametrosDaInutilizacaoRecusamOQueASefazRejeitaria()
    {
        const string cnpj = NotasTeste.CnpjEmitente;

        Assert.Throws<ArgumentException>(() => new Inutilizacao("11222333000182", 1, 1, 2, Justificativa));
        Assert.Throws<ArgumentException>(() => new Inutilizacao(cnpj, -1, 1, 2, Justificativa));
        Assert.Throws<ArgumentException>(() => new Inutilizacao(cnpj, 1000, 1, 2, Justificativa));
        Assert.Throws<ArgumentException>(() => new Inutilizacao(cnpj, 1, 0, 2, Justificativa));
        Assert.Throws<ArgumentException>(() => new Inutilizacao(cnpj, 1, 5, 4, Justificativa));
        Assert.Throws<ArgumentException>(() => new Inutilizacao(cnpj, 1, 1, 2, ""));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Inutilizacao(cnpj, 1, 1, 2, Justificativa) { Ano = 100 });
        Assert.Equal(cnpj, new Inutilizacao("11.222.333/0001-81", 1, 1, 2, Justificativa).Cnpj);
    }

    // O envEvento

    [Fact]
    public async Task OCancelamentoSaiNoSchemaAssinadoComOIdEOOrgao()
    {
        var chave = ChaveNota();
        var evento = Cancelar(chave);

        var (resultado, envio) = await Executar(new EnvioEvento(evento) { Lote = 77 }, RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.Contains("NFeRecepcaoEvento4", envio!.Url.AbsolutePath, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio.Mensagem));
        var raiz = XElement.Parse(envio.Mensagem);
        XNamespace ns = NamespaceNFe;
        Assert.Equal("77", raiz.Element(ns + "idLote")!.Value);
        var informacao = raiz.Element(ns + "evento")!.Element(ns + "infEvento")!;
        Assert.Equal($"ID110111{chave}01", (string?)informacao.Attribute("Id"));
        Assert.Equal(["31", "2", NotasTeste.CnpjEmitente, chave.ToString(), "2026-10-20T15:30:00-03:00", "110111", "1", "1.00"],
            informacao.Elements().Where(e => e.Name != ns + "detEvento").Select(e => e.Value));
        var detalhe = informacao.Element(ns + "detEvento")!;
        Assert.Equal("1.00", (string?)detalhe.Attribute("versao"));
        Assert.Equal(["Cancelamento", ProtocoloNota, Justificativa], detalhe.Elements().Select(e => e.Value));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID110111{chave}01", NotasTeste.CnpjEmitente));
        // a Signature é filha do evento, depois do infEvento
        Assert.Equal("Signature", raiz.Element(ns + "evento")!.Elements().Last().Name.LocalName);
    }

    [Fact]
    public async Task OMomentoInformadoSubstituiORelogioEOMesmoMomentoDaOsMesmosBytes()
    {
        var chave = ChaveNota();
        var momento = new DateTimeOffset(2026, 10, 20, 15, 0, 7, TimeSpan.FromHours(-3));
        var evento = new Cancelamento(chave, ProtocoloNota, Justificativa) { Momento = momento };

        var (primeiro, envio1) = await Executar(new EnvioEvento(evento), RetornoEventos());
        var (segundo, envio2) = await Executar(new EnvioEvento(evento) { Lote = 5 }, RetornoEventos());

        Assert.Contains("<dhEvento>2026-10-20T15:00:07-03:00</dhEvento>", envio1!.Mensagem, StringComparison.Ordinal);
        Assert.Equal(EventoEnviado(envio1.Mensagem), EventoEnviado(envio2!.Mensagem));
        Assert.True(primeiro.Processado && segundo.Processado);
    }

    private static string EventoEnviado(string mensagem) => OperacaoEventoApoio.Enviados(mensagem).Single();

    [Fact]
    public async Task OIdLoteVemDoRelogioQuandoNaoEInformado()
    {
        var (_, envio) = await Executar(new EnvioEvento(Cancelar(ChaveNota())), RetornoEventos());

        // Agora = 2026-10-20T15:30-03:00 = 18:30 UTC
        Assert.Contains("<idLote>261020183000000</idLote>", envio!.Mensagem, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(20)]
    public async Task UmDoisEVinteEventosCadaUmComASuaAssinatura(int quantidade)
    {
        var eventos = Enumerable.Range(1, quantidade).Select(n => Cancelar(ChaveNota(n))).ToArray();

        var (resultado, envio) = await Executar(new EnvioEvento(eventos), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio!.Mensagem));
        Assert.Equal(quantidade, resultado.Retorno!.Eventos.Count);
        foreach (var evento in eventos)
            Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID110111{evento.Chave}01", NotasTeste.CnpjEmitente));
        Assert.Equal(quantidade, Regex.Count(envio.Mensagem, "<Signature "));
    }

    [Fact]
    public async Task OAutorCpfDaChaveVaiComoCpf()
    {
        var chave = ChaveAcesso.Montar(UF.MG, NotasTeste.Emissao, "52998224725", Documento.NFCe, 920, 7, TipoEmissao.Normal, 12345678);

        var (resultado, envio) = await Executar(new EnvioEvento(Cancelar(chave)), RetornoEventos(), Documento.NFCe);

        Assert.True(resultado.Processado);
        Assert.Contains("<CPF>52998224725</CPF>", envio!.Mensagem, StringComparison.Ordinal);
        Assert.DoesNotContain("<CNPJ>", envio.Mensagem, StringComparison.Ordinal);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio.Mensagem));
    }

    [Theory]
    [InlineData(14, "Justificativa")]
    [InlineData(256, "Justificativa")]
    public async Task AJustificativaForaDoSchemaEDoEventoRecusaCom493(int tamanho, string propriedade)
    {
        var justificativa = new string('a', tamanho);

        var (resultado, envio) = await Executar(new EnvioEvento(Cancelar(ChaveNota(), justificativa)), RetornoEventos());

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("493", Severidade.Erro, "xJust", $"Eventos[0].{propriedade}"), (problema.Codigo, problema.Severidade, problema.Tag, problema.Propriedade));
        Assert.Equal("envEvento/evento/infEvento/detEvento/xJust", problema.Caminho);
    }

    [Fact]
    public async Task AJustificativaComEspacoNoComecoERecusadaNoSegundoEvento()
    {
        var eventos = new[] { Cancelar(ChaveNota(1)), Cancelar(ChaveNota(2), " Cancelamento por erro de digitacao") };

        var (resultado, envio) = await Executar(new EnvioEvento(eventos), RetornoEventos());

        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("493", "Eventos[1].Justificativa", "envEvento/evento[2]/infEvento/detEvento/xJust"), (problema.Codigo, problema.Propriedade, problema.Caminho));
    }

    [Fact]
    public async Task OCaractereQueOXmlNaoAceitaERecusadoSemExcecao()
    {
        var (resultado, envio) = await Executar(new EnvioEvento(Cancelar(ChaveNota(), "Cancelamento por erro \u0001 de digitacao")), RetornoEventos());

        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-CARACTERE", "Eventos[0].Justificativa"), (problema.Codigo, problema.Propriedade));
        Assert.DoesNotContain("digitacao", problema.Mensagem, StringComparison.Ordinal);
    }

    // O destino

    [Fact]
    public async Task OCancelamentoDeNotaDaSvcVaiASvcEOutroTipoVaiAoNormal()
    {
        var chave = ChaveNota(1, TipoEmissao.ContingenciaSvcAN);
        var evento = Cancelar(chave);

        var (svc, _) = await Executar(new EnvioEvento(evento), RetornoEventos());
        var (normal, _) = await Executar(new EnvioEvento(evento) { Destino = Destino.MG }, RetornoEventos());
        var (outro, _) = await Executar(new EnvioEvento(new EventoFalso(chave)), RetornoEventos());

        Assert.Equal(Destino.SvcAN, svc.Rastro.Autorizador);
        Assert.Equal(Destino.MG, normal.Rastro.Autorizador);
        Assert.Equal(Destino.MG, outro.Rastro.Autorizador);
    }

    [Fact]
    public async Task AChaveDeOutroModeloENFeNumNucleoDeNFCeERecusada()
    {
        var nfce = ChaveAcesso.Ler(Chave(NotasTeste.Assinar(NotasTeste.Escrever(NotasTeste.NFCe()))));

        var (resultado, envio) = await Executar(new EnvioEvento(Cancelar(nfce)), RetornoEventos());
        var (outro, _) = await Executar(new EnvioEvento(Cancelar(ChaveNota())), RetornoEventos(), Documento.NFCe);

        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-CHAVE", "envEvento/evento/infEvento/chNFe", "Eventos[0].Chave"), (problema.Codigo, problema.Caminho, problema.Propriedade));
        Assert.Equal("NFE-CHAVE", Assert.Single(outro.Problemas).Codigo);
    }

    [Fact]
    public async Task OsDestinosMisturadosEOEventoRepetidoSaoNFeLote()
    {
        var misturados = new[] { Cancelar(ChaveNota(1)), Cancelar(ChaveNota(2, TipoEmissao.ContingenciaSvcAN)) };
        var repetidos = new[] { Cancelar(ChaveNota(1)), Cancelar(ChaveNota(1)) };

        var (mistura, envio) = await Executar(new EnvioEvento(misturados), RetornoEventos());
        var (repeticao, _) = await Executar(new EnvioEvento(repetidos), RetornoEventos());

        Assert.Null(envio);
        var problema = Assert.Single(mistura.Problemas);
        Assert.Equal(("NFE-LOTE", "Eventos[1].Chave"), (problema.Codigo, problema.Propriedade));
        var repetido = Assert.Single(repeticao.Problemas);
        Assert.Equal(("NFE-LOTE", "Eventos[1]"), (repetido.Codigo, repetido.Propriedade));
    }

    // O retEnvEvento

    [Fact]
    public async Task OLoteProcessadoTraz135ComOProcEventoDeCadaEvento()
    {
        var eventos = new[] { Cancelar(ChaveNota(1)), Cancelar(ChaveNota(2)) };

        var (resultado, envio) = await Executar(new EnvioEvento(eventos), RetornoEventos());

        Assert.Empty(resultado.Problemas);
        var retorno = resultado.Retorno!;
        Assert.Equal((128, Ambiente.Homologacao, "MG-TESTE", 31, 123L), (retorno.Status, retorno.Ambiente, retorno.VersaoAplicativo, retorno.Orgao, retorno.Lote));
        Assert.Equal(2, retorno.Eventos.Count);
        for (var k = 0; k < 2; k++)
        {
            var e = retorno.Eventos[k];
            Assert.Same(eventos[k], e.Evento);
            Assert.Equal((135, true, "131260000000099", "Cancelamento homologado"), (e.Status, e.Registrado, e.Protocolo, e.Descricao));
            Assert.Equal(eventos[k].Chave, e.Chave);
            Assert.NotNull(e.Registro);
            Assert.Equal(OperacaoEventoApoio.Enviados(envio!.Mensagem)[k], e.XmlEvento);
            Assert.Empty(SchemasNFe400.EventoProcessado.Validar(e.XmlProcessado!));
            Assert.Contains(e.XmlEvento!, e.XmlProcessado, StringComparison.Ordinal);
            Assert.Empty(AssinaturaDigital.Verificar(e.XmlProcessado!, $"ID110111{eventos[k].Chave}01", NotasTeste.CnpjEmitente));
        }
    }

    [Theory]
    [InlineData(136, true)]
    [InlineData(155, true)]
    [InlineData(573, false)]
    [InlineData(494, false)]
    public async Task OStatusDoEventoDizSeFoiRegistrado(int status, bool registrado)
    {
        var (resultado, _) = await Executar(new EnvioEvento(Cancelar(ChaveNota())), RetornoEventos(status));

        var e = Assert.Single(resultado.Retorno!.Eventos);
        Assert.Equal((status, registrado), (e.Status, e.Registrado));
        Assert.Equal(registrado, e.XmlProcessado is not null);
        Assert.NotNull(e.XmlEvento);
    }

    [Theory]
    [InlineData(215)]
    [InlineData(516)]
    [InlineData(108)]
    [InlineData(109)]
    public async Task OLoteRejeitadoNaoTemEventos(int status)
    {
        var (resultado, _) = await Executar(new EnvioEvento(Cancelar(ChaveNota())), RetornoEventos(lote: status));

        Assert.Equal(status, resultado.CStat);
        Assert.Empty(resultado.Retorno!.Eventos);
    }

    [Fact]
    public async Task OEventoSemChaveNoRetornoLigaPelaPosicao()
    {
        var eventos = new[] { Cancelar(ChaveNota(1)), Cancelar(ChaveNota(2)) };

        var (resultado, _) = await Executar(new EnvioEvento(eventos), RetornoEventos(573, semChave: true));

        Assert.Empty(resultado.Problemas);
        Assert.Same(eventos[0], resultado.Retorno!.Eventos[0].Evento);
        Assert.Same(eventos[1], resultado.Retorno.Eventos[1].Evento);
        Assert.Null(resultado.Retorno.Eventos[0].Chave);
    }

    [Fact]
    public async Task OQueSobraDoRetornoViraResultadoSemEventoComAviso()
    {
        var evento = Cancelar(ChaveNota(1));

        var (resultado, _) = await Executar(new EnvioEvento(evento), pedido =>
            RetEnvEvento(128, "Lote de Evento Processado",
                RetEvento(135, "110111", evento.Chave.ToString(), "1") + RetEvento(573, "110111", ChaveNota(9).ToString(), "1")));

        var retorno = resultado.Retorno!;
        Assert.Equal(2, retorno.Eventos.Count);
        Assert.Same(evento, retorno.Eventos[0].Evento);
        Assert.Null(retorno.Eventos[1].Evento);
        Assert.Null(retorno.Eventos[1].XmlEvento);
        var aviso = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-EVENTO", Severidade.Aviso), (aviso.Codigo, aviso.Severidade));
    }

    [Fact]
    public async Task OEventoSemRetEventoGeraAviso()
    {
        var (resultado, _) = await Executar(new EnvioEvento(Cancelar(ChaveNota(1)), Cancelar(ChaveNota(2))), pedido =>
            RetEnvEvento(128, "Lote de Evento Processado", RetEvento(135, "110111", Regex.Match(pedido, "ID110111(\\d{44})").Groups[1].Value, "1")));

        Assert.Single(resultado.Retorno!.Eventos);
        var aviso = Assert.Single(resultado.Problemas);
        Assert.Equal(("NFE-EVENTO", "Eventos[1]"), (aviso.Codigo, aviso.Propriedade));
    }

    [Fact]
    public async Task ARespostaIlegivelDoEventoELancadaComORastro()
    {
        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Executar(new EnvioEvento(Cancelar(ChaveNota())), _ => "<retEnvEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\"><idLote>1"));
        var semStatus = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Executar(new EnvioEvento(Cancelar(ChaveNota())), _ => "<retEnvEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\"><idLote>1</idLote></retEnvEvento>"));
        var outraRaiz = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Executar(new EnvioEvento(Cancelar(ChaveNota())), _ => Retorno(104, "x")));

        Assert.All([falha, semStatus, outraRaiz], f => Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, f.Tipo));
        Assert.NotNull(falha.Rastro.XmlPedido);
    }

    // A inutilização

    private static Func<string, string> RetInutilizacao(int status, bool faixa = true) => pedido =>
    {
        var id = Regex.Match(pedido, "Id=\"ID(\\d{2})(\\d{2})(\\w{14})(\\d{2})(\\d{3})(\\d{9})(\\d{9})\"").Groups;
        var numero = (string g) => int.Parse(g, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        return "<retInutNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><infInut><tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic>"
            + $"<cStat>{status}</cStat><xMotivo>Motivo {status}</xMotivo><cUF>31</cUF>"
            + (faixa
                ? $"<ano>{id[2].Value}</ano><CNPJ>{id[3].Value}</CNPJ><mod>{id[4].Value}</mod><serie>{numero(id[5].Value)}</serie><nNFIni>{numero(id[6].Value)}</nNFIni><nNFFin>{numero(id[7].Value)}</nNFFin>"
                : "")
            + $"<dhRecbto>{Recebimento}</dhRecbto>" + (faixa ? "<nProt>131260000000050</nProt>" : "") + "</infInut></retInutNFe>";
    };

    [Theory]
    [InlineData(Documento.NFe, "55")]
    [InlineData(Documento.NFCe, "65")]
    public async Task AInutilizacaoSaiNoSchemaAssinadaComOIdEOModelo(Documento documento, string modelo)
    {
        var (resultado, envio) = await Executar(new Inutilizacao("11.222.333/0001-81", 7, 10, 15, Justificativa), RetInutilizacao(102), documento);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.Contains("NFeInutilizacao4", envio!.Url.AbsolutePath, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(SchemasNFe400.Inutilizacao.Validar(envio.Mensagem));
        var id = $"ID3126{NotasTeste.CnpjEmitente}{modelo}007000000010000000015";
        var informacao = XElement.Parse(envio.Mensagem).Element(XName.Get("infInut", NamespaceNFe))!;
        Assert.Equal(id, (string?)informacao.Attribute("Id"));
        Assert.Equal(["2", "INUTILIZAR", "31", "26", NotasTeste.CnpjEmitente, modelo, "7", "10", "15", Justificativa],
            informacao.Elements().Select(e => e.Value));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, id, NotasTeste.CnpjEmitente));
    }

    [Fact]
    public async Task OAnoDaInutilizacaoVemDoRelogioEOInformadoSubstitui()
    {
        var (_, doRelogio) = await Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 1, 1, 1, Justificativa), RetInutilizacao(102));
        var (_, informado) = await Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 1, 1, 1, Justificativa) { Ano = 25 }, RetInutilizacao(102));

        Assert.Contains("<ano>26</ano>", doRelogio!.Mensagem, StringComparison.Ordinal);
        Assert.Contains("<ano>25</ano>", informado!.Mensagem, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AInutilizacaoComONucleoEmSvcVaiAoNormal()
    {
        using var conexoes = new ConexoesSefaz();
        var envios = new List<Envio>();
        using var svc = new ConexoesSefaz(_ => new HandlerResposta(RetInutilizacao(102), envios));
        var configuracao = Configuracao(svc);
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
        {
            Ambiente = configuracao.Ambiente,
            UF = configuracao.UF,
            Certificado = configuracao.Certificado,
            Conexoes = svc,
            Relogio = configuracao.Relogio,
            Emissao = TipoEmissao.ContingenciaSvcAN,
            Contingencia = new Contingencia(NotasTeste.Emissao, "Teste da inutilizacao em contingencia"),
        });

        var resultado = await dfe.ExecutarAsync(new Inutilizacao(NotasTeste.CnpjEmitente, 1, 1, 1, Justificativa), Ct);

        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
    }

    [Fact]
    public async Task AInutilizacaoHomologadaTrazOProcInutNFeValidoComOsBytesEnviados()
    {
        var (resultado, envio) = await Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 7, 10, 15, Justificativa), RetInutilizacao(102));

        var retorno = resultado.Retorno!;
        Assert.True(retorno.Homologada);
        Assert.Equal((Documento.NFe, 26, NotasTeste.CnpjEmitente, 7, 10, 15, "131260000000050", UF.MG),
            (retorno.Modelo, retorno.Ano, retorno.Cnpj, retorno.Serie, retorno.NumeroInicial, retorno.NumeroFinal, retorno.Protocolo, retorno.UF));
        Assert.NotNull(retorno.Recebimento);
        Assert.Empty(SchemasNFe400.InutilizacaoProcessada.Validar(retorno.XmlProcessado!));
        Assert.Contains(envio!.Mensagem, retorno.XmlProcessado, StringComparison.Ordinal);
        Assert.StartsWith("<ProcInutNFe ", retorno.XmlProcessado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AFaixaJaInutilizadaTrazAFaixaEOProtocoloSemOProc()
    {
        var (resultado, _) = await Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 7, 10, 15, Justificativa), RetInutilizacao(563));

        var retorno = resultado.Retorno!;
        Assert.False(retorno.Homologada);
        Assert.Equal((563, 10, 15, "131260000000050"), (retorno.Status, retorno.NumeroInicial, retorno.NumeroFinal, retorno.Protocolo));
        Assert.Null(retorno.XmlProcessado);
    }

    [Theory]
    [InlineData(241)]
    [InlineData(256)]
    [InlineData(266)]
    [InlineData(453)]
    [InlineData(454)]
    [InlineData(201)]
    [InlineData(224)]
    public async Task AsRejeicoesDaInutilizacaoNaoTemFaixaNemProc(int status)
    {
        var (resultado, _) = await Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 7, 10, 15, Justificativa), RetInutilizacao(status, faixa: false));

        var retorno = resultado.Retorno!;
        Assert.Equal(status, retorno.Status);
        Assert.False(retorno.Homologada);
        Assert.Null(retorno.XmlProcessado);
        Assert.Null(retorno.NumeroInicial);
        Assert.Null(retorno.Protocolo);
        Assert.NotNull(retorno.Recebimento);
    }

    [Fact]
    public async Task AJustificativaDaInutilizacaoForaDoSchemaEDoMensagem215ComAPropriedade()
    {
        var (resultado, envio) = await Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 7, 10, 15, "curta"), RetInutilizacao(102));

        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("215", "Justificativa"), (problema.Codigo, problema.Propriedade));
    }

    [Fact]
    public async Task ARespostaIlegivelDaInutilizacaoELancada()
    {
        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 7, 10, 15, Justificativa), _ => "<retInutNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infInut>"));
        var semInfo = await Assert.ThrowsAsync<FalhaTransmissaoException>(
            () => Executar(new Inutilizacao(NotasTeste.CnpjEmitente, 7, 10, 15, Justificativa), _ => "<retInutNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"/>"));

        Assert.All([falha, semInfo], f => Assert.Equal(TipoFalhaTransmissao.RespostaIlegivel, f.Tipo));
    }

    // O evento de teste: prova a extensão da base (a 6.2 e a 8.2 só derivam, sem mudar o envio nem a operação). Não é
    // cancelamento: não aceita a SVC.
    private sealed class EventoFalso(ChaveAcesso chave) : OpenDFe.NFe.Evento(chave, TipoEvento.CartaCorrecao, 1)
    {
        internal override string Descricao => "Carta de Correcao";

        internal override string Autor => NotasTeste.CnpjEmitente;

        internal override SchemaXml SchemaDetalhe => SchemasNFe400.DetalheCancelamento;

        internal override IEnumerable<(string Tag, string Propriedade, string Valor)> Textos => [];

        internal override void EscreverDetalhe(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("detEvento", NamespaceNFe);
            xml.WriteAttributeString("versao", "1.00");
            xml.WriteElementString("descEvento", NamespaceNFe, "Cancelamento");
            xml.WriteElementString("nProt", NamespaceNFe, ProtocoloNota);
            xml.WriteElementString("xJust", NamespaceNFe, Justificativa);
            xml.WriteEndElement();
        }

        internal override string? PropriedadeDetalhe(string passo) => null;
    }
}

// O apoio dos testes dos eventos: os eventos tirados da mensagem.
internal static class OperacaoEventoApoio
{
    internal static List<string> Enviados(string mensagem) =>
        [.. Regex.Matches(mensagem, "<evento .*?</evento>", RegexOptions.Singleline).Select(m => m.Value)];
}
