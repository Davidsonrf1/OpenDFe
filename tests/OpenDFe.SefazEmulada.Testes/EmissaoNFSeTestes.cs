using System.Globalization;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OpenDFe.Testes;

// O POST /SefinNacional/nfse (E.7): a NFS-e gerada e assinada pela AC de teste, a recepção (E12xx do Anexo I da NFS-e) e
// as regras da DPS que o emulador confere.
public sealed class EmissaoNFSeTestes : IClassFixture<EmuladorFixture>
{
    private static readonly XNamespace N = DpsTeste.Namespace;
    private readonly EmuladorFixture _fixture;

    public EmissaoNFSeTestes(EmuladorFixture fixture)
    {
        _fixture = fixture;
        _fixture.Emulador.Municipios.Limpar();
    }

    private Task<(HttpResponseMessage Resposta, System.Text.Json.JsonElement Json)> EmitirAsync(DpsTeste dps) => Sefin.EmitirAsync(_fixture, dps);

    private async Task RejeitaAsync(DpsTeste dps, string codigo)
    {
        var (resposta, json) = await EmitirAsync(dps);
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(codigo, Sefin.Erro(json));
    }

    private async Task RecusaCorpoAsync(string corpo, string codigo)
    {
        var (resposta, json) = await Sefin.EmitirAsync(_fixture, corpo);
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(codigo, Sefin.Erro(json));
        Assert.False(json.TryGetProperty("idDps", out _));
    }

    // A NFS-e gerada

    [Fact]
    public async Task GeraANFSeValidaNoSchemaEAssinadaPelaAcDeTeste()
    {
        var dps = new DpsTeste();

        var (resposta, json) = await EmitirAsync(dps);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.Equal("application/json", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal(2, json.GetProperty("tipoAmbiente").GetInt32());
        Assert.Equal("OpenDFe Emulada 1.0", json.GetProperty("versaoAplicativo").GetString());
        Assert.Equal(dps.IdComposto, json.GetProperty("idDps").GetString());
        Assert.False(json.TryGetProperty("alertas", out _));
        var chave = json.GetProperty("chaveAcesso").GetString()!;
        var xml = DpsTeste.Descompactar(json.GetProperty("nfseXmlGZipB64").GetString()!);
        Assert.Empty(SchemasLeiaute.NFSe.Validar(xml));

        // A assinatura da NFS-e confere com o certificado da AC, e a da DPS, de dentro, continua valendo.
        var documento = new XmlDocument { PreserveWhitespace = true };
        documento.LoadXml(xml);
        var assinatura = new SignedXml(documento);
        assinatura.LoadXml((XmlElement)documento.DocumentElement!.LastChild!);
        Assert.True(assinatura.CheckSignature(_fixture.Emulador.Autoridade, verifySignatureOnly: true));
        Assert.Equal("#NFS" + chave, assinatura.SignedInfo!.References.Cast<Reference>().Single().Uri);
        Assert.Empty(AssinaturaDigital.Verificar(xml, dps.IdComposto, dps.Cnpj!));

        var inf = XDocument.Parse(xml).Root!.Element(N + "infNFSe")!;
        Assert.Equal("NFS" + chave, inf.Attribute("Id")!.Value);
        Assert.Equal("100", inf.Element(N + "cStat")!.Value);
        Assert.Equal("2", inf.Element(N + "ambGer")!.Value);
        Assert.Equal("Município 3106200", inf.Element(N + "xLocEmi")!.Value);
        Assert.Equal(dps.Cnpj, inf.Element(N + "emit")!.Element(N + "CNPJ")!.Value);
        Assert.Equal("MG", inf.Element(N + "emit")!.Element(N + "enderNac")!.Element(N + "UF")!.Value);
        Assert.Equal(dps.IdComposto, inf.Element(N + "DPS")!.Element(N + "infDPS")!.Attribute("Id")!.Value);
    }

    [Fact]
    public async Task ComporAChaveComOMunicipioAInscricaoONumeroEODigito()
    {
        var dps = new DpsTeste { Cnpj = null, Cpf = "52998224725", Certificado = Clientes.CertificadoCpf };

        var (_, json) = await EmitirAsync(dps);

        var chave = json.GetProperty("chaveAcesso").GetString()!;
        Assert.Equal(50, chave.Length);
        Assert.StartsWith("3106200" + "2" + "1" + "00052998224725", chave, StringComparison.Ordinal);
        var nfse = Sefin.NFSe(json).Root!.Element(N + "infNFSe")!;
        Assert.Equal(long.Parse(nfse.Element(N + "nNFSe")!.Value, CultureInfo.InvariantCulture), long.Parse(chave[23..36], CultureInfo.InvariantCulture));
        Assert.Equal(NotaTeste.Agora().ToString("yyMM", CultureInfo.InvariantCulture), chave[36..40]);
        Assert.Equal(NotaTeste.Modulo11(chave[..49]), chave[49] - '0');
        Assert.Equal("52998224725", nfse.Element(N + "emit")!.Element(N + "CPF")!.Value);
    }

    [Fact]
    public async Task NumeraEmSequenciaPorMunicipioEInscricao()
    {
        var (_, primeira) = await EmitirAsync(new DpsTeste { Cnpj = "11444777000161", Certificado = Clientes.CertificadoTomador });
        var (_, segunda) = await EmitirAsync(new DpsTeste { Cnpj = "11444777000161", Certificado = Clientes.CertificadoTomador });

        var numero = (System.Text.Json.JsonElement json) => long.Parse(Sefin.NFSe(json).Root!.Element(N + "infNFSe")!.Element(N + "nNFSe")!.Value, CultureInfo.InvariantCulture);
        Assert.Equal(numero(primeira) + 1, numero(segunda));
    }

    [Fact]
    public async Task CalculaOIssComAAliquotaPadrao()
    {
        var valores = await ValoresAsync(new DpsTeste { Valor = 1000.00m, DescontoIncondicionado = 100.00m });

        Assert.Equal("900.00", valores["vBC"]);
        Assert.Equal("2.00", valores["pAliqAplic"]);
        Assert.Equal("18.00", valores["vISSQN"]);
        Assert.False(valores.ContainsKey("vTotalRet"));
        Assert.Equal("900.00", valores["vLiq"]);
    }

    [Fact]
    public async Task CalculaOIssComAAliquotaDoCodigoERetido()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Aliquotas: new Dictionary<string, decimal> { ["010101"] = 3.50m }, Retencoes: ["52998224725"]));

        var valores = await ValoresAsync(new DpsTeste { Valor = 200.00m, Retencao = "2", Tomador = "52998224725" });

        Assert.Equal("3.50", valores["pAliqAplic"]);
        Assert.Equal("7.00", valores["vISSQN"]);
        Assert.Equal("7.00", valores["vTotalRet"]);
        Assert.Equal("193.00", valores["vLiq"]);
    }

    [Fact]
    public async Task UsaAAliquotaInformadaNaDps()
    {
        var valores = await ValoresAsync(new DpsTeste { Valor = 100.00m, Aliquota = 4.00m });

        Assert.Equal("4.00", valores["pAliqAplic"]);
        Assert.Equal("4.00", valores["vISSQN"]);
    }

    [Fact]
    public async Task AplicaOsBeneficiosDoMunicipio()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Beneficios:
        [
            new BeneficioMunicipio("00000000000001", 1),
            new BeneficioMunicipio("00000000000002", 2),
            new BeneficioMunicipio("00000000000004", 4, 3.00m),
        ]));

        var isencao = await ValoresAsync(new DpsTeste { Beneficio = "00000000000001" });
        var reducao = await ValoresAsync(new DpsTeste { Beneficio = "00000000000002", ReducaoPercentual = 10.00m });
        var diferenciada = await ValoresAsync(new DpsTeste { Beneficio = "00000000000004" });

        Assert.Equal(("1", "0.00"), (isencao["tpBM"], isencao["vISSQN"]));
        Assert.Equal(("2", "100.00", "900.00", "18.00"), (reducao["tpBM"], reducao["vCalcBM"], reducao["vBC"], reducao["vISSQN"]));
        Assert.Equal(("4", "3.00", "30.00"), (diferenciada["tpBM"], diferenciada["pAliqAplic"], diferenciada["vISSQN"]));
    }

    [Fact]
    public async Task NaoCalculaOIssForaDoTributavel()
    {
        var valores = await ValoresAsync(new DpsTeste { Tributacao = "4" });

        Assert.Equal(["vLiq"], valores.Keys);
        Assert.Equal("1000.00", valores["vLiq"]);
    }

    [Fact]
    public async Task UsaONomeDoMunicipioConfigurado()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Nome: "Belo Horizonte"));

        var (_, json) = await EmitirAsync(new DpsTeste());

        var inf = Sefin.NFSe(json).Root!.Element(N + "infNFSe")!;
        Assert.Equal("Belo Horizonte", inf.Element(N + "xLocEmi")!.Value);
        Assert.Equal("Belo Horizonte", inf.Element(N + "xLocPrestacao")!.Value);
    }

    private async Task<Dictionary<string, string>> ValoresAsync(DpsTeste dps)
    {
        var (resposta, json) = await EmitirAsync(dps);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var nfse = Sefin.NFSe(json);
        Assert.Empty(SchemasLeiaute.NFSe.Validar(nfse.ToString(SaveOptions.DisableFormatting)));
        return nfse.Root!.Element(N + "infNFSe")!.Element(N + "valores")!.Elements().ToDictionary(e => e.Name.LocalName, e => e.Value);
    }

    // A recepção

    [Fact]
    public async Task RecusaOCorpoSemOCampoDaDps() => await RecusaCorpoAsync("{\"dps\": \"x\"}", "E1225");

    [Fact]
    public async Task RecusaOCorpoQueNaoEJson() => await RecusaCorpoAsync("<DPS/>", "E1225");

    [Fact]
    public async Task RecusaOBase64Invalido() => await RecusaCorpoAsync("{\"dpsXmlGZipB64\": \"%%%\"}", "E1225");

    [Fact]
    public async Task RecusaOQueNaoEGZip() =>
        await RecusaCorpoAsync($"{{\"dpsXmlGZipB64\": \"{Convert.ToBase64String(Encoding.UTF8.GetBytes(new DpsTeste().Gerar()))}\"}}", "E1226");

    [Fact]
    public async Task RecusaOXmlMalFormado() => await RecusaCorpoAsync(DpsTeste.Corpo("<DPS"), "E1226");

    [Fact]
    public async Task RecusaODtd() =>
        await RecusaCorpoAsync(DpsTeste.Corpo("<!DOCTYPE DPS [<!ENTITY x \"y\">]>" + new DpsTeste().Gerar()), "E1226");

    [Fact]
    public async Task RecusaOPrefixoDeNamespace() =>
        await RecusaCorpoAsync(DpsTeste.Corpo(new DpsTeste { Certificado = null }.Gerar().Replace("<DPS xmlns=", "<n:DPS xmlns:n=", StringComparison.Ordinal)
            .Replace("</DPS>", "</n:DPS>", StringComparison.Ordinal)), "E1228");

    [Fact]
    public async Task RecusaOQueNaoEUtf8()
    {
        var latin1 = Encoding.Latin1.GetBytes(new DpsTeste().Gerar().Replace("Desenvolvimento", "Manutenção", StringComparison.Ordinal));
        await RecusaCorpoAsync(DpsTeste.Corpo(latin1), "E1229");
    }

    [Fact]
    public async Task RecusaADeclaracaoDeOutraCodificacao() =>
        await RecusaCorpoAsync(DpsTeste.Corpo("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>" + new DpsTeste().Gerar()), "E1229");

    [Fact]
    public async Task AceitaADeclaracaoUtf8()
    {
        var (resposta, _) = await Sefin.EmitirAsync(_fixture, DpsTeste.Corpo("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + new DpsTeste().Gerar()));
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task RecusaOutroDocumento() =>
        await RecusaCorpoAsync(DpsTeste.Corpo($"<NFSe xmlns=\"{DpsTeste.Namespace}\" versao=\"1.01\"/>"), "E1242");

    [Fact]
    public async Task RecusaAFalhaDeSchemaComOCampo()
    {
        var (resposta, json) = await Sefin.EmitirAsync(_fixture, DpsTeste.Corpo(new DpsTeste().Gerar().Replace("<tpAmb>2</tpAmb>", "<tpAmb>7</tpAmb>", StringComparison.Ordinal)));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var erro = json.GetProperty("erros")[0];
        Assert.Equal("E1235", erro.GetProperty("codigo").GetString());
        Assert.Equal("Falha no esquema XML do DF-e.", erro.GetProperty("descricao").GetString());
        Assert.Contains("tpAmb", erro.GetProperty("complemento").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RecusaOutroTipoDeConteudo()
    {
        var (resposta, _) = await Sefin.EmitirAsync(_fixture, DpsTeste.Corpo(new DpsTeste().Gerar()), "text/plain");
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, resposta.StatusCode);
    }

    // As regras da DPS

    [Fact]
    public async Task RejeitaADpsSemAssinatura() => await RejeitaAsync(new DpsTeste { Certificado = null }, "E0717");

    [Fact]
    public async Task RejeitaAAssinaturaDeOutroTitular() => await RejeitaAsync(new DpsTeste { Certificado = Clientes.OutroCertificado }, "E0718");

    [Fact]
    public async Task RejeitaAAssinaturaQueNaoConfere()
    {
        var xml = new DpsTeste().Gerar().Replace("Desenvolvimento de software", "Desenvolvimento de sistemas", StringComparison.Ordinal);
        var (resposta, json) = await Sefin.EmitirAsync(_fixture, DpsTeste.Corpo(xml));
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("E0714", Sefin.Erro(json));
    }

    [Fact]
    public async Task RejeitaOEmitenteQueNaoEOPrestador() => await RejeitaAsync(new DpsTeste { Emitente = "2" }, "E9996");

    [Fact]
    public async Task RejeitaOCnpjDoPrestadorComODigitoErrado() => await RejeitaAsync(new DpsTeste { Cnpj = "11222333000182" }, "E0080");

    [Fact]
    public async Task RejeitaOCpfDoPrestadorComODigitoErrado() => await RejeitaAsync(new DpsTeste { Cnpj = null, Cpf = "52998224726" }, "E0096");

    [Fact]
    public async Task RejeitaOIdQueNaoEOsCampos()
    {
        var dps = new DpsTeste();
        await RejeitaAsync(dps with { Id = dps.IdComposto[..^1] + (dps.IdComposto[^1] == '9' ? '1' : (char)(dps.IdComposto[^1] + 1)) }, "E0004");
    }

    [Fact]
    public async Task RejeitaAProducao() => await RejeitaAsync(new DpsTeste { Ambiente = "1" }, "E0006");

    [Fact]
    public async Task RejeitaAVersao100() => await RejeitaAsync(new DpsTeste { Versao = "1.00" }, "E0001");

    [Fact]
    public async Task RejeitaAEmissaoNoFuturo() => await RejeitaAsync(new DpsTeste { Emissao = NotaTeste.Agora().AddMinutes(10) }, "E0008");

    [Fact]
    public async Task AceitaAEmissaoDentroDaTolerancia()
    {
        var (resposta, _) = await EmitirAsync(new DpsTeste { Emissao = NotaTeste.Agora().AddMinutes(3) });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task RejeitaACompetenciaDepoisDaEmissao()
    {
        var emissao = NotaTeste.Agora().AddMinutes(-1);
        await RejeitaAsync(new DpsTeste { Emissao = emissao, Competencia = DateOnly.FromDateTime(emissao.DateTime).AddDays(1) }, "E0015");
    }

    [Fact]
    public async Task RejeitaADuplicidadeComAChaveGerada()
    {
        var dps = new DpsTeste();
        var (_, primeira) = await EmitirAsync(dps);

        var (resposta, json) = await EmitirAsync(dps);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("E0014", Sefin.Erro(json));
        Assert.Equal(dps.IdComposto, json.GetProperty("idDps").GetString());
        Assert.Contains(primeira.GetProperty("chaveAcesso").GetString()!, json.GetProperty("erros")[0].GetProperty("complemento").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RejeitaAAliquotaAcimaDe5() => await RejeitaAsync(new DpsTeste { Aliquota = 6.00m }, "E0595");

    // O município

    [Fact]
    public async Task RejeitaOMunicipioDeUfQueNaoExiste() => await RejeitaAsync(new DpsTeste { Municipio = 9906200 }, "E0037");

    [Fact]
    public async Task RejeitaOMunicipioSemConvenio()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Convenio: SituacaoConvenio.Inexistente));
        await RejeitaAsync(new DpsTeste(), "E0037");
    }

    [Fact]
    public async Task RejeitaOConvenioInativo()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Convenio: SituacaoConvenio.Inativo));
        await RejeitaAsync(new DpsTeste(), "E0038");
    }

    [Fact]
    public async Task RejeitaOMunicipioComEmissorProprio()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Emissor: EmissorMunicipio.Proprio));
        await RejeitaAsync(new DpsTeste(), "E0039");
    }

    [Fact]
    public async Task RespondeIndisponivelNoMunicipioParalisado()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, Paralisado: true));

        var (resposta, json) = await EmitirAsync(new DpsTeste());

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        Assert.Equal("EMULADA-PARALISACAO", Sefin.Erro(json));
        var (outro, _) = await EmitirAsync(new DpsTeste { Municipio = 3550308 });
        Assert.Equal(HttpStatusCode.Created, outro.StatusCode);
    }

    [Fact]
    public async Task RejeitaORegimeEspecialNaoAdmitido()
    {
        _fixture.Emulador.Municipios.Cadastrar(new Municipio(3106200, RegimesEspeciais: [1]));

        await RejeitaAsync(new DpsTeste { RegimeEspecial = "3" }, "E0177");
        var (resposta, _) = await EmitirAsync(new DpsTeste { RegimeEspecial = "1" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task RejeitaOBeneficioQueOMunicipioNaoTem() => await RejeitaAsync(new DpsTeste { Beneficio = "12345678901234" }, "E0541");

    [Fact]
    public async Task RejeitaARetencaoPeloCpfNaoAutorizado() =>
        await RejeitaAsync(new DpsTeste { Retencao = "2", Tomador = "52998224725" }, "E0667");

    [Fact]
    public async Task AceitaARetencaoPeloCnpjDoTomador()
    {
        var (resposta, _) = await EmitirAsync(new DpsTeste { Retencao = "2", Tomador = "11444777000161" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }
}
