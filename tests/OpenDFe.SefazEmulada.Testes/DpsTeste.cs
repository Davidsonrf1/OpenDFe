using System.Globalization;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// Uma DPS mínima que passa no XSD 1.01 da produção (docs/NTS.md 12.1), escrita em texto e assinada com o SignedXml do
// .NET (NotaTeste.Assinar), sem a lib: a verificação do emulador é conferida contra uma assinatura independente. Cada
// DPS sai com um número novo, para a duplicidade não cruzar os testes que dividem o emulador.
internal sealed record DpsTeste
{
    public const string Namespace = "http://www.sped.fazenda.gov.br/nfse";

    private static long _ultimoNumero = DateTimeOffset.UtcNow.ToUnixTimeSeconds() * 100;

    public int Municipio { get; init; } = 3106200;

    public string? Cnpj { get; init; } = "11222333000181";

    public string? Cpf { get; init; }

    public string Serie { get; init; } = "1";

    public long Numero { get; init; } = Interlocked.Increment(ref _ultimoNumero);

    public string Ambiente { get; init; } = "2";

    public string Versao { get; init; } = "1.01";

    public string Emitente { get; init; } = "1";

    public DateTimeOffset Emissao { get; init; } = NotaTeste.Agora().AddMinutes(-1);

    // Nula: a data da emissão.
    public DateOnly? Competencia { get; init; }

    public string TributacaoNacional { get; init; } = "010101";

    public decimal Valor { get; init; } = 1000.00m;

    public decimal? DescontoIncondicionado { get; init; }

    public string Tributacao { get; init; } = "1";

    // 1 não retido, 2 retido pelo tomador, 3 pelo intermediário.
    public string Retencao { get; init; } = "1";

    public decimal? Aliquota { get; init; }

    public string RegimeEspecial { get; init; } = "0";

    public string SimplesNacional { get; init; } = "1";

    // O nBM e, no tipo 2, o percentual de redução.
    public string? Beneficio { get; init; }

    public decimal? ReducaoPercentual { get; init; }

    // O CPF ou o CNPJ do tomador; nulo, sem tomador.
    public string? Tomador { get; init; }

    public string? Substituida { get; init; }

    // O xInfComp, com a marca EMULADA: quando o teste quer.
    public string? Observacao { get; init; }

    // Nulo: o Id composto dos campos.
    public string? Id { get; init; }

    public X509Certificate2? Certificado { get; init; } = Clientes.CertificadoCliente;

    public string IdComposto => "DPS" + Municipio.ToString("D7", CultureInfo.InvariantCulture) + (Cnpj is not null ? "2" : "1")
        + (Cnpj ?? Cpf!).PadLeft(14, '0') + Serie.PadLeft(5, '0') + Numero.ToString("D15", CultureInfo.InvariantCulture);

    // O XML assinado (sem a assinatura, com o Certificado nulo).
    public string Gerar()
    {
        var id = Id ?? IdComposto;
        var data = (Competencia ?? DateOnly.FromDateTime(Emissao.DateTime)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        static string Documento(string documento) => documento.Length == 11 ? $"<CPF>{documento}</CPF>" : $"<CNPJ>{documento}</CNPJ>";
        var substituicao = Substituida is null ? "" : $"<subst><chSubstda>{Substituida}</chSubstda><cMotivo>99</cMotivo><xMotivo>Outro motivo de teste</xMotivo></subst>";
        var tomador = Tomador is null ? "" : $"<toma>{Documento(Tomador)}<xNome>TOMADOR DE TESTE</xNome></toma>";
        var informacoes = Observacao is null ? "" : $"<infoCompl><xInfComp>{Observacao}</xInfComp></infoCompl>";
        var desconto = DescontoIncondicionado is null ? "" : $"<vDescCondIncond><vDescIncond>{Decimal(DescontoIncondicionado.Value)}</vDescIncond></vDescCondIncond>";
        var beneficio = Beneficio is null ? "" : $"<BM><nBM>{Beneficio}</nBM>{(ReducaoPercentual is null ? "" : $"<pRedBCBM>{Decimal(ReducaoPercentual.Value)}</pRedBCBM>")}</BM>";
        var aliquota = Aliquota is null ? "" : $"<pAliq>{Decimal(Aliquota.Value)}</pAliq>";
        var xml = $"<DPS xmlns=\"{Namespace}\" versao=\"{Versao}\"><infDPS Id=\"{id}\">"
            + $"<tpAmb>{Ambiente}</tpAmb><dhEmi>{Emissao.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)}</dhEmi><verAplic>TESTE 1.0</verAplic>"
            + $"<serie>{Serie}</serie><nDPS>{Numero}</nDPS><dCompet>{data}</dCompet><tpEmit>{Emitente}</tpEmit><cLocEmi>{Municipio}</cLocEmi>"
            + substituicao
            + $"<prest>{Documento(Cnpj ?? Cpf!)}<regTrib><opSimpNac>{SimplesNacional}</opSimpNac><regEspTrib>{RegimeEspecial}</regEspTrib></regTrib></prest>"
            + tomador
            + $"<serv><locPrest><cLocPrestacao>{Municipio}</cLocPrestacao></locPrest><cServ><cTribNac>{TributacaoNacional}</cTribNac>"
            + $"<xDescServ>Desenvolvimento de software</xDescServ></cServ>{informacoes}</serv>"
            + $"<valores><vServPrest><vServ>{Decimal(Valor)}</vServ></vServPrest>{desconto}<trib><tribMun><tribISSQN>{Tributacao}</tribISSQN>{beneficio}"
            + $"<tpRetISSQN>{Retencao}</tpRetISSQN>{aliquota}</tribMun><totTrib><indTotTrib>0</indTotTrib></totTrib></trib></valores>"
            + "</infDPS></DPS>";
        return Certificado is null ? xml : NotaTeste.Assinar(xml, id, Certificado);
    }

    // O corpo do POST /nfse: o XML em GZip e Base64 no JSON.
    public static string Corpo(string xml) => Corpo(Encoding.UTF8.GetBytes(xml));

    public static string Corpo(byte[] xml) => JsonSerializer.Serialize(new Dictionary<string, string> { ["dpsXmlGZipB64"] = Compactar(xml) });

    public static string Compactar(byte[] dados)
    {
        using var saida = new MemoryStream();
        using (var gzip = new GZipStream(saida, CompressionLevel.Optimal))
            gzip.Write(dados);
        return Convert.ToBase64String(saida.ToArray());
    }

    public static string Descompactar(string base64)
    {
        using var entrada = new MemoryStream(Convert.FromBase64String(base64));
        using var gzip = new GZipStream(entrada, CompressionMode.Decompress);
        using var leitor = new StreamReader(gzip, Encoding.UTF8);
        return leitor.ReadToEnd();
    }

    private static string Decimal(decimal valor) => valor.ToString("0.00", CultureInfo.InvariantCulture);
}

// A Sefin emulada pelos testes: os pedidos em JSON e a resposta lida.
internal static class Sefin
{
    public static async Task<(HttpResponseMessage Resposta, JsonElement Json)> EmitirAsync(EmuladorFixture fixture, string corpo, string tipo = "application/json")
    {
        using var conteudo = new StringContent(corpo, Encoding.UTF8);
        conteudo.Headers.ContentType = new MediaTypeHeaderValue(tipo);
        var resposta = await fixture.Http.PostAsync(new Uri(fixture.Emulador.SefinNacional, "nfse"), conteudo, TestContext.Current.CancellationToken);
        return (resposta, await LerAsync(resposta));
    }

    public static Task<(HttpResponseMessage Resposta, JsonElement Json)> EmitirAsync(EmuladorFixture fixture, DpsTeste dps) =>
        EmitirAsync(fixture, DpsTeste.Corpo(dps.Gerar()));

    public static async Task<(HttpResponseMessage Resposta, JsonElement Json)> ObterAsync(EmuladorFixture fixture, string caminho)
    {
        var resposta = await fixture.Http.GetAsync(new Uri(fixture.Emulador.SefinNacional, caminho), TestContext.Current.CancellationToken);
        return (resposta, await LerAsync(resposta));
    }

    // O código do primeiro erro.
    public static string? Erro(JsonElement json) =>
        json.ValueKind == JsonValueKind.Object && json.TryGetProperty("erros", out var erros) ? erros[0].GetProperty("codigo").GetString() : null;

    // A NFS-e do JSON, descompactada.
    public static XDocument NFSe(JsonElement json) => XDocument.Parse(DpsTeste.Descompactar(json.GetProperty("nfseXmlGZipB64").GetString()!), LoadOptions.PreserveWhitespace);

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta)
    {
        var texto = await resposta.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return texto.Length == 0 ? default : JsonDocument.Parse(texto).RootElement.Clone();
    }
}
