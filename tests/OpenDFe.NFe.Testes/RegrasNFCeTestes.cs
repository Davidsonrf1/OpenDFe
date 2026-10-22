using OpenDFe.NFe;

namespace OpenDFe.Testes;

// As regras da NFC-e da etapa 3.3 (REGRAS.md 12): cada uma acusa a NFC-e que a SEFAZ rejeitaria, no campo certo; as da
// NT 2026.002 valem também para a NF-e com o DANFE Simplificado Tipo 2, e nenhuma roda na NF-e comum.
public class RegrasNFCeTestes
{
    private static List<Problema> Conferir(NotaFiscal nota, Documento documento = Documento.NFCe, UF uf = UF.MG) =>
        RegrasNFe400.Conferir(RegrasNFe400.Lista, nota, "", new ContextoRegra(Nucleo(documento, uf), null, NotasTeste.Emissao));

    private static ValoresNucleo Nucleo(Documento documento, UF uf) =>
        new() { Documento = documento, UF = uf, Ambiente = Ambiente.Homologacao };

    // O destinatário que a NFC-e aceita: o consumidor com CPF, não contribuinte.
    private static Destinatario Consumidor() =>
        new() { Cpf = "52998224725", Nome = "CONSUMIDOR", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte };

    // Cada regra: o identificador, o código, a mudança na NFC-e de teste e o caminho do problema.
    public static TheoryData<string, string, string> Casos => new(Mudancas.Select(m => (m.Key, m.Value.Codigo, m.Value.Caminho)));

    // As regras da NT 2026.002, que valem para a NFC-e e para a NF-e com tpImp=6.
    public static TheoryData<string> Simplificadas => new(Mudancas.Keys.Where(k => RegrasNFe400.Lista.Single(r => r.Identificador == k).Modelos.Count == 2));

    internal static readonly Dictionary<string, (string Codigo, string Caminho, Action<NotaFiscal> Mudar)> Mudancas = new()
    {
        ["B10-10"] = ("705", "NFe/infNFe/ide/dhSaiEnt", n => n.Identificacao.SaidaEntrada = NotasTeste.Emissao),
        ["B10a-10"] = ("1153", "NFe/infNFe/ide/dPrevEntrega", n => n.Identificacao.PrevisaoEntrega = new DateOnly(2026, 10, 25)),
        ["B11-10"] = ("706", "NFe/infNFe/ide/tpNF", n => n.Identificacao.Tipo = TipoNota.Entrada),
        ["B11a-10"] = ("707", "NFe/infNFe/ide/idDest", n => n.Identificacao.LocalDestino = LocalDestino.Interestadual),
        ["B21-10"] = ("709", "NFe/infNFe/ide/tpImp", n => n.Identificacao.FormatoImpressao = FormatoImpressao.DanfeRetrato),
        ["B25-20"] = ("715", "NFe/infNFe/ide/finNFe", n => n.Identificacao.Finalidade = FinalidadeEmissao.Devolucao),
        ["B25a-10"] = ("716", "NFe/infNFe/ide/indFinal", n => n.Identificacao.ConsumidorFinal = IndicadorConsumidorFinal.Normal),
        ["B26-50"] = ("957", "NFe/infNFe/ide/procEmi", n => n.Identificacao.Processo = ProcessoEmissao.AvulsaFisco),
        ["BA01-10"] = ("708", "NFe/infNFe/ide/NFref", n => n.Identificacao.Referencias.Add(new NFeReferenciada { Chave = "31261011222333000181550010000001231123456780" })),
        ["BB01-10"] = ("1006", "NFe/infNFe/ide/gCompraGov", n => n.Identificacao.CompraGovernamental = new CompraGovernamental { Ente = EnteGovernamental.Uniao, PercentualReducao = 0m }),
        ["C18-10"] = ("718", "NFe/infNFe/emit/IEST", n => n.Emitente.InscricaoEstadualSubstituto = "0012345670099"),
        ["E01-20"] = ("787", "NFe/infNFe/ide/indPres", n =>
        {
            n.Identificacao.Presenca = PresencaComprador.NaoPresencialEntrega;
            n.Transporte.Transportador = new Transportador { Nome = "ENTREGAS" };
        }),
        ["E02-20"] = ("220", "NFe/infNFe/dest/CNPJ", n => n.Destinatario = new Destinatario
        {
            Cnpj = n.Emitente.Cnpj, Nome = "A PROPRIA", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte,
        }),
        ["E05-20"] = ("788", "NFe/infNFe/dest", n =>
        {
            n.Identificacao.Presenca = PresencaComprador.NaoPresencialEntrega;
            n.Transporte.Transportador = new Transportador { Nome = "ENTREGAS" };
            n.Destinatario = Consumidor();
        }),
        ["E16a-10"] = ("789", "NFe/infNFe/dest/indIEDest", n => n.Destinatario = new Destinatario
        {
            Cnpj = "11222333000181", Nome = "CONTRIBUINTE", IndicadorIE = IndicadorIEDestinatario.Contribuinte, InscricaoEstadual = "0012345670099",
        }),
        ["E17-10"] = ("729", "NFe/infNFe/dest/IE", n =>
        {
            n.Identificacao.LocalDestino = LocalDestino.Exterior;
            n.Destinatario = Consumidor();
            n.Destinatario.InscricaoEstadual = "0012345670099";
        }),
        ["E18-10"] = ("730", "NFe/infNFe/dest/ISUF", n =>
        {
            n.Destinatario = Consumidor();
            n.Destinatario.InscricaoSuframa = "200100000";
        }),
        ["J01-10"] = ("736", "NFe/infNFe/det/prod/veicProd", n => n.Itens[0].Produto.VeiculoNovo = new VeiculoNovo()),
        ["L01-10"] = ("738", "NFe/infNFe/det/prod/arma", n => n.Itens[0].Produto.Armas.Add(new Arma())),
        ["LB01-10"] = ("348", "NFe/infNFe/det/prod/nRECOPI", n => n.Itens[0].Produto.Recopi = "20130000000000000001"),
        ["N12-50"] = ("741", "NFe/infNFe/det/imposto/ICMS/ICMSPart", n => n.Itens[0].Imposto.Icms = new IcmsPartilha()),
        ["NA01-10"] = ("807", "NFe/infNFe/det/imposto/ICMSUFDest", n => n.Itens[0].Imposto.IcmsUFDestino = new IcmsUFDestino()),
        ["O01-10"] = ("742", "NFe/infNFe/det/imposto/IPI", n => n.Itens[0].Imposto.Ipi = new Ipi()),
        ["P01-10"] = ("743", "NFe/infNFe/det/imposto/II", n => n.Itens[0].Imposto.ImpostoImportacao = new ImpostoImportacao()),
        ["R01-10"] = ("746", "NFe/infNFe/det/imposto/PISST", n => n.Itens[0].Imposto.PisSt = new PisSt()),
        ["T01-10"] = ("749", "NFe/infNFe/det/imposto/COFINSST", n => n.Itens[0].Imposto.CofinsSt = new CofinsSt()),
        ["X03-10"] = ("754", "NFe/infNFe/transp/transporta", n => n.Transporte.Transportador = new Transportador { Nome = "ENTREGAS" }),
        ["X03-20"] = ("786", "NFe/infNFe/transp", n =>
        {
            n.Identificacao.Presenca = PresencaComprador.NaoPresencialEntrega;
            n.Destinatario = Consumidor();
        }),
        ["YA02-10"] = ("857", "NFe/infNFe/pag/detPag[2]/tPag", n => n.Pagamento.Formas.Add(new FormaPagamento { Meio = MeioPagamento.DuplicataMercantil, Valor = 1m })),
        ["YA02-40"] = ("899", "NFe/infNFe/pag/detPag/tPag", n => n.Pagamento.Formas[0].Meio = MeioPagamento.Nenhum),
        ["ZA01-30"] = ("814", "NFe/infNFe/exporta", n => n.Exportacao = new Exportacao { UFSaida = UF.SP, LocalEmbarque = "SANTOS" }),
    };

    [Theory]
    [MemberData(nameof(Casos))]
    public void ARegraAcusaANFCeQueASefazRejeitaria(string identificador, string codigo, string caminho)
    {
        var nota = NotasTeste.NFCe();
        Mudancas[identificador].Mudar(nota);
        var regra = RegrasNFe400.Lista.Single(r => r.Identificador == identificador);

        var problemas = Conferir(nota);

        var problema = Assert.Single(problemas, p => p.Codigo == codigo);
        Assert.Equal(caminho, problema.Caminho);
        Assert.Equal(regra.Severidade, problema.Severidade);
        Assert.StartsWith(regra.Mensagem, problema.Mensagem, StringComparison.Ordinal);
        Assert.NotNull(problema.Propriedade);
    }

    [Theory]
    [MemberData(nameof(Simplificadas))]
    public void ARegraDaNT2026002ValeNaNFeComODanfeSimplificadoTipo2(string identificador)
    {
        var (codigo, caminho, mudar) = Mudancas[identificador];
        var nota = NotasTeste.NFCe();
        mudar(nota);

        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
        Assert.Contains(Conferir(nota, Documento.NFe), p => p.Codigo == codigo && p.Caminho == caminho);

        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeRetrato;
        Assert.DoesNotContain(Conferir(nota, Documento.NFe), p => p.Codigo == codigo);
    }

    [Fact]
    public void AsRegrasSoDaNFCeNaoRodamNaNFe()
    {
        foreach (var (identificador, (codigo, _, mudar)) in Mudancas)
        {
            if (RegrasNFe400.Lista.Single(r => r.Identificador == identificador).Modelos.Count == 2)
                continue;
            var nota = NotasTeste.NFCe();
            mudar(nota);
            nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
            Assert.DoesNotContain(Conferir(nota, Documento.NFe), p => p.Codigo == codigo);
        }
    }

    [Fact]
    public void ANFCeDeEntregaComDestinatarioEnderecoETransportadorPassa()
    {
        var nota = NotasTeste.NFCe();
        nota.Identificacao.Presenca = PresencaComprador.NaoPresencialEntrega;
        nota.Destinatario = Consumidor();
        nota.Destinatario.Endereco = new EnderecoDestinatario { Logradouro = "RUA A", Numero = "1", Bairro = "CENTRO", CodigoMunicipio = 3106200, Municipio = "BELO HORIZONTE", UF = UF.MG };
        nota.Transporte.Transportador = new Transportador { Nome = "ENTREGAS" };

        Assert.Empty(Conferir(nota));
    }

    [Fact]
    public void OProcessoDeEmissaoTemAsExcecoesDaNFFEDeSC()
    {
        // Exceção 1 da B26-50: em SC, a NFC-e pelo aplicativo da nota avulsa.
        var nota = NotasTeste.NFCe();
        nota.Identificacao.Processo = ProcessoEmissao.AvulsaContribuinteSite;
        Assert.DoesNotContain(Conferir(nota, uf: UF.SC), p => p.Codigo == "957");
        Assert.Contains(Conferir(nota), p => p.Codigo == "957");

        // A NFF (tpEmis=3) não é conferida.
        nota.Identificacao.Processo = ProcessoEmissao.AplicativoFisco;
        nota.Identificacao.TipoEmissao = (TipoEmissao)3;
        Assert.DoesNotContain(Conferir(nota), p => p.Codigo == "957");
    }

    [Fact]
    public void OGrupoNoSegundoItemApontaOItem()
    {
        var nota = NotasTeste.NFCe();
        nota.Itens.Add(NotasTeste.NFCe().Itens[0]);
        nota.Itens[1].Imposto.Ipi = new Ipi();

        var problema = Assert.Single(Conferir(nota), p => p.Codigo == "742");

        Assert.Equal("NFe/infNFe/det[2]/imposto/IPI", problema.Caminho);
        Assert.Equal("Itens[1].Imposto.Ipi", problema.Propriedade);
    }
}
