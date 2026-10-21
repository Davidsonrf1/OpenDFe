using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// Os emitentes e destinatários irregulares (NT 2024.001: sem denegação): o cadastro pela rota de controle, a
// marca no infCpl e as rejeições da autorização (781, 302).
public sealed class IrregularesTestes : IClassFixture<EmuladorFixture>, IDisposable
{
    private readonly EmuladorFixture _fixture;

    public IrregularesTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Irregulares.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => _fixture.Emulador.Irregulares.Limpar();

    private Uri Controle(string caminho = "") => new(_fixture.Emulador.Endereco, "_controle/irregulares" + caminho);

    private async Task<XElement> AutorizarAsync(NotaTeste nota)
    {
        var retorno = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), true, [nota.Gerar()], Ct);
        return retorno.Element(Mensagens.NFe + "protNFe")!.Protocolo();
    }

    // A rota de controle

    [Fact]
    public async Task CadastraListaETiraPelaRota()
    {
        using (var criado = await _fixture.Http.PostAsJsonAsync(Controle(), new { documento = "98765432000198" }, Ct))
        {
            Assert.Equal(HttpStatusCode.Created, criado.StatusCode);
            Assert.Equal("/_controle/irregulares/98765432000198", criado.Headers.Location?.OriginalString);
        }
        using (var cpf = await _fixture.Http.PostAsJsonAsync(Controle(), new { documento = "52998224725" }, Ct))
            Assert.Equal(HttpStatusCode.Created, cpf.StatusCode);

        using (var lista = await _fixture.Http.GetAsync(Controle(), Ct))
            Assert.Equal(["52998224725", "98765432000198"], JsonSerializer.Deserialize<string[]>(await lista.Content.ReadAsStringAsync(Ct))!);

        using (var tirado = await _fixture.Http.DeleteAsync(Controle("/52998224725"), Ct))
            Assert.Equal(HttpStatusCode.NoContent, tirado.StatusCode);
        using (var inexistente = await _fixture.Http.DeleteAsync(Controle("/52998224725"), Ct))
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        using (var todos = await _fixture.Http.DeleteAsync(Controle(), Ct))
            Assert.Equal(HttpStatusCode.NoContent, todos.StatusCode);
        Assert.Empty(_fixture.Emulador.Irregulares.Listar());
    }

    [Theory]
    [InlineData("98765432000199")]
    [InlineData("52998224726")]
    [InlineData("123")]
    [InlineData(null)]
    public async Task RecusaODocumentoInvalido(string? documento)
    {
        using var resposta = await _fixture.Http.PostAsJsonAsync(Controle(), new { documento }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync(Ct));
        Assert.Contains("CNPJ", json.RootElement.GetProperty("erro").GetString(), StringComparison.Ordinal);
    }

    // A autorização

    [Fact]
    public async Task RecusaOEmitenteIrregularNaNfeENaNfce()
    {
        _fixture.Emulador.Irregulares.Cadastrar("11222333000181");

        var nfe = await AutorizarAsync(new NotaTeste());
        var nfce = await AutorizarAsync(new NotaTeste { Modelo = "65" });

        Assert.Equal(("781", "Rejeição: Emissor não habilitado para emissão da NF-e/NFC-e"), (nfe.Campo("cStat"), nfe.Campo("xMotivo")));
        Assert.Equal("781", nfce.Campo("cStat"));
        Assert.Null(nfe.Element(Mensagens.NFe + "nProt"));
    }

    [Fact]
    public async Task AFilialDoEmitenteIrregularNaoEAtingida()
    {
        _fixture.Emulador.Irregulares.Cadastrar("11222333000181");

        Assert.Equal("100", (await AutorizarAsync(new NotaTeste { Cnpj = "11222333000262" })).Campo("cStat"));
    }

    [Fact]
    public async Task RecusaODestinatarioIrregularSoNaNfe()
    {
        _fixture.Emulador.Irregulares.Cadastrar("52998224725");

        var nfe = await AutorizarAsync(new NotaTeste { Destinatario = "52998224725" });
        var nfce = await AutorizarAsync(new NotaTeste { Modelo = "65", Destinatario = "52998224725" });

        Assert.Equal(("302", "Rejeição: Irregularidade fiscal do destinatário"), (nfe.Campo("cStat"), nfe.Campo("xMotivo")));
        Assert.Equal("100", nfce.Campo("cStat"));
    }

    [Fact]
    public async Task AMarcaPoeANotaEmSituacaoIrregular()
    {
        var emitente = await AutorizarAsync(new NotaTeste { Observacao = "Pedido 7 EMULADA:irregular:emitente" });
        var destinatario = await AutorizarAsync(new NotaTeste { Destinatario = "98765432000198", Observacao = "EMULADA:irregular:destinatario" });
        var outra = await AutorizarAsync(new NotaTeste());

        Assert.Equal("781", emitente.Campo("cStat"));
        Assert.Equal("302", destinatario.Campo("cStat"));
        Assert.Equal("100", outra.Campo("cStat"));
        Assert.Empty(_fixture.Emulador.Irregulares.Listar());
    }

    [Fact]
    public async Task AMarcaValeSoParaANotaQueATraz()
    {
        var marcada = new NotaTeste { Observacao = "EMULADA:irregular:emitente" };
        var limpa = new NotaTeste();

        var lote = await _fixture.Http.AutorizarAsync(_fixture.Url("MG", Servico.NomeAutorizacao), false, [marcada.Gerar(), limpa.Gerar()], Ct);

        Assert.Equal("103", lote.Campo("cStat"));
        var consulta = await _fixture.Http.ConsultarReciboAsync(_fixture.Url("MG", Servico.NomeRetAutorizacao), lote.Element(Mensagens.NFe + "infRec")!.Campo("nRec"), Ct);
        Assert.Equal(["781", "100"], consulta.Elements(Mensagens.NFe + "protNFe").Select(p => p.Protocolo().Campo("cStat")));
    }

    [Theory]
    [InlineData("EMULADA:irregular")]
    [InlineData("EMULADA:irregular:transportadora")]
    public async Task AMarcaQueNaoServeEFalhaSoap(string marca)
    {
        var nota = new NotaTeste { Observacao = marca };

        using var resposta = await _fixture.Http.EnviarAsync(_fixture.Url("MG", Servico.NomeAutorizacao),
            Mensagens.Envelope(Mensagens.Lote(true, nota.Gerar()), Servico.NomeAutorizacao), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("soap:Sender", Mensagens.Falha(await resposta.Content.ReadAsStringAsync(Ct)));
    }
}
