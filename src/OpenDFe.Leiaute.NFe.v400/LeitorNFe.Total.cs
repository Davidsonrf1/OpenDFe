namespace OpenDFe.NFe;

// A leitura dos totais (docs/LEITURA.md 3): um método por grupo, espelho do EscritorNFe.Total; o pai entra e sai
// do grupo.
internal static partial class LeitorNFe
{
    private static void Ler(LeituraNFe l, Total g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "ICMSTot":
                    g.Icms = l.Grupo(g.Icms, "Icms", Ler);
                    break;
                case "ISSQNtot":
                    g.Issqn = l.Grupo(g.Issqn, "Issqn", Ler);
                    break;
                case "retTrib":
                    g.Retencao = l.Grupo(g.Retencao, "Retencao", Ler);
                    break;
                case "ISTot":
                    g.ImpostoSeletivo = l.Grupo(g.ImpostoSeletivo, "ImpostoSeletivo", Ler);
                    break;
                case "IBSCBSTot":
                    g.IbsCbs = l.Grupo(g.IbsCbs, "IbsCbs", Ler);
                    break;
                case "vNFTot":
                    g.ValorTotalNota = l.Decimal(g.ValorTotalNota, "ValorTotalNota");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalIcms g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vICMSDeson":
                    g.ValorDesonerado = l.Decimal(g.ValorDesonerado, "ValorDesonerado");
                    break;
                case "vFCPUFDest":
                    g.ValorFcpUFDestino = l.Decimal(g.ValorFcpUFDestino, "ValorFcpUFDestino");
                    break;
                case "vICMSUFDest":
                    g.ValorIcmsUFDestino = l.Decimal(g.ValorIcmsUFDestino, "ValorIcmsUFDestino");
                    break;
                case "vICMSUFRemet":
                    g.ValorIcmsUFRemetente = l.Decimal(g.ValorIcmsUFRemetente, "ValorIcmsUFRemetente");
                    break;
                case "vFCP":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "vST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "vFCPSTRet":
                    g.ValorFcpStRetido = l.Decimal(g.ValorFcpStRetido, "ValorFcpStRetido");
                    break;
                case "qBCMono":
                    g.QuantidadeBaseMonofasico = l.Decimal(g.QuantidadeBaseMonofasico, "QuantidadeBaseMonofasico");
                    break;
                case "vICMSMono":
                    g.ValorMonofasico = l.Decimal(g.ValorMonofasico, "ValorMonofasico");
                    break;
                case "qBCMonoReten":
                    g.QuantidadeBaseRetencao = l.Decimal(g.QuantidadeBaseRetencao, "QuantidadeBaseRetencao");
                    break;
                case "vICMSMonoReten":
                    g.ValorMonofasicoRetencao = l.Decimal(g.ValorMonofasicoRetencao, "ValorMonofasicoRetencao");
                    break;
                case "qBCMonoRet":
                    g.QuantidadeBaseRetida = l.Decimal(g.QuantidadeBaseRetida, "QuantidadeBaseRetida");
                    break;
                case "vICMSMonoRet":
                    g.ValorMonofasicoRetido = l.Decimal(g.ValorMonofasicoRetido, "ValorMonofasicoRetido");
                    break;
                case "vProd":
                    g.ValorProdutos = l.Decimal(g.ValorProdutos, "ValorProdutos");
                    break;
                case "vFrete":
                    g.Frete = l.Decimal(g.Frete, "Frete");
                    break;
                case "vSeg":
                    g.Seguro = l.Decimal(g.Seguro, "Seguro");
                    break;
                case "vDesc":
                    g.Desconto = l.Decimal(g.Desconto, "Desconto");
                    break;
                case "vII":
                    g.ValorImpostoImportacao = l.Decimal(g.ValorImpostoImportacao, "ValorImpostoImportacao");
                    break;
                case "vIPI":
                    g.ValorIpi = l.Decimal(g.ValorIpi, "ValorIpi");
                    break;
                case "vIPIDevol":
                    g.ValorIpiDevolvido = l.Decimal(g.ValorIpiDevolvido, "ValorIpiDevolvido");
                    break;
                case "vPIS":
                    g.ValorPis = l.Decimal(g.ValorPis, "ValorPis");
                    break;
                case "vCOFINS":
                    g.ValorCofins = l.Decimal(g.ValorCofins, "ValorCofins");
                    break;
                case "vOutro":
                    g.OutrasDespesas = l.Decimal(g.OutrasDespesas, "OutrasDespesas");
                    break;
                case "vNF":
                    g.ValorNota = l.Decimal(g.ValorNota, "ValorNota");
                    break;
                case "vTotTrib":
                    g.ValorTotalTributos = l.Decimal(g.ValorTotalTributos, "ValorTotalTributos");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalIssqn g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vServ":
                    g.ValorServicos = l.Decimal(g.ValorServicos, "ValorServicos");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "vISS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vPIS":
                    g.ValorPis = l.Decimal(g.ValorPis, "ValorPis");
                    break;
                case "vCOFINS":
                    g.ValorCofins = l.Decimal(g.ValorCofins, "ValorCofins");
                    break;
                case "dCompet":
                    g.Competencia = l.Data(g.Competencia, "Competencia");
                    break;
                case "vDeducao":
                    g.Deducao = l.Decimal(g.Deducao, "Deducao");
                    break;
                case "vOutro":
                    g.OutrasRetencoes = l.Decimal(g.OutrasRetencoes, "OutrasRetencoes");
                    break;
                case "vDescIncond":
                    g.DescontoIncondicionado = l.Decimal(g.DescontoIncondicionado, "DescontoIncondicionado");
                    break;
                case "vDescCond":
                    g.DescontoCondicionado = l.Decimal(g.DescontoCondicionado, "DescontoCondicionado");
                    break;
                case "vISSRet":
                    g.Retencao = l.Decimal(g.Retencao, "Retencao");
                    break;
                case "cRegTrib":
                    g.RegimeEspecial = l.Enumeracao(g.RegimeEspecial, "RegimeEspecial");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, RetencaoTributos g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vRetPIS":
                    g.Pis = l.Decimal(g.Pis, "Pis");
                    break;
                case "vRetCOFINS":
                    g.Cofins = l.Decimal(g.Cofins, "Cofins");
                    break;
                case "vRetCSLL":
                    g.Csll = l.Decimal(g.Csll, "Csll");
                    break;
                case "vBCIRRF":
                    g.BaseCalculoIrrf = l.Decimal(g.BaseCalculoIrrf, "BaseCalculoIrrf");
                    break;
                case "vIRRF":
                    g.Irrf = l.Decimal(g.Irrf, "Irrf");
                    break;
                case "vBCRetPrev":
                    g.BaseCalculoPrevidencia = l.Decimal(g.BaseCalculoPrevidencia, "BaseCalculoPrevidencia");
                    break;
                case "vRetPrev":
                    g.Previdencia = l.Decimal(g.Previdencia, "Previdencia");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalImpostoSeletivo g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vIS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalIbsCbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBCIBSCBS":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "gIBS":
                    g.Ibs = l.Grupo(g.Ibs, "Ibs", Ler);
                    break;
                case "gCBS":
                    g.Cbs = l.Grupo(g.Cbs, "Cbs", Ler);
                    break;
                case "gMono":
                    g.Monofasia = l.Grupo(g.Monofasia, "Monofasia", Ler);
                    break;
                case "gEstornoCred":
                    g.EstornoCredito = l.Grupo(g.EstornoCredito, "EstornoCredito", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalIbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "gIBSUF":
                    g.UF = l.Grupo(g.UF, "UF", Ler);
                    break;
                case "gIBSMun":
                    g.Municipio = l.Grupo(g.Municipio, "Municipio", Ler);
                    break;
                case "vIBS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vCredPres":
                    g.CreditoPresumido = l.Decimal(g.CreditoPresumido, "CreditoPresumido");
                    break;
                case "vCredPresCondSus":
                    g.CreditoPresumidoCondicaoSuspensiva = l.Decimal(g.CreditoPresumidoCondicaoSuspensiva, "CreditoPresumidoCondicaoSuspensiva");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalCbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vDif":
                    g.Diferimento = l.Decimal(g.Diferimento, "Diferimento");
                    break;
                case "vDevTrib":
                    g.Devolucao = l.Decimal(g.Devolucao, "Devolucao");
                    break;
                case "vCBS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vCredPres":
                    g.CreditoPresumido = l.Decimal(g.CreditoPresumido, "CreditoPresumido");
                    break;
                case "vCredPresCondSus":
                    g.CreditoPresumidoCondicaoSuspensiva = l.Decimal(g.CreditoPresumidoCondicaoSuspensiva, "CreditoPresumidoCondicaoSuspensiva");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalMonofasia g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vIBSMono":
                    g.Ibs = l.Decimal(g.Ibs, "Ibs");
                    break;
                case "vCBSMono":
                    g.Cbs = l.Decimal(g.Cbs, "Cbs");
                    break;
                case "vIBSMonoReten":
                    g.IbsRetencao = l.Decimal(g.IbsRetencao, "IbsRetencao");
                    break;
                case "vCBSMonoReten":
                    g.CbsRetencao = l.Decimal(g.CbsRetencao, "CbsRetencao");
                    break;
                case "vIBSMonoRet":
                    g.IbsRetido = l.Decimal(g.IbsRetido, "IbsRetido");
                    break;
                case "vCBSMonoRet":
                    g.CbsRetido = l.Decimal(g.CbsRetido, "CbsRetido");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, EstornoCredito g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vIBSEstCred":
                    g.ValorIbs = l.Decimal(g.ValorIbs, "ValorIbs");
                    break;
                case "vCBSEstCred":
                    g.ValorCbs = l.Decimal(g.ValorCbs, "ValorCbs");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalIbsUF g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vDif":
                    g.Diferimento = l.Decimal(g.Diferimento, "Diferimento");
                    break;
                case "vDevTrib":
                    g.Devolucao = l.Decimal(g.Devolucao, "Devolucao");
                    break;
                case "vIBSUF":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TotalIbsMunicipio g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vDif":
                    g.Diferimento = l.Decimal(g.Diferimento, "Diferimento");
                    break;
                case "vDevTrib":
                    g.Devolucao = l.Decimal(g.Devolucao, "Devolucao");
                    break;
                case "vIBSMun":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }
}
