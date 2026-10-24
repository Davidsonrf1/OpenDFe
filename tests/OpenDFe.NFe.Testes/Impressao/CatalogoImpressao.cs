using OpenDFe.Impressao;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>Um caso do catálogo: a nota lida, o documento de impressão que o leiaute montou e o que o PDF precisa trazer.</summary>
/// <param name="Nome">O nome do caso (o arquivo da amostra de inspeção).</param>
/// <param name="Lida">A nota lida.</param>
/// <param name="Documento">O documento de impressão montado.</param>
internal sealed record CasoImpressao(string Nome, NotaLida Lida, DocumentoImpressao Documento)
{
    /// <summary>Se a nota tem o protocolo de autorização.</summary>
    public bool Autorizada => !string.IsNullOrEmpty(Lida.Protocolo?.Numero);

    public override string ToString() => Nome;
}

/// <summary>
/// Os documentos completos do DANFCE (etapa 7.2) e do DANFE (etapa 7.3), os mesmos casos dos golden de texto, para o PDF
/// (etapa 7.4): cada caso é a nota do golden montada de novo pelos auxiliares <see cref="NotasDanfce"/> e
/// <see cref="NotasDanfe"/>, e o documento sai do leiaute de verdade.
/// </summary>
internal static class CatalogoImpressao
{
    private static readonly LeiauteNFe400 Leiaute = new();

    private static CasoImpressao Danfce(string nome, NotaLida lida, OpcoesDanfce? opcoes = null) =>
        new(nome, lida, Leiaute.Danfce(lida, opcoes));

    private static CasoImpressao Danfe(string nome, NotaLida lida, OpcoesDanfe? opcoes = null) =>
        new(nome, lida, Leiaute.Danfe(lida, opcoes));

    /// <summary>Os casos do DANFCE (a NFC-e na bobina de 80 e de 58 mm), 20 notas.</summary>
    public static IReadOnlyList<CasoImpressao> Danfces()
    {
        var casos = new List<CasoImpressao>
        {
            Danfce("Exemplo1", NotasDanfce.Autorizada(NotasDanfce.Exemplo1())),
            Danfce("Exemplo2", NotasDanfce.Autorizada(NotasDanfce.Exemplo2())),
            Danfce("Exemplo3Consumidor", NotasDanfce.Lida(NotasDanfce.Exemplo3()),
                new OpcoesDanfce { Logotipo = NotasDanfce.Logotipo, Via = ViaDanfce.Consumidor }),
            Danfce("Exemplo3Estabelecimento", NotasDanfce.Lida(NotasDanfce.Exemplo3()),
                new OpcoesDanfce { Logotipo = NotasDanfce.Logotipo, Via = ViaDanfce.Estabelecimento }),
            Danfce("UmItemDinheiroTroco", NotasDanfce.Autorizada(NotasDanfce.UmItem())),
            Danfce("ContingenciaAutorizada", NotasDanfce.Autorizada(NotasDanfce.Exemplo3())),
            Danfce("Bobina58Contingencia", NotasDanfce.Lida(NotasDanfce.Exemplo3()), new OpcoesDanfce { Papel = PapelImpressao.Bobina58 }),
            Danfce("Bobina58Exemplo1", NotasDanfce.Autorizada(NotasDanfce.Exemplo1()), new OpcoesDanfce { Papel = PapelImpressao.Bobina58 }),
            Danfce("LogotipoAssinaturaERodape", NotasDanfce.Autorizada(NotasDanfce.UmItem()),
                new OpcoesDanfce { Logotipo = NotasDanfce.Logotipo, AssinaturaSoftware = NotasDanfce.Assinatura, TextoRodape = "ERP Core - DLA Tec" }),
        };

        var cinco = NotasDanfce.Base();
        cinco.Item("001", "ARROZ TIPO 1 5KG", 2m, "PCT", 24.90m);
        cinco.Item("002", "FEIJAO CARIOCA 1KG", 3m, "PCT", 8.49m);
        cinco.Item("003", "OLEO DE SOJA 900ML", 1m, "UN", 7.99m);
        cinco.Item("004", "ACUCAR REFINADO 1KG", 4m, "PCT", 4.59m);
        cinco.Item("005", "CAFE EM PO 500G", 2m, "PCT", 18.75m);
        cinco.Totalizar(desconto: 10.00m, frete: 12.00m, seguro: 1.50m, outras: 2.25m);
        cinco.Pagar(MeioPagamento.CartaoDebito, 155.0m);
        casos.Add(Danfce("CincoItensDescontoEAcrescimos", NotasDanfce.Autorizada(cinco)));

        var cartaoDinheiro = NotasDanfce.Base();
        cartaoDinheiro.Item("10", "CAMISETA ALGODAO", 3m, "UN", 59.90m);
        cartaoDinheiro.Totalizar();
        cartaoDinheiro.Pagar(MeioPagamento.CartaoCredito, 100m);
        cartaoDinheiro.Pagar(MeioPagamento.Dinheiro, 85m);
        cartaoDinheiro.Pagamento.Troco = 5.30m;
        casos.Add(Danfce("CartaoEDinheiro", NotasDanfce.Autorizada(cartaoDinheiro)));

        var cpf = NotasDanfce.UmItem();
        cpf.Destinatario = new Destinatario { Cpf = "52998224725", Nome = "José da Silva" };
        casos.Add(Danfce("ConsumidorCpf", NotasDanfce.Autorizada(cpf)));

        var estrangeiro = NotasDanfce.UmItem();
        estrangeiro.Destinatario = new Destinatario { IdEstrangeiro = "AB1234567", Nome = "John Smith" };
        casos.Add(Danfce("ConsumidorEstrangeiro", NotasDanfce.Autorizada(estrangeiro)));

        var entrega = NotasDanfce.UmItem();
        entrega.Identificacao.Presenca = PresencaComprador.NaoPresencialEntrega;
        entrega.Destinatario = new Destinatario
        {
            Nome = "Maria Souza",
            Endereco = new EnderecoDestinatario { Logradouro = "Rua das Flores", Numero = "10", Complemento = "apto 2", Bairro = "Centro", Municipio = "Contagem", UF = UF.MG },
        };
        casos.Add(Danfce("EntregaSemDocumento", NotasDanfce.Autorizada(entrega)));

        var homologacao = NotasDanfce.UmItem();
        homologacao.Identificacao.Ambiente = Ambiente.Homologacao;
        casos.Add(Danfce("Homologacao", NotasDanfce.Autorizada(homologacao)));

        var longos = NotasDanfce.Base();
        longos.Item("7891234567895", "TELEVISOR LED 55 POLEGADAS 4K ULTRA HD SMART COM CONVERSOR DIGITAL INTEGRADO E WIFI", 1m, "UN", 2899.90m);
        longos.Item("PALAVRA-GRANDE-DEMAIS-PARA-CABER-EM-UMA-LINHA-DA-BOBINA-DE-OITENTA", "ITEM", 1m, "UN", 1m);
        longos.Totalizar();
        longos.Pagar(MeioPagamento.Cheque, 2900.90m);
        longos.InformacoesAdicionais = new InformacoesAdicionais
        {
            Fisco = "Documento emitido por ME ou EPP optante pelo Simples Nacional.",
            Complementares = "Troca somente com a nota fiscal em até 7 dias corridos. Produto com garantia de 12 meses contra defeitos de fabricação. "
                + "Atendimento ao cliente: 0800 000 0000, de segunda a sexta, das 8h às 18h.\nVolte sempre!",
        };
        casos.Add(Danfce("DescricaoEInfCplLongos", NotasDanfce.Autorizada(longos)));

        var fracionada = NotasDanfce.Base();
        fracionada.Item("QF1", "QUEIJO MUSSARELA KG", 1.235m, "KG", 45.90m, 56.69m);
        fracionada.Item("QF2", "NOTEBOOK AÇAÍ EDIÇÃO LIMITADA", 2m, "UN", 12345.67m);
        fracionada.Item("QF3", "PARAFUSO", 1000m, "UN", 0.123456m, 123.46m);
        fracionada.Totalizar();
        fracionada.Pagar(MeioPagamento.CartaoCredito, 24871.49m);
        casos.Add(Danfce("QuantidadeFracionadaEMilhar", NotasDanfce.Autorizada(fracionada)));

        var emitenteCpf = NotasDanfce.UmItem();
        (emitenteCpf.Emitente.Cnpj, emitenteCpf.Emitente.Cpf) = (null, "52998224725");
        emitenteCpf.Emitente.Nome = "João da Silva - Produtor Rural";
        casos.Add(Danfce("EmitenteCpf", NotasDanfce.Autorizada(emitenteCpf)));

        return casos;
    }

    /// <summary>Os casos do DANFE do retrato e da paisagem (A4) e do Simplificado Tipo 2 (bobina), 28 notas.</summary>
    public static IReadOnlyList<CasoImpressao> Danfes()
    {
        var casos = new List<CasoImpressao>
        {
            Danfe("Simples", NotasDanfe.Autorizada(NotasDanfe.Simples())),
            Danfe("Paisagem", NotasDanfe.Autorizada(NotasDanfe.Base(formato: FormatoImpressao.DanfePaisagem).AcrescentarUmItem())),
            Danfe("SemCanhoto", NotasDanfe.Autorizada(NotasDanfe.Simples()), new OpcoesDanfe { Canhoto = false }),
            Danfe("Cancelada", NotasDanfe.Autorizada(NotasDanfe.Simples()), new OpcoesDanfe { Cancelada = true }),
            Danfe("LogotipoRodape", NotasDanfe.Autorizada(NotasDanfe.Simples()),
                new OpcoesDanfe { Logotipo = NotasDanfe.Logotipo, TextoRodape = "Impresso por ERP Core" }),
        };

        var simples = NotasDanfe.Base();
        simples.Emitente.RegimeTributario = RegimeTributario.SimplesNacional;
        simples.ItemDanfe("CAF-1", "Café torrado 500g", 2m, "UN", 25m,
            new IcmsSn102 { Origem = OrigemMercadoria.EstrangeiraImportacaoDireta, Csosn = CsosnIcms.NaoPermiteCredito }, ncm: "09012100");
        simples.TotalizarDanfe();
        casos.Add(Danfe("SimplesNacional", NotasDanfe.Autorizada(simples)));

        var st = NotasDanfe.Base();
        st.ItemDanfe("MAQ-1", "Máquina industrial", 1m, "UN", 1000m,
            new Icms10
            {
                Origem = OrigemMercadoria.Nacional, Cst = CstIcms.TributadaSubstituicao, ModalidadeBaseCalculo = ModalidadeBaseCalculo.ValorOperacao,
                BaseCalculo = 1000m, Aliquota = 18m, Valor = 180m, ModalidadeBaseCalculoSt = ModalidadeBaseCalculoSt.MargemValorAgregado,
                BaseCalculoSt = 1400m, AliquotaSt = 18m, ValorSt = 72m,
            },
            NotasDanfe.IpiTributado(1000m, 10m), ncm: "84798999", cfop: 5401);
        st.TotalizarDanfe(frete: 30m, seguro: 5m, outras: 2.5m, tributos: 372.5m);
        casos.Add(Danfe("IcmsStIpi", NotasDanfe.Autorizada(st)));

        var servico = NotasDanfe.Base();
        servico.Itens.Add(new Item
        {
            Produto =
            {
                Codigo = "S1", Descricao = "Instalação de equipamento", Ncm = "00", Cfop = 5933, UnidadeComercial = "UN", QuantidadeComercial = 1m,
                ValorUnitarioComercial = 122m, Valor = 122m, Totalizacao = IndicadorTotal.NaoCompoe,
            },
            Imposto = { Issqn = new Issqn { BaseCalculo = 122m, Aliquota = 5m, Valor = 6.1m, ItemListaServico = "14.06" } },
        });
        servico.TotalizarDanfe();
        servico.Total.Issqn = new TotalIssqn { ValorServicos = 122m, BaseCalculo = 122m, Valor = 6.1m };
        casos.Add(Danfe("ServicoIssqn", NotasDanfe.Autorizada(servico)));

        var duplicatas = NotasDanfe.Simples();
        duplicatas.Cobranca = new Cobranca { Fatura = new Fatura { Numero = "123", ValorOriginal = 150m, Desconto = 0m, ValorLiquido = 150m } };
        for (var i = 1; i <= 4; i++)
            duplicatas.Cobranca.Duplicatas.Add(new Duplicata { Numero = $"00{i}", Vencimento = new DateOnly(2026, 10, 14).AddMonths(i), Valor = 37.5m });
        casos.Add(Danfe("Duplicatas", NotasDanfe.Autorizada(duplicatas)));

        var transporte = NotasDanfe.Simples();
        transporte.Transporte.Modalidade = ModalidadeFrete.Destinatario;
        transporte.Transporte.Transportador = new Transportador
        {
            Cnpj = "11444777000161", Nome = "Transportes Rápidos SA", InscricaoEstadual = "110042490114",
            Endereco = "Rod. BR-040, km 10", Municipio = "Sete Lagoas", UF = UF.MG,
        };
        transporte.Transporte.Veiculo = new Veiculo { Placa = "ABC1D23", UF = UF.MG, Rntc = "12345678" };
        transporte.Transporte.Volumes.Add(new Volume { Quantidade = 2, Especie = "CAIXA", Marca = "EXEMPLO", Numeracao = "001", PesoBruto = 11.25m, PesoLiquido = 10.5m });
        casos.Add(Danfe("Transporte", NotasDanfe.Autorizada(transporte)));

        var retirada = NotasDanfe.Simples();
        retirada.Retirada = new Local
        {
            Cnpj = NotasDanfe.Cnpj, Nome = "Depósito Central", InscricaoEstadual = "0623079040081", Logradouro = "Rua B", Numero = "2",
            Bairro = "Distrito Industrial", Municipio = "Contagem", UF = UF.MG, Cep = "32000000", Telefone = "3133335555",
        };
        retirada.Entrega = new Local
        {
            Cpf = "52998224725", Nome = "Maria Recebedora", Logradouro = "Rua C", Numero = "3", Complemento = "Apto 4", Bairro = "Savassi",
            Municipio = "Belo Horizonte", UF = UF.MG, Cep = "30140000",
        };
        casos.Add(Danfe("RetiradaEntrega", NotasDanfe.Autorizada(retirada)));

        var pessoaFisica = NotasDanfe.Simples();
        pessoaFisica.Destinatario = new Destinatario
        {
            Cpf = "52998224725", Nome = "José da Silva", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte,
            Endereco = new EnderecoDestinatario { Logradouro = "Rua D", Numero = "4", Bairro = "Centro", Municipio = "Nova Lima", UF = UF.MG, Cep = "34000000" },
        };
        casos.Add(Danfe("DestinatarioCpf", NotasDanfe.Autorizada(pessoaFisica)));

        var medicamento = NotasDanfe.Base();
        var item = medicamento.ItemDanfe("MED-1", "Analgésico 500mg cx 20", 12m, "CX", 8.99m,
            adicional: "Validade mínima de 12 meses\nArmazenar abaixo de 25 graus", ncm: "30049069");
        item.Produto.Gtin = "7891234567895";
        item.Produto.Rastros.Add(new Rastro { Lote = "L2026A", Quantidade = 7m, Fabricacao = new DateOnly(2026, 1, 10), Validade = new DateOnly(2028, 1, 10) });
        item.Produto.Medicamento = new Medicamento { CodigoAnvisa = "1234567890123", PrecoMaximoConsumidor = 12.5m };
        medicamento.TotalizarDanfe();
        casos.Add(Danfe("InfAdProd", NotasDanfe.Autorizada(medicamento)));

        var adicionais = NotasDanfe.Simples();
        adicionais.InformacoesAdicionais = new InformacoesAdicionais
        {
            Fisco = "Valores totais do ICMS Interestadual: DIFAL da UF destino R$216,00 + FCP R$40,00",
            Complementares = "PEDIDO 99 | ENTREGA EM HORARIO COMERCIAL",
        };
        adicionais.Total.Icms.ValorFcp = 3m;
        adicionais.Total.Icms.ValorIcmsUFDestino = 216m;
        adicionais.Total.Icms.ValorFcpUFDestino = 40m;
        adicionais.Total.Icms.ValorDesonerado = 12.34m;
        adicionais.Total.Icms.ValorTotalTributos = 50.5m;
        casos.Add(Danfe("Adicionais", NotasDanfe.Autorizada(adicionais)));

        var homologacao = NotasDanfe.Simples();
        homologacao.Identificacao.Ambiente = Ambiente.Homologacao;
        casos.Add(Danfe("Homologacao", NotasDanfe.Autorizada(homologacao)));

        foreach (var (emissao, nome) in new[]
        {
            (TipoEmissao.ContingenciaFSDA, "ContingenciaFsda"), (TipoEmissao.ContingenciaFSIA, "ContingenciaFsia"),
        })
        {
            var contingencia = NotasDanfe.Simples();
            contingencia.Identificacao.TipoEmissao = emissao;
            contingencia.Identificacao.InicioContingencia = NotasDanfe.Emissao.AddMinutes(-20);
            contingencia.Identificacao.JustificativaContingencia = "Falha no link com a SEFAZ";
            casos.Add(Danfe(nome, NotasDanfe.Lida(contingencia)));
        }

        var epec = NotasDanfe.Simples();
        epec.Identificacao.TipoEmissao = TipoEmissao.ContingenciaEpec;
        casos.Add(Danfe("ContingenciaEpec", NotasDanfe.Lida(epec)));

        var sessenta = NotasDanfe.Base();
        for (var i = 1; i <= 60; i++)
            sessenta.ItemDanfe($"P{i:000}", $"Produto de teste número {i}", i, "UN", 1.5m + i / 100m);
        sessenta.TotalizarDanfe();
        casos.Add(Danfe("SessentaItens", NotasDanfe.Autorizada(sessenta)));

        // O Tipo 2 (tpImp 6): o leiaute do DANFCE numa NF-e, na bobina.
        casos.Add(Danfe("Tipo2Normal", NotasDanfe.Autorizada(Tipo2()), new OpcoesDanfe { Logotipo = NotasDanfe.Logotipo }));
        casos.Add(Danfe("Tipo2Bobina58", NotasDanfe.Autorizada(Tipo2(tributos: true)), new OpcoesDanfe { Papel = PapelImpressao.Bobina58 }));
        casos.Add(Danfe("Tipo2ContingenciaEstabelecimento", NotasDanfe.Lida(Tipo2Contingencia()), new OpcoesDanfe { Via = ViaDanfce.Estabelecimento }));

        return casos;
    }

    /// <summary>A NF-e do Tipo 2 (a do <c>DanfeSimplificadoTestes</c>): os dois itens do exemplo 2 da NT 2026.003.</summary>
    internal static NotaFiscal Tipo2(bool tributos = false)
    {
        var nota = NotasDanfe.Base(formato: FormatoImpressao.DanfeSimplificadoTipo2);
        nota.Identificacao.ConsumidorFinal = IndicadorConsumidorFinal.ConsumidorFinal;
        nota.Destinatario = new Destinatario { Cnpj = "99999999000191", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte };
        nota.ItemDanfe("003277", "Cadeira Est. Comtemp.", 2m, "Peça", 500m);
        nota.ItemDanfe("085273", "Mesa", 1m, "Peça", 1500m);
        nota.TotalizarDanfe(desconto: 500m, frete: 50m);
        nota.Suplementares = new InformacoesSuplementares { QrCode = NotasDanfe.QrCodeFixo, UrlChave = NotasDanfe.UrlConsulta };
        if (tributos)
        {
            nota.Total.IbsCbs = new TotalIbsCbs { Ibs = new TotalIbs { Valor = 2.05m }, Cbs = new TotalCbs { Valor = 18.45m } };
            nota.Total.ImpostoSeletivo = new TotalImpostoSeletivo { Valor = 2m };
            nota.Total.Icms.ValorNota += 18.45m + 2.05m + 2m;
        }

        return nota;
    }

    private static NotaFiscal Tipo2Contingencia()
    {
        var nota = Tipo2();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaOffline;
        nota.Identificacao.InicioContingencia = NotasDanfe.Emissao.AddMinutes(-30);
        nota.Identificacao.JustificativaContingencia = "Sem comunicacao com a SEFAZ";
        return nota;
    }
}
