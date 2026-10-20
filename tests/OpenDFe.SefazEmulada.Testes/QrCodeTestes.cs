using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// A NFC-e e a NF-e com DANFE Simplificado Tipo 2 no NFeAutorizacao4: o QR Code nas versões 2 e 3, online e
// offline (Manual do DANFE NFC-e e QR Code 6.0; MOC 7.0, Anexo I, grupo ZX; NT 2025.001 e NT 2026.002), o
// CSC pela rota /_controle/csc e a autorização fora de prazo (150).
public sealed class QrCodeTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public QrCodeTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Codigos.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => _fixture.Emulador.Codigos.Limpar();

    private Uri Url(string destino = "MG") => _fixture.Url(destino, Servico.NomeAutorizacao);

    private Uri Csc(string caminho = "") => new(_fixture.Emulador.Endereco, "_controle/csc" + caminho);

    private async Task<XElement> AutorizarAsync(NotaTeste nota)
    {
        var retorno = await _fixture.Http.AutorizarAsync(Url(), true, [nota.Gerar()], Ct);
        Assert.Equal("104", retorno.Campo("cStat"));
        return retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
    }

    private async Task<string> CodigoAsync(NotaTeste nota) => (await AutorizarAsync(nota)).Campo("cStat");

    private static NotaTeste Nfce(string versao = "3", string tipoEmissao = "1") =>
        new() { Modelo = "65", VersaoQrCode = versao, TipoEmissao = tipoEmissao };

    // O cálculo, contra os exemplos do Manual (4.3.6.1 e 4.3.6.2). O CSC dos exemplos é o texto que entra no
    // cálculo deles ("SEU-CODIGO-..."), e não o da lista de parâmetros.

    [Fact]
    public void CalculaOHashComoOsExemplosDoManual()
    {
        const string Csc = "SEU-CODIGO-CSC-CONTRIBUINTE-36-CARACTERES";

        Assert.Equal("DC6AE2C2B9A992BE59679AC365E29922DE6B7511", QrCode.Hash("28170800156225000131650110000151341562040824|2|1|1", Csc));
        Assert.Equal("797a4759685578312f5859597a6b7357422b6650523351633530633d", QrCode.Hexadecimal("yzGYhUx1/XYYzksWB+fPR3Qc50c="));
        Assert.Equal("4615A93BB0D7C4E780F8D30EE77EDD5BA55C7D66", QrCode.Hash(
            "28170800156225000131650110000151349562040824|2|1|02|60.90|797a4759685578312f5859597a6b7357422b6650523351633530633d|1", Csc));
    }

    [Fact]
    public void OCscPadraoEOQueODocumentoDiz()
    {
        Assert.Equal((NotaTeste.CscPadrao, "1"), (CodigosSeguranca.ValorPadrao, CodigosSeguranca.IdPadrao));
    }

    // Autorização

    [Theory]
    [InlineData("3", "1")]
    [InlineData("3", "9")]
    [InlineData("2", "1")]
    [InlineData("2", "9")]
    public async Task AutorizaANfceComOQrCode(string versao, string tipoEmissao)
    {
        Assert.Equal("100", await CodigoAsync(Nfce(versao, tipoEmissao)));
    }

    [Theory]
    [InlineData("52998224725")]
    [InlineData("12345678000195")]
    [InlineData("EX")]
    public async Task AutorizaAOfflineDaVersao3ComODestinatario(string destinatario)
    {
        Assert.Equal("100", await CodigoAsync(Nfce(tipoEmissao: "9") with { Destinatario = destinatario }));
    }

    [Fact]
    public async Task AutorizaAVersao3DoEmitenteCpf()
    {
        var certificado = Pfx(new CertificadoTeste { Cpf = "52998224725", Nome = "PRODUTOR DE TESTE" });

        Assert.Equal("100", await CodigoAsync(Nfce(tipoEmissao: "9") with { Cnpj = null, Cpf = "52998224725", Certificado = certificado }));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("9")]
    public async Task AutorizaANfeComDanfeSimplificadoTipo2(string tipoEmissao)
    {
        Assert.Equal("100", await CodigoAsync(new NotaTeste { TipoImpressao = "6", TipoEmissao = tipoEmissao }));
    }

    // Prazo

    [Fact]
    public async Task AutorizaForaDePrazoAOfflineTransmitidaDepoisDe24Horas()
    {
        var info = await AutorizarAsync(Nfce(tipoEmissao: "9") with { Emissao = NotaTeste.Agora().AddHours(-25) });

        Assert.Equal(("150", "Autorizado o uso da NF-e, autorização fora de prazo"), (info.Campo("cStat"), info.Campo("xMotivo")));
        Assert.NotNull(info.Element(Mensagens.NFe + "nProt"));
        Assert.Equal("100", await CodigoAsync(Nfce(tipoEmissao: "9") with { Emissao = NotaTeste.Agora().AddHours(-23) }));
    }

    [Fact]
    public async Task AutorizaForaDePrazoANfeDepoisDe7Dias()
    {
        Assert.Equal("100", await CodigoAsync(new NotaTeste { Emissao = NotaTeste.Agora().AddDays(-6) }));
        Assert.Equal("150", await CodigoAsync(new NotaTeste { Emissao = NotaTeste.Agora().AddDays(-8) }));
    }

    [Fact]
    public async Task AutorizaAContingenciaDepoisDe30Dias()
    {
        // B09-20, exceção 01: depois de 30 dias, só a de contingência (aqui a do DANFE Simplificado Tipo 2, que
        // segue a regra das 24 horas).
        Assert.Equal("150", await CodigoAsync(new NotaTeste { TipoImpressao = "6", TipoEmissao = "9", Emissao = NotaTeste.Agora().AddDays(-40) }));
    }

    [Fact]
    public async Task RejeitaANfeComDanfeSimplificadoTipo2Atrasada()
    {
        Assert.Equal("704", await CodigoAsync(new NotaTeste { TipoImpressao = "6", Emissao = NotaTeste.Agora().AddMinutes(-10) }));
    }

    [Fact]
    public async Task RejeitaANfeOfflineSemODanfeSimplificadoTipo2()
    {
        var info = await AutorizarAsync(new NotaTeste { TipoEmissao = "9" });

        Assert.Equal(("711", "Rejeição: NF-e com contingência off-line em operação não permitida"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    // Regras do QR Code

    [Fact]
    public async Task RejeitaANfceSemQrCode()
    {
        Assert.Equal("394", await CodigoAsync(new NotaTeste { Modelo = "65", OmitirQrCode = true }));
        Assert.Equal("394", await CodigoAsync(new NotaTeste { TipoImpressao = "6", OmitirQrCode = true }));
    }

    [Fact]
    public async Task RejeitaOQrCodeNaNfeComum()
    {
        Assert.Equal("393", await CodigoAsync(new NotaTeste { VersaoQrCode = "3" }));
    }

    [Fact]
    public async Task RejeitaAVersao100()
    {
        Assert.Equal("903", await CodigoAsync(Nfce("100")));
        Assert.Equal("672", await CodigoAsync(new NotaTeste { TipoImpressao = "6", VersaoQrCode = "100" }));
    }

    [Fact]
    public async Task RejeitaAVersao2NaNfeComDanfeSimplificadoTipo2()
    {
        var info = await AutorizarAsync(new NotaTeste { TipoImpressao = "6", VersaoQrCode = "2" });

        Assert.Equal(("672", "Rejeição: NF-e com DANFE Simplificado Tipo 2 com versão de qrCode não permitida"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaAVersao2DoEmitenteCpf()
    {
        var certificado = Pfx(new CertificadoTeste { Cpf = "52998224725", Nome = "PRODUTOR DE TESTE" });

        Assert.Equal("444", await CodigoAsync(Nfce("2") with { Cnpj = null, Cpf = "52998224725", Certificado = certificado }));
    }

    [Theory]
    [InlineData("3", "1", 0, "chNFe")]
    [InlineData("2", "9", 0, "chNFe")]
    [InlineData("3", "1", 2, "tpAmb")]
    [InlineData("3", "9", 3, "dhEmi")]
    [InlineData("2", "9", 3, "dhEmi")]
    [InlineData("3", "9", 4, "vNF")]
    [InlineData("2", "9", 5, "digVal")]
    public async Task RejeitaOParametroDivergente(string versao, string tipoEmissao, int posicao, string nome)
    {
        var nota = Nfce(versao, tipoEmissao);
        var outra = nota with { Numero = nota.Numero + 1000000 };
        var trocado = nome switch
        {
            "chNFe" => outra.Chave,
            "tpAmb" => "1",
            "dhEmi" => nota.Emissao.Day == 1 ? "02" : "01",
            "vNF" => "11.00",
            _ => new string('a', 56),
        };

        var info = await AutorizarAsync(nota with { Parametros = p => [.. p[..posicao], trocado, .. p[(posicao + 1)..]] });

        Assert.Equal(("397", $"Rejeição: Parâmetro do QR-Code divergente da Nota Fiscal [Param: {nome}]"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Theory]
    [InlineData(null, "2", "52998224725")]
    [InlineData("52998224725", "", "")]
    [InlineData("52998224725", "1", "52998224725")]
    [InlineData("52998224725", "2", "11144477735")]
    [InlineData("EX", "", "")]
    public async Task RejeitaODestinatarioDivergenteNaVersao3(string? destinatario, string tipo, string documento)
    {
        var nota = Nfce(tipoEmissao: "9") with { Destinatario = destinatario, Parametros = p => [.. p[..5], tipo, documento] };

        var info = await AutorizarAsync(nota);

        Assert.Equal(("397", "Rejeição: Parâmetro do QR-Code divergente da Nota Fiscal [Param: idDest]"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaAAssinaturaDaVersao3QueNaoConfere()
    {
        var info = await AutorizarAsync(Nfce(tipoEmissao: "9") with { FinalQrCode = Convert.ToBase64String(new byte[256]) });

        Assert.Equal(("583", "Rejeição: Valor da assinatura do qrCode difere do valor calculado"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Fact]
    public async Task RejeitaAAssinaturaDaVersao3DeOutroCertificado()
    {
        // Os parâmetros assinados por outro certificado, e a nota pelo do emitente.
        var nota = Nfce(tipoEmissao: "9");
        var outra = nota with { Certificado = Clientes.OutroCertificado };
        var assinatura = QrCodeTexto(outra.Gerar()).Split('|')[^1];

        Assert.Equal("583", await CodigoAsync(nota with { FinalQrCode = assinatura }));
    }

    // CSC

    [Fact]
    public async Task RejeitaOHashQueNaoConfere()
    {
        var info = await AutorizarAsync(Nfce("2") with { Csc = "OUTRO-CSC-DO-EMITENTE-0001" });

        Assert.Equal(("464", "Rejeição: Código de Hash no QR-Code difere do calculado"), (info.Campo("cStat"), info.Campo("xMotivo")));
        Assert.Equal("464", await CodigoAsync(Nfce("2", "9") with { FinalQrCode = new string('A', 40) }));
    }

    [Fact]
    public async Task RejeitaOCscNaoCadastrado()
    {
        var info = await AutorizarAsync(Nfce("2") with { IdCsc = "7" });

        Assert.Equal(("462", "Rejeição: Código Identificador do CSC no QR-Code não cadastrado na SEFAZ"), (info.Campo("cStat"), info.Campo("xMotivo")));
    }

    [Fact]
    public async Task AceitaOCscCadastradoPeloControle()
    {
        using var resposta = await _fixture.Http.PostAsJsonAsync(Csc(), new { uf = "MG", cnpj = "11222333", id = "2", csc = "CSC-CADASTRADO-PELO-TESTE" }, Ct);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal("/_controle/csc/MG/11222333/2", resposta.Headers.Location?.OriginalString);

        Assert.Equal("100", await CodigoAsync(Nfce("2") with { IdCsc = "2", Csc = "CSC-CADASTRADO-PELO-TESTE" }));
        Assert.Equal("100", await CodigoAsync(Nfce("2") with { Cnpj = "11222333000262", IdCsc = "2", Csc = "CSC-CADASTRADO-PELO-TESTE" }));
        // O CSC é da UF e da raiz do CNPJ: em outra UF, o identificador 2 não existe.
        Assert.Equal("462", await CodigoAsync(Nfce("2") with { Uf = "35", IdCsc = "2", Csc = "CSC-CADASTRADO-PELO-TESTE" }, "SP"));
    }

    [Fact]
    public async Task RejeitaOCscRevogadoAntesDaEmissao()
    {
        // Emitida 2 minutos antes da revogação: ainda vale. Emitida depois: 463.
        var antes = Nfce("2") with { Emissao = NotaTeste.Agora().AddMinutes(-2) };
        using var revogacao = await _fixture.Http.DeleteAsync(Csc("/MG/11222333/1"), Ct);
        Assert.Equal(HttpStatusCode.NoContent, revogacao.StatusCode);
        var depois = Nfce("2") with { Emissao = NotaTeste.Agora().AddSeconds(1) };

        Assert.Equal("100", await CodigoAsync(antes));
        var info = await AutorizarAsync(depois);
        Assert.Equal(("463", "Rejeição: Código Identificador do CSC no QR-Code foi revogado pela empresa"), (info.Campo("cStat"), info.Campo("xMotivo")));
        // A versão 3 não depende do CSC.
        Assert.Equal("100", await CodigoAsync(Nfce()));
    }

    [Fact]
    public async Task ListaEEsqueceOsCscsPeloControle()
    {
        using (await _fixture.Http.PostAsJsonAsync(Csc(), new { uf = "SP", cnpj = "12ABC345", id = "3", csc = "CSC-DA-EMPRESA-ALFANUMERICA" }, Ct))
        using (await _fixture.Http.DeleteAsync(Csc("/MG/11222333/1"), Ct))
        {
        }

        using var lista = await _fixture.Http.GetAsync(Csc(), Ct);
        using var json = JsonDocument.Parse(await lista.Content.ReadAsStringAsync(Ct));
        var cscs = json.RootElement.EnumerateArray().Select(c => (c.GetProperty("uf").GetString(), c.GetProperty("id").GetString(), c.TryGetProperty("revogacao", out _))).ToList();
        Assert.Equal([("MG", "1", true), ("SP", "3", false)], cscs);

        using var limpeza = await _fixture.Http.DeleteAsync(Csc(), Ct);
        Assert.Equal(HttpStatusCode.NoContent, limpeza.StatusCode);
        Assert.Empty(_fixture.Emulador.Codigos.Listar());
        Assert.Equal("100", await CodigoAsync(Nfce("2")));
    }

    [Theory]
    [InlineData("XX", "11222333", "1", "CSC-VALIDO-DE-TESTE-01")]
    [InlineData("MG", "11222333000181", "1", "CSC-VALIDO-DE-TESTE-01")]
    [InlineData("MG", "11222333", "01", "CSC-VALIDO-DE-TESTE-01")]
    [InlineData("MG", "11222333", "1234567", "CSC-VALIDO-DE-TESTE-01")]
    [InlineData("MG", "11222333", "1", "CURTO")]
    [InlineData("MG", "11222333", "1", "CSC COM ESPACO NO MEIO")]
    [InlineData("MG", "11222333", "1", "CSC|COM|SEPARADOR|DO|QR")]
    public async Task RecusaOCscQueNaoServe(string uf, string cnpj, string id, string csc)
    {
        using var resposta = await _fixture.Http.PostAsJsonAsync(Csc(), new { uf, cnpj, id, csc }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(Ct));
        Assert.False(string.IsNullOrEmpty(json.RootElement.GetProperty("erro").GetString()));
    }

    [Fact]
    public async Task NaoRevogaOCscQueNaoExiste()
    {
        using var resposta = await _fixture.Http.DeleteAsync(Csc("/MG/11222333/5"), Ct);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    private async Task<string> CodigoAsync(NotaTeste nota, string destino)
    {
        var retorno = await _fixture.Http.AutorizarAsync(Url(destino), true, [nota.Gerar()], Ct);
        return retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo().Campo("cStat");
    }

    private static string QrCodeTexto(string xml) => XElement.Parse(xml).Descendants(Mensagens.NFe + "qrCode").Single().Value;

    private static System.Security.Cryptography.X509Certificates.X509Certificate2 Pfx(CertificadoTeste certificado) =>
        System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(certificado.GerarPfx("teste"), "teste");
}
