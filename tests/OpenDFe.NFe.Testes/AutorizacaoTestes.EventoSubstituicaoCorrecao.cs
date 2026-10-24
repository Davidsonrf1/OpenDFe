using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O cancelamento por substituição e a carta de correção (etapa 6.2, CONTRATOS.md 18.5), pelo ExecutarAsync com o handler que
// responde conforme o pedido: os parâmetros, o detEvento de cada tipo no schema do tipo, o evento assinado no envEvento, o
// destino, os limites do schema (493), o lote misto e a leitura do 135 com o cOrgaoAutor. Contra a SEFAZ emulada, em
// SubstituicaoEmuladaTestes e CartaCorrecaoEmuladaTestes.
public partial class AutorizacaoTestes
{
    private const string Correcao = "Correcao do endereco do destinatario";

    private static ChaveAcesso ChaveNFCe(int numero = 1, TipoEmissao emissao = TipoEmissao.Normal) =>
        ChaveAcesso.Montar(UF.MG, NotasTeste.Emissao, NotasTeste.CnpjEmitente, Documento.NFCe, 1, numero, emissao, 12345678);

    private static CancelamentoSubstituicao Substituir(ChaveAcesso? chave = null, ChaveAcesso? substituta = null, string justificativa = Justificativa) =>
        new(chave ?? ChaveNFCe(1), ProtocoloNota, justificativa, substituta ?? ChaveNFCe(2));

    private static CartaCorrecao Corrigir(ChaveAcesso? chave = null, int sequencia = 1, string correcao = Correcao) =>
        new(chave ?? ChaveNota(), sequencia, correcao);

    // O retEnvEvento do 110112: o 135 com o cOrgaoAutor depois do nSeqEvento, como no retEvento do Portal.
    private static string RetornoSubstituicao(string pedido)
    {
        var m = Regex.Match(pedido, "<infEvento Id=\"ID(\\d{6})(\\d{44})(\\d{2})\"");
        return RetEnvEvento(128, "Lote de Evento Processado",
            "<retEvento versao=\"1.00\"><infEvento><tpAmb>2</tpAmb><verAplic>MG-TESTE</verAplic><cOrgao>31</cOrgao>"
            + $"<cStat>135</cStat><xMotivo>Evento registrado e vinculado a NF-e</xMotivo><chNFe>{m.Groups[2].Value}</chNFe>"
            + $"<tpEvento>{m.Groups[1].Value}</tpEvento><xEvento>Cancelamento por substituicao homologado</xEvento>"
            + $"<nSeqEvento>1</nSeqEvento><cOrgaoAutor>31</cOrgaoAutor><dhRegEvento>{Recebimento}</dhRegEvento>"
            + "<nProt>131260000000099</nProt></infEvento></retEvento>");
    }

    private static XElement Detalhe(string mensagem) =>
        XElement.Parse(mensagem).Descendants(XName.Get("detEvento", NamespaceNFe)).First();

    // Os parâmetros

    [Fact]
    public void OsParametrosDaSubstituicaoRecusamOQueASefazRejeitaria()
    {
        var cancelada = ChaveNFCe(1);
        var substituta = ChaveNFCe(2);
        var nfe = ChaveNota();

        Assert.Throws<ArgumentNullException>(() => new CancelamentoSubstituicao(null!, ProtocoloNota, Justificativa, substituta));
        Assert.Throws<ArgumentNullException>(() => new CancelamentoSubstituicao(cancelada, ProtocoloNota, Justificativa, null!));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(nfe, ProtocoloNota, Justificativa, substituta));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, ProtocoloNota, Justificativa, nfe));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, ProtocoloNota, Justificativa, cancelada));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, "12345678901234", Justificativa, substituta));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, "1234567890123456", Justificativa, substituta));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, "13126000000000A", Justificativa, substituta));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, null!, Justificativa, substituta));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, ProtocoloNota, "", substituta));
        Assert.Throws<ArgumentException>(() => new CancelamentoSubstituicao(cancelada, ProtocoloNota, null!, substituta));
        var evento = Substituir(cancelada, substituta);
        Assert.Equal((TipoEvento.CancelamentoSubstituicao, 1, ProtocoloNota, Justificativa), (evento.Tipo, evento.Sequencia, evento.Protocolo, evento.Justificativa));
        Assert.Same(cancelada, evento.Chave);
        Assert.Same(substituta, evento.ChaveSubstituta);
    }

    [Fact]
    public void OsParametrosDaCartaDeCorrecaoRecusamOQueASefazRejeitaria()
    {
        var nfe = ChaveNota();

        Assert.Throws<ArgumentNullException>(() => new CartaCorrecao(null!, 1, Correcao));
        Assert.Throws<ArgumentException>(() => new CartaCorrecao(ChaveNFCe(), 1, Correcao));
        Assert.Throws<ArgumentException>(() => new CartaCorrecao(nfe, 1, ""));
        Assert.Throws<ArgumentException>(() => new CartaCorrecao(nfe, 1, null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CartaCorrecao(nfe, 0, Correcao));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CartaCorrecao(nfe, 21, Correcao));
        var carta = new CartaCorrecao(nfe, 20, Correcao);
        Assert.Equal((TipoEvento.CartaCorrecao, 20, Correcao), (carta.Tipo, carta.Sequencia, carta.Correcao));
        Assert.Equal(1, new CartaCorrecao(nfe, 1, Correcao).Sequencia);
    }

    [Fact]
    public void OVerAplicPadraoEOpenDFeComAVersaoDoPacoteECortadoEmVinte()
    {
        var padrao = Substituir().VersaoAplicativo;
        var esperada = typeof(CancelamentoSubstituicao).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion.Split('+')[0];

        Assert.StartsWith("OpenDFe ", padraoOuCortada(esperada), StringComparison.Ordinal);
        Assert.Equal(padraoOuCortada(esperada), padrao);
        Assert.InRange(padrao.Length, 9, 20);

        var chave = ChaveNFCe(1);
        var informada = new CancelamentoSubstituicao(chave, ProtocoloNota, Justificativa, ChaveNFCe(2)) { VersaoAplicativo = "PDV 1.0" };
        var longa = new CancelamentoSubstituicao(chave, ProtocoloNota, Justificativa, ChaveNFCe(2)) { VersaoAplicativo = new string('v', 30) };
        var nula = new CancelamentoSubstituicao(chave, ProtocoloNota, Justificativa, ChaveNFCe(2)) { VersaoAplicativo = null! };
        Assert.Equal("PDV 1.0", informada.VersaoAplicativo);
        Assert.Equal(new string('v', 20), longa.VersaoAplicativo);
        Assert.Equal(padrao, nula.VersaoAplicativo);

        static string padraoOuCortada(string versao)
        {
            var texto = "OpenDFe " + versao;
            return texto.Length > 20 ? texto[..20] : texto;
        }
    }

    // O detEvento de cada tipo

    [Fact]
    public async Task ASubstituicaoSaiNoSchemaDoTipoAssinadaComOCOrgaoAutorEOTpAutor()
    {
        var cancelada = ChaveNFCe(1);
        var substituta = ChaveNFCe(2);
        var evento = new CancelamentoSubstituicao(cancelada, ProtocoloNota, Justificativa, substituta) { VersaoAplicativo = "PDV 1.0" };

        var (resultado, envio) = await Executar(new EnvioEvento(evento), RetornoSubstituicao, Documento.NFCe);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio!.Mensagem));
        var detalhe = Detalhe(envio.Mensagem);
        Assert.Empty(SchemasNFe400.DetalheSubstituicao.Validar(detalhe.ToString(SaveOptions.DisableFormatting)));
        Assert.Equal("1.00", (string?)detalhe.Attribute("versao"));
        Assert.Equal(
            [("descEvento", "Cancelamento por substituicao"), ("cOrgaoAutor", "31"), ("tpAutor", "1"), ("verAplic", "PDV 1.0"),
                ("nProt", ProtocoloNota), ("xJust", Justificativa), ("chNFeRef", substituta.ToString())],
            detalhe.Elements().Select(e => (e.Name.LocalName, e.Value)));
        var informacao = XElement.Parse(envio.Mensagem).Descendants(XName.Get("infEvento", NamespaceNFe)).Single();
        Assert.Equal($"ID110112{cancelada}01", (string?)informacao.Attribute("Id"));
        Assert.Equal(["31", "2", NotasTeste.CnpjEmitente, cancelada.ToString(), "2026-10-20T15:30:00-03:00", "110112", "1", "1.00"],
            informacao.Elements().Where(e => e.Name.LocalName != "detEvento").Select(e => e.Value));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID110112{cancelada}01", NotasTeste.CnpjEmitente));
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
    }

    [Fact]
    public async Task ACartaDeCorrecaoSaiNoSchemaDoTipoAssinadaComOXCondUsoDoSchema()
    {
        var chave = ChaveNota();
        var carta = Corrigir(chave, 2);

        var (resultado, envio) = await Executar(new EnvioEvento(carta), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio!.Mensagem));
        var detalhe = Detalhe(envio.Mensagem);
        Assert.Empty(SchemasNFe400.DetalheCartaCorrecao.Validar(detalhe.ToString(SaveOptions.DisableFormatting)));
        Assert.Equal([("descEvento", "Carta de Correcao"), ("xCorrecao", Correcao), ("xCondUso", CartaCorrecao.CondicaoUso)],
            detalhe.Elements().Select(e => (e.Name.LocalName, e.Value)));
        var informacao = XElement.Parse(envio.Mensagem).Descendants(XName.Get("infEvento", NamespaceNFe)).Single();
        Assert.Equal($"ID110110{chave}02", (string?)informacao.Attribute("Id"));
        Assert.Equal("2", informacao.Element(XName.Get("nSeqEvento", NamespaceNFe))!.Value);
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID110110{chave}02", NotasTeste.CnpjEmitente));
    }

    [Fact]
    public void OTextoDaCondicaoDeUsoEUmDosDoSchemaEmbutido()
    {
        var assembly = typeof(SchemasNFe400).Assembly;
        var nome = assembly.GetManifestResourceNames().Single(n => n.EndsWith("e110110_v1.00.xsd", StringComparison.Ordinal));
        using var fluxo = assembly.GetManifestResourceStream(nome)!;
        var xsd = XDocument.Load(fluxo);
        XNamespace xs = "http://www.w3.org/2001/XMLSchema";
        var condicao = xsd.Descendants(xs + "element").Single(e => (string?)e.Attribute("name") == "xCondUso");
        var valores = condicao.Descendants(xs + "enumeration").Select(e => (string?)e.Attribute("value")).ToList();

        Assert.Contains(CartaCorrecao.CondicaoUso, valores);
        // o texto sem acentos é o que o MOC mostra (Anexo I, 4.4): um acento no texto mostraria o desvio
        Assert.All(CartaCorrecao.CondicaoUso, c => Assert.True(c < 128, "a condição de uso é a forma sem acentos"));
    }

    [Fact]
    public void ADescricaoDeCadaEventoEUmaDasDoSchema()
    {
        var assembly = typeof(SchemasNFe400).Assembly;
        foreach (var (arquivo, descricao) in new[] { ("e110110_v1.00.xsd", "Carta de Correcao"), ("e110112_v1.00.xsd", "Cancelamento por substituicao") })
        {
            var nome = assembly.GetManifestResourceNames().Single(n => n.EndsWith(arquivo, StringComparison.Ordinal));
            using var fluxo = assembly.GetManifestResourceStream(nome)!;
            XNamespace xs = "http://www.w3.org/2001/XMLSchema";
            var campo = XDocument.Load(fluxo).Descendants(xs + "element").Single(e => (string?)e.Attribute("name") == "descEvento");
            Assert.Contains(descricao, campo.Descendants(xs + "enumeration").Select(e => (string?)e.Attribute("value")));
        }
    }

    // As recusas do schema (493), com a propriedade

    [Theory]
    [InlineData(14)]
    [InlineData(1001)]
    public async Task ACorrecaoForaDe15A1000CaracteresRecusaCom493(int tamanho)
    {
        var (resultado, envio) = await Executar(new EnvioEvento(Corrigir(correcao: new string('c', tamanho))), RetornoEventos());

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Null(envio);
        var problema = Assert.Single(resultado.Problemas);
        Assert.Equal(("493", "Eventos[0].Correcao"), (problema.Codigo, problema.Propriedade));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(1000)]
    public async Task ACorrecaoDe15E1000CaracteresPassa(int tamanho)
    {
        var (resultado, envio) = await Executar(new EnvioEvento(Corrigir(correcao: new string('c', tamanho))), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.NotNull(envio);
    }

    [Fact]
    public async Task ACorrecaoComEspacoNoComecoERecusadaEACaractereInvalidoENFeCaractere()
    {
        var (espaco, _) = await Executar(new EnvioEvento(Corrigir(correcao: " " + Correcao)), RetornoEventos());
        var (invalido, _) = await Executar(new EnvioEvento(Corrigir(correcao: Correcao + "\u0001")), RetornoEventos());

        Assert.Equal("493", Assert.Single(espaco.Problemas).Codigo);
        var problema = Assert.Single(invalido.Problemas);
        Assert.Equal(("NFE-CARACTERE", "Eventos[0].Correcao"), (problema.Codigo, problema.Propriedade));
    }

    [Fact]
    public async Task AJustificativaEOVerAplicDaSubstituicaoSaoConferidos()
    {
        var (curta, _) = await Executar(new EnvioEvento(Substituir(justificativa: new string('j', 14))), RetornoSubstituicao, Documento.NFCe);
        var (vazia, _) = await Executar(
            new EnvioEvento(new CancelamentoSubstituicao(ChaveNFCe(1), ProtocoloNota, Justificativa, ChaveNFCe(2)) { VersaoAplicativo = "" }),
            RetornoSubstituicao, Documento.NFCe);

        Assert.Equal(("493", "Eventos[0].Justificativa"), (Assert.Single(curta.Problemas).Codigo, Assert.Single(curta.Problemas).Propriedade));
        Assert.Equal(("493", "Eventos[0].VersaoAplicativo"), (Assert.Single(vazia.Problemas).Codigo, Assert.Single(vazia.Problemas).Propriedade));
    }

    // O destino

    [Fact]
    public async Task ASubstituicaoDeNotaOfflineEACartaDeNotaDaSvcVaoAoAutorizadorNormal()
    {
        var offline = ChaveNFCe(1, TipoEmissao.ContingenciaOffline);
        var svc = ChaveNota(1, TipoEmissao.ContingenciaSvcAN);

        var (substituicao, envio) = await Executar(new EnvioEvento(Substituir(offline, ChaveNFCe(2))), RetornoSubstituicao, Documento.NFCe);
        var (carta, _) = await Executar(new EnvioEvento(Corrigir(svc)), RetornoEventos());

        Assert.Equal(Destino.MG, substituicao.Rastro.Autorizador);
        Assert.Equal(Destino.MG, carta.Rastro.Autorizador);
        Assert.Contains("NFeRecepcaoEvento4", envio!.Url.AbsolutePath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AChaveDeOutraUfVaiAoAutorizadorDela()
    {
        var chave = ChaveAcesso.Montar(UF.SP, NotasTeste.Emissao, NotasTeste.CnpjEmitente, Documento.NFe, 1, 1, TipoEmissao.Normal, 12345678);

        var (resultado, envio) = await Executar(new EnvioEvento(Corrigir(chave)), RetornoEventos());

        Assert.Equal(Destino.SP, resultado.Rastro.Autorizador);
        Assert.Contains("<cOrgao>35</cOrgao>", envio!.Mensagem, StringComparison.Ordinal);
    }

    // O lote

    [Fact]
    public async Task UmLoteComCartaECancelamentoDaMesmaNotaVaiJuntoAssinadoCadaUm()
    {
        var chave = ChaveNota();
        var carta = Corrigir(chave);
        var cancelamento = Cancelar(chave);

        var (resultado, envio) = await Executar(new EnvioEvento(carta, cancelamento), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Empty(SchemasNFe400.Evento.Validar(envio!.Mensagem));
        Assert.Equal([carta, cancelamento], resultado.Retorno!.Eventos.Select(e => e.Evento));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID110110{chave}01", NotasTeste.CnpjEmitente));
        Assert.Empty(AssinaturaDigital.Verificar(envio.Mensagem, $"ID110111{chave}01", NotasTeste.CnpjEmitente));
    }

    [Fact]
    public async Task DuasCartasDeSequenciaDiferenteNaoSaoOMesmoEvento()
    {
        var chave = ChaveNota();

        var (resultado, _) = await Executar(new EnvioEvento(Corrigir(chave, 1), Corrigir(chave, 2)), RetornoEventos());

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(2, resultado.Retorno!.Eventos.Count);
    }

    [Fact]
    public async Task AMesmaCartaDuasVezesNoEnvioENFeLote()
    {
        var chave = ChaveNota();

        var (resultado, envio) = await Executar(new EnvioEvento(Corrigir(chave, 1), Corrigir(chave, 1)), RetornoEventos());

        Assert.Null(envio);
        Assert.Equal("NFE-LOTE", Assert.Single(resultado.Problemas).Codigo);
    }

    // A leitura

    [Fact]
    public async Task OCancelamentoPorSubstituicaoRegistradoTrazOProcEventoValidoComOCOrgaoAutor()
    {
        var evento = Substituir();

        var (resultado, envio) = await Executar(new EnvioEvento(evento), RetornoSubstituicao, Documento.NFCe);

        Assert.Empty(resultado.Problemas);
        var e = Assert.Single(resultado.Retorno!.Eventos);
        Assert.Equal((135, true, "131260000000099"), (e.Status, e.Registrado, e.Protocolo));
        Assert.Same(evento, e.Evento);
        Assert.Equal(OperacaoEventoApoio.Enviados(envio!.Mensagem)[0], e.XmlEvento);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(e.XmlProcessado!));
        Assert.Contains("<cOrgaoAutor>31</cOrgaoAutor>", e.XmlProcessado, StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(e.XmlProcessado!, $"ID110112{evento.Chave}01", NotasTeste.CnpjEmitente));
    }

    [Fact]
    public async Task ACartaRegistradaTrazOProcEventoValidoNoSchema()
    {
        var carta = Corrigir();

        var (resultado, _) = await Executar(new EnvioEvento(carta), RetornoEventos());

        var e = Assert.Single(resultado.Retorno!.Eventos);
        Assert.Same(carta, e.Evento);
        Assert.Empty(SchemasNFe400.EventoProcessado.Validar(e.XmlProcessado!));
        Assert.Empty(AssinaturaDigital.Verificar(e.XmlProcessado!, $"ID110110{carta.Chave}01", NotasTeste.CnpjEmitente));
    }
}
