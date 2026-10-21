using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O CadConsultaCadastro4 (MOC 7.0, Visão Geral, 5.6) sobre os contribuintes da rota /_controle/cadastros: o 111 e o
// 112, a UF do Web Service (265), o argumento (258 a 264), o solicitante irregular (257) e a situação do irregular.
public sealed class ConsultaCadastroTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private const string Cnpj = "98765432000198";

    private readonly EmuladorFixture _fixture;

    public ConsultaCadastroTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => Limpar();

    private void Limpar()
    {
        _fixture.Emulador.Controle.Limpar();
        _fixture.Emulador.Cadastros.Limpar();
        _fixture.Emulador.Irregulares.Limpar();
    }

    private Uri Controle(string caminho = "") => new(_fixture.Emulador.Endereco, "_controle/cadastros" + caminho);

    private static Contribuinte Contribuinte(string uf = "MG", string ie = "0623079040081", string? cnpj = Cnpj, string? cpf = null) =>
        new(uf, ie, cnpj, cpf, "CONTRIBUINTE DE TESTE LTDA");

    private Task<XElement> ConsultarAsync(string destino, string uf, string argumento) =>
        _fixture.Http.ConsultarCadastroAsync(_fixture.Url(destino, Servico.NomeConsultaCadastro), Mensagens.Cadastro(uf, argumento), Ct);

    // O infCons do retConsCad.
    private static XElement Consulta(XElement retorno) => retorno.Element(Mensagens.NFe + "infCons")!;

    private async Task<(string Codigo, string Motivo)> CodigoAsync(string destino, string uf, string argumento)
    {
        var consulta = Consulta(await ConsultarAsync(destino, uf, argumento));
        return (consulta.Campo("cStat"), consulta.Campo("xMotivo"));
    }

    // Achado

    [Fact]
    public async Task AchaOContribuintePeloCnpj()
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte() with
        {
            Fantasia = "TESTE",
            RegimeApuracao = "NORMAL - REGIME PERIÓDICO DE APURAÇÃO",
            Cnae = "4751201",
            InicioAtividade = new DateOnly(2010, 3, 1),
            UltimaSituacao = new DateOnly(2020, 5, 2),
            Endereco = new EnderecoContribuinte("RUA DE TESTE", "100", "SALA 2", "CENTRO", "3106200", "BELO HORIZONTE", "30110000"),
        });
        var antes = DateTimeOffset.UtcNow.AddSeconds(-1);

        var retorno = await ConsultarAsync("MG", "MG", $"<CNPJ>{Cnpj}</CNPJ>");

        Assert.Equal(Mensagens.NFe + "retConsCad", retorno.Name);
        Assert.Equal("2.00", retorno.Attribute("versao")?.Value);
        var consulta = Consulta(retorno);
        Assert.Equal(["verAplic", "cStat", "xMotivo", "UF", "CNPJ", "dhCons", "cUF", "infCad"], consulta.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("MG-EMULADA", "111", "Consulta cadastro com uma ocorrência", "MG", Cnpj, "31"),
            (consulta.Campo("verAplic"), consulta.Campo("cStat"), consulta.Campo("xMotivo"), consulta.Campo("UF"), consulta.Campo("CNPJ"), consulta.Campo("cUF")));
        Assert.InRange(DateTimeOffset.Parse(consulta.Campo("dhCons"), System.Globalization.CultureInfo.InvariantCulture), antes, DateTimeOffset.UtcNow.AddSeconds(1));
        var cadastro = consulta.Element(Mensagens.NFe + "infCad")!;
        Assert.Equal(["IE", "CNPJ", "UF", "cSit", "indCredNFe", "indCredCTe", "xNome", "xFant", "xRegApur", "CNAE", "dIniAtiv", "dUltSit", "ender"],
            cadastro.Elements().Select(e => e.Name.LocalName));
        Assert.Equal(("0623079040081", "1", "1", "4", "CONTRIBUINTE DE TESTE LTDA", "2010-03-01"),
            (cadastro.Campo("IE"), cadastro.Campo("cSit"), cadastro.Campo("indCredNFe"), cadastro.Campo("indCredCTe"), cadastro.Campo("xNome"), cadastro.Campo("dIniAtiv")));
        Assert.Equal(["xLgr", "nro", "xCpl", "xBairro", "cMun", "xMun", "CEP"], cadastro.Element(Mensagens.NFe + "ender")!.Elements().Select(e => e.Name.LocalName));
        Assert.Empty(Esquemas.Validar("retConsCad_v2.00.xsd", retorno));
    }

    [Fact]
    public async Task AchaPelaIeEPeloCpf()
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(ie: "0012345670012", cnpj: null, cpf: "52998224725"));

        var ie = Consulta(await ConsultarAsync("MG", "MG", "<IE>0012345670012</IE>"));
        var cpf = Consulta(await ConsultarAsync("MG", "MG", "<CPF>52998224725</CPF>"));

        Assert.Equal(("111", "0012345670012"), (ie.Campo("cStat"), ie.Campo("IE")));
        Assert.Equal(("111", "52998224725"), (cpf.Campo("cStat"), cpf.Element(Mensagens.NFe + "infCad")!.Campo("CPF")));
    }

    [Fact]
    public async Task AIeUnicaDevolveTodosOsEstabelecimentos()
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(ie: "1000000001", cnpj: "11222333000181") with { IeUnica = "1000000001" });
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(ie: "1000000002", cnpj: "98765432000198") with { IeUnica = "1000000001" });

        var retorno = await ConsultarAsync("MG", "MG", "<IE>1000000001</IE>");

        var consulta = Consulta(retorno);
        Assert.Equal(("112", "Consulta cadastro com mais de uma ocorrência"), (consulta.Campo("cStat"), consulta.Campo("xMotivo")));
        Assert.Equal(["1000000001", "1000000002"], consulta.Elements(Mensagens.NFe + "infCad").Select(c => c.Campo("IE")));
        Assert.Empty(Esquemas.Validar("retConsCad_v2.00.xsd", retorno));
    }

    [Fact]
    public async Task OCnpjComDuasInscricoesNaUfDevolveAsDuas()
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(ie: "1000000001"));
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(ie: "1000000002"));
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(uf: "SP", ie: "1000000003"));

        Assert.Equal(2, Consulta(await ConsultarAsync("MG", "MG", $"<CNPJ>{Cnpj}</CNPJ>")).Elements(Mensagens.NFe + "infCad").Count());
    }

    [Fact]
    public async Task OIrregularSaiNaoHabilitado()
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte());
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(ie: "1000000002", cnpj: "11222333000181") with { Habilitado = false, CredenciadoNFe = "0" });
        _fixture.Emulador.Irregulares.Cadastrar(Cnpj);

        var irregular = Consulta(await ConsultarAsync("MG", "MG", $"<CNPJ>{Cnpj}</CNPJ>")).Element(Mensagens.NFe + "infCad")!;
        var inabilitado = Consulta(await ConsultarAsync("MG", "MG", "<IE>1000000002</IE>")).Element(Mensagens.NFe + "infCad")!;

        Assert.Equal("0", irregular.Campo("cSit"));
        Assert.Equal(("0", "0"), (inabilitado.Campo("cSit"), inabilitado.Campo("indCredNFe")));
    }

    [Fact]
    public async Task ASvrsConsultaSoOCadastroDasUfsDela()
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(uf: "SC"));
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte(uf: "RJ"));

        var sc = Consulta(await ConsultarAsync("SVRS", "SC", $"<CNPJ>{Cnpj}</CNPJ>"));

        Assert.Equal(("111", "43"), (sc.Campo("cStat"), sc.Campo("cUF")));
        Assert.Equal("265", (await CodigoAsync("SVRS", "RJ", $"<CNPJ>{Cnpj}</CNPJ>")).Codigo);
    }

    // As regras

    [Theory]
    [InlineData("MG", "SP")]
    [InlineData("MG", "SU")]
    [InlineData("RS", "SC")]
    public async Task AUfQueNaoEADoWebServiceE265(string destino, string uf)
    {
        Assert.Equal(("265", "Rejeição: Sigla da UF da consulta difere da UF do Web Service"), await CodigoAsync(destino, uf, $"<CNPJ>{Cnpj}</CNPJ>"));
    }

    [Theory]
    [InlineData("<CNPJ>98765432000199</CNPJ>", "258", "Rejeição: CNPJ da consulta inválido")]
    [InlineData("<CNPJ>11222333000181</CNPJ>", "259", "Rejeição: CNPJ da consulta não cadastrado como contribuinte na UF")]
    [InlineData("<IE>00000</IE>", "260", "Rejeição: IE da consulta inválida")]
    [InlineData("<IE>ISENTO</IE>", "260", "Rejeição: IE da consulta inválida")]
    [InlineData("<IE>1234567</IE>", "261", "Rejeição: IE da consulta não cadastrada como contribuinte na UF")]
    [InlineData("<CPF>52998224726</CPF>", "263", "Rejeição: CPF da consulta inválido")]
    [InlineData("<CPF>123</CPF>", "263", "Rejeição: CPF da consulta inválido")]
    [InlineData("<CPF>52998224725</CPF>", "264", "Rejeição: CPF da consulta não cadastrado como contribuinte na UF")]
    public async Task ConfereOArgumento(string argumento, string codigo, string motivo)
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte());

        var retorno = await ConsultarAsync("MG", "MG", argumento);

        Assert.Equal((codigo, motivo), (Consulta(retorno).Campo("cStat"), Consulta(retorno).Campo("xMotivo")));
        Assert.Empty(Consulta(retorno).Elements(Mensagens.NFe + "infCad"));
        Assert.Empty(Esquemas.Validar("retConsCad_v2.00.xsd", retorno));
    }

    [Fact]
    public async Task OSolicitanteIrregularE257()
    {
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte());
        _fixture.Emulador.Irregulares.Cadastrar("11222333000181");

        Assert.Equal(("257", "Rejeição: Solicitante não habilitado para emissão da NF-e"), await CodigoAsync("MG", "MG", $"<CNPJ>{Cnpj}</CNPJ>"));
    }

    [Theory]
    [InlineData("<ConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\"><infCons><xServ>CONSULTAR</xServ><UF>MG</UF><IE>1234567</IE></infCons></ConsCad>", "215")]
    [InlineData("<consCad xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"2.00\"/>", "516")]
    [InlineData("<ConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\"><infCons/></ConsCad>", "517")]
    [InlineData("<ConsCad xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.01\"><infCons/></ConsCad>", "239")]
    public async Task ConfereARaizEOSchema(string pedido, string codigo)
    {
        var retorno = await _fixture.Http.ConsultarCadastroAsync(_fixture.Url("MG", Servico.NomeConsultaCadastro), pedido, Ct);

        Assert.Equal((codigo, "MG", "00000000000000"), (Consulta(retorno).Campo("cStat"), Consulta(retorno).Campo("UF"), Consulta(retorno).Campo("CNPJ")));
        Assert.Empty(Esquemas.Validar("retConsCad_v2.00.xsd", retorno));
    }

    [Fact]
    public async Task AParalisacaoVemDepoisDaUf()
    {
        _fixture.Emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", Servico.NomeConsultaCadastro));

        Assert.Equal("265", (await CodigoAsync("MG", "SP", $"<CNPJ>{Cnpj}</CNPJ>")).Codigo);
        Assert.Equal("108", (await CodigoAsync("MG", "MG", $"<CNPJ>{Cnpj}</CNPJ>")).Codigo);
    }

    // A rota de controle

    [Fact]
    public async Task CadastraTrocaListaETiraPelaRota()
    {
        using (var criado = await _fixture.Http.PostAsJsonAsync(Controle(), new { uf = "mg", ie = "0623079040081", cnpj = Cnpj, nome = "CONTRIBUINTE DE TESTE LTDA" }, Ct))
        {
            Assert.Equal(HttpStatusCode.Created, criado.StatusCode);
            Assert.Equal("/_controle/cadastros/MG/0623079040081", criado.Headers.Location?.OriginalString);
            using var json = JsonDocument.Parse(await criado.Content.ReadAsStringAsync(Ct));
            Assert.Equal(("MG", "1", "4", true), (json.RootElement.GetProperty("uf").GetString(), json.RootElement.GetProperty("credenciadoNFe").GetString(),
                json.RootElement.GetProperty("credenciadoCTe").GetString(), json.RootElement.GetProperty("habilitado").GetBoolean()));
        }
        using (var trocado = await _fixture.Http.PostAsJsonAsync(Controle(), new { uf = "MG", ie = "0623079040081", cnpj = Cnpj, nome = "OUTRO NOME", habilitado = false }, Ct))
            Assert.Equal(HttpStatusCode.OK, trocado.StatusCode);

        using (var lista = await _fixture.Http.GetAsync(Controle(), Ct))
        {
            using var json = JsonDocument.Parse(await lista.Content.ReadAsStringAsync(Ct));
            var unico = Assert.Single(json.RootElement.EnumerateArray());
            Assert.Equal(("OUTRO NOME", false), (unico.GetProperty("nome").GetString(), unico.GetProperty("habilitado").GetBoolean()));
        }
        Assert.Equal("0", Consulta(await ConsultarAsync("MG", "MG", $"<CNPJ>{Cnpj}</CNPJ>")).Element(Mensagens.NFe + "infCad")!.Campo("cSit"));

        using (var tirado = await _fixture.Http.DeleteAsync(Controle("/MG/0623079040081"), Ct))
            Assert.Equal(HttpStatusCode.NoContent, tirado.StatusCode);
        using (var inexistente = await _fixture.Http.DeleteAsync(Controle("/MG/0623079040081"), Ct))
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        _fixture.Emulador.Cadastros.Cadastrar(Contribuinte());
        using (var todos = await _fixture.Http.DeleteAsync(Controle(), Ct))
            Assert.Equal(HttpStatusCode.NoContent, todos.StatusCode);
        Assert.Empty(_fixture.Emulador.Cadastros.Listar());
    }

    public static TheoryData<string, string> Recusados => new()
    {
        { """{ "uf": "XX", "ie": "0623079040081", "cnpj": "98765432000198", "nome": "A" }""", "uf" },
        { """{ "uf": "SU", "ie": "0623079040081", "cnpj": "98765432000198", "nome": "A" }""", "uf" },
        { """{ "uf": "MG", "ie": "00", "cnpj": "98765432000198", "nome": "A" }""", "ie" },
        { """{ "uf": "MG", "ie": "ISENTO", "cnpj": "98765432000198", "nome": "A" }""", "ie" },
        { """{ "uf": "MG", "ie": "0623079040081", "nome": "A" }""", "cnpj" },
        { """{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198", "cpf": "52998224725", "nome": "A" }""", "cnpj" },
        { """{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000199", "nome": "A" }""", "cnpj" },
        { """{ "uf": "MG", "ie": "0623079040081", "cpf": "52998224726", "nome": "A" }""", "cpf" },
        { """{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198" }""", "nome" },
        { """{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198", "nome": " A" }""", "nome" },
        { """{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198", "nome": "A", "cnae": "47" }""", "cnae" },
        { """{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198", "nome": "A", "credenciadoNFe": "5" }""", "credenciadoNFe" },
        { """{ "uf": "MG", "ie": "0623079040081", "cnpj": "98765432000198", "nome": "A", "endereco": { "cep": "301" } }""", "cep" },
    };

    [Theory]
    [MemberData(nameof(Recusados))]
    public async Task RecusaOContribuinteInvalido(string corpo, string campo)
    {
        using var conteudo = new StringContent(corpo, System.Text.Encoding.UTF8, "application/json");
        using var resposta = await _fixture.Http.PostAsync(Controle(), conteudo, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(Ct));
        Assert.Contains(campo, json.RootElement.GetProperty("erro").GetString(), StringComparison.Ordinal);
        Assert.Empty(_fixture.Emulador.Cadastros.Listar());
    }
}
