using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;

namespace OpenDFe.Testes;

/// <summary>
/// A NFC-e emitida offline pela lib inteira e transmitida depois, contra a SEFAZ emulada (etapa 5.2): o núcleo com
/// <c>tpEmis</c> 9 prepara a nota (versão 2 e versão 3), e o núcleo normal a transmite, em síncrona ou em lote, dentro e
/// fora das 24 horas (100 e 150), a NF-e com o DANFE Simplificado Tipo 2, o CSC errado e revogado, a duplicidade e a
/// recuperação, e a leitura do <c>nfeProc</c> de volta.
/// </summary>
public sealed class NFCeContingenciaEmuladaTestes : IClassFixture<EmuladorFixture>
{
    private const string CscPadrao = "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO";
    private const string RaizEmitente = "11222333";

    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = NotasTeste.CnpjEmitente }.GerarPfx("teste"), "teste");

    private readonly Emulador _emulador;

    public NFCeContingenciaEmuladaTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
        _emulador.Codigos.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ConfiguracaoNFCe? Configuracao(bool versao2, string csc = CscPadrao) =>
        versao2 ? new ConfiguracaoNFCe(new CodigoSeguranca(1, csc)) : null;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AOfflinePreparadaHa2HorasETransmitidaDepoisEAutorizada(bool versao2)
    {
        using var conexoes = Conexoes();
        var preparado = Offline(conexoes, Documento.NFCe, Configuracao(versao2)).Preparar(Nota(1, horasAtras: 2));
        Assert.True(preparado.Preparado);

        var resultado = await Normal(conexoes).ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        var nota = Autorizada(resultado, 100);
        Assert.Equal(preparado.Xml, nota.Xml);
        Assert.Equal(TipoEmissao.ContingenciaOffline, nota.Chave!.TipoEmissao);
        Assert.Contains(versao2 ? "|2|2|" : "|3|2|", QrCode(nota.Xml!), StringComparison.Ordinal);
        Assert.Contains(_emulador.Repositorio.Notas(), n => n.Chave == nota.Chave.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AOfflineTransmitidaDepoisDe30HorasEAutorizadaForaDePrazo(bool versao2)
    {
        using var conexoes = Conexoes();
        var preparado = Offline(conexoes, Documento.NFCe, Configuracao(versao2)).Preparar(Nota(1, horasAtras: 30));

        var resultado = await Normal(conexoes).ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        var nota = Autorizada(resultado, 150);
        Assert.True(nota.Autorizada);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TresNotasOfflineVaoNumLoteEOReciboAutoriza(bool versao2)
    {
        using var conexoes = Conexoes();
        var offline = Offline(conexoes, Documento.NFCe, Configuracao(versao2));
        var xmls = Enumerable.Range(1, 3).Select(i => offline.Preparar(Nota(i, horasAtras: 1)).Xml!).ToArray();
        var normal = Normal(conexoes);

        var lote = await normal.ExecutarAsync(new Autorizacao(xmls), Ct);
        Assert.Equal(103, lote.CStat);
        var consulta = await normal.ExecutarAsync(new ConsultaRecibo(lote.Retorno!.Recibo!) { Xmls = xmls }, Ct);

        Assert.Equal(104, consulta.CStat);
        Assert.Empty(consulta.Problemas);
        Assert.Equal(3, consulta.Retorno!.Notas.Count);
        Assert.All(consulta.Retorno.Notas, n =>
        {
            Assert.Equal(100, n.Protocolo!.Status);
            Assert.Empty(SchemasNFe400.NotaProcessada.Validar(n.XmlProcessado!));
        });
    }

    [Fact]
    public async Task ANFeComTpImp6OfflineNaVersao3EAutorizada()
    {
        using var conexoes = Conexoes();
        var nota = Nota(1, horasAtras: 2);
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
        var preparado = Offline(conexoes, Documento.NFe).Preparar(nota);
        Assert.True(preparado.Preparado);

        var resultado = await Normal(conexoes, Documento.NFe).ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        var autorizada = Autorizada(resultado, 100);
        Assert.Equal(Documento.NFe, autorizada.Chave!.Modelo);
        Assert.Equal("3", QrCode(autorizada.Xml!).Split("?p=")[1].Split('|')[1]);
    }

    [Fact]
    public async Task OCscComOutroCodigoNaVersao2OfflineE464()
    {
        using var conexoes = Conexoes();
        var preparado = Offline(conexoes, Documento.NFCe, Configuracao(true, "CSC-QUE-A-SEFAZ-NAO-TEM-0001")).Preparar(Nota(1, horasAtras: 2));

        var resultado = await Normal(conexoes).ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        Assert.Equal(464, Assert.Single(resultado.Retorno!.Notas).Protocolo!.Status);
    }

    [Fact]
    public async Task OCscRevogadoAntesDaEmissaoNaVersao2OfflineE463()
    {
        Assert.True(_emulador.Codigos.Revogar("MG", RaizEmitente, "1", DateTimeOffset.UtcNow.AddHours(-1)));
        using var conexoes = Conexoes();
        var preparado = Offline(conexoes, Documento.NFCe, Configuracao(true)).Preparar(Nota(1, horasAtras: 0));

        var resultado = await Normal(conexoes).ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        Assert.Equal(463, Assert.Single(resultado.Retorno!.Notas).Protocolo!.Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ATransmissaoDuplicadaE204ComADuplicidadeEARecuperacaoDaONfeProc(bool versao2)
    {
        using var conexoes = Conexoes();
        var preparado = Offline(conexoes, Documento.NFCe, Configuracao(versao2)).Preparar(Nota(1, horasAtras: 2));
        var normal = Normal(conexoes);

        var primeira = await normal.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);
        var segunda = await normal.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);

        Autorizada(primeira, 100);
        var duplicada = Assert.Single(segunda.Retorno!.Notas);
        Assert.Equal(204, duplicada.Protocolo!.Status);
        Assert.False(duplicada.Autorizada);
        Assert.NotNull(duplicada.Duplicidade);
        Assert.Equal(preparado.Chave, duplicada.Duplicidade!.Chave);

        var recuperada = await normal.ExecutarAsync(new RecuperacaoAutorizacao(preparado.Xml!), Ct);

        Assert.Equal(100, recuperada.CStat);
        var nota = Assert.Single(recuperada.Retorno!.Notas);
        Assert.True(nota.Autorizada);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        Assert.Contains(preparado.Xml!, nota.XmlProcessado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ONfeProcDaNotaOfflineELidoDeVoltaComOSuplementaresEOTpEmis9()
    {
        using var conexoes = Conexoes();
        var preparado = Offline(conexoes, Documento.NFCe, null).Preparar(Nota(1, horasAtras: 2));
        var resultado = await Normal(conexoes).ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);
        var autorizada = Autorizada(resultado, 100);

        var lida = new LeiauteNFe400().Ler(autorizada.XmlProcessado!);

        Assert.Empty(lida.Problemas);
        Assert.Equal(TipoEmissao.ContingenciaOffline, lida.Nota!.Identificacao.TipoEmissao);
        Assert.NotNull(lida.Nota.Identificacao.InicioContingencia);
        Assert.Equal("Sem comunicacao com a SEFAZ", lida.Nota.Identificacao.JustificativaContingencia);
        Assert.Equal(QrCode(preparado.Xml!), lida.Nota.Suplementares!.QrCode);
        Assert.Equal(100, lida.Protocolo!.Status);
    }

    [Fact]
    public async Task AQuedaDepoisDeAutorizarANotaOfflineERecuperada()
    {
        using var conexoes = Conexoes();
        var preparado = Offline(conexoes, Documento.NFCe, Configuracao(true)).Preparar(Nota(1, horasAtras: 2));
        var normal = Normal(conexoes);
        _emulador.Controle.Acrescentar(new PedidoFalha(TipoFalha.ConexaoDerrubada, "MG", Vezes: 1, Depois: true));

        var falha = await Assert.ThrowsAsync<FalhaTransmissaoException>(() => normal.ExecutarAsync(new Autorizacao(preparado.Xml!), Ct));
        var xml = Autorizacao.NotasEnviadas(falha.Rastro.XmlPedido!)[0];
        var recuperacao = await normal.ExecutarAsync(new RecuperacaoAutorizacao(xml), Ct);

        Assert.True(falha.PodeTerSidoProcessado);
        Assert.Equal(100, recuperacao.CStat);
        var nota = Assert.Single(recuperacao.Retorno!.Notas);
        Assert.True(nota.Autorizada);
        Assert.Contains(preparado.Xml!, nota.XmlProcessado, StringComparison.Ordinal);
    }

    // A nota autorizada (100 ou 150) do resultado de uma nota só, com o nfeProc válido.
    private static ResultadoNota Autorizada(ResultadoWS<RetornoAutorizacao> resultado, int status)
    {
        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal(104, resultado.CStat);
        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.True(nota.Autorizada, $"cStat {nota.Protocolo?.Status}: {nota.Protocolo?.Motivo}");
        Assert.Equal(status, nota.Protocolo!.Status);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        return nota;
    }

    private static string QrCode(string nota) => XElement.Parse(nota).Element(XName.Get("infNFeSupl", "http://www.portalfiscal.inf.br/nfe"))!
        .Element(XName.Get("qrCode", "http://www.portalfiscal.inf.br/nfe"))!.Value;

    // A NFC-e de teste emitida há <paramref name="horasAtras"/> horas, com o número do caso e o cNF fixo.
    private static NotaFiscal Nota(int numero, int horasAtras)
    {
        var nota = NotasTeste.NFCe();
        var emissao = DateTimeOffset.UtcNow.AddHours(-horasAtras).AddMinutes(-1).ToOffset(TimeSpan.FromHours(-3));
        emissao = emissao.AddTicks(-(emissao.Ticks % TimeSpan.TicksPerSecond));
        nota.Identificacao.Numero = numero;
        nota.Identificacao.CodigoNumerico = 12345678;
        nota.Identificacao.Emissao = emissao;
        return nota;
    }

    private ConexoesSefaz Conexoes() =>
        new(ConexaoTeste.Fabrica(_emulador.Autoridade)) { TempoResposta = TimeSpan.FromSeconds(30) };

    // O núcleo offline: só prepara (a rede não existe para ele); a Contingencia entra na nota.
    private DFe Offline(ConexoesSefaz conexoes, Documento documento, ConfiguracaoNFCe? configuracao = null) =>
        DFe.Criar(new LeiauteNFe400(), documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Emissao = TipoEmissao.ContingenciaOffline,
            Contingencia = new Contingencia(DateTimeOffset.UtcNow.AddHours(-3).ToOffset(TimeSpan.FromHours(-3)), "Sem comunicacao com a SEFAZ"),
            Documento = configuracao,
            Urls = Urls(documento),
        });

    // O núcleo normal: transmite o XML que o offline preparou.
    private DFe Normal(ConexoesSefaz conexoes, Documento documento = Documento.NFCe) =>
        DFe.Criar(new LeiauteNFe400(), documento, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
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
