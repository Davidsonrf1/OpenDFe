using OpenDFe.NFe;

namespace OpenDFe.Testes;

// As três regras da contingência offline da etapa 5.2 (REGRAS.md 16, CONTRATOS.md 17, decisão 5): a NF-e com tpEmis 9 sem
// tpImp 6 (711) e a data e a justificativa da contingência que sobram na emissão normal (556) ou faltam na contingência
// (557). O tipo de emissão é o da nota ou, nulo, o do núcleo; a data e a justificativa, as da nota ou as do núcleo.
public class RegrasContingenciaTestes
{
    private static readonly Contingencia Entrada = new(NotasTeste.Emissao, "Sem comunicacao com a SEFAZ");

    private static List<Problema> Conferir(NotaFiscal nota, Documento documento, TipoEmissao emissao = TipoEmissao.Normal,
        Contingencia? contingencia = null) =>
        RegrasNFe400.Conferir(RegrasNFe400.Lista, nota, "", new ContextoRegra(
            NotasTeste.Nucleo(documento, emissao, contingencia), null, NotasTeste.Emissao));

    // O que cada regra acusa: o código, o caminho, o documento e a mudança na nota de teste.
    internal static readonly Dictionary<string, (string Codigo, string Caminho, Documento Documento, Action<NotaFiscal> Mudar)> Mudancas = new()
    {
        ["B22-10"] = ("711", "NFe/infNFe/ide/tpEmis", Documento.NFe, n => n.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline),
        ["B28-10"] = ("556", "NFe/infNFe/ide/dhCont", Documento.NFCe, n => n.Identificacao.InicioContingencia = NotasTeste.Emissao),
        ["B28-20"] = ("557", "NFe/infNFe/ide/dhCont", Documento.NFCe, n => n.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline),
    };

    public static TheoryData<string> Regras => new(Mudancas.Keys);

    [Theory]
    [MemberData(nameof(Regras))]
    public void ARegraAcusaANotaQueASefazRejeitaria(string identificador)
    {
        var (codigo, caminho, documento, mudar) = Mudancas[identificador];
        var regra = RegrasNFe400.Lista.Single(r => r.Identificador == identificador);
        var nota = NotasTeste.NFCe();
        mudar(nota);

        var problema = Assert.Single(Conferir(nota, documento), p => p.Codigo == codigo && p.Caminho == caminho);

        Assert.Equal(Severidade.Erro, problema.Severidade);
        Assert.StartsWith(regra.Mensagem, problema.Mensagem, StringComparison.Ordinal);
        Assert.NotNull(problema.Propriedade);
    }

    [Fact]
    public void A711SoAcusaANFeEmOfflineSemOTpImp6()
    {
        var nota = NotasTeste.NFe();

        Assert.DoesNotContain(Conferir(nota, Documento.NFe), p => p.Codigo == "711");
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline;
        Assert.Contains(Conferir(nota, Documento.NFe, contingencia: Entrada), p => p.Codigo == "711");
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
        Assert.DoesNotContain(Conferir(nota, Documento.NFe, contingencia: Entrada), p => p.Codigo == "711");
        // O tipo de emissão do núcleo vale quando a nota não o traz; a NFC-e não é alvo (o modelo 65 tem o tpImp 4 ou 5).
        nota.Identificacao.TipoEmissao = null;
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeRetrato;
        Assert.Contains(Conferir(nota, Documento.NFe, TipoEmissao.ContingenciaOffline, Entrada), p => p.Codigo == "711");
        Assert.DoesNotContain(Conferir(NotasTeste.NFCe(), Documento.NFCe, TipoEmissao.ContingenciaOffline, Entrada), p => p.Codigo == "711");
    }

    [Fact]
    public void A556AcusaADataEAJustificativaNaEmissaoNormalMasNaoNaContingencia()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.InicioContingencia = NotasTeste.Emissao;
        nota.Identificacao.JustificativaContingencia = "Sem comunicacao com a SEFAZ";

        var problemas = Conferir(nota, Documento.NFCe).Where(p => p.Codigo == "556").ToList();

        Assert.Equal(["NFe/infNFe/ide/dhCont", "NFe/infNFe/ide/xJust"], problemas.Select(p => p.Caminho));
        Assert.Empty(Conferir(NotasTeste.NFCe(), Documento.NFCe));
        // Na contingência (tpEmis 9), os dois campos são o que se espera.
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline;
        Assert.DoesNotContain(Conferir(nota, Documento.NFCe), p => p.Codigo is "556" or "557");
        // Os da contingência do núcleo, sem o tipo na nota, também contam como informados na emissão normal.
        Assert.Contains(Conferir(NotasTeste.NFCe(), Documento.NFCe, contingencia: Entrada), p => p.Codigo == "556");
    }

    [Fact]
    public void A557AcusaOQueFaltaNaContingenciaEAceitaOQueVemDoNucleo()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline;

        var problemas = Conferir(nota, Documento.NFCe).Where(p => p.Codigo == "557").ToList();
        Assert.Equal(["NFe/infNFe/ide/dhCont", "NFe/infNFe/ide/xJust"], problemas.Select(p => p.Caminho));

        Assert.DoesNotContain(Conferir(nota, Documento.NFCe, TipoEmissao.ContingenciaOffline, Entrada), p => p.Codigo == "557");
        nota.Identificacao.InicioContingencia = NotasTeste.Emissao;
        Assert.Equal("NFe/infNFe/ide/xJust", Assert.Single(Conferir(nota, Documento.NFCe), p => p.Codigo == "557").Caminho);
        nota.Identificacao.JustificativaContingencia = "Sem comunicacao com a SEFAZ";
        Assert.DoesNotContain(Conferir(nota, Documento.NFCe), p => p.Codigo == "557");
    }
}
