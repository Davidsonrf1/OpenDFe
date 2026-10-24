using OpenDFe.NFe;

namespace OpenDFe.Testes;

// As três regras do QR Code da etapa 5.1 (REGRAS.md 15, CONTRATOS.md 16, decisão 10): a NF-e sem tpImp 6 com o
// Suplementares (393), a versão 2 com o emitente CPF (444) e a NF-e com tpImp 6 com a versão 2 (672). As outras do
// grupo ZX a montagem evita (EscritaQrCodeTestes) ou ficam para a SEFAZ.
public class RegrasQrCodeTestes
{
    private static readonly ConfiguracaoNFCe Versao2 = new(new CodigoSeguranca(1, "OPENDFE-SEFAZ-EMULADA-CSC-PADRAO"));

    private static List<Problema> Conferir(NotaFiscal nota, Documento documento, ConfiguracaoNFCe? configuracao = null) =>
        RegrasNFe400.Conferir(RegrasNFe400.Lista, nota, "", new ContextoRegra(
            new ValoresNucleo { Documento = documento, UF = UF.MG, Ambiente = Ambiente.Homologacao, NFCe = configuracao }, null, NotasTeste.Emissao));

    // O que cada regra acusa: o código, o caminho, o documento do núcleo, a configuração e a mudança na nota.
    internal static readonly Dictionary<string, (string Codigo, string Caminho, Documento Documento, ConfiguracaoNFCe? Configuracao, Action<NotaFiscal> Mudar)> Mudancas = new()
    {
        ["ZX01-10"] = ("393", "NFe/infNFeSupl", Documento.NFe, null,
            n => n.Suplementares = new InformacoesSuplementares { QrCode = "https://qr?p=1", UrlChave = "https://consulta" }),
        ["ZX02-222"] = ("444", "NFe/infNFe/emit/CPF", Documento.NFCe, Versao2,
            n => (n.Emitente.Cnpj, n.Emitente.Cpf) = (null, "52998224725")),
        ["ZX02-220"] = ("672", "NFe/infNFeSupl/qrCode", Documento.NFe, Versao2,
            n => n.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2),
    };

    public static TheoryData<string> Regras => new(Mudancas.Keys);

    [Theory]
    [MemberData(nameof(Regras))]
    public void ARegraAcusaANotaQueASefazRejeitaria(string identificador)
    {
        var (codigo, caminho, documento, configuracao, mudar) = Mudancas[identificador];
        var regra = RegrasNFe400.Lista.Single(r => r.Identificador == identificador);
        var nota = NotasTeste.NFCe();
        mudar(nota);

        var problema = Assert.Single(Conferir(nota, documento, configuracao), p => p.Codigo == codigo);

        Assert.Equal(caminho, problema.Caminho);
        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.StartsWith(regra.Mensagem, problema.Mensagem, StringComparison.Ordinal);
        Assert.NotNull(problema.Propriedade);
        Assert.Equal([documento], regra.Modelos);
    }

    [Fact]
    public void A393SoAcusaANFeSemOTpImp6()
    {
        var nota = NotasTeste.NFCe();
        Mudancas["ZX01-10"].Mudar(nota);

        // A NFC-e e a NF-e com tpImp 6 levam o grupo; a lib o confere contra o gerado (NFE-NUCLEO), não pela 393.
        Assert.DoesNotContain(Conferir(nota, Documento.NFCe), p => p.Codigo == "393");
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
        Assert.DoesNotContain(Conferir(nota, Documento.NFe), p => p.Codigo == "393");
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeRetrato;
        Assert.Contains(Conferir(nota, Documento.NFe), p => p.Codigo == "393");
        nota.Suplementares = null;
        Assert.DoesNotContain(Conferir(nota, Documento.NFe), p => p.Codigo == "393");
    }

    [Fact]
    public void A444SoAcusaAVersao2ComEmitenteCpf()
    {
        var nota = NotasTeste.NFCe();
        Mudancas["ZX02-222"].Mudar(nota);

        Assert.Contains(Conferir(nota, Documento.NFCe, Versao2), p => p.Codigo == "444");
        Assert.DoesNotContain(Conferir(nota, Documento.NFCe, new ConfiguracaoNFCe()), p => p.Codigo == "444");
        Assert.DoesNotContain(Conferir(nota, Documento.NFCe), p => p.Codigo == "444");
        Assert.DoesNotContain(Conferir(NotasTeste.NFCe(), Documento.NFCe, Versao2), p => p.Codigo == "444");
    }

    [Fact]
    public void A672SoAcusaANFeComTpImp6EAVersao2()
    {
        var nota = NotasTeste.NFCe();
        Mudancas["ZX02-220"].Mudar(nota);

        Assert.Contains(Conferir(nota, Documento.NFe, Versao2), p => p.Codigo == "672");
        Assert.DoesNotContain(Conferir(nota, Documento.NFe, new ConfiguracaoNFCe()), p => p.Codigo == "672");
        Assert.DoesNotContain(Conferir(nota, Documento.NFe), p => p.Codigo == "672");
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeRetrato;
        Assert.DoesNotContain(Conferir(nota, Documento.NFe, Versao2), p => p.Codigo == "672");
        Assert.DoesNotContain(Conferir(nota, Documento.NFCe, Versao2), p => p.Codigo == "672");
    }
}
