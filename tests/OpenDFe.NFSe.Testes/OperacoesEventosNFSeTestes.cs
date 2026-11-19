using System.IO.Compression;
using System.Text;
using System.Text.Json;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// Os eventos da 9.3 sem rede (docs/NFSE.md 3.3 e D9): os parâmetros, a montagem da consulta e a leitura das respostas da Sefin
// pelo JSON (o evento registrado, a lista da consulta, os erros, o que não se lê). A ponta a ponta está em EventosEmuladosTestes.
public class OperacoesEventosNFSeTestes
{
    private static readonly ChaveNFSe Chave = ChaveNFSe.Ler(DpsTeste.Chave);

    private static string Compactar(string texto)
    {
        using var saida = new MemoryStream();
        using (var gzip = new GZipStream(saida, CompressionLevel.Optimal, leaveOpen: true))
            gzip.Write(Encoding.UTF8.GetBytes(texto));
        return Convert.ToBase64String(saida.ToArray());
    }

    private static string Json(object valor) => JsonSerializer.Serialize(valor);

    private static CancelamentoNFSe Cancelamento() => new()
    {
        Ambiente = Ambiente.Homologacao,
        Data = new DateTimeOffset(2026, 11, 20, 9, 0, 0, TimeSpan.FromHours(-3)),
        CnpjAutor = DpsTeste.CnpjPrestador,
        Chave = DpsTeste.Chave,
        Motivo = MotivoCancelamento.ErroEmissao,
        Descricao = "Erro no valor do serviço",
    };

    // O evento registrado de um tipo, como a Sefin o devolve.
    private static string EventoXml(EventoNFSe pedido) =>
        EventosNFSeTestes.Registrado(EscritorNFSe.EscreverPedido(pedido, null).Xml, pedido.Tipo);

    // Os parâmetros

    [Fact]
    public void OEnvioRecusaOEventoNulo() =>
        Assert.Throws<ArgumentNullException>(() => new EnvioEventoNFSe(null!));

    [Theory]
    [MemberData(nameof(EventosNFSeTestes.Tipos), MemberType = typeof(EventosNFSeTestes))]
    public void CadaTipoDoXSDEAceitoOuRecusadoPeloAutor(string tipo)
    {
        var evento = EventosNFSeTestes.UmDeCada().First(e => e.Tipo == tipo);

        if (tipo is "101101" or "101103")
            Assert.Same(evento, new EnvioEventoNFSe(evento).Evento);
        else
            Assert.Contains(tipo, Assert.Throws<ArgumentException>(() => new EnvioEventoNFSe(evento)).Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AConsultaRecusaAChaveVaziaOTipoEONumeroForaDaForma()
    {
        Assert.Throws<ArgumentException>(() => new ConsultaEventosNFSe(default));
        Assert.Throws<ArgumentException>(() => new ConsultaEventosNFSe(Chave) { Tipo = "10110" });
        Assert.Throws<ArgumentException>(() => new ConsultaEventosNFSe(Chave) { Tipo = "10110A" });
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConsultaEventosNFSe(Chave) { Tipo = "101101", Sequencia = 0 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConsultaEventosNFSe(Chave) { Tipo = "101101", Sequencia = 1000 });
    }

    [Fact]
    public void AConsultaMontaOCaminhoComOTipoEONumero()
    {
        var operacao = new OperacaoConsultaEventosNFSe();

        var todos = operacao.Montar(new ConsultaEventosNFSe(Chave), null!);
        var tipo = operacao.Montar(new ConsultaEventosNFSe(Chave) { Tipo = "105102" }, null!);
        var numero = operacao.Montar(new ConsultaEventosNFSe(Chave) { Tipo = "105102", Sequencia = 1 }, null!);

        Assert.Equal((MetodoRest.Get, $"nfse/{DpsTeste.Chave}/eventos", "SefinNacional"), (todos.Metodo, todos.Caminho, todos.Servico));
        Assert.Equal($"nfse/{DpsTeste.Chave}/eventos/105102", tipo.Caminho);
        Assert.Equal($"nfse/{DpsTeste.Chave}/eventos/105102/1", numero.Caminho);
        Assert.All([todos, tipo, numero], p =>
        {
            Assert.Null(p.Xml);
            Assert.Equal(Destino.SefinNacional, p.Destino);
        });
        Assert.Throws<ArgumentException>(() => operacao.Montar(new ConsultaEventosNFSe(Chave) { Sequencia = 1 }, null!));
    }

    // A leitura do registro

    [Fact]
    public void ORegistroTrazOEventoLidoComOPedido()
    {
        var xml = EventoXml(Cancelamento());
        var corpo = Json(new { tipoAmbiente = 2, chaveAcesso = DpsTeste.Chave, tipoEvento = "101101", numSeqEvento = 1, eventoXmlGZipB64 = Compactar(xml) });

        var leitura = new OperacaoEnvioEventoNFSe().Ler(new EnvioEventoNFSe(Cancelamento()), "<pedRegEvento/>", new RespostaRest(201, corpo), null!);

        Assert.Equal((201, "Evento registrado"), (leitura.CStat, leitura.Motivo));
        Assert.True(leitura.Retorno.Registrado);
        var evento = leitura.Retorno.Evento!;
        Assert.Equal(("101101", 1, xml), (evento.Tipo, evento.Sequencia, evento.Xml));
        Assert.Equal(new DateTimeOffset(2026, 11, 20, 9, 0, 5, TimeSpan.FromHours(-3)), evento.Processamento);
        var pedido = Assert.IsType<CancelamentoNFSe>(evento.Registrado!.Pedido);
        Assert.Equal(MotivoCancelamento.ErroEmissao, pedido.Motivo);
        Assert.Empty(leitura.Retorno.Erros);
        Assert.Empty(leitura.Problemas);
    }

    [Fact]
    public void ARejeicaoDoRegistroTrazOsErros()
    {
        var corpo = Json(new { erros = new[] { new { codigo = "E0840", descricao = "Já existe o evento de cancelamento" } } });

        var leitura = new OperacaoEnvioEventoNFSe().Ler(new EnvioEventoNFSe(Cancelamento()), "<pedRegEvento/>", new RespostaRest(400, corpo), null!);

        Assert.Equal(400, leitura.CStat);
        Assert.StartsWith("E0840: ", leitura.Motivo, StringComparison.Ordinal);
        Assert.False(leitura.Retorno.Registrado);
        Assert.Null(leitura.Retorno.Evento);
    }

    [Fact]
    public void OProblemaDaLeituraDoEventoVaiNosProblemasDoResultado()
    {
        var xml = EventoXml(Cancelamento()).Replace("<xDesc>Cancelamento de NFS-e</xDesc>", "<xDesc>Outro</xDesc>", StringComparison.Ordinal);
        var corpo = Json(new { tipoEvento = "101101", numSeqEvento = 1, eventoXmlGZipB64 = Compactar(xml) });

        var leitura = new OperacaoEnvioEventoNFSe().Ler(new EnvioEventoNFSe(Cancelamento()), null, new RespostaRest(201, corpo), null!);

        Assert.True(leitura.Retorno.Registrado);
        Assert.Equal("NFSE-EVENTO", Assert.Single(leitura.Problemas).Codigo);
    }

    [Fact]
    public void SemOXmlLidoOTipoONumeroEOProcessamentoVemDoJson()
    {
        var corpo = Json(new { tipoEvento = "101101", numSeqEvento = 2, dataHoraProcessamento = "2026-11-20T10:00:00-03:00", eventoXmlGZipB64 = Compactar("<outro/>") });

        var leitura = new OperacaoEnvioEventoNFSe().Ler(new EnvioEventoNFSe(Cancelamento()), null, new RespostaRest(201, corpo), null!);

        var evento = leitura.Retorno.Evento!;
        Assert.Null(evento.Registrado);
        Assert.Equal(("101101", 2), (evento.Tipo, evento.Sequencia));
        Assert.Equal(new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.FromHours(-3)), evento.Processamento);
        Assert.Contains(leitura.Problemas, p => p.Severidade == Severidade.Erro);
    }

    public static TheoryData<string> RegistrosIlegiveis() =>
    [
        "{\"tipoEvento\": \"101101\"}",
        "{}",
        "{\"tipoEvento\": \"101101\", \"numSeqEvento\": \"um\", \"eventoXmlGZipB64\": \"" + Compactar("<evento/>") + "\"}",
        "{\"tipoEvento\": \"101101\", \"eventoXmlGZipB64\": \"não é base64\"}",
    ];

    [Theory]
    [MemberData(nameof(RegistrosIlegiveis))]
    public void ORegistroSemOEventoOuForaDaFormaEFormatException(string corpo) =>
        Assert.Throws<FormatException>(() => new OperacaoEnvioEventoNFSe().Ler(new EnvioEventoNFSe(Cancelamento()), null, new RespostaRest(201, corpo), null!));

    // A leitura da consulta

    [Fact]
    public void AConsultaLeOsEventosDeQualquerAutorNaOrdem()
    {
        var cancelamento = EventoXml(Cancelamento());
        var deferido = EventoXml(new CancelamentoDeferido
        {
            Ambiente = Ambiente.Homologacao, Data = new DateTimeOffset(2026, 11, 20, 9, 0, 0, TimeSpan.FromHours(-3)), CpfAutor = DpsTeste.Cpf,
            Chave = DpsTeste.Chave, CpfAgente = DpsTeste.Cpf, Motivo = MotivoDeferimento.Deferido, Descricao = "Deferido pelo fisco",
        });
        var corpo = Json(new
        {
            chaveAcesso = DpsTeste.Chave,
            eventos = new[]
            {
                new { chaveAcesso = DpsTeste.Chave, tipoEvento = "101101", numSeqEvento = 1, eventoXmlGZipB64 = Compactar(cancelamento) },
                new { chaveAcesso = DpsTeste.Chave, tipoEvento = "105104", numSeqEvento = 1, eventoXmlGZipB64 = Compactar(deferido) },
            },
        });

        var leitura = new OperacaoConsultaEventosNFSe().Ler(new ConsultaEventosNFSe(Chave), null, new RespostaRest(200, corpo), null!);

        Assert.Equal((200, "Eventos encontrados"), (leitura.CStat, leitura.Motivo));
        Assert.True(leitura.Retorno.Encontrada);
        Assert.Collection(leitura.Retorno.Eventos,
            e => Assert.IsType<CancelamentoNFSe>(e.Registrado!.Pedido),
            e =>
            {
                Assert.Equal("105104", e.Tipo);
                Assert.Equal(DpsTeste.Cpf, Assert.IsType<CancelamentoDeferido>(e.Registrado!.Pedido).CpfAgente);
            });
        Assert.Empty(leitura.Problemas);
    }

    [Fact]
    public void AConsultaSemEventosEOQueNaoExiste()
    {
        var operacao = new OperacaoConsultaEventosNFSe();

        var vazia = operacao.Ler(new ConsultaEventosNFSe(Chave), null, new RespostaRest(200, Json(new { chaveAcesso = DpsTeste.Chave, eventos = Array.Empty<object>() })), null!);
        var ausente = operacao.Ler(new ConsultaEventosNFSe(Chave), null, new RespostaRest(404, Json(new { erros = new[] { new { codigo = "E0044", descricao = "NFS-e não existe" } } })), null!);

        Assert.True(vazia.Retorno.Encontrada);
        Assert.Empty(vazia.Retorno.Eventos);
        Assert.False(ausente.Retorno.Encontrada);
        Assert.Empty(ausente.Retorno.Eventos);
        Assert.Equal("E0044", Assert.Single(ausente.Retorno.Erros).Codigo);
    }

    [Fact]
    public void AConsultaPeloNumeroAceitaOEventoForaDaLista()
    {
        var corpo = Json(new { chaveAcesso = DpsTeste.Chave, tipoEvento = "101101", numSeqEvento = 1, eventoXmlGZipB64 = Compactar(EventoXml(Cancelamento())) });

        var leitura = new OperacaoConsultaEventosNFSe().Ler(new ConsultaEventosNFSe(Chave) { Tipo = "101101", Sequencia = 1 }, null, new RespostaRest(200, corpo), null!);

        Assert.Equal("101101", Assert.Single(leitura.Retorno.Eventos).Tipo);
    }

    public static TheoryData<string> ConsultasIlegiveis() =>
    [
        "{\"chaveAcesso\": \"" + DpsTeste.Chave + "\"}",
        "{\"eventos\": {\"tipoEvento\": \"101101\"}}",
        "{\"eventos\": [1]}",
        "{\"eventos\": [{\"tipoEvento\": \"101101\"}]}",
    ];

    [Theory]
    [MemberData(nameof(ConsultasIlegiveis))]
    public void AConsultaSemAListaOuForaDaFormaEFormatException(string corpo) =>
        Assert.Throws<FormatException>(() => new OperacaoConsultaEventosNFSe().Ler(new ConsultaEventosNFSe(Chave), null, new RespostaRest(200, corpo), null!));
}
