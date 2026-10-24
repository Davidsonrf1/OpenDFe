using System.Xml.Linq;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// A NFC-e nas operações da 4.4 e da 4.5 (etapa 5.1, CONTRATOS.md 16, decisão 11), pelo ExecutarAsync com o handler que
// responde conforme o pedido: a síncrona, o lote, a consulta do recibo, a recuperação, o Preparar e a autorização do XML
// preparado, e o QR Code que vai na mensagem. Contra a SEFAZ emulada, em NFCeEmuladaTestes.
public partial class AutorizacaoTestes
{
    private const string UrlQrCodeMG = "https://portalsped.fazenda.mg.gov.br/portalnfce/sistema/qrcode.xhtml";

    // A NFC-e de teste com o número e o cNF fixos.
    private static NotaFiscal NotaNFCe(int numero = 1)
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.Numero = numero;
        nota.Identificacao.CodigoNumerico = 12345678;
        return nota;
    }

    // A NFC-e escrita e assinada fora do núcleo.
    private static string AssinadaNFCe(NotaFiscal nota) =>
        NotasTeste.Assinar(EscritorNFe.Escrever(nota, NotasTeste.Nucleo(Documento.NFCe)));

    private static string QrCode(string nota) => XElement.Parse(nota).Element(XName.Get("infNFeSupl", NamespaceNFe))!
        .Element(XName.Get("qrCode", NamespaceNFe))!.Value;

    [Fact]
    public async Task UmaNFCeVaiNaSincronaComOQrCodeEVoltaComONfeProc()
    {
        var (resultado, envio) = await Executar(new Autorizacao(NotaNFCe()), Sincrona(100), Documento.NFCe);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.Equal((104, "Lote processado"), (resultado.CStat, resultado.Motivo));
        Assert.Equal(Destino.MG, resultado.Rastro.Autorizador);
        Assert.Equal("1", XElement.Parse(envio!.Mensagem).Element(XName.Get("indSinc", NamespaceNFe))!.Value);
        var enviada = Enviadas(envio.Mensagem).Single();
        Assert.StartsWith($"{UrlQrCodeMG}?p={Chave(enviada)}|3|2", QrCode(enviada), StringComparison.Ordinal);
        Assert.Empty(SchemasNFe400.Lote.Validar(envio.Mensagem));

        var nota = Assert.Single(resultado.Retorno.Notas);
        Assert.True(nota.Autorizada);
        Assert.Equal(Documento.NFCe, nota.Chave!.Modelo);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        Assert.Contains(enviada, nota.XmlProcessado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OLoteDeDuasNFCeVaiComORecibo()
    {
        var (resultado, envio) = await Executar(new Autorizacao(NotaNFCe(1), NotaNFCe(2)), Recebido, Documento.NFCe);

        Assert.True(resultado.Processado);
        Assert.Equal(103, resultado.CStat);
        Assert.Equal("311000000000001", resultado.Retorno!.Recibo);
        Assert.Equal("0", XElement.Parse(envio!.Mensagem).Element(XName.Get("indSinc", NamespaceNFe))!.Value);
        Assert.Equal(2, resultado.Retorno.Notas.Count);
        Assert.All(Enviadas(envio.Mensagem), nota => Assert.Contains("|3|2", QrCode(nota), StringComparison.Ordinal));
    }

    [Fact]
    public async Task AConsultaDoReciboDaNFCeTrazOsProtocolosEONfeProc()
    {
        var notas = new[] { AssinadaNFCe(NotaNFCe(1)), AssinadaNFCe(NotaNFCe(2)) };

        var (resultado, _) = await Executar(new ConsultaRecibo("311000000000001") { Xmls = notas }, _ =>
            RetornoRecibo(104, "Lote processado", Protocolo(Chave(notas[0]), Digest(notas[0]), 100), Protocolo(Chave(notas[1]), Digest(notas[1]), 100)),
            Documento.NFCe);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        Assert.All(resultado.Retorno!.Notas, n =>
        {
            Assert.True(n.Autorizada);
            Assert.Empty(SchemasNFe400.NotaProcessada.Validar(n.XmlProcessado!));
        });
    }

    [Fact]
    public async Task ARecuperacaoDaNFCeAutorizadaTrazONfeProc()
    {
        var xml = AssinadaNFCe(NotaNFCe());
        var chave = Chave(xml);

        var (resultado, _) = await Executar(new RecuperacaoAutorizacao(xml),
            _ => Situacao(100, "Autorizado o uso da NF-e", chave, Protocolo(chave, Digest(xml), 100)), Documento.NFCe);

        Assert.True(resultado.Processado);
        Assert.Empty(resultado.Problemas);
        var nota = Assert.Single(resultado.Retorno!.Notas);
        Assert.True(nota.Autorizada);
        Assert.Empty(SchemasNFe400.NotaProcessada.Validar(nota.XmlProcessado!));
        Assert.Contains(xml, nota.XmlProcessado, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AChaveDeNFeNoNucleoDeNFCeEErroNaRecuperacao()
    {
        var xml = Assinada(Nota());   // uma NF-e (modelo 55)

        var (resultado, envio) = await Executar(new RecuperacaoAutorizacao(xml), _ => throw new InvalidOperationException(), Documento.NFCe);

        Assert.False(resultado.Processado);
        Assert.Null(envio);
        Assert.Contains(resultado.Problemas, p => p.Codigo == "NFE-CHAVE");
    }

    [Fact]
    public async Task OPrepararDaNFCeDaOXmlAssinadoComOQrCodeEAAutorizacaoOUsaComOsMesmosBytes()
    {
        using var conexoes = new ConexoesSefaz();
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, Configuracao(conexoes));

        var preparado = dfe.Preparar(NotaNFCe());

        Assert.True(preparado.Preparado);
        Assert.Empty(preparado.Problemas);
        Assert.Empty(SchemasNFe400.Nota.Validar(preparado.Xml!));
        Assert.StartsWith($"{UrlQrCodeMG}?p={preparado.Chave}|3|2", QrCode(preparado.Xml!), StringComparison.Ordinal);
        Assert.Empty(AssinaturaDigital.Verificar(preparado.Xml!, "NFe" + preparado.Chave, NotasTeste.CnpjEmitente));

        var (resultado, envio) = await Executar(new Autorizacao(preparado.Xml!), Sincrona(100), Documento.NFCe);

        Assert.True(resultado.Processado);
        Assert.Contains(preparado.Xml!, envio!.Mensagem, StringComparison.Ordinal);
        Assert.True(Assert.Single(resultado.Retorno!.Notas).Autorizada);
    }

    [Fact]
    public void ONFCeNumaUFSemUrlNaTabelaNaoVaiSemAUrlConfigurada()
    {
        using var conexoes = new ConexoesSefaz();
        var configuracao = Configuracao(conexoes);
        var dfe = DFe.Criar(new LeiauteNFe400(), Documento.NFCe, new ConfiguracaoDFe
        {
            Ambiente = configuracao.Ambiente, UF = UF.SP, Certificado = configuracao.Certificado, Conexoes = conexoes, Relogio = configuracao.Relogio,
        });

        var preparado = dfe.Preparar(NotasTeste.NFCe());

        Assert.False(preparado.Preparado);
        Assert.Contains(preparado.Problemas, p => p.Codigo == "NFE-QRCODE" && p.Severidade == Severidade.Erro);
    }

    [Fact]
    public void ANFCeAceitaAConfiguracaoDoDocumentoEANFeTambem()
    {
        using var conexoes = new ConexoesSefaz();
        var basico = Configuracao(conexoes);
        ConfiguracaoDFe Configurar(ConfiguracaoDocumento documento) => new()
        {
            Ambiente = basico.Ambiente, UF = basico.UF, Certificado = basico.Certificado, Conexoes = conexoes, Relogio = basico.Relogio,
            Documento = documento,
        };

        Assert.NotNull(DFe.Criar(new LeiauteNFe400(), Documento.NFCe, Configurar(new ConfiguracaoNFCe())));
        Assert.NotNull(DFe.Criar(new LeiauteNFe400(), Documento.NFe, Configurar(new ConfiguracaoNFCe())));
        Assert.Throws<ArgumentException>(() => DFe.Criar(new LeiauteNFe400(), Documento.NFCe, Configurar(new OutraConfiguracao())));
    }

    private sealed class OutraConfiguracao : ConfiguracaoDocumento;
}
