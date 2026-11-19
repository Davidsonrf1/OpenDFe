using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// Os eventos da NFS-e na Sefin emulada (E.8): o POST /nfse/{chave}/eventos (o cancelamento e a análise fiscal, com as
// regras do Anexo II), a consulta dos eventos, a substituição pela DPS com o subst (o 105102 gerado pela Sefin), os
// eventos do município pela rota de controle e a matriz de eventos.
public sealed class EventosNFSeTestes : IClassFixture<EmuladorFixture>
{
    private static readonly XNamespace N = DpsTeste.Namespace;
    private readonly EmuladorFixture _fixture;

    public EventosNFSeTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Municipios.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Uri Controle(string caminho) => new(_fixture.Emulador.Endereco, "_controle/" + caminho);

    private Task<string> EmitirAsync(DpsTeste? dps = null) => Sefin.EmitirChaveAsync(_fixture, dps ?? new DpsTeste());

    private Task<(HttpResponseMessage Resposta, JsonElement Json)> RegistrarAsync(PedidoEventoTeste pedido) => Sefin.RegistrarEventoAsync(_fixture, pedido);

    private async Task RejeitaAsync(PedidoEventoTeste pedido, string codigo)
    {
        var (resposta, json) = await RegistrarAsync(pedido);
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(codigo, Sefin.Erro(json));
    }

    private async Task<JsonElement> EventosAsync(string chave, string? resto = null)
    {
        var (resposta, json) = await Sefin.ObterAsync(_fixture, $"nfse/{chave}/eventos{resto}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return json.GetProperty("eventos");
    }

    private async Task<HttpResponseMessage> MunicipioAsync(string chave, object pedido) =>
        await _fixture.Http.PostAsJsonAsync(Controle($"nfse/{chave}/eventos"), pedido, Ct);

    private async Task<string> SituacaoAsync(string chave)
    {
        using var nota = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle("nfse/" + chave), Ct));
        return nota.RootElement.GetProperty("situacao").GetString()!;
    }

    // O cancelamento e o evento gerado

    [Fact]
    public async Task CancelaANFSeComOEventoValidoNoSchemaEAssinadoPelaAc()
    {
        var chave = await EmitirAsync();
        var pedido = new PedidoEventoTeste { Chave = chave };

        var (resposta, json) = await RegistrarAsync(pedido);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal(2, json.GetProperty("tipoAmbiente").GetInt32());
        Assert.Equal(chave, json.GetProperty("chaveAcesso").GetString());
        Assert.Equal("101101", json.GetProperty("tipoEvento").GetString());
        Assert.Equal(1, json.GetProperty("numSeqEvento").GetInt32());
        var xml = Sefin.Evento(json);
        Assert.Empty(SchemasLeiaute.EventoNFSe.Validar(xml));

        // A assinatura do evento confere com a AC, e a do pedido, de dentro, continua valendo.
        var documento = new XmlDocument { PreserveWhitespace = true };
        documento.LoadXml(xml);
        var assinatura = new SignedXml(documento);
        assinatura.LoadXml((XmlElement)documento.DocumentElement!.LastChild!);
        var id = "EVT" + chave + "101101" + "001";
        Assert.True(assinatura.CheckSignature(_fixture.Emulador.Autoridade, verifySignatureOnly: true));
        Assert.Equal("#" + id, assinatura.SignedInfo!.References.Cast<Reference>().Single().Uri);
        Assert.Empty(AssinaturaDigital.Verificar(xml, pedido.IdComposto, pedido.Cnpj!));

        var inf = XDocument.Parse(xml).Root!.Element(N + "infEvento")!;
        Assert.Equal(id, inf.Attribute("Id")!.Value);
        Assert.Equal("2", inf.Element(N + "ambGer")!.Value);
        Assert.Equal("1", inf.Element(N + "nSeqEvento")!.Value);
        var nota = _fixture.Emulador.NFSe.Chave(chave)!;
        Assert.True(long.Parse(inf.Element(N + "nDFSe")!.Value, System.Globalization.CultureInfo.InvariantCulture) > nota.NumeroDfse);
        Assert.Equal(pedido.IdComposto, inf.Element(N + "pedRegEvento")!.Element(N + "infPedReg")!.Attribute("Id")!.Value);
        Assert.Equal("cancelada", await SituacaoAsync(chave));
    }

    [Fact]
    public async Task ConsultaOsEventosPorChaveTipoESequencia()
    {
        var chave = await EmitirAsync();
        await RegistrarAsync(new PedidoEventoTeste { Chave = chave });

        var todos = await EventosAsync(chave);
        var doTipo = await EventosAsync(chave, "/101101");
        var exato = await EventosAsync(chave, "/101101/1");
        var outroTipo = await EventosAsync(chave, "/101103");
        var (semSequencia, semSequenciaJson) = await Sefin.ObterAsync(_fixture, $"nfse/{chave}/eventos/101101/2");
        var (semNota, semNotaJson) = await Sefin.ObterAsync(_fixture, $"nfse/{new string('1', 50)}/eventos");
        var (invalida, invalidaJson) = await Sefin.ObterAsync(_fixture, $"nfse/{chave}/eventos/1011");

        Assert.Equal("101101", Assert.Single(todos.EnumerateArray()).GetProperty("tipoEvento").GetString());
        Assert.Single(doTipo.EnumerateArray());
        Assert.Contains("<chNFSe>" + chave + "</chNFSe>", Sefin.Evento(Assert.Single(exato.EnumerateArray())), StringComparison.Ordinal);
        Assert.Empty(outroTipo.EnumerateArray());
        Assert.Equal(HttpStatusCode.NotFound, semSequencia.StatusCode);
        Assert.Equal("EMULADA-EVENTO", Sefin.Erro(semSequenciaJson));
        Assert.Equal(HttpStatusCode.NotFound, semNota.StatusCode);
        Assert.Equal("E0044", Sefin.Erro(semNotaJson));
        Assert.Equal(HttpStatusCode.BadRequest, invalida.StatusCode);
        Assert.Equal("EMULADA-CONSULTA", Sefin.Erro(invalidaJson));
    }

    [Fact]
    public async Task ANFSeSemEventosTemAListaVazia()
    {
        var chave = await EmitirAsync();

        Assert.Empty((await EventosAsync(chave)).EnumerateArray());
        Assert.Equal("normal", await SituacaoAsync(chave));
    }

    [Fact]
    public async Task CancelaComOCpfDoPrestador()
    {
        var chave = await EmitirAsync(new DpsTeste { Cnpj = null, Cpf = "52998224725", Certificado = Clientes.CertificadoCpf });

        var (resposta, _) = await RegistrarAsync(new PedidoEventoTeste { Chave = chave, Cnpj = null, Cpf = "52998224725", Certificado = Clientes.CertificadoCpf });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    // A matriz de eventos (aba RN_EVENTOSxEVENTOS)

    [Fact]
    public async Task RejeitaOCancelamentoDaNFSeJaCancelada()
    {
        var chave = await EmitirAsync();
        await RegistrarAsync(new PedidoEventoTeste { Chave = chave });

        var (resposta, json) = await RegistrarAsync(new PedidoEventoTeste { Chave = chave });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("E0840", Sefin.Erro(json));
        Assert.Contains("o evento de Cancelamento de NFS-e já está vinculado", json.GetProperty("erros")[0].GetProperty("descricao").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task AAnaliseFiscalPendenteImpedeOCancelamentoOutraAnaliseEASubstituicao()
    {
        var chave = await EmitirAsync();
        var (analise, _) = await RegistrarAsync(new PedidoEventoTeste { Chave = chave, Tipo = "101103" });

        Assert.Equal(HttpStatusCode.Created, analise.StatusCode);
        Assert.Equal("emAnalise", await SituacaoAsync(chave));
        await RejeitaAsync(new PedidoEventoTeste { Chave = chave }, "E0840");
        await RejeitaAsync(new PedidoEventoTeste { Chave = chave, Tipo = "101103" }, "E0848");
        var (_, substituta) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = chave });
        Assert.Equal("E0068", Sefin.Erro(substituta));
    }

    [Fact]
    public async Task ODeferimentoDoMunicipioCancelaANFSe()
    {
        var chave = await EmitirAsync();
        await RegistrarAsync(new PedidoEventoTeste { Chave = chave, Tipo = "101103" });

        using var deferido = await MunicipioAsync(chave, new { tipo = "105104", cpfAgente = "52998224725", processo = "123", descricao = "Cancelamento deferido no teste" });
        using var indeferido = await MunicipioAsync(chave, new { tipo = "105105", cpfAgente = "52998224725", descricao = "Cancelamento indeferido no teste" });

        Assert.Equal(HttpStatusCode.Created, deferido.StatusCode);
        Assert.Equal("cancelada", await SituacaoAsync(chave));
        Assert.Equal(HttpStatusCode.BadRequest, indeferido.StatusCode);
        Assert.Contains("E0856", await indeferido.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        var eventos = await EventosAsync(chave);
        Assert.Equal(["101103", "105104"], eventos.EnumerateArray().Select(e => e.GetProperty("tipoEvento").GetString()));
        var xml = Sefin.Evento(eventos[1]);
        Assert.Empty(SchemasLeiaute.EventoNFSe.Validar(xml));
        Assert.Contains("<CPFAutor>52998224725</CPFAutor>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ODeferimentoSemAnaliseERecusado()
    {
        var chave = await EmitirAsync();

        using var deferido = await MunicipioAsync(chave, new { tipo = "105104", cpfAgente = "52998224725", descricao = "Cancelamento deferido no teste" });

        Assert.Equal(HttpStatusCode.BadRequest, deferido.StatusCode);
        Assert.Contains("E0853", await deferido.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task DepoisDoIndeferimentoSoOOficioCancela()
    {
        var chave = await EmitirAsync();
        await RegistrarAsync(new PedidoEventoTeste { Chave = chave, Tipo = "101103" });
        using var indeferido = await MunicipioAsync(chave, new { tipo = "105105", cpfAgente = "52998224725", motivo = "2", descricao = "Cancelamento indeferido no teste" });

        Assert.Equal(HttpStatusCode.Created, indeferido.StatusCode);
        Assert.Equal("normal", await SituacaoAsync(chave));
        await RejeitaAsync(new PedidoEventoTeste { Chave = chave }, "E0840");
        using var oficio = await MunicipioAsync(chave, new { tipo = "305101", cpfAgente = "52998224725", processo = "456", descricao = "Processo administrativo de teste" });
        Assert.Equal(HttpStatusCode.Created, oficio.StatusCode);
        Assert.Equal("cancelada", await SituacaoAsync(chave));
    }

    [Theory]
    [InlineData("{\"tipo\":\"101101\",\"cpfAgente\":\"52998224725\",\"descricao\":\"Descrição longa o bastante\"}", "O tipo é")]
    [InlineData("{\"tipo\":\"105104\",\"cpfAgente\":\"52998224720\",\"descricao\":\"Descrição longa o bastante\"}", "cpfAgente")]
    [InlineData("{\"tipo\":\"105104\",\"cpfAgente\":\"52998224725\",\"descricao\":\"curta\"}", "xMotivo")]
    [InlineData("{\"tipo\":\"305101\",\"cpfAgente\":\"52998224725\",\"descricao\":\"Descrição longa o bastante\"}", "nProcAdm")]
    public async Task RecusaOEventoDoMunicipioMalFormado(string corpo, string trecho)
    {
        var chave = await EmitirAsync();
        using var conteudo = new StringContent(corpo, System.Text.Encoding.UTF8, "application/json");

        using var resposta = await _fixture.Http.PostAsync(Controle($"nfse/{chave}/eventos"), conteudo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(trecho, await resposta.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OEventoDoMunicipioSemANFSeE404()
    {
        using var resposta = await MunicipioAsync(new string('1', 50), new { tipo = "305101", cpfAgente = "52998224725" });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // As regras do pedido (aba RN_EVENTO_PED.REG.EVENTO)

    [Fact]
    public async Task RejeitaOPedidoSemAssinatura() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Certificado = null }, "E1989");

    [Fact]
    public async Task RejeitaOAutorQueNaoEOTitularDoCertificado() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Certificado = Clientes.OutroCertificado }, "E0812");

    [Fact]
    public async Task RejeitaOCpfAutorQueNaoEOTitularDoCertificado() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Cnpj = null, Cpf = "52998224725" }, "E0815");

    [Fact]
    public async Task RejeitaOAutorQueNaoEOPrestador() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Cnpj = "12345678000195", Certificado = Clientes.OutroCertificado }, "E0813");

    [Fact]
    public async Task RejeitaOIdQueNaoEAConcatenacao()
    {
        var chave = await EmitirAsync();
        await RejeitaAsync(new PedidoEventoTeste { Chave = chave, Id = "PRE" + chave + "101103" }, "E1827");
    }

    [Fact]
    public async Task RejeitaOAmbienteDeProducao() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Ambiente = "1" }, "E1845");

    [Fact]
    public async Task RejeitaOPedidoDoFuturo() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Data = NotaTeste.Agora().AddMinutes(10) }, "E1843");

    [Fact]
    public async Task RejeitaAChaveDoCaminhoDiferenteDaDoPedido()
    {
        var chave = await EmitirAsync();
        var outra = await EmitirAsync();

        var (resposta, json) = await Sefin.RegistrarEventoAsync(_fixture, outra, PedidoEventoTeste.Corpo(new PedidoEventoTeste { Chave = chave }.Gerar()));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("EMULADA-CHAVE", Sefin.Erro(json));
    }

    [Fact]
    public async Task RejeitaOTipoQueASefinNaoRecebeDeQuemEmite() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Tipo = "202201" }, "EMULADA-TIPO");

    [Fact]
    public async Task RejeitaAChaveQueNaoExiste() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = "3106200" + "2" + "2" + "11222333000181" + new string('9', 27) }, "E1831");

    [Fact]
    public async Task RejeitaOCancelamentoForaDoPrazoDoMunicipio()
    {
        var chave = await EmitirAsync();
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, PrazoCancelamento: 0));

        await RejeitaAsync(new PedidoEventoTeste { Chave = chave }, "E0822");
        var (analise, _) = await RegistrarAsync(new PedidoEventoTeste { Chave = chave, Tipo = "101103" });
        Assert.Equal(HttpStatusCode.Created, analise.StatusCode);
    }

    // A recepção (as regras da aba RN_RECEPCAO_DPS, também no pedido de evento)

    [Fact]
    public async Task RecusaOCorpoSemOCampo()
    {
        var chave = await EmitirAsync();

        var (resposta, json) = await Sefin.RegistrarEventoAsync(_fixture, chave, DpsTeste.Corpo(new PedidoEventoTeste { Chave = chave }.Gerar()));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("E1225", Sefin.Erro(json));
    }

    [Fact]
    public async Task RecusaADpsNaRotaDosEventos()
    {
        var chave = await EmitirAsync();

        var (_, json) = await Sefin.RegistrarEventoAsync(_fixture, chave, PedidoEventoTeste.Corpo(new DpsTeste().Gerar()));

        Assert.Equal("E1242", Sefin.Erro(json));
    }

    [Fact]
    public async Task RecusaOPedidoForaDoSchema() =>
        await RejeitaAsync(new PedidoEventoTeste { Chave = await EmitirAsync(), Descricao = "curta" }, "E1235");

    [Fact]
    public async Task RecusaOConteudoQueNaoEJson()
    {
        var chave = await EmitirAsync();

        var (resposta, _) = await Sefin.RegistrarEventoAsync(_fixture, chave, PedidoEventoTeste.Corpo(new PedidoEventoTeste { Chave = chave }.Gerar()), "text/plain");

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, resposta.StatusCode);
    }

    // A substituição (a DPS com o subst)

    [Fact]
    public async Task ASubstituicaoGeraASubstitutaEO105102SobreASubstituida()
    {
        var substituida = await EmitirAsync();

        var (resposta, json) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = substituida });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var substituta = json.GetProperty("chaveAcesso").GetString()!;
        var evento = Assert.Single((await EventosAsync(substituida)).EnumerateArray());
        Assert.Equal("105102", evento.GetProperty("tipoEvento").GetString());
        var xml = Sefin.Evento(evento);
        Assert.Empty(SchemasLeiaute.EventoNFSe.Validar(xml));
        var detalhe = XDocument.Parse(xml).Descendants(N + "e105102").Single();
        Assert.Equal(substituta, detalhe.Element(N + "chSubstituta")!.Value);
        Assert.Equal("99", detalhe.Element(N + "cMotivo")!.Value);
        Assert.Equal("Outro motivo de teste", detalhe.Element(N + "xMotivo")!.Value);
        Assert.Equal("substituida", await SituacaoAsync(substituida));
        Assert.Equal("normal", await SituacaoAsync(substituta));
        Assert.True(_fixture.Emulador.NFSe.Eventos(substituida)[0].NumeroDfse > _fixture.Emulador.NFSe.Chave(substituta)!.NumeroDfse);
    }

    [Fact]
    public async Task ASubstituidaNaoESubstituidaDeNovoNemCancelada()
    {
        var substituida = await EmitirAsync();
        await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = substituida });

        var (_, outra) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = substituida });

        Assert.Equal("E0046", Sefin.Erro(outra));
        var (_, cancelamento) = await RegistrarAsync(new PedidoEventoTeste { Chave = substituida });
        Assert.Equal("E0840", Sefin.Erro(cancelamento));
        Assert.Contains("Cancelamento de NFS-e por Substituição", cancelamento.GetProperty("erros")[0].GetProperty("descricao").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACanceladaNaoESubstituida()
    {
        var chave = await EmitirAsync();
        await RegistrarAsync(new PedidoEventoTeste { Chave = chave });

        var (_, json) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = chave });

        Assert.Equal("E0046", Sefin.Erro(json));
    }

    [Fact]
    public async Task DuasSubstituicoesAoMesmoTempoSoUmaPassa()
    {
        var substituida = await EmitirAsync();

        var respostas = await Task.WhenAll(
            Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = substituida }),
            Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = substituida }));

        Assert.Single(respostas, r => r.Resposta.StatusCode == HttpStatusCode.Created);
        Assert.Single(respostas, r => Sefin.Erro(r.Json) == "E0046");
        Assert.Single(_fixture.Emulador.NFSe.Eventos(substituida));
    }

    [Fact]
    public async Task RejeitaASubstituidaDeOutroPrestador()
    {
        var chave = await EmitirAsync(new DpsTeste { Cnpj = "11444777000161", Certificado = Clientes.CertificadoTomador });

        var (_, json) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = chave });

        Assert.Equal("E0042", Sefin.Erro(json));
    }

    [Fact]
    public async Task RejeitaASubstituidaQueNaoExiste()
    {
        var (_, json) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = "3106200" + "2" + "2" + "11222333000181" + new string('9', 27) });

        Assert.Equal("E0044", Sefin.Erro(json));
    }

    [Fact]
    public async Task RejeitaOMotivoOutrosSemDescricao()
    {
        var chave = await EmitirAsync();

        var (_, json) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = chave, DescricaoSubstituicao = null });

        Assert.Equal("E0078", Sefin.Erro(json));
    }

    [Fact]
    public async Task RejeitaASubstituicaoForaDoPrazoSalvoOEnquadramentoNoSimples()
    {
        var chave = await EmitirAsync();
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, PrazoSubstituicao: 0));

        var (_, fora) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = chave });
        var (dentro, _) = await Sefin.EmitirAsync(_fixture, new DpsTeste { Substituida = chave, MotivoSubstituicao = "01", DescricaoSubstituicao = null });

        Assert.Equal("E0050", Sefin.Erro(fora));
        Assert.Equal(HttpStatusCode.Created, dentro.StatusCode);
    }

    // As falhas e o controle

    [Fact]
    public async Task AQuedaNoMeioDoEventoDeixaOEventoRegistrado()
    {
        var chave = await EmitirAsync();
        using var falha = await _fixture.Http.PostAsJsonAsync(Controle("falhas"),
            new { tipo = "conexaoDerrubada", depois = true, destino = "SefinNacional", servico = "EventoNFSe", vezes = 1 }, Ct);
        Assert.Equal(HttpStatusCode.Created, falha.StatusCode);

        await Assert.ThrowsAsync<HttpRequestException>(() => RegistrarAsync(new PedidoEventoTeste { Chave = chave }));

        Assert.Single((await EventosAsync(chave)).EnumerateArray());
    }

    [Fact]
    public async Task OControleMostraANFSeComOsEventos()
    {
        var chave = await EmitirAsync();
        await RegistrarAsync(new PedidoEventoTeste { Chave = chave });

        using var nota = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle("nfse/" + chave), Ct));
        using var lista = JsonDocument.Parse(await _fixture.Http.GetStringAsync(Controle("nfse"), Ct));

        var evento = Assert.Single(nota.RootElement.GetProperty("eventos").EnumerateArray());
        Assert.Equal("101101", evento.GetProperty("tipo").GetString());
        Assert.StartsWith("<evento", evento.GetProperty("xml").GetString(), StringComparison.Ordinal);
        Assert.Equal("cancelada", lista.RootElement.EnumerateArray().Single(n => n.GetProperty("chave").GetString() == chave).GetProperty("situacao").GetString());
    }
}
