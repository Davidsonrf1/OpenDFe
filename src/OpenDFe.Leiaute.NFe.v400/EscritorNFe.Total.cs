namespace OpenDFe.NFe;

// A escrita dos totais (MODELO.md 11.7): um método por grupo, na ordem do XSD; o pai abre e fecha o grupo.
internal static partial class EscritorNFe
{
    private static void Escrever(EscritaNFe e, Total g)
    {
        if (g.Icms is { } icms)
        {
            e.Abrir("ICMSTot", "Icms");
            Escrever(e, icms);
            e.Fechar();
        }
        if (g.Issqn is { } issqn)
        {
            e.Abrir("ISSQNtot", "Issqn");
            Escrever(e, issqn);
            e.Fechar();
        }
        if (g.Retencao is { } retencao)
        {
            e.Abrir("retTrib", "Retencao");
            Escrever(e, retencao);
            e.Fechar();
        }
        if (g.ImpostoSeletivo is { } impostoSeletivo)
        {
            e.Abrir("ISTot", "ImpostoSeletivo");
            Escrever(e, impostoSeletivo);
            e.Fechar();
        }
        if (g.IbsCbs is { } ibsCbs)
        {
            e.Abrir("IBSCBSTot", "IbsCbs");
            Escrever(e, ibsCbs);
            e.Fechar();
        }
        e.Decimal("vNFTot", "ValorTotalNota", g.ValorTotalNota, 2);
    }

    private static void Escrever(EscritaNFe e, TotalIcms g)
    {
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Decimal("vICMSDeson", "ValorDesonerado", g.ValorDesonerado, 2);
        e.Decimal("vFCPUFDest", "ValorFcpUFDestino", g.ValorFcpUFDestino, 2);
        e.Decimal("vICMSUFDest", "ValorIcmsUFDestino", g.ValorIcmsUFDestino, 2);
        e.Decimal("vICMSUFRemet", "ValorIcmsUFRemetente", g.ValorIcmsUFRemetente, 2);
        e.Decimal("vFCP", "ValorFcp", g.ValorFcp, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("vST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("vFCPSTRet", "ValorFcpStRetido", g.ValorFcpStRetido, 2);
        e.Decimal("qBCMono", "QuantidadeBaseMonofasico", g.QuantidadeBaseMonofasico, 2);
        e.Decimal("vICMSMono", "ValorMonofasico", g.ValorMonofasico, 2);
        e.Decimal("qBCMonoReten", "QuantidadeBaseRetencao", g.QuantidadeBaseRetencao, 2);
        e.Decimal("vICMSMonoReten", "ValorMonofasicoRetencao", g.ValorMonofasicoRetencao, 2);
        e.Decimal("qBCMonoRet", "QuantidadeBaseRetida", g.QuantidadeBaseRetida, 2);
        e.Decimal("vICMSMonoRet", "ValorMonofasicoRetido", g.ValorMonofasicoRetido, 2);
        e.Decimal("vProd", "ValorProdutos", g.ValorProdutos, 2);
        e.Decimal("vFrete", "Frete", g.Frete, 2);
        e.Decimal("vSeg", "Seguro", g.Seguro, 2);
        e.Decimal("vDesc", "Desconto", g.Desconto, 2);
        e.Decimal("vII", "ValorImpostoImportacao", g.ValorImpostoImportacao, 2);
        e.Decimal("vIPI", "ValorIpi", g.ValorIpi, 2);
        e.Decimal("vIPIDevol", "ValorIpiDevolvido", g.ValorIpiDevolvido, 2);
        e.Decimal("vPIS", "ValorPis", g.ValorPis, 2);
        e.Decimal("vCOFINS", "ValorCofins", g.ValorCofins, 2);
        e.Decimal("vOutro", "OutrasDespesas", g.OutrasDespesas, 2);
        e.Decimal("vNF", "ValorNota", g.ValorNota, 2);
        e.Decimal("vTotTrib", "ValorTotalTributos", g.ValorTotalTributos, 2);
    }

    private static void Escrever(EscritaNFe e, TotalIssqn g)
    {
        e.Decimal("vServ", "ValorServicos", g.ValorServicos, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("vISS", "Valor", g.Valor, 2);
        e.Decimal("vPIS", "ValorPis", g.ValorPis, 2);
        e.Decimal("vCOFINS", "ValorCofins", g.ValorCofins, 2);
        e.Data("dCompet", "Competencia", g.Competencia);
        e.Decimal("vDeducao", "Deducao", g.Deducao, 2);
        e.Decimal("vOutro", "OutrasRetencoes", g.OutrasRetencoes, 2);
        e.Decimal("vDescIncond", "DescontoIncondicionado", g.DescontoIncondicionado, 2);
        e.Decimal("vDescCond", "DescontoCondicionado", g.DescontoCondicionado, 2);
        e.Decimal("vISSRet", "Retencao", g.Retencao, 2);
        e.Inteiro("cRegTrib", "RegimeEspecial", (int?)g.RegimeEspecial, 1);
    }

    private static void Escrever(EscritaNFe e, RetencaoTributos g)
    {
        e.Decimal("vRetPIS", "Pis", g.Pis, 2);
        e.Decimal("vRetCOFINS", "Cofins", g.Cofins, 2);
        e.Decimal("vRetCSLL", "Csll", g.Csll, 2);
        e.Decimal("vBCIRRF", "BaseCalculoIrrf", g.BaseCalculoIrrf, 2);
        e.Decimal("vIRRF", "Irrf", g.Irrf, 2);
        e.Decimal("vBCRetPrev", "BaseCalculoPrevidencia", g.BaseCalculoPrevidencia, 2);
        e.Decimal("vRetPrev", "Previdencia", g.Previdencia, 2);
    }

    private static void Escrever(EscritaNFe e, TotalImpostoSeletivo g)
    {
        e.Decimal("vIS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, TotalIbsCbs g)
    {
        e.Decimal("vBCIBSCBS", "BaseCalculo", g.BaseCalculo, 2);
        if (g.Ibs is { } ibs)
        {
            e.Abrir("gIBS", "Ibs");
            Escrever(e, ibs);
            e.Fechar();
        }
        if (g.Cbs is { } cbs)
        {
            e.Abrir("gCBS", "Cbs");
            Escrever(e, cbs);
            e.Fechar();
        }
        if (g.Monofasia is { } monofasia)
        {
            e.Abrir("gMono", "Monofasia");
            Escrever(e, monofasia);
            e.Fechar();
        }
        if (g.EstornoCredito is { } estornoCredito)
        {
            e.Abrir("gEstornoCred", "EstornoCredito");
            Escrever(e, estornoCredito);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, TotalIbs g)
    {
        if (g.UF is { } uF)
        {
            e.Abrir("gIBSUF", "UF");
            Escrever(e, uF);
            e.Fechar();
        }
        if (g.Municipio is { } municipio)
        {
            e.Abrir("gIBSMun", "Municipio");
            Escrever(e, municipio);
            e.Fechar();
        }
        e.Decimal("vIBS", "Valor", g.Valor, 2);
        e.Decimal("vCredPres", "CreditoPresumido", g.CreditoPresumido, 2);
        e.Decimal("vCredPresCondSus", "CreditoPresumidoCondicaoSuspensiva", g.CreditoPresumidoCondicaoSuspensiva, 2);
    }

    private static void Escrever(EscritaNFe e, TotalCbs g)
    {
        e.Decimal("vDif", "Diferimento", g.Diferimento, 2);
        e.Decimal("vDevTrib", "Devolucao", g.Devolucao, 2);
        e.Decimal("vCBS", "Valor", g.Valor, 2);
        e.Decimal("vCredPres", "CreditoPresumido", g.CreditoPresumido, 2);
        e.Decimal("vCredPresCondSus", "CreditoPresumidoCondicaoSuspensiva", g.CreditoPresumidoCondicaoSuspensiva, 2);
    }

    private static void Escrever(EscritaNFe e, TotalMonofasia g)
    {
        e.Decimal("vIBSMono", "Ibs", g.Ibs, 2);
        e.Decimal("vCBSMono", "Cbs", g.Cbs, 2);
        e.Decimal("vIBSMonoReten", "IbsRetencao", g.IbsRetencao, 2);
        e.Decimal("vCBSMonoReten", "CbsRetencao", g.CbsRetencao, 2);
        e.Decimal("vIBSMonoRet", "IbsRetido", g.IbsRetido, 2);
        e.Decimal("vCBSMonoRet", "CbsRetido", g.CbsRetido, 2);
    }

    private static void Escrever(EscritaNFe e, EstornoCredito g)
    {
        e.Decimal("vIBSEstCred", "ValorIbs", g.ValorIbs, 2);
        e.Decimal("vCBSEstCred", "ValorCbs", g.ValorCbs, 2);
    }

    private static void Escrever(EscritaNFe e, TotalIbsUF g)
    {
        e.Decimal("vDif", "Diferimento", g.Diferimento, 2);
        e.Decimal("vDevTrib", "Devolucao", g.Devolucao, 2);
        e.Decimal("vIBSUF", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, TotalIbsMunicipio g)
    {
        e.Decimal("vDif", "Diferimento", g.Diferimento, 2);
        e.Decimal("vDevTrib", "Devolucao", g.Devolucao, 2);
        e.Decimal("vIBSMun", "Valor", g.Valor, 2);
    }
}
