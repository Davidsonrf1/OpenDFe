using System.Globalization;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O NFeAutorizacao4 (MOC 7.0, Visão Geral, 5.1; Anexo I, 4.1 e 4.2.1) na resposta síncrona: o lote, as regras
// da nota e a duplicidade.
public sealed class AutorizacaoTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private static readonly XNamespace Assinatura = "http://www.w3.org/2000/09/xmldsig#";

    private readonly EmuladorFixture _fixture;

    public AutorizacaoTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Controle.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => _fixture.Emulador.Controle.Limpar();

    private Uri Url(string destino = "MG") => _fixture.Url(destino, Servico.NomeAutorizacao);

    // A resposta síncrona de uma nota: o retEnviNFe com o 104 e o protNFe.
    private async Task<XElement> AutorizarAsync(string nota, string destino = "MG")
    {
        var retorno = await _fixture.Http.AutorizarAsync(Url(destino), true, [nota], Ct);
        Assert.Equal("104", retorno.Campo("cStat"));
        return retorno.Element(Mensagens.NFe + "protNFe")!;
    }

    private async Task<string> RejeicaoAsync(NotaTeste nota, string destino = "MG") =>
        (await AutorizarAsync(nota.Gerar(), destino)).Protocolo().Campo("cStat");

    // Autorização

    [Fact]
    public async Task AutorizaANotaNaRespostaSincrona()
    {
        var nota = new NotaTeste();
        var xml = nota.Gerar();
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);

        var retorno = await _fixture.Http.AutorizarAsync(Url(), true, [xml], Ct);

        Assert.Equal(Mensagens.NFe + "retEnviNFe", retorno.Name);
        Assert.Equal("4.00", retorno.Attribute("versao")?.Value);
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "dhRecbto", "protNFe"], retorno.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("2", "MG-EMULADA", "104", "Lote processado", "31"),
            (retorno.Campo("tpAmb"), retorno.Campo("verAplic"), retorno.Campo("cStat"), retorno.Campo("xMotivo"), retorno.Campo("cUF")));
        var protocolo = retorno.Element(Mensagens.NFe + "protNFe")!;
        Assert.Equal("4.00", protocolo.Attribute("versao")?.Value);
        var info = protocolo.Protocolo();
        Assert.Null(info.Attribute("Id"));
        Assert.Equal(["tpAmb", "verAplic", "chNFe", "dhRecbto", "nProt", "digVal", "cStat", "xMotivo"], info.Elements().Select(e => e.Name.LocalName));
        Assert.Equal((nota.Chave, "100", "Autorizado o uso da NF-e"), (info.Campo("chNFe"), info.Campo("cStat"), info.Campo("xMotivo")));
        // Visão Geral, 4.3.5: o tipo de autorizador (1, SEFAZ estadual), o cUF, o ano e o sequencial.
        Assert.Matches("^13126[0-9]{10}$", info.Campo("nProt"));
        var resumo = XElement.Parse(xml).Descendants(Assinatura + "DigestValue").Single().Value;
        Assert.Equal(resumo, info.Campo("digVal"));
        var recebimento = DateTimeOffset.ParseExact(info.Campo("dhRecbto"), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
        Assert.InRange(recebimento, antes, DateTimeOffset.UtcNow.AddSeconds(1));
        Assert.Contains(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave && n.Protocolo == info.Campo("nProt") && n.Recibo is null);
    }

    [Theory]
    [InlineData("SVRS", "42", "342")]
    [InlineData("SVAN", "21", "421")]
    [InlineData("SP", "35", "135")]
    public async Task AutorizaNoAutorizadorDaUF(string destino, string uf, string inicioProtocolo)
    {
        var info = (await AutorizarAsync(new NotaTeste { Uf = uf }.Gerar(), destino)).Protocolo();

        Assert.Equal("100", info.Campo("cStat"));
        Assert.StartsWith(inicioProtocolo, info.Campo("nProt"), StringComparison.Ordinal);
        Assert.Equal(destino + "-EMULADA", info.Campo("verAplic"));
    }

    [Fact]
    public async Task AutorizaANfce()
    {
        var info = (await AutorizarAsync(new NotaTeste { Modelo = "65" }.Gerar())).Protocolo();

        Assert.Equal("100", info.Campo("cStat"));
    }

    [Fact]
    public async Task AutorizaOEmitenteCpf()
    {
        var certificado = X509CertificateLoaderPfx(new CertificadoTeste { Cpf = "52998224725", Nome = "PRODUTOR DE TESTE" });

        var info = (await AutorizarAsync(new NotaTeste { Cnpj = null, Cpf = "52998224725", Certificado = certificado }.Gerar())).Protocolo();

        Assert.Equal("100", info.Campo("cStat"));
    }

    [Fact]
    public async Task AutorizaOCnpjAlfanumerico()
    {
        var certificado = X509CertificateLoaderPfx(new CertificadoTeste { Cnpj = "12ABC34501DE35" });

        var info = (await AutorizarAsync(new NotaTeste { Cnpj = "12ABC34501DE35", Certificado = certificado }.Gerar())).Protocolo();

        Assert.Equal("100", info.Campo("cStat"));
    }

    [Fact]
    public async Task AutorizaANotaComEspacoEntreAsTagsAssinadoComoVeio()
    {
        var xml = new NotaTeste { Recuar = true }.Gerar();
        Assert.Contains("\n  <emit>", xml, StringComparison.Ordinal);

        var info = (await AutorizarAsync(xml)).Protocolo();

        Assert.Equal("100", info.Campo("cStat"));
    }

    [Fact]
    public async Task AutorizaComOCertificadoDeOutroEstabelecimentoDaEmpresa()
    {
        // F03: a raiz do CNPJ; o certificado da matriz (0001) assina pela filial.
        var info = (await AutorizarAsync(new NotaTeste { Cnpj = "11222333000262" }.Gerar())).Protocolo();

        Assert.Equal("100", info.Campo("cStat"));
    }

    // Duplicidade

    [Fact]
    public async Task RejeitaAMesmaNotaDeNovoComoDuplicidade()
    {
        var xml = new NotaTeste().Gerar();
        await AutorizarAsync(xml);

        var info = (await AutorizarAsync(xml)).Protocolo();

        Assert.Equal(("204", "Rejeição: Duplicidade de NF-e"), (info.Campo("cStat"), info.Campo("xMotivo")));
        Assert.Null(info.Element(Mensagens.NFe + "nProt"));
        Assert.NotNull(info.Element(Mensagens.NFe + "digVal"));
    }

    [Fact]
    public async Task RejeitaAMesmaNumeracaoComOutraChave()
    {
        var primeira = new NotaTeste();
        await AutorizarAsync(primeira.Gerar());

        var info = (await AutorizarAsync((primeira with { Codigo = "56473829" }).Gerar())).Protocolo();

        Assert.Equal("539", info.Campo("cStat"));
        Assert.Equal($"Rejeição: Duplicidade de NF-e com diferença na Chave de Acesso [chNFe:{primeira.Chave}]", info.Campo("xMotivo"));
    }

    [Fact]
    public async Task AceitaAMesmaNumeracaoEmOutraSerieOuOutroModelo()
    {
        var primeira = new NotaTeste();
        await AutorizarAsync(primeira.Gerar());

        Assert.Equal("100", await RejeicaoAsync(primeira with { Serie = "2" }));
        Assert.Equal("100", await RejeicaoAsync(primeira with { Modelo = "65" }));
    }

    [Fact]
    public async Task NaNfceSeparaANumeracaoPeloTipoDeEmissao()
    {
        // Anexo I, 2B08-10: na NFC-e, a duplicidade compara também o tipo de emissão.
        var normal = new NotaTeste { Modelo = "65" };
        await AutorizarAsync(normal.Gerar());

        Assert.Equal("100", await RejeicaoAsync(normal with { TipoEmissao = "9" }));
    }

    [Fact]
    public async Task ADuplicidadeValeEntreAutorizadores()
    {
        // A mesma nota de SC na SVRS e depois na SVC-AN: a base é a do país.
        var nota = new NotaTeste { Uf = "42" };
        await AutorizarAsync(nota.Gerar(), "SVRS");

        // A chave muda com o tipo de emissão; a numeração da NF-e, não.
        Assert.Equal("539", await RejeicaoAsync(nota with { TipoEmissao = "6" }, "SVC-AN"));
    }

    [Fact]
    public async Task NaoRegistraANotaRejeitada()
    {
        var nota = new NotaTeste { Ambiente = "1" };
        Assert.Equal("252", await RejeicaoAsync(nota));

        Assert.Equal("100", await RejeicaoAsync(nota with { Ambiente = "2" }));
    }

    // Regras da nota

    [Fact]
    public async Task RejeitaOCnpjDoEmitenteInvalido()
    {
        Assert.Equal("207", await RejeicaoAsync(new NotaTeste { Cnpj = "11222333000180" }));
    }

    [Fact]
    public async Task RejeitaOCpfDoEmitenteInvalido()
    {
        Assert.Equal("401", await RejeicaoAsync(new NotaTeste { Cnpj = null, Cpf = "52998224724" }));
    }

    [Fact]
    public async Task RejeitaANotaAlteradaDepoisDeAssinada()
    {
        var xml = new NotaTeste().Gerar().Replace("PRODUTO DE TESTE", "PRODUTO ALTERADO", StringComparison.Ordinal);

        var info = (await AutorizarAsync(xml)).Protocolo();

        Assert.Equal(("297", "Rejeição: Assinatura difere do calculado"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaANotaAssinadaPorOutraEmpresa()
    {
        var info = (await AutorizarAsync(new NotaTeste { Certificado = Clientes.OutroCertificado }.Gerar())).Protocolo();

        Assert.Equal(("213", "Rejeição: CNPJ-Base do Emitente difere do CNPJ-Base do Certificado Digital"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaOEmitenteCpfComOCertificadoDeOutroCpf()
    {
        var certificado = X509CertificateLoaderPfx(new CertificadoTeste { Cpf = "11144477735", Nome = "OUTRO PRODUTOR" });

        Assert.Equal("227", await RejeicaoAsync(new NotaTeste { Cnpj = null, Cpf = "52998224725", Certificado = certificado }));
    }

    [Fact]
    public async Task RejeitaOIdQueNaoCorrespondeAosCampos()
    {
        var nota = new NotaTeste();
        var outra = nota with { Numero = nota.Numero + 100000 };

        Assert.Equal("502", await RejeicaoAsync(nota with { Id = "NFe" + outra.Chave }));
    }

    [Fact]
    public async Task RejeitaODigitoDaChaveInvalido()
    {
        var nota = new NotaTeste();
        var digito = (nota.Chave[43] - '0' + 1) % 10;

        Assert.Equal("253", await RejeicaoAsync(nota with { Digito = digito }));
    }

    [Fact]
    public async Task RejeitaAmbienteDeProducaoNaNota()
    {
        var info = (await AutorizarAsync(new NotaTeste { Ambiente = "1" }.Gerar())).Protocolo();

        Assert.Equal(("252", "Rejeição: Ambiente informado diverge do Ambiente de recebimento"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Theory]
    [InlineData("MG", "35")]
    [InlineData("SVRS", "31")]
    [InlineData("SVAN", "29")]
    public async Task RejeitaAUFQueOAutorizadorNaoAtende(string destino, string uf)
    {
        Assert.Equal("226", await RejeicaoAsync(new NotaTeste { Uf = uf }, destino));
    }

    [Theory]
    [InlineData("3")]
    [InlineData("6")]
    [InlineData("7")]
    public async Task RejeitaOTipoDeEmissaoDaSvcNoAutorizadorNormal(string tipoEmissao)
    {
        Assert.Equal("570", await RejeicaoAsync(new NotaTeste { TipoEmissao = tipoEmissao }));
    }

    [Theory]
    [InlineData("SVC-AN", "31", "6")]
    [InlineData("SVC-RS", "41", "7")]
    public async Task AutorizaNaSvcComOTipoDeEmissaoDela(string destino, string uf, string tipoEmissao)
    {
        Assert.Equal("100", await RejeicaoAsync(new NotaTeste { Uf = uf, TipoEmissao = tipoEmissao }, destino));
    }

    [Theory]
    [InlineData("SVC-AN", "31", "1")]
    [InlineData("SVC-AN", "31", "7")]
    [InlineData("SVC-RS", "41", "6")]
    public async Task RejeitaNaSvcOutroTipoDeEmissao(string destino, string uf, string tipoEmissao)
    {
        Assert.Equal("713", await RejeicaoAsync(new NotaTeste { Uf = uf, TipoEmissao = tipoEmissao }, destino));
    }

    [Fact]
    public async Task RejeitaANfceNaSvc()
    {
        // Sem o QR Code: o da versão 3 online não aceita o tpEmis 6 na chave (o schema recusaria o lote, 225).
        Assert.Equal("783", await RejeicaoAsync(new NotaTeste { Modelo = "65", TipoEmissao = "6", OmitirQrCode = true }, "SVC-AN"));
    }

    [Fact]
    public async Task AceitaAEmissaoAteCincoMinutosNoFuturo()
    {
        Assert.Equal("100", await RejeicaoAsync(new NotaTeste { Emissao = NotaTeste.Agora().AddMinutes(4) }));
    }

    [Fact]
    public async Task RejeitaAEmissaoPosteriorAoRecebimento()
    {
        Assert.Equal("703", await RejeicaoAsync(new NotaTeste { Emissao = NotaTeste.Agora().AddMinutes(10) }));
    }

    [Fact]
    public async Task RejeitaANfeEmitidaHaMaisDeTrintaDias()
    {
        Assert.Equal("228", await RejeicaoAsync(new NotaTeste { Emissao = NotaTeste.Agora().AddDays(-31) }));
        Assert.Equal("150", await RejeicaoAsync(new NotaTeste { Emissao = NotaTeste.Agora().AddDays(-29) }));
    }

    [Fact]
    public async Task RejeitaANfceOnLineAtrasada()
    {
        Assert.Equal("704", await RejeicaoAsync(new NotaTeste { Modelo = "65", Emissao = NotaTeste.Agora().AddMinutes(-10) }));
    }

    [Fact]
    public async Task NaoRejeitaComoAtrasadaANfceEmContingenciaOffline()
    {
        Assert.Equal("100", await RejeicaoAsync(new NotaTeste { Modelo = "65", TipoEmissao = "9", Emissao = NotaTeste.Agora().AddHours(-2) }));
    }

    // Lote

    [Fact]
    public async Task RejeitaARespostaSincronaComMaisDeUmaNota()
    {
        var retorno = await _fixture.Http.AutorizarAsync(Url(), true, [new NotaTeste().Gerar(), new NotaTeste().Gerar()], Ct);

        Assert.Equal("764", retorno.Campo("cStat"));
        Assert.Equal("Rejeição: Solicitada resposta síncrona para Lote com mais de uma NF-e (indSinc=1)", retorno.Campo("xMotivo"));
        Assert.Equal(["tpAmb", "verAplic", "cStat", "xMotivo", "cUF", "dhRecbto"], retorno.Elements().Select(e => e.Name.LocalName));
    }

    [Fact]
    public async Task RejeitaOLoteComNfeENfce()
    {
        var retorno = await _fixture.Http.AutorizarAsync(Url(), false, [new NotaTeste().Gerar(), new NotaTeste { Modelo = "65" }.Gerar()], Ct);

        Assert.Equal("765", retorno.Campo("cStat"));
    }

    [Fact]
    public async Task RejeitaOLoteComANotaSemAssinaturaPeloSchema()
    {
        var xml = new NotaTeste().Gerar();
        var semAssinatura = xml[..xml.IndexOf("<Signature", StringComparison.Ordinal)] + "</NFe>";

        var retorno = await _fixture.Http.AutorizarAsync(Url(), true, [semAssinatura], Ct);

        Assert.Equal(("225", "Rejeição: Falha no Schema XML do lote de NFe"), (retorno.Campo("cStat"), retorno.Campo("xMotivo")));
    }

    [Theory]
    [InlineData("<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\"><idLote>1</idLote><indSinc>1</indSinc></enviNFe>", "568")]
    [InlineData("<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"3.10\"><idLote>1</idLote><indSinc>1</indSinc></enviNFe>", "239")]
    [InlineData("<consStatServ xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"/>", "565")]
    [InlineData("<n:enviNFe xmlns:n=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><n:idLote>1</n:idLote></n:enviNFe>", "404")]
    [InlineData("<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\"><idLote>1</idLote><indSinc>1</indSinc></enviNFe>", "225")]
    [InlineData("", "565")]
    public async Task RejeitaOLoteQueNaoServe(string dados, string codigo)
    {
        using var resposta = await _fixture.Http.EnviarAsync(Url(), Mensagens.Envelope(dados, Servico.NomeAutorizacao), Ct);
        var retorno = Mensagens.Retorno(await resposta.Content.ReadAsStringAsync(Ct));

        Assert.Equal(Mensagens.NFe + "retEnviNFe", retorno.Name);
        Assert.Equal(codigo, retorno.Campo("cStat"));
        Assert.Equal("31", retorno.Campo("cUF"));
    }

    [Fact]
    public async Task RejeitaANotaDeOutraUFNoMesmoLote()
    {
        var retorno = await _fixture.Http.AutorizarAsync(Url("SVRS"), false, [new NotaTeste { Uf = "42" }.Gerar(), new NotaTeste { Uf = "33" }.Gerar()], Ct);
        var protocolos = await ProtocolosAsync("SVRS", retorno.Element(Mensagens.NFe + "infRec")!.Campo("nRec"));

        Assert.Equal(["100", "476"], protocolos.Select(p => p.Protocolo().Campo("cStat")));
    }

    [Fact]
    public async Task ParalisaAAutorizacao()
    {
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Servico.NomeAutorizacao, Codigo: "109", Vezes: 1));
        var nota = new NotaTeste();

        var retorno = await _fixture.Http.AutorizarAsync(Url(), true, [nota.Gerar()], Ct);

        Assert.Equal("109", retorno.Campo("cStat"));
        Assert.Null(retorno.Element(Mensagens.NFe + "protNFe"));
        Assert.DoesNotContain(_fixture.Emulador.Repositorio.Notas(), n => n.Chave == nota.Chave);
    }

    private async Task<IReadOnlyList<XElement>> ProtocolosAsync(string destino, string recibo)
    {
        var retorno = await _fixture.Http.ConsultarReciboAsync(_fixture.Url(destino, Servico.NomeRetAutorizacao), recibo, Ct);
        Assert.Equal("104", retorno.Campo("cStat"));
        return [.. retorno.Elements(Mensagens.NFe + "protNFe")];
    }

    private static System.Security.Cryptography.X509Certificates.X509Certificate2 X509CertificateLoaderPfx(CertificadoTeste certificado) =>
        System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(certificado.GerarPfx("teste"), "teste");
}
