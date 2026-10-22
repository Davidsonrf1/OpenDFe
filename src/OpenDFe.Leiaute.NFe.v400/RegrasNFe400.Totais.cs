namespace OpenDFe.NFe;

// Os totais contra a soma dos itens (REGRAS.md 11): o ICMSTot, o vNF e o ISSQNtot. O campo não informado vale zero,
// no total e no item; a soma é exata, salvo o vNF, com a tolerância de R$ 0,50 do MOC (nota *3 do Anexo I).
internal static partial class RegrasNFe400
{
    private const decimal ToleranciaNota = 0.50m;

    private static IEnumerable<RegraNFe> Totais() =>
    [
        SomaIcms("W03-10", "531", Ambos, AplicacaoRegra.Facultativa, "Total da BC ICMS difere do somatório dos itens",
            "vBC", t => t.BaseCalculo, i => BaseIcms(i.Imposto.Icms)),
        SomaIcms("W04-10", "532", Ambos, AplicacaoRegra.Facultativa, "Total do ICMS difere do somatório dos itens",
            "vICMS", t => t.Valor, i => ValorIcms(i.Imposto.Icms)),
        SomaIcms("W04a-10", "795", Ambos, AplicacaoRegra.Facultativa,
            "Total do ICMS desonerado difere do somatório dos itens",
            "vICMSDeson", t => t.ValorDesonerado, i => DesoneradoIcms(i.Imposto.Icms)),
        SomaIcms("W04b-10", "861", Ambos, AplicacaoRegra.Obrigatoria, "Total do FCP difere do somatório dos itens",
            "vFCP", t => t.ValorFcp, i => FcpIcms(i.Imposto.Icms)),
        SomaIcms("W04c-10", "798", SoNFe, AplicacaoRegra.Obrigatoria,
            "Valor total do ICMS relativo Fundo de Combate à Pobreza (FCP) da UF de destino difere do somatório do valor dos itens",
            "vFCPUFDest", t => t.ValorFcpUFDestino, i => i.Imposto.IcmsUFDestino?.ValorFcp ?? 0),
        SomaIcms("W04e-10", "799", SoNFe, AplicacaoRegra.Obrigatoria,
            "Valor total do ICMS Interestadual da UF de destino difere do somatório dos itens",
            "vICMSUFDest", t => t.ValorIcmsUFDestino, i => i.Imposto.IcmsUFDestino?.ValorDestino ?? 0),
        SomaIcms("W04g-10", "800", SoNFe, AplicacaoRegra.Obrigatoria,
            "Valor total do ICMS Interestadual da UF do remetente difere do somatório dos itens",
            "vICMSUFRemet", t => t.ValorIcmsUFRemetente, i => i.Imposto.IcmsUFDestino?.ValorRemetente ?? 0),
        SomaIcms("W05-10", "533", Ambos, AplicacaoRegra.Facultativa, "Total da BC ICMS-ST difere do somatório dos itens",
            "vBCST", t => t.BaseCalculoSt, i => BaseStIcms(i.Imposto.Icms)),
        SomaIcms("W06-10", "534", Ambos, AplicacaoRegra.Facultativa, "Total do ICMS-ST difere do somatório dos itens",
            "vST", t => t.ValorSt, i => StIcms(i.Imposto.Icms)),
        SomaIcms("W06a-10", "862", SoNFe, AplicacaoRegra.Obrigatoria, "Total do FCP ST difere do somatório dos itens",
            "vFCPST", t => t.ValorFcpSt, i => FcpStIcms(i.Imposto.Icms)),
        SomaIcms("W06b-10", "859", SoNFe, AplicacaoRegra.Obrigatoria,
            "Total do FCP retido anteriormente por Substituição Tributária difere do somatório dos itens",
            "vFCPSTRet", t => t.ValorFcpStRetido, i => FcpStRetidoIcms(i.Imposto.Icms)),
        // NT 2025.002: só os itens que compõem o total (indTot=1) sujeitos ao ICMS ou ao IBS, à CBS e ao IS, isto é,
        // os que não são de serviço (sem o grupo ISSQN, que entram no vServ).
        SomaIcms("W07-10", "564", Ambos, AplicacaoRegra.Obrigatoria, "Total do Produto / Serviço difere do somatório dos itens",
            "vProd", t => t.ValorProdutos,
            i => i.Produto.Totalizacao == IndicadorTotal.Compoe && i.Imposto.Issqn is null ? i.Produto.Valor ?? 0 : 0),
        SomaIcms("W08-10", "535", Ambos, AplicacaoRegra.Facultativa, "Total do Frete difere do somatório dos itens",
            "vFrete", t => t.Frete, i => i.Produto.Frete ?? 0),
        SomaIcms("W09-10", "536", Ambos, AplicacaoRegra.Facultativa, "Total do Seguro difere do somatório dos itens",
            "vSeg", t => t.Seguro, i => i.Produto.Seguro ?? 0),
        SomaIcms("W10-10", "537", Ambos, AplicacaoRegra.Facultativa, "Total do Desconto difere do somatório dos itens",
            "vDesc", t => t.Desconto, i => i.Produto.Desconto ?? 0),
        SomaIcms("W11-10", "601", SoNFe, AplicacaoRegra.Facultativa, "Total do II difere do somatório dos itens",
            "vII", t => t.ValorImpostoImportacao, i => i.Imposto.ImpostoImportacao?.Valor ?? 0),
        SomaIcms("W12-10", "538", SoNFe, AplicacaoRegra.Facultativa, "Total do IPI difere do somatório dos itens",
            "vIPI", t => t.ValorIpi, i => i.Imposto.Ipi?.Tributado?.Valor ?? 0),
        SomaIcms("W12a-10", "863", SoNFe, AplicacaoRegra.Facultativa, "Total do IPI devolvido difere do somatório dos itens",
            "vIPIDevol", t => t.ValorIpiDevolvido, i => i.ImpostoDevolvido?.Ipi.Valor ?? 0),
        SomaIcms("W13-10", "602", Ambos, AplicacaoRegra.Facultativa, "Total do PIS difere do somatório dos itens sujeitos ao ICMS",
            "vPIS", t => t.ValorPis, i => i.Imposto.Icms is null ? 0 : ValorPis(i.Imposto.Pis)),
        SomaIcms("W14-10", "603", Ambos, AplicacaoRegra.Facultativa,
            "Total da COFINS difere do somatório dos itens sujeitos ao ICMS",
            "vCOFINS", t => t.ValorCofins, i => i.Imposto.Icms is null ? 0 : ValorCofins(i.Imposto.Cofins)),
        SomaIcms("W15-10", "604", Ambos, AplicacaoRegra.Facultativa, "Total do vOutro difere do somatório dos itens",
            "vOutro", t => t.OutrasDespesas, i => i.Produto.OutrasDespesas ?? 0),
        Regra("W16-10", "610", Ambos, AplicacaoRegra.Facultativa,
            "Total da NF difere do somatório dos Valores que compõe o valor Total da NF", (n, _) => TotalNota(n)),
        SomaIcms("W16a-10", "685", Ambos, AplicacaoRegra.Facultativa,
            "Total do Valor Aproximado dos Tributos difere do somatório dos itens",
            "vTotTrib", t => t.ValorTotalTributos, i => i.Imposto.ValorTotalTributos ?? 0),

        SomaIssqn("W18-10", "605", AplicacaoRegra.Facultativa,
            "Total do vServ difere do somatório do vProd dos itens sujeitos ao ISSQN",
            "vServ", t => t.ValorServicos, i => i.Produto.Valor ?? 0),
        SomaIssqn("W19-10", "606", AplicacaoRegra.Facultativa, "Total do vBC do ISS difere do somatório dos itens",
            "vBC", t => t.BaseCalculo, i => i.Imposto.Issqn!.BaseCalculo ?? 0),
        SomaIssqn("W20-10", "607", AplicacaoRegra.Facultativa, "Total do ISS difere do somatório dos itens",
            "vISS", t => t.Valor, i => i.Imposto.Issqn!.Valor ?? 0),
        SomaIssqn("W21-10", "608", AplicacaoRegra.Facultativa,
            "Total do PIS difere do somatório dos itens sujeitos ao ISSQN",
            "vPIS", t => t.ValorPis, i => ValorPis(i.Imposto.Pis)),
        SomaIssqn("W22-10", "609", AplicacaoRegra.Facultativa,
            "Total da COFINS difere do somatório dos itens sujeitos ao ISSQN",
            "vCOFINS", t => t.ValorCofins, i => ValorCofins(i.Imposto.Cofins)),
        SomaIssqn("W22b-10", "364", AplicacaoRegra.Obrigatoria,
            "Total do valor da dedução do ISS difere do somatório dos itens",
            "vDeducao", t => t.Deducao, i => i.Imposto.Issqn!.Deducao ?? 0),
        SomaIssqn("W22c-10", "365", AplicacaoRegra.Obrigatoria, "Total de outras retenções difere do somatório dos itens",
            "vOutro", t => t.OutrasRetencoes, i => i.Imposto.Issqn!.OutrasRetencoes ?? 0),
        SomaIssqn("W22d-10", "366", AplicacaoRegra.Obrigatoria,
            "Total do desconto incondicionado ISS difere do somatório dos itens",
            "vDescIncond", t => t.DescontoIncondicionado, i => i.Imposto.Issqn!.DescontoIncondicionado ?? 0),
        SomaIssqn("W22e-10", "367", AplicacaoRegra.Obrigatoria,
            "Total do desconto condicionado ISS difere do somatório dos itens",
            "vDescCond", t => t.DescontoCondicionado, i => i.Imposto.Issqn!.DescontoCondicionado ?? 0),
        SomaIssqn("W22f-10", "368", AplicacaoRegra.Obrigatoria, "Total de ISS retido difere do somatório dos itens",
            "vISSRet", t => t.Retencao, i => i.Imposto.Issqn!.Retencao ?? 0),
    ];

    private static RegraNFe SomaIcms(string identificador, string codigo, Documento[] modelos, AplicacaoRegra aplicacao,
        string mensagem, string tag, Func<TotalIcms, decimal?> total, Func<Item, decimal> item) =>
        Regra(identificador, codigo, modelos, aplicacao, mensagem, (n, _) =>
            Comparar(total(n.Total.Icms) ?? 0, n.Itens.Sum(item), tag, $"{Raiz}/total/ICMSTot/{tag}"));

    // Só com o ISSQNtot, e só os itens com o grupo ISSQN.
    private static RegraNFe SomaIssqn(string identificador, string codigo, AplicacaoRegra aplicacao, string mensagem,
        string tag, Func<TotalIssqn, decimal?> total, Func<Item, decimal> item) =>
        Regra(identificador, codigo, Ambos, aplicacao, mensagem, (n, _) =>
            n.Total.Issqn is { } issqn
                ? Comparar(total(issqn) ?? 0, n.Itens.Where(i => i.Imposto.Issqn is not null).Sum(item), tag,
                    $"{Raiz}/total/ISSQNtot/{tag}")
                : []);

    private static IEnumerable<OcorrenciaRegra> Comparar(decimal informado, decimal soma, string tag, string caminho) =>
        informado == soma ? [] : [new OcorrenciaRegra(caminho, Comparacao(tag, informado, soma))];

    // W16-10 (NT 2023.001): o vNF é a soma dos totais que o compõem. Exceção 1: no faturamento direto de veículo novo
    // (tpOp=2), sem o ST, o FCP ST, o ICMS monofásico retido e o IPI devolvido. Exceção 2: não vale na importação (CFOP
    // iniciado por 3). Exceção 3: aceita o vNF sem descontar o ICMS desonerado.
    private static IEnumerable<OcorrenciaRegra> TotalNota(NotaFiscal nota)
    {
        if (nota.Itens.Any(i => i.Produto.Cfop is >= 3000 and < 4000))
            return [];
        var t = nota.Total.Icms;
        var faturamentoDireto = nota.Itens.Any(i => i.Produto.VeiculoNovo?.TipoOperacao == TipoOperacaoVeiculo.FaturamentoDireto);
        var soma = (t.ValorProdutos ?? 0) - (t.Desconto ?? 0) - (t.ValorDesonerado ?? 0)
            + (t.Frete ?? 0) + (t.Seguro ?? 0) + (t.OutrasDespesas ?? 0) + (t.ValorImpostoImportacao ?? 0) + (t.ValorIpi ?? 0)
            + (nota.Total.Issqn?.ValorServicos ?? 0)
            + nota.Itens.Where(i => i.Imposto.PisSt?.Totalizacao == IndicadorTotal.Compoe).Sum(i => i.Imposto.PisSt!.Valor ?? 0)
            + nota.Itens.Where(i => i.Imposto.CofinsSt?.Totalizacao == IndicadorTotal.Compoe).Sum(i => i.Imposto.CofinsSt!.Valor ?? 0);
        if (!faturamentoDireto)
            soma += (t.ValorSt ?? 0) + (t.ValorFcpSt ?? 0) + (t.ValorMonofasicoRetencao ?? 0) + (t.ValorIpiDevolvido ?? 0);

        var informado = t.ValorNota ?? 0;
        if (Math.Abs(informado - soma) <= ToleranciaNota || Math.Abs(informado - (soma + (t.ValorDesonerado ?? 0))) <= ToleranciaNota)
            return [];
        return [new OcorrenciaRegra($"{Raiz}/total/ICMSTot/vNF", Comparacao("vNF", informado, soma, "soma dos totais"))];
    }
}
