using System.Security.Cryptography.X509Certificates;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// Notas de teste para a escrita (2.4): uma NFC-e mínima e uma NF-e com os grupos do cabeçalho e das partes, válidas
// no XSD do pacote embutido depois de assinadas; e o caminho do teste (escrever, assinar, validar).
internal static class NotasTeste
{
    internal const string CnpjEmitente = "11222333000181";

    internal static readonly DateTimeOffset Emissao = new(2026, 10, 20, 14, 30, 0, TimeSpan.FromHours(-3));

    private static readonly Lazy<X509Certificate2> Certificado = new(() => new CertificadoTeste { Cnpj = CnpjEmitente }.Gerar());

    internal static ValoresNucleo Nucleo(Documento documento = Documento.NFCe, TipoEmissao emissao = TipoEmissao.Normal,
        Contingencia? contingencia = null) => new()
    {
        Documento = documento,
        UF = UF.MG,
        Ambiente = Ambiente.Homologacao,
        Emissao = emissao,
        Contingencia = contingencia,
    };

    // A escrita com o cNF fixo, para o XML não depender do sorteio.
    internal static NotaEscrita Escrever(NotaFiscal nota, ValoresNucleo? nucleo = null)
    {
        nota.Identificacao.CodigoNumerico ??= 12345678;
        return EscritorNFe.Escrever(nota, nucleo ?? Nucleo(nota.Identificacao.FormatoImpressao == FormatoImpressao.DanfeNFCe ? Documento.NFCe : Documento.NFe));
    }

    // Assina o infNFe pelo Id e valida o NFe pelo schema da nota; devolve os problemas do schema.
    internal static IReadOnlyList<Problema> Validar(NotaEscrita escrita) => SchemasNFe400.Nota.Validar(Assinar(escrita));

    // O NFe assinado pelo certificado de teste.
    internal static string Assinar(NotaEscrita escrita)
    {
        Assert.Empty(escrita.Problemas);
        Assert.NotNull(escrita.Id);
        return AssinaturaDigital.Assinar(escrita.Xml, escrita.Id!, Certificado.Value);
    }

    internal static void AssertValida(NotaFiscal nota, ValoresNucleo? nucleo = null)
    {
        var escrita = Escrever(nota, nucleo);
        var problemas = Validar(escrita);
        Assert.True(problemas.Count == 0, string.Join(Environment.NewLine, problemas.Select(p => $"{p} [{p.Caminho}]")) + Environment.NewLine + escrita.Xml);
    }

    internal static NotaFiscal NFCe()
    {
        var nota = new NotaFiscal
        {
            Identificacao =
            {
                NaturezaOperacao = "VENDA", Serie = 1, Numero = 123, Emissao = Emissao, Tipo = TipoNota.Saida,
                LocalDestino = LocalDestino.Interna, MunicipioFatoGerador = 3106200, FormatoImpressao = FormatoImpressao.DanfeNFCe,
                Finalidade = FinalidadeEmissao.Normal, ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal,
                Presenca = PresencaComprador.Presencial, Processo = ProcessoEmissao.AplicativoContribuinte, VersaoAplicativo = "PDV 1.0",
            },
            Emitente = Emitente(RegimeTributario.SimplesNacional),
            Transporte = { Modalidade = ModalidadeFrete.NenhumTransporte },
            Pagamento =
            {
                Formas =
                {
                    new FormaPagamento
                    {
                        Meio = MeioPagamento.PixDinamico, Valor = 50.00m,
                        Transacao = new TransacaoEletronica { Integracao = TipoIntegracao.NaoIntegrado },
                    },
                },
            },
        };
        nota.Itens.Add(ItemSimples());
        nota.Total.Icms.BaseCalculo = 0m;
        nota.Total.Icms.Valor = 0m;
        nota.Total.Icms.ValorDesonerado = 0m;
        nota.Total.Icms.ValorFcp = 0m;
        nota.Total.Icms.BaseCalculoSt = 0m;
        nota.Total.Icms.ValorSt = 0m;
        nota.Total.Icms.ValorFcpSt = 0m;
        nota.Total.Icms.ValorFcpStRetido = 0m;
        nota.Total.Icms.ValorProdutos = 50.00m;
        nota.Total.Icms.Frete = 0m;
        nota.Total.Icms.Seguro = 0m;
        nota.Total.Icms.Desconto = 0m;
        nota.Total.Icms.ValorImpostoImportacao = 0m;
        nota.Total.Icms.ValorIpi = 0m;
        nota.Total.Icms.ValorIpiDevolvido = 0m;
        nota.Total.Icms.ValorPis = 0m;
        nota.Total.Icms.ValorCofins = 0m;
        nota.Total.Icms.OutrasDespesas = 0m;
        nota.Total.Icms.ValorNota = 50.00m;
        return nota;
    }

    // Uma NF-e com os grupos do cabeçalho e das partes, um item de mercadoria (ICMS, IPI, II, IBS e CBS, IS) e um de
    // serviço (ISSQN), e os totais.
    internal static NotaFiscal NFe()
    {
        var nota = new NotaFiscal
        {
            Identificacao =
            {
                NaturezaOperacao = "VENDA DE MERCADORIA", Serie = 1, Numero = 4567, Emissao = Emissao,
                SaidaEntrada = Emissao.AddHours(1), Tipo = TipoNota.Saida, LocalDestino = LocalDestino.Interestadual,
                MunicipioFatoGerador = 3106200, MunicipioFatoGeradorIbsCbs = 3106200, FormatoImpressao = FormatoImpressao.DanfeRetrato,
                Finalidade = FinalidadeEmissao.Normal, ConsumidorFinal = IndicadorConsumidorFinal.Normal,
                Presenca = PresencaComprador.NaoPresencialInternet, Intermediacao = IndicadorIntermediador.PlataformaTerceiros,
                Processo = ProcessoEmissao.AplicativoContribuinte, VersaoAplicativo = "ERP 1.0",
                Referencias =
                {
                    new NFeReferenciada { Chave = "31261011222333000181550010000001231123456780" },
                    new NotaModelo1Referenciada { UF = UF.MG, AnoMes = "2610", Cnpj = CnpjEmitente, Modelo = ModeloNotaReferenciada.Modelo1, Serie = 1, Numero = 10 },
                    new NotaProdutorReferenciada { UF = UF.MG, AnoMes = "2610", Cpf = "52998224725", InscricaoEstadual = "ISENTO", Modelo = ModeloNotaProdutor.NotaProdutor, Serie = 0, Numero = 11 },
                    new CTeReferenciado { Chave = "31261011222333000181570010000001231123456780" },
                    new CupomFiscalReferenciado { Modelo = ModeloCupomFiscal.Ecf, NumeroEcf = 1, Coo = 123 },
                },
            },
            Emitente = Emitente(RegimeTributario.Normal),
            Destinatario = new Destinatario
            {
                Cnpj = "11444777000161", Nome = "CLIENTE EXEMPLO SA", IndicadorIE = IndicadorIEDestinatario.Contribuinte,
                InscricaoEstadual = "110042490114", Email = "compras@cliente.com.br",
                Endereco = new EnderecoDestinatario
                {
                    Logradouro = "AV PAULISTA", Numero = "1000", Complemento = "SALA 1", Bairro = "BELA VISTA", CodigoMunicipio = 3550308,
                    Municipio = "SAO PAULO", UF = UF.SP, Cep = "01310100", CodigoPais = 1058, Pais = "BRASIL", Telefone = "1133334444",
                },
            },
            Retirada = new Local
            {
                Cnpj = CnpjEmitente, Nome = "DEPOSITO", Logradouro = "RUA B", Numero = "2", Bairro = "CENTRO", CodigoMunicipio = 3106200,
                Municipio = "BELO HORIZONTE", UF = UF.MG,
            },
            Entrega = new Local
            {
                Cpf = "52998224725", Nome = "RECEBEDOR", Logradouro = "RUA C", Numero = "3", Bairro = "CENTRO", CodigoMunicipio = 3550308,
                Municipio = "SAO PAULO", UF = UF.SP,
            },
            AutorizadosXml = { new PessoaAutorizada { Cpf = "52998224725" } },
            Transporte =
            {
                Modalidade = ModalidadeFrete.Remetente,
                Transportador = new Transportador { Cnpj = "11444777000161", Nome = "TRANSPORTES SA", InscricaoEstadual = "110042490114", Endereco = "RUA D, 4", Municipio = "SAO PAULO", UF = UF.SP },
                Retencao = new RetencaoIcmsTransporte { ValorServico = 100m, BaseCalculo = 100m, Aliquota = 12m, Valor = 12m, Cfop = 5353, MunicipioFatoGerador = 3106200 },
                Veiculo = new Veiculo { Placa = "ABC1D23", UF = UF.MG, Rntc = "12345678" },
                Reboques = { new Veiculo { Placa = "XYZ9A87", UF = UF.MG } },
                Volumes = { new Volume { Quantidade = 2, Especie = "CAIXA", Marca = "EXEMPLO", Numeracao = "1", PesoLiquido = 10.5m, PesoBruto = 11.25m, Lacres = { new Lacre { Numero = "L1" } } } },
            },
            Cobranca = new Cobranca
            {
                Fatura = new Fatura { Numero = "4567", ValorOriginal = 1222.00m, Desconto = 0m, ValorLiquido = 1222.00m },
                Duplicatas = { new Duplicata { Numero = "001", Vencimento = new DateOnly(2026, 11, 20), Valor = 1222.00m } },
            },
            Pagamento =
            {
                Formas =
                {
                    new FormaPagamento
                    {
                        Indicador = IndicadorPagamento.Prazo, Meio = MeioPagamento.CartaoCredito, Valor = 1222.00m,
                        Transacao = new TransacaoEletronica { Integracao = TipoIntegracao.Integrado, CnpjInstituicao = "11444777000161", Bandeira = BandeiraCartao.Visa, Autorizacao = "A1B2C3" },
                    },
                },
            },
            Intermediador = new Intermediador { Cnpj = "11444777000161", Identificador = "LOJA-EXEMPLO" },
            InformacoesAdicionais = new InformacoesAdicionais
            {
                Fisco = "INFORMACAO AO FISCO",
                Complementares = "PEDIDO 99 | ENTREGA EM HORARIO COMERCIAL & PORTARIA <2>",
                ObservacoesContribuinte = { new Observacao { Campo = "pedido", Texto = "99" } },
                ObservacoesFisco = { new Observacao { Campo = "regime", Texto = "especial" } },
                Processos = { new ProcessoReferenciado { Numero = "123", Origem = OrigemProcesso.JusticaFederal } },
            },
            Compra = new Compra { NotaEmpenho = "EMP1", Pedido = "PED1", Contrato = "CTR1" },
            ResponsavelTecnico = new ResponsavelTecnico { Cnpj = CnpjEmitente, Contato = "SUPORTE", Email = "suporte@exemplo.com.br", Telefone = "3133334444" },
        };
        nota.Itens.Add(ItemMercadoria());
        nota.Itens.Add(ItemServico());

        var icms = nota.Total.Icms;
        icms.BaseCalculo = 1000m; icms.Valor = 180m; icms.ValorDesonerado = 0m; icms.ValorFcp = 0m; icms.BaseCalculoSt = 0m;
        icms.ValorSt = 0m; icms.ValorFcpSt = 0m; icms.ValorFcpStRetido = 0m; icms.ValorProdutos = 1000m; icms.Frete = 0m;
        icms.Seguro = 0m; icms.Desconto = 0m; icms.ValorImpostoImportacao = 0m; icms.ValorIpi = 100m; icms.ValorIpiDevolvido = 0m;
        icms.ValorPis = 16.5m; icms.ValorCofins = 76m; icms.OutrasDespesas = 0m; icms.ValorNota = 1222.00m; icms.ValorTotalTributos = 372.5m;
        nota.Total.Issqn = new TotalIssqn
        {
            ValorServicos = 122m, BaseCalculo = 122m, Valor = 6.1m, Competencia = new DateOnly(2026, 10, 20),
            RegimeEspecial = RegimeEspecialTributacao.MicroempresaMunicipal,
        };
        nota.Total.ImpostoSeletivo = new TotalImpostoSeletivo { Valor = 0m };
        nota.Total.IbsCbs = new TotalIbsCbs
        {
            BaseCalculo = 1000m,
            Ibs = new TotalIbs { UF = { Diferimento = 0m, Devolucao = 0m, Valor = 1m }, Municipio = { Diferimento = 0m, Devolucao = 0m, Valor = 0m }, Valor = 1m, CreditoPresumido = 0m, CreditoPresumidoCondicaoSuspensiva = 0m },
            Cbs = new TotalCbs { Diferimento = 0m, Devolucao = 0m, Valor = 9m, CreditoPresumido = 0m, CreditoPresumidoCondicaoSuspensiva = 0m },
        };
        nota.Total.ValorTotalNota = 1232.00m;
        return nota;
    }

    internal static Item ItemMercadoria() => new()
    {
        Produto =
        {
            Codigo = "P1", Gtin = "SEM GTIN", Descricao = "MAQUINA INDUSTRIAL", Ncm = "84798999", Nve = { "AA0001" }, Cest = "0100100",
            Escala = IndicadorEscala.Relevante, Cfop = 6102, UnidadeComercial = "UN", QuantidadeComercial = 1m,
            ValorUnitarioComercial = 1000m, Valor = 1000m, GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN",
            QuantidadeTributavel = 1m, ValorUnitarioTributavel = 1000m, Totalizacao = IndicadorTotal.Compoe, Pedido = "PED1", ItemPedido = "1",
            Rastros = { new Rastro { Lote = "L2026", Quantidade = 1m, Fabricacao = new DateOnly(2026, 1, 10), Validade = new DateOnly(2030, 1, 10) } },
        },
        Imposto =
        {
            ValorTotalTributos = 372.5m,
            Icms = new Icms00 { Origem = OrigemMercadoria.Nacional, ModalidadeBaseCalculo = ModalidadeBaseCalculo.ValorOperacao, BaseCalculo = 1000m, Aliquota = 18m, Valor = 180m },
            Ipi = new Ipi { CodigoEnquadramento = "999", Tributado = new IpiTributado { Cst = CstIpi.SaidaTributada, BaseCalculo = 1000m, Aliquota = 10m, Valor = 100m } },
            Pis = new PisAliquota { Cst = CstPisCofins.AliquotaBasica, BaseCalculo = 1000m, Aliquota = 1.65m, Valor = 16.5m },
            Cofins = new CofinsAliquota { Cst = CstPisCofins.AliquotaBasica, BaseCalculo = 1000m, Aliquota = 7.6m, Valor = 76m },
            IbsCbs = new IbsCbs
            {
                Cst = "000", ClassificacaoTributaria = "000001",
                Tributacao = new TributacaoIbsCbs
                {
                    BaseCalculo = 1000m, ValorIbs = 1m,
                    IbsUF = { Aliquota = 0.1m, Valor = 1m },
                    IbsMunicipio = { Aliquota = 0m, Valor = 0m },
                    Cbs = { Aliquota = 0.9m, Valor = 9m },
                },
            },
        },
        InformacoesAdicionais = "GARANTIA DE 1 ANO",
    };

    internal static Item ItemServico() => new()
    {
        Produto =
        {
            Codigo = "S1", Gtin = "SEM GTIN", Descricao = "INSTALACAO", Ncm = "00", Cfop = 6933, UnidadeComercial = "UN",
            QuantidadeComercial = 1m, ValorUnitarioComercial = 122m, Valor = 122m, GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN",
            QuantidadeTributavel = 1m, ValorUnitarioTributavel = 122m, Totalizacao = IndicadorTotal.NaoCompoe,
        },
        Imposto =
        {
            Issqn = new Issqn
            {
                BaseCalculo = 122m, Aliquota = 5m, Valor = 6.1m, MunicipioFatoGerador = 3106200, ItemListaServico = "14.06",
                Exigibilidade = ExigibilidadeIss.Exigivel, IncentivoFiscal = IndicadorIncentivo.Nao,
            },
            Pis = new PisNaoTributado { Cst = CstPisCofins.Isenta },
            Cofins = new CofinsNaoTributado { Cst = CstPisCofins.Isenta },
        },
    };

    internal static Emitente Emitente(RegimeTributario regime) => new()
    {
        Cnpj = CnpjEmitente, Nome = "LOJA EXEMPLO LTDA", InscricaoEstadual = "0010000000000", RegimeTributario = regime,
        Endereco =
        {
            Logradouro = "RUA A", Numero = "1", Bairro = "CENTRO", CodigoMunicipio = 3106200, Municipio = "BELO HORIZONTE",
            UF = UF.MG, Cep = "30110000",
        },
    };

    // O item do exemplo do desenho (MODELO.md 11.11), sem o IBS e a CBS.
    internal static Item ItemSimples() => new()
    {
        Produto =
        {
            Codigo = "123", Gtin = "SEM GTIN", Descricao = "CAFE TORRADO 500G", Ncm = "09012100", Cfop = 5102,
            UnidadeComercial = "UN", QuantidadeComercial = 2m, ValorUnitarioComercial = 25.00m, Valor = 50.00m,
            GtinTributavel = "SEM GTIN", UnidadeTributavel = "UN", QuantidadeTributavel = 2m, ValorUnitarioTributavel = 25.00m,
            Totalizacao = IndicadorTotal.Compoe,
        },
        Imposto =
        {
            Icms = new IcmsSn102 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.NaoPermiteCredito },
            Pis = new PisOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
            Cofins = new CofinsOutros { Cst = CstPisCofins.OutrasSaidas, BaseCalculo = 0m, Aliquota = 0m, Valor = 0m },
        },
    };
}
