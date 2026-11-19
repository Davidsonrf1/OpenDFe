using System.Text.Json;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// A parametrização municipal da 9.4 sem rede (docs/NFSE.md 11): os argumentos, o caminho de cada rota e a leitura tolerante
// do JSON (os tipos misturados, o que não se lê com o aviso e o JSON ao lado, os erros). A ponta a ponta está em
// ParametrizacaoEmuladaTestes.
public class OperacoesParametrizacaoTestes
{
    private const int Bh = 3106200;

    private static readonly DateOnly Competencia = new(2026, 11, 1);

    private static string Json(object valor) => JsonSerializer.Serialize(valor);

    // Os argumentos e o caminho

    [Fact]
    public void OsArgumentosForaDaFormaSaoRecusados()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConsultaConvenio(310620));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConsultaConvenio(31062000));
        Assert.Throws<ArgumentException>(() => new ConsultaAliquota(Bh, "01010"));
        Assert.Throws<ArgumentException>(() => new ConsultaAliquota(Bh, "01010A"));
        Assert.Throws<ArgumentException>(() => new ConsultaAliquota(Bh, null!));
        Assert.Throws<ArgumentException>(() => new ConsultaRegimesEspeciais(Bh, "010101", Competencia, "01"));
        Assert.Throws<ArgumentException>(() => new ConsultaBeneficio(Bh, "123", Competencia));
        Assert.Throws<ArgumentException>(() => new ConsultaBeneficio(Bh, "3106200000000A", Competencia));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConsultaRetencoes(0, Competencia));
    }

    [Fact]
    public void CadaConsultaMontaOCaminhoDaRotaNoAdn()
    {
        var pedidos = new[]
        {
            new OperacaoConsultaConvenio().Montar(new ConsultaConvenio(Bh), null!),
            new OperacaoConsultaAliquota().Montar(new ConsultaAliquota(Bh, "010101") { Competencia = Competencia }, null!),
            new OperacaoConsultaAliquota().Montar(new ConsultaAliquota(Bh, "010101", "002"), null!),
            new OperacaoConsultaRegimesEspeciais().Montar(new ConsultaRegimesEspeciais(Bh, "171901", Competencia), null!),
            new OperacaoConsultaRetencoes().Montar(new ConsultaRetencoes(Bh, Competencia), null!),
            new OperacaoConsultaBeneficio().Montar(new ConsultaBeneficio(Bh, "31062000000001", Competencia), null!),
        };

        Assert.Equal(
        [
            "3106200/convenio",
            "3106200/01.01.01.000/11-01-2026/aliquota",
            "3106200/01.01.01.002/historicoaliquotas",
            "3106200/17.19.01.000/11-01-2026/regimes_especiais",
            "3106200/11-01-2026/retencoes",
            "3106200/31062000000001/11-01-2026/beneficio",
        ], pedidos.Select(p => p.Caminho));
        Assert.All(pedidos, p =>
        {
            Assert.Equal((MetodoRest.Get, "Parametrizacao", (Destino?)Destino.ADN), (p.Metodo, p.Servico, p.Destino));
            Assert.Null(p.Xml);
        });
        Assert.Equal("01.01.01.000", new ConsultaAliquota(Bh, "010101").Servico);
    }

    // A leitura

    [Fact]
    public void OConvenioLeOsTiposMisturadosEGuardaOJson()
    {
        var operacao = new OperacaoConsultaConvenio();

        var booleano = operacao.Ler(new ConsultaConvenio(Bh), null, new RespostaRest(200, Json(new { parametrosConvenio = new { aderenteAmbienteNacional = true, aderenteEmissorNacional = false } })), null!);
        var texto = operacao.Ler(new ConsultaConvenio(Bh), null, new RespostaRest(200, "{\"ParametrosConvenio\": {\"AderenteAmbienteNacional\": \"Sim\", \"aderenteEmissorNacional\": \"0\"}}"), null!);
        var numero = operacao.Ler(new ConsultaConvenio(Bh), null, new RespostaRest(200, "{\"parametrosConvenio\": {\"aderenteAmbienteNacional\": 1, \"aderenteEmissorNacional\": \"N\"}}"), null!);

        Assert.All([booleano, texto, numero], l =>
        {
            Assert.Equal((200, "Convênio encontrado"), (l.CStat, l.Motivo));
            Assert.True(l.Retorno.Encontrado);
            Assert.True(l.Retorno.AderenteAmbienteNacional);
            Assert.False(l.Retorno.AderenteEmissorNacional);
            Assert.Empty(l.Problemas);
            Assert.Equal(JsonValueKind.Object, l.Retorno.Json!.Value.ValueKind);
        });
    }

    [Fact]
    public void OCampoQueNaoSeLeFicaNuloComOAvisoEOJsonAoLado()
    {
        var leitura = new OperacaoConsultaConvenio().Ler(new ConsultaConvenio(Bh), null,
            new RespostaRest(200, "{\"parametrosConvenio\": {\"aderenteAmbienteNacional\": \"talvez\", \"aderenteEmissorNacional\": [1]}}"), null!);

        Assert.True(leitura.Retorno.Encontrado);
        Assert.Null(leitura.Retorno.AderenteAmbienteNacional);
        Assert.Null(leitura.Retorno.AderenteEmissorNacional);
        Assert.Collection(leitura.Problemas,
            p => Assert.Equal(("NFSE-PARAMETRO", Severidade.Aviso, "AderenteAmbienteNacional", "parametrosConvenio/aderenteAmbienteNacional"), (p.Codigo, p.Severidade, p.Propriedade, p.Caminho)),
            p => Assert.Equal("AderenteEmissorNacional", p.Propriedade));
        Assert.Equal("talvez", leitura.Retorno.Json!.Value.GetProperty("parametrosConvenio").GetProperty("aderenteAmbienteNacional").GetString());
    }

    [Fact]
    public void OSucessoSemOGrupoEAviso()
    {
        var leitura = new OperacaoConsultaConvenio().Ler(new ConsultaConvenio(Bh), null, new RespostaRest(200, "{}"), null!);

        Assert.True(leitura.Retorno.Encontrado);
        Assert.Equal("parametrosConvenio", Assert.Single(leitura.Problemas).Caminho);
    }

    [Fact]
    public void OErroTrazAMensagemNoMotivo()
    {
        var leitura = new OperacaoConsultaConvenio().Ler(new ConsultaConvenio(Bh), null,
            new RespostaRest(404, Json(new { tipoAmbiente = 2, mensagem = "O município não tem convênio." })), null!);
        var semCorpo = new OperacaoConsultaConvenio().Ler(new ConsultaConvenio(Bh), null, new RespostaRest(404, null), null!);

        Assert.Equal((404, "O município não tem convênio."), (leitura.CStat, leitura.Motivo));
        Assert.False(leitura.Retorno.Encontrado);
        Assert.Equal("O município não tem convênio.", leitura.Retorno.Mensagem);
        Assert.Null(leitura.Retorno.AderenteAmbienteNacional);
        Assert.Empty(leitura.Problemas);
        Assert.Equal("HTTP 404", semCorpo.Motivo);
        Assert.Null(semCorpo.Retorno.Json);
    }

    [Theory]
    [InlineData("[1, 2]")]
    [InlineData("\"texto\"")]
    public void ARaizQueNaoEObjetoEFormatException(string corpo) =>
        Assert.Throws<FormatException>(() => new OperacaoConsultaConvenio().Ler(new ConsultaConvenio(Bh), null, new RespostaRest(200, corpo), null!));

    [Fact]
    public void OCorpoQueNaoEJsonEJsonException() =>
        Assert.ThrowsAny<JsonException>(() => new OperacaoConsultaConvenio().Ler(new ConsultaConvenio(Bh), null, new RespostaRest(200, "<html>"), null!));

    [Fact]
    public void AAliquotaLeAVigenciaDoServicoConsultado()
    {
        var corpo = "{\"aliquotas\": {\"01.01.01.000\": [{\"Aliq\": 2.5, \"DtIni\": \"2022-07-01T00:00:00\", \"DtFim\": \"2025-12-31T00:00:00\"},"
            + " {\"Aliq\": \"3,00\", \"DtIni\": \"2026-01-01\", \"DtFim\": null}], \"09.99.99.000\": [{\"Aliq\": 5}]}}";

        var leitura = new OperacaoConsultaAliquota().Ler(new ConsultaAliquota(Bh, "010101"), null, new RespostaRest(200, corpo), null!);

        Assert.Empty(leitura.Problemas);
        Assert.Collection(leitura.Retorno.Aliquotas,
            a =>
            {
                Assert.Equal((2.5m, new DateTime(2022, 7, 1), new DateTime(2025, 12, 31)), (a.Aliquota, a.Inicio, a.Fim));
                Assert.Equal(DateTimeKind.Unspecified, a.Inicio!.Value.Kind);
            },
            a => Assert.Equal((3.00m, new DateTime(2026, 1, 1), (DateTime?)null), (a.Aliquota, a.Inicio, a.Fim)));
    }

    [Fact]
    public void AAliquotaDeOutroServicoUnicoELidaComOAviso()
    {
        var leitura = new OperacaoConsultaAliquota().Ler(new ConsultaAliquota(Bh, "010101"), null,
            new RespostaRest(200, "{\"aliquotas\": {\"01.01.01\": [{\"Aliq\": 2, \"DtIni\": \"ontem\"}]}}"), null!);

        var aliquota = Assert.Single(leitura.Retorno.Aliquotas);
        Assert.Equal((2m, (DateTime?)null), (aliquota.Aliquota, aliquota.Inicio));
        Assert.Equal(2, leitura.Problemas.Count);
        Assert.All(leitura.Problemas, p => Assert.Equal("NFSE-PARAMETRO", p.Codigo));
    }

    [Fact]
    public void OsRegimesSaoUmItemPorVigencia()
    {
        var corpo = "{\"regimesEspeciais\": {\"17.19.01.000\": {\"1\": [{\"sit\": 1, \"dtIni\": \"2022-07-01T00:00:00\"}], \"6\": [{\"sit\": \"2\"}, {\"sit\": 1}], \"7\": [{\"sit\": 1}]}}}";

        var leitura = new OperacaoConsultaRegimesEspeciais().Ler(new ConsultaRegimesEspeciais(Bh, "171901", Competencia), null, new RespostaRest(200, corpo), null!);

        Assert.Equal(
            new (RegimeEspecialTributacao?, int?)[]
            {
                (RegimeEspecialTributacao.AtoCooperado, 1),
                (RegimeEspecialTributacao.SociedadeProfissionais, 2),
                (RegimeEspecialTributacao.SociedadeProfissionais, 1),
                (null, 1),
            },
            leitura.Retorno.Regimes.Select(r => (r.Regime, r.Situacao)));
        Assert.Equal(new DateTime(2022, 7, 1), leitura.Retorno.Regimes[0].Inicio);
        Assert.Equal("regimesEspeciais/17.19.01.000/7", Assert.Single(leitura.Problemas).Caminho);
    }

    [Fact]
    public void AsRetencoesLeemOArtigo6OsTiposEOsResponsaveis()
    {
        var corpo = Json(new
        {
            retencoes = new
            {
                art6 = new { habilitado = "S" },
                retMun = new object[]
                {
                    new { desc = "Construção civil", tpRet = new object[] { 2, "3", 8 }, respTrib = new object[] { new { tpInsc = 1, insc = "52998224725" }, new { tpInsc = 2, insc = 11222333000181 } } },
                },
            },
        });

        var leitura = new OperacaoConsultaRetencoes().Ler(new ConsultaRetencoes(Bh, Competencia), null, new RespostaRest(200, corpo), null!);

        Assert.True(leitura.Retorno.Artigo6Habilitado);
        var retencao = Assert.Single(leitura.Retorno.Retencoes);
        Assert.Equal("Construção civil", retencao.Descricao);
        Assert.Equal([RetencaoIssqn.Tomador, RetencaoIssqn.Intermediario], retencao.Tipos);
        Assert.Equal(["52998224725", "11222333000181"], retencao.Responsaveis);
        Assert.Equal("retencoes/retMun/tpRet", Assert.Single(leitura.Problemas).Caminho);
    }

    [Fact]
    public void ORetMunVazioEAsRetencoesSemArtigo6()
    {
        var leitura = new OperacaoConsultaRetencoes().Ler(new ConsultaRetencoes(Bh, Competencia), null, new RespostaRest(200, "{\"retencoes\": {\"retMun\": []}}"), null!);

        Assert.Null(leitura.Retorno.Artigo6Habilitado);
        Assert.Empty(leitura.Retorno.Retencoes);
        Assert.Empty(leitura.Problemas);
    }

    [Fact]
    public void OBeneficioLeOTipoEAAliquota()
    {
        var corpo = Json(new { beneficio = new { numBenef = "31062000000001", desc = "Alíquota reduzida", dtIni = "2022-07-01T00:00:00", tpoBenef = "4", aliqDiferenc = 2.0m, restAoMun = false } });

        var leitura = new OperacaoConsultaBeneficio().Ler(new ConsultaBeneficio(Bh, "31062000000001", Competencia), null, new RespostaRest(200, corpo), null!);

        var retorno = leitura.Retorno;
        Assert.True(retorno.Encontrado);
        Assert.Equal(("31062000000001", "Alíquota reduzida", (TipoBeneficio?)TipoBeneficio.AliquotaDiferenciada, (decimal?)2.0m), (retorno.Numero, retorno.Descricao, retorno.Tipo, retorno.AliquotaDiferenciada));
        Assert.Equal(new DateTime(2022, 7, 1), retorno.Inicio);
        Assert.Null(retorno.Fim);
        Assert.Empty(leitura.Problemas);
    }

    [Fact]
    public void OBeneficioComOTipoForaDaEnumeracaoEOQueNaoHa()
    {
        var operacao = new OperacaoConsultaBeneficio();
        var parametro = new ConsultaBeneficio(Bh, "31062000000001", Competencia);

        var fora = operacao.Ler(parametro, null, new RespostaRest(200, Json(new { beneficio = new { numBenef = "31062000000001", tpoBenef = 7 } })), null!);
        var ausente = operacao.Ler(parametro, null, new RespostaRest(404, Json(new { mensagem = "Não há benefício." })), null!);

        Assert.Null(fora.Retorno.Tipo);
        Assert.Equal(("Tipo", Severidade.Aviso), (Assert.Single(fora.Problemas).Propriedade, fora.Problemas[0].Severidade));
        Assert.False(ausente.Retorno.Encontrado);
        Assert.Null(ausente.Retorno.Numero);
        Assert.Equal("Não há benefício.", ausente.Retorno.Mensagem);
    }
}
