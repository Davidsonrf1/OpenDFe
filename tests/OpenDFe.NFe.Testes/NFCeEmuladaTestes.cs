using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;
using ConsultaProtocolo = OpenDFe.NFe.ConsultaProtocolo;

namespace OpenDFe.Testes;

/// <summary>
/// A NFC-e pela lib inteira contra a SEFAZ emulada (etapa 5.1): o QR Code na versão 3 e na versão 2 com o CSC padrão do
/// emulador, o CSC não cadastrado (462), revogado (463) e errado (464), o emitente CPF na versão 3, a NF-e com o DANFE
/// Simplificado Tipo 2, o lote com a consulta do recibo, o Preparar, a recuperação depois da queda e a consulta do
/// protocolo. As notas são emitidas agora, e o repositório e o CSC do emulador são limpos a cada teste.
/// </summary>
public sealed class NFCeEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private const string CscPadrao = "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO";
    private const string RaizEmitente = "11222333";

    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = NotasTeste.CnpjEmitente }.GerarPfx("teste"), "teste");

    private readonly Emulador _emulador;

    public NFCeEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
        _emulador.Codigos.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task UmaNFCeEAutorizadaNaVersao3()
    {
        using var conexoes = Conexoes();

        var resultado = await Criar(conexoes).ExecutarAsync(new Autorizacao(Nota(1)), Ct);

        var nota = Autorizada(resultado);
        Assert.Equal(Documento.NFCe, nota.Chave!.Modelo);
        Assert.EndsWith("|3|2", QrCode(nota.Xml!), StringComparison.Ordinal);
        Assert.Contains(_emulador.Repositorio.Notas(), n => n.Chave == nota.Chave.ToString());
    }

    [Fact]
    public async Task UmaNFCeEAutorizadaNaVersao2ComOCscPadraoDoEmulador()
    {
        using var conexoes = Conexoes();
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(1, CscPadrao));

        var resultado = await Criar(conexoes, configuracao).ExecutarAsync(new Autorizacao(Nota(1)), Ct);

        var nota = Autorizada(resultado);
        Assert.Contains("|2|2|1|", QrCode(nota.Xml!), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OCscNaoCadastradoE462()
    {
        using var conexoes = Conexoes();
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(7, "UM-CSC-QUE-A-SEFAZ-NAO-CONHECE"));

        var resultado = await Criar(conexoes, configuracao).ExecutarAsync(new Autorizacao(Nota(1)), Ct);

        Assert.True(resultado.Processado);
        Assert.Equal(462, Assert.Single(resultado.Retorno.Notas).Protocolo!.Status);
    }

    [Fact]
    public async Task OCscRevogadoE463()
    {
        _emulador.Codigos.Cadastrar(new PedidoCsc("MG", RaizEmitente, "2", "CSC-CADASTRADO-E-REVOGADO-0002"));
        Assert.True(_emulador.Codigos.Revogar("MG", RaizEmitente, "2", DateTimeOffset.UtcNow.AddHours(-1)));
        using var conexoes = Conexoes();
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(2, "CSC-CADASTRADO-E-REVOGADO-0002"));

        var resultado = await Criar(conexoes, configuracao).ExecutarAsync(new Autorizacao(Nota(1)), Ct);

        Assert.Equal(463, Assert.Single(resultado.Retorno!.Notas).Protocolo!.Status);
    }

    [Fact]
    public async Task OCscComOutroCodigoE464()
    {
        _emulador.Codigos.Cadastrar(new PedidoCsc("MG", RaizEmitente, "2", "CSC-QUE-A-SEFAZ-TEM-NA-BASE-0002"));
        using var conexoes = Conexoes();
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(2, "CSC-DIFERENTE-DO-DA-SEFAZ-000002"));

        var resultado = await Criar(conexoes, configuracao).ExecutarAsync(new Autorizacao(Nota(1)), Ct);

        Assert.Equal(464, Assert.Single(resultado.Retorno!.Notas).Protocolo!.Status);
    }

    [Fact]
    public async Task OCscCadastradoPelaRotaEAutoriza()
    {
        _emulador.Codigos.Cadastrar(new PedidoCsc("MG", RaizEmitente, "2", "CSC-CADASTRADO-PELA-ROTA-00002"));
        using var conexoes = Conexoes();
        var configuracao = new ConfiguracaoNFCe(new CodigoSeguranca(2, "CSC-CADASTRADO-PELA-ROTA-00002"));

        var resultado = await Criar(conexoes, configuracao).ExecutarAsync(new Autorizacao(Nota(1)), Ct);

        Autorizada(resultado);
    }

    [Fact]
    public async Task OEmitenteCpfEAutorizadoNaVersao3()
    {
        using var conexoes = Conexoes();
        var nota = Nota(1);
        (nota.Emitente.Cnpj, nota.Emitente.Cpf) = (null, "52998224725");
        nota.Identificacao.Serie = 920;
        using var certificado = X509CertificateLoader.LoadPkcs12(
            new CertificadoTeste { Cpf = "52998224725" }.GerarPfx("teste"), "teste");

        var resultado = await Criar(conexoes, certificado: certificado).ExecutarAsync(new Autorizacao(nota), Ct);

        var autorizada = Autorizada(resultado);
        Assert.EndsWith("|3|2", QrCode(autorizada.Xml!), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ANFeComODanfeSimplificadoTipo2EAutorizadaComOQrCode()
    {
        using var conexoes = Conexoes();
        var nota = Nota(1);
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;

        var resultado = await Criar(conexoes, documento: Documento.NFe).ExecutarAsync(new Autorizacao(nota), Ct);

        var autorizada = Autorizada(resultado);
        Assert.Equal(Documento.NFe, autorizada.Chave!.Modelo);
        Assert.EndsWith("|3|2", QrCode(autorizada.Xml!), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OLoteDeDuasNFCeEAutorizadoPelaConsultaDoRecibo()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);

        var lote = await dfe.ExecutarAsync(new Autorizacao(Nota(1), Nota(2)), Ct);
        Assert.Equal(103, lote.CStat);

        var consulta = await dfe.ExecutarAsync(
            new ConsultaRecibo(lote.Retorno!.Recibo!) { Xmls = [.. lote.Retorno.Notas.Select(n => n.Xml!)] }, Ct);

        Assert.Equal(104, consulta.CStat);
        Assert.Empty(consulta.Problemas);
        Assert.All(consulta.Retorno!.Notas, n =>
        {
            Assert.True(n.Autorizada);
            Assert.Empty(SchemasNFe400.NotaProcessada.Validar(n.XmlProcessado!));
        });
    }

    [Fact]
    public async Task ANFCePreparadaEAutorizadaEOReenvioEDuplicidade()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var preparado = dfe.Preparar(Nota(1));
        Assert.True(preparado.Preparado);

        var primeira = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);
        var reenvio = await dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        var autorizada = Autorizada(primeira);
        Assert.Equal(preparado.Xml, autorizada.Xml);
        Assert.Equal(204, Assert.Single(reenvio.Retorno!.Notas).Protocolo!.Status);
    }

    [Fact]
    public async Task AQuedaDepoisDeAutorizarERecuperadaComONfeProc()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var preparado = dfe.Preparar(Nota(1));
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => dfe.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct));
        var xml = Autorizacao.NotasEnviadas(falha.Rastro.XmlPedido!)[0];
        var recuperacao = await dfe.ExecutarAsync(new RecuperacaoAutorizacao(xml), Ct);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(100, recuperacao.CStat);
        var nota = Assert.Single(recuperacao.Retorno!.Notas);
        Assert.True(nota.Autorizada);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        Assert.Contains(preparado.Xml!, nota.XmlProcessado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AConsultaDoProtocoloAchaANFCeAutorizada()
    {
        using var conexoes = Conexoes();
        var dfe = Criar(conexoes);
        var autorizada = Autorizada(await dfe.ExecutarAsync(new Autorizacao(Nota(1)), Ct));

        var achada = await dfe.ExecutarAsync(new ConsultaProtocolo(autorizada.Chave!), Ct);

        Assert.True(achada.Processado);
        Assert.Empty(achada.Problemas);
        Assert.True(achada.Retorno.Autorizada);
        Assert.Equal(autorizada.Protocolo!.Numero, achada.Retorno.Protocolo!.Numero);
    }

    // A nota autorizada (100) do resultado de uma nota só, com o nfeProc válido.
    private static ResultadoNota Autorizada(ResultadoWS<RetornoAutorizacao> resultado)
    {
        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(104, resultado.CStat);
        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.True(nota.Autorizada, $"cStat {nota.Protocolo?.Status}: {nota.Protocolo?.Motivo}");
        Assert.Equal(100, nota.Protocolo!.Status);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        return nota;
    }

    private static string QrCode(string nota) => XElement.Parse(nota).Element(XName.Get("infNFeSupl", "http://www.portalfiscal.inf.br/nfe"))!
        .Element(XName.Get("qrCode", "http://www.portalfiscal.inf.br/nfe"))!.Value;

    // A NFC-e de teste emitida agora, com o número do caso e o cNF fixo.
    private static NotaFiscal Nota(int numero)
    {
        var nota = NotasTeste.NFCe();
        var agora = DateTimeOffset.UtcNow.AddMinutes(-1).ToOffset(TimeSpan.FromHours(-3));
        agora = agora.AddTicks(-(agora.Ticks % TimeSpan.TicksPerSecond));
        nota.Identificacao.Numero = numero;
        nota.Identificacao.CodigoNumerico = 12345678;
        nota.Identificacao.Emissao = agora;
        return nota;
    }

    private ConexoesSefaz Conexoes() =>
        new(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };

    private DFe Criar(ConexoesSefaz conexoes, ConfiguracaoNFCe? configuracao = null, Documento documento = Documento.NFCe,
        X509Certificate2? certificado = null) =>
        DFe.Criar(new LeiauteNFe400(), documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = certificado ?? Certificado,
            Conexoes = conexoes,
            Documento = configuracao,
            Urls = Urls(documento),
        });

    // As URLs do emulador para os serviços que o leiaute declara.
    private List<EnderecoServico> Urls(Documento documento)
    {
        var servicos = DeclaracaoLeiaute.Criar(new LeiauteNFe400(), documento).Servicos.Select(s => s.Nome).ToHashSet(StringComparer.Ordinal);
        var urls = new List<EnderecoServico>();
        foreach (var url in _emulador.Urls)
        {
            if (servicos.Contains(url.Servico) && Destinos.TentarLerSigla(url.Destino, out var destino))
                urls.Add(new EnderecoServico(destino, Ambiente.Homologacao, url.Servico, url.Url));
        }
        return urls;
    }
}
