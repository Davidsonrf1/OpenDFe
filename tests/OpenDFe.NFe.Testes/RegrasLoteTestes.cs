using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O lote de regras da etapa 3.2 (REGRAS.md 11): as notas de teste passam sem problema, e cada regra acusa a nota que a
// SEFAZ rejeitaria, no campo certo.
public class RegrasLoteTestes
{
    private const string CnpjErrado = "11444777000160";
    private const string CpfErrado = "52998224724";
    private const string ChaveErrada = "31261011222333000181550010000001231123456781";

    private static List<Problema> Conferir(NotaFiscal nota, Documento documento = Documento.NFe) =>
        RegrasNFe400.Conferir(RegrasNFe400.Lista, nota, "", new ContextoRegra(NotasTeste.Nucleo(documento), null, NotasTeste.Emissao));

    // Cada regra da lista: o identificador, o código, a mudança na NF-e de teste e o caminho do problema.
    public static TheoryData<string, string, string> Casos => new(Mudancas.Select(m => (m.Key, m.Value.Codigo, m.Value.Caminho)));

    private static readonly Dictionary<string, (string Codigo, string Caminho, Action<NotaFiscal> Mudar)> Mudancas = new()
    {
        ["C02-10"] = ("207", "NFe/infNFe/emit/CNPJ", n => n.Emitente.Cnpj = "11222333000180"),
        ["C02a-20"] = ("401", "NFe/infNFe/emit/CPF", n => (n.Emitente.Cnpj, n.Emitente.Cpf) = (null, CpfErrado)),
        ["E02-10"] = ("208", "NFe/infNFe/dest/CNPJ", n => n.Destinatario!.Cnpj = CnpjErrado),
        ["E03-10"] = ("237", "NFe/infNFe/dest/CPF", n => (n.Destinatario!.Cnpj, n.Destinatario.Cpf) = (null, "11111111111")),
        ["F02-10"] = ("512", "NFe/infNFe/retirada/CNPJ", n => n.Retirada!.Cnpj = "00000000000000"),
        ["F02a-10"] = ("540", "NFe/infNFe/retirada/CPF", n => (n.Retirada!.Cnpj, n.Retirada.Cpf) = (null, "12345678900")),
        ["G02-10"] = ("514", "NFe/infNFe/entrega/CNPJ", n => (n.Entrega!.Cpf, n.Entrega.Cnpj) = (null, CnpjErrado)),
        ["G02a-10"] = ("541", "NFe/infNFe/entrega/CPF", n => n.Entrega!.Cpf = CpfErrado),
        ["GA02-10"] = ("323", "NFe/infNFe/autXML[2]/CNPJ", n => n.AutorizadosXml.Add(new PessoaAutorizada { Cnpj = CnpjErrado })),
        ["GA03-10"] = ("325", "NFe/infNFe/autXML/CPF", n => n.AutorizadosXml[0].Cpf = CpfErrado),
        ["I05e-20"] = ("489", "NFe/infNFe/det[1]/prod/CNPJFab", n => n.Itens[0].Produto.CnpjFabricante = CnpjErrado),
        ["I23d-20"] = ("332", "NFe/infNFe/det[1]/prod/DI/CNPJ", n => n.Itens[0].Produto.Importacoes.Add(new DeclaracaoImportacao { Cnpj = CnpjErrado })),
        ["I23d1-10"] = ("332", "NFe/infNFe/det[1]/prod/DI/CPF", n => n.Itens[0].Produto.Importacoes.Add(new DeclaracaoImportacao { Cpf = CpfErrado })),
        ["X04-20"] = ("542", "NFe/infNFe/transp/transporta/CNPJ", n => n.Transporte.Transportador!.Cnpj = CnpjErrado),
        ["X05-10"] = ("543", "NFe/infNFe/transp/transporta/CPF", n => (n.Transporte.Transportador!.Cnpj, n.Transporte.Transportador.Cpf) = (null, CpfErrado)),
        ["YA03c-10"] = ("961", "NFe/infNFe/pag/detPag/CNPJPag", n => n.Pagamento.Formas[0].CnpjTransacao = CnpjErrado),
        ["YB02-10"] = ("440", "NFe/infNFe/infIntermed/CNPJ", n => n.Intermediador!.Cnpj = CnpjErrado),
        ["ZD02-10"] = ("973", "NFe/infNFe/infRespTec/CNPJ", n => n.ResponsavelTecnico!.Cnpj = CnpjErrado),
        ["BA02-10"] = ("547", "NFe/infNFe/ide/NFref[1]/refNFe", n => ((NFeReferenciada)n.Identificacao.Referencias[0]).Chave = ChaveErrada),
        ["BA19-10"] = ("547", "NFe/infNFe/ide/NFref[4]/refCTe", n => ((CTeReferenciado)n.Identificacao.Referencias[3]).Chave = "31261011222333000181570010000001231123456782"),
        ["I54-10"] = ("342", "NFe/infNFe/det[1]/prod/detExport/exportInd/chNFe", n => n.Itens[0].Produto.Exportacoes.Add(
            new DetalheExportacao { Indireta = new ExportacaoIndireta { RegistroExportacao = "1", Chave = ChaveErrada, Quantidade = 1m } })),
        ["BB05-70"] = ("1204", "NFe/infNFe/ide/gCompraGov/refDFeAnt", n => n.Identificacao.CompraGovernamental =
            new CompraGovernamental { Ente = EnteGovernamental.Uniao, PercentualReducao = 0m, DocumentosAnteriores = { ChaveErrada } }),

        ["W03-10"] = ("531", "NFe/infNFe/total/ICMSTot/vBC", n => n.Total.Icms.BaseCalculo = 999m),
        ["W04-10"] = ("532", "NFe/infNFe/total/ICMSTot/vICMS", n => n.Total.Icms.Valor = 181m),
        ["W04a-10"] = ("795", "NFe/infNFe/total/ICMSTot/vICMSDeson", n => n.Total.Icms.ValorDesonerado = 1m),
        ["W04b-10"] = ("861", "NFe/infNFe/total/ICMSTot/vFCP", n => n.Total.Icms.ValorFcp = 1m),
        ["W04c-10"] = ("798", "NFe/infNFe/total/ICMSTot/vFCPUFDest", n => n.Total.Icms.ValorFcpUFDestino = 1m),
        ["W04e-10"] = ("799", "NFe/infNFe/total/ICMSTot/vICMSUFDest", n => n.Total.Icms.ValorIcmsUFDestino = 1m),
        ["W04g-10"] = ("800", "NFe/infNFe/total/ICMSTot/vICMSUFRemet", n => n.Total.Icms.ValorIcmsUFRemetente = 1m),
        ["W05-10"] = ("533", "NFe/infNFe/total/ICMSTot/vBCST", n => n.Total.Icms.BaseCalculoSt = 1m),
        ["W06-10"] = ("534", "NFe/infNFe/total/ICMSTot/vST", n => n.Total.Icms.ValorSt = 1m),
        ["W06a-10"] = ("862", "NFe/infNFe/total/ICMSTot/vFCPST", n => n.Total.Icms.ValorFcpSt = 1m),
        ["W06b-10"] = ("859", "NFe/infNFe/total/ICMSTot/vFCPSTRet", n => n.Total.Icms.ValorFcpStRetido = 1m),
        ["W07-10"] = ("564", "NFe/infNFe/total/ICMSTot/vProd", n => n.Total.Icms.ValorProdutos = 999m),
        ["W08-10"] = ("535", "NFe/infNFe/total/ICMSTot/vFrete", n => n.Total.Icms.Frete = 1m),
        ["W09-10"] = ("536", "NFe/infNFe/total/ICMSTot/vSeg", n => n.Total.Icms.Seguro = 1m),
        ["W10-10"] = ("537", "NFe/infNFe/total/ICMSTot/vDesc", n => n.Total.Icms.Desconto = 1m),
        ["W11-10"] = ("601", "NFe/infNFe/total/ICMSTot/vII", n => n.Total.Icms.ValorImpostoImportacao = 1m),
        ["W12-10"] = ("538", "NFe/infNFe/total/ICMSTot/vIPI", n => n.Total.Icms.ValorIpi = 99m),
        ["W12a-10"] = ("863", "NFe/infNFe/total/ICMSTot/vIPIDevol", n => n.Total.Icms.ValorIpiDevolvido = 1m),
        ["W13-10"] = ("602", "NFe/infNFe/total/ICMSTot/vPIS", n => n.Total.Icms.ValorPis = 16m),
        ["W14-10"] = ("603", "NFe/infNFe/total/ICMSTot/vCOFINS", n => n.Total.Icms.ValorCofins = 75m),
        ["W15-10"] = ("604", "NFe/infNFe/total/ICMSTot/vOutro", n => n.Total.Icms.OutrasDespesas = 1m),
        ["W16-10"] = ("610", "NFe/infNFe/total/ICMSTot/vNF", n => n.Total.Icms.ValorNota = 1223m),
        ["W16a-10"] = ("685", "NFe/infNFe/total/ICMSTot/vTotTrib", n => n.Total.Icms.ValorTotalTributos = 1m),
        ["W18-10"] = ("605", "NFe/infNFe/total/ISSQNtot/vServ", n => n.Total.Issqn!.ValorServicos = 121m),
        ["W19-10"] = ("606", "NFe/infNFe/total/ISSQNtot/vBC", n => n.Total.Issqn!.BaseCalculo = 121m),
        ["W20-10"] = ("607", "NFe/infNFe/total/ISSQNtot/vISS", n => n.Total.Issqn!.Valor = 6m),
        ["W21-10"] = ("608", "NFe/infNFe/total/ISSQNtot/vPIS", n => n.Total.Issqn!.ValorPis = 1m),
        ["W22-10"] = ("609", "NFe/infNFe/total/ISSQNtot/vCOFINS", n => n.Total.Issqn!.ValorCofins = 1m),
        ["W22b-10"] = ("364", "NFe/infNFe/total/ISSQNtot/vDeducao", n => n.Total.Issqn!.Deducao = 1m),
        ["W22c-10"] = ("365", "NFe/infNFe/total/ISSQNtot/vOutro", n => n.Total.Issqn!.OutrasRetencoes = 1m),
        ["W22d-10"] = ("366", "NFe/infNFe/total/ISSQNtot/vDescIncond", n => n.Total.Issqn!.DescontoIncondicionado = 1m),
        ["W22e-10"] = ("367", "NFe/infNFe/total/ISSQNtot/vDescCond", n => n.Total.Issqn!.DescontoCondicionado = 1m),
        ["W22f-10"] = ("368", "NFe/infNFe/total/ISSQNtot/vISSRet", n => n.Total.Issqn!.Retencao = 1m),

        ["I08-30"] = ("731", "NFe/infNFe/det[1]/prod/CFOP", n => n.Itens[0].Produto.Cfop = 7101),
        ["I08-40"] = ("732", "NFe/infNFe/det[1]/prod/CFOP", n => n.Identificacao.LocalDestino = LocalDestino.Interna),
        ["I08-50"] = ("733", "NFe/infNFe/det[1]/prod/CFOP", n => n.Itens[0].Produto.Cfop = 5102),
        ["E16a-40"] = ("696", "NFe/infNFe/dest/indIEDest", n => (n.Destinatario!.IndicadorIE, n.Destinatario.InscricaoEstadual) = (IndicadorIEDestinatario.NaoContribuinte, null)),
        ["E17-20"] = ("728", "NFe/infNFe/dest/indIEDest", n => n.Destinatario!.InscricaoEstadual = null),
        ["E17-30"] = ("791", "NFe/infNFe/dest/IE", n => n.Destinatario!.IndicadorIE = IndicadorIEDestinatario.ContribuinteIsento),
        ["E17-40"] = ("792", "NFe/infNFe/dest/IE", n => (n.Destinatario!.Endereco!.UF, n.Destinatario.Endereco.Exterior) = (null, true)),
        ["B09-10"] = ("703", "NFe/infNFe/ide/dhEmi", n => n.Identificacao.Emissao = NotasTeste.Emissao.AddMinutes(6)),
        ["B10-40"] = ("506", "NFe/infNFe/ide/dhSaiEnt", n => n.Identificacao.SaidaEntrada = NotasTeste.Emissao.AddMinutes(-1)),
        ["I83-10"] = ("877", "NFe/infNFe/det[1]/prod/rastro/dFab", n => n.Itens[0].Produto.Rastros[0].Fabricacao = new DateOnly(2026, 10, 21)),
        ["I84-10"] = ("870", "NFe/infNFe/det[1]/prod/rastro/dVal", n => n.Itens[0].Produto.Rastros[0].Validade = new DateOnly(2025, 1, 10)),
        ["Y09-20"] = ("900", "NFe/infNFe/cobr/dup/dVenc", n => n.Cobranca!.Duplicatas[0].Vencimento = new DateOnly(2026, 10, 19)),
        ["Y09-30"] = ("850", "NFe/infNFe/cobr/dup[2]/dVenc", n => n.Cobranca!.Duplicatas.Add(new Duplicata { Numero = "002", Vencimento = new DateOnly(2026, 11, 19), Valor = 1m })),
    };

    [Fact]
    public void AsNotasDeTestePassamSemProblema()
    {
        Assert.Empty(Conferir(NotasTeste.NFe()));
        Assert.Empty(Conferir(NotasTeste.NFCe(), Documento.NFCe));
    }

    [Fact]
    public void CadaRegraTemOSeuCaso()
    {
        // As da NFC-e (3.3) têm os casos em RegrasNFCeTestes; as do QR Code (5.1), em RegrasQrCodeTestes; as da contingência (5.2), em RegrasContingenciaTestes.
        Assert.Equal(RegrasNFe400.Lista.Select(r => r.Identificador).Order(StringComparer.Ordinal),
            Mudancas.Keys.Concat(RegrasNFCeTestes.Mudancas.Keys).Concat(RegrasQrCodeTestes.Mudancas.Keys).Concat(RegrasContingenciaTestes.Mudancas.Keys).Order(StringComparer.Ordinal));
        Assert.Equal(RegrasNFe400.Lista.Count, RegrasNFe400.Lista.Select(r => (r.Identificador, r.Codigo)).Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(Casos))]
    public void ARegraAcusaANotaQueASefazRejeitaria(string identificador, string codigo, string caminho)
    {
        var nota = NotasTeste.NFe();
        Mudancas[identificador].Mudar(nota);
        var regra = RegrasNFe400.Lista.Single(r => r.Identificador == identificador);

        var problemas = Conferir(nota);

        var problema = Assert.Single(problemas, p => p.Codigo == codigo && p.Caminho == caminho);
        Assert.Equal(regra.Severidade, problema.Severidade);
        Assert.StartsWith(regra.Mensagem, problema.Mensagem, StringComparison.Ordinal);
        Assert.Equal(caminho, problema.Caminho);
    }

    [Fact]
    public void ATotalDifereComOsValoresNoDetalheEAPropriedade()
    {
        var nota = NotasTeste.NFe();
        nota.Total.Icms.ValorProdutos = 999.5m;

        var problema = Assert.Single(Conferir(nota), p => p.Codigo == "564");

        Assert.Equal("Total do Produto / Serviço difere do somatório dos itens (vProd 999,50; soma dos itens 1000,00)", problema.Mensagem);
        Assert.Equal("Total.Icms.ValorProdutos", problema.Propriedade);
        Assert.Equal(Severidade.Erro, problema.Severidade);
    }

    [Fact]
    public void OCpfErradoNaoVaiNaMensagem()
    {
        var nota = NotasTeste.NFe();
        Mudancas["E03-10"].Mudar(nota);

        var problema = Assert.Single(Conferir(nota), p => p.Codigo == "237");

        Assert.DoesNotContain("11111111111", problema.Mensagem, StringComparison.Ordinal);
        Assert.Equal("Destinatario.Cpf", problema.Propriedade);
    }

    [Fact]
    public void ONFeTemAToleranciaDeCinquentaCentavosEAceitaODesoneradoSemDescontar()
    {
        var nota = NotasTeste.NFe();
        nota.Total.Icms.ValorNota = 1222.50m;
        Assert.DoesNotContain(Conferir(nota), p => p.Codigo == "610");

        // Exceção 3: o vNF sem descontar o vICMSDeson também é aceito.
        nota = NotasTeste.NFe();
        nota.Itens[0].Imposto.Icms = new Icms40 { Origem = OrigemMercadoria.Nacional, Cst = CstIcms.Isenta, ValorDesonerado = 10m, MotivoDesoneracao = MotivoDesoneracao.Outros };
        nota.Total.Icms.ValorDesonerado = 10m;
        nota.Total.Icms.BaseCalculo = 0m;
        nota.Total.Icms.Valor = 0m;
        Assert.DoesNotContain(Conferir(nota), p => p.Codigo == "610");
        nota.Total.Icms.ValorNota = 1212m;
        Assert.DoesNotContain(Conferir(nota), p => p.Codigo == "610");
        nota.Total.Icms.ValorNota = 1200m;
        Assert.Contains(Conferir(nota), p => p.Codigo == "610");
    }

    [Fact]
    public void ONFeNaoEConferidoNaImportacao()
    {
        var nota = NotasTeste.NFe();
        nota.Itens[0].Produto.Cfop = 3102;
        nota.Total.Icms.ValorNota = 1m;

        Assert.DoesNotContain(Conferir(nota), p => p.Codigo == "610");
    }

    [Fact]
    public void OCfopDeExportacaoValeComOCombustivelConsumidoNoExterior()
    {
        var nota = NotasTeste.NFe();
        nota.Itens[0].Produto.Cfop = 7667;
        nota.Itens[0].Produto.Combustivel = new Combustivel { CodigoAnp = "210203001", DescricaoAnp = "GLP", Exterior = true };

        Assert.DoesNotContain(Conferir(nota), p => p.Codigo == "731");
    }

    [Fact]
    public void AsRegrasSoDaNFeNaoRodamNaNFCe()
    {
        var nota = NotasTeste.NFCe();
        nota.Itens[0].Produto.Cfop = 6102;

        Assert.DoesNotContain(Conferir(nota, Documento.NFCe), p => p.Codigo == "732");
        Assert.Contains(Conferir(nota, Documento.NFe), p => p.Codigo == "732");
    }
}
