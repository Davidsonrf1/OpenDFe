using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using OpenDFe.Impressao;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;

namespace OpenDFe.Testes;

/// <summary>
/// O DANFCE de uma NFC-e produzida pela lib inteira (etapa 7.2): a nota do modelo, o <c>Preparar</c> e a autorização contra
/// a SEFAZ emulada, o <c>nfeProc</c> lido de volta e impresso; e a NFC-e offline (etapa 5.2), preparada, lida e impressa sem
/// protocolo, e depois autorizada e impressa de novo. É o teste de que o QR Code que a 5.1 e a 5.2 geram é o que o DANFCE
/// imprime.
/// </summary>
public sealed class DanfceEmuladoTestes : IClassFixture<EmuladorFixture>
{
    private const string CscPadrao = "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO";
    private static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";

    private static readonly X509Certificate2 Certificado =
        CertificadoDigital.AbrirPfx(new CertificadoTeste { Cnpj = NotasTeste.CnpjEmitente }.GerarPfx("teste"), "teste");

    private static readonly LeiauteNFe400 Leiaute = new();

    private readonly Emulador _emulador;

    public DanfceEmuladoTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
        _emulador.Codigos.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ANFCeAutorizadaPelaLibEImpressaComOQrCodeDela(bool versao2)
    {
        using var conexoes = Conexoes();
        var configuracao = versao2 ? new ConfiguracaoNFCe(new CodigoSeguranca(1, CscPadrao)) : null;
        var nota = Nota(1, horasAtras: 0);
        nota.Destinatario = new Destinatario { Cpf = "52998224725", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte };

        var resultado = await Normal(conexoes, configuracao).ExecutarAsync(new Autorizacao(nota), Ct);

        Assert.True(resultado.Processado, string.Join("; ", resultado.Problemas.Select(p => $"{p.Codigo} {p.Mensagem}")));
        var autorizada = Assert.Single(resultado.Retorno.Notas);
        Assert.True(autorizada.Autorizada, $"cStat {autorizada.Protocolo?.Status}: {autorizada.Protocolo?.Motivo}");
        var lida = Leiaute.Ler(autorizada.XmlProcessado!);
        Assert.Empty(lida.Problemas);

        var documento = Leiaute.Danfce(lida);

        // O QR Code do DANFCE é o que a lib escreveu na nota, byte a byte, e o que o emulador aceitou.
        var qrXml = XElement.Parse(autorizada.Xml!).Descendants(Nfe + "qrCode").Single().Value;
        var qr = Assert.Single(documento.Elementos.OfType<QrCodeImpressao>());
        Assert.Equal(qrXml, qr.Texto);
        Assert.Equal(lida.Nota!.Suplementares!.QrCode, qr.Texto);
        Assert.Contains(versao2 ? "|2|2|1|" : "|3|2", qr.Texto, StringComparison.Ordinal);

        var textos = documento.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
        Assert.Contains(textos, t => t == "Protocolo de autorização: " + autorizada.Protocolo!.Numero);
        Assert.Contains(textos, t => t.Replace(" ", string.Empty, StringComparison.Ordinal) == autorizada.Chave!.ToString());
        Assert.Contains(textos, t => t == lida.Nota.Suplementares.UrlChave);
        Assert.Contains(textos, t => t == "CONSUMIDOR CPF: 529.982.247-25");
        Assert.Contains(textos, t => t.StartsWith("EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO", StringComparison.Ordinal));
        Assert.DoesNotContain(textos, t => t.Contains("CONTINGÊNCIA", StringComparison.Ordinal));

        var total = Assert.Single(documento.Elementos.OfType<LinhaImpressao>(), l => l.Celulas[0].Texto == "Valor total R$");
        Assert.Equal("50,00", total.Celulas[1].Texto);
        Assert.Equal(autorizada.Chave!.ToString(), lida.Chave!.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ANFCeOfflineSaiComALegendaEOMesmoQrCodeDepoisDeAutorizadaSaiComOProtocolo(bool versao2)
    {
        using var conexoes = Conexoes();
        var configuracao = versao2 ? new ConfiguracaoNFCe(new CodigoSeguranca(1, CscPadrao)) : null;
        var preparado = Offline(conexoes, configuracao).Preparar(Nota(1, horasAtras: 2));
        Assert.True(preparado.Preparado);

        // Sem protocolo: o NFe assinado, lido, impresso nas duas vias.
        var lida = Leiaute.Ler(preparado.Xml!);
        Assert.Null(lida.Protocolo);
        var qrXml = XElement.Parse(preparado.Xml!).Descendants(Nfe + "qrCode").Single().Value;
        foreach (var via in new[] { ViaDanfce.Consumidor, ViaDanfce.Estabelecimento })
        {
            var documento = Leiaute.Danfce(lida, new OpcoesDanfce { Via = via });
            Assert.Equal(qrXml, Assert.Single(documento.Elementos.OfType<QrCodeImpressao>()).Texto);
            var textos = documento.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
            Assert.Equal(2, textos.Count(t => t == "EMITIDA EM CONTINGÊNCIA"));
            Assert.Equal(2, textos.Count(t => t == "Pendente de autorização"));
            Assert.DoesNotContain(textos, t => t.StartsWith("Protocolo de autorização", StringComparison.Ordinal));
            Assert.Contains(textos, t => t == (via == ViaDanfce.Consumidor ? "Via do Consumidor" : "Via do Estabelecimento"));
        }

        // Depois de transmitida e autorizada, o nfeProc imprime o protocolo e a legenda some; o QR Code é o mesmo.
        var resultado = await Normal(conexoes).ExecutarAsync(new Autorizacao(preparado.Xml!), Ct);
        var autorizada = Assert.Single(resultado.Retorno!.Notas);
        Assert.True(autorizada.Autorizada, $"cStat {autorizada.Protocolo?.Status}: {autorizada.Protocolo?.Motivo}");
        var processada = Leiaute.Ler(autorizada.XmlProcessado!);
        var impresso = Leiaute.Danfce(processada);
        var impressos = impresso.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();

        Assert.Equal(qrXml, Assert.Single(impresso.Elementos.OfType<QrCodeImpressao>()).Texto);
        Assert.Contains(impressos, t => t == "Protocolo de autorização: " + autorizada.Protocolo!.Numero);
        Assert.DoesNotContain(impressos, t => t.Contains("CONTINGÊNCIA", StringComparison.Ordinal));
    }

    // A NFC-e de teste emitida há `horasAtras` horas, com o número do caso e o cNF fixo.
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
    private DFe Offline(ConexoesSefaz conexoes, ConfiguracaoNFCe? configuracao) =>
        DFe.Criar(Leiaute, Documento.NFCe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Emissao = TipoEmissao.ContingenciaOffline,
            Contingencia = new Contingencia(DateTimeOffset.UtcNow.AddHours(-3).ToOffset(TimeSpan.FromHours(-3)), "Sem comunicacao com a SEFAZ"),
            Documento = configuracao,
            Urls = Urls(),
        });

    private DFe Normal(ConexoesSefaz conexoes, ConfiguracaoNFCe? configuracao = null) =>
        DFe.Criar(Leiaute, Documento.NFCe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = Certificado,
            Conexoes = conexoes,
            Documento = configuracao,
            Urls = Urls(),
        });

    // As URLs do emulador para os serviços que o leiaute declara.
    private List<EnderecoServico> Urls()
    {
        var servicos = DeclaracaoLeiaute.Criar(Leiaute, Documento.NFCe).Servicos.Select(s => s.Nome).ToHashSet(StringComparer.Ordinal);
        var urls = new List<EnderecoServico>();
        foreach (var url in _emulador.Urls)
        {
            if (servicos.Contains(url.Servico) && Destinos.TentarLerSigla(url.Destino, out var destino))
                urls.Add(new EnderecoServico(destino, Ambiente.Homologacao, url.Servico, url.Url));
        }
        return urls;
    }
}
