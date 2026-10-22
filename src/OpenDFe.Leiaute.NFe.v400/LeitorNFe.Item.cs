namespace OpenDFe.NFe;

// A leitura dos itens e dos tributos (docs/LEITURA.md 3): um método por grupo, espelho do EscritorNFe.Item; o pai
// entra e sai do grupo.
internal static partial class LeitorNFe
{
    private static void Ler(LeituraNFe l, Item g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "prod":
                    g.Produto = l.Grupo(g.Produto, "Produto", Ler);
                    break;
                case "imposto":
                    g.Imposto = l.Grupo(g.Imposto, "Imposto", Ler);
                    break;
                case "impostoDevol":
                    g.ImpostoDevolvido = l.Grupo(g.ImpostoDevolvido, "ImpostoDevolvido", Ler);
                    break;
                case "infAdProd":
                    g.InformacoesAdicionais = l.Texto(g.InformacoesAdicionais, "InformacoesAdicionais");
                    break;
                case "obsItem":
                    g.Observacoes = l.Grupo(g.Observacoes, "Observacoes", Ler);
                    break;
                case "vItem":
                    g.ValorTotal = l.Decimal(g.ValorTotal, "ValorTotal");
                    break;
                case "DFeReferenciado":
                    g.Referencia = l.Grupo(g.Referencia, "Referencia", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Produto g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cProd":
                    g.Codigo = l.Texto(g.Codigo, "Codigo");
                    break;
                case "cEAN":
                    g.Gtin = l.Texto(g.Gtin, "Gtin");
                    break;
                case "cBarra":
                    g.CodigoBarras = l.Texto(g.CodigoBarras, "CodigoBarras");
                    break;
                case "xProd":
                    g.Descricao = l.Texto(g.Descricao, "Descricao");
                    break;
                case "NCM":
                    g.Ncm = l.Texto(g.Ncm, "Ncm");
                    break;
                case "NVE":
                    l.Textos(g.Nve, "Nve");
                    break;
                case "CEST":
                    g.Cest = l.Texto(g.Cest, "Cest");
                    break;
                case "indEscala":
                    g.Escala = l.Literal(g.Escala, "Escala", Literais.LerIndicadorEscala);
                    break;
                case "CNPJFab":
                    g.CnpjFabricante = l.Texto(g.CnpjFabricante, "CnpjFabricante");
                    break;
                case "cBenef":
                    g.CodigoBeneficio = l.Texto(g.CodigoBeneficio, "CodigoBeneficio");
                    break;
                case "gCred":
                    l.Lista(g.CreditosPresumidos, "CreditosPresumidos", Ler);
                    break;
                case "tpCredPresIBSZFM":
                    g.ClassificacaoZfm = l.Enumeracao(g.ClassificacaoZfm, "ClassificacaoZfm");
                    break;
                case "EXTIPI":
                    g.ExTipi = l.Texto(g.ExTipi, "ExTipi");
                    break;
                case "CFOP":
                    g.Cfop = l.Inteiro(g.Cfop, "Cfop");
                    break;
                case "uCom":
                    g.UnidadeComercial = l.Texto(g.UnidadeComercial, "UnidadeComercial");
                    break;
                case "qCom":
                    g.QuantidadeComercial = l.Decimal(g.QuantidadeComercial, "QuantidadeComercial");
                    break;
                case "vUnCom":
                    g.ValorUnitarioComercial = l.Decimal(g.ValorUnitarioComercial, "ValorUnitarioComercial");
                    break;
                case "vProd":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "cEANTrib":
                    g.GtinTributavel = l.Texto(g.GtinTributavel, "GtinTributavel");
                    break;
                case "cBarraTrib":
                    g.CodigoBarrasTributavel = l.Texto(g.CodigoBarrasTributavel, "CodigoBarrasTributavel");
                    break;
                case "uTrib":
                    g.UnidadeTributavel = l.Texto(g.UnidadeTributavel, "UnidadeTributavel");
                    break;
                case "qTrib":
                    g.QuantidadeTributavel = l.Decimal(g.QuantidadeTributavel, "QuantidadeTributavel");
                    break;
                case "vUnTrib":
                    g.ValorUnitarioTributavel = l.Decimal(g.ValorUnitarioTributavel, "ValorUnitarioTributavel");
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
                case "vOutro":
                    g.OutrasDespesas = l.Decimal(g.OutrasDespesas, "OutrasDespesas");
                    break;
                case "indTot":
                    g.Totalizacao = l.Enumeracao(g.Totalizacao, "Totalizacao");
                    break;
                case "indBemMovelUsado":
                    g.BemMovelUsado = l.Enumeracao(g.BemMovelUsado, "BemMovelUsado");
                    break;
                case "DI":
                    l.Lista(g.Importacoes, "Importacoes", Ler);
                    break;
                case "detExport":
                    l.Lista(g.Exportacoes, "Exportacoes", Ler);
                    break;
                case "xPed":
                    g.Pedido = l.Texto(g.Pedido, "Pedido");
                    break;
                case "nItemPed":
                    g.ItemPedido = l.Texto(g.ItemPedido, "ItemPedido");
                    break;
                case "nFCI":
                    g.NumeroFci = l.Texto(g.NumeroFci, "NumeroFci");
                    break;
                case "rastro":
                    l.Lista(g.Rastros, "Rastros", Ler);
                    break;
                case "infProdNFF":
                    g.Nff = l.Grupo(g.Nff, "Nff", Ler);
                    break;
                case "infProdEmb":
                    g.Embalagem = l.Grupo(g.Embalagem, "Embalagem", Ler);
                    break;
                case "veicProd":
                    g.VeiculoNovo = l.Grupo(g.VeiculoNovo, "VeiculoNovo", Ler);
                    break;
                case "med":
                    g.Medicamento = l.Grupo(g.Medicamento, "Medicamento", Ler);
                    break;
                case "arma":
                    l.Lista(g.Armas, "Armas", Ler);
                    break;
                case "comb":
                    g.Combustivel = l.Grupo(g.Combustivel, "Combustivel", Ler);
                    break;
                case "nRECOPI":
                    g.Recopi = l.Texto(g.Recopi, "Recopi");
                    break;
                case "cCredPresumido":
                    l.Retirado("o pacote 9m (NT 2023.004 v1.01, 07/02/2024), substituído pelo gCred no 9n");
                    break;
                case "pCredPresumido":
                    l.Retirado("o pacote 9m (NT 2023.004 v1.01, 07/02/2024), substituído pelo gCred no 9n");
                    break;
                case "vCredPresumido":
                    l.Retirado("o pacote 9m (NT 2023.004 v1.01, 07/02/2024), substituído pelo gCred no 9n");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Imposto g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vTotTrib":
                    g.ValorTotalTributos = l.Decimal(g.ValorTotalTributos, "ValorTotalTributos");
                    break;
                case "ICMS":
                    g.Icms = l.Escolha(g.Icms, "Icms", LerIcms);
                    break;
                case "IPI":
                    g.Ipi = l.Grupo(g.Ipi, "Ipi", Ler);
                    break;
                case "II":
                    g.ImpostoImportacao = l.Grupo(g.ImpostoImportacao, "ImpostoImportacao", Ler);
                    break;
                case "ISSQN":
                    g.Issqn = l.Grupo(g.Issqn, "Issqn", Ler);
                    break;
                case "PIS":
                    g.Pis = l.Escolha(g.Pis, "Pis", LerPis);
                    break;
                case "PISST":
                    g.PisSt = l.Grupo(g.PisSt, "PisSt", Ler);
                    break;
                case "COFINS":
                    g.Cofins = l.Escolha(g.Cofins, "Cofins", LerCofins);
                    break;
                case "COFINSST":
                    g.CofinsSt = l.Grupo(g.CofinsSt, "CofinsSt", Ler);
                    break;
                case "ICMSUFDest":
                    g.IcmsUFDestino = l.Grupo(g.IcmsUFDestino, "IcmsUFDestino", Ler);
                    break;
                case "IS":
                    g.ImpostoSeletivo = l.Grupo(g.ImpostoSeletivo, "ImpostoSeletivo", Ler);
                    break;
                case "IBSCBS":
                    g.IbsCbs = l.Grupo(g.IbsCbs, "IbsCbs", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static Icms? LerIcms(LeituraNFe l, string nome) => nome switch
    {
        "ICMS00" => l.Subgrupo(new Icms00(), Ler),
        "ICMS02" => l.Subgrupo(new Icms02(), Ler),
        "ICMS10" => l.Subgrupo(new Icms10(), Ler),
        "ICMS15" => l.Subgrupo(new Icms15(), Ler),
        "ICMS20" => l.Subgrupo(new Icms20(), Ler),
        "ICMS30" => l.Subgrupo(new Icms30(), Ler),
        "ICMS40" => l.Subgrupo(new Icms40(), Ler),
        "ICMS51" => l.Subgrupo(new Icms51(), Ler),
        "ICMS53" => l.Subgrupo(new Icms53(), Ler),
        "ICMS60" => l.Subgrupo(new Icms60(), Ler),
        "ICMS61" => l.Subgrupo(new Icms61(), Ler),
        "ICMS70" => l.Subgrupo(new Icms70(), Ler),
        "ICMS90" => l.Subgrupo(new Icms90(), Ler),
        "ICMSPart" => l.Subgrupo(new IcmsPartilha(), Ler),
        "ICMSST" => l.Subgrupo(new IcmsRepasse(), Ler),
        "ICMSSN101" => l.Subgrupo(new IcmsSn101(), Ler),
        "ICMSSN102" => l.Subgrupo(new IcmsSn102(), Ler),
        "ICMSSN201" => l.Subgrupo(new IcmsSn201(), Ler),
        "ICMSSN202" => l.Subgrupo(new IcmsSn202(), Ler),
        "ICMSSN500" => l.Subgrupo(new IcmsSn500(), Ler),
        "ICMSSN900" => l.Subgrupo(new IcmsSn900(), Ler),
        _ => null,
    };

    private static Pis? LerPis(LeituraNFe l, string nome) => nome switch
    {
        "PISAliq" => l.Subgrupo(new PisAliquota(), Ler),
        "PISNT" => l.Subgrupo(new PisNaoTributado(), Ler),
        "PISOutr" => l.Subgrupo(new PisOutros(), Ler),
        "PISQtde" => l.Subgrupo(new PisQuantidade(), Ler),
        _ => null,
    };

    private static Cofins? LerCofins(LeituraNFe l, string nome) => nome switch
    {
        "COFINSAliq" => l.Subgrupo(new CofinsAliquota(), Ler),
        "COFINSNT" => l.Subgrupo(new CofinsNaoTributado(), Ler),
        "COFINSOutr" => l.Subgrupo(new CofinsOutros(), Ler),
        "COFINSQtde" => l.Subgrupo(new CofinsQuantidade(), Ler),
        _ => null,
    };

    private static void Ler(LeituraNFe l, ImpostoDevolvido g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pDevol":
                    g.Percentual = l.Decimal(g.Percentual, "Percentual");
                    break;
                case "IPI":
                    g.Ipi = l.Grupo(g.Ipi, "Ipi", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ObservacoesItem g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "obsCont":
                    g.Contribuinte = l.Grupo(g.Contribuinte, "Contribuinte", Ler);
                    break;
                case "obsFisco":
                    g.Fisco = l.Grupo(g.Fisco, "Fisco", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ItemReferenciado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "chaveAcesso":
                    g.Chave = l.Texto(g.Chave, "Chave");
                    break;
                case "nItem":
                    g.NumeroItem = l.Inteiro(g.NumeroItem, "NumeroItem");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CreditoPresumidoIcms g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cCredPresumido":
                    g.Codigo = l.Texto(g.Codigo, "Codigo");
                    break;
                case "pCredPresumido":
                    g.Percentual = l.Decimal(g.Percentual, "Percentual");
                    break;
                case "vCredPresumido":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, DeclaracaoImportacao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nDI":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "dDI":
                    g.Data = l.Data(g.Data, "Data");
                    break;
                case "xLocDesemb":
                    g.LocalDesembaraco = l.Texto(g.LocalDesembaraco, "LocalDesembaraco");
                    break;
                case "UFDesemb":
                    g.UFDesembaraco = l.Sigla(g.UFDesembaraco, "UFDesembaraco");
                    break;
                case "dDesemb":
                    g.DataDesembaraco = l.Data(g.DataDesembaraco, "DataDesembaraco");
                    break;
                case "tpViaTransp":
                    g.ViaTransporte = l.Enumeracao(g.ViaTransporte, "ViaTransporte");
                    break;
                case "vAFRMM":
                    g.ValorAfrmm = l.Decimal(g.ValorAfrmm, "ValorAfrmm");
                    break;
                case "tpIntermedio":
                    g.FormaImportacao = l.Enumeracao(g.FormaImportacao, "FormaImportacao");
                    break;
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "CPF":
                    g.Cpf = l.Texto(g.Cpf, "Cpf");
                    break;
                case "UFTerceiro":
                    g.UFTerceiro = l.Sigla(g.UFTerceiro, "UFTerceiro");
                    break;
                case "cExportador":
                    g.CodigoExportador = l.Texto(g.CodigoExportador, "CodigoExportador");
                    break;
                case "adi":
                    l.Lista(g.Adicoes, "Adicoes", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, DetalheExportacao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nDraw":
                    g.Drawback = l.Texto(g.Drawback, "Drawback");
                    break;
                case "exportInd":
                    g.Indireta = l.Grupo(g.Indireta, "Indireta", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Rastro g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nLote":
                    g.Lote = l.Texto(g.Lote, "Lote");
                    break;
                case "qLote":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "dFab":
                    g.Fabricacao = l.Data(g.Fabricacao, "Fabricacao");
                    break;
                case "dVal":
                    g.Validade = l.Data(g.Validade, "Validade");
                    break;
                case "cAgreg":
                    g.CodigoAgregacao = l.Texto(g.CodigoAgregacao, "CodigoAgregacao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ProdutoNff g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cProdFisco":
                    g.CodigoFisco = l.Texto(g.CodigoFisco, "CodigoFisco");
                    break;
                case "cOperNFF":
                    g.CodigoOperacao = l.Inteiro(g.CodigoOperacao, "CodigoOperacao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, EmbalagemProduto g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "xEmb":
                    g.Descricao = l.Texto(g.Descricao, "Descricao");
                    break;
                case "qVolEmb":
                    g.Volume = l.Decimal(g.Volume, "Volume");
                    break;
                case "uEmb":
                    g.Unidade = l.Texto(g.Unidade, "Unidade");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, VeiculoNovo g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "tpOp":
                    g.TipoOperacao = l.Enumeracao(g.TipoOperacao, "TipoOperacao");
                    break;
                case "chassi":
                    g.Chassi = l.Texto(g.Chassi, "Chassi");
                    break;
                case "cCor":
                    g.CodigoCor = l.Texto(g.CodigoCor, "CodigoCor");
                    break;
                case "xCor":
                    g.Cor = l.Texto(g.Cor, "Cor");
                    break;
                case "pot":
                    g.Potencia = l.Texto(g.Potencia, "Potencia");
                    break;
                case "cilin":
                    g.Cilindrada = l.Texto(g.Cilindrada, "Cilindrada");
                    break;
                case "pesoL":
                    g.PesoLiquido = l.Texto(g.PesoLiquido, "PesoLiquido");
                    break;
                case "pesoB":
                    g.PesoBruto = l.Texto(g.PesoBruto, "PesoBruto");
                    break;
                case "nSerie":
                    g.Serie = l.Texto(g.Serie, "Serie");
                    break;
                case "tpComb":
                    g.TipoCombustivel = l.Texto(g.TipoCombustivel, "TipoCombustivel");
                    break;
                case "nMotor":
                    g.Motor = l.Texto(g.Motor, "Motor");
                    break;
                case "CMT":
                    g.CapacidadeTracao = l.Texto(g.CapacidadeTracao, "CapacidadeTracao");
                    break;
                case "dist":
                    g.DistanciaEixos = l.Texto(g.DistanciaEixos, "DistanciaEixos");
                    break;
                case "anoMod":
                    g.AnoModelo = l.Inteiro(g.AnoModelo, "AnoModelo");
                    break;
                case "anoFab":
                    g.AnoFabricacao = l.Inteiro(g.AnoFabricacao, "AnoFabricacao");
                    break;
                case "tpPint":
                    g.TipoPintura = l.Texto(g.TipoPintura, "TipoPintura");
                    break;
                case "tpVeic":
                    g.TipoVeiculo = l.Texto(g.TipoVeiculo, "TipoVeiculo");
                    break;
                case "espVeic":
                    g.EspecieVeiculo = l.Inteiro(g.EspecieVeiculo, "EspecieVeiculo");
                    break;
                case "VIN":
                    g.Vin = l.Literal(g.Vin, "Vin", Literais.LerCondicaoVin);
                    break;
                case "condVeic":
                    g.Condicao = l.Enumeracao(g.Condicao, "Condicao");
                    break;
                case "cMod":
                    g.MarcaModelo = l.Texto(g.MarcaModelo, "MarcaModelo");
                    break;
                case "cCorDENATRAN":
                    g.CorDenatran = l.Texto(g.CorDenatran, "CorDenatran");
                    break;
                case "lota":
                    g.Lotacao = l.Inteiro(g.Lotacao, "Lotacao");
                    break;
                case "tpRest":
                    g.Restricao = l.Enumeracao(g.Restricao, "Restricao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Medicamento g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cProdANVISA":
                    g.CodigoAnvisa = l.Texto(g.CodigoAnvisa, "CodigoAnvisa");
                    break;
                case "xMotivoIsencao":
                    g.MotivoIsencao = l.Texto(g.MotivoIsencao, "MotivoIsencao");
                    break;
                case "vPMC":
                    g.PrecoMaximoConsumidor = l.Decimal(g.PrecoMaximoConsumidor, "PrecoMaximoConsumidor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Arma g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "tpArma":
                    g.Tipo = l.Enumeracao(g.Tipo, "Tipo");
                    break;
                case "nSerie":
                    g.Serie = l.Texto(g.Serie, "Serie");
                    break;
                case "nCano":
                    g.SerieCano = l.Texto(g.SerieCano, "SerieCano");
                    break;
                case "descr":
                    g.Descricao = l.Texto(g.Descricao, "Descricao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Combustivel g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cProdANP":
                    g.CodigoAnp = l.Texto(g.CodigoAnp, "CodigoAnp");
                    break;
                case "descANP":
                    g.DescricaoAnp = l.Texto(g.DescricaoAnp, "DescricaoAnp");
                    break;
                case "pGLP":
                    g.PercentualGlp = l.Decimal(g.PercentualGlp, "PercentualGlp");
                    break;
                case "pGNn":
                    g.PercentualGasNacional = l.Decimal(g.PercentualGasNacional, "PercentualGasNacional");
                    break;
                case "pGNi":
                    g.PercentualGasImportado = l.Decimal(g.PercentualGasImportado, "PercentualGasImportado");
                    break;
                case "vPart":
                    g.ValorPartida = l.Decimal(g.ValorPartida, "ValorPartida");
                    break;
                case "CODIF":
                    g.Codif = l.Texto(g.Codif, "Codif");
                    break;
                case "qTemp":
                    g.QuantidadeTemperaturaAmbiente = l.Decimal(g.QuantidadeTemperaturaAmbiente, "QuantidadeTemperaturaAmbiente");
                    break;
                case "UFCons":
                    (g.UFConsumo, g.Exterior) = l.SiglaExterior(g.UFConsumo, g.Exterior, "UFConsumo");
                    break;
                case "CIDE":
                    g.Cide = l.Grupo(g.Cide, "Cide", Ler);
                    break;
                case "encerrante":
                    g.Encerrante = l.Grupo(g.Encerrante, "Encerrante", Ler);
                    break;
                case "pBio":
                    g.PercentualBiodiesel = l.Decimal(g.PercentualBiodiesel, "PercentualBiodiesel");
                    break;
                case "origComb":
                    l.Lista(g.Origens, "Origens", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms00 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "pFCP":
                    g.PercentualFcp = l.Decimal(g.PercentualFcp, "PercentualFcp");
                    break;
                case "vFCP":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms02 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "qBCMono":
                    g.QuantidadeBaseMonofasico = l.Decimal(g.QuantidadeBaseMonofasico, "QuantidadeBaseMonofasico");
                    break;
                case "adRemICMS":
                    g.AliquotaAdRem = l.Decimal(g.AliquotaAdRem, "AliquotaAdRem");
                    break;
                case "vICMSMono":
                    g.ValorMonofasico = l.Decimal(g.ValorMonofasico, "ValorMonofasico");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms10 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vBCFCP":
                    g.BaseCalculoFcp = l.Decimal(g.BaseCalculoFcp, "BaseCalculoFcp");
                    break;
                case "pFCP":
                    g.PercentualFcp = l.Decimal(g.PercentualFcp, "PercentualFcp");
                    break;
                case "vFCP":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "vICMSSTDeson":
                    g.ValorStDesonerado = l.Decimal(g.ValorStDesonerado, "ValorStDesonerado");
                    break;
                case "motDesICMSST":
                    g.MotivoDesoneracaoSt = l.Enumeracao(g.MotivoDesoneracaoSt, "MotivoDesoneracaoSt");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms15 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "qBCMono":
                    g.QuantidadeBaseMonofasico = l.Decimal(g.QuantidadeBaseMonofasico, "QuantidadeBaseMonofasico");
                    break;
                case "adRemICMS":
                    g.AliquotaAdRem = l.Decimal(g.AliquotaAdRem, "AliquotaAdRem");
                    break;
                case "vICMSMono":
                    g.ValorMonofasico = l.Decimal(g.ValorMonofasico, "ValorMonofasico");
                    break;
                case "qBCMonoReten":
                    g.QuantidadeBaseRetencao = l.Decimal(g.QuantidadeBaseRetencao, "QuantidadeBaseRetencao");
                    break;
                case "adRemICMSReten":
                    g.AliquotaAdRemRetencao = l.Decimal(g.AliquotaAdRemRetencao, "AliquotaAdRemRetencao");
                    break;
                case "vICMSMonoReten":
                    g.ValorMonofasicoRetencao = l.Decimal(g.ValorMonofasicoRetencao, "ValorMonofasicoRetencao");
                    break;
                case "pRedAdRem":
                    g.PercentualReducaoAdRem = l.Decimal(g.PercentualReducaoAdRem, "PercentualReducaoAdRem");
                    break;
                case "motRedAdRem":
                    g.MotivoReducaoAdRem = l.Enumeracao(g.MotivoReducaoAdRem, "MotivoReducaoAdRem");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms20 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "pRedBC":
                    g.PercentualReducaoBase = l.Decimal(g.PercentualReducaoBase, "PercentualReducaoBase");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vBCFCP":
                    g.BaseCalculoFcp = l.Decimal(g.BaseCalculoFcp, "BaseCalculoFcp");
                    break;
                case "pFCP":
                    g.PercentualFcp = l.Decimal(g.PercentualFcp, "PercentualFcp");
                    break;
                case "vFCP":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                case "vICMSDeson":
                    g.ValorDesonerado = l.Decimal(g.ValorDesonerado, "ValorDesonerado");
                    break;
                case "motDesICMS":
                    g.MotivoDesoneracao = l.Enumeracao(g.MotivoDesoneracao, "MotivoDesoneracao");
                    break;
                case "indDeduzDeson":
                    g.DeducaoDesoneracao = l.Enumeracao(g.DeducaoDesoneracao, "DeducaoDesoneracao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms30 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "vICMSDeson":
                    g.ValorDesonerado = l.Decimal(g.ValorDesonerado, "ValorDesonerado");
                    break;
                case "motDesICMS":
                    g.MotivoDesoneracao = l.Enumeracao(g.MotivoDesoneracao, "MotivoDesoneracao");
                    break;
                case "indDeduzDeson":
                    g.DeducaoDesoneracao = l.Enumeracao(g.DeducaoDesoneracao, "DeducaoDesoneracao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms40 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vICMSDeson":
                    g.ValorDesonerado = l.Decimal(g.ValorDesonerado, "ValorDesonerado");
                    break;
                case "motDesICMS":
                    g.MotivoDesoneracao = l.Enumeracao(g.MotivoDesoneracao, "MotivoDesoneracao");
                    break;
                case "indDeduzDeson":
                    g.DeducaoDesoneracao = l.Enumeracao(g.DeducaoDesoneracao, "DeducaoDesoneracao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms51 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "pRedBC":
                    g.PercentualReducaoBase = l.Decimal(g.PercentualReducaoBase, "PercentualReducaoBase");
                    break;
                case "cBenefRBC":
                    g.CodigoBeneficioReducao = l.Texto(g.CodigoBeneficioReducao, "CodigoBeneficioReducao");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMSOp":
                    g.ValorOperacao = l.Decimal(g.ValorOperacao, "ValorOperacao");
                    break;
                case "pDif":
                    g.PercentualDiferimento = l.Decimal(g.PercentualDiferimento, "PercentualDiferimento");
                    break;
                case "vICMSDif":
                    g.ValorDiferido = l.Decimal(g.ValorDiferido, "ValorDiferido");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vBCFCP":
                    g.BaseCalculoFcp = l.Decimal(g.BaseCalculoFcp, "BaseCalculoFcp");
                    break;
                case "pFCP":
                    g.PercentualFcp = l.Decimal(g.PercentualFcp, "PercentualFcp");
                    break;
                case "vFCP":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                case "pFCPDif":
                    g.PercentualDiferimentoFcp = l.Decimal(g.PercentualDiferimentoFcp, "PercentualDiferimentoFcp");
                    break;
                case "vFCPDif":
                    g.ValorFcpDiferido = l.Decimal(g.ValorFcpDiferido, "ValorFcpDiferido");
                    break;
                case "vFCPEfet":
                    g.ValorFcpEfetivo = l.Decimal(g.ValorFcpEfetivo, "ValorFcpEfetivo");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms53 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "qBCMono":
                    g.QuantidadeBaseMonofasico = l.Decimal(g.QuantidadeBaseMonofasico, "QuantidadeBaseMonofasico");
                    break;
                case "adRemICMS":
                    g.AliquotaAdRem = l.Decimal(g.AliquotaAdRem, "AliquotaAdRem");
                    break;
                case "vICMSMonoOp":
                    g.ValorMonofasicoOperacao = l.Decimal(g.ValorMonofasicoOperacao, "ValorMonofasicoOperacao");
                    break;
                case "pDif":
                    g.PercentualDiferimento = l.Decimal(g.PercentualDiferimento, "PercentualDiferimento");
                    break;
                case "vICMSMonoDif":
                    g.ValorMonofasicoDiferido = l.Decimal(g.ValorMonofasicoDiferido, "ValorMonofasicoDiferido");
                    break;
                case "vICMSMono":
                    g.ValorMonofasico = l.Decimal(g.ValorMonofasico, "ValorMonofasico");
                    break;
                case "qBCMonoDif":
                    g.QuantidadeBaseDiferida = l.Decimal(g.QuantidadeBaseDiferida, "QuantidadeBaseDiferida");
                    break;
                case "adRemICMSDif":
                    g.AliquotaAdRemDiferida = l.Decimal(g.AliquotaAdRemDiferida, "AliquotaAdRemDiferida");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms60 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vBCSTRet":
                    g.BaseCalculoStRetido = l.Decimal(g.BaseCalculoStRetido, "BaseCalculoStRetido");
                    break;
                case "pST":
                    g.AliquotaConsumidorFinal = l.Decimal(g.AliquotaConsumidorFinal, "AliquotaConsumidorFinal");
                    break;
                case "vICMSSubstituto":
                    g.ValorSubstituto = l.Decimal(g.ValorSubstituto, "ValorSubstituto");
                    break;
                case "vICMSSTRet":
                    g.ValorStRetido = l.Decimal(g.ValorStRetido, "ValorStRetido");
                    break;
                case "vBCFCPSTRet":
                    g.BaseCalculoFcpStRetido = l.Decimal(g.BaseCalculoFcpStRetido, "BaseCalculoFcpStRetido");
                    break;
                case "pFCPSTRet":
                    g.PercentualFcpStRetido = l.Decimal(g.PercentualFcpStRetido, "PercentualFcpStRetido");
                    break;
                case "vFCPSTRet":
                    g.ValorFcpStRetido = l.Decimal(g.ValorFcpStRetido, "ValorFcpStRetido");
                    break;
                case "pRedBCEfet":
                    g.PercentualReducaoBaseEfetiva = l.Decimal(g.PercentualReducaoBaseEfetiva, "PercentualReducaoBaseEfetiva");
                    break;
                case "vBCEfet":
                    g.BaseCalculoEfetiva = l.Decimal(g.BaseCalculoEfetiva, "BaseCalculoEfetiva");
                    break;
                case "pICMSEfet":
                    g.AliquotaEfetiva = l.Decimal(g.AliquotaEfetiva, "AliquotaEfetiva");
                    break;
                case "vICMSEfet":
                    g.ValorEfetivo = l.Decimal(g.ValorEfetivo, "ValorEfetivo");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms61 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "qBCMonoRet":
                    g.QuantidadeBaseRetida = l.Decimal(g.QuantidadeBaseRetida, "QuantidadeBaseRetida");
                    break;
                case "adRemICMSRet":
                    g.AliquotaAdRemRetida = l.Decimal(g.AliquotaAdRemRetida, "AliquotaAdRemRetida");
                    break;
                case "vICMSMonoRet":
                    g.ValorMonofasicoRetido = l.Decimal(g.ValorMonofasicoRetido, "ValorMonofasicoRetido");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms70 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "pRedBC":
                    g.PercentualReducaoBase = l.Decimal(g.PercentualReducaoBase, "PercentualReducaoBase");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vBCFCP":
                    g.BaseCalculoFcp = l.Decimal(g.BaseCalculoFcp, "BaseCalculoFcp");
                    break;
                case "pFCP":
                    g.PercentualFcp = l.Decimal(g.PercentualFcp, "PercentualFcp");
                    break;
                case "vFCP":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "vICMSDeson":
                    g.ValorDesonerado = l.Decimal(g.ValorDesonerado, "ValorDesonerado");
                    break;
                case "motDesICMS":
                    g.MotivoDesoneracao = l.Enumeracao(g.MotivoDesoneracao, "MotivoDesoneracao");
                    break;
                case "indDeduzDeson":
                    g.DeducaoDesoneracao = l.Enumeracao(g.DeducaoDesoneracao, "DeducaoDesoneracao");
                    break;
                case "vICMSSTDeson":
                    g.ValorStDesonerado = l.Decimal(g.ValorStDesonerado, "ValorStDesonerado");
                    break;
                case "motDesICMSST":
                    g.MotivoDesoneracaoSt = l.Enumeracao(g.MotivoDesoneracaoSt, "MotivoDesoneracaoSt");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Icms90 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pRedBC":
                    g.PercentualReducaoBase = l.Decimal(g.PercentualReducaoBase, "PercentualReducaoBase");
                    break;
                case "cBenefRBC":
                    g.CodigoBeneficioReducao = l.Texto(g.CodigoBeneficioReducao, "CodigoBeneficioReducao");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMSOp":
                    g.ValorOperacao = l.Decimal(g.ValorOperacao, "ValorOperacao");
                    break;
                case "pDif":
                    g.PercentualDiferimento = l.Decimal(g.PercentualDiferimento, "PercentualDiferimento");
                    break;
                case "vICMSDif":
                    g.ValorDiferido = l.Decimal(g.ValorDiferido, "ValorDiferido");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vBCFCP":
                    g.BaseCalculoFcp = l.Decimal(g.BaseCalculoFcp, "BaseCalculoFcp");
                    break;
                case "pFCP":
                    g.PercentualFcp = l.Decimal(g.PercentualFcp, "PercentualFcp");
                    break;
                case "vFCP":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                case "pFCPDif":
                    g.PercentualDiferimentoFcp = l.Decimal(g.PercentualDiferimentoFcp, "PercentualDiferimentoFcp");
                    break;
                case "vFCPDif":
                    g.ValorFcpDiferido = l.Decimal(g.ValorFcpDiferido, "ValorFcpDiferido");
                    break;
                case "vFCPEfet":
                    g.ValorFcpEfetivo = l.Decimal(g.ValorFcpEfetivo, "ValorFcpEfetivo");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "vICMSDeson":
                    g.ValorDesonerado = l.Decimal(g.ValorDesonerado, "ValorDesonerado");
                    break;
                case "motDesICMS":
                    g.MotivoDesoneracao = l.Enumeracao(g.MotivoDesoneracao, "MotivoDesoneracao");
                    break;
                case "indDeduzDeson":
                    g.DeducaoDesoneracao = l.Enumeracao(g.DeducaoDesoneracao, "DeducaoDesoneracao");
                    break;
                case "vICMSSTDeson":
                    g.ValorStDesonerado = l.Decimal(g.ValorStDesonerado, "ValorStDesonerado");
                    break;
                case "motDesICMSST":
                    g.MotivoDesoneracaoSt = l.Enumeracao(g.MotivoDesoneracaoSt, "MotivoDesoneracaoSt");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsPartilha g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pRedBC":
                    g.PercentualReducaoBase = l.Decimal(g.PercentualReducaoBase, "PercentualReducaoBase");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "pBCOp":
                    g.PercentualBaseOperacao = l.Decimal(g.PercentualBaseOperacao, "PercentualBaseOperacao");
                    break;
                case "UFST":
                    (g.UFSt, g.Exterior) = l.SiglaExterior(g.UFSt, g.Exterior, "UFSt");
                    break;
                case "vICMSDeson":
                    g.ValorDesonerado = l.Decimal(g.ValorDesonerado, "ValorDesonerado");
                    break;
                case "motDesICMS":
                    g.MotivoDesoneracao = l.Enumeracao(g.MotivoDesoneracao, "MotivoDesoneracao");
                    break;
                case "indDeduzDeson":
                    g.DeducaoDesoneracao = l.Enumeracao(g.DeducaoDesoneracao, "DeducaoDesoneracao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsRepasse g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vBCSTRet":
                    g.BaseCalculoStRetido = l.Decimal(g.BaseCalculoStRetido, "BaseCalculoStRetido");
                    break;
                case "pST":
                    g.AliquotaConsumidorFinal = l.Decimal(g.AliquotaConsumidorFinal, "AliquotaConsumidorFinal");
                    break;
                case "vICMSSubstituto":
                    g.ValorSubstituto = l.Decimal(g.ValorSubstituto, "ValorSubstituto");
                    break;
                case "vICMSSTRet":
                    g.ValorStRetido = l.Decimal(g.ValorStRetido, "ValorStRetido");
                    break;
                case "vBCFCPSTRet":
                    g.BaseCalculoFcpStRetido = l.Decimal(g.BaseCalculoFcpStRetido, "BaseCalculoFcpStRetido");
                    break;
                case "pFCPSTRet":
                    g.PercentualFcpStRetido = l.Decimal(g.PercentualFcpStRetido, "PercentualFcpStRetido");
                    break;
                case "vFCPSTRet":
                    g.ValorFcpStRetido = l.Decimal(g.ValorFcpStRetido, "ValorFcpStRetido");
                    break;
                case "vBCSTDest":
                    g.BaseCalculoStDestino = l.Decimal(g.BaseCalculoStDestino, "BaseCalculoStDestino");
                    break;
                case "vICMSSTDest":
                    g.ValorStDestino = l.Decimal(g.ValorStDestino, "ValorStDestino");
                    break;
                case "pRedBCEfet":
                    g.PercentualReducaoBaseEfetiva = l.Decimal(g.PercentualReducaoBaseEfetiva, "PercentualReducaoBaseEfetiva");
                    break;
                case "vBCEfet":
                    g.BaseCalculoEfetiva = l.Decimal(g.BaseCalculoEfetiva, "BaseCalculoEfetiva");
                    break;
                case "pICMSEfet":
                    g.AliquotaEfetiva = l.Decimal(g.AliquotaEfetiva, "AliquotaEfetiva");
                    break;
                case "vICMSEfet":
                    g.ValorEfetivo = l.Decimal(g.ValorEfetivo, "ValorEfetivo");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsSn101 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CSOSN":
                    g.Csosn = l.Enumeracao(g.Csosn, "Csosn");
                    break;
                case "pCredSN":
                    g.AliquotaCredito = l.Decimal(g.AliquotaCredito, "AliquotaCredito");
                    break;
                case "vCredICMSSN":
                    g.ValorCredito = l.Decimal(g.ValorCredito, "ValorCredito");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsSn102 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CSOSN":
                    g.Csosn = l.Enumeracao(g.Csosn, "Csosn");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsSn201 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CSOSN":
                    g.Csosn = l.Enumeracao(g.Csosn, "Csosn");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "pCredSN":
                    g.AliquotaCredito = l.Decimal(g.AliquotaCredito, "AliquotaCredito");
                    break;
                case "vCredICMSSN":
                    g.ValorCredito = l.Decimal(g.ValorCredito, "ValorCredito");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsSn202 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CSOSN":
                    g.Csosn = l.Enumeracao(g.Csosn, "Csosn");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsSn500 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CSOSN":
                    g.Csosn = l.Enumeracao(g.Csosn, "Csosn");
                    break;
                case "vBCSTRet":
                    g.BaseCalculoStRetido = l.Decimal(g.BaseCalculoStRetido, "BaseCalculoStRetido");
                    break;
                case "pST":
                    g.AliquotaConsumidorFinal = l.Decimal(g.AliquotaConsumidorFinal, "AliquotaConsumidorFinal");
                    break;
                case "vICMSSubstituto":
                    g.ValorSubstituto = l.Decimal(g.ValorSubstituto, "ValorSubstituto");
                    break;
                case "vICMSSTRet":
                    g.ValorStRetido = l.Decimal(g.ValorStRetido, "ValorStRetido");
                    break;
                case "vBCFCPSTRet":
                    g.BaseCalculoFcpStRetido = l.Decimal(g.BaseCalculoFcpStRetido, "BaseCalculoFcpStRetido");
                    break;
                case "pFCPSTRet":
                    g.PercentualFcpStRetido = l.Decimal(g.PercentualFcpStRetido, "PercentualFcpStRetido");
                    break;
                case "vFCPSTRet":
                    g.ValorFcpStRetido = l.Decimal(g.ValorFcpStRetido, "ValorFcpStRetido");
                    break;
                case "pRedBCEfet":
                    g.PercentualReducaoBaseEfetiva = l.Decimal(g.PercentualReducaoBaseEfetiva, "PercentualReducaoBaseEfetiva");
                    break;
                case "vBCEfet":
                    g.BaseCalculoEfetiva = l.Decimal(g.BaseCalculoEfetiva, "BaseCalculoEfetiva");
                    break;
                case "pICMSEfet":
                    g.AliquotaEfetiva = l.Decimal(g.AliquotaEfetiva, "AliquotaEfetiva");
                    break;
                case "vICMSEfet":
                    g.ValorEfetivo = l.Decimal(g.ValorEfetivo, "ValorEfetivo");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsSn900 g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "orig":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "CSOSN":
                    g.Csosn = l.Enumeracao(g.Csosn, "Csosn");
                    break;
                case "modBC":
                    g.ModalidadeBaseCalculo = l.Enumeracao(g.ModalidadeBaseCalculo, "ModalidadeBaseCalculo");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pRedBC":
                    g.PercentualReducaoBase = l.Decimal(g.PercentualReducaoBase, "PercentualReducaoBase");
                    break;
                case "pICMS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "modBCST":
                    g.ModalidadeBaseCalculoSt = l.Enumeracao(g.ModalidadeBaseCalculoSt, "ModalidadeBaseCalculoSt");
                    break;
                case "pMVAST":
                    g.PercentualMargemSt = l.Decimal(g.PercentualMargemSt, "PercentualMargemSt");
                    break;
                case "pRedBCST":
                    g.PercentualReducaoBaseSt = l.Decimal(g.PercentualReducaoBaseSt, "PercentualReducaoBaseSt");
                    break;
                case "vBCST":
                    g.BaseCalculoSt = l.Decimal(g.BaseCalculoSt, "BaseCalculoSt");
                    break;
                case "pICMSST":
                    g.AliquotaSt = l.Decimal(g.AliquotaSt, "AliquotaSt");
                    break;
                case "vICMSST":
                    g.ValorSt = l.Decimal(g.ValorSt, "ValorSt");
                    break;
                case "vBCFCPST":
                    g.BaseCalculoFcpSt = l.Decimal(g.BaseCalculoFcpSt, "BaseCalculoFcpSt");
                    break;
                case "pFCPST":
                    g.PercentualFcpSt = l.Decimal(g.PercentualFcpSt, "PercentualFcpSt");
                    break;
                case "vFCPST":
                    g.ValorFcpSt = l.Decimal(g.ValorFcpSt, "ValorFcpSt");
                    break;
                case "pCredSN":
                    g.AliquotaCredito = l.Decimal(g.AliquotaCredito, "AliquotaCredito");
                    break;
                case "vCredICMSSN":
                    g.ValorCredito = l.Decimal(g.ValorCredito, "ValorCredito");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Ipi g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJProd":
                    g.CnpjProdutor = l.Texto(g.CnpjProdutor, "CnpjProdutor");
                    break;
                case "cSelo":
                    g.CodigoSelo = l.Texto(g.CodigoSelo, "CodigoSelo");
                    break;
                case "qSelo":
                    g.QuantidadeSelo = l.Inteiro(g.QuantidadeSelo, "QuantidadeSelo");
                    break;
                case "cEnq":
                    g.CodigoEnquadramento = l.Texto(g.CodigoEnquadramento, "CodigoEnquadramento");
                    break;
                case "IPITrib":
                    g.Tributado = l.Grupo(g.Tributado, "Tributado", Ler);
                    break;
                case "IPINT":
                    g.NaoTributado = l.Grupo(g.NaoTributado, "NaoTributado", Ler);
                    break;
                case "clEnq":
                    l.Retirado("a NT 2016.002 v1.40 (24/10/2017)");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ImpostoImportacao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "vDespAdu":
                    g.DespesasAduaneiras = l.Decimal(g.DespesasAduaneiras, "DespesasAduaneiras");
                    break;
                case "vII":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vIOF":
                    g.Iof = l.Decimal(g.Iof, "Iof");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Issqn g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "vAliq":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vISSQN":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "cMunFG":
                    g.MunicipioFatoGerador = l.Inteiro(g.MunicipioFatoGerador, "MunicipioFatoGerador");
                    break;
                case "cListServ":
                    g.ItemListaServico = l.Texto(g.ItemListaServico, "ItemListaServico");
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
                case "indISS":
                    g.Exigibilidade = l.Enumeracao(g.Exigibilidade, "Exigibilidade");
                    break;
                case "cServico":
                    g.CodigoServico = l.Texto(g.CodigoServico, "CodigoServico");
                    break;
                case "cMun":
                    g.MunicipioIncidencia = l.Inteiro(g.MunicipioIncidencia, "MunicipioIncidencia");
                    break;
                case "cPais":
                    g.CodigoPais = l.Inteiro(g.CodigoPais, "CodigoPais");
                    break;
                case "nProcesso":
                    g.Processo = l.Texto(g.Processo, "Processo");
                    break;
                case "indIncentivo":
                    g.IncentivoFiscal = l.Enumeracao(g.IncentivoFiscal, "IncentivoFiscal");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, PisAliquota g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pPIS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vPIS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, PisNaoTributado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, PisOutros g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pPIS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "qBCProd":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vAliqProd":
                    g.AliquotaUnidade = l.Decimal(g.AliquotaUnidade, "AliquotaUnidade");
                    break;
                case "vPIS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, PisQuantidade g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "qBCProd":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vAliqProd":
                    g.AliquotaUnidade = l.Decimal(g.AliquotaUnidade, "AliquotaUnidade");
                    break;
                case "vPIS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, PisSt g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pPIS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "qBCProd":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vAliqProd":
                    g.AliquotaUnidade = l.Decimal(g.AliquotaUnidade, "AliquotaUnidade");
                    break;
                case "vPIS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "indSomaPISST":
                    g.Totalizacao = l.Enumeracao(g.Totalizacao, "Totalizacao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CofinsAliquota g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pCOFINS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vCOFINS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CofinsNaoTributado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CofinsOutros g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pCOFINS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "qBCProd":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vAliqProd":
                    g.AliquotaUnidade = l.Decimal(g.AliquotaUnidade, "AliquotaUnidade");
                    break;
                case "vCOFINS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CofinsQuantidade g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "qBCProd":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vAliqProd":
                    g.AliquotaUnidade = l.Decimal(g.AliquotaUnidade, "AliquotaUnidade");
                    break;
                case "vCOFINS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CofinsSt g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pCOFINS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "qBCProd":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vAliqProd":
                    g.AliquotaUnidade = l.Decimal(g.AliquotaUnidade, "AliquotaUnidade");
                    break;
                case "vCOFINS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "indSomaCOFINSST":
                    g.Totalizacao = l.Enumeracao(g.Totalizacao, "Totalizacao");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IcmsUFDestino g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBCUFDest":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "vBCFCPUFDest":
                    g.BaseCalculoFcp = l.Decimal(g.BaseCalculoFcp, "BaseCalculoFcp");
                    break;
                case "pFCPUFDest":
                    g.PercentualFcp = l.Decimal(g.PercentualFcp, "PercentualFcp");
                    break;
                case "pICMSUFDest":
                    g.AliquotaInterna = l.Decimal(g.AliquotaInterna, "AliquotaInterna");
                    break;
                case "pICMSInter":
                    g.AliquotaInterestadual = l.Decimal(g.AliquotaInterestadual, "AliquotaInterestadual");
                    break;
                case "pICMSInterPart":
                    g.PercentualPartilha = l.Decimal(g.PercentualPartilha, "PercentualPartilha");
                    break;
                case "vFCPUFDest":
                    g.ValorFcp = l.Decimal(g.ValorFcp, "ValorFcp");
                    break;
                case "vICMSUFDest":
                    g.ValorDestino = l.Decimal(g.ValorDestino, "ValorDestino");
                    break;
                case "vICMSUFRemet":
                    g.ValorRemetente = l.Decimal(g.ValorRemetente, "ValorRemetente");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ImpostoSeletivo g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CSTIS":
                    g.Cst = l.Texto(g.Cst, "Cst");
                    break;
                case "cClassTribIS":
                    g.ClassificacaoTributaria = l.Texto(g.ClassificacaoTributaria, "ClassificacaoTributaria");
                    break;
                case "vBCIS":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pIS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "adRemIS":
                    g.AliquotaEspecifica = l.Decimal(g.AliquotaEspecifica, "AliquotaEspecifica");
                    break;
                case "uTrib":
                    g.UnidadeTributavel = l.Texto(g.UnidadeTributavel, "UnidadeTributavel");
                    break;
                case "qTrib":
                    g.QuantidadeTributavel = l.Decimal(g.QuantidadeTributavel, "QuantidadeTributavel");
                    break;
                case "vIS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "pISEspec":
                    l.Retirado("as versões da NT 2025.002 anteriores à v1.50 (010a ao 010e), retirado no 010f");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IbsCbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Texto(g.Cst, "Cst");
                    break;
                case "cClassTrib":
                    g.ClassificacaoTributaria = l.Texto(g.ClassificacaoTributaria, "ClassificacaoTributaria");
                    break;
                case "indDoacao":
                    g.Doacao = l.Enumeracao(g.Doacao, "Doacao");
                    break;
                case "gIBSCBS":
                    g.Tributacao = l.Grupo(g.Tributacao, "Tributacao", Ler);
                    break;
                case "gIBSCBSMono":
                    g.Monofasia = l.Grupo(g.Monofasia, "Monofasia", Ler);
                    break;
                case "gTransfCred":
                    g.TransferenciaCredito = l.Grupo(g.TransferenciaCredito, "TransferenciaCredito", Ler);
                    break;
                case "gAjusteCompet":
                    g.AjusteCompetencia = l.Grupo(g.AjusteCompetencia, "AjusteCompetencia", Ler);
                    break;
                case "gEstornoCred":
                    g.EstornoCredito = l.Grupo(g.EstornoCredito, "EstornoCredito", Ler);
                    break;
                case "gCredPresOper":
                    g.CreditoPresumido = l.Grupo(g.CreditoPresumido, "CreditoPresumido", Ler);
                    break;
                case "gCredPresIBSZFM":
                    g.CreditoPresumidoZfm = l.Grupo(g.CreditoPresumidoZfm, "CreditoPresumidoZfm", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IpiDevolvido g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vIPIDevol":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Adicao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nAdicao":
                    g.Numero = l.Inteiro(g.Numero, "Numero");
                    break;
                case "nSeqAdic":
                    g.Sequencial = l.Inteiro(g.Sequencial, "Sequencial");
                    break;
                case "cFabricante":
                    g.CodigoFabricante = l.Texto(g.CodigoFabricante, "CodigoFabricante");
                    break;
                case "vDescDI":
                    g.Desconto = l.Decimal(g.Desconto, "Desconto");
                    break;
                case "nDraw":
                    g.Drawback = l.Texto(g.Drawback, "Drawback");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ExportacaoIndireta g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nRE":
                    g.RegistroExportacao = l.Texto(g.RegistroExportacao, "RegistroExportacao");
                    break;
                case "chNFe":
                    g.Chave = l.Texto(g.Chave, "Chave");
                    break;
                case "qExport":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Cide g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qBCProd":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "vAliqProd":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vCIDE":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Encerrante g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nBico":
                    g.Bico = l.Inteiro(g.Bico, "Bico");
                    break;
                case "nBomba":
                    g.Bomba = l.Inteiro(g.Bomba, "Bomba");
                    break;
                case "nTanque":
                    g.Tanque = l.Inteiro(g.Tanque, "Tanque");
                    break;
                case "vEncIni":
                    g.Inicial = l.Decimal(g.Inicial, "Inicial");
                    break;
                case "vEncFin":
                    g.Final = l.Decimal(g.Final, "Final");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, OrigemCombustivel g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "indImport":
                    g.Importacao = l.Enumeracao(g.Importacao, "Importacao");
                    break;
                case "cUFOrig":
                    g.UF = l.Enumeracao(g.UF, "UF");
                    break;
                case "pOrig":
                    g.Percentual = l.Decimal(g.Percentual, "Percentual");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IpiTributado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pIPI":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "qUnid":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vUnid":
                    g.ValorUnidade = l.Decimal(g.ValorUnidade, "ValorUnidade");
                    break;
                case "vIPI":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IpiNaoTributado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CST":
                    g.Cst = l.Enumeracao(g.Cst, "Cst");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TributacaoIbsCbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBC":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "gIBSUF":
                    g.IbsUF = l.Grupo(g.IbsUF, "IbsUF", Ler);
                    break;
                case "gIBSMun":
                    g.IbsMunicipio = l.Grupo(g.IbsMunicipio, "IbsMunicipio", Ler);
                    break;
                case "vIBS":
                    g.ValorIbs = l.Decimal(g.ValorIbs, "ValorIbs");
                    break;
                case "gCBS":
                    g.Cbs = l.Grupo(g.Cbs, "Cbs", Ler);
                    break;
                case "gTribRegular":
                    g.TributacaoRegular = l.Grupo(g.TributacaoRegular, "TributacaoRegular", Ler);
                    break;
                case "gTribCompraGov":
                    g.CompraGovernamental = l.Grupo(g.CompraGovernamental, "CompraGovernamental", Ler);
                    break;
                case "gIBSCredPres":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.20), substituído no 010b da v1.30");
                    break;
                case "gCBSCredPres":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.20), substituído no 010b da v1.30");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsCbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "gIBSMonoAdRem":
                    g.IbsAdRem = l.Grupo(g.IbsAdRem, "IbsAdRem", Ler);
                    break;
                case "gIBSMonoAdValorem":
                    g.IbsAdValorem = l.Grupo(g.IbsAdValorem, "IbsAdValorem", Ler);
                    break;
                case "gCBSMonoAdRem":
                    g.CbsAdRem = l.Grupo(g.CbsAdRem, "CbsAdRem", Ler);
                    break;
                case "gCBSMonoAdValorem":
                    g.CbsAdValorem = l.Grupo(g.CbsAdValorem, "CbsAdValorem", Ler);
                    break;
                case "vTotIBSMonoItem":
                    g.TotalIbs = l.Decimal(g.TotalIbs, "TotalIbs");
                    break;
                case "vTotCBSMonoItem":
                    g.TotalCbs = l.Decimal(g.TotalCbs, "TotalCbs");
                    break;
                case "adRemCBS":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "adRemCBSRet":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "adRemCBSReten":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "adRemIBS":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "adRemIBSRet":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "adRemIBSReten":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "pDifCBS":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "pDifIBS":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "qBCMono":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "qBCMonoRet":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "qBCMonoReten":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vCBSMono":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vCBSMonoDif":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vCBSMonoRet":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vCBSMonoReten":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vIBSMono":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vIBSMonoDif":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vIBSMonoRet":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "vIBSMonoReten":
                    l.Retirado("as versões preliminares da NT 2025.002 (010a ao 010b da v1.10), agrupado no 010b da v1.20");
                    break;
                case "gMonoPadrao":
                    l.Retirado("as versões da NT 2025.002 anteriores à v1.50 (010b da v1.20 ao 010e), reformulado no 010f");
                    break;
                case "gMonoReten":
                    l.Retirado("as versões da NT 2025.002 anteriores à v1.50 (010b da v1.20 ao 010e), reformulado no 010f");
                    break;
                case "gMonoRet":
                    l.Retirado("as versões da NT 2025.002 anteriores à v1.50 (010b da v1.20 ao 010e), reformulado no 010f");
                    break;
                case "gMonoDif":
                    l.Retirado("as versões da NT 2025.002 anteriores à v1.50 (010b da v1.20 ao 010e), reformulado no 010f");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TransferenciaCredito g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vIBS":
                    g.ValorIbs = l.Decimal(g.ValorIbs, "ValorIbs");
                    break;
                case "vCBS":
                    g.ValorCbs = l.Decimal(g.ValorCbs, "ValorCbs");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, AjusteCompetencia g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "competApur":
                    g.Competencia = l.Texto(g.Competencia, "Competencia");
                    break;
                case "vIBS":
                    g.ValorIbs = l.Decimal(g.ValorIbs, "ValorIbs");
                    break;
                case "vCBS":
                    g.ValorCbs = l.Decimal(g.ValorCbs, "ValorCbs");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CreditoPresumidoOperacao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBCCredPres":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "cCredPres":
                    g.Codigo = l.Texto(g.Codigo, "Codigo");
                    break;
                case "gIBSCredPres":
                    g.Ibs = l.Grupo(g.Ibs, "Ibs", Ler);
                    break;
                case "gCBSCredPres":
                    g.Cbs = l.Grupo(g.Cbs, "Cbs", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CreditoPresumidoZfm g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "competApur":
                    g.Competencia = l.Texto(g.Competencia, "Competencia");
                    break;
                case "tpCredPresIBSZFM":
                    g.Classificacao = l.Enumeracao(g.Classificacao, "Classificacao");
                    break;
                case "vCredPresIBSZFM":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, IbsUF g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pIBSUF":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "gDif":
                    g.Diferimento = l.Grupo(g.Diferimento, "Diferimento", Ler);
                    break;
                case "gDevTrib":
                    g.Devolucao = l.Grupo(g.Devolucao, "Devolucao", Ler);
                    break;
                case "gRed":
                    g.Reducao = l.Grupo(g.Reducao, "Reducao", Ler);
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

    private static void Ler(LeituraNFe l, IbsMunicipio g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pIBSMun":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "gDif":
                    g.Diferimento = l.Grupo(g.Diferimento, "Diferimento", Ler);
                    break;
                case "gDevTrib":
                    g.Devolucao = l.Grupo(g.Devolucao, "Devolucao", Ler);
                    break;
                case "gRed":
                    g.Reducao = l.Grupo(g.Reducao, "Reducao", Ler);
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

    private static void Ler(LeituraNFe l, Cbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pCBS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "gDif":
                    g.Diferimento = l.Grupo(g.Diferimento, "Diferimento", Ler);
                    break;
                case "gDevTrib":
                    g.Devolucao = l.Grupo(g.Devolucao, "Devolucao", Ler);
                    break;
                case "gRed":
                    g.Reducao = l.Grupo(g.Reducao, "Reducao", Ler);
                    break;
                case "gALCZFMCBS":
                    g.AreaIncentivada = l.Grupo(g.AreaIncentivada, "AreaIncentivada", Ler);
                    break;
                case "vCBS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TributacaoRegular g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CSTReg":
                    g.Cst = l.Texto(g.Cst, "Cst");
                    break;
                case "cClassTribReg":
                    g.ClassificacaoTributaria = l.Texto(g.ClassificacaoTributaria, "ClassificacaoTributaria");
                    break;
                case "pAliqEfetRegIBSUF":
                    g.AliquotaIbsUF = l.Decimal(g.AliquotaIbsUF, "AliquotaIbsUF");
                    break;
                case "vTribRegIBSUF":
                    g.ValorIbsUF = l.Decimal(g.ValorIbsUF, "ValorIbsUF");
                    break;
                case "pAliqEfetRegIBSMun":
                    g.AliquotaIbsMunicipio = l.Decimal(g.AliquotaIbsMunicipio, "AliquotaIbsMunicipio");
                    break;
                case "vTribRegIBSMun":
                    g.ValorIbsMunicipio = l.Decimal(g.ValorIbsMunicipio, "ValorIbsMunicipio");
                    break;
                case "pAliqEfetRegCBS":
                    g.AliquotaCbs = l.Decimal(g.AliquotaCbs, "AliquotaCbs");
                    break;
                case "vTribRegCBS":
                    g.ValorCbs = l.Decimal(g.ValorCbs, "ValorCbs");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TributacaoCompraGovernamental g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pAliqIBSUF":
                    g.AliquotaIbsUF = l.Decimal(g.AliquotaIbsUF, "AliquotaIbsUF");
                    break;
                case "vTribIBSUF":
                    g.ValorIbsUF = l.Decimal(g.ValorIbsUF, "ValorIbsUF");
                    break;
                case "pAliqIBSMun":
                    g.AliquotaIbsMunicipio = l.Decimal(g.AliquotaIbsMunicipio, "AliquotaIbsMunicipio");
                    break;
                case "vTribIBSMun":
                    g.ValorIbsMunicipio = l.Decimal(g.ValorIbsMunicipio, "ValorIbsMunicipio");
                    break;
                case "pAliqCBS":
                    g.AliquotaCbs = l.Decimal(g.AliquotaCbs, "AliquotaCbs");
                    break;
                case "vTribCBS":
                    g.ValorCbs = l.Decimal(g.ValorCbs, "ValorCbs");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsAdRem g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "gMonoPadrao":
                    g.Padrao = l.Grupo(g.Padrao, "Padrao", Ler);
                    break;
                case "gMonoReten":
                    g.Retencao = l.Grupo(g.Retencao, "Retencao", Ler);
                    break;
                case "gMonoRet":
                    g.Retido = l.Grupo(g.Retido, "Retido", Ler);
                    break;
                case "gpBioDiferenca":
                    g.Biocombustivel = l.Grupo(g.Biocombustivel, "Biocombustivel", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsAdValorem g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "gMonoPadrao":
                    g.Padrao = l.Grupo(g.Padrao, "Padrao", Ler);
                    break;
                case "gMonoReten":
                    g.Retencao = l.Grupo(g.Retencao, "Retencao", Ler);
                    break;
                case "gMonoRet":
                    g.Retido = l.Grupo(g.Retido, "Retido", Ler);
                    break;
                case "gpBioDiferenca":
                    g.Biocombustivel = l.Grupo(g.Biocombustivel, "Biocombustivel", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsAdRem g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "gMonoPadrao":
                    g.Padrao = l.Grupo(g.Padrao, "Padrao", Ler);
                    break;
                case "gMonoReten":
                    g.Retencao = l.Grupo(g.Retencao, "Retencao", Ler);
                    break;
                case "gMonoRet":
                    g.Retido = l.Grupo(g.Retido, "Retido", Ler);
                    break;
                case "gpBioDiferenca":
                    g.Biocombustivel = l.Grupo(g.Biocombustivel, "Biocombustivel", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsAdValorem g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "gMonoPadrao":
                    g.Padrao = l.Grupo(g.Padrao, "Padrao", Ler);
                    break;
                case "gMonoReten":
                    g.Retencao = l.Grupo(g.Retencao, "Retencao", Ler);
                    break;
                case "gMonoRet":
                    g.Retido = l.Grupo(g.Retido, "Retido", Ler);
                    break;
                case "gpBioDiferenca":
                    g.Biocombustivel = l.Grupo(g.Biocombustivel, "Biocombustivel", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CreditoPresumidoIbsCbs g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pCredPres":
                    g.Percentual = l.Decimal(g.Percentual, "Percentual");
                    break;
                case "vCredPres":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vCredPresCondSus":
                    g.ValorCondicaoSuspensiva = l.Decimal(g.ValorCondicaoSuspensiva, "ValorCondicaoSuspensiva");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Diferimento g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pDif":
                    g.Percentual = l.Decimal(g.Percentual, "Percentual");
                    break;
                case "vDif":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "vCBSOp":
                    l.Retirado("a NT 2025.002 v1.01 (pacote 010a), retirado no 010b");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, DevolucaoTributo g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pDevTrib":
                    g.Percentual = l.Decimal(g.Percentual, "Percentual");
                    break;
                case "vDevTrib":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ReducaoAliquota g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "pRedAliq":
                    g.Percentual = l.Decimal(g.Percentual, "Percentual");
                    break;
                case "pAliqEfet":
                    g.AliquotaEfetiva = l.Decimal(g.AliquotaEfetiva, "AliquotaEfetiva");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CbsAreaIncentivada g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "tpALCZFMCBS":
                    g.Tipo = l.Enumeracao(g.Tipo, "Tipo");
                    break;
                case "nProcSuframa":
                    g.ProcessoSuframa = l.Texto(g.ProcessoSuframa, "ProcessoSuframa");
                    break;
                case "pAliqEfetRegCBS":
                    g.AliquotaEfetiva = l.Decimal(g.AliquotaEfetiva, "AliquotaEfetiva");
                    break;
                case "vTribRegCBS":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsAdRemPadrao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qBCMono":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "adRemIBS":
                    g.AliquotaAdRem = l.Decimal(g.AliquotaAdRem, "AliquotaAdRem");
                    break;
                case "vIBSMono":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsAdRemRetencao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qBCMonoReten":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "adRemIBSReten":
                    g.AliquotaAdRem = l.Decimal(g.AliquotaAdRem, "AliquotaAdRem");
                    break;
                case "vIBSMonoReten":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsRetido g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vIBSMonoRet":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsBiocombustivel g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qBCBioComb":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vIBSDiferenca":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsAdValoremPadrao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBCMono":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pAliqMonoUF":
                    g.AliquotaUF = l.Decimal(g.AliquotaUF, "AliquotaUF");
                    break;
                case "vIBSMonoUF":
                    g.ValorUF = l.Decimal(g.ValorUF, "ValorUF");
                    break;
                case "pAliqMonoMun":
                    g.AliquotaMunicipio = l.Decimal(g.AliquotaMunicipio, "AliquotaMunicipio");
                    break;
                case "vIBSMonoMun":
                    g.ValorMunicipio = l.Decimal(g.ValorMunicipio, "ValorMunicipio");
                    break;
                case "vIBSMono":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaIbsAdValoremRetencao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBCMonoReten":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pAliqMonoReten":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vIBSMonoReten":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsAdRemPadrao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qBCMono":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "adRemCBS":
                    g.AliquotaAdRem = l.Decimal(g.AliquotaAdRem, "AliquotaAdRem");
                    break;
                case "vCBSMono":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsAdRemRetencao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qBCMonoReten":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "adRemCBSReten":
                    g.AliquotaAdRem = l.Decimal(g.AliquotaAdRem, "AliquotaAdRem");
                    break;
                case "vCBSMonoReten":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsRetido g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vCBSMonoRet":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsBiocombustivel g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qBCBioComb":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                case "vCBSDiferenca":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsAdValoremPadrao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBCMono":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pAliqMonoCBS":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vCBSMono":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, MonofasiaCbsAdValoremRetencao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vBCMonoReten":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pAliqMonoReten":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vCBSMonoReten":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }
}
