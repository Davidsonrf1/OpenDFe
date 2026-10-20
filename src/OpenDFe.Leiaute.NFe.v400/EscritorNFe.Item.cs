namespace OpenDFe.NFe;

// A escrita dos itens e dos tributos (MODELO.md 11.2 a 11.6): um método por grupo, na ordem do XSD; o pai abre e fecha o grupo.
internal static partial class EscritorNFe
{
    private static void Escrever(EscritaNFe e, Item g)
    {
        if (g.Produto is { } produto)
        {
            e.Abrir("prod", "Produto");
            Escrever(e, produto);
            e.Fechar();
        }
        if (g.Imposto is { } imposto)
        {
            e.Abrir("imposto", "Imposto");
            Escrever(e, imposto);
            e.Fechar();
        }
        if (g.ImpostoDevolvido is { } impostoDevolvido)
        {
            e.Abrir("impostoDevol", "ImpostoDevolvido");
            Escrever(e, impostoDevolvido);
            e.Fechar();
        }
        e.Texto("infAdProd", "InformacoesAdicionais", g.InformacoesAdicionais);
        if (g.Observacoes is { } observacoes)
        {
            e.Abrir("obsItem", "Observacoes");
            Escrever(e, observacoes);
            e.Fechar();
        }
        e.Decimal("vItem", "ValorTotal", g.ValorTotal, 2);
        if (g.Referencia is { } referencia)
        {
            e.Abrir("DFeReferenciado", "Referencia");
            Escrever(e, referencia);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Produto g)
    {
        e.Escolha(("VeiculoNovo", g.VeiculoNovo is not null), ("Medicamento", g.Medicamento is not null), ("Armas", g.Armas.Count > 0), ("Combustivel", g.Combustivel is not null), ("Recopi", !string.IsNullOrEmpty(g.Recopi)));
        e.Texto("cProd", "Codigo", g.Codigo);
        e.Texto("cEAN", "Gtin", g.Gtin);
        e.Texto("cBarra", "CodigoBarras", g.CodigoBarras);
        e.Texto("xProd", "Descricao", g.Descricao);
        e.Texto("NCM", "Ncm", g.Ncm);
        for (var i = 0; i < g.Nve.Count; i++)
            e.Texto("NVE", "Nve", g.Nve[i], i, g.Nve.Count);
        e.Texto("CEST", "Cest", g.Cest);
        e.Literal("indEscala", "Escala", Literais.IndicadorEscala(g.Escala));
        e.Texto("CNPJFab", "CnpjFabricante", g.CnpjFabricante);
        e.Texto("cBenef", "CodigoBeneficio", g.CodigoBeneficio);
        for (var i = 0; i < g.CreditosPresumidos.Count; i++)
        {
            e.Abrir("gCred", "CreditosPresumidos", i, g.CreditosPresumidos.Count);
            Escrever(e, g.CreditosPresumidos[i]);
            e.Fechar();
        }
        e.Inteiro("tpCredPresIBSZFM", "ClassificacaoZfm", (int?)g.ClassificacaoZfm, 1);
        e.Texto("EXTIPI", "ExTipi", g.ExTipi);
        e.Inteiro("CFOP", "Cfop", g.Cfop, 0);
        e.Texto("uCom", "UnidadeComercial", g.UnidadeComercial);
        e.Decimal("qCom", "QuantidadeComercial", g.QuantidadeComercial, 2);
        e.Decimal("vUnCom", "ValorUnitarioComercial", g.ValorUnitarioComercial, 2);
        e.Decimal("vProd", "Valor", g.Valor, 2);
        e.Texto("cEANTrib", "GtinTributavel", g.GtinTributavel);
        e.Texto("cBarraTrib", "CodigoBarrasTributavel", g.CodigoBarrasTributavel);
        e.Texto("uTrib", "UnidadeTributavel", g.UnidadeTributavel);
        e.Decimal("qTrib", "QuantidadeTributavel", g.QuantidadeTributavel, 2);
        e.Decimal("vUnTrib", "ValorUnitarioTributavel", g.ValorUnitarioTributavel, 2);
        e.Decimal("vFrete", "Frete", g.Frete, 2);
        e.Decimal("vSeg", "Seguro", g.Seguro, 2);
        e.Decimal("vDesc", "Desconto", g.Desconto, 2);
        e.Decimal("vOutro", "OutrasDespesas", g.OutrasDespesas, 2);
        e.Inteiro("indTot", "Totalizacao", (int?)g.Totalizacao, 1);
        e.Inteiro("indBemMovelUsado", "BemMovelUsado", (int?)g.BemMovelUsado, 1);
        for (var i = 0; i < g.Importacoes.Count; i++)
        {
            e.Abrir("DI", "Importacoes", i, g.Importacoes.Count);
            Escrever(e, g.Importacoes[i]);
            e.Fechar();
        }
        for (var i = 0; i < g.Exportacoes.Count; i++)
        {
            e.Abrir("detExport", "Exportacoes", i, g.Exportacoes.Count);
            Escrever(e, g.Exportacoes[i]);
            e.Fechar();
        }
        e.Texto("xPed", "Pedido", g.Pedido);
        e.Texto("nItemPed", "ItemPedido", g.ItemPedido);
        e.Texto("nFCI", "NumeroFci", g.NumeroFci);
        for (var i = 0; i < g.Rastros.Count; i++)
        {
            e.Abrir("rastro", "Rastros", i, g.Rastros.Count);
            Escrever(e, g.Rastros[i]);
            e.Fechar();
        }
        if (g.Nff is { } nff)
        {
            e.Abrir("infProdNFF", "Nff");
            Escrever(e, nff);
            e.Fechar();
        }
        if (g.Embalagem is { } embalagem)
        {
            e.Abrir("infProdEmb", "Embalagem");
            Escrever(e, embalagem);
            e.Fechar();
        }
        if (g.VeiculoNovo is { } veiculoNovo)
        {
            e.Abrir("veicProd", "VeiculoNovo");
            Escrever(e, veiculoNovo);
            e.Fechar();
        }
        if (g.Medicamento is { } medicamento)
        {
            e.Abrir("med", "Medicamento");
            Escrever(e, medicamento);
            e.Fechar();
        }
        for (var i = 0; i < g.Armas.Count; i++)
        {
            e.Abrir("arma", "Armas", i, g.Armas.Count);
            Escrever(e, g.Armas[i]);
            e.Fechar();
        }
        if (g.Combustivel is { } combustivel)
        {
            e.Abrir("comb", "Combustivel");
            Escrever(e, combustivel);
            e.Fechar();
        }
        e.Texto("nRECOPI", "Recopi", g.Recopi);
    }

    private static void Escrever(EscritaNFe e, Imposto g)
    {
        e.Escolha(("Icms, ImpostoImportacao", g.Icms is not null || g.ImpostoImportacao is not null), ("Issqn", g.Issqn is not null));
        e.Decimal("vTotTrib", "ValorTotalTributos", g.ValorTotalTributos, 2);
        if (g.Icms is { } icms)
        {
            e.Abrir("ICMS", "Icms");
            switch (icms)
            {
                case Icms00 x:
                    e.Abrir("ICMS00", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms02 x:
                    e.Abrir("ICMS02", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms10 x:
                    e.Abrir("ICMS10", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms15 x:
                    e.Abrir("ICMS15", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms20 x:
                    e.Abrir("ICMS20", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms30 x:
                    e.Abrir("ICMS30", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms40 x:
                    e.Abrir("ICMS40", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms51 x:
                    e.Abrir("ICMS51", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms53 x:
                    e.Abrir("ICMS53", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms60 x:
                    e.Abrir("ICMS60", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms61 x:
                    e.Abrir("ICMS61", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms70 x:
                    e.Abrir("ICMS70", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case Icms90 x:
                    e.Abrir("ICMS90", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsPartilha x:
                    e.Abrir("ICMSPart", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsRepasse x:
                    e.Abrir("ICMSST", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsSn101 x:
                    e.Abrir("ICMSSN101", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsSn102 x:
                    e.Abrir("ICMSSN102", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsSn201 x:
                    e.Abrir("ICMSSN201", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsSn202 x:
                    e.Abrir("ICMSSN202", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsSn500 x:
                    e.Abrir("ICMSSN500", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case IcmsSn900 x:
                    e.Abrir("ICMSSN900", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
            }
            e.Fechar();
        }
        if (g.Ipi is { } ipi)
        {
            e.Abrir("IPI", "Ipi");
            Escrever(e, ipi);
            e.Fechar();
        }
        if (g.ImpostoImportacao is { } impostoImportacao)
        {
            e.Abrir("II", "ImpostoImportacao");
            Escrever(e, impostoImportacao);
            e.Fechar();
        }
        if (g.Issqn is { } issqn)
        {
            e.Abrir("ISSQN", "Issqn");
            Escrever(e, issqn);
            e.Fechar();
        }
        if (g.Pis is { } pis)
        {
            e.Abrir("PIS", "Pis");
            switch (pis)
            {
                case PisAliquota x:
                    e.Abrir("PISAliq", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case PisNaoTributado x:
                    e.Abrir("PISNT", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case PisOutros x:
                    e.Abrir("PISOutr", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case PisQuantidade x:
                    e.Abrir("PISQtde", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
            }
            e.Fechar();
        }
        if (g.PisSt is { } pisSt)
        {
            e.Abrir("PISST", "PisSt");
            Escrever(e, pisSt);
            e.Fechar();
        }
        if (g.Cofins is { } cofins)
        {
            e.Abrir("COFINS", "Cofins");
            switch (cofins)
            {
                case CofinsAliquota x:
                    e.Abrir("COFINSAliq", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case CofinsNaoTributado x:
                    e.Abrir("COFINSNT", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case CofinsOutros x:
                    e.Abrir("COFINSOutr", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case CofinsQuantidade x:
                    e.Abrir("COFINSQtde", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
            }
            e.Fechar();
        }
        if (g.CofinsSt is { } cofinsSt)
        {
            e.Abrir("COFINSST", "CofinsSt");
            Escrever(e, cofinsSt);
            e.Fechar();
        }
        if (g.IcmsUFDestino is { } icmsUFDestino)
        {
            e.Abrir("ICMSUFDest", "IcmsUFDestino");
            Escrever(e, icmsUFDestino);
            e.Fechar();
        }
        if (g.ImpostoSeletivo is { } impostoSeletivo)
        {
            e.Abrir("IS", "ImpostoSeletivo");
            Escrever(e, impostoSeletivo);
            e.Fechar();
        }
        if (g.IbsCbs is { } ibsCbs)
        {
            e.Abrir("IBSCBS", "IbsCbs");
            Escrever(e, ibsCbs);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, ImpostoDevolvido g)
    {
        e.Decimal("pDevol", "Percentual", g.Percentual, 2);
        if (g.Ipi is { } ipi)
        {
            e.Abrir("IPI", "Ipi");
            Escrever(e, ipi);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, ObservacoesItem g)
    {
        if (g.Contribuinte is { } contribuinte)
        {
            e.Abrir("obsCont", "Contribuinte");
            Escrever(e, contribuinte);
            e.Fechar();
        }
        if (g.Fisco is { } fisco)
        {
            e.Abrir("obsFisco", "Fisco");
            Escrever(e, fisco);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, ItemReferenciado g)
    {
        e.Texto("chaveAcesso", "Chave", g.Chave);
        e.Inteiro("nItem", "NumeroItem", g.NumeroItem, 0);
    }

    private static void Escrever(EscritaNFe e, CreditoPresumidoIcms g)
    {
        e.Texto("cCredPresumido", "Codigo", g.Codigo);
        e.Decimal("pCredPresumido", "Percentual", g.Percentual, 2);
        e.Decimal("vCredPresumido", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, DeclaracaoImportacao g)
    {
        e.Escolha(("Cnpj", !string.IsNullOrEmpty(g.Cnpj)), ("Cpf", !string.IsNullOrEmpty(g.Cpf)));
        e.Texto("nDI", "Numero", g.Numero);
        e.Data("dDI", "Data", g.Data);
        e.Texto("xLocDesemb", "LocalDesembaraco", g.LocalDesembaraco);
        e.Sigla("UFDesemb", "UFDesembaraco", g.UFDesembaraco, false);
        e.Data("dDesemb", "DataDesembaraco", g.DataDesembaraco);
        e.Inteiro("tpViaTransp", "ViaTransporte", (int?)g.ViaTransporte, 0);
        e.Decimal("vAFRMM", "ValorAfrmm", g.ValorAfrmm, 2);
        e.Inteiro("tpIntermedio", "FormaImportacao", (int?)g.FormaImportacao, 1);
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("CPF", "Cpf", g.Cpf);
        e.Sigla("UFTerceiro", "UFTerceiro", g.UFTerceiro, false);
        e.Texto("cExportador", "CodigoExportador", g.CodigoExportador);
        for (var i = 0; i < g.Adicoes.Count; i++)
        {
            e.Abrir("adi", "Adicoes", i, g.Adicoes.Count);
            Escrever(e, g.Adicoes[i]);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, DetalheExportacao g)
    {
        e.Texto("nDraw", "Drawback", g.Drawback);
        if (g.Indireta is { } indireta)
        {
            e.Abrir("exportInd", "Indireta");
            Escrever(e, indireta);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Rastro g)
    {
        e.Texto("nLote", "Lote", g.Lote);
        e.Decimal("qLote", "Quantidade", g.Quantidade, 3);
        e.Data("dFab", "Fabricacao", g.Fabricacao);
        e.Data("dVal", "Validade", g.Validade);
        e.Texto("cAgreg", "CodigoAgregacao", g.CodigoAgregacao);
    }

    private static void Escrever(EscritaNFe e, ProdutoNff g)
    {
        e.Texto("cProdFisco", "CodigoFisco", g.CodigoFisco);
        e.Inteiro("cOperNFF", "CodigoOperacao", g.CodigoOperacao, 0);
    }

    private static void Escrever(EscritaNFe e, EmbalagemProduto g)
    {
        e.Texto("xEmb", "Descricao", g.Descricao);
        e.Decimal("qVolEmb", "Volume", g.Volume, 3);
        e.Texto("uEmb", "Unidade", g.Unidade);
    }

    private static void Escrever(EscritaNFe e, VeiculoNovo g)
    {
        e.Inteiro("tpOp", "TipoOperacao", (int?)g.TipoOperacao, 1);
        e.Texto("chassi", "Chassi", g.Chassi);
        e.Texto("cCor", "CodigoCor", g.CodigoCor);
        e.Texto("xCor", "Cor", g.Cor);
        e.Texto("pot", "Potencia", g.Potencia);
        e.Texto("cilin", "Cilindrada", g.Cilindrada);
        e.Texto("pesoL", "PesoLiquido", g.PesoLiquido);
        e.Texto("pesoB", "PesoBruto", g.PesoBruto);
        e.Texto("nSerie", "Serie", g.Serie);
        e.Texto("tpComb", "TipoCombustivel", g.TipoCombustivel);
        e.Texto("nMotor", "Motor", g.Motor);
        e.Texto("CMT", "CapacidadeTracao", g.CapacidadeTracao);
        e.Texto("dist", "DistanciaEixos", g.DistanciaEixos);
        e.Inteiro("anoMod", "AnoModelo", g.AnoModelo, 4);
        e.Inteiro("anoFab", "AnoFabricacao", g.AnoFabricacao, 4);
        e.Texto("tpPint", "TipoPintura", g.TipoPintura);
        e.Texto("tpVeic", "TipoVeiculo", g.TipoVeiculo);
        e.Inteiro("espVeic", "EspecieVeiculo", g.EspecieVeiculo, 1);
        e.Literal("VIN", "Vin", Literais.CondicaoVin(g.Vin));
        e.Inteiro("condVeic", "Condicao", (int?)g.Condicao, 1);
        e.Texto("cMod", "MarcaModelo", g.MarcaModelo);
        e.Texto("cCorDENATRAN", "CorDenatran", g.CorDenatran);
        e.Inteiro("lota", "Lotacao", g.Lotacao, 0);
        e.Inteiro("tpRest", "Restricao", (int?)g.Restricao, 1);
    }

    private static void Escrever(EscritaNFe e, Medicamento g)
    {
        e.Texto("cProdANVISA", "CodigoAnvisa", g.CodigoAnvisa);
        e.Texto("xMotivoIsencao", "MotivoIsencao", g.MotivoIsencao);
        e.Decimal("vPMC", "PrecoMaximoConsumidor", g.PrecoMaximoConsumidor, 2);
    }

    private static void Escrever(EscritaNFe e, Arma g)
    {
        e.Inteiro("tpArma", "Tipo", (int?)g.Tipo, 1);
        e.Texto("nSerie", "Serie", g.Serie);
        e.Texto("nCano", "SerieCano", g.SerieCano);
        e.Texto("descr", "Descricao", g.Descricao);
    }

    private static void Escrever(EscritaNFe e, Combustivel g)
    {
        e.Texto("cProdANP", "CodigoAnp", g.CodigoAnp);
        e.Texto("descANP", "DescricaoAnp", g.DescricaoAnp);
        e.Decimal("pGLP", "PercentualGlp", g.PercentualGlp, 2);
        e.Decimal("pGNn", "PercentualGasNacional", g.PercentualGasNacional, 2);
        e.Decimal("pGNi", "PercentualGasImportado", g.PercentualGasImportado, 2);
        e.Decimal("vPart", "ValorPartida", g.ValorPartida, 2);
        e.Texto("CODIF", "Codif", g.Codif);
        e.Decimal("qTemp", "QuantidadeTemperaturaAmbiente", g.QuantidadeTemperaturaAmbiente, 4);
        e.Sigla("UFCons", "UFConsumo", g.UFConsumo, g.Exterior);
        if (g.Cide is { } cide)
        {
            e.Abrir("CIDE", "Cide");
            Escrever(e, cide);
            e.Fechar();
        }
        if (g.Encerrante is { } encerrante)
        {
            e.Abrir("encerrante", "Encerrante");
            Escrever(e, encerrante);
            e.Fechar();
        }
        e.Decimal("pBio", "PercentualBiodiesel", g.PercentualBiodiesel, 2);
        for (var i = 0; i < g.Origens.Count; i++)
        {
            e.Abrir("origComb", "Origens", i, g.Origens.Count);
            Escrever(e, g.Origens[i]);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Icms00 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.Tributada), 2);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Decimal("pFCP", "PercentualFcp", g.PercentualFcp, 2);
        e.Decimal("vFCP", "ValorFcp", g.ValorFcp, 2);
    }

    private static void Escrever(EscritaNFe e, Icms02 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.MonofasicaPropria), 2);
        e.Decimal("qBCMono", "QuantidadeBaseMonofasico", g.QuantidadeBaseMonofasico, 2);
        e.Decimal("adRemICMS", "AliquotaAdRem", g.AliquotaAdRem, 2);
        e.Decimal("vICMSMono", "ValorMonofasico", g.ValorMonofasico, 2);
    }

    private static void Escrever(EscritaNFe e, Icms10 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.TributadaSubstituicao), 2);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Decimal("vBCFCP", "BaseCalculoFcp", g.BaseCalculoFcp, 2);
        e.Decimal("pFCP", "PercentualFcp", g.PercentualFcp, 2);
        e.Decimal("vFCP", "ValorFcp", g.ValorFcp, 2);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("vICMSSTDeson", "ValorStDesonerado", g.ValorStDesonerado, 2);
        e.Inteiro("motDesICMSST", "MotivoDesoneracaoSt", (int?)g.MotivoDesoneracaoSt, 0);
    }

    private static void Escrever(EscritaNFe e, Icms15 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.MonofasicaPropriaRetencao), 2);
        e.Decimal("qBCMono", "QuantidadeBaseMonofasico", g.QuantidadeBaseMonofasico, 2);
        e.Decimal("adRemICMS", "AliquotaAdRem", g.AliquotaAdRem, 2);
        e.Decimal("vICMSMono", "ValorMonofasico", g.ValorMonofasico, 2);
        e.Decimal("qBCMonoReten", "QuantidadeBaseRetencao", g.QuantidadeBaseRetencao, 2);
        e.Decimal("adRemICMSReten", "AliquotaAdRemRetencao", g.AliquotaAdRemRetencao, 2);
        e.Decimal("vICMSMonoReten", "ValorMonofasicoRetencao", g.ValorMonofasicoRetencao, 2);
        e.Decimal("pRedAdRem", "PercentualReducaoAdRem", g.PercentualReducaoAdRem, 2);
        e.Inteiro("motRedAdRem", "MotivoReducaoAdRem", (int?)g.MotivoReducaoAdRem, 1);
    }

    private static void Escrever(EscritaNFe e, Icms20 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.ReducaoBase), 2);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("pRedBC", "PercentualReducaoBase", g.PercentualReducaoBase, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Decimal("vBCFCP", "BaseCalculoFcp", g.BaseCalculoFcp, 2);
        e.Decimal("pFCP", "PercentualFcp", g.PercentualFcp, 2);
        e.Decimal("vFCP", "ValorFcp", g.ValorFcp, 2);
        e.Decimal("vICMSDeson", "ValorDesonerado", g.ValorDesonerado, 2);
        e.Inteiro("motDesICMS", "MotivoDesoneracao", (int?)g.MotivoDesoneracao, 0);
        e.Inteiro("indDeduzDeson", "DeducaoDesoneracao", (int?)g.DeducaoDesoneracao, 1);
    }

    private static void Escrever(EscritaNFe e, Icms30 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.IsentaSubstituicao), 2);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("vICMSDeson", "ValorDesonerado", g.ValorDesonerado, 2);
        e.Inteiro("motDesICMS", "MotivoDesoneracao", (int?)g.MotivoDesoneracao, 1);
        e.Inteiro("indDeduzDeson", "DeducaoDesoneracao", (int?)g.DeducaoDesoneracao, 1);
    }

    private static void Escrever(EscritaNFe e, Icms40 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Decimal("vICMSDeson", "ValorDesonerado", g.ValorDesonerado, 2);
        e.Inteiro("motDesICMS", "MotivoDesoneracao", (int?)g.MotivoDesoneracao, 0);
        e.Inteiro("indDeduzDeson", "DeducaoDesoneracao", (int?)g.DeducaoDesoneracao, 1);
    }

    private static void Escrever(EscritaNFe e, Icms51 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.Diferimento), 2);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("pRedBC", "PercentualReducaoBase", g.PercentualReducaoBase, 2);
        e.Texto("cBenefRBC", "CodigoBeneficioReducao", g.CodigoBeneficioReducao);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMSOp", "ValorOperacao", g.ValorOperacao, 2);
        e.Decimal("pDif", "PercentualDiferimento", g.PercentualDiferimento, 2);
        e.Decimal("vICMSDif", "ValorDiferido", g.ValorDiferido, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Decimal("vBCFCP", "BaseCalculoFcp", g.BaseCalculoFcp, 2);
        e.Decimal("pFCP", "PercentualFcp", g.PercentualFcp, 2);
        e.Decimal("vFCP", "ValorFcp", g.ValorFcp, 2);
        e.Decimal("pFCPDif", "PercentualDiferimentoFcp", g.PercentualDiferimentoFcp, 2);
        e.Decimal("vFCPDif", "ValorFcpDiferido", g.ValorFcpDiferido, 2);
        e.Decimal("vFCPEfet", "ValorFcpEfetivo", g.ValorFcpEfetivo, 2);
    }

    private static void Escrever(EscritaNFe e, Icms53 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.MonofasicaDiferida), 2);
        e.Decimal("qBCMono", "QuantidadeBaseMonofasico", g.QuantidadeBaseMonofasico, 2);
        e.Decimal("adRemICMS", "AliquotaAdRem", g.AliquotaAdRem, 2);
        e.Decimal("vICMSMonoOp", "ValorMonofasicoOperacao", g.ValorMonofasicoOperacao, 2);
        e.Decimal("pDif", "PercentualDiferimento", g.PercentualDiferimento, 2);
        e.Decimal("vICMSMonoDif", "ValorMonofasicoDiferido", g.ValorMonofasicoDiferido, 2);
        e.Decimal("vICMSMono", "ValorMonofasico", g.ValorMonofasico, 2);
        e.Decimal("qBCMonoDif", "QuantidadeBaseDiferida", g.QuantidadeBaseDiferida, 2);
        e.Decimal("adRemICMSDif", "AliquotaAdRemDiferida", g.AliquotaAdRemDiferida, 2);
    }

    private static void Escrever(EscritaNFe e, Icms60 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.CobradoAnteriormente), 2);
        e.Decimal("vBCSTRet", "BaseCalculoStRetido", g.BaseCalculoStRetido, 2);
        e.Decimal("pST", "AliquotaConsumidorFinal", g.AliquotaConsumidorFinal, 2);
        e.Decimal("vICMSSubstituto", "ValorSubstituto", g.ValorSubstituto, 2);
        e.Decimal("vICMSSTRet", "ValorStRetido", g.ValorStRetido, 2);
        e.Decimal("vBCFCPSTRet", "BaseCalculoFcpStRetido", g.BaseCalculoFcpStRetido, 2);
        e.Decimal("pFCPSTRet", "PercentualFcpStRetido", g.PercentualFcpStRetido, 2);
        e.Decimal("vFCPSTRet", "ValorFcpStRetido", g.ValorFcpStRetido, 2);
        e.Decimal("pRedBCEfet", "PercentualReducaoBaseEfetiva", g.PercentualReducaoBaseEfetiva, 2);
        e.Decimal("vBCEfet", "BaseCalculoEfetiva", g.BaseCalculoEfetiva, 2);
        e.Decimal("pICMSEfet", "AliquotaEfetiva", g.AliquotaEfetiva, 2);
        e.Decimal("vICMSEfet", "ValorEfetivo", g.ValorEfetivo, 2);
    }

    private static void Escrever(EscritaNFe e, Icms61 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.MonofasicaCobradaAnteriormente), 2);
        e.Decimal("qBCMonoRet", "QuantidadeBaseRetida", g.QuantidadeBaseRetida, 2);
        e.Decimal("adRemICMSRet", "AliquotaAdRemRetida", g.AliquotaAdRemRetida, 2);
        e.Decimal("vICMSMonoRet", "ValorMonofasicoRetido", g.ValorMonofasicoRetido, 2);
    }

    private static void Escrever(EscritaNFe e, Icms70 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.ReducaoBaseSubstituicao), 2);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("pRedBC", "PercentualReducaoBase", g.PercentualReducaoBase, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Decimal("vBCFCP", "BaseCalculoFcp", g.BaseCalculoFcp, 2);
        e.Decimal("pFCP", "PercentualFcp", g.PercentualFcp, 2);
        e.Decimal("vFCP", "ValorFcp", g.ValorFcp, 2);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("vICMSDeson", "ValorDesonerado", g.ValorDesonerado, 2);
        e.Inteiro("motDesICMS", "MotivoDesoneracao", (int?)g.MotivoDesoneracao, 0);
        e.Inteiro("indDeduzDeson", "DeducaoDesoneracao", (int?)g.DeducaoDesoneracao, 1);
        e.Decimal("vICMSSTDeson", "ValorStDesonerado", g.ValorStDesonerado, 2);
        e.Inteiro("motDesICMSST", "MotivoDesoneracaoSt", (int?)g.MotivoDesoneracaoSt, 0);
    }

    private static void Escrever(EscritaNFe e, Icms90 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstIcms.Outras), 2);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pRedBC", "PercentualReducaoBase", g.PercentualReducaoBase, 2);
        e.Texto("cBenefRBC", "CodigoBeneficioReducao", g.CodigoBeneficioReducao);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMSOp", "ValorOperacao", g.ValorOperacao, 2);
        e.Decimal("pDif", "PercentualDiferimento", g.PercentualDiferimento, 2);
        e.Decimal("vICMSDif", "ValorDiferido", g.ValorDiferido, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Decimal("vBCFCP", "BaseCalculoFcp", g.BaseCalculoFcp, 2);
        e.Decimal("pFCP", "PercentualFcp", g.PercentualFcp, 2);
        e.Decimal("vFCP", "ValorFcp", g.ValorFcp, 2);
        e.Decimal("pFCPDif", "PercentualDiferimentoFcp", g.PercentualDiferimentoFcp, 2);
        e.Decimal("vFCPDif", "ValorFcpDiferido", g.ValorFcpDiferido, 2);
        e.Decimal("vFCPEfet", "ValorFcpEfetivo", g.ValorFcpEfetivo, 2);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("vICMSDeson", "ValorDesonerado", g.ValorDesonerado, 2);
        e.Inteiro("motDesICMS", "MotivoDesoneracao", (int?)g.MotivoDesoneracao, 0);
        e.Inteiro("indDeduzDeson", "DeducaoDesoneracao", (int?)g.DeducaoDesoneracao, 1);
        e.Decimal("vICMSSTDeson", "ValorStDesonerado", g.ValorStDesonerado, 2);
        e.Inteiro("motDesICMSST", "MotivoDesoneracaoSt", (int?)g.MotivoDesoneracaoSt, 0);
    }

    private static void Escrever(EscritaNFe e, IcmsPartilha g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pRedBC", "PercentualReducaoBase", g.PercentualReducaoBase, 2);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("pBCOp", "PercentualBaseOperacao", g.PercentualBaseOperacao, 2);
        e.Sigla("UFST", "UFSt", g.UFSt, g.Exterior);
        e.Decimal("vICMSDeson", "ValorDesonerado", g.ValorDesonerado, 2);
        e.Inteiro("motDesICMS", "MotivoDesoneracao", (int?)g.MotivoDesoneracao, 0);
        e.Inteiro("indDeduzDeson", "DeducaoDesoneracao", (int?)g.DeducaoDesoneracao, 1);
    }

    private static void Escrever(EscritaNFe e, IcmsRepasse g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Decimal("vBCSTRet", "BaseCalculoStRetido", g.BaseCalculoStRetido, 2);
        e.Decimal("pST", "AliquotaConsumidorFinal", g.AliquotaConsumidorFinal, 2);
        e.Decimal("vICMSSubstituto", "ValorSubstituto", g.ValorSubstituto, 2);
        e.Decimal("vICMSSTRet", "ValorStRetido", g.ValorStRetido, 2);
        e.Decimal("vBCFCPSTRet", "BaseCalculoFcpStRetido", g.BaseCalculoFcpStRetido, 2);
        e.Decimal("pFCPSTRet", "PercentualFcpStRetido", g.PercentualFcpStRetido, 2);
        e.Decimal("vFCPSTRet", "ValorFcpStRetido", g.ValorFcpStRetido, 2);
        e.Decimal("vBCSTDest", "BaseCalculoStDestino", g.BaseCalculoStDestino, 2);
        e.Decimal("vICMSSTDest", "ValorStDestino", g.ValorStDestino, 2);
        e.Decimal("pRedBCEfet", "PercentualReducaoBaseEfetiva", g.PercentualReducaoBaseEfetiva, 2);
        e.Decimal("vBCEfet", "BaseCalculoEfetiva", g.BaseCalculoEfetiva, 2);
        e.Decimal("pICMSEfet", "AliquotaEfetiva", g.AliquotaEfetiva, 2);
        e.Decimal("vICMSEfet", "ValorEfetivo", g.ValorEfetivo, 2);
    }

    private static void Escrever(EscritaNFe e, IcmsSn101 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CSOSN", "Csosn", (int?)(g.Csosn ?? CsosnIcms.PermiteCredito), 3);
        e.Decimal("pCredSN", "AliquotaCredito", g.AliquotaCredito, 2);
        e.Decimal("vCredICMSSN", "ValorCredito", g.ValorCredito, 2);
    }

    private static void Escrever(EscritaNFe e, IcmsSn102 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CSOSN", "Csosn", (int?)g.Csosn, 3);
    }

    private static void Escrever(EscritaNFe e, IcmsSn201 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CSOSN", "Csosn", (int?)(g.Csosn ?? CsosnIcms.PermiteCreditoSubstituicao), 3);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("pCredSN", "AliquotaCredito", g.AliquotaCredito, 2);
        e.Decimal("vCredICMSSN", "ValorCredito", g.ValorCredito, 2);
    }

    private static void Escrever(EscritaNFe e, IcmsSn202 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CSOSN", "Csosn", (int?)g.Csosn, 3);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
    }

    private static void Escrever(EscritaNFe e, IcmsSn500 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CSOSN", "Csosn", (int?)(g.Csosn ?? CsosnIcms.CobradoAnteriormente), 3);
        e.Decimal("vBCSTRet", "BaseCalculoStRetido", g.BaseCalculoStRetido, 2);
        e.Decimal("pST", "AliquotaConsumidorFinal", g.AliquotaConsumidorFinal, 2);
        e.Decimal("vICMSSubstituto", "ValorSubstituto", g.ValorSubstituto, 2);
        e.Decimal("vICMSSTRet", "ValorStRetido", g.ValorStRetido, 2);
        e.Decimal("vBCFCPSTRet", "BaseCalculoFcpStRetido", g.BaseCalculoFcpStRetido, 2);
        e.Decimal("pFCPSTRet", "PercentualFcpStRetido", g.PercentualFcpStRetido, 2);
        e.Decimal("vFCPSTRet", "ValorFcpStRetido", g.ValorFcpStRetido, 2);
        e.Decimal("pRedBCEfet", "PercentualReducaoBaseEfetiva", g.PercentualReducaoBaseEfetiva, 2);
        e.Decimal("vBCEfet", "BaseCalculoEfetiva", g.BaseCalculoEfetiva, 2);
        e.Decimal("pICMSEfet", "AliquotaEfetiva", g.AliquotaEfetiva, 2);
        e.Decimal("vICMSEfet", "ValorEfetivo", g.ValorEfetivo, 2);
    }

    private static void Escrever(EscritaNFe e, IcmsSn900 g)
    {
        e.Inteiro("orig", "Origem", (int?)g.Origem, 1);
        e.Inteiro("CSOSN", "Csosn", (int?)(g.Csosn ?? CsosnIcms.Outros), 3);
        e.Inteiro("modBC", "ModalidadeBaseCalculo", (int?)g.ModalidadeBaseCalculo, 1);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pRedBC", "PercentualReducaoBase", g.PercentualReducaoBase, 2);
        e.Decimal("pICMS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMS", "Valor", g.Valor, 2);
        e.Inteiro("modBCST", "ModalidadeBaseCalculoSt", (int?)g.ModalidadeBaseCalculoSt, 1);
        e.Decimal("pMVAST", "PercentualMargemSt", g.PercentualMargemSt, 2);
        e.Decimal("pRedBCST", "PercentualReducaoBaseSt", g.PercentualReducaoBaseSt, 2);
        e.Decimal("vBCST", "BaseCalculoSt", g.BaseCalculoSt, 2);
        e.Decimal("pICMSST", "AliquotaSt", g.AliquotaSt, 2);
        e.Decimal("vICMSST", "ValorSt", g.ValorSt, 2);
        e.Decimal("vBCFCPST", "BaseCalculoFcpSt", g.BaseCalculoFcpSt, 2);
        e.Decimal("pFCPST", "PercentualFcpSt", g.PercentualFcpSt, 2);
        e.Decimal("vFCPST", "ValorFcpSt", g.ValorFcpSt, 2);
        e.Decimal("pCredSN", "AliquotaCredito", g.AliquotaCredito, 2);
        e.Decimal("vCredICMSSN", "ValorCredito", g.ValorCredito, 2);
    }

    private static void Escrever(EscritaNFe e, Ipi g)
    {
        e.Escolha(("Tributado", g.Tributado is not null), ("NaoTributado", g.NaoTributado is not null));
        e.Texto("CNPJProd", "CnpjProdutor", g.CnpjProdutor);
        e.Texto("cSelo", "CodigoSelo", g.CodigoSelo);
        e.Inteiro("qSelo", "QuantidadeSelo", g.QuantidadeSelo, 0);
        e.Texto("cEnq", "CodigoEnquadramento", g.CodigoEnquadramento);
        if (g.Tributado is { } tributado)
        {
            e.Abrir("IPITrib", "Tributado");
            Escrever(e, tributado);
            e.Fechar();
        }
        if (g.NaoTributado is { } naoTributado)
        {
            e.Abrir("IPINT", "NaoTributado");
            Escrever(e, naoTributado);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, ImpostoImportacao g)
    {
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("vDespAdu", "DespesasAduaneiras", g.DespesasAduaneiras, 2);
        e.Decimal("vII", "Valor", g.Valor, 2);
        e.Decimal("vIOF", "Iof", g.Iof, 2);
    }

    private static void Escrever(EscritaNFe e, Issqn g)
    {
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("vAliq", "Aliquota", g.Aliquota, 2);
        e.Decimal("vISSQN", "Valor", g.Valor, 2);
        e.Inteiro("cMunFG", "MunicipioFatoGerador", g.MunicipioFatoGerador, 7);
        e.Texto("cListServ", "ItemListaServico", g.ItemListaServico);
        e.Decimal("vDeducao", "Deducao", g.Deducao, 2);
        e.Decimal("vOutro", "OutrasRetencoes", g.OutrasRetencoes, 2);
        e.Decimal("vDescIncond", "DescontoIncondicionado", g.DescontoIncondicionado, 2);
        e.Decimal("vDescCond", "DescontoCondicionado", g.DescontoCondicionado, 2);
        e.Decimal("vISSRet", "Retencao", g.Retencao, 2);
        e.Inteiro("indISS", "Exigibilidade", (int?)g.Exigibilidade, 1);
        e.Texto("cServico", "CodigoServico", g.CodigoServico);
        e.Inteiro("cMun", "MunicipioIncidencia", g.MunicipioIncidencia, 7);
        e.Inteiro("cPais", "CodigoPais", g.CodigoPais, 0);
        e.Texto("nProcesso", "Processo", g.Processo);
        e.Inteiro("indIncentivo", "IncentivoFiscal", (int?)g.IncentivoFiscal, 1);
    }

    private static void Escrever(EscritaNFe e, PisAliquota g)
    {
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pPIS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vPIS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, PisNaoTributado g)
    {
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
    }

    private static void Escrever(EscritaNFe e, PisOutros g)
    {
        e.Escolha(("BaseCalculo, Aliquota", g.BaseCalculo.HasValue || g.Aliquota.HasValue), ("Quantidade, AliquotaUnidade", g.Quantidade.HasValue || g.AliquotaUnidade.HasValue));
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pPIS", "Aliquota", g.Aliquota, 2);
        e.Decimal("qBCProd", "Quantidade", g.Quantidade, 2);
        e.Decimal("vAliqProd", "AliquotaUnidade", g.AliquotaUnidade, 2);
        e.Decimal("vPIS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, PisQuantidade g)
    {
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstPisCofins.AliquotaUnidade), 2);
        e.Decimal("qBCProd", "Quantidade", g.Quantidade, 2);
        e.Decimal("vAliqProd", "AliquotaUnidade", g.AliquotaUnidade, 2);
        e.Decimal("vPIS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, PisSt g)
    {
        e.Escolha(("BaseCalculo, Aliquota", g.BaseCalculo.HasValue || g.Aliquota.HasValue), ("Quantidade, AliquotaUnidade", g.Quantidade.HasValue || g.AliquotaUnidade.HasValue));
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pPIS", "Aliquota", g.Aliquota, 2);
        e.Decimal("qBCProd", "Quantidade", g.Quantidade, 4);
        e.Decimal("vAliqProd", "AliquotaUnidade", g.AliquotaUnidade, 4);
        e.Decimal("vPIS", "Valor", g.Valor, 2);
        e.Inteiro("indSomaPISST", "Totalizacao", (int?)g.Totalizacao, 1);
    }

    private static void Escrever(EscritaNFe e, CofinsAliquota g)
    {
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pCOFINS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vCOFINS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, CofinsNaoTributado g)
    {
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
    }

    private static void Escrever(EscritaNFe e, CofinsOutros g)
    {
        e.Escolha(("BaseCalculo, Aliquota", g.BaseCalculo.HasValue || g.Aliquota.HasValue), ("Quantidade, AliquotaUnidade", g.Quantidade.HasValue || g.AliquotaUnidade.HasValue));
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pCOFINS", "Aliquota", g.Aliquota, 2);
        e.Decimal("qBCProd", "Quantidade", g.Quantidade, 2);
        e.Decimal("vAliqProd", "AliquotaUnidade", g.AliquotaUnidade, 2);
        e.Decimal("vCOFINS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, CofinsQuantidade g)
    {
        e.Inteiro("CST", "Cst", (int?)(g.Cst ?? CstPisCofins.AliquotaUnidade), 2);
        e.Decimal("qBCProd", "Quantidade", g.Quantidade, 2);
        e.Decimal("vAliqProd", "AliquotaUnidade", g.AliquotaUnidade, 2);
        e.Decimal("vCOFINS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, CofinsSt g)
    {
        e.Escolha(("BaseCalculo, Aliquota", g.BaseCalculo.HasValue || g.Aliquota.HasValue), ("Quantidade, AliquotaUnidade", g.Quantidade.HasValue || g.AliquotaUnidade.HasValue));
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pCOFINS", "Aliquota", g.Aliquota, 2);
        e.Decimal("qBCProd", "Quantidade", g.Quantidade, 4);
        e.Decimal("vAliqProd", "AliquotaUnidade", g.AliquotaUnidade, 4);
        e.Decimal("vCOFINS", "Valor", g.Valor, 2);
        e.Inteiro("indSomaCOFINSST", "Totalizacao", (int?)g.Totalizacao, 1);
    }

    private static void Escrever(EscritaNFe e, IcmsUFDestino g)
    {
        e.Decimal("vBCUFDest", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("vBCFCPUFDest", "BaseCalculoFcp", g.BaseCalculoFcp, 2);
        e.Decimal("pFCPUFDest", "PercentualFcp", g.PercentualFcp, 2);
        e.Decimal("pICMSUFDest", "AliquotaInterna", g.AliquotaInterna, 2);
        e.Decimal("pICMSInter", "AliquotaInterestadual", g.AliquotaInterestadual, 2);
        e.Decimal("pICMSInterPart", "PercentualPartilha", g.PercentualPartilha, 2);
        e.Decimal("vFCPUFDest", "ValorFcp", g.ValorFcp, 2);
        e.Decimal("vICMSUFDest", "ValorDestino", g.ValorDestino, 2);
        e.Decimal("vICMSUFRemet", "ValorRemetente", g.ValorRemetente, 2);
    }

    private static void Escrever(EscritaNFe e, ImpostoSeletivo g)
    {
        e.Texto("CSTIS", "Cst", g.Cst);
        e.Texto("cClassTribIS", "ClassificacaoTributaria", g.ClassificacaoTributaria);
        e.Decimal("vBCIS", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pIS", "Aliquota", g.Aliquota, 2);
        e.Decimal("adRemIS", "AliquotaEspecifica", g.AliquotaEspecifica, 2);
        e.Texto("uTrib", "UnidadeTributavel", g.UnidadeTributavel);
        e.Decimal("qTrib", "QuantidadeTributavel", g.QuantidadeTributavel, 4);
        e.Decimal("vIS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, IbsCbs g)
    {
        e.Escolha(("Tributacao", g.Tributacao is not null), ("Monofasia", g.Monofasia is not null), ("TransferenciaCredito", g.TransferenciaCredito is not null), ("AjusteCompetencia", g.AjusteCompetencia is not null));
        e.Escolha(("CreditoPresumido", g.CreditoPresumido is not null), ("CreditoPresumidoZfm", g.CreditoPresumidoZfm is not null));
        e.Texto("CST", "Cst", g.Cst);
        e.Texto("cClassTrib", "ClassificacaoTributaria", g.ClassificacaoTributaria);
        e.Inteiro("indDoacao", "Doacao", (int?)g.Doacao, 1);
        if (g.Tributacao is { } tributacao)
        {
            e.Abrir("gIBSCBS", "Tributacao");
            Escrever(e, tributacao);
            e.Fechar();
        }
        if (g.Monofasia is { } monofasia)
        {
            e.Abrir("gIBSCBSMono", "Monofasia");
            Escrever(e, monofasia);
            e.Fechar();
        }
        if (g.TransferenciaCredito is { } transferenciaCredito)
        {
            e.Abrir("gTransfCred", "TransferenciaCredito");
            Escrever(e, transferenciaCredito);
            e.Fechar();
        }
        if (g.AjusteCompetencia is { } ajusteCompetencia)
        {
            e.Abrir("gAjusteCompet", "AjusteCompetencia");
            Escrever(e, ajusteCompetencia);
            e.Fechar();
        }
        if (g.EstornoCredito is { } estornoCredito)
        {
            e.Abrir("gEstornoCred", "EstornoCredito");
            Escrever(e, estornoCredito);
            e.Fechar();
        }
        if (g.CreditoPresumido is { } creditoPresumido)
        {
            e.Abrir("gCredPresOper", "CreditoPresumido");
            Escrever(e, creditoPresumido);
            e.Fechar();
        }
        if (g.CreditoPresumidoZfm is { } creditoPresumidoZfm)
        {
            e.Abrir("gCredPresIBSZFM", "CreditoPresumidoZfm");
            Escrever(e, creditoPresumidoZfm);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, IpiDevolvido g)
    {
        e.Decimal("vIPIDevol", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, Adicao g)
    {
        e.Inteiro("nAdicao", "Numero", g.Numero, 0);
        e.Inteiro("nSeqAdic", "Sequencial", g.Sequencial, 0);
        e.Texto("cFabricante", "CodigoFabricante", g.CodigoFabricante);
        e.Decimal("vDescDI", "Desconto", g.Desconto, 2);
        e.Texto("nDraw", "Drawback", g.Drawback);
    }

    private static void Escrever(EscritaNFe e, ExportacaoIndireta g)
    {
        e.Texto("nRE", "RegistroExportacao", g.RegistroExportacao);
        e.Texto("chNFe", "Chave", g.Chave);
        e.Decimal("qExport", "Quantidade", g.Quantidade, 2);
    }

    private static void Escrever(EscritaNFe e, Cide g)
    {
        e.Decimal("qBCProd", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("vAliqProd", "Aliquota", g.Aliquota, 4);
        e.Decimal("vCIDE", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, Encerrante g)
    {
        e.Inteiro("nBico", "Bico", g.Bico, 0);
        e.Inteiro("nBomba", "Bomba", g.Bomba, 0);
        e.Inteiro("nTanque", "Tanque", g.Tanque, 0);
        e.Decimal("vEncIni", "Inicial", g.Inicial, 3);
        e.Decimal("vEncFin", "Final", g.Final, 3);
    }

    private static void Escrever(EscritaNFe e, OrigemCombustivel g)
    {
        e.Inteiro("indImport", "Importacao", (int?)g.Importacao, 1);
        e.Inteiro("cUFOrig", "UF", (int?)g.UF, 2);
        e.Decimal("pOrig", "Percentual", g.Percentual, 2);
    }

    private static void Escrever(EscritaNFe e, IpiTributado g)
    {
        e.Escolha(("BaseCalculo, Aliquota", g.BaseCalculo.HasValue || g.Aliquota.HasValue), ("Quantidade, ValorUnidade", g.Quantidade.HasValue || g.ValorUnidade.HasValue));
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pIPI", "Aliquota", g.Aliquota, 2);
        e.Decimal("qUnid", "Quantidade", g.Quantidade, 2);
        e.Decimal("vUnid", "ValorUnidade", g.ValorUnidade, 4);
        e.Decimal("vIPI", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, IpiNaoTributado g)
    {
        e.Inteiro("CST", "Cst", (int?)g.Cst, 2);
    }

    private static void Escrever(EscritaNFe e, TributacaoIbsCbs g)
    {
        e.Decimal("vBC", "BaseCalculo", g.BaseCalculo, 2);
        if (g.IbsUF is { } ibsUF)
        {
            e.Abrir("gIBSUF", "IbsUF");
            Escrever(e, ibsUF);
            e.Fechar();
        }
        if (g.IbsMunicipio is { } ibsMunicipio)
        {
            e.Abrir("gIBSMun", "IbsMunicipio");
            Escrever(e, ibsMunicipio);
            e.Fechar();
        }
        e.Decimal("vIBS", "ValorIbs", g.ValorIbs, 2);
        if (g.Cbs is { } cbs)
        {
            e.Abrir("gCBS", "Cbs");
            Escrever(e, cbs);
            e.Fechar();
        }
        if (g.TributacaoRegular is { } tributacaoRegular)
        {
            e.Abrir("gTribRegular", "TributacaoRegular");
            Escrever(e, tributacaoRegular);
            e.Fechar();
        }
        if (g.CompraGovernamental is { } compraGovernamental)
        {
            e.Abrir("gTribCompraGov", "CompraGovernamental");
            Escrever(e, compraGovernamental);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsCbs g)
    {
        e.Escolha(("IbsAdRem", g.IbsAdRem is not null), ("IbsAdValorem", g.IbsAdValorem is not null));
        e.Escolha(("CbsAdRem", g.CbsAdRem is not null), ("CbsAdValorem", g.CbsAdValorem is not null));
        if (g.IbsAdRem is { } ibsAdRem)
        {
            e.Abrir("gIBSMonoAdRem", "IbsAdRem");
            Escrever(e, ibsAdRem);
            e.Fechar();
        }
        if (g.IbsAdValorem is { } ibsAdValorem)
        {
            e.Abrir("gIBSMonoAdValorem", "IbsAdValorem");
            Escrever(e, ibsAdValorem);
            e.Fechar();
        }
        if (g.CbsAdRem is { } cbsAdRem)
        {
            e.Abrir("gCBSMonoAdRem", "CbsAdRem");
            Escrever(e, cbsAdRem);
            e.Fechar();
        }
        if (g.CbsAdValorem is { } cbsAdValorem)
        {
            e.Abrir("gCBSMonoAdValorem", "CbsAdValorem");
            Escrever(e, cbsAdValorem);
            e.Fechar();
        }
        e.Decimal("vTotIBSMonoItem", "TotalIbs", g.TotalIbs, 2);
        e.Decimal("vTotCBSMonoItem", "TotalCbs", g.TotalCbs, 2);
    }

    private static void Escrever(EscritaNFe e, TransferenciaCredito g)
    {
        e.Decimal("vIBS", "ValorIbs", g.ValorIbs, 2);
        e.Decimal("vCBS", "ValorCbs", g.ValorCbs, 2);
    }

    private static void Escrever(EscritaNFe e, AjusteCompetencia g)
    {
        e.Texto("competApur", "Competencia", g.Competencia);
        e.Decimal("vIBS", "ValorIbs", g.ValorIbs, 2);
        e.Decimal("vCBS", "ValorCbs", g.ValorCbs, 2);
    }

    private static void Escrever(EscritaNFe e, CreditoPresumidoOperacao g)
    {
        e.Decimal("vBCCredPres", "BaseCalculo", g.BaseCalculo, 2);
        e.Texto("cCredPres", "Codigo", g.Codigo);
        if (g.Ibs is { } ibs)
        {
            e.Abrir("gIBSCredPres", "Ibs");
            Escrever(e, ibs);
            e.Fechar();
        }
        if (g.Cbs is { } cbs)
        {
            e.Abrir("gCBSCredPres", "Cbs");
            Escrever(e, cbs);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, CreditoPresumidoZfm g)
    {
        e.Texto("competApur", "Competencia", g.Competencia);
        e.Inteiro("tpCredPresIBSZFM", "Classificacao", (int?)g.Classificacao, 1);
        e.Decimal("vCredPresIBSZFM", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, IbsUF g)
    {
        e.Decimal("pIBSUF", "Aliquota", g.Aliquota, 2);
        if (g.Diferimento is { } diferimento)
        {
            e.Abrir("gDif", "Diferimento");
            Escrever(e, diferimento);
            e.Fechar();
        }
        if (g.Devolucao is { } devolucao)
        {
            e.Abrir("gDevTrib", "Devolucao");
            Escrever(e, devolucao);
            e.Fechar();
        }
        if (g.Reducao is { } reducao)
        {
            e.Abrir("gRed", "Reducao");
            Escrever(e, reducao);
            e.Fechar();
        }
        e.Decimal("vIBSUF", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, IbsMunicipio g)
    {
        e.Decimal("pIBSMun", "Aliquota", g.Aliquota, 2);
        if (g.Diferimento is { } diferimento)
        {
            e.Abrir("gDif", "Diferimento");
            Escrever(e, diferimento);
            e.Fechar();
        }
        if (g.Devolucao is { } devolucao)
        {
            e.Abrir("gDevTrib", "Devolucao");
            Escrever(e, devolucao);
            e.Fechar();
        }
        if (g.Reducao is { } reducao)
        {
            e.Abrir("gRed", "Reducao");
            Escrever(e, reducao);
            e.Fechar();
        }
        e.Decimal("vIBSMun", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, Cbs g)
    {
        e.Decimal("pCBS", "Aliquota", g.Aliquota, 2);
        if (g.Diferimento is { } diferimento)
        {
            e.Abrir("gDif", "Diferimento");
            Escrever(e, diferimento);
            e.Fechar();
        }
        if (g.Devolucao is { } devolucao)
        {
            e.Abrir("gDevTrib", "Devolucao");
            Escrever(e, devolucao);
            e.Fechar();
        }
        if (g.Reducao is { } reducao)
        {
            e.Abrir("gRed", "Reducao");
            Escrever(e, reducao);
            e.Fechar();
        }
        if (g.AreaIncentivada is { } areaIncentivada)
        {
            e.Abrir("gALCZFMCBS", "AreaIncentivada");
            Escrever(e, areaIncentivada);
            e.Fechar();
        }
        e.Decimal("vCBS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, TributacaoRegular g)
    {
        e.Texto("CSTReg", "Cst", g.Cst);
        e.Texto("cClassTribReg", "ClassificacaoTributaria", g.ClassificacaoTributaria);
        e.Decimal("pAliqEfetRegIBSUF", "AliquotaIbsUF", g.AliquotaIbsUF, 2);
        e.Decimal("vTribRegIBSUF", "ValorIbsUF", g.ValorIbsUF, 2);
        e.Decimal("pAliqEfetRegIBSMun", "AliquotaIbsMunicipio", g.AliquotaIbsMunicipio, 2);
        e.Decimal("vTribRegIBSMun", "ValorIbsMunicipio", g.ValorIbsMunicipio, 2);
        e.Decimal("pAliqEfetRegCBS", "AliquotaCbs", g.AliquotaCbs, 2);
        e.Decimal("vTribRegCBS", "ValorCbs", g.ValorCbs, 2);
    }

    private static void Escrever(EscritaNFe e, TributacaoCompraGovernamental g)
    {
        e.Decimal("pAliqIBSUF", "AliquotaIbsUF", g.AliquotaIbsUF, 2);
        e.Decimal("vTribIBSUF", "ValorIbsUF", g.ValorIbsUF, 2);
        e.Decimal("pAliqIBSMun", "AliquotaIbsMunicipio", g.AliquotaIbsMunicipio, 2);
        e.Decimal("vTribIBSMun", "ValorIbsMunicipio", g.ValorIbsMunicipio, 2);
        e.Decimal("pAliqCBS", "AliquotaCbs", g.AliquotaCbs, 2);
        e.Decimal("vTribCBS", "ValorCbs", g.ValorCbs, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsAdRem g)
    {
        if (g.Padrao is { } padrao)
        {
            e.Abrir("gMonoPadrao", "Padrao");
            Escrever(e, padrao);
            e.Fechar();
        }
        if (g.Retencao is { } retencao)
        {
            e.Abrir("gMonoReten", "Retencao");
            Escrever(e, retencao);
            e.Fechar();
        }
        if (g.Retido is { } retido)
        {
            e.Abrir("gMonoRet", "Retido");
            Escrever(e, retido);
            e.Fechar();
        }
        if (g.Biocombustivel is { } biocombustivel)
        {
            e.Abrir("gpBioDiferenca", "Biocombustivel");
            Escrever(e, biocombustivel);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsAdValorem g)
    {
        if (g.Padrao is { } padrao)
        {
            e.Abrir("gMonoPadrao", "Padrao");
            Escrever(e, padrao);
            e.Fechar();
        }
        if (g.Retencao is { } retencao)
        {
            e.Abrir("gMonoReten", "Retencao");
            Escrever(e, retencao);
            e.Fechar();
        }
        if (g.Retido is { } retido)
        {
            e.Abrir("gMonoRet", "Retido");
            Escrever(e, retido);
            e.Fechar();
        }
        if (g.Biocombustivel is { } biocombustivel)
        {
            e.Abrir("gpBioDiferenca", "Biocombustivel");
            Escrever(e, biocombustivel);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsAdRem g)
    {
        if (g.Padrao is { } padrao)
        {
            e.Abrir("gMonoPadrao", "Padrao");
            Escrever(e, padrao);
            e.Fechar();
        }
        if (g.Retencao is { } retencao)
        {
            e.Abrir("gMonoReten", "Retencao");
            Escrever(e, retencao);
            e.Fechar();
        }
        if (g.Retido is { } retido)
        {
            e.Abrir("gMonoRet", "Retido");
            Escrever(e, retido);
            e.Fechar();
        }
        if (g.Biocombustivel is { } biocombustivel)
        {
            e.Abrir("gpBioDiferenca", "Biocombustivel");
            Escrever(e, biocombustivel);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsAdValorem g)
    {
        if (g.Padrao is { } padrao)
        {
            e.Abrir("gMonoPadrao", "Padrao");
            Escrever(e, padrao);
            e.Fechar();
        }
        if (g.Retencao is { } retencao)
        {
            e.Abrir("gMonoReten", "Retencao");
            Escrever(e, retencao);
            e.Fechar();
        }
        if (g.Retido is { } retido)
        {
            e.Abrir("gMonoRet", "Retido");
            Escrever(e, retido);
            e.Fechar();
        }
        if (g.Biocombustivel is { } biocombustivel)
        {
            e.Abrir("gpBioDiferenca", "Biocombustivel");
            Escrever(e, biocombustivel);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, CreditoPresumidoIbsCbs g)
    {
        e.Escolha(("Valor", g.Valor.HasValue), ("ValorCondicaoSuspensiva", g.ValorCondicaoSuspensiva.HasValue));
        e.Decimal("pCredPres", "Percentual", g.Percentual, 2);
        e.Decimal("vCredPres", "Valor", g.Valor, 2);
        e.Decimal("vCredPresCondSus", "ValorCondicaoSuspensiva", g.ValorCondicaoSuspensiva, 2);
    }

    private static void Escrever(EscritaNFe e, Diferimento g)
    {
        e.Decimal("pDif", "Percentual", g.Percentual, 2);
        e.Decimal("vDif", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, DevolucaoTributo g)
    {
        e.Decimal("pDevTrib", "Percentual", g.Percentual, 2);
        e.Decimal("vDevTrib", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, ReducaoAliquota g)
    {
        e.Decimal("pRedAliq", "Percentual", g.Percentual, 2);
        e.Decimal("pAliqEfet", "AliquotaEfetiva", g.AliquotaEfetiva, 2);
    }

    private static void Escrever(EscritaNFe e, CbsAreaIncentivada g)
    {
        e.Inteiro("tpALCZFMCBS", "Tipo", (int?)g.Tipo, 1);
        e.Texto("nProcSuframa", "ProcessoSuframa", g.ProcessoSuframa);
        e.Decimal("pAliqEfetRegCBS", "AliquotaEfetiva", g.AliquotaEfetiva, 2);
        e.Decimal("vTribRegCBS", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsAdRemPadrao g)
    {
        e.Decimal("qBCMono", "Quantidade", g.Quantidade, 4);
        e.Decimal("adRemIBS", "AliquotaAdRem", g.AliquotaAdRem, 2);
        e.Decimal("vIBSMono", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsAdRemRetencao g)
    {
        e.Decimal("qBCMonoReten", "Quantidade", g.Quantidade, 4);
        e.Decimal("adRemIBSReten", "AliquotaAdRem", g.AliquotaAdRem, 2);
        e.Decimal("vIBSMonoReten", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsRetido g)
    {
        e.Decimal("vIBSMonoRet", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsBiocombustivel g)
    {
        e.Decimal("qBCBioComb", "Quantidade", g.Quantidade, 4);
        e.Decimal("vIBSDiferenca", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsAdValoremPadrao g)
    {
        e.Decimal("vBCMono", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pAliqMonoUF", "AliquotaUF", g.AliquotaUF, 2);
        e.Decimal("vIBSMonoUF", "ValorUF", g.ValorUF, 2);
        e.Decimal("pAliqMonoMun", "AliquotaMunicipio", g.AliquotaMunicipio, 2);
        e.Decimal("vIBSMonoMun", "ValorMunicipio", g.ValorMunicipio, 2);
        e.Decimal("vIBSMono", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaIbsAdValoremRetencao g)
    {
        e.Decimal("vBCMonoReten", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pAliqMonoReten", "Aliquota", g.Aliquota, 2);
        e.Decimal("vIBSMonoReten", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsAdRemPadrao g)
    {
        e.Decimal("qBCMono", "Quantidade", g.Quantidade, 4);
        e.Decimal("adRemCBS", "AliquotaAdRem", g.AliquotaAdRem, 2);
        e.Decimal("vCBSMono", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsAdRemRetencao g)
    {
        e.Decimal("qBCMonoReten", "Quantidade", g.Quantidade, 4);
        e.Decimal("adRemCBSReten", "AliquotaAdRem", g.AliquotaAdRem, 2);
        e.Decimal("vCBSMonoReten", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsRetido g)
    {
        e.Decimal("vCBSMonoRet", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsBiocombustivel g)
    {
        e.Decimal("qBCBioComb", "Quantidade", g.Quantidade, 4);
        e.Decimal("vCBSDiferenca", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsAdValoremPadrao g)
    {
        e.Decimal("vBCMono", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pAliqMonoCBS", "Aliquota", g.Aliquota, 2);
        e.Decimal("vCBSMono", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, MonofasiaCbsAdValoremRetencao g)
    {
        e.Decimal("vBCMonoReten", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pAliqMonoReten", "Aliquota", g.Aliquota, 2);
        e.Decimal("vCBSMonoReten", "Valor", g.Valor, 2);
    }
}
