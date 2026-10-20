using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O NFeConsultaProtocolo4 (MOC 7.0, Visão Geral, 5.4): a nota autorizada com o protocolo, as regras da chave
// (Tabela 5-16), a recuperação depois da queda no meio (PROJETO.md 8.6) e a contingência SVC com o autorizador
// normal paralisado (Anexo III, 2.1.3).
public sealed class ConsultaProtocoloTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public ConsultaProtocoloTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Controle.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => _fixture.Emulador.Controle.Limpar();

    private Uri Url(string destino = "MG") => _fixture.Url(destino, Servico.NomeConsultaProtocolo);

    private Task<XElement> ConsultarAsync(string chave, string destino = "MG", string ambiente = "2") =>
        _fixture.Http.ConsultarProtocoloAsync(Url(destino), chave, Ct, ambiente);

    // O protNFe da resposta síncrona.
    private async Task<XElement> AutorizarAsync(NotaTeste nota, string destino = "MG")
    {
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url(destino, Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!;
        Assert.Contains(protocolo.Protocolo().Campo("cStat"), (string[])["100", "150"]);
        return protocolo;
    }

    // A chave de 43 posições com o DV.
    private static string Completar(string parte) => parte + NotaTeste.Modulo11(parte);

    // Nota achada

    [Fact]
    public async Task ConsultaANotaAutorizadaComOProtocolo()
    {
        var nota = new NotaTeste();
        var protocolo = await AutorizarAsync(nota);
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);

        var retorno = await ConsultarAsync(nota.Chave);

        Assert.Equal(Mensagens.NFe + "retConsSitNFe", retorno.Name);
        Assert.Equal("4.00", retorno.Attribute("versao")?.Value);
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "dhRecbto", "chNFe", "protNFe"], retorno.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("2", "MG-EMULADA", "100", "Autorizado o uso da NF-e", "31", nota.Chave),
            (retorno.Campo("tpAmb"), retorno.Campo("verAplic"), retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("cUF"), retorno.Campo("chNFe")));
        Assert.Equal(protocolo.ToString(), retorno.Element(Mensagens.NFe + "protNFe")!.ToString());
        var recebimento = DateTimeOffset.ParseExact(retorno.Campo("dhRecbto"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        Assert.InRange(recebimento, antes, DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task ConsultaANotaAutorizadaForaDePrazo()
    {
        var nota = new NotaTeste { Emissao = NotaTeste.Agora().AddDays(-10) };
        await AutorizarAsync(nota);

        var retorno = await ConsultarAsync(nota.Chave);

        Assert.Equal(("150", "Autorizado o uso da NF-e, autorização fora de prazo"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
        Assert.Equal("150", retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("cStat"));
    }

    [Fact]
    public async Task ConsultaANotaDoLoteAssincrono()
    {
        var primeira = new NotaTeste();
        var segunda = new NotaTeste();
        var lote = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), false, [primeira.Gerar(), segunda.Gerar()], Ct);
        Assert.Equal("103", lote.Campo("cStat"));

        var retorno = await ConsultarAsync(segunda.Chave);

        Assert.Equal("100", retorno.Campo("cStat"));
        Assert.Equal(segunda.Chave, retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("chNFe"));
    }

    [Fact]
    public async Task ConsultaANfceEOEmitenteCpf()
    {
        var nfce = new NotaTeste { Modelo = "65" };
        await AutorizarAsync(nfce);
        // J02e: o CPF na chave só da série 910 a 969.
        var certificado = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(
            new CertificadoTeste { Cpf = "52998224725", Nome = "PRODUTOR DE TESTE" }.GerarPfx("teste"), "teste");
        var produtor = new NotaTeste { Cnpj = null, Cpf = "52998224725", Serie = "920", Certificado = certificado };
        await AutorizarAsync(produtor);

        Assert.Equal("100", (await ConsultarAsync(nfce.Chave)).Campo("cStat"));
        Assert.Equal("100", (await ConsultarAsync(produtor.Chave)).Campo("cStat"));
    }

    [Fact]
    public async Task RecuperaPelaChaveANotaAutorizadaQuandoAConexaoCaiu()
    {
        // A queda no meio (PROJETO.md 8.6): a SEFAZ autorizou e a resposta se perdeu; a consulta pela chave
        // devolve o protocolo para montar o nfeProc.
        var nota = new NotaTeste { Observacao = "EMULADA:conexaoDerrubada:depois" };
        await Assert.ThrowsAsync<HttpRequestException>(() => _fixture.Http.EnviarAsync(
            _fixture.Url("MG", Servico.NomeAutorizacao), Mensagens.Envelope(Mensagens.Lote(true, nota.Gerar()), Servico.NomeAutorizacao), Ct));

        var retorno = await ConsultarAsync(nota.Chave);

        Assert.Equal("100", retorno.Campo("cStat"));
        var registrada = Assert.Single(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave);
        Assert.Equal(registrada.Protocolo, retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("nProt"));
    }

    // Nota não achada ou divergente

    [Fact]
    public async Task NaoAchaANotaQueNaoFoiAutorizada()
    {
        var nota = new NotaTeste();

        var retorno = await ConsultarAsync(nota.Chave);

        Assert.Equal(("217", "Rejeição: NF-e não consta na base de dados da SEFAZ", nota.Chave), (retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("chNFe")));
        Assert.Null(retorno.Element(Mensagens.NFe + "protNFe"));
    }

    [Fact]
    public async Task NaoAchaANotaRejeitada()
    {
        var nota = new NotaTeste { Emissao = NotaTeste.Agora().AddMinutes(10) };
        Assert.Equal("703", (await AutorizarRejeicaoAsync(nota)));

        Assert.Equal("217", (await ConsultarAsync(nota.Chave)).Campo("cStat"));
    }

    [Fact]
    public async Task RejeitaOCodigoNumericoDivergenteComAChaveDaBase()
    {
        var nota = new NotaTeste();
        await AutorizarAsync(nota);
        var outra = nota with { Codigo = "56473829" };

        var retorno = await ConsultarAsync(outra.Chave);

        // J04: o transmissor (o certificado do TLS) é o emitente, e o motivo leva a chave da base.
        Assert.Equal("562", retorno.Campo("cStat"));
        Assert.Equal($"Rejeição: Código Numérico informado na Chave de Acesso difere do Código Numérico da NF-e [chNFe:{nota.Chave}]", retorno.Campo("xMotivo"));
        Assert.Equal(outra.Chave, retorno.Campo("chNFe"));
    }

    [Fact]
    public async Task NaoMostraAChaveDaBaseAOutroTransmissor()
    {
        var nota = new NotaTeste();
        await AutorizarAsync(nota);
        using var outro = Clientes.Criar(_fixture.Emulador.Autoridade, Clientes.OutroCertificado);

        var retorno = await outro.ConsultarProtocoloAsync(Url(), (nota with { Codigo = "56473829" }).Chave, Ct);

        Assert.Equal(("562", "Rejeição: Código Numérico informado na Chave de Acesso difere do Código Numérico da NF-e"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaOMesDivergente()
    {
        var nota = new NotaTeste();
        await AutorizarAsync(nota);

        var retorno = await ConsultarAsync((nota with { Emissao = nota.Emissao.AddMonths(-1) }).Chave);

        Assert.Equal(("561", "Rejeição: Mês de Emissão informado na Chave de Acesso difere do Mês de Emissão da NF-e"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaAChaveQueDifereDaBase()
    {
        // Na NF-e, a numeração não tem o tipo de emissão: a mesma nota com o tpEmis 6 na chave.
        var nota = new NotaTeste();
        await AutorizarAsync(nota);

        var retorno = await ConsultarAsync((nota with { TipoEmissao = "6" }).Chave);

        Assert.Equal(("613", "Rejeição: Chave de Acesso difere da existente em BD"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    // Regras da chave

    [Fact]
    public async Task RejeitaAConsultaDeProducao()
    {
        var retorno = await ConsultarAsync(new NotaTeste().Chave, ambiente: "1");

        Assert.Equal(("252", "Rejeição: Ambiente informado diverge do Ambiente de recebimento"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    [Theory]
    [InlineData("MG", "35")]
    [InlineData("SVRS", "31")]
    [InlineData("SVC-RS", "31")]
    [InlineData("MG", "99")]
    public async Task RejeitaAUFQueOAutorizadorNaoAtende(string destino, string uf)
    {
        var chave = Completar(uf + new NotaTeste().Chave[2..43]);

        Assert.Equal("226", (await ConsultarAsync(chave, destino)).Campo("cStat"));
    }

    [Fact]
    public async Task RejeitaODigitoInvalido()
    {
        var nota = new NotaTeste();
        var chave = nota.Chave[..43] + ((nota.Chave[43] - '0' + 1) % 10);

        var retorno = await ConsultarAsync(chave);

        Assert.Equal(("236", "Rejeição: Chave de Acesso com dígito verificador inválido"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    [Theory]
    [InlineData(2, "05", "615")]
    [InlineData(2, "99", "615")]
    [InlineData(4, "00", "616")]
    [InlineData(4, "13", "616")]
    [InlineData(6, "11222333000180", "617")]
    [InlineData(6, "00000000000000", "617")]
    [InlineData(20, "57", "618")]
    [InlineData(25, "000000000", "619")]
    public async Task RejeitaAChaveInvalida(int posicao, string trecho, string codigo)
    {
        var parte = new NotaTeste().Chave[..43];
        var chave = Completar(parte[..posicao] + trecho + parte[(posicao + trecho.Length)..]);

        Assert.Equal(codigo, (await ConsultarAsync(chave)).Campo("cStat"));
    }

    [Theory]
    [InlineData("00052998224724")]
    [InlineData("11222333000181")]
    public async Task RejeitaOCpfInvalidoNaSerieDoEmitenteCpf(string documento)
    {
        var parte = new NotaTeste { Serie = "920" }.Chave[..43];
        var chave = Completar(parte[..6] + documento + parte[20..]);

        Assert.Equal("617", (await ConsultarAsync(chave)).Campo("cStat"));
    }

    [Theory]
    [InlineData("<consSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><xServ>STATUS</xServ><chNFe>31261011222333000181550010000000011102938476</chNFe></consSitNFe>", "215")]
    [InlineData("<consSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><tpAmb>2</tpAmb><xServ>CONSULTAR</xServ><chNFe>3126101122233300018155001000000001110293847</chNFe></consSitNFe>", "215")]
    [InlineData("<consSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><tpAmb>2</tpAmb><xServ>CONSULTAR</xServ><chNFe>31261011222333000181550010000000011102938476</chNFe></consSitNFe>", "517")]
    [InlineData("<consSitNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"3.10\"><tpAmb>2</tpAmb><xServ>CONSULTAR</xServ><chNFe>31261011222333000181550010000000011102938476</chNFe></consSitNFe>", "239")]
    [InlineData("<n:consSitNFe xmlns:n=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><n:tpAmb>2</n:tpAmb></n:consSitNFe>", "404")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"/>", "516")]
    [InlineData("", "516")]
    public async Task RejeitaAConsultaQueNaoServeComAChaveZerada(string dados, string codigo)
    {
        using var resposta = await _fixture.Http.EnviarAsync(Url(), Mensagens.Envelope(dados, Servico.NomeConsultaProtocolo), Ct);
        var retorno = Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(Ct));

        Assert.Equal(Mensagens.NFe + "retConsSitNFe", retorno.Name);
        Assert.Equal((codigo, new string('0', 44), "31"), (retorno.Campo("cStat"), retorno.Campo("chNFe"), retorno.Campo("cUF")));
    }

    [Fact]
    public async Task ParalisaAConsulta()
    {
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Servico.NomeConsultaProtocolo, Codigo: "109", Vezes: 1));
        var nota = new NotaTeste();
        await AutorizarAsync(nota);

        var paralisada = await ConsultarAsync(nota.Chave);
        var seguinte = await ConsultarAsync(nota.Chave);

        Assert.Equal(("109", nota.Chave), (paralisada.Campo("cStat"), paralisada.Campo("chNFe")));
        Assert.Null(paralisada.Element(Mensagens.NFe + "protNFe"));
        Assert.Equal("100", seguinte.Campo("cStat"));
    }

    // Contingência SVC

    [Fact]
    public async Task ASvcAtendeComOAutorizadorNormalParalisado()
    {
        // Anexo III, 2.1.3: com MG fora, a SVC-AN responde 107 para MG e autoriza a nota com o tpEmis 6.
        await ComandarAsync(new { tipo = "paralisacao", destino = "MG", codigo = "109" });
        Assert.Equal("109", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("MG"), Ct)).Campo("cStat"));
        Assert.Equal("107", (await _fixture.Http.ConsultarStatusAsync(_fixture.Url("SVC-AN"), Ct, uf: "31")).Campo("cStat"));
        var paralisada = new NotaTeste();
        Assert.Equal("109", (await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [paralisada.Gerar()], Ct)).Campo("cStat"));

        var contingencia = paralisada with { TipoEmissao = "6" };
        var protocolo = await AutorizarAsync(contingencia, "SVC-AN");

        // O protocolo da Receita (tipo 2) e a consulta na própria SVC.
        Assert.StartsWith("231", protocolo.Protocolo().Campo("nProt"), StringComparison.Ordinal);
        var naSvc = await ConsultarAsync(contingencia.Chave, "SVC-AN");
        Assert.Equal(("100", "SVC-AN-EMULADA", "91"), (naSvc.Campo("cStat"), naSvc.Campo("verAplic"), naSvc.Campo("cUF")));

        // Passada a paralisação, o autorizador normal já tem a nota da SVC (Anexo III, 2.1.3.4, h).
        _fixture.Emulador.Controle.Limpar();
        var noNormal = await ConsultarAsync(contingencia.Chave);
        Assert.Equal("100", noNormal.Campo("cStat"));
        Assert.Equal(protocolo.ToString(), noNormal.Element(Mensagens.NFe + "protNFe")!.ToString());
    }

    [Fact]
    public async Task ASvcSoAchaAsNotasQueElaAutorizou()
    {
        var nota = new NotaTeste();
        await AutorizarAsync(nota);

        Assert.Equal("217", (await ConsultarAsync(nota.Chave, "SVC-AN")).Campo("cStat"));
        Assert.Equal("100", (await ConsultarAsync(nota.Chave)).Campo("cStat"));
    }

    [Fact]
    public async Task ASvcRsConsultaAsNotasDela()
    {
        var nota = new NotaTeste { Uf = "41", TipoEmissao = "7" };
        await AutorizarAsync(nota, "SVC-RS");

        Assert.Equal("100", (await ConsultarAsync(nota.Chave, "SVC-RS")).Campo("cStat"));
        Assert.Equal("100", (await ConsultarAsync(nota.Chave, "PR")).Campo("cStat"));
        // O PR é da SVC-RS, não da SVC-AN.
        Assert.Equal("226", (await ConsultarAsync(nota.Chave, "SVC-AN")).Campo("cStat"));
    }

    // Controle das notas

    [Fact]
    public async Task ListaAsNotasComOAutorizadorEASituacao()
    {
        var nota = new NotaTeste { Emissao = NotaTeste.Agora().AddDays(-8) };
        await AutorizarAsync(nota);

        using var lista = await _fixture.Http.GetAsync(new Uri(_fixture.Emulador.Endereco, "_controle/notas"), Ct);
        using var json = JsonDocument.Parse(await lista.Content.ReadAsStringAsync(Ct));

        var registrada = json.RootElement.EnumerateArray().Single(n => n.GetProperty("chave").GetString() == nota.Chave);
        Assert.Equal(("MG", "150"), (registrada.GetProperty("autorizador").GetString(), registrada.GetProperty("situacao").GetString()));
        Assert.False(registrada.TryGetProperty("protocoloXml", out _));
    }

    private async Task<string> AutorizarRejeicaoAsync(NotaTeste nota)
    {
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        return retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("cStat");
    }

    private async Task ComandarAsync(object pedido)
    {
        using var resposta = await _fixture.Http.PostAsJsonAsync(new Uri(_fixture.Emulador.Endereco, "_controle/falhas"), pedido, Ct);
        Assert.Equal(System.Net.HttpStatusCode.Created, resposta.StatusCode);
    }
}
