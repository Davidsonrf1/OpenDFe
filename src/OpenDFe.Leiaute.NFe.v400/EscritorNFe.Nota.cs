namespace OpenDFe.NFe;

// A escrita do cabeçalho e das partes (MODELO.md 2): um método por grupo, na ordem do XSD; o pai abre e fecha o grupo.
internal static partial class EscritorNFe
{
    private static void Escrever(EscritaNFe e, NotaFiscal g)
    {
        if (g.Identificacao is { } identificacao)
        {
            e.Abrir("ide", "Identificacao");
            Escrever(e, identificacao);
            e.Fechar();
        }
        if (g.Emitente is { } emitente)
        {
            e.Abrir("emit", "Emitente");
            Escrever(e, emitente);
            e.Fechar();
        }
        if (g.Avulsa is { } avulsa)
        {
            e.Abrir("avulsa", "Avulsa");
            Escrever(e, avulsa);
            e.Fechar();
        }
        if (g.Destinatario is { } destinatario)
        {
            e.Abrir("dest", "Destinatario");
            Escrever(e, destinatario);
            e.Fechar();
        }
        if (g.Retirada is { } retirada)
        {
            e.Abrir("retirada", "Retirada");
            Escrever(e, retirada);
            e.Fechar();
        }
        if (g.Entrega is { } entrega)
        {
            e.Abrir("entrega", "Entrega");
            Escrever(e, entrega);
            e.Fechar();
        }
        for (var i = 0; i < g.AutorizadosXml.Count; i++)
        {
            e.Abrir("autXML", "AutorizadosXml", i, g.AutorizadosXml.Count);
            Escrever(e, g.AutorizadosXml[i]);
            e.Fechar();
        }
        for (var i = 0; i < g.Itens.Count; i++)
        {
            e.Abrir("det", "Itens", i, g.Itens.Count);
            e.InteiroAtributo("nItem", null, i + 1, 0);
            Escrever(e, g.Itens[i]);
            e.Fechar();
        }
        if (g.Total is { } total)
        {
            e.Abrir("total", "Total");
            Escrever(e, total);
            e.Fechar();
        }
        if (g.Transporte is { } transporte)
        {
            e.Abrir("transp", "Transporte");
            Escrever(e, transporte);
            e.Fechar();
        }
        if (g.Cobranca is { } cobranca)
        {
            e.Abrir("cobr", "Cobranca");
            Escrever(e, cobranca);
            e.Fechar();
        }
        if (g.Pagamento is { } pagamento)
        {
            e.Abrir("pag", "Pagamento");
            Escrever(e, pagamento);
            e.Fechar();
        }
        if (g.Intermediador is { } intermediador)
        {
            e.Abrir("infIntermed", "Intermediador");
            Escrever(e, intermediador);
            e.Fechar();
        }
        if (g.InformacoesAdicionais is { } informacoesAdicionais)
        {
            e.Abrir("infAdic", "InformacoesAdicionais");
            Escrever(e, informacoesAdicionais);
            e.Fechar();
        }
        if (g.Exportacao is { } exportacao)
        {
            e.Abrir("exporta", "Exportacao");
            Escrever(e, exportacao);
            e.Fechar();
        }
        if (g.Compra is { } compra)
        {
            e.Abrir("compra", "Compra");
            Escrever(e, compra);
            e.Fechar();
        }
        if (g.Cana is { } cana)
        {
            e.Abrir("cana", "Cana");
            Escrever(e, cana);
            e.Fechar();
        }
        if (g.ResponsavelTecnico is { } responsavelTecnico)
        {
            e.Abrir("infRespTec", "ResponsavelTecnico");
            Escrever(e, responsavelTecnico);
            e.Fechar();
        }
        if (g.SolicitacaoNff is { } solicitacaoNff)
        {
            e.Abrir("infSolicNFF", "SolicitacaoNff");
            Escrever(e, solicitacaoNff);
            e.Fechar();
        }
        if (g.Agropecuario is { } agropecuario)
        {
            e.Abrir("agropecuario", "Agropecuario");
            Escrever(e, agropecuario);
            e.Fechar();
        }
        if (g.ProvedorAssinatura is { } provedorAssinatura)
        {
            e.Abrir("infPAA", "ProvedorAssinatura");
            Escrever(e, provedorAssinatura);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, InformacoesSuplementares g)
    {
        e.Texto("qrCode", "QrCode", g.QrCode);
        e.Texto("urlChave", "UrlChave", g.UrlChave);
    }

    private static void Escrever(EscritaNFe e, Identificacao g)
    {
        // Os campos que o núcleo já sabe (MODELO.md, decisão 6) saem do núcleo; o cNF, o cDV e a chave, da montagem.
        var nucleo = e.Nucleo;
        e.Inteiro("cUF", "UF", (int?)e.Configurado("cUF", "UF", g.UF, nucleo.UF, nucleo.UF.ToString()), 2);
        e.Inteiro("cNF", "CodigoNumerico", e.Chave?.CodigoNumerico ?? g.CodigoNumerico, 8);
        e.Texto("natOp", "NaturezaOperacao", g.NaturezaOperacao);
        e.Inteiro("mod", "Modelo", (int?)e.Configurado("mod", "Modelo", g.Modelo, nucleo.Documento, nucleo.Documento.ToString()), 2);
        e.Inteiro("serie", "Serie", g.Serie, 0);
        e.Inteiro("nNF", "Numero", g.Numero, 0);
        e.DataHora("dhEmi", "Emissao", g.Emissao);
        e.DataHora("dhSaiEnt", "SaidaEntrada", g.SaidaEntrada);
        e.Data("dPrevEntrega", "PrevisaoEntrega", g.PrevisaoEntrega);
        e.Inteiro("tpNF", "Tipo", (int?)g.Tipo, 1);
        e.Inteiro("idDest", "LocalDestino", (int?)g.LocalDestino, 1);
        e.Inteiro("cMunFG", "MunicipioFatoGerador", g.MunicipioFatoGerador, 7);
        e.Inteiro("cMunFGIBS", "MunicipioFatoGeradorIbsCbs", g.MunicipioFatoGeradorIbsCbs, 7);
        e.Inteiro("tpImp", "FormatoImpressao", (int?)g.FormatoImpressao, 1);
        e.Inteiro("tpEmis", "TipoEmissao", (int?)e.Configurado("tpEmis", "TipoEmissao", g.TipoEmissao, nucleo.Emissao, nucleo.Emissao.ToString()), 1);
        e.Inteiro("cDV", null, e.Chave?.DigitoVerificador, 1);
        e.Inteiro("tpAmb", "Ambiente", (int?)e.Configurado("tpAmb", "Ambiente", g.Ambiente, nucleo.Ambiente, nucleo.Ambiente.ToString()), 1);
        e.Inteiro("finNFe", "Finalidade", (int?)g.Finalidade, 1);
        e.Inteiro("tpNFDebito", "TipoDebito", (int?)g.TipoDebito, 2);
        e.Inteiro("tpNFCredito", "TipoCredito", (int?)g.TipoCredito, 2);
        e.Inteiro("indFinal", "ConsumidorFinal", (int?)g.ConsumidorFinal, 1);
        e.Inteiro("indPres", "Presenca", (int?)g.Presenca, 1);
        e.Inteiro("indIntermed", "Intermediacao", (int?)g.Intermediacao, 1);
        e.Texto("cIndOp", "CodigoLocalOperacao", g.CodigoLocalOperacao);
        e.Inteiro("procEmi", "Processo", (int?)g.Processo, 1);
        e.Texto("verProc", "VersaoAplicativo", g.VersaoAplicativo);
        e.DataHora("dhCont", "InicioContingencia", e.Configurado("dhCont", "InicioContingencia", g.InicioContingencia,
            nucleo.Contingencia?.Inicio, nucleo.Contingencia is null ? "emissão normal" : FormatadorInvariavel.DataHora(nucleo.Contingencia.Inicio)));
        e.Texto("xJust", "JustificativaContingencia", e.Configurado("xJust", "JustificativaContingencia", g.JustificativaContingencia,
            nucleo.Contingencia?.Justificativa, nucleo.Contingencia is null ? "emissão normal" : "outra justificativa"));
        for (var i = 0; i < g.Referencias.Count; i++)
        {
            e.Abrir("NFref", "Referencias", i, g.Referencias.Count);
            switch (g.Referencias[i])
            {
                case CTeReferenciado x:
                    e.Texto("refCTe", "Chave", x.Chave);
                    break;
                case CupomFiscalReferenciado x:
                    e.Abrir("refECF", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case NFeReferenciada x:
                    e.Texto("refNFe", "Chave", x.Chave);
                    break;
                case NFeSigilosaReferenciada x:
                    e.Texto("refNFeSig", "Chave", x.Chave);
                    break;
                case NotaModelo1Referenciada x:
                    e.Abrir("refNF", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
                case NotaProdutorReferenciada x:
                    e.Abrir("refNFP", null);
                    Escrever(e, x);
                    e.Fechar();
                    break;
            }
            e.Fechar();
        }
        if (g.CompraGovernamental is { } compraGovernamental)
        {
            e.Abrir("gCompraGov", "CompraGovernamental");
            Escrever(e, compraGovernamental);
            e.Fechar();
        }
        if (g.PagamentoAntecipado is { } pagamentoAntecipado)
        {
            e.Abrir("gPagAntecipado", "PagamentoAntecipado");
            Escrever(e, pagamentoAntecipado);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Emitente g)
    {
        e.Escolha(("Cnpj", !string.IsNullOrEmpty(g.Cnpj)), ("Cpf", !string.IsNullOrEmpty(g.Cpf)));
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("CPF", "Cpf", g.Cpf);
        e.Texto("xNome", "Nome", g.Nome);
        e.Texto("xFant", "NomeFantasia", g.NomeFantasia);
        if (g.Endereco is { } endereco)
        {
            e.Abrir("enderEmit", "Endereco");
            Escrever(e, endereco);
            e.Fechar();
        }
        e.Texto("IE", "InscricaoEstadual", g.InscricaoEstadual);
        e.Texto("IEST", "InscricaoEstadualSubstituto", g.InscricaoEstadualSubstituto);
        e.Texto("IM", "InscricaoMunicipal", g.InscricaoMunicipal);
        e.Texto("CNAE", "Cnae", g.Cnae);
        e.Inteiro("CRT", "RegimeTributario", (int?)g.RegimeTributario, 1);
        e.Texto("ISUFEmit", "InscricaoSuframa", g.InscricaoSuframa);
    }

    private static void Escrever(EscritaNFe e, FiscoEmitente g)
    {
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("xOrgao", "Orgao", g.Orgao);
        e.Texto("matr", "MatriculaAgente", g.MatriculaAgente);
        e.Texto("xAgente", "NomeAgente", g.NomeAgente);
        e.Texto("fone", "Telefone", g.Telefone);
        e.Sigla("UF", "UF", g.UF, false);
        e.Texto("nDAR", "NumeroDar", g.NumeroDar);
        e.Data("dEmi", "EmissaoDar", g.EmissaoDar);
        e.Decimal("vDAR", "ValorDar", g.ValorDar, 2);
        e.Texto("repEmi", "Reparticao", g.Reparticao);
        e.Data("dPag", "PagamentoDar", g.PagamentoDar);
    }

    private static void Escrever(EscritaNFe e, Destinatario g)
    {
        e.Escolha(("Cnpj", !string.IsNullOrEmpty(g.Cnpj)), ("Cpf", !string.IsNullOrEmpty(g.Cpf)), ("IdEstrangeiro", g.IdEstrangeiro is not null));
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("CPF", "Cpf", g.Cpf);
        e.TextoVazio("idEstrangeiro", "IdEstrangeiro", g.IdEstrangeiro);
        e.Texto("xNome", "Nome", g.Nome);
        if (g.Endereco is { } endereco)
        {
            e.Abrir("enderDest", "Endereco");
            Escrever(e, endereco);
            e.Fechar();
        }
        e.Inteiro("indIEDest", "IndicadorIE", (int?)g.IndicadorIE, 1);
        e.Texto("IE", "InscricaoEstadual", g.InscricaoEstadual);
        e.Texto("ISUF", "InscricaoSuframa", g.InscricaoSuframa);
        e.Texto("IM", "InscricaoMunicipal", g.InscricaoMunicipal);
        e.Texto("email", "Email", g.Email);
    }

    private static void Escrever(EscritaNFe e, Local g)
    {
        e.Escolha(("Cnpj", !string.IsNullOrEmpty(g.Cnpj)), ("Cpf", !string.IsNullOrEmpty(g.Cpf)));
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("CPF", "Cpf", g.Cpf);
        e.Texto("xNome", "Nome", g.Nome);
        e.Texto("xLgr", "Logradouro", g.Logradouro);
        e.Texto("nro", "Numero", g.Numero);
        e.Texto("xCpl", "Complemento", g.Complemento);
        e.Texto("xBairro", "Bairro", g.Bairro);
        e.Inteiro("cMun", "CodigoMunicipio", g.CodigoMunicipio, 7);
        e.Texto("xMun", "Municipio", g.Municipio);
        e.Sigla("UF", "UF", g.UF, g.Exterior);
        e.Texto("CEP", "Cep", g.Cep);
        e.Inteiro("cPais", "CodigoPais", g.CodigoPais, 0);
        e.Texto("xPais", "Pais", g.Pais);
        e.Texto("fone", "Telefone", g.Telefone);
        e.Texto("email", "Email", g.Email);
        e.Texto("IE", "InscricaoEstadual", g.InscricaoEstadual);
    }

    private static void Escrever(EscritaNFe e, PessoaAutorizada g)
    {
        e.Escolha(("Cnpj", !string.IsNullOrEmpty(g.Cnpj)), ("Cpf", !string.IsNullOrEmpty(g.Cpf)));
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("CPF", "Cpf", g.Cpf);
    }

    private static void Escrever(EscritaNFe e, Transporte g)
    {
        e.Escolha(("Veiculo, Reboques", g.Veiculo is not null || g.Reboques.Count > 0), ("Vagao", !string.IsNullOrEmpty(g.Vagao)), ("Balsa", !string.IsNullOrEmpty(g.Balsa)));
        e.Inteiro("modFrete", "Modalidade", (int?)g.Modalidade, 1);
        if (g.Transportador is { } transportador)
        {
            e.Abrir("transporta", "Transportador");
            Escrever(e, transportador);
            e.Fechar();
        }
        if (g.Retencao is { } retencao)
        {
            e.Abrir("retTransp", "Retencao");
            Escrever(e, retencao);
            e.Fechar();
        }
        if (g.Veiculo is { } veiculo)
        {
            e.Abrir("veicTransp", "Veiculo");
            Escrever(e, veiculo);
            e.Fechar();
        }
        for (var i = 0; i < g.Reboques.Count; i++)
        {
            e.Abrir("reboque", "Reboques", i, g.Reboques.Count);
            Escrever(e, g.Reboques[i]);
            e.Fechar();
        }
        e.Texto("vagao", "Vagao", g.Vagao);
        e.Texto("balsa", "Balsa", g.Balsa);
        for (var i = 0; i < g.Volumes.Count; i++)
        {
            e.Abrir("vol", "Volumes", i, g.Volumes.Count);
            Escrever(e, g.Volumes[i]);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Cobranca g)
    {
        if (g.Fatura is { } fatura)
        {
            e.Abrir("fat", "Fatura");
            Escrever(e, fatura);
            e.Fechar();
        }
        for (var i = 0; i < g.Duplicatas.Count; i++)
        {
            e.Abrir("dup", "Duplicatas", i, g.Duplicatas.Count);
            Escrever(e, g.Duplicatas[i]);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Pagamento g)
    {
        for (var i = 0; i < g.Formas.Count; i++)
        {
            e.Abrir("detPag", "Formas", i, g.Formas.Count);
            Escrever(e, g.Formas[i]);
            e.Fechar();
        }
        e.Decimal("vTroco", "Troco", g.Troco, 2);
    }

    private static void Escrever(EscritaNFe e, Intermediador g)
    {
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("idCadIntTran", "Identificador", g.Identificador);
    }

    private static void Escrever(EscritaNFe e, InformacoesAdicionais g)
    {
        e.Texto("infAdFisco", "Fisco", g.Fisco);
        e.Texto("infCpl", "Complementares", g.Complementares);
        for (var i = 0; i < g.ObservacoesContribuinte.Count; i++)
        {
            e.Abrir("obsCont", "ObservacoesContribuinte", i, g.ObservacoesContribuinte.Count);
            Escrever(e, g.ObservacoesContribuinte[i]);
            e.Fechar();
        }
        for (var i = 0; i < g.ObservacoesFisco.Count; i++)
        {
            e.Abrir("obsFisco", "ObservacoesFisco", i, g.ObservacoesFisco.Count);
            Escrever(e, g.ObservacoesFisco[i]);
            e.Fechar();
        }
        for (var i = 0; i < g.Processos.Count; i++)
        {
            e.Abrir("procRef", "Processos", i, g.Processos.Count);
            Escrever(e, g.Processos[i]);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Exportacao g)
    {
        e.Sigla("UFSaidaPais", "UFSaida", g.UFSaida, false);
        e.Texto("xLocExporta", "LocalEmbarque", g.LocalEmbarque);
        e.Texto("xLocDespacho", "LocalDespacho", g.LocalDespacho);
    }

    private static void Escrever(EscritaNFe e, Compra g)
    {
        e.Texto("xNEmp", "NotaEmpenho", g.NotaEmpenho);
        e.Texto("xPed", "Pedido", g.Pedido);
        e.Texto("xCont", "Contrato", g.Contrato);
    }

    private static void Escrever(EscritaNFe e, AquisicaoCana g)
    {
        e.Texto("safra", "Safra", g.Safra);
        e.Texto("ref", "Referencia", g.Referencia);
        for (var i = 0; i < g.Fornecimentos.Count; i++)
        {
            e.Abrir("forDia", "Fornecimentos", i, g.Fornecimentos.Count);
            Escrever(e, g.Fornecimentos[i]);
            e.Fechar();
        }
        e.Decimal("qTotMes", "QuantidadeMes", g.QuantidadeMes, 2);
        e.Decimal("qTotAnt", "QuantidadeAnterior", g.QuantidadeAnterior, 2);
        e.Decimal("qTotGer", "QuantidadeGeral", g.QuantidadeGeral, 2);
        for (var i = 0; i < g.Deducoes.Count; i++)
        {
            e.Abrir("deduc", "Deducoes", i, g.Deducoes.Count);
            Escrever(e, g.Deducoes[i]);
            e.Fechar();
        }
        e.Decimal("vFor", "ValorFornecimentos", g.ValorFornecimentos, 2);
        e.Decimal("vTotDed", "ValorDeducoes", g.ValorDeducoes, 2);
        e.Decimal("vLiqFor", "ValorLiquido", g.ValorLiquido, 2);
    }

    private static void Escrever(EscritaNFe e, ResponsavelTecnico g)
    {
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("xContato", "Contato", g.Contato);
        e.Texto("email", "Email", g.Email);
        e.Texto("fone", "Telefone", g.Telefone);
        e.Inteiro("idCSRT", "IdCsrt", g.IdCsrt, 2);
        e.Texto("hashCSRT", "HashCsrt", g.HashCsrt);
    }

    private static void Escrever(EscritaNFe e, SolicitacaoNff g)
    {
        e.Texto("xSolic", "Texto", g.Texto);
    }

    private static void Escrever(EscritaNFe e, Agropecuario g)
    {
        e.Escolha(("Defensivos", g.Defensivos.Count > 0), ("GuiaTransito", g.GuiaTransito is not null));
        for (var i = 0; i < g.Defensivos.Count; i++)
        {
            e.Abrir("defensivo", "Defensivos", i, g.Defensivos.Count);
            Escrever(e, g.Defensivos[i]);
            e.Fechar();
        }
        if (g.GuiaTransito is { } guiaTransito)
        {
            e.Abrir("guiaTransito", "GuiaTransito");
            Escrever(e, guiaTransito);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, ProvedorAssinatura g)
    {
        e.Texto("CNPJPAA", "Cnpj", g.Cnpj);
        if (g.Assinatura is { } assinatura)
        {
            e.Abrir("PAASignature", "Assinatura");
            Escrever(e, assinatura);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, CupomFiscalReferenciado g)
    {
        e.Literal("mod", "Modelo", Literais.ModeloCupomFiscal(g.Modelo));
        e.Inteiro("nECF", "NumeroEcf", g.NumeroEcf, 0);
        e.Inteiro("nCOO", "Coo", g.Coo, 0);
    }

    private static void Escrever(EscritaNFe e, NotaModelo1Referenciada g)
    {
        e.Inteiro("cUF", "UF", (int?)g.UF, 2);
        e.Texto("AAMM", "AnoMes", g.AnoMes);
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Inteiro("mod", "Modelo", (int?)g.Modelo, 2);
        e.Inteiro("serie", "Serie", g.Serie, 0);
        e.Inteiro("nNF", "Numero", g.Numero, 0);
    }

    private static void Escrever(EscritaNFe e, NotaProdutorReferenciada g)
    {
        e.Escolha(("Cnpj", !string.IsNullOrEmpty(g.Cnpj)), ("Cpf", !string.IsNullOrEmpty(g.Cpf)));
        e.Inteiro("cUF", "UF", (int?)g.UF, 2);
        e.Texto("AAMM", "AnoMes", g.AnoMes);
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("CPF", "Cpf", g.Cpf);
        e.Texto("IE", "InscricaoEstadual", g.InscricaoEstadual);
        e.Inteiro("mod", "Modelo", (int?)g.Modelo, 2);
        e.Inteiro("serie", "Serie", g.Serie, 0);
        e.Inteiro("nNF", "Numero", g.Numero, 0);
    }

    private static void Escrever(EscritaNFe e, CompraGovernamental g)
    {
        e.Inteiro("tpEnteGov", "Ente", (int?)g.Ente, 1);
        e.Decimal("pRedutor", "PercentualReducao", g.PercentualReducao, 2);
        e.Inteiro("tpOperGov", "Operacao", (int?)g.Operacao, 1);
        for (var i = 0; i < g.DocumentosAnteriores.Count; i++)
            e.Texto("refDFeAnt", "DocumentosAnteriores", g.DocumentosAnteriores[i], i, g.DocumentosAnteriores.Count);
    }

    private static void Escrever(EscritaNFe e, PagamentoAntecipado g)
    {
        for (var i = 0; i < g.Chaves.Count; i++)
            e.Texto("refNFe", "Chaves", g.Chaves[i], i, g.Chaves.Count);
    }

    private static void Escrever(EscritaNFe e, EnderecoEmitente g)
    {
        e.Texto("xLgr", "Logradouro", g.Logradouro);
        e.Texto("nro", "Numero", g.Numero);
        e.Texto("xCpl", "Complemento", g.Complemento);
        e.Texto("xBairro", "Bairro", g.Bairro);
        e.Inteiro("cMun", "CodigoMunicipio", g.CodigoMunicipio, 7);
        e.Texto("xMun", "Municipio", g.Municipio);
        e.Sigla("UF", "UF", g.UF, false);
        e.Texto("CEP", "Cep", g.Cep);
        e.Inteiro("cPais", "CodigoPais", g.CodigoPais, 0);
        e.Texto("xPais", "Pais", g.Pais);
        e.Texto("fone", "Telefone", g.Telefone);
    }

    private static void Escrever(EscritaNFe e, EnderecoDestinatario g)
    {
        e.Texto("xLgr", "Logradouro", g.Logradouro);
        e.Texto("nro", "Numero", g.Numero);
        e.Texto("xCpl", "Complemento", g.Complemento);
        e.Texto("xBairro", "Bairro", g.Bairro);
        e.Inteiro("cMun", "CodigoMunicipio", g.CodigoMunicipio, 7);
        e.Texto("xMun", "Municipio", g.Municipio);
        e.Sigla("UF", "UF", g.UF, g.Exterior);
        e.Texto("CEP", "Cep", g.Cep);
        e.Inteiro("cPais", "CodigoPais", g.CodigoPais, 0);
        e.Texto("xPais", "Pais", g.Pais);
        e.Texto("fone", "Telefone", g.Telefone);
    }

    private static void Escrever(EscritaNFe e, Transportador g)
    {
        e.Escolha(("Cnpj", !string.IsNullOrEmpty(g.Cnpj)), ("Cpf", !string.IsNullOrEmpty(g.Cpf)));
        e.Texto("CNPJ", "Cnpj", g.Cnpj);
        e.Texto("CPF", "Cpf", g.Cpf);
        e.Texto("xNome", "Nome", g.Nome);
        e.Texto("IE", "InscricaoEstadual", g.InscricaoEstadual);
        e.Texto("xEnder", "Endereco", g.Endereco);
        e.Texto("xMun", "Municipio", g.Municipio);
        e.Sigla("UF", "UF", g.UF, g.Exterior);
    }

    private static void Escrever(EscritaNFe e, RetencaoIcmsTransporte g)
    {
        e.Decimal("vServ", "ValorServico", g.ValorServico, 2);
        e.Decimal("vBCRet", "BaseCalculo", g.BaseCalculo, 2);
        e.Decimal("pICMSRet", "Aliquota", g.Aliquota, 2);
        e.Decimal("vICMSRet", "Valor", g.Valor, 2);
        e.Inteiro("CFOP", "Cfop", g.Cfop, 0);
        e.Inteiro("cMunFG", "MunicipioFatoGerador", g.MunicipioFatoGerador, 7);
    }

    private static void Escrever(EscritaNFe e, Veiculo g)
    {
        e.Texto("placa", "Placa", g.Placa);
        e.Sigla("UF", "UF", g.UF, g.Exterior);
        e.Texto("RNTC", "Rntc", g.Rntc);
    }

    private static void Escrever(EscritaNFe e, Volume g)
    {
        e.Inteiro("qVol", "Quantidade", g.Quantidade, 0);
        e.Texto("esp", "Especie", g.Especie);
        e.Texto("marca", "Marca", g.Marca);
        e.Texto("nVol", "Numeracao", g.Numeracao);
        e.Decimal("pesoL", "PesoLiquido", g.PesoLiquido, 3);
        e.Decimal("pesoB", "PesoBruto", g.PesoBruto, 3);
        for (var i = 0; i < g.Lacres.Count; i++)
        {
            e.Abrir("lacres", "Lacres", i, g.Lacres.Count);
            Escrever(e, g.Lacres[i]);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Fatura g)
    {
        e.Texto("nFat", "Numero", g.Numero);
        e.Decimal("vOrig", "ValorOriginal", g.ValorOriginal, 2);
        e.Decimal("vDesc", "Desconto", g.Desconto, 2);
        e.Decimal("vLiq", "ValorLiquido", g.ValorLiquido, 2);
    }

    private static void Escrever(EscritaNFe e, Duplicata g)
    {
        e.Texto("nDup", "Numero", g.Numero);
        e.Data("dVenc", "Vencimento", g.Vencimento);
        e.Decimal("vDup", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, FormaPagamento g)
    {
        e.Inteiro("indPag", "Indicador", (int?)g.Indicador, 1);
        e.Inteiro("tPag", "Meio", (int?)g.Meio, 2);
        e.Texto("xPag", "Descricao", g.Descricao);
        e.Decimal("vPag", "Valor", g.Valor, 2);
        e.Data("dPag", "Data", g.Data);
        e.Texto("CNPJPag", "CnpjTransacao", g.CnpjTransacao);
        e.Sigla("UFPag", "UFTransacao", g.UFTransacao, false);
        if (g.Transacao is { } transacao)
        {
            e.Abrir("card", "Transacao");
            Escrever(e, transacao);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Observacao g)
    {
        e.TextoAtributo("xCampo", "Campo", g.Campo);
        e.Texto("xTexto", "Texto", g.Texto);
    }

    private static void Escrever(EscritaNFe e, ProcessoReferenciado g)
    {
        e.Texto("nProc", "Numero", g.Numero);
        e.Inteiro("indProc", "Origem", (int?)g.Origem, 1);
        e.Inteiro("tpAto", "TipoAto", (int?)g.TipoAto, 2);
    }

    private static void Escrever(EscritaNFe e, FornecimentoDiario g)
    {
        e.InteiroAtributo("dia", "Dia", g.Dia, 0);
        e.Decimal("qtde", "Quantidade", g.Quantidade, 2);
    }

    private static void Escrever(EscritaNFe e, Deducao g)
    {
        e.Texto("xDed", "Descricao", g.Descricao);
        e.Decimal("vDed", "Valor", g.Valor, 2);
    }

    private static void Escrever(EscritaNFe e, Defensivo g)
    {
        e.Texto("nReceituario", "Receituario", g.Receituario);
        e.Texto("CPFRespTec", "CpfResponsavel", g.CpfResponsavel);
    }

    private static void Escrever(EscritaNFe e, GuiaTransito g)
    {
        e.Inteiro("tpGuia", "Tipo", (int?)g.Tipo, 1);
        e.Sigla("UFGuia", "UF", g.UF, false);
        e.Texto("serieGuia", "Serie", g.Serie);
        e.Inteiro("nGuia", "Numero", g.Numero, 0);
    }

    private static void Escrever(EscritaNFe e, AssinaturaProvedor g)
    {
        e.Texto("SignatureValue", "Valor", g.Valor);
        if (g.ChavePublica is { } chavePublica)
        {
            e.Abrir("RSAKeyValue", "ChavePublica");
            Escrever(e, chavePublica);
            e.Fechar();
        }
    }

    private static void Escrever(EscritaNFe e, Lacre g)
    {
        e.Texto("nLacre", "Numero", g.Numero);
    }

    private static void Escrever(EscritaNFe e, TransacaoEletronica g)
    {
        e.Inteiro("tpIntegra", "Integracao", (int?)g.Integracao, 1);
        e.Texto("CNPJ", "CnpjInstituicao", g.CnpjInstituicao);
        e.Inteiro("tBand", "Bandeira", (int?)g.Bandeira, 2);
        e.Texto("cAut", "Autorizacao", g.Autorizacao);
        e.Texto("CNPJReceb", "CnpjBeneficiario", g.CnpjBeneficiario);
        e.Texto("idTermPag", "Terminal", g.Terminal);
    }

    private static void Escrever(EscritaNFe e, ChavePublicaRsa g)
    {
        e.Texto("Modulus", "Modulo", g.Modulo);
        e.Texto("Exponent", "Expoente", g.Expoente);
    }
}
