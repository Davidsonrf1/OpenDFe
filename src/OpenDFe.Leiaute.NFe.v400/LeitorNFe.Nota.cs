namespace OpenDFe.NFe;

// A leitura do cabeçalho e das partes (docs/LEITURA.md 3): um método por grupo, espelho do EscritorNFe.Nota; o pai
// entra e sai do grupo.
internal static partial class LeitorNFe
{
    private static void Ler(LeituraNFe l, NotaFiscal g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "ide":
                    g.Identificacao = l.Grupo(g.Identificacao, "Identificacao", Ler);
                    break;
                case "emit":
                    g.Emitente = l.Grupo(g.Emitente, "Emitente", Ler);
                    break;
                case "avulsa":
                    g.Avulsa = l.Grupo(g.Avulsa, "Avulsa", Ler);
                    break;
                case "dest":
                    g.Destinatario = l.Grupo(g.Destinatario, "Destinatario", Ler);
                    break;
                case "retirada":
                    g.Retirada = l.Grupo(g.Retirada, "Retirada", Ler);
                    break;
                case "entrega":
                    g.Entrega = l.Grupo(g.Entrega, "Entrega", Ler);
                    break;
                case "autXML":
                    l.Lista(g.AutorizadosXml, "AutorizadosXml", Ler);
                    break;
                case "det":
                    LerItem(l, g.Itens);
                    break;
                case "total":
                    g.Total = l.Grupo(g.Total, "Total", Ler);
                    break;
                case "transp":
                    g.Transporte = l.Grupo(g.Transporte, "Transporte", Ler);
                    break;
                case "cobr":
                    g.Cobranca = l.Grupo(g.Cobranca, "Cobranca", Ler);
                    break;
                case "pag":
                    g.Pagamento = l.Grupo(g.Pagamento, "Pagamento", Ler);
                    break;
                case "infIntermed":
                    g.Intermediador = l.Grupo(g.Intermediador, "Intermediador", Ler);
                    break;
                case "infAdic":
                    g.InformacoesAdicionais = l.Grupo(g.InformacoesAdicionais, "InformacoesAdicionais", Ler);
                    break;
                case "exporta":
                    g.Exportacao = l.Grupo(g.Exportacao, "Exportacao", Ler);
                    break;
                case "compra":
                    g.Compra = l.Grupo(g.Compra, "Compra", Ler);
                    break;
                case "cana":
                    g.Cana = l.Grupo(g.Cana, "Cana", Ler);
                    break;
                case "infRespTec":
                    g.ResponsavelTecnico = l.Grupo(g.ResponsavelTecnico, "ResponsavelTecnico", Ler);
                    break;
                case "infSolicNFF":
                    g.SolicitacaoNff = l.Grupo(g.SolicitacaoNff, "SolicitacaoNff", Ler);
                    break;
                case "agropecuario":
                    g.Agropecuario = l.Grupo(g.Agropecuario, "Agropecuario", Ler);
                    break;
                case "infPAA":
                    g.ProvedorAssinatura = l.Grupo(g.ProvedorAssinatura, "ProvedorAssinatura", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, InformacoesSuplementares g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qrCode":
                    g.QrCode = l.Texto(g.QrCode, "QrCode");
                    break;
                case "urlChave":
                    g.UrlChave = l.Texto(g.UrlChave, "UrlChave");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Identificacao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cUF":
                    g.UF = l.Enumeracao(g.UF, "UF");
                    break;
                case "cNF":
                    g.CodigoNumerico = l.Inteiro(g.CodigoNumerico, "CodigoNumerico");
                    break;
                case "natOp":
                    g.NaturezaOperacao = l.Texto(g.NaturezaOperacao, "NaturezaOperacao");
                    break;
                case "mod":
                    g.Modelo = l.Enumeracao(g.Modelo, "Modelo");
                    break;
                case "serie":
                    g.Serie = l.Inteiro(g.Serie, "Serie");
                    break;
                case "nNF":
                    g.Numero = l.Inteiro(g.Numero, "Numero");
                    break;
                case "dhEmi":
                    g.Emissao = l.DataHora(g.Emissao, "Emissao");
                    break;
                case "dhSaiEnt":
                    g.SaidaEntrada = l.DataHora(g.SaidaEntrada, "SaidaEntrada");
                    break;
                case "dPrevEntrega":
                    g.PrevisaoEntrega = l.Data(g.PrevisaoEntrega, "PrevisaoEntrega");
                    break;
                case "tpNF":
                    g.Tipo = l.Enumeracao(g.Tipo, "Tipo");
                    break;
                case "idDest":
                    g.LocalDestino = l.Enumeracao(g.LocalDestino, "LocalDestino");
                    break;
                case "cMunFG":
                    g.MunicipioFatoGerador = l.Inteiro(g.MunicipioFatoGerador, "MunicipioFatoGerador");
                    break;
                case "cMunFGIBS":
                    g.MunicipioFatoGeradorIbsCbs = l.Inteiro(g.MunicipioFatoGeradorIbsCbs, "MunicipioFatoGeradorIbsCbs");
                    break;
                case "tpImp":
                    g.FormatoImpressao = l.Enumeracao(g.FormatoImpressao, "FormatoImpressao");
                    break;
                case "tpEmis":
                    g.TipoEmissao = l.Enumeracao(g.TipoEmissao, "TipoEmissao");
                    break;
                case "cDV":
                    l.DigitoVerificador = l.Inteiro(l.DigitoVerificador, null);
                    break;
                case "tpAmb":
                    g.Ambiente = l.Enumeracao(g.Ambiente, "Ambiente");
                    break;
                case "finNFe":
                    g.Finalidade = l.Enumeracao(g.Finalidade, "Finalidade");
                    break;
                case "tpNFDebito":
                    g.TipoDebito = l.Enumeracao(g.TipoDebito, "TipoDebito");
                    break;
                case "tpNFCredito":
                    g.TipoCredito = l.Enumeracao(g.TipoCredito, "TipoCredito");
                    break;
                case "indFinal":
                    g.ConsumidorFinal = l.Enumeracao(g.ConsumidorFinal, "ConsumidorFinal");
                    break;
                case "indPres":
                    g.Presenca = l.Enumeracao(g.Presenca, "Presenca");
                    break;
                case "indIntermed":
                    g.Intermediacao = l.Enumeracao(g.Intermediacao, "Intermediacao");
                    break;
                case "cIndOp":
                    g.CodigoLocalOperacao = l.Texto(g.CodigoLocalOperacao, "CodigoLocalOperacao");
                    break;
                case "procEmi":
                    g.Processo = l.Enumeracao(g.Processo, "Processo");
                    break;
                case "verProc":
                    g.VersaoAplicativo = l.Texto(g.VersaoAplicativo, "VersaoAplicativo");
                    break;
                case "dhCont":
                    g.InicioContingencia = l.DataHora(g.InicioContingencia, "InicioContingencia");
                    break;
                case "xJust":
                    g.JustificativaContingencia = l.Texto(g.JustificativaContingencia, "JustificativaContingencia");
                    break;
                case "NFref":
                    l.ListaEscolha(g.Referencias, "Referencias", LerDocumentoReferenciado);
                    break;
                case "gCompraGov":
                    g.CompraGovernamental = l.Grupo(g.CompraGovernamental, "CompraGovernamental", Ler);
                    break;
                case "gPagAntecipado":
                    g.PagamentoAntecipado = l.Grupo(g.PagamentoAntecipado, "PagamentoAntecipado", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static DocumentoReferenciado? LerDocumentoReferenciado(LeituraNFe l, string nome) => nome switch
    {
        "refCTe" => new CTeReferenciado { Chave = l.Texto("Chave") },
        "refECF" => l.Subgrupo(new CupomFiscalReferenciado(), Ler),
        "refNFe" => new NFeReferenciada { Chave = l.Texto("Chave") },
        "refNFeSig" => new NFeSigilosaReferenciada { Chave = l.Texto("Chave") },
        "refNF" => l.Subgrupo(new NotaModelo1Referenciada(), Ler),
        "refNFP" => l.Subgrupo(new NotaProdutorReferenciada(), Ler),
        _ => null,
    };

    private static void Ler(LeituraNFe l, Emitente g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "CPF":
                    g.Cpf = l.Texto(g.Cpf, "Cpf");
                    break;
                case "xNome":
                    g.Nome = l.Texto(g.Nome, "Nome");
                    break;
                case "xFant":
                    g.NomeFantasia = l.Texto(g.NomeFantasia, "NomeFantasia");
                    break;
                case "enderEmit":
                    g.Endereco = l.Grupo(g.Endereco, "Endereco", Ler);
                    break;
                case "IE":
                    g.InscricaoEstadual = l.Texto(g.InscricaoEstadual, "InscricaoEstadual");
                    break;
                case "IEST":
                    g.InscricaoEstadualSubstituto = l.Texto(g.InscricaoEstadualSubstituto, "InscricaoEstadualSubstituto");
                    break;
                case "IM":
                    g.InscricaoMunicipal = l.Texto(g.InscricaoMunicipal, "InscricaoMunicipal");
                    break;
                case "CNAE":
                    g.Cnae = l.Texto(g.Cnae, "Cnae");
                    break;
                case "CRT":
                    g.RegimeTributario = l.Enumeracao(g.RegimeTributario, "RegimeTributario");
                    break;
                case "ISUFEmit":
                    g.InscricaoSuframa = l.Texto(g.InscricaoSuframa, "InscricaoSuframa");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, FiscoEmitente g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "xOrgao":
                    g.Orgao = l.Texto(g.Orgao, "Orgao");
                    break;
                case "matr":
                    g.MatriculaAgente = l.Texto(g.MatriculaAgente, "MatriculaAgente");
                    break;
                case "xAgente":
                    g.NomeAgente = l.Texto(g.NomeAgente, "NomeAgente");
                    break;
                case "fone":
                    g.Telefone = l.Texto(g.Telefone, "Telefone");
                    break;
                case "UF":
                    g.UF = l.Sigla(g.UF, "UF");
                    break;
                case "nDAR":
                    g.NumeroDar = l.Texto(g.NumeroDar, "NumeroDar");
                    break;
                case "dEmi":
                    g.EmissaoDar = l.Data(g.EmissaoDar, "EmissaoDar");
                    break;
                case "vDAR":
                    g.ValorDar = l.Decimal(g.ValorDar, "ValorDar");
                    break;
                case "repEmi":
                    g.Reparticao = l.Texto(g.Reparticao, "Reparticao");
                    break;
                case "dPag":
                    g.PagamentoDar = l.Data(g.PagamentoDar, "PagamentoDar");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Destinatario g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "CPF":
                    g.Cpf = l.Texto(g.Cpf, "Cpf");
                    break;
                case "idEstrangeiro":
                    g.IdEstrangeiro = l.Texto(g.IdEstrangeiro, "IdEstrangeiro");
                    break;
                case "xNome":
                    g.Nome = l.Texto(g.Nome, "Nome");
                    break;
                case "enderDest":
                    g.Endereco = l.Grupo(g.Endereco, "Endereco", Ler);
                    break;
                case "indIEDest":
                    g.IndicadorIE = l.Enumeracao(g.IndicadorIE, "IndicadorIE");
                    break;
                case "IE":
                    g.InscricaoEstadual = l.Texto(g.InscricaoEstadual, "InscricaoEstadual");
                    break;
                case "ISUF":
                    g.InscricaoSuframa = l.Texto(g.InscricaoSuframa, "InscricaoSuframa");
                    break;
                case "IM":
                    g.InscricaoMunicipal = l.Texto(g.InscricaoMunicipal, "InscricaoMunicipal");
                    break;
                case "email":
                    g.Email = l.Texto(g.Email, "Email");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Local g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "CPF":
                    g.Cpf = l.Texto(g.Cpf, "Cpf");
                    break;
                case "xNome":
                    g.Nome = l.Texto(g.Nome, "Nome");
                    break;
                case "xLgr":
                    g.Logradouro = l.Texto(g.Logradouro, "Logradouro");
                    break;
                case "nro":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "xCpl":
                    g.Complemento = l.Texto(g.Complemento, "Complemento");
                    break;
                case "xBairro":
                    g.Bairro = l.Texto(g.Bairro, "Bairro");
                    break;
                case "cMun":
                    g.CodigoMunicipio = l.Inteiro(g.CodigoMunicipio, "CodigoMunicipio");
                    break;
                case "xMun":
                    g.Municipio = l.Texto(g.Municipio, "Municipio");
                    break;
                case "UF":
                    (g.UF, g.Exterior) = l.SiglaExterior(g.UF, g.Exterior, "UF");
                    break;
                case "CEP":
                    g.Cep = l.Texto(g.Cep, "Cep");
                    break;
                case "cPais":
                    g.CodigoPais = l.Inteiro(g.CodigoPais, "CodigoPais");
                    break;
                case "xPais":
                    g.Pais = l.Texto(g.Pais, "Pais");
                    break;
                case "fone":
                    g.Telefone = l.Texto(g.Telefone, "Telefone");
                    break;
                case "email":
                    g.Email = l.Texto(g.Email, "Email");
                    break;
                case "IE":
                    g.InscricaoEstadual = l.Texto(g.InscricaoEstadual, "InscricaoEstadual");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, PessoaAutorizada g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "CPF":
                    g.Cpf = l.Texto(g.Cpf, "Cpf");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Transporte g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "modFrete":
                    g.Modalidade = l.Enumeracao(g.Modalidade, "Modalidade");
                    break;
                case "transporta":
                    g.Transportador = l.Grupo(g.Transportador, "Transportador", Ler);
                    break;
                case "retTransp":
                    g.Retencao = l.Grupo(g.Retencao, "Retencao", Ler);
                    break;
                case "veicTransp":
                    g.Veiculo = l.Grupo(g.Veiculo, "Veiculo", Ler);
                    break;
                case "reboque":
                    l.Lista(g.Reboques, "Reboques", Ler);
                    break;
                case "vagao":
                    g.Vagao = l.Texto(g.Vagao, "Vagao");
                    break;
                case "balsa":
                    g.Balsa = l.Texto(g.Balsa, "Balsa");
                    break;
                case "vol":
                    l.Lista(g.Volumes, "Volumes", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Cobranca g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "fat":
                    g.Fatura = l.Grupo(g.Fatura, "Fatura", Ler);
                    break;
                case "dup":
                    l.Lista(g.Duplicatas, "Duplicatas", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Pagamento g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "detPag":
                    l.Lista(g.Formas, "Formas", Ler);
                    break;
                case "vTroco":
                    g.Troco = l.Decimal(g.Troco, "Troco");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Intermediador g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "idCadIntTran":
                    g.Identificador = l.Texto(g.Identificador, "Identificador");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, InformacoesAdicionais g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "infAdFisco":
                    g.Fisco = l.Texto(g.Fisco, "Fisco");
                    break;
                case "infCpl":
                    g.Complementares = l.Texto(g.Complementares, "Complementares");
                    break;
                case "obsCont":
                    l.Lista(g.ObservacoesContribuinte, "ObservacoesContribuinte", Ler);
                    break;
                case "obsFisco":
                    l.Lista(g.ObservacoesFisco, "ObservacoesFisco", Ler);
                    break;
                case "procRef":
                    l.Lista(g.Processos, "Processos", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Exportacao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "UFSaidaPais":
                    g.UFSaida = l.Sigla(g.UFSaida, "UFSaida");
                    break;
                case "xLocExporta":
                    g.LocalEmbarque = l.Texto(g.LocalEmbarque, "LocalEmbarque");
                    break;
                case "xLocDespacho":
                    g.LocalDespacho = l.Texto(g.LocalDespacho, "LocalDespacho");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Compra g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "xNEmp":
                    g.NotaEmpenho = l.Texto(g.NotaEmpenho, "NotaEmpenho");
                    break;
                case "xPed":
                    g.Pedido = l.Texto(g.Pedido, "Pedido");
                    break;
                case "xCont":
                    g.Contrato = l.Texto(g.Contrato, "Contrato");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, AquisicaoCana g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "safra":
                    g.Safra = l.Texto(g.Safra, "Safra");
                    break;
                case "ref":
                    g.Referencia = l.Texto(g.Referencia, "Referencia");
                    break;
                case "forDia":
                    l.Lista(g.Fornecimentos, "Fornecimentos", Ler);
                    break;
                case "qTotMes":
                    g.QuantidadeMes = l.Decimal(g.QuantidadeMes, "QuantidadeMes");
                    break;
                case "qTotAnt":
                    g.QuantidadeAnterior = l.Decimal(g.QuantidadeAnterior, "QuantidadeAnterior");
                    break;
                case "qTotGer":
                    g.QuantidadeGeral = l.Decimal(g.QuantidadeGeral, "QuantidadeGeral");
                    break;
                case "deduc":
                    l.Lista(g.Deducoes, "Deducoes", Ler);
                    break;
                case "vFor":
                    g.ValorFornecimentos = l.Decimal(g.ValorFornecimentos, "ValorFornecimentos");
                    break;
                case "vTotDed":
                    g.ValorDeducoes = l.Decimal(g.ValorDeducoes, "ValorDeducoes");
                    break;
                case "vLiqFor":
                    g.ValorLiquido = l.Decimal(g.ValorLiquido, "ValorLiquido");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ResponsavelTecnico g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "xContato":
                    g.Contato = l.Texto(g.Contato, "Contato");
                    break;
                case "email":
                    g.Email = l.Texto(g.Email, "Email");
                    break;
                case "fone":
                    g.Telefone = l.Texto(g.Telefone, "Telefone");
                    break;
                case "idCSRT":
                    g.IdCsrt = l.Inteiro(g.IdCsrt, "IdCsrt");
                    break;
                case "hashCSRT":
                    g.HashCsrt = l.Texto(g.HashCsrt, "HashCsrt");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, SolicitacaoNff g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "xSolic":
                    g.Texto = l.Texto(g.Texto, "Texto");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Agropecuario g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "defensivo":
                    l.Lista(g.Defensivos, "Defensivos", Ler);
                    break;
                case "guiaTransito":
                    g.GuiaTransito = l.Grupo(g.GuiaTransito, "GuiaTransito", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ProvedorAssinatura g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJPAA":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "PAASignature":
                    g.Assinatura = l.Grupo(g.Assinatura, "Assinatura", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CupomFiscalReferenciado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "mod":
                    g.Modelo = l.Literal(g.Modelo, "Modelo", Literais.LerModeloCupomFiscal);
                    break;
                case "nECF":
                    g.NumeroEcf = l.Inteiro(g.NumeroEcf, "NumeroEcf");
                    break;
                case "nCOO":
                    g.Coo = l.Inteiro(g.Coo, "Coo");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, NotaModelo1Referenciada g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cUF":
                    g.UF = l.Enumeracao(g.UF, "UF");
                    break;
                case "AAMM":
                    g.AnoMes = l.Texto(g.AnoMes, "AnoMes");
                    break;
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "mod":
                    g.Modelo = l.Enumeracao(g.Modelo, "Modelo");
                    break;
                case "serie":
                    g.Serie = l.Inteiro(g.Serie, "Serie");
                    break;
                case "nNF":
                    g.Numero = l.Inteiro(g.Numero, "Numero");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, NotaProdutorReferenciada g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "cUF":
                    g.UF = l.Enumeracao(g.UF, "UF");
                    break;
                case "AAMM":
                    g.AnoMes = l.Texto(g.AnoMes, "AnoMes");
                    break;
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "CPF":
                    g.Cpf = l.Texto(g.Cpf, "Cpf");
                    break;
                case "IE":
                    g.InscricaoEstadual = l.Texto(g.InscricaoEstadual, "InscricaoEstadual");
                    break;
                case "mod":
                    g.Modelo = l.Enumeracao(g.Modelo, "Modelo");
                    break;
                case "serie":
                    g.Serie = l.Inteiro(g.Serie, "Serie");
                    break;
                case "nNF":
                    g.Numero = l.Inteiro(g.Numero, "Numero");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, CompraGovernamental g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "tpEnteGov":
                    g.Ente = l.Enumeracao(g.Ente, "Ente");
                    break;
                case "pRedutor":
                    g.PercentualReducao = l.Decimal(g.PercentualReducao, "PercentualReducao");
                    break;
                case "tpOperGov":
                    g.Operacao = l.Enumeracao(g.Operacao, "Operacao");
                    break;
                case "refDFeAnt":
                    l.Textos(g.DocumentosAnteriores, "DocumentosAnteriores");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, PagamentoAntecipado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "refNFe":
                    l.Textos(g.Chaves, "Chaves");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, EnderecoEmitente g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "xLgr":
                    g.Logradouro = l.Texto(g.Logradouro, "Logradouro");
                    break;
                case "nro":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "xCpl":
                    g.Complemento = l.Texto(g.Complemento, "Complemento");
                    break;
                case "xBairro":
                    g.Bairro = l.Texto(g.Bairro, "Bairro");
                    break;
                case "cMun":
                    g.CodigoMunicipio = l.Inteiro(g.CodigoMunicipio, "CodigoMunicipio");
                    break;
                case "xMun":
                    g.Municipio = l.Texto(g.Municipio, "Municipio");
                    break;
                case "UF":
                    g.UF = l.Sigla(g.UF, "UF");
                    break;
                case "CEP":
                    g.Cep = l.Texto(g.Cep, "Cep");
                    break;
                case "cPais":
                    g.CodigoPais = l.Inteiro(g.CodigoPais, "CodigoPais");
                    break;
                case "xPais":
                    g.Pais = l.Texto(g.Pais, "Pais");
                    break;
                case "fone":
                    g.Telefone = l.Texto(g.Telefone, "Telefone");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, EnderecoDestinatario g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "xLgr":
                    g.Logradouro = l.Texto(g.Logradouro, "Logradouro");
                    break;
                case "nro":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "xCpl":
                    g.Complemento = l.Texto(g.Complemento, "Complemento");
                    break;
                case "xBairro":
                    g.Bairro = l.Texto(g.Bairro, "Bairro");
                    break;
                case "cMun":
                    g.CodigoMunicipio = l.Inteiro(g.CodigoMunicipio, "CodigoMunicipio");
                    break;
                case "xMun":
                    g.Municipio = l.Texto(g.Municipio, "Municipio");
                    break;
                case "UF":
                    (g.UF, g.Exterior) = l.SiglaExterior(g.UF, g.Exterior, "UF");
                    break;
                case "CEP":
                    g.Cep = l.Texto(g.Cep, "Cep");
                    break;
                case "cPais":
                    g.CodigoPais = l.Inteiro(g.CodigoPais, "CodigoPais");
                    break;
                case "xPais":
                    g.Pais = l.Texto(g.Pais, "Pais");
                    break;
                case "fone":
                    g.Telefone = l.Texto(g.Telefone, "Telefone");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Transportador g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "CNPJ":
                    g.Cnpj = l.Texto(g.Cnpj, "Cnpj");
                    break;
                case "CPF":
                    g.Cpf = l.Texto(g.Cpf, "Cpf");
                    break;
                case "xNome":
                    g.Nome = l.Texto(g.Nome, "Nome");
                    break;
                case "IE":
                    g.InscricaoEstadual = l.Texto(g.InscricaoEstadual, "InscricaoEstadual");
                    break;
                case "xEnder":
                    g.Endereco = l.Texto(g.Endereco, "Endereco");
                    break;
                case "xMun":
                    g.Municipio = l.Texto(g.Municipio, "Municipio");
                    break;
                case "UF":
                    (g.UF, g.Exterior) = l.SiglaExterior(g.UF, g.Exterior, "UF");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, RetencaoIcmsTransporte g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "vServ":
                    g.ValorServico = l.Decimal(g.ValorServico, "ValorServico");
                    break;
                case "vBCRet":
                    g.BaseCalculo = l.Decimal(g.BaseCalculo, "BaseCalculo");
                    break;
                case "pICMSRet":
                    g.Aliquota = l.Decimal(g.Aliquota, "Aliquota");
                    break;
                case "vICMSRet":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "CFOP":
                    g.Cfop = l.Inteiro(g.Cfop, "Cfop");
                    break;
                case "cMunFG":
                    g.MunicipioFatoGerador = l.Inteiro(g.MunicipioFatoGerador, "MunicipioFatoGerador");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Veiculo g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "placa":
                    g.Placa = l.Texto(g.Placa, "Placa");
                    break;
                case "UF":
                    (g.UF, g.Exterior) = l.SiglaExterior(g.UF, g.Exterior, "UF");
                    break;
                case "RNTC":
                    g.Rntc = l.Texto(g.Rntc, "Rntc");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Volume g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qVol":
                    g.Quantidade = l.Inteiro(g.Quantidade, "Quantidade");
                    break;
                case "esp":
                    g.Especie = l.Texto(g.Especie, "Especie");
                    break;
                case "marca":
                    g.Marca = l.Texto(g.Marca, "Marca");
                    break;
                case "nVol":
                    g.Numeracao = l.Texto(g.Numeracao, "Numeracao");
                    break;
                case "pesoL":
                    g.PesoLiquido = l.Decimal(g.PesoLiquido, "PesoLiquido");
                    break;
                case "pesoB":
                    g.PesoBruto = l.Decimal(g.PesoBruto, "PesoBruto");
                    break;
                case "lacres":
                    l.Lista(g.Lacres, "Lacres", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Fatura g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nFat":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "vOrig":
                    g.ValorOriginal = l.Decimal(g.ValorOriginal, "ValorOriginal");
                    break;
                case "vDesc":
                    g.Desconto = l.Decimal(g.Desconto, "Desconto");
                    break;
                case "vLiq":
                    g.ValorLiquido = l.Decimal(g.ValorLiquido, "ValorLiquido");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Duplicata g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nDup":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "dVenc":
                    g.Vencimento = l.Data(g.Vencimento, "Vencimento");
                    break;
                case "vDup":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, FormaPagamento g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "indPag":
                    g.Indicador = l.Enumeracao(g.Indicador, "Indicador");
                    break;
                case "tPag":
                    g.Meio = l.Enumeracao(g.Meio, "Meio");
                    break;
                case "xPag":
                    g.Descricao = l.Texto(g.Descricao, "Descricao");
                    break;
                case "vPag":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                case "dPag":
                    g.Data = l.Data(g.Data, "Data");
                    break;
                case "CNPJPag":
                    g.CnpjTransacao = l.Texto(g.CnpjTransacao, "CnpjTransacao");
                    break;
                case "UFPag":
                    g.UFTransacao = l.Sigla(g.UFTransacao, "UFTransacao");
                    break;
                case "card":
                    g.Transacao = l.Grupo(g.Transacao, "Transacao", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Observacao g)
    {
        g.Campo = l.TextoAtributo("xCampo", "Campo");
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "xTexto":
                    g.Texto = l.Texto(g.Texto, "Texto");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ProcessoReferenciado g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nProc":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                case "indProc":
                    g.Origem = l.Enumeracao(g.Origem, "Origem");
                    break;
                case "tpAto":
                    g.TipoAto = l.Enumeracao(g.TipoAto, "TipoAto");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, FornecimentoDiario g)
    {
        g.Dia = l.InteiroAtributo("dia", "Dia");
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "qtde":
                    g.Quantidade = l.Decimal(g.Quantidade, "Quantidade");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Deducao g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "xDed":
                    g.Descricao = l.Texto(g.Descricao, "Descricao");
                    break;
                case "vDed":
                    g.Valor = l.Decimal(g.Valor, "Valor");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Defensivo g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nReceituario":
                    g.Receituario = l.Texto(g.Receituario, "Receituario");
                    break;
                case "CPFRespTec":
                    g.CpfResponsavel = l.Texto(g.CpfResponsavel, "CpfResponsavel");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, GuiaTransito g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "tpGuia":
                    g.Tipo = l.Enumeracao(g.Tipo, "Tipo");
                    break;
                case "UFGuia":
                    g.UF = l.Sigla(g.UF, "UF");
                    break;
                case "serieGuia":
                    g.Serie = l.Texto(g.Serie, "Serie");
                    break;
                case "nGuia":
                    g.Numero = l.Inteiro(g.Numero, "Numero");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, AssinaturaProvedor g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "SignatureValue":
                    g.Valor = l.Texto(g.Valor, "Valor");
                    break;
                case "RSAKeyValue":
                    g.ChavePublica = l.Grupo(g.ChavePublica, "ChavePublica", Ler);
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, Lacre g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "nLacre":
                    g.Numero = l.Texto(g.Numero, "Numero");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, TransacaoEletronica g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "tpIntegra":
                    g.Integracao = l.Enumeracao(g.Integracao, "Integracao");
                    break;
                case "CNPJ":
                    g.CnpjInstituicao = l.Texto(g.CnpjInstituicao, "CnpjInstituicao");
                    break;
                case "tBand":
                    g.Bandeira = l.Enumeracao(g.Bandeira, "Bandeira");
                    break;
                case "cAut":
                    g.Autorizacao = l.Texto(g.Autorizacao, "Autorizacao");
                    break;
                case "CNPJReceb":
                    g.CnpjBeneficiario = l.Texto(g.CnpjBeneficiario, "CnpjBeneficiario");
                    break;
                case "idTermPag":
                    g.Terminal = l.Texto(g.Terminal, "Terminal");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }

    private static void Ler(LeituraNFe l, ChavePublicaRsa g)
    {
        while (l.Filho())
        {
            switch (l.Nome)
            {
                case "Modulus":
                    g.Modulo = l.Texto(g.Modulo, "Modulo");
                    break;
                case "Exponent":
                    g.Expoente = l.Texto(g.Expoente, "Expoente");
                    break;
                default:
                    l.Desconhecido();
                    break;
            }
        }
    }
}
