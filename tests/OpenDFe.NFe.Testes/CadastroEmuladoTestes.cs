using System.Net;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using ConsultaCadastro = OpenDFe.NFe.ConsultaCadastro;

namespace OpenDFe.Testes;

/// <summary>
/// A consulta cadastro pela lib inteira contra a SEFAZ emulada (etapa 8.2): os contribuintes entram pela rota
/// <c>POST /_controle/cadastros</c> (e o solicitante irregular, pela <c>/_controle/irregulares</c>), com um <c>HttpClient</c> de
/// TLS mútuo que confia só na AC do emulador. Cobre a consulta por CNPJ, por inscrição e por CPF, o CNPJ com mais de uma
/// inscrição (112), os não cadastrados (259, 261, 264), a UF que o autorizador não atende (265), o solicitante irregular (257),
/// o serviço paralisado, a SVRS para o ES e o <c>NFE-CADASTRO</c> para outra UF da SVRS.
/// </summary>
public sealed class CadastroEmuladoTestes : IClassFixture<EmuladorFixture>
{
    private const string CnpjA = NotasTeste.CnpjEmitente; // o solicitante: o certificado do núcleo
    private const string CnpjContribuinte = "98765432000198";
    private const string CpfContribuinte = "52998224725";
    private const string Inscricao = "0623079040081";

    private static readonly X509Certificate2 Certificado = ApoioEmulado.Certificado;

    private readonly Emulador _emulador;
    private readonly HttpClient _http;

    public CadastroEmuladoTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
        _emulador.Cadastros.Limpar();
        _emulador.Irregulares.Limpar();
        _http = Cliente(_emulador);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // O cliente do /_controle: o mesmo TLS mútuo (o certificado do núcleo) e a confiança só na AC do emulador.
    private static HttpClient Cliente(Emulador emulador)
    {
        var autoridade = emulador.Autoridade;
        var handler = new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions { ClientCertificates = [Certificado] },
        };
        handler.SslOptions.RemoteCertificateValidationCallback = (_, certificado, _, erros) =>
            erros == SslPolicyErrors.None || ConexaoTeste.SobeAutoridade(autoridade, certificado, erros);
        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
    }

    private async Task Cadastrar(object contribuinte)
    {
        using var resposta = await _http.PostAsJsonAsync(new Uri(_emulador.Endereco, "_controle/cadastros"), contribuinte, Ct);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private static object Contribuinte(string uf = "MG", string ie = Inscricao, string? cnpj = CnpjContribuinte, string? cpf = null) => new
    {
        uf,
        ie,
        cnpj,
        cpf,
        nome = "CONTRIBUINTE DE TESTE LTDA",
        fantasia = "TESTE",
        regimeApuracao = "NORMAL",
        cnae = "4751201",
        inicioAtividade = "2010-03-01",
        ultimaSituacao = "2020-05-10",
        ieUnica = "0623079040999",
        endereco = new
        {
            logradouro = "RUA DE TESTE",
            numero = "100",
            complemento = "SALA 2",
            bairro = "CENTRO",
            codigoMunicipio = "3106200",
            municipio = "BELO HORIZONTE",
            cep = "30110000",
        },
    };

    private DFe Criar(ConexoesSefaz conexoes) => DFe.Criar(new LeiauteNFe400(), Documento.NFe, new ConfiguracaoDFe
    {
        Ambiente = Ambiente.Homologacao,
        UF = UF.MG,
        Certificado = Certificado,
        Conexoes = conexoes,
        Urls = ApoioEmulado.Urls(_emulador, Documento.NFe),
    });

    private async Task<ResultadoWS<RetornoCadastro>> Consultar(ConsultaCadastro consulta)
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var resultado = await Criar(conexoes).ExecutarAsync(consulta, Ct);
        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Empty(SchemaXml.Carregar(typeof(LeiauteNFe400).Assembly, SchemasNFe400.Recurso("CadConsultaCadastro/retConsCad_v2.00.xsd"), "215")
            .Validar(resultado.Rastro.XmlRetorno!));
        return resultado;
    }

    [Fact]
    public async Task APorCnpjTrazOContribuinteComTodosOsCamposEOEndereco()
    {
        await Cadastrar(Contribuinte());

        var resultado = await Consultar(new ConsultaCadastro(UF.MG, "98.765.432/0001-98"));

        Assert.Equal(111, resultado.CStat);
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        var r = resultado.Retorno!;
        Assert.Equal((111, "Consulta cadastro com uma ocorrência", UF.MG, UF.MG, true), (r.Status, r.Motivo, r.UF, r.UFAtendimento, r.Encontrado));
        Assert.StartsWith("MG-", r.VersaoAplicativo, StringComparison.Ordinal);
        Assert.NotNull(r.Consulta);
        var c = Assert.Single(r.Contribuintes);
        Assert.Equal((Inscricao, CnpjContribuinte, (string?)null, UF.MG), (c.InscricaoEstadual, c.Cnpj, c.Cpf, c.UF));
        Assert.Equal((SituacaoCadastral.Habilitado, CredenciamentoDFe.Credenciado, CredenciamentoDFe.NaoInformado),
            (c.Situacao, c.CredenciamentoNFe, c.CredenciamentoCTe));
        Assert.Equal(("CONTRIBUINTE DE TESTE LTDA", "TESTE", "NORMAL", "4751201"), (c.Nome, c.Fantasia, c.RegimeApuracao, c.Cnae));
        Assert.Equal((new DateOnly(2010, 3, 1), new DateOnly(2020, 5, 10), (DateOnly?)null), (c.InicioAtividade, c.UltimaSituacao, c.Baixa));
        Assert.Equal(("0623079040999", (string?)null), (c.InscricaoUnica, c.InscricaoAtual));
        var e = c.Endereco!;
        Assert.Equal(("RUA DE TESTE", "100", "SALA 2", "CENTRO", "3106200", "BELO HORIZONTE", "30110000"),
            (e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.CodigoMunicipio, e.Municipio, e.Cep));
    }

    [Fact]
    public async Task PelaInscricaoTrazOContribuinteDela()
    {
        await Cadastrar(Contribuinte());

        var resultado = await Consultar(ConsultaCadastro.Inscricao(UF.MG, Inscricao));

        Assert.Equal(111, resultado.CStat);
        Assert.Equal(CnpjContribuinte, Assert.Single(resultado.Retorno!.Contribuintes).Cnpj);
    }

    [Fact]
    public async Task PorCpfTrazOContribuinteDePessoaFisica()
    {
        await Cadastrar(Contribuinte(ie: "0623079040155", cnpj: null, cpf: CpfContribuinte));

        var resultado = await Consultar(new ConsultaCadastro(UF.MG, "529.982.247-25"));

        Assert.Equal(111, resultado.CStat);
        var c = Assert.Single(resultado.Retorno!.Contribuintes);
        Assert.Equal((CpfContribuinte, (string?)null), (c.Cpf, c.Cnpj));
    }

    [Fact]
    public async Task OCnpjComMaisDeUmaInscricaoNaUfE112()
    {
        await Cadastrar(Contribuinte());
        await Cadastrar(Contribuinte(ie: "0623079040155"));

        var resultado = await Consultar(new ConsultaCadastro(UF.MG, CnpjContribuinte));

        Assert.Equal(112, resultado.CStat);
        Assert.Equal((112, true), (resultado.Retorno!.Status, resultado.Retorno.Encontrado));
        Assert.Equal([Inscricao, "0623079040155"], resultado.Retorno.Contribuintes.Select(c => c.InscricaoEstadual).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task OContribuinteNaoCadastradoE259PorCnpj261PorInscricaoE264PorCpf()
    {
        var cnpj = await Consultar(new ConsultaCadastro(UF.MG, CnpjContribuinte));
        var ie = await Consultar(ConsultaCadastro.Inscricao(UF.MG, Inscricao));
        var cpf = await Consultar(new ConsultaCadastro(UF.MG, CpfContribuinte));

        Assert.Equal([259, 261, 264], [cnpj.CStat!.Value, ie.CStat!.Value, cpf.CStat!.Value]);
        Assert.All([cnpj, ie, cpf], r =>
        {
            Assert.False(r.Retorno!.Encontrado);
            Assert.Empty(r.Retorno.Contribuintes);
        });
    }

    [Fact]
    public async Task OCadastroDeOutraUfNaoApareceNaConsultaDaUfDeLa()
    {
        await Cadastrar(Contribuinte(uf: "SP"));

        var emMinas = await Consultar(new ConsultaCadastro(UF.MG, CnpjContribuinte));
        var emSaoPaulo = await Consultar(new ConsultaCadastro(UF.SP, CnpjContribuinte));

        Assert.Equal((259, 111), (emMinas.CStat!.Value, emSaoPaulo.CStat!.Value));
        Assert.Equal(Destino.SP, emSaoPaulo.Rastro.Autorizador);
    }

    [Fact]
    public async Task AUfQueOAutorizadorNaoAtendeE265QuandoODestinoEInformado()
    {
        var consulta = new ConsultaCadastro(UF.RJ, CnpjContribuinte) { Destino = Destino.MG };

        var resultado = await Consultar(consulta);

        Assert.Equal(265, resultado.CStat);
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.Equal(UF.RJ, resultado.Retorno!.UF);
    }

    [Fact]
    public async Task OSolicitanteIrregularE257()
    {
        using (var resposta = await _http.PostAsJsonAsync(new Uri(_emulador.Endereco, "_controle/irregulares"), new { documento = CnpjA }, Ct))
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        await Cadastrar(Contribuinte());

        var resultado = await Consultar(new ConsultaCadastro(UF.MG, CnpjContribuinte));

        Assert.Equal(257, resultado.CStat);
        Assert.Empty(resultado.Retorno!.Contribuintes);
    }

    [Theory]
    [InlineData("108")]
    [InlineData("109")]
    public async Task OServicoParalisadoVoltaSemContribuintes(string codigo)
    {
        await Cadastrar(Contribuinte());
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.Paralisacao, "MG", "CadConsultaCadastro4", Codigo: codigo));

        var resultado = await Consultar(new ConsultaCadastro(UF.MG, CnpjContribuinte));

        Assert.Equal(int.Parse(codigo, System.Globalization.CultureInfo.InvariantCulture), resultado.CStat);
        Assert.Empty(resultado.Retorno!.Contribuintes);
    }

    [Fact]
    public async Task OEspiritoSantoVaiASvrsEAcheONoCadastroDele()
    {
        await Cadastrar(Contribuinte(uf: "ES"));

        var resultado = await Consultar(new ConsultaCadastro(UF.ES, CnpjContribuinte));

        Assert.Equal(111, resultado.CStat);
        Assert.Equal(Destino.SVRS, resultado.Rastro.Autorizador);
        Assert.Equal(UF.ES, resultado.Retorno!.UF);
        Assert.StartsWith("SVRS-", resultado.Retorno.VersaoAplicativo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OutraUfDaSvrsENFeCadastroSemChegarAoEmulador()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = Criar(conexoes);
        var antes = _emulador.Pedidos.Listar().Count;

        var resultado = await dfe.ExecutarAsync(new ConsultaCadastro(UF.RJ, CnpjContribuinte), Ct);

        Assert.Equal(SituacaoResultado.RecusadoLocalmente, resultado.Situacao);
        Assert.Equal("NFE-CADASTRO", Assert.Single(resultado.Problemas).Codigo);
        Assert.Equal(antes, _emulador.Pedidos.Listar().Count);
    }
}
