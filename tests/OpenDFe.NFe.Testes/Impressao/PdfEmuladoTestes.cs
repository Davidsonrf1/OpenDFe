using System.Xml.Linq;
using OpenDFe.Impressao;
using OpenDFe.Impressao.Pdf;
using OpenDFe.Impressao.Pdf.Testes;
using OpenDFe.NFe;
using OpenDFe.SefazEmulada;
using Autorizacao = OpenDFe.NFe.Autorizacao;

namespace OpenDFe.Testes;

/// <summary>
/// O PDF de uma nota produzida pela lib inteira (etapa 7.4): a nota do modelo, a autorização contra a SEFAZ emulada, o
/// <c>nfeProc</c> lido de volta, o DANFCE ou o DANFE montado e o PDF gerado. O QR Code do PDF é o <c>qrCode</c> do XML autorizado
/// (a matriz do vetor igual à do QRCoder para o texto da nota) e o código de barras é o da chave da nota.
/// </summary>
public sealed class PdfEmuladoTestes : IClassFixture<EmuladorFixture>
{
    private const string CscPadrao = "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO";
    private const double MmPt = 72 / 25.4;
    private static readonly XNamespace Nfe = "http://www.portalfiscal.inf.br/nfe";
    private static readonly LeiauteNFe400 Leiaute = new();

    private readonly Emulador _emulador;

    public PdfEmuladoTestes(EmuladorFixture fixture)
    {
        _emulador = fixture.Emulador;
        _emulador.Controle.Limpar();
        _emulador.Repositorio.Limpar();
        _emulador.Codigos.Limpar();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DFe CriarNfce(ConexoesSefaz conexoes, ConfiguracaoNFCe? configuracao = null, TipoEmissao emissao = TipoEmissao.Normal) =>
        DFe.Criar(Leiaute, Documento.NFCe, new ConfiguracaoDFe
        {
            Ambiente = Ambiente.Homologacao,
            UF = UF.MG,
            Certificado = ApoioEmulado.Certificado,
            Conexoes = conexoes,
            Emissao = emissao,
            Contingencia = emissao == TipoEmissao.Normal
                ? null
                : new Contingencia(DateTimeOffset.UtcNow.AddHours(-3).ToOffset(TimeSpan.FromHours(-3)), "Sem comunicacao com a SEFAZ"),
            Documento = configuracao,
            Urls = ApoioEmulado.Urls(_emulador, Documento.NFCe),
        });

    private static void ConferirOQrCode(string qrXml, PdfLeitor pdf)
    {
        var esperada = PdfVetores.MatrizQrCode(qrXml);
        var (matriz, caixa, modulo) = PdfVetores.MatrizQrCodePdf(pdf, 1, esperada.GetLength(0));
        Assert.True(esperada.Cast<bool>().SequenceEqual(matriz.Cast<bool>()), "o QR Code do PDF não é o qrCode do XML autorizado");
        // Lado total de 30 mm com a margem de silêncio dentro: 3 mm (10% de 30 mm) e 4 módulos, no mínimo.
        var margemMm = (30 - caixa.Largura / MmPt) / 2;
        Assert.True(margemMm >= 3 - 0.01 && margemMm >= 4 * modulo / MmPt - 0.01, $"margem de silêncio de {margemMm:F2} mm");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ANFCeAutorizadaPelaLibViraPdfComOQrCodeDoXml(bool versao2)
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var configuracao = versao2 ? new ConfiguracaoNFCe(new CodigoSeguranca(1, CscPadrao)) : null;

        var autorizada = await ApoioEmulado.Autorizar(CriarNfce(conexoes, configuracao), Documento.NFCe, 1,
            ajustar: nota => nota.Destinatario = new Destinatario { Cpf = "52998224725", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte });
        var lida = Leiaute.Ler(autorizada.XmlProcessado!);
        Assert.Empty(lida.Problemas);
        var qrXml = XElement.Parse(autorizada.Xml!).Descendants(Nfe + "qrCode").Single().Value;
        Assert.Equal(lida.Nota!.Suplementares!.QrCode, qrXml);

        var bytes = RenderizadorPdf.Gerar(Leiaute.Danfce(lida));
        AmostrasPdf.Gravar(versao2 ? "emulado-nfce-v2" : "emulado-nfce-v3", bytes);

        using var pdf = new PdfLeitor(bytes);
        var texto = pdf.TextoCompacto();
        Assert.Equal(1, pdf.Paginas);
        ConferirOQrCode(qrXml, pdf);
        Assert.Contains(versao2 ? "|2|2|1|" : "|3|2", qrXml, StringComparison.Ordinal);
        Assert.Contains(autorizada.Chave!.ToString(), texto, StringComparison.Ordinal);
        Assert.Contains("Protocolodeautorização:" + autorizada.Protocolo!.Numero, texto, StringComparison.Ordinal);
        Assert.Contains("CONSUMIDORCPF:529.982.247-25", texto, StringComparison.Ordinal);
        Assert.Contains("EMITIDAEMAMBIENTEDEHOMOLOGAÇÃO", texto, StringComparison.Ordinal);
        Assert.Contains("ValortotalR$50,00", texto, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ANFCeOfflineSaiEmDuasViasSemProtocoloEDepoisDeAutorizadaSaiComOProtocolo()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var offline = CriarNfce(conexoes, emissao: TipoEmissao.ContingenciaOffline);
        var nota = ApoioEmulado.Nota(Documento.NFCe, 1, DateTimeOffset.UtcNow.AddHours(-2));
        var preparada = offline.Preparar(nota);
        Assert.True(preparada.Preparado);
        var qrXml = XElement.Parse(preparada.Xml!).Descendants(Nfe + "qrCode").Single().Value;

        // Sem protocolo, nas duas vias: a legenda nos dois lugares, a via, o mesmo QR Code.
        var lida = Leiaute.Ler(preparada.Xml!);
        foreach (var (via, rotulo) in new[] { (ViaDanfce.Consumidor, "ViadoConsumidor"), (ViaDanfce.Estabelecimento, "ViadoEstabelecimento") })
        {
            using var pdf = new PdfLeitor(RenderizadorPdf.Gerar(Leiaute.Danfce(lida, new OpcoesDanfce { Via = via })));
            var texto = pdf.TextoCompacto();
            Assert.Equal(2, Contar(texto, "EMITIDAEMCONTINGÊNCIA"));
            Assert.Contains(rotulo, texto, StringComparison.Ordinal);
            Assert.DoesNotContain("Protocolodeautorização", texto, StringComparison.Ordinal);
            ConferirOQrCode(qrXml, pdf);
        }

        // Depois de transmitida e autorizada: o protocolo entra, a legenda sai, e o QR Code é o mesmo.
        var resultado = await CriarNfce(conexoes).ExecutarAsync(new Autorizacao(preparada.Xml!), Ct);
        var autorizada = Assert.Single(resultado.Retorno!.Notas);
        Assert.True(autorizada.Autorizada, $"cStat {autorizada.Protocolo?.Status}: {autorizada.Protocolo?.Motivo}");
        using var depois = new PdfLeitor(RenderizadorPdf.Gerar(Leiaute.Danfce(Leiaute.Ler(autorizada.XmlProcessado!))));
        Assert.Contains("Protocolodeautorização:" + autorizada.Protocolo!.Numero, depois.TextoCompacto(), StringComparison.Ordinal);
        Assert.DoesNotContain("CONTINGÊNCIA", depois.TextoCompacto(), StringComparison.Ordinal);
        ConferirOQrCode(qrXml, depois);
    }

    [Fact]
    public async Task ANFeAutorizadaPelaLibViraPdfComOCodigoDeBarrasDaChaveEAMarcaDeHomologacao()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var autorizada = await ApoioEmulado.Autorizar(dfe, Documento.NFe, 1);
        var lida = Leiaute.Ler(autorizada.XmlProcessado!);
        Assert.Empty(lida.Problemas);

        var bytes = RenderizadorPdf.Gerar(Leiaute.Danfe(lida));
        AmostrasPdf.Gravar("emulado-nfe", bytes);

        using var pdf = new PdfLeitor(bytes);
        var chave = autorizada.Chave!.ToString();
        Assert.Equal(595.28, pdf.Pagina(1).Width, 0.5);
        Assert.Equal(841.89, pdf.Pagina(1).Height, 0.5);
        for (var pagina = 1; pagina <= pdf.Paginas; pagina++)
        {
            // A chave do XML nas barras (posição e largura de cada uma) e nos 11 blocos do texto; a folha e a marca d'água.
            PdfVetores.ConferirBarras(pdf, pagina, PdfVetores.CaminhoBarras(pdf, pagina, chave), chave);
            Assert.Contains(chave, pdf.TextoCompacto(pagina), StringComparison.Ordinal);
            Assert.Contains($"FOLHA{pagina}/{pdf.Paginas}", pdf.TextoCompacto(pagina), StringComparison.Ordinal);
            Assert.Equal("SEMVALORFISCAL", string.Concat(pdf.Letras(pagina).Where(l => l.Tamanho > 20).Select(l => l.Valor)).Replace(" ", string.Empty, StringComparison.Ordinal));
        }

        Assert.Contains(autorizada.Protocolo!.Numero!, pdf.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains("VALORTOTALDANOTA", pdf.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains(FormatadorImpressao.Dinheiro(lida.Nota!.Total.Icms.ValorNota!.Value), pdf.TextoCompacto(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OSimplificadoTipo2AutorizadoViraPdfDeBobinaComOQrCodeDoXml()
    {
        using var conexoes = ApoioEmulado.Conexoes(_emulador);
        var dfe = ApoioEmulado.Criar(_emulador, conexoes);
        var autorizada = await ApoioEmulado.Autorizar(dfe, Documento.NFCe, 1,
            ajustar: nota => nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2);
        var lida = Leiaute.Ler(autorizada.XmlProcessado!);
        var qrXml = XElement.Parse(autorizada.Xml!).Descendants(Nfe + "qrCode").Single().Value;

        using var pdf = new PdfLeitor(RenderizadorPdf.Gerar(Leiaute.Danfe(lida)));

        Assert.Equal(80 * MmPt, pdf.Pagina(1).Width, 0.5);
        ConferirOQrCode(qrXml, pdf);
        Assert.Contains("DANFESimplificadoTipo2", pdf.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains(autorizada.Chave!.ToString(), pdf.TextoCompacto(), StringComparison.Ordinal);
    }

    [Fact]
    public void ANFeDeVerdadeDoErpCoreViraPdfSemExcecao()
    {
        var xml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Exemplos", "erpcore-nfeproc-entrada.xml"));
        var lida = Leiaute.Ler(xml);
        var documento = Leiaute.Danfe(lida);

        var bytes = RenderizadorPdf.Gerar(documento);
        AmostrasPdf.Gravar("erpcore-nfe", bytes);

        using var pdf = new PdfLeitor(bytes);
        Assert.True(pdf.Paginas >= 1);
        Assert.Contains(lida.Chave!.ToString(), pdf.TextoCompacto(), StringComparison.Ordinal);
        PdfVetores.ConferirBarras(pdf, 1, PdfVetores.CaminhoBarras(pdf, 1, lida.Chave.ToString()), lida.Chave.ToString());
    }

    private static int Contar(string texto, string trecho)
    {
        var total = 0;
        for (var i = texto.IndexOf(trecho, StringComparison.Ordinal); i >= 0; i = texto.IndexOf(trecho, i + trecho.Length, StringComparison.Ordinal))
            total++;
        return total;
    }
}
