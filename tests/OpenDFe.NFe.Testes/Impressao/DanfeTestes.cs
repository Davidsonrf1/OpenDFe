using OpenDFe.Impressao;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// O DANFE da NF-e (etapa 7.3): os golden files de texto, conferidos linha a linha contra o Anexo II do MOC 7.0 e a NT 2026.010
/// (docs/IMPRESSAO.md 2), do retrato e da paisagem aos casos de borda. O texto vem de <see cref="TextoPlano"/> (só de teste).
/// </summary>
public sealed class DanfeTestes
{
    private static readonly LeiauteNFe400 Leiaute = new();

    private static string Texto(NotaLida lida, OpcoesDanfe? opcoes = null) => TextoPlano.Renderizar(Leiaute.Danfe(lida, opcoes));

    [Fact]
    public void UmaNotaSimplesNoRetrato() =>
        ArquivoEsperado.Conferir("DanfeSimples", Texto(NotasDanfe.Autorizada(NotasDanfe.Simples())));

    [Fact]
    public void UmaNotaSimplesNaPaisagem() =>
        ArquivoEsperado.Conferir("DanfePaisagem",
            Texto(NotasDanfe.Autorizada(NotasDanfe.Base(formato: FormatoImpressao.DanfePaisagem).AcrescentarUmItem())));

    [Fact]
    public void ORetratoSemCanhoto() =>
        ArquivoEsperado.Conferir("DanfeSemCanhoto",
            Texto(NotasDanfe.Autorizada(NotasDanfe.Simples()), new OpcoesDanfe { Canhoto = false }));

    [Fact]
    public void ApaisagemNaoSuprimeOCanhoto()
    {
        var nota = NotasDanfe.Base(formato: FormatoImpressao.DanfePaisagem).AcrescentarUmItem();
        var completo = Texto(NotasDanfe.Autorizada(nota));
        var suprimido = Texto(NotasDanfe.Autorizada(nota), new OpcoesDanfe { Canhoto = false });

        Assert.Equal(completo, suprimido);
        Assert.StartsWith(" P |RECEBEMOS DE", completo, StringComparison.Ordinal);
    }

    [Fact]
    public void ARetratoEPaisagemTemOMesmoConteudoEOutroPapel()
    {
        var retrato = Leiaute.Danfe(NotasDanfe.Autorizada(NotasDanfe.Base().AcrescentarUmItem()));
        var paisagem = Leiaute.Danfe(NotasDanfe.Autorizada(NotasDanfe.Base(formato: FormatoImpressao.DanfePaisagem).AcrescentarUmItem()));

        Assert.Same(PapelImpressao.A4Retrato, retrato.Papel);
        Assert.Same(PapelImpressao.A4Paisagem, paisagem.Papel);
        Assert.Equal(TextoPlano.Renderizar(retrato), TextoPlano.Renderizar(paisagem));
    }

    [Fact]
    public void AOrientacaoDasOpcoesVenceOTpImpDaNota()
    {
        var nota = NotasDanfe.Autorizada(NotasDanfe.Base().AcrescentarUmItem());

        Assert.Same(PapelImpressao.A4Paisagem, Leiaute.Danfe(nota, new OpcoesDanfe { Orientacao = OrientacaoDanfe.Paisagem }).Papel);
        Assert.Same(PapelImpressao.A4Retrato, Leiaute.Danfe(nota, new OpcoesDanfe { Orientacao = OrientacaoDanfe.Retrato }).Papel);
    }

    [Fact]
    public void ASimplesNacionalComCsosnEOrigemNoCst()
    {
        var nota = NotasDanfe.Base();
        nota.Emitente.RegimeTributario = RegimeTributario.SimplesNacional;
        nota.ItemDanfe("CAF-1", "Café torrado 500g", 2m, "UN", 25m,
            new IcmsSn102 { Origem = OrigemMercadoria.EstrangeiraImportacaoDireta, Csosn = CsosnIcms.NaoPermiteCredito }, ncm: "09012100");
        nota.TotalizarDanfe();
        ArquivoEsperado.Conferir("DanfeSimplesNacional", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComIcmsComSubstituicaoTributariaEIpi()
    {
        var nota = NotasDanfe.Base();
        nota.ItemDanfe("MAQ-1", "Máquina industrial", 1m, "UN", 1000m,
            new Icms10
            {
                Origem = OrigemMercadoria.Nacional, Cst = CstIcms.TributadaSubstituicao, ModalidadeBaseCalculo = ModalidadeBaseCalculo.ValorOperacao,
                BaseCalculo = 1000m, Aliquota = 18m, Valor = 180m, ModalidadeBaseCalculoSt = ModalidadeBaseCalculoSt.MargemValorAgregado,
                BaseCalculoSt = 1400m, AliquotaSt = 18m, ValorSt = 72m,
            },
            NotasDanfe.IpiTributado(1000m, 10m), ncm: "84798999", cfop: 5401);
        nota.TotalizarDanfe(frete: 30m, seguro: 5m, outras: 2.5m, tributos: 372.5m);
        ArquivoEsperado.Conferir("DanfeIcmsStIpi", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComOIcmsDeCadaGrupoNasColunas()
    {
        var nota = NotasDanfe.Base();
        nota.ItemDanfe("A", "Reducao de base", 1m, "UN", 100m,
            new Icms20 { Origem = OrigemMercadoria.Nacional, Cst = CstIcms.ReducaoBase, BaseCalculo = 50m, Aliquota = 12m, Valor = 6m });
        nota.ItemDanfe("B", "Cobrado antes por ST", 1m, "UN", 100m,
            new Icms60 { Origem = OrigemMercadoria.Nacional, Cst = CstIcms.CobradoAnteriormente, BaseCalculoStRetido = 100m, ValorStRetido = 18m });
        nota.ItemDanfe("C", "Isenta", 1m, "UN", 100m, new Icms40 { Origem = OrigemMercadoria.Nacional, Cst = CstIcms.Isenta });
        nota.ItemDanfe("D", "ST do Simples", 1m, "UN", 100m,
            new IcmsSn201 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.PermiteCreditoSubstituicao, BaseCalculoSt = 130m, ValorSt = 23.4m });
        nota.ItemDanfe("E", "Outras do Simples", 1m, "UN", 100m,
            new IcmsSn900 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.Outros, BaseCalculo = 100m, Aliquota = 7m, Valor = 7m });
        nota.ItemDanfe("F", "Sem ICMS", 1m, "UN", 100m, new Icms02 { Origem = OrigemMercadoria.Nacional, Cst = CstIcms.MonofasicaPropria });
        nota.TotalizarDanfe();
        ArquivoEsperado.Conferir("DanfeIcmsGrupos", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComServicoEOIssqn()
    {
        var nota = NotasDanfe.Base();
        nota.Itens.Add(new Item
        {
            Produto =
            {
                Codigo = "S1", Descricao = "Instalação de equipamento", Ncm = "00", Cfop = 5933, UnidadeComercial = "UN", QuantidadeComercial = 1m,
                ValorUnitarioComercial = 122m, Valor = 122m, Totalizacao = IndicadorTotal.NaoCompoe,
            },
            Imposto = { Issqn = new Issqn { BaseCalculo = 122m, Aliquota = 5m, Valor = 6.1m, ItemListaServico = "14.06" } },
        });
        nota.TotalizarDanfe();
        nota.Total.Issqn = new TotalIssqn { ValorServicos = 122m, BaseCalculo = 122m, Valor = 6.1m };
        ArquivoEsperado.Conferir("DanfeServicoIssqn", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComFaturaEDuplicatas()
    {
        var nota = NotasDanfe.Simples();
        nota.Cobranca = new Cobranca
        {
            Fatura = new Fatura { Numero = "123", ValorOriginal = 150m, Desconto = 0m, ValorLiquido = 150m },
        };
        for (var i = 1; i <= 4; i++)
            nota.Cobranca.Duplicatas.Add(new Duplicata { Numero = $"00{i}", Vencimento = new DateOnly(2026, 10, 14).AddMonths(i), Valor = 37.5m });
        ArquivoEsperado.Conferir("DanfeDuplicatas", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComTransportadorEVolumes()
    {
        var nota = NotasDanfe.Simples();
        nota.Transporte.Modalidade = ModalidadeFrete.Destinatario;
        nota.Transporte.Transportador = new Transportador
        {
            Cnpj = "11444777000161", Nome = "Transportes Rápidos SA", InscricaoEstadual = "110042490114",
            Endereco = "Rod. BR-040, km 10", Municipio = "Sete Lagoas", UF = UF.MG,
        };
        nota.Transporte.Veiculo = new Veiculo { Placa = "ABC1D23", UF = UF.MG, Rntc = "12345678" };
        nota.Transporte.Volumes.Add(new Volume { Quantidade = 2, Especie = "CAIXA", Marca = "EXEMPLO", Numeracao = "001", PesoBruto = 11.25m, PesoLiquido = 10.5m });
        nota.Transporte.Volumes.Add(new Volume { Quantidade = 1, Especie = "PALETE", PesoBruto = 120m, PesoLiquido = 100m });
        ArquivoEsperado.Conferir("DanfeTransporte", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComLocalDeRetiradaEDeEntrega()
    {
        var nota = NotasDanfe.Simples();
        nota.Retirada = new Local
        {
            Cnpj = NotasDanfe.Cnpj, Nome = "Depósito Central", InscricaoEstadual = "0623079040081", Logradouro = "Rua B", Numero = "2",
            Bairro = "Distrito Industrial", Municipio = "Contagem", UF = UF.MG, Cep = "32000000", Telefone = "3133335555",
        };
        nota.Entrega = new Local
        {
            Cpf = "52998224725", Nome = "Maria Recebedora", Logradouro = "Rua C", Numero = "3", Complemento = "Apto 4", Bairro = "Savassi",
            Municipio = "Belo Horizonte", UF = UF.MG, Cep = "30140000",
        };
        ArquivoEsperado.Conferir("DanfeRetiradaEntrega", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComDestinatarioPessoaFisica()
    {
        var nota = NotasDanfe.Simples();
        nota.Destinatario = new Destinatario
        {
            Cpf = "52998224725", Nome = "José da Silva", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte,
            Endereco = new EnderecoDestinatario { Logradouro = "Rua D", Numero = "4", Bairro = "Centro", Municipio = "Nova Lima", UF = UF.MG, Cep = "34000000" },
        };
        ArquivoEsperado.Conferir("DanfeDestinatarioCpf", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComDestinatarioEstrangeiroEOperacaoDeExportacao()
    {
        var nota = NotasDanfe.Base();
        nota.Identificacao.LocalDestino = LocalDestino.Exterior;
        nota.Identificacao.NaturezaOperacao = "EXPORTACAO DE MERCADORIA";
        nota.Destinatario = new Destinatario
        {
            IdEstrangeiro = "AR12345678", Nome = "Cliente Internacional SA", IndicadorIE = IndicadorIEDestinatario.NaoContribuinte,
            Endereco = new EnderecoDestinatario
            {
                Logradouro = "Calle Florida", Numero = "100", Bairro = "Retiro", Municipio = "Buenos Aires", Exterior = true, CodigoPais = 639,
                Pais = "ARGENTINA", Cep = "1005",
            },
        };
        nota.Exportacao = new Exportacao { UFSaida = UF.SP, LocalEmbarque = "Porto de Santos" };
        nota.ItemDanfe("EXP-1", "Café em grãos 60kg", 100m, "SC", 450m,
            new IcmsSn102 { Origem = OrigemMercadoria.Nacional, Csosn = CsosnIcms.NaoPermiteCredito }, ncm: "09011110", cfop: 7101);
        nota.TotalizarDanfe();
        ArquivoEsperado.Conferir("DanfeExportacao", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComInformacoesAdicionaisDoProdutoELoteEMedicamento()
    {
        var nota = NotasDanfe.Base();
        var item = nota.ItemDanfe("MED-1", "Analgésico 500mg cx 20", 12m, "CX", 8.99m, adicional: "Validade mínima de 12 meses\nArmazenar abaixo de 25 graus", ncm: "30049069");
        item.Produto.Gtin = "7891234567895";
        item.Produto.UnidadeTributavel = "UN";
        item.Produto.QuantidadeTributavel = 240m;
        item.Produto.ValorUnitarioTributavel = 0.4495m;
        item.Produto.Rastros.Add(new Rastro { Lote = "L2026A", Quantidade = 7m, Fabricacao = new DateOnly(2026, 1, 10), Validade = new DateOnly(2028, 1, 10) });
        item.Produto.Rastros.Add(new Rastro { Lote = "L2026B", Quantidade = 5m, Fabricacao = new DateOnly(2026, 2, 15), Validade = new DateOnly(2028, 2, 15) });
        item.Produto.Medicamento = new Medicamento { CodigoAnvisa = "1234567890123", PrecoMaximoConsumidor = 12.5m };
        nota.TotalizarDanfe();
        ArquivoEsperado.Conferir("DanfeInfAdProd", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComVeiculoCombustivelEArma()
    {
        var nota = NotasDanfe.Base();
        var veiculo = nota.ItemDanfe("V1", "Automóvel novo", 1m, "UN", 80000m, ncm: "87032310");
        veiculo.Produto.VeiculoNovo = new VeiculoNovo { Chassi = "9BWZZZ377VT004251", Cor = "PRATA", MarcaModelo = "EXEMPLO 1.0" };
        var combustivel = nota.ItemDanfe("C1", "Gasolina comum", 40m, "L", 5.5m, ncm: "27101259");
        combustivel.Produto.Combustivel = new Combustivel { CodigoAnp = "320101001", DescricaoAnp = "GASOLINA C COMUM" };
        var arma = nota.ItemDanfe("A1", "Pistola", 1m, "UN", 3000m, ncm: "93020000");
        arma.Produto.Armas.Add(new Arma { Serie = "S123", SerieCano = "C456", Descricao = "Calibre 380" });
        nota.TotalizarDanfe();
        ArquivoEsperado.Conferir("DanfeVeiculoCombustivelArma", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ComSessentaItensATabelaInteiraNoModelo()
    {
        var nota = NotasDanfe.Base();
        for (var i = 1; i <= 60; i++)
            nota.ItemDanfe($"P{i:000}", $"Produto de teste número {i}", i, "UN", 1.5m + i / 100m);
        nota.TotalizarDanfe();
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));

        var tabela = Assert.Single(documento.Elementos.OfType<TabelaImpressao>());
        Assert.Equal(60, tabela.Linhas.Count);
        Assert.True(tabela.LinhasIndivisiveis);
        Assert.True(tabela.DivisorLinhas);
        ArquivoEsperado.Conferir("DanfeSessentaItens", TextoPlano.Renderizar(documento));
    }

    [Fact]
    public void ComIbsCbsEImpostoSeletivo()
    {
        var nota = NotasTeste.NFe();
        nota.Itens.RemoveAt(1);
        nota.Retirada = null;
        nota.Entrega = null;
        nota.Identificacao.Emissao = NotasDanfe.Emissao;
        nota.Identificacao.SaidaEntrada = NotasDanfe.Emissao;
        nota.Identificacao.Ambiente = Ambiente.Producao;
        nota.Identificacao.Modelo = Documento.NFe;
        nota.Identificacao.TipoEmissao = TipoEmissao.Normal;
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeRetrato;
        nota.Suplementares = new InformacoesSuplementares { UrlChave = NotasDanfe.UrlConsulta };
        nota.Total.Issqn = null;
        nota.Total.ImpostoSeletivo = new TotalImpostoSeletivo { Valor = 2m };
        ((Icms00)nota.Itens[0].Imposto.Icms!).Cst = CstIcms.Tributada;
        var tributacao = nota.Itens[0].Imposto.IbsCbs!.Tributacao!;
        tributacao.IbsUF.Reducao = new ReducaoAliquota { Percentual = 50m, AliquotaEfetiva = 0.05m };
        nota.Itens[0].Imposto.ImpostoSeletivo = new ImpostoSeletivo { Cst = "000", ClassificacaoTributaria = "000001", BaseCalculo = 1000m, Aliquota = 0.2m, Valor = 2m };
        ArquivoEsperado.Conferir("DanfeIbsCbs", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void EmHomologacaoComAMarcaDAguaEAFraseNasInformacoes()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.Ambiente = Ambiente.Homologacao;
        ArquivoEsperado.Conferir("DanfeHomologacao", Texto(NotasDanfe.Autorizada(nota)));
    }

    [Fact]
    public void ACanceladaTemAMarcaDAguaCancelada() =>
        ArquivoEsperado.Conferir("DanfeCancelada",
            Texto(NotasDanfe.Autorizada(NotasDanfe.Simples()), new OpcoesDanfe { Cancelada = true }));

    [Fact]
    public void ACanceladaEmHomologacaoMantemACanceladaEAFraseNasInformacoes()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.Ambiente = Ambiente.Homologacao;
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota), new OpcoesDanfe { Cancelada = true });

        Assert.Equal("CANCELADA", documento.MarcaAgua);
        Assert.Contains("SEM VALOR FISCAL", TextoPlano.Renderizar(documento), StringComparison.Ordinal);
    }

    [Fact]
    public void ComLogotipoERodape() =>
        ArquivoEsperado.Conferir("DanfeLogotipoRodape",
            Texto(NotasDanfe.Autorizada(NotasDanfe.Simples()),
                new OpcoesDanfe { Logotipo = NotasDanfe.Logotipo, TextoRodape = "Impresso por ERP Core" }));

    [Fact]
    public void ComInformacoesAoFiscoEComplementares()
    {
        var nota = NotasDanfe.Simples();
        nota.InformacoesAdicionais = new InformacoesAdicionais
        {
            Fisco = "Valores totais do ICMS Interestadual: DIFAL da UF destino R$216,00 + FCP R$40,00",
            Complementares = "PEDIDO 99 | ENTREGA EM HORARIO COMERCIAL",
        };
        nota.Total.Icms.ValorFcp = 3m;
        nota.Total.Icms.ValorIcmsUFDestino = 216m;
        nota.Total.Icms.ValorFcpUFDestino = 40m;
        nota.Total.Icms.ValorDesonerado = 12.34m;
        nota.Total.Icms.ValorTotalTributos = 50.5m;
        ArquivoEsperado.Conferir("DanfeAdicionais", Texto(NotasDanfe.Autorizada(nota)));
    }

    // A contingência com o formulário de segurança (Anexo II 3.9.2): o código de barras dos Dados da NF-e no lugar do protocolo.
    [Theory]
    [InlineData(TipoEmissao.ContingenciaFSDA, "DanfeContingenciaFsda")]
    [InlineData(TipoEmissao.ContingenciaFSIA, "DanfeContingenciaFsia")]
    public void AContingenciaComFormularioDeSeguranca(TipoEmissao emissao, string golden)
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.TipoEmissao = emissao;
        nota.Identificacao.InicioContingencia = NotasDanfe.Emissao.AddMinutes(-20);
        nota.Identificacao.JustificativaContingencia = "Falha no link com a SEFAZ";
        ArquivoEsperado.Conferir(golden, Texto(NotasDanfe.Lida(nota)));
    }

    [Fact]
    public void AContingenciaEpecSemOProtocoloDoEpec() =>
        ArquivoEsperado.Conferir("DanfeContingenciaEpec", Texto(NotasDanfe.Lida(NotaTipoEmissao(TipoEmissao.ContingenciaEpec))));

    [Fact]
    public void ASvcSaiComoAEmissaoNormal() =>
        ArquivoEsperado.Conferir("DanfeSvc", Texto(NotasDanfe.Autorizada(NotaTipoEmissao(TipoEmissao.ContingenciaSvcAN))));

    [Fact]
    public void AnotaDeFormularioDeSegurancaJaAutorizadaSaiComOProtocolo()
    {
        var texto = Texto(NotasDanfe.Autorizada(NotaTipoEmissao(TipoEmissao.ContingenciaFSDA)));

        Assert.Contains("PROTOCOLO DE AUTORIZAÇÃO DE USO: 131260000012345 14/10/2026 09:30:05", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("DADOS DA NF-e", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("Contingência", texto, StringComparison.Ordinal);
    }

    private static NotaFiscal NotaTipoEmissao(TipoEmissao emissao)
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.TipoEmissao = emissao;
        return nota;
    }
}
