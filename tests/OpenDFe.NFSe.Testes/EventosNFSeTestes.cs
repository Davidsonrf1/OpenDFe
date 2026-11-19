using System.Text;
using System.Xml;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// A leitura dos eventos da NFS-e (etapa 9.1, docs/NFSE.md 7.5): um pedido de cada tipo do XSD, escrito pelo escritor
// interno (o envio é da 9.3), válido no XSD e lido de volta igual; o evento registrado, assinado; e a rejeição da 1.00, com
// os campos dentro do infRej.
public class EventosNFSeTestes
{
    private static readonly DateTimeOffset Data = new(2026, 11, 20, 9, 0, 0, TimeSpan.FromHours(-3));

    internal static IEnumerable<EventoNFSe> UmDeCada()
    {
        yield return new CancelamentoNFSe { Motivo = MotivoCancelamento.ErroEmissao, Descricao = "Erro no valor do serviço" };
        yield return new CancelamentoSubstituicao { Motivo = MotivoSubstituicao.Outros, Descricao = "Correção do tomador", ChaveSubstituta = DpsTeste.Chave };
        yield return new AnaliseFiscalCancelamento { Motivo = MotivoAnaliseFiscal.ServicoNaoPrestado, Descricao = "Serviço cancelado pelo cliente" };
        yield return new CancelamentoDeferido { CpfAgente = DpsTeste.Cpf, ProcessoAdministrativo = "123", Motivo = MotivoDeferimento.Deferido, Descricao = "Deferido pelo fisco" };
        yield return new CancelamentoIndeferido { CpfAgente = DpsTeste.Cpf, ProcessoAdministrativo = "124", Motivo = MotivoIndeferimento.IndeferidoMerito, Descricao = "Indeferido pelo fisco" };
        yield return new ConfirmacaoPrestador();
        yield return new ConfirmacaoTomador();
        yield return new ConfirmacaoIntermediario();
        yield return new ConfirmacaoTacita();
        yield return new RejeicaoPrestador { Motivo = MotivoRejeicao.Duplicidade, Descricao = "Nota em duplicidade" };
        yield return new RejeicaoTomador { Motivo = MotivoRejeicao.ErroValores };
        yield return new RejeicaoIntermediario { Motivo = MotivoRejeicao.Outros, Descricao = "Outro motivo qualquer" };
        yield return new AnulacaoRejeicao { CpfAgente = DpsTeste.Cpf, EventoRejeicao = DpsTeste.Chave + "203206" + "001", Descricao = "Rejeição anulada" };
        yield return new CancelamentoOficio { CpfAgente = DpsTeste.Cpf, ProcessoAdministrativo = "999", DescricaoProcesso = "Processo de ofício" };
        yield return new BloqueioOficio { CpfAgente = DpsTeste.Cpf, EventoBloqueado = "e101101", Descricao = "Bloqueio pelo fisco" };
        yield return new DesbloqueioOficio { CpfAgente = DpsTeste.Cpf, Bloqueio = DpsTeste.Chave + "305102" + "001" };
    }

    public static TheoryData<string> Tipos => new(UmDeCada().Select(e => e.Tipo));

    private static EventoNFSe Criar(string tipo)
    {
        var evento = UmDeCada().First(e => e.Tipo == tipo);
        evento.Ambiente = Ambiente.Homologacao;
        evento.VersaoAplicativo = "ERP 1.0";
        evento.Data = Data;
        evento.CnpjAutor = DpsTeste.CnpjPrestador;
        evento.Chave = DpsTeste.Chave;
        return evento;
    }

    [Fact]
    public void HaUmaSubclassePorTipoDoXSD()
    {
        Assert.Equal(16, MapasNFSe.Eventos.Length);
        Assert.Equal(MapasNFSe.Eventos.Select(t => t.Tipo).Order(), UmDeCada().Select(e => e.Tipo).Order());
        Assert.All(MapasNFSe.Eventos, t => Assert.Equal(t.Tipo, t.Criar().Tipo));
        Assert.Equal(["101101", "101103"], MapasNFSe.Eventos.Where(t => t.Emitente).Select(t => t.Tipo));
    }

    [Theory]
    [MemberData(nameof(Tipos))]
    public void OPedidoDeCadaTipoEValidoNoXSDEVoltaIgual(string tipo)
    {
        var evento = Criar(tipo);
        var escrito = EscritorNFSe.EscreverPedido(evento, null);

        Assert.Empty(escrito.Problemas);
        Assert.Empty(SchemasNFSe101.Pedido.Validar(escrito.Xml));
        var lido = LeiauteNFSe101.Instancia.LerEvento(escrito.Xml);
        Assert.Empty(lido.Problemas);
        Assert.Equal("PRE" + DpsTeste.Chave + tipo, lido.Id);
        Assert.Null(lido.Registrado);
        Assert.Empty(ComparacaoNFSe.Diferencas(evento, lido.Evento!));
    }

    [Fact]
    public void OEventoRegistradoEValidoNoXSDELidoComOPedido()
    {
        var pedido = Criar("101101");
        var xml = Registrado(EscritorNFSe.EscreverPedido(pedido, null).Xml, "101101");
        Assert.Empty(SchemasNFSe101.Evento.Validar(xml));

        var lido = LeiauteNFSe101.Instancia.LerEvento(xml);

        Assert.Empty(lido.Problemas);
        Assert.Equal("EVT" + DpsTeste.Chave + "101101" + "001", lido.Id);
        var registrado = lido.Registrado!;
        Assert.Equal((AmbienteGeradorEvento.SefinNacional, 1, 55L), (registrado.AmbienteGerador, registrado.Sequencia, registrado.NumeroDfse));
        Assert.Equal(new DateTimeOffset(2026, 11, 20, 9, 0, 5, TimeSpan.FromHours(-3)), registrado.Processamento);
        Assert.Same(registrado.Pedido, lido.Evento);
        Assert.Empty(ComparacaoNFSe.Diferencas(pedido, lido.Evento!));
    }

    [Fact]
    public void OProblemaDoPedidoDentroDoEventoRegistradoApontaOPedido()
    {
        var xml = Registrado(EscritorNFSe.EscreverPedido(Criar("101101"), null).Xml.Replace("<cMotivo>1</cMotivo>", "<cMotivo>X</cMotivo>", StringComparison.Ordinal), "101101");

        var problema = Assert.Single(LeiauteNFSe101.Instancia.LerEvento(xml).Problemas);

        Assert.Equal("Pedido.Motivo", problema.Propriedade);
        Assert.Equal("evento/infEvento/pedRegEvento/infPedReg/e101101/cMotivo", problema.Caminho);
    }

    [Fact]
    public void ARejeicao100ELidaComOsCamposDentroDoInfRej()
    {
        var xml = "<pedRegEvento versao=\"1.00\" xmlns=\"http://www.sped.fazenda.gov.br/nfse\"><infPedReg Id=\"PRE" + DpsTeste.Chave + "203206\">"
            + "<tpAmb>2</tpAmb><verAplic>ERP 0.9</verAplic><dhEvento>2025-08-02T10:00:00-03:00</dhEvento><CNPJAutor>" + DpsTeste.CnpjTomador + "</CNPJAutor>"
            + "<chNFSe>" + DpsTeste.Chave + "</chNFSe><e203206><xDesc>Rejeição do Tomador</xDesc><infRej><cMotivo>3</cMotivo><xMotivo>Sem fato gerador</xMotivo></infRej></e203206>"
            + "</infPedReg></pedRegEvento>";
        Assert.Empty(SchemasNFSe101.Pedido100.Validar(xml));

        var lido = LeiauteNFSe101.Instancia.LerEvento(xml);

        Assert.Empty(lido.Problemas);
        Assert.Equal("1.00", lido.Versao);
        var rejeicao = Assert.IsType<RejeicaoTomador>(lido.Evento);
        Assert.Equal((MotivoRejeicao.FatoGeradorInexistente, "Sem fato gerador"), (rejeicao.Motivo, rejeicao.Descricao));
    }

    [Fact]
    public void OAmbienteEAVersaoDoAplicativoNulosValemOsDoNucleoEOPadrao()
    {
        var evento = Criar("101101");
        evento.Ambiente = null;
        evento.VersaoAplicativo = null;

        var escrito = EscritorNFSe.EscreverPedido(evento, Ambiente.Producao);

        Assert.Contains("<tpAmb>1</tpAmb>", escrito.Xml, StringComparison.Ordinal);
        Assert.Contains("<verAplic>OpenDFe ", escrito.Xml, StringComparison.Ordinal);
        Assert.Empty(SchemasNFSe101.Pedido.Validar(escrito.Xml));
    }

    [Fact]
    public void AChaveInvalidaDeixaOPedidoSemIdComOProblema()
    {
        var evento = Criar("101101");
        evento.Chave = "123";

        var escrito = EscritorNFSe.EscreverPedido(evento, null);

        Assert.Null(escrito.Id);
        Assert.Equal(("NFSE-ID", "Chave"), (Assert.Single(escrito.Problemas).Codigo, escrito.Problemas[0].Propriedade));
    }

    [Fact]
    public void OXDescDeOutroTipoEAviso()
    {
        var xml = EscritorNFSe.EscreverPedido(Criar("101101"), null).Xml.Replace("<xDesc>Cancelamento de NFS-e</xDesc>", "<xDesc>Outro</xDesc>", StringComparison.Ordinal);

        var problema = Assert.Single(LeiauteNFSe101.Instancia.LerEvento(xml).Problemas);

        Assert.Equal(("NFSE-EVENTO", Severidade.Aviso, "xDesc"), (problema.Codigo, problema.Severidade, problema.Tag));
    }

    [Fact]
    public void OTipoDesconhecidoEErroSemOEvento()
    {
        var xml = EscritorNFSe.EscreverPedido(Criar("101101"), null).Xml.Replace("e101101", "e999999", StringComparison.Ordinal);

        var lido = LeiauteNFSe101.Instancia.LerEvento(xml);

        Assert.Null(lido.Evento);
        Assert.Contains(lido.Problemas, p => p.Codigo == "NFSE-EVENTO" && p.Severidade == Severidade.Erro);
    }

    [Fact]
    public void OTipoDoIdDiferenteDoDetalheEErro()
    {
        var xml = EscritorNFSe.EscreverPedido(Criar("101101"), null).Xml.Replace("101101\"", "101103\"", StringComparison.Ordinal);

        var problema = Assert.Single(LeiauteNFSe101.Instancia.LerEvento(xml).Problemas);

        Assert.Equal(("NFSE-EVENTO", "Id"), (problema.Codigo, problema.Tag));
    }

    // O evento registrado em volta do pedido, assinado pelo certificado de teste no papel da Sefin (o schema exige a Signature).
    internal static string Registrado(string pedido, string tipo)
    {
        var id = "EVT" + DpsTeste.Chave + tipo + "001";
        var texto = new StringBuilder();
        using (var xml = XmlWriter.Create(texto, new XmlWriterSettings { OmitXmlDeclaration = true }))
        {
            xml.WriteStartElement("evento", "http://www.sped.fazenda.gov.br/nfse");
            xml.WriteAttributeString("versao", "1.01");
            xml.WriteStartElement("infEvento", "http://www.sped.fazenda.gov.br/nfse");
            xml.WriteAttributeString("Id", id);
            xml.WriteElementString("verAplic", "http://www.sped.fazenda.gov.br/nfse", "SefinNac_1.0");
            xml.WriteElementString("ambGer", "http://www.sped.fazenda.gov.br/nfse", "2");
            xml.WriteElementString("nSeqEvento", "http://www.sped.fazenda.gov.br/nfse", "1");
            xml.WriteElementString("dhProc", "http://www.sped.fazenda.gov.br/nfse", "2026-11-20T09:00:05-03:00");
            xml.WriteElementString("nDFSe", "http://www.sped.fazenda.gov.br/nfse", "55");
            xml.WriteRaw(pedido);
            xml.WriteEndElement();
            xml.WriteEndElement();
        }
        return AssinaturaDigital.Assinar(texto.ToString(), id, NFSeTeste.Certificado);
    }
}
