using System.IO.Compression;
using System.Text;
using System.Text.Json;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

// As operações da 9.2 sem rede (docs/NFSE.md 3.3, D4, D5 e D12): o que o leiaute declara, a montagem dos pedidos e a leitura
// das respostas da Sefin pelo JSON (o sucesso, os erros, o que não se lê). A ponta a ponta está em EmissaoEmuladaTestes.
public class OperacoesNFSeTestes
{
    private static string Compactar(string texto)
    {
        using var saida = new MemoryStream();
        using (var gzip = new GZipStream(saida, CompressionLevel.Optimal, leaveOpen: true))
            gzip.Write(Encoding.UTF8.GetBytes(texto));
        return Convert.ToBase64String(saida.ToArray());
    }

    private static string Json(object valor) => JsonSerializer.Serialize(valor);

    private static string NotaXml() => NFSeTeste.Xml(NFSeTeste.Nota(DpsTeste.Minima()));

    private static readonly IdentificadorDps Id = IdentificadorDps.Montar(DpsTeste.Municipio, DpsTeste.Cpf, "1", 1);

    // A declaração

    [Fact]
    public void DeclaraAsDezOperacoesEAsBasesDaSefinEDoAdnSemAtendimento()
    {
        var declaracao = DeclaracaoLeiaute.Criar(LeiauteNFSe101.Instancia, Documento.NFSe);

        Assert.Equal([typeof(EmissaoNFSe), typeof(ConsultaNFSe), typeof(ConsultaDps), typeof(EnvioEventoNFSe), typeof(ConsultaEventosNFSe),
            typeof(ConsultaConvenio), typeof(ConsultaAliquota), typeof(ConsultaRegimesEspeciais), typeof(ConsultaRetencoes), typeof(ConsultaBeneficio)],
            declaracao.Operacoes.Select(o => o.Parametro));
        Assert.Equal([typeof(RetornoEmissaoNFSe), typeof(RetornoConsultaNFSe), typeof(RetornoConsultaDps), typeof(RetornoEventoNFSe), typeof(RetornoEventosNFSe),
            typeof(RetornoConvenio), typeof(RetornoAliquota), typeof(RetornoRegimesEspeciais), typeof(RetornoRetencoes), typeof(RetornoBeneficio)],
            declaracao.Operacoes.Select(o => o.Retorno));
        Assert.Empty(declaracao.Atendimentos);
        Assert.Empty(declaracao.Servicos);
        Assert.Collection(declaracao.Enderecos,
            e =>
            {
                Assert.Equal((Destino.SefinNacional, Ambiente.Producao, "SefinNacional"), (e.Autorizador, e.Ambiente, e.Servico));
                Assert.Equal(new Uri("https://sefin.nfse.gov.br/SefinNacional"), e.Url);
            },
            e =>
            {
                Assert.Equal((Destino.SefinNacional, Ambiente.Homologacao, "SefinNacional"), (e.Autorizador, e.Ambiente, e.Servico));
                Assert.Equal(new Uri("https://sefin.producaorestrita.nfse.gov.br/SefinNacional"), e.Url);
            },
            e =>
            {
                Assert.Equal((Destino.ADN, Ambiente.Producao, "Parametrizacao"), (e.Autorizador, e.Ambiente, e.Servico));
                Assert.Equal(new Uri("https://adn.nfse.gov.br/parametrizacao"), e.Url);
            },
            e =>
            {
                Assert.Equal((Destino.ADN, Ambiente.Homologacao, "Parametrizacao"), (e.Autorizador, e.Ambiente, e.Servico));
                Assert.Equal(new Uri("https://adn.producaorestrita.nfse.gov.br/parametrizacao"), e.Url);
            });
    }

    [Theory]
    [InlineData(UF.MG)]
    [InlineData(UF.AC)]
    public void ONucleoDaNFSeAtendeQualquerUFEMasNaoContingencia(UF uf)
    {
        using var conexoes = new ConexoesSefaz();
        var certificado = new CertificadoTeste { Cnpj = DpsTeste.CnpjPrestador }.Gerar();

        var dfe = DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = uf,
            Certificado = certificado,
            Conexoes = conexoes,
        });

        Assert.True(dfe.Suporta<EmissaoNFSe>());
        Assert.False(dfe.Suporta<EnvioEventoFalso>());
        Assert.Throws<ArgumentException>(() => DFe.Criar(LeiauteNFSe101.Instancia, Documento.NFSe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = uf,
            Certificado = certificado,
            Conexoes = conexoes,
            Emissao = TipoEmissao.ContingenciaOffline,
            Contingencia = new Contingencia(DateTimeOffset.UtcNow, "Sem internet no momento"),
        }));
    }

    // Os parâmetros e a montagem

    [Fact]
    public void OsParametrosRecusamOVazio()
    {
        Assert.Throws<ArgumentNullException>(() => new EmissaoNFSe(null!));
        Assert.Throws<ArgumentException>(() => new ConsultaNFSe(default));
        Assert.Throws<ArgumentException>(() => new ConsultaDps(default));
    }

    [Fact]
    public void AsConsultasMontamOCaminhoEOMetodo()
    {
        var chave = ChaveNFSe.Ler(NFSeTeste.Chave);

        var nota = new OperacaoConsultaNFSe().Montar(new ConsultaNFSe(chave), null!);
        var get = new OperacaoConsultaDps().Montar(new ConsultaDps(Id), null!);
        var head = new OperacaoConsultaDps().Montar(new ConsultaDps(Id) { ApenasExistencia = true }, null!);

        Assert.Equal((MetodoRest.Get, "nfse/" + NFSeTeste.Chave, "SefinNacional"), (nota.Metodo, nota.Caminho, nota.Servico));
        Assert.Equal((MetodoRest.Get, "dps/" + Id.Valor), (get.Metodo, get.Caminho));
        Assert.Equal((MetodoRest.Head, "dps/" + Id.Valor), (head.Metodo, head.Caminho));
        Assert.All([nota, get, head], p =>
        {
            Assert.Null(p.Xml);
            Assert.Equal(Destino.SefinNacional, p.Destino);
        });
    }

    // A leitura da emissão

    [Fact]
    public void AEmissaoGeradaTrazAChaveOXmlEANotaLida()
    {
        var xml = NotaXml();
        var corpo = Json(new { tipoAmbiente = 2, idDps = Id.Valor, chaveAcesso = NFSeTeste.Chave, nfseXmlGZipB64 = Compactar(xml), alertas = new[] { new { codigo = "A01", descricao = "Alerta" } } });

        var leitura = new OperacaoEmissaoNFSe().Ler(new EmissaoNFSe(DpsTeste.Minima()), "<DPS/>", new RespostaRest(201, corpo), null!);

        Assert.Equal(201, leitura.CStat);
        Assert.Equal("NFS-e gerada", leitura.Motivo);
        var retorno = leitura.Retorno;
        Assert.True(retorno.Emitida);
        Assert.Equal(NFSeTeste.Chave, retorno.Chave!.Value.Valor);
        Assert.Equal(xml, retorno.Xml);
        Assert.Equal(42L, retorno.Nota!.Numero);
        Assert.Empty(retorno.Erros);
        Assert.Equal("A01: Alerta", Assert.Single(retorno.Alertas).ToString());
        Assert.Empty(leitura.Problemas);
    }

    [Fact]
    public void ARejeicaoTrazOsErrosNaOrdemEOPrimeiroNoMotivo()
    {
        var corpo = Json(new
        {
            erros = new object[]
            {
                new { codigo = "E0014", descricao = "DPS já existe", complemento = "A NFS-e gerada é a " + NFSeTeste.Chave },
                new { Codigo = 1235, Descricao = "Schema" },
            },
        });

        var leitura = new OperacaoEmissaoNFSe().Ler(new EmissaoNFSe(DpsTeste.Minima()), "<DPS/>", new RespostaRest(400, corpo), null!);

        Assert.Equal(400, leitura.CStat);
        Assert.Equal("E0014: DPS já existe (A NFS-e gerada é a " + NFSeTeste.Chave + ")", leitura.Motivo);
        Assert.False(leitura.Retorno.Emitida);
        Assert.Null(leitura.Retorno.Chave);
        Assert.Null(leitura.Retorno.Nota);
        Assert.Collection(leitura.Retorno.Erros,
            e => Assert.Equal(("E0014", "DPS já existe"), (e.Codigo, e.Descricao)),
            e => Assert.Equal(("1235", "Schema", (string?)null), (e.Codigo, e.Descricao, e.Complemento)));
    }

    [Fact]
    public void OErroSemCorpoOuSemErrosTemOStatusNoMotivo()
    {
        var leitura = new OperacaoEmissaoNFSe().Ler(new EmissaoNFSe(DpsTeste.Minima()), "<DPS/>", new RespostaRest(422, "{\"outro\": 1}"), null!);

        Assert.Equal("HTTP 422", leitura.Motivo);
        Assert.Empty(leitura.Retorno.Erros);
    }

    public static TheoryData<string> Ilegiveis() =>
    [
        "{\"chaveAcesso\": \"" + NFSeTeste.Chave + "\"}",
        "{\"nfseXmlGZipB64\": \"" + Compactar("<NFSe/>") + "\"}",
        "{\"chaveAcesso\": \"123\", \"nfseXmlGZipB64\": \"" + Compactar("<NFSe/>") + "\"}",
        "{\"chaveAcesso\": \"" + NFSeTeste.Chave + "\", \"nfseXmlGZipB64\": \"não é base64\"}",
        "{\"chaveAcesso\": \"" + NFSeTeste.Chave + "\", \"nfseXmlGZipB64\": \"" + Convert.ToBase64String(Encoding.UTF8.GetBytes("sem gzip")) + "\"}",
        "{\"chaveAcesso\": true}",
        "{\"erros\": {\"codigo\": \"E1\"}}",
        "{\"erros\": [\"E1\"]}",
        "[1, 2]",
    ];

    [Theory]
    [MemberData(nameof(Ilegiveis))]
    public void ASucessoSemANotaOuOJsonForaDaFormaEFormatException(string corpo) =>
        Assert.Throws<FormatException>(() => new OperacaoEmissaoNFSe().Ler(new EmissaoNFSe(DpsTeste.Minima()), "<DPS/>", new RespostaRest(201, corpo), null!));

    [Fact]
    public void OJsonMalFormadoEJsonException() =>
        Assert.ThrowsAny<JsonException>(() => new OperacaoEmissaoNFSe().Ler(new EmissaoNFSe(DpsTeste.Minima()), "<DPS/>", new RespostaRest(201, "{ quebrado"), null!));

    [Fact]
    public void ODocumentoCompactadoQueNaoEUtf8EFormatException()
    {
        using var saida = new MemoryStream();
        using (var gzip = new GZipStream(saida, CompressionLevel.Optimal, leaveOpen: true))
            gzip.Write([0xC3, 0x28]);

        Assert.Throws<FormatException>(() => RespostaSefin.Descompactar(Convert.ToBase64String(saida.ToArray())));
    }

    // A consulta pela chave

    [Fact]
    public void AConsultaDaChaveAchaANotaOuDevolveOErro()
    {
        var chave = ChaveNFSe.Ler(NFSeTeste.Chave);
        var xml = NotaXml();
        var operacao = new OperacaoConsultaNFSe();

        var achada = operacao.Ler(new ConsultaNFSe(chave), null, new RespostaRest(200, Json(new { chaveAcesso = NFSeTeste.Chave, nfseXmlGZipB64 = Compactar(xml) })), null!);
        var ausente = operacao.Ler(new ConsultaNFSe(chave), null, new RespostaRest(404, Json(new { erros = new[] { new { codigo = "E0044", descricao = "NFS-e não existe" } } })), null!);

        Assert.True(achada.Retorno.Encontrada);
        Assert.Equal("NFS-e encontrada", achada.Motivo);
        Assert.Equal(xml, achada.Retorno.Xml);
        Assert.NotNull(achada.Retorno.Nota);
        Assert.False(ausente.Retorno.Encontrada);
        Assert.Equal(404, ausente.CStat);
        Assert.Equal("E0044", Assert.Single(ausente.Retorno.Erros).Codigo);
    }

    // A consulta da DPS

    [Fact]
    public void AConsultaDaDpsDizSeExisteENoGetAChave()
    {
        var operacao = new OperacaoConsultaDps();

        var get = operacao.Ler(new ConsultaDps(Id), null, new RespostaRest(200, Json(new { idDps = Id.Valor, chaveAcesso = NFSeTeste.Chave })), null!);
        var head = operacao.Ler(new ConsultaDps(Id) { ApenasExistencia = true }, null, new RespostaRest(200, null), null!);
        var naoHead = operacao.Ler(new ConsultaDps(Id) { ApenasExistencia = true }, null, new RespostaRest(404, null), null!);
        var naoGet = operacao.Ler(new ConsultaDps(Id), null, new RespostaRest(404, Json(new { erros = new[] { new { codigo = "X", descricao = "Não há" } } })), null!);

        Assert.True(get.Retorno.Existe);
        Assert.Equal(NFSeTeste.Chave, get.Retorno.Chave!.Value.Valor);
        Assert.True(head.Retorno.Existe);
        Assert.Null(head.Retorno.Chave);
        Assert.False(naoHead.Retorno.Existe);
        Assert.Equal("HTTP 404", naoHead.Motivo);
        Assert.False(naoGet.Retorno.Existe);
        Assert.Equal("X: Não há", naoGet.Motivo);
    }

    [Fact]
    public void OGetDaDpsSemAChaveEFormatException() =>
        Assert.Throws<FormatException>(() => new OperacaoConsultaDps().Ler(new ConsultaDps(Id), null, new RespostaRest(200, "{}"), null!));

    // Um parâmetro que o leiaute não registra.
    private sealed class EnvioEventoFalso : ParametroWS<RetornoConsultaDps>
    {
    }
}
