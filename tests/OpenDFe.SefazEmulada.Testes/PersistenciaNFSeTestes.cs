using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace OpenDFe.Testes;

// A persistência da NFS-e (E.9, --persistir sim): os municípios, as NFS-e, os eventos e a numeração guardados numa
// partida valem na seguinte, com os dados no controle.db e os XMLs em SefinNacional/. Esquecer as notas da NF-e não
// apaga os da NFS-e.
public sealed class PersistenciaNFSeTestes : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "OpenDFe.SefazEmulada.Testes", Guid.NewGuid().ToString("N"));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private string PastaXml => Path.Combine(_pasta, "persistencia", "xml", "SefinNacional");

    public void Dispose()
    {
        try
        {
            Directory.Delete(_pasta, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private Task<Emulador> IniciarAsync(bool persistir = true) =>
        Emulador.IniciarAsync(new OpcoesEmulador { Porta = 0, Pasta = _pasta, Persistir = persistir }, Ct);

    private static HttpClient Cliente(Emulador emulador) => Clientes.Criar(emulador.Autoridade, Clientes.CertificadoCliente);

    private static async Task<(HttpStatusCode Status, JsonElement Json)> PostarAsync(HttpClient http, Uri url, string corpo)
    {
        using var conteudo = new StringContent(corpo, Encoding.UTF8);
        conteudo.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using var resposta = await http.PostAsync(url, conteudo, Ct);
        var texto = await resposta.Content.ReadAsStringAsync(Ct);
        return (resposta.StatusCode, texto.Length == 0 ? default : JsonDocument.Parse(texto).RootElement.Clone());
    }

    private static async Task<JsonElement> EmitirAsync(HttpClient http, Emulador emulador, DpsTeste dps)
    {
        var (status, json) = await PostarAsync(http, new Uri(emulador.SefinNacional, "nfse"), DpsTeste.Corpo(dps.Gerar()));
        Assert.Equal(HttpStatusCode.Created, status);
        return json;
    }

    private static string Numero(JsonElement json, string campo) =>
        Sefin.NFSe(json).Descendants(XmlNs + campo).First().Value;

    private static readonly System.Xml.Linq.XNamespace XmlNs = DpsTeste.Namespace;

    [Fact]
    public async Task OsMunicipiosAsNFSeOsEventosEANumeracaoValemNaPartidaSeguinte()
    {
        var dps = new DpsTeste { Municipio = 3550308 };
        string chave;
        string xmlGerado;
        string numero;
        string dfse;
        await using (var emulador = await IniciarAsync())
        {
            using var http = Cliente(emulador);
            emulador.Municipios.Cadastrar(new Municipio(3550308, "São Paulo", AliquotaPadrao: 3.5m, Paralisado: false, PrazoCancelamento: 30,
                Aliquotas: new Dictionary<string, decimal> { ["010101"] = 4m }, Beneficios: [new BeneficioMunicipio("35503080000001", 4, 2.5m)]));
            emulador.Municipios.Cadastrar(new Municipio(3106200, Convenio: SituacaoConvenio.Inativo));
            emulador.Municipios.Remover(3106200);
            var emitida = await EmitirAsync(http, emulador, dps);
            chave = emitida.GetProperty("chaveAcesso").GetString()!;
            xmlGerado = DpsTeste.Descompactar(emitida.GetProperty("nfseXmlGZipB64").GetString()!);
            numero = Numero(emitida, "nNFSe");
            dfse = Numero(emitida, "nDFSe");
            var (status, _) = await PostarAsync(http, new Uri(emulador.SefinNacional, $"nfse/{chave}/eventos"),
                PedidoEventoTeste.Corpo(new PedidoEventoTeste { Chave = chave, Tipo = "101103" }.Gerar()));
            Assert.Equal(HttpStatusCode.Created, status);
        }

        Assert.True(File.Exists(Path.Combine(PastaXml, "nfse", chave + ".xml")));
        Assert.True(File.Exists(Path.Combine(PastaXml, "eventos", chave + "-101103-001.xml")));

        await using (var emulador = await IniciarAsync())
        {
            using var http = Cliente(emulador);

            // O município como foi cadastrado, e o removido de volta ao padrão.
            var municipio = Assert.Single(emulador.Municipios.Listar());
            Assert.Equal("São Paulo", municipio.Nome);
            Assert.Equal(3.5m, municipio.AliquotaPadrao);
            Assert.Equal(4m, municipio.Aliquotas!["010101"]);
            Assert.Equal(30, municipio.PrazoCancelamento);
            Assert.Equal(2.5m, Assert.Single(municipio.Beneficios!).Aliquota);
            Assert.False(emulador.Municipios.Procurar(3106200).Configurado);

            // A NFS-e consultável pela chave e pela DPS, com o XML byte a byte (a assinatura da AC continua valendo).
            using (var consulta = await http.GetAsync(new Uri(emulador.SefinNacional, "nfse/" + chave), Ct))
            {
                Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
                using var json = JsonDocument.Parse(await consulta.Content.ReadAsStringAsync(Ct));
                Assert.Equal(xmlGerado, DpsTeste.Descompactar(json.RootElement.GetProperty("nfseXmlGZipB64").GetString()!));
            }
            using (var head = new HttpRequestMessage(HttpMethod.Head, new Uri(emulador.SefinNacional, "dps/" + dps.IdComposto)))
            using (var resposta = await http.SendAsync(head, Ct))
                Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            // A mesma DPS de novo: a duplicidade vê a de antes.
            var (repetida, erro) = await PostarAsync(http, new Uri(emulador.SefinNacional, "nfse"), DpsTeste.Corpo(dps.Gerar()));
            Assert.Equal(HttpStatusCode.BadRequest, repetida);
            Assert.Equal("E0014", Sefin.Erro(erro));

            // A análise fiscal pendente: o cancelamento é recusado pela matriz, e o deferimento do município passa.
            var (cancelamento, recusa) = await PostarAsync(http, new Uri(emulador.SefinNacional, $"nfse/{chave}/eventos"),
                PedidoEventoTeste.Corpo(new PedidoEventoTeste { Chave = chave }.Gerar()));
            Assert.Equal(HttpStatusCode.BadRequest, cancelamento);
            Assert.NotNull(Sefin.Erro(recusa));
            Assert.Equal("emAnalise", EventosNFSe.Situacao(emulador.NFSe.Eventos(chave)));

            // A numeração segue: o nNFSe do mesmo prestador e município e o nDFSe do ambiente não se repetem.
            var outra = await EmitirAsync(http, emulador, new DpsTeste { Municipio = 3550308 });
            Assert.Equal(long.Parse(numero, System.Globalization.CultureInfo.InvariantCulture) + 1,
                long.Parse(Numero(outra, "nNFSe"), System.Globalization.CultureInfo.InvariantCulture));
            Assert.Equal(long.Parse(dfse, System.Globalization.CultureInfo.InvariantCulture) + 2,
                long.Parse(Numero(outra, "nDFSe"), System.Globalization.CultureInfo.InvariantCulture));
            Assert.Equal(2, emulador.NFSe.Listar().Count);
        }
    }

    [Fact]
    public async Task EsquecerAsNotasDaNFeNaoApagaANFSe()
    {
        string chave;
        await using (var emulador = await IniciarAsync())
        {
            using var http = Cliente(emulador);
            chave = (await EmitirAsync(http, emulador, new DpsTeste())).GetProperty("chaveAcesso").GetString()!;
            emulador.Repositorio.Limpar();
        }

        Assert.True(File.Exists(Path.Combine(PastaXml, "nfse", chave + ".xml")));
        await using (var emulador = await IniciarAsync())
            Assert.NotNull(emulador.NFSe.Chave(chave));
    }

    [Fact]
    public async Task EsquecerAsNFSeApagaOsArquivosEMantemANumeracao()
    {
        string numero;
        await using (var emulador = await IniciarAsync())
        {
            using var http = Cliente(emulador);
            var emitida = await EmitirAsync(http, emulador, new DpsTeste());
            numero = Numero(emitida, "nNFSe");
            using var esquecer = await http.DeleteAsync(new Uri(emulador.Endereco, "_controle/nfse"), Ct);
            Assert.Equal(HttpStatusCode.NoContent, esquecer.StatusCode);
        }

        Assert.False(Directory.Exists(PastaXml));
        await using (var emulador = await IniciarAsync())
        {
            Assert.Empty(emulador.NFSe.Listar());
            using var http = Cliente(emulador);
            var outra = await EmitirAsync(http, emulador, new DpsTeste());
            Assert.Equal(long.Parse(numero, System.Globalization.CultureInfo.InvariantCulture) + 1,
                long.Parse(Numero(outra, "nNFSe"), System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    [Fact]
    public async Task ZerarTudoEsqueceOsMunicipiosEAsNFSe()
    {
        await using (var emulador = await IniciarAsync())
        {
            using var http = Cliente(emulador);
            using var cadastro = await http.PostAsJsonAsync(new Uri(emulador.Endereco, "_controle/municipios"), new { codigo = 3550308, paralisado = true }, Ct);
            Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);
            await EmitirAsync(http, emulador, new DpsTeste());
            using var zerar = await http.DeleteAsync(new Uri(emulador.Endereco, "_controle"), Ct);
            Assert.Equal(HttpStatusCode.NoContent, zerar.StatusCode);
        }

        await using (var emulador = await IniciarAsync())
        {
            Assert.Empty(emulador.Municipios.Listar());
            Assert.Empty(emulador.NFSe.Listar());
        }
    }

    [Fact]
    public async Task SemAPersistenciaANFSeNaoVaiParaODisco()
    {
        await using (var emulador = await IniciarAsync(persistir: false))
        {
            using var http = Cliente(emulador);
            emulador.Municipios.Cadastrar(new Municipio(3550308));
            await EmitirAsync(http, emulador, new DpsTeste());
        }

        Assert.False(Directory.Exists(Path.Combine(_pasta, "persistencia")));
        await using (var emulador = await IniciarAsync())
        {
            Assert.Empty(emulador.Municipios.Listar());
            Assert.Empty(emulador.NFSe.Listar());
        }
    }
}
