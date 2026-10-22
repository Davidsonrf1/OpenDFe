using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// A autorização assíncrona (MOC 7.0, Visão Geral, 5.1.4.2) e o NFeRetAutorizacao4 (5.2), com o lote em
// processamento, a queda depois de autorizar e a marca no infCpl (PROJETO.md 13.5).
public sealed class RetAutorizacaoTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public RetAutorizacaoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Controle.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => _fixture.Emulador.Controle.Limpar();

    private Uri Autorizacao(string destino = "MG") => _fixture.Url(destino, Servico.NomeAutorizacao);

    private Uri Recibo(string destino = "MG") => _fixture.Url(destino, Servico.NomeRetAutorizacao);

    // O recibo do lote assíncrono.
    private async Task<string> EnviarAsync(string destino, params string[] notas)
    {
        var retorno = await _fixture.Http.AutorizarAsync(Autorizacao(destino), false, notas, Ct);
        Assert.Equal("103", retorno.Campo("cStat"));
        return retorno.Element(Mensagens.NFe + "infRec")!.Campo("nRec");
    }

    private Task<XElement> ConsultarAsync(string recibo, string destino = "MG", string ambiente = "2") =>
        _fixture.Http.ConsultarReciboAsync(Recibo(destino), recibo, Ct, ambiente);

    // Recibo

    [Fact]
    public async Task DevolveORecibo()
    {
        var retorno = await _fixture.Http.AutorizarAsync(Autorizacao(), false, [new NotaTeste().Gerar(), new NotaTeste().Gerar()], Ct);

        Assert.Equal(("103", "Lote recebido com sucesso", "31"), (retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("cUF")));
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "dhRecbto", "infRec"], retorno.Elements().Select(e => e.Name.LocalName));
        var recibo = retorno.Element(Mensagens.NFe + "infRec")!;
        Assert.Equal(["nRec", "tMed"], recibo.Elements().Select(e => e.Name.LocalName));
        // Visão Geral, 4.3.4: o cUF de onde o lote foi entregue, o tipo de autorizador e 12 posições.
        Assert.Matches("^311[0-9]{12}$", recibo.Campo("nRec"));
        Assert.Equal("1", recibo.Campo("tMed"));
    }

    [Theory]
    [InlineData("SVRS", "42", "433")]
    [InlineData("SVAN", "21", "914")]
    [InlineData("SVC-AN", "31", "912")]
    [InlineData("SVC-RS", "41", "433")]
    public async Task OReciboTrazACasaEOTipoDoAutorizador(string destino, string uf, string inicio)
    {
        var tipoEmissao = destino switch { "SVC-AN" => "6", "SVC-RS" => "7", _ => "1" };

        var recibo = await EnviarAsync(destino, new NotaTeste { Uf = uf, TipoEmissao = tipoEmissao }.Gerar(), new NotaTeste { Uf = uf, TipoEmissao = tipoEmissao }.Gerar());

        Assert.StartsWith(inicio, recibo, StringComparison.Ordinal);
        Assert.Equal(["100", "100"], (await ConsultarAsync(recibo, destino)).Elements(Mensagens.NFe + "protNFe").Select(p => p.Protocolo().Campo("cStat")));
    }

    [Fact]
    public async Task ConsultaOLoteProcessadoComOsProtocolosNaOrdem()
    {
        var autorizada = new NotaTeste();
        var rejeitada = new NotaTeste { Ambiente = "1" };
        var recibo = await EnviarAsync("MG", autorizada.Gerar(), rejeitada.Gerar());

        var retorno = await ConsultarAsync(recibo);

        Assert.Equal(Mensagens.NFe + "retConsReciNFe", retorno.Name);
        Assert.Equal("4.00", retorno.Attribute("versao")?.Value);
        Assert.Equal(["tpAmb", "verAplic", "nRec", "cStat", "xMotivo", "cUF", "dhRecbto", "protNFe", "protNFe"], retorno.Elements().Select(e => e.Name.LocalName));
        Assert.Equal((recibo, "104", "Lote processado", "31"), (retorno.Campo("nRec"), retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("cUF")));
        var protocolos = retorno.Elements(Mensagens.NFe + "protNFe").Select(p => p.Protocolo()).ToList();
        Assert.Equal([(autorizada.Chave, "100"), (rejeitada.Chave, "252")], protocolos.Select(p => (p.Campo("chNFe"), p.Campo("cStat"))));
        Assert.Contains(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == autorizada.Chave && n.Recibo == recibo);
    }

    [Fact]
    public async Task ConsultaOMesmoLoteMaisDeUmaVez()
    {
        var recibo = await EnviarAsync("MG", new NotaTeste().Gerar(), new NotaTeste().Gerar());

        var primeira = await ConsultarAsync(recibo);
        var segunda = await ConsultarAsync(recibo);

        Assert.Equal(string.Concat(primeira.Elements(Mensagens.NFe + "protNFe")), string.Concat(segunda.Elements(Mensagens.NFe + "protNFe")));
    }

    [Fact]
    public async Task ADuplicidadeDeLoteAssincronoLevaORecibo()
    {
        var xml = new NotaTeste().Gerar();
        var recibo = await EnviarAsync("MG", xml, new NotaTeste().Gerar());

        var info = Assert.Single((await _fixture.Http.AutorizarAsync(Autorizacao(), true, [xml], Ct)).Elements(Mensagens.NFe + "protNFe")).Protocolo();

        Assert.Equal(("204", $"Rejeição: Duplicidade de NF-e [nRec:{recibo}]"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Fact]
    public async Task ADuplicidadeDentroDoMesmoLote()
    {
        // A mesma nota duas vezes não passa do schema (o Id é xs:ID, único no lote): 225. Com outro cNF, a
        // segunda chega à duplicidade e vê a primeira, já autorizada.
        var nota = new NotaTeste();
        var xml = nota.Gerar();
        Assert.Equal("225", (await _fixture.Http.AutorizarAsync(Autorizacao(), false, [xml, xml], Ct)).Campo("cStat"));

        var recibo = await EnviarAsync("MG", xml, (nota with { Codigo = "56473829" }).Gerar());

        var protocolos = (await ConsultarAsync(recibo)).Elements(Mensagens.NFe + "protNFe").Select(p => p.Protocolo().Campo("cStat"));
        Assert.Equal(["100", "539"], protocolos);
    }

    [Fact]
    public async Task RejeitaOLoteAssincronoComUmaSoNota()
    {
        // NT 2025.001, GAP03a-3 (em produção desde 13/10/2025): a nota sozinha vai na resposta síncrona.
        var nota = new NotaTeste();

        var retorno = await _fixture.Http.AutorizarAsync(Autorizacao(), false, [nota.Gerar()], Ct);

        Assert.Equal(("452", "Rejeição: Solicitada resposta assíncrona para Lote com somente 1 (uma) NF-e"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "dhRecbto"], retorno.Elements().Select(e => e.Name.LocalName));
        Assert.DoesNotContain(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave);
    }

    // Regras da consulta

    [Fact]
    public async Task NaoLocalizaOLoteQueNaoExiste()
    {
        var retorno = await ConsultarAsync("311999999999999");

        Assert.Equal(("106", "Lote não localizado", "311999999999999"), (retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("nRec")));
        Assert.Null(retorno.Element(Mensagens.NFe + "protNFe"));
    }

    [Fact]
    public async Task NaoLocalizaNumAutorizadorOLoteDeOutro()
    {
        // SVRS e SVC-RS: mesma casa e mesmo tipo de autorizador, filas separadas.
        var recibo = await EnviarAsync("SVRS", new NotaTeste { Uf = "42" }.Gerar(), new NotaTeste { Uf = "42" }.Gerar());

        Assert.Equal("106", (await ConsultarAsync(recibo, "SVC-RS")).Campo("cStat"));
    }

    [Fact]
    public async Task RejeitaOReciboDeOutraUF()
    {
        var recibo = await EnviarAsync("SP", new NotaTeste { Uf = "35" }.Gerar(), new NotaTeste { Uf = "35" }.Gerar());

        var retorno = await ConsultarAsync(recibo);

        Assert.Equal(("248", "Rejeição: UF do Recibo diverge da UF autorizadora"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaOTipoDeAutorizadorDoRecibo()
    {
        Assert.Equal("553", (await ConsultarAsync("313000000000001")).Campo("cStat"));
    }

    [Fact]
    public async Task RejeitaAConsultaDeProducao()
    {
        Assert.Equal("252", (await ConsultarAsync("311000000000001", ambiente: "1")).Campo("cStat"));
    }

    [Theory]
    [InlineData("<consReciNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><nRec>31100000000000A</nRec></consReciNFe>", "215")]
    [InlineData("<consReciNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><tpAmb>2</tpAmb><nRec>311000000000001</nRec></consReciNFe>", "517")]
    [InlineData("<consReciNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"3.10\"><tpAmb>2</tpAmb><nRec>311000000000001</nRec></consReciNFe>", "239")]
    [InlineData("<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"/>", "516")]
    [InlineData("", "516")]
    public async Task RejeitaAConsultaQueNaoServeComOReciboZerado(string dados, string codigo)
    {
        using var resposta = await _fixture.Http.EnviarAsync(Recibo(), Mensagens.Envelope(dados, Servico.NomeRetAutorizacao), Ct);
        var retorno = Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(Ct));

        Assert.Equal((codigo, "000000000000000"), (retorno.Campo("cStat"), retorno.Campo("nRec")));
    }

    [Fact]
    public async Task SoOTransmissorDoLoteConsulta()
    {
        var recibo = await EnviarAsync("MG", new NotaTeste().Gerar(), new NotaTeste().Gerar());
        using var outro = Clientes.Criar(_fixture.Emulador.Autoridade, Clientes.OutroCertificado);

        var retorno = await outro.ConsultarReciboAsync(Recibo(), recibo, Ct);

        Assert.Equal("223", retorno.Campo("cStat"));
        Assert.Equal("Rejeição: CNPJ/CPF do transmissor do lote difere do CNPJ/CPF do transmissor da consulta", retorno.Campo("xMotivo"));
    }

    [Fact]
    public async Task OTransmissorPodeSerOutraEmpresaQueOEmitente()
    {
        // Visão Geral, 4.3.3: o transmissor não precisa ser o emitente.
        using var outro = Clientes.Criar(_fixture.Emulador.Autoridade, Clientes.OutroCertificado);
        var retorno = await outro.AutorizarAsync(Autorizacao(), false, [new NotaTeste().Gerar(), new NotaTeste().Gerar()], Ct);
        var recibo = retorno.Element(Mensagens.NFe + "infRec")!.Campo("nRec");

        var consulta = await outro.ConsultarReciboAsync(Recibo(), recibo, Ct);

        Assert.Equal(["100", "100"], consulta.Elements(Mensagens.NFe + "protNFe").Select(p => p.Protocolo().Campo("cStat")));
    }

    // Lote em processamento

    [Fact]
    public async Task RespondeEmProcessamentoPeloControle()
    {
        var recibo = await EnviarAsync("MG", new NotaTeste().Gerar(), new NotaTeste().Gerar());
        await ComandarAsync(new { tipo = "loteProcessando", destino = "MG", vezes = 2 });

        var primeira = await ConsultarAsync(recibo);
        var segunda = await ConsultarAsync(recibo);
        var terceira = await ConsultarAsync(recibo);

        Assert.Equal(("105", "Lote em processamento"), (primeira.Campo("cStat"), primeira.Campo("xMotivo")));
        Assert.Null(primeira.Element(Mensagens.NFe + "protNFe"));
        Assert.Equal(["105", "104"], new[] { segunda, terceira }.Select(r => r.Campo("cStat")));
    }

    [Fact]
    public async Task RespondeEmProcessamentoPelaMarca()
    {
        var recibo = await EnviarAsync("MG", new NotaTeste { Observacao = "Teste EMULADA:loteProcessando:2 do caixa" }.Gerar(), new NotaTeste().Gerar());

        var consultas = new List<string>();
        for (var i = 0; i < 3; i++)
            consultas.Add((await ConsultarAsync(recibo)).Campo("cStat"));

        Assert.Equal(["105", "105", "104"], consultas);
    }

    [Fact]
    public async Task OLoteProcessandoNaoEscondeOLoteQueNaoExiste()
    {
        await ComandarAsync(new { tipo = "loteProcessando" });

        Assert.Equal("106", (await ConsultarAsync("311999999999998")).Campo("cStat"));
    }

    // Queda

    [Fact]
    public async Task AutorizaEDerrubaAConexaoPeloControle()
    {
        await ComandarAsync(new { tipo = "conexaoDerrubada", destino = "MG", servico = Servico.NomeAutorizacao, depois = true, vezes = 1 });
        var nota = new NotaTeste();
        var xml = nota.Gerar();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _fixture.Http.EnviarAsync(Autorizacao(), Mensagens.Envelope(Mensagens.Lote(true, xml), Servico.NomeAutorizacao), Ct));

        // A SEFAZ autorizou e a resposta se perdeu (PROJETO.md 8.6): o reenvio é duplicidade.
        Assert.Contains(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave);
        var info = Assert.Single((await _fixture.Http.AutorizarAsync(Autorizacao(), true, [xml], Ct)).Elements(Mensagens.NFe + "protNFe")).Protocolo();
        Assert.Equal("204", info.Campo("cStat"));
    }

    [Fact]
    public async Task AutorizaEDerrubaAConexaoPelaMarca()
    {
        var nota = new NotaTeste { Observacao = "EMULADA:conexaoDerrubada:depois" };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _fixture.Http.EnviarAsync(Autorizacao(), Mensagens.Envelope(Mensagens.Lote(true, nota.Gerar()), Servico.NomeAutorizacao), Ct));

        Assert.Contains(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave);
    }

    [Fact]
    public async Task DerrubaAntesDeProcessarSemAutorizar()
    {
        var nota = new NotaTeste { Observacao = "EMULADA:conexaoDerrubada" };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _fixture.Http.EnviarAsync(Autorizacao(), Mensagens.Envelope(Mensagens.Lote(true, nota.Gerar()), Servico.NomeAutorizacao), Ct));

        Assert.DoesNotContain(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave);
    }

    // Marca

    [Fact]
    public async Task AMarcaParalisaSoOProprioPedido()
    {
        var marcada = await _fixture.Http.AutorizarAsync(Autorizacao(), true, [new NotaTeste { Observacao = "EMULADA:paralisacao:109" }.Gerar()], Ct);
        var seguinte = await _fixture.Http.AutorizarAsync(Autorizacao(), true, [new NotaTeste().Gerar()], Ct);

        Assert.Equal(("109", "104"), (marcada.Campo("cStat"), seguinte.Campo("cStat")));
    }

    [Fact]
    public async Task AMarcaPassaNaFrenteDoControleQueFicaParaOProximo()
    {
        await ComandarAsync(new { tipo = "paralisacao", destino = "MG", servico = Servico.NomeAutorizacao, vezes = 1 });

        var marcada = await _fixture.Http.AutorizarAsync(Autorizacao(), true, [new NotaTeste { Observacao = "EMULADA:paralisacao:109" }.Gerar()], Ct);
        var seguinte = await _fixture.Http.AutorizarAsync(Autorizacao(), true, [new NotaTeste().Gerar()], Ct);
        var depois = await _fixture.Http.AutorizarAsync(Autorizacao(), true, [new NotaTeste().Gerar()], Ct);

        Assert.Equal(["109", "108", "104"], new[] { marcada, seguinte, depois }.Select(r => r.Campo("cStat")));
    }

    [Fact]
    public async Task AMarcaDaFalhaSoap()
    {
        using var resposta = await _fixture.Http.EnviarAsync(Autorizacao(),
            Mensagens.Envelope(Mensagens.Lote(true, new NotaTeste { Observacao = "EMULADA:falhaSoap" }.Gerar()), Servico.NomeAutorizacao), Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("soap:Receiver", Mensagens.Falha(await resposta.Content.ReadAsStringAsync(Ct)));
    }

    [Theory]
    [InlineData("EMULADA:paralisacao:107")]
    [InlineData("EMULADA:atraso")]
    [InlineData("EMULADA:atraso:0")]
    [InlineData("EMULADA:tempoEsgotado:5")]
    [InlineData("EMULADA:conexaoDerrubada:antes")]
    [InlineData("EMULADA:qualquerCoisa")]
    [InlineData("EMULADA:falhaSoap EMULADA:respostaIlegivel")]
    public async Task RecusaAMarcaQueNaoServeComFalhaSoap(string marca)
    {
        var nota = new NotaTeste { Observacao = marca };

        using var resposta = await _fixture.Http.EnviarAsync(Autorizacao(),
            Mensagens.Envelope(Mensagens.Lote(true, nota.Gerar()), Servico.NomeAutorizacao), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("soap:Sender", Mensagens.Falha(await resposta.Content.ReadAsStringAsync(Ct)));
        Assert.DoesNotContain(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave);
    }

    // Controle das notas

    [Fact]
    public async Task ListaEEsqueceAsNotasPeloControle()
    {
        var nota = new NotaTeste();
        await _fixture.Http.AutorizarAsync(Autorizacao(), true, [nota.Gerar()], Ct);

        using var lista = await _fixture.Http.GetAsync(new Uri(_fixture.Emulador.Endereco, "_controle/notas"), Ct);
        using var json = JsonDocument.Parse(await lista.Content.ReadAsStringAsync(Ct));
        Assert.Contains(json.RootElement.EnumerateArray(), n => n.GetProperty("chave").GetString() == nota.Chave);

        using var limpeza = await _fixture.Http.DeleteAsync(new Uri(_fixture.Emulador.Endereco, "_controle/notas"), Ct);
        Assert.Equal(HttpStatusCode.NoContent, limpeza.StatusCode);
        Assert.Empty(_fixture.Emulador.Repositorio.Notas());
        Assert.Equal("100", Assert.Single((await _fixture.Http.AutorizarAsync(Autorizacao(), true, [nota.Gerar()], Ct)).Elements(Mensagens.NFe + "protNFe")).Protocolo().Campo("cStat"));
    }

    private async Task ComandarAsync(object pedido)
    {
        using var resposta = await _fixture.Http.PostAsJsonAsync(new Uri(_fixture.Emulador.Endereco, "_controle/falhas"), pedido, Ct);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }
}
