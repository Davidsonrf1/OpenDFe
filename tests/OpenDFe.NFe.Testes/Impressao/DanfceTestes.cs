using OpenDFe.Impressao;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// O DANFCE da lib (etapa 7.2): os golden files de texto, conferidos linha a linha contra o Manual de Padrões Técnicos do
/// DANFE NFC-e e QR Code 6.0 (docs/IMPRESSAO.md 1), e as conferências do modelo que não dependem do texto.
/// </summary>
public sealed class DanfceTestes
{
    private static readonly LeiauteNFe400 Leiaute = new();

    private static string Texto(NotaLida lida, OpcoesDanfce? opcoes = null) => TextoPlano.Renderizar(Leiaute.Danfce(lida, opcoes));

    [Fact]
    public void Exemplo1DoManualNormalComVariosItensESemConsumidor() =>
        ArquivoEsperado.Conferir("Exemplo1", Texto(NotasDanfce.Autorizada(NotasDanfce.Exemplo1())));

    [Fact]
    public void Exemplo2DoManualComEntregaEmDomicilioEDoisPagamentos() =>
        ArquivoEsperado.Conferir("Exemplo2", Texto(NotasDanfce.Autorizada(NotasDanfce.Exemplo2())));

    [Fact]
    public void Exemplo3DoManualContingenciaViaConsumidorComLogotipo() =>
        ArquivoEsperado.Conferir("Exemplo3Consumidor", Texto(NotasDanfce.Lida(NotasDanfce.Exemplo3()),
            new OpcoesDanfce { Logotipo = NotasDanfce.Logotipo, Via = ViaDanfce.Consumidor }));

    [Fact]
    public void Exemplo3DoManualContingenciaViaEstabelecimento() =>
        ArquivoEsperado.Conferir("Exemplo3Estabelecimento", Texto(NotasDanfce.Lida(NotasDanfce.Exemplo3()),
            new OpcoesDanfce { Logotipo = NotasDanfce.Logotipo, Via = ViaDanfce.Estabelecimento }));

    [Fact]
    public void UmItemAVistaEmDinheiroComTroco() =>
        ArquivoEsperado.Conferir("UmItemDinheiroTroco", Texto(NotasDanfce.Autorizada(NotasDanfce.UmItem())));

    [Fact]
    public void CincoItensComDescontoEAcrescimos()
    {
        var nota = NotasDanfce.Base();
        nota.Item("001", "ARROZ TIPO 1 5KG", 2m, "PCT", 24.90m);
        nota.Item("002", "FEIJAO CARIOCA 1KG", 3m, "PCT", 8.49m);
        nota.Item("003", "OLEO DE SOJA 900ML", 1m, "UN", 7.99m);
        nota.Item("004", "ACUCAR REFINADO 1KG", 4m, "PCT", 4.59m);
        nota.Item("005", "CAFE EM PO 500G", 2m, "PCT", 18.75m);
        nota.Totalizar(desconto: 10.00m, frete: 12.00m, seguro: 1.50m, outras: 2.25m);
        nota.Pagar(MeioPagamento.CartaoDebito, 155.0m);

        ArquivoEsperado.Conferir("CincoItensDescontoEAcrescimos", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void CartaoDeCreditoEDinheiroJuntos()
    {
        var nota = NotasDanfce.Base();
        nota.Item("10", "CAMISETA ALGODAO", 3m, "UN", 59.90m);
        nota.Totalizar();
        nota.Pagar(MeioPagamento.CartaoCredito, 100m);
        nota.Pagar(MeioPagamento.Dinheiro, 85m);
        nota.Pagamento.Troco = 5.30m;

        ArquivoEsperado.Conferir("CartaoEDinheiro", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void ConsumidorComCpf()
    {
        var nota = NotasDanfce.UmItem();
        nota.Destinatario = new Destinatario { Cpf = "52998224725", Nome = "José da Silva" };
        ArquivoEsperado.Conferir("ConsumidorCpf", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void ConsumidorComCnpj()
    {
        var nota = NotasDanfce.UmItem();
        nota.Destinatario = new Destinatario { Cnpj = "99999999000199" };
        ArquivoEsperado.Conferir("ConsumidorCnpj", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void ConsumidorEstrangeiro()
    {
        var nota = NotasDanfce.UmItem();
        nota.Destinatario = new Destinatario { IdEstrangeiro = "AB1234567", Nome = "John Smith" };
        ArquivoEsperado.Conferir("ConsumidorEstrangeiro", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void ConsumidorNaoIdentificado() =>
        ArquivoEsperado.Conferir("ConsumidorNaoIdentificado", Texto(NotasDanfce.Autorizada(NotasDanfce.UmItem())));

    [Fact]
    public void ConsumidorSoComNomeSemDocumentoNaoEhNaoIdentificadoSeAEntregaForEmDomicilio()
    {
        var nota = NotasDanfce.UmItem();
        nota.Identificacao.Presenca = PresencaComprador.NaoPresencialEntrega;
        nota.Destinatario = new Destinatario
        {
            Nome = "Maria Souza",
            Endereco = new EnderecoDestinatario { Logradouro = "Rua das Flores", Numero = "10", Complemento = "apto 2", Bairro = "Centro", Municipio = "Contagem", UF = UF.MG },
        };
        ArquivoEsperado.Conferir("EntregaSemDocumento", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void Homologacao()
    {
        var nota = NotasDanfce.UmItem();
        nota.Identificacao.Ambiente = Ambiente.Homologacao;
        ArquivoEsperado.Conferir("Homologacao", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void ContingenciaJaAutorizadaImprimeOProtocoloESemALegenda()
    {
        var nota = NotasDanfce.Exemplo3();
        ArquivoEsperado.Conferir("ContingenciaAutorizada", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void ComEFsemOTotalDeTributos()
    {
        var com = NotasDanfce.UmItem();
        com.Total.Icms.ValorTotalTributos = 12.34m;
        ArquivoEsperado.Conferir("TributosPresente", Texto(NotasDanfce.Autorizada(com)));

        var sem = NotasDanfce.UmItem();
        sem.Total.Icms.ValorTotalTributos = null;
        ArquivoEsperado.Conferir("TributosAusente", Texto(NotasDanfce.Autorizada(sem)));
    }

    [Fact]
    public void DescricaoLongaEInformacoesComplementaresLongas()
    {
        var nota = NotasDanfce.Base();
        nota.Item("7891234567895", "TELEVISOR LED 55 POLEGADAS 4K ULTRA HD SMART COM CONVERSOR DIGITAL INTEGRADO E WIFI", 1m, "UN", 2899.90m);
        nota.Item("PALAVRA-GRANDE-DEMAIS-PARA-CABER-EM-UMA-LINHA-DA-BOBINA-DE-OITENTA", "ITEM", 1m, "UN", 1m);
        nota.Totalizar();
        nota.Pagar(MeioPagamento.Cheque, 2900.90m);
        nota.InformacoesAdicionais = new InformacoesAdicionais
        {
            Fisco = "Documento emitido por ME ou EPP optante pelo Simples Nacional.",
            Complementares = "Troca somente com a nota fiscal em até 7 dias corridos. Produto com garantia de 12 meses contra defeitos de fabricação. "
                + "Atendimento ao cliente: 0800 000 0000, de segunda a sexta, das 8h às 18h.\nVolte sempre!",
        };

        ArquivoEsperado.Conferir("DescricaoEInfCplLongos", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void PapelDe58Mm()
    {
        var lida = NotasDanfce.Lida(NotasDanfce.Exemplo3());
        ArquivoEsperado.Conferir("Bobina58Contingencia", Texto(lida, new OpcoesDanfce { Papel = PapelImpressao.Bobina58 }));
        ArquivoEsperado.Conferir("Bobina58Exemplo1", Texto(NotasDanfce.Autorizada(NotasDanfce.Exemplo1()), new OpcoesDanfce { Papel = PapelImpressao.Bobina58 }));
    }

    [Fact]
    public void ComLogotipoAssinaturaDoSoftwareERodape()
    {
        var opcoes = new OpcoesDanfce { Logotipo = NotasDanfce.Logotipo, AssinaturaSoftware = NotasDanfce.Assinatura, TextoRodape = "ERP Core - DLA Tec" };
        ArquivoEsperado.Conferir("LogotipoAssinaturaERodape", Texto(NotasDanfce.Autorizada(NotasDanfce.UmItem()), opcoes));
    }

    [Fact]
    public void QuantidadeFracionadaEValoresComMilhar()
    {
        var nota = NotasDanfce.Base();
        nota.Item("QF1", "QUEIJO MUSSARELA KG", 1.235m, "KG", 45.90m, 56.69m);
        nota.Item("QF2", "NOTEBOOK AÇAÍ EDIÇÃO LIMITADA", 2m, "UN", 12345.67m);
        nota.Item("QF3", "PARAFUSO", 1000m, "UN", 0.123456m, 123.46m);
        nota.Totalizar();
        nota.Pagar(MeioPagamento.CartaoCredito, 24871.49m);

        ArquivoEsperado.Conferir("QuantidadeFracionadaEMilhar", Texto(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void EmitenteComCpf()
    {
        var nota = NotasDanfce.UmItem();
        (nota.Emitente.Cnpj, nota.Emitente.Cpf) = (null, "52998224725");
        nota.Emitente.Nome = "João da Silva - Produtor Rural";
        ArquivoEsperado.Conferir("EmitenteCpf", Texto(NotasDanfce.Autorizada(nota)));
    }

    // Os testes que olham o modelo, não o texto ------------------------------------------------------------------

    [Fact]
    public void OQrCodeDoDocumentoEOMesmoDaNotaByteAByte()
    {
        var nota = NotasDanfce.UmItem();
        nota.Suplementares!.QrCode = "https://exemplo.gov.br/qr?p=ÇÃO|3|2 com espaço e acento";

        var documento = Leiaute.Danfce(NotasDanfce.Autorizada(nota));

        var qr = Assert.Single(documento.Elementos.OfType<QrCodeImpressao>());
        Assert.Equal(nota.Suplementares.QrCode, qr.Texto);
        Assert.Equal(AlinhamentoImpressao.Centro, qr.Alinhamento);
        // O manual: ao menos 25 mm no total (22 de conteúdo e 3 de margem).
        Assert.True(qr.LadoMm >= 25);
    }

    [Fact]
    public void AUrlDaConsultaEAChaveSaoAsDaNota()
    {
        var lida = NotasDanfce.Autorizada(NotasDanfce.UmItem());

        var documento = Leiaute.Danfce(lida);

        var textos = documento.Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
        Assert.Contains(NotasDanfce.UrlConsulta, textos);
        var chave = Assert.Single(textos, t => t.Replace(" ", string.Empty, StringComparison.Ordinal) == lida.Chave!.ToString());
        Assert.Equal(44, chave.Replace(" ", string.Empty, StringComparison.Ordinal).Length);
        Assert.Equal(11, chave.Split(' ').Length);
        Assert.All(chave.Split(' '), bloco => Assert.Equal(4, bloco.Length));
        Assert.True(chave.Replace(" ", string.Empty, StringComparison.Ordinal).All(char.IsAsciiDigit));
    }

    [Fact]
    public void OsTotaisBatemComOVNFEComASomaDosItensExibidos()
    {
        var nota = NotasDanfce.Exemplo2();
        var documento = Leiaute.Danfce(NotasDanfce.Autorizada(nota));

        var linhas = documento.Elementos.OfType<LinhaImpressao>().ToArray();
        string Valor(string rotulo) => Assert.Single(linhas, l => l.Celulas[0].Texto == rotulo).Celulas[1].Texto;

        Assert.Equal("2", Valor("Qtde. total de itens"));
        Assert.Equal("2.500,00", Valor("Valor total R$"));
        Assert.Equal("500,00", Valor("Desconto R$"));
        Assert.Equal("50,00", Valor("Frete R$"));
        Assert.Equal("2.050,00", Valor("Valor a Pagar R$"));
        Assert.Equal(nota.Total.Icms.ValorNota, 2050m);

        // A soma dos totais dos itens exibidos é o valor total dos produtos.
        var totaisItens = linhas.Where(l => l.Celulas.Count == 2 && l.Celulas[0].Texto.Contains(" x ", StringComparison.Ordinal) && l.Celulas[1].Texto != "Vl Total")
            .Select(l => decimal.Parse(l.Celulas[1].Texto.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(nota.Total.Icms.ValorProdutos, totaisItens.Sum());
        Assert.Equal(nota.Itens.Count, totaisItens.Count());
    }

    [Fact]
    public void OValorAPagarSoSaiComAcrescimoOuDesconto()
    {
        var documento = Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.UmItem()));

        var rotulos = documento.Elementos.OfType<LinhaImpressao>().Select(l => l.Celulas[0].Texto).ToArray();

        Assert.Contains("Valor total R$", rotulos);
        Assert.DoesNotContain("Valor a Pagar R$", rotulos);
        Assert.DoesNotContain("Desconto R$", rotulos);
        Assert.DoesNotContain("Frete R$", rotulos);
    }

    [Fact]
    public void AsSecoesVemNaOrdemDoManual()
    {
        var documento = Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.Exemplo2()), new OpcoesDanfce { Logotipo = NotasDanfce.Logotipo });
        var elementos = documento.Elementos;

        int Indice(Func<ElementoImpressao, bool> condicao) => Assert.Single(elementos.Select((e, i) => (e, i)).Where(p => condicao(p.e)).Select(p => p.i).Take(1));
        bool Texto(ElementoImpressao e, string trecho) => e is TextoImpressao t && t.Texto.Contains(trecho, StringComparison.Ordinal);
        bool Linha(ElementoImpressao e, string rotulo) => e is LinhaImpressao l && l.Celulas[0].Texto == rotulo;

        var ordem = new[]
        {
            Indice(e => e is ImagemImpressao),
            Indice(e => Texto(e, "CNPJ: 11.222.333/0001-81 Razão Social da Empresa")),
            Indice(e => Texto(e, "Documento Auxiliar da Nota Fiscal de Consumidor Eletrônica")),
            Indice(e => Texto(e, "Código Descrição")),
            Indice(e => Linha(e, "Qtde. total de itens")),
            Indice(e => Linha(e, "FORMA PAGAMENTO")),
            Indice(e => Texto(e, "Consulte pela Chave de Acesso em")),
            Indice(e => Texto(e, "CONSUMIDOR CPF:")),
            Indice(e => Texto(e, "NFC-e nº 000000001 Série 001 10/03/2015 15:03:53")),
            Indice(e => Texto(e, "Protocolo de autorização: 131260000000019")),
            Indice(e => Texto(e, "Data de autorização 10/03/2015 15:03:58")),
            Indice(e => e is QrCodeImpressao),
            Indice(e => Texto(e, "Tributos Totais Incidentes (Lei Federal 12.741/2012): R$200,00")),
        };

        Assert.Equal(ordem.Order(), ordem);
    }

    [Fact]
    public void AHoraDaEmissaoENaoConvertidaDeFuso()
    {
        // A nota traz a hora do emitente (-03:00); o DANFCE a imprime como veio, não em UTC nem no fuso do computador.
        var nota = NotasDanfce.UmItem();
        nota.Identificacao.Emissao = new DateTimeOffset(2026, 10, 24, 23, 30, 0, TimeSpan.FromHours(-3));
        var protocolo = NotasDanfce.ProtocoloAutorizado();
        protocolo.Recebimento = new DateTimeOffset(2026, 10, 24, 23, 30, 12, TimeSpan.FromHours(-3));

        var textos = Leiaute.Danfce(NotasDanfce.Lida(nota, protocolo)).Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();

        Assert.Contains(textos, t => t.EndsWith("24/10/2026 23:30:00", StringComparison.Ordinal));
        Assert.Contains("Data de autorização 24/10/2026 23:30:12", textos);
        Assert.DoesNotContain(textos, t => t.Contains("25/10/2026", StringComparison.Ordinal));
    }

    [Fact]
    public void ALegendaDeContingenciaSaiNosDoisLocaisESemProtocolo()
    {
        var documento = Leiaute.Danfce(NotasDanfce.Lida(NotasDanfce.Exemplo3()));
        var elementos = documento.Elementos;

        var legendas = elementos.Select((e, i) => (e, i)).Where(p => p.e is TextoImpressao { Texto: "EMITIDA EM CONTINGÊNCIA" }).Select(p => p.i).ToArray();
        var pendentes = elementos.Select((e, i) => (e, i)).Where(p => p.e is TextoImpressao { Texto: "Pendente de autorização" }).Select(p => p.i).ToArray();
        var identificacao = elementos.ToList().FindIndex(e => e is TextoImpressao t && t.Texto.StartsWith("NFC-e nº", StringComparison.Ordinal));
        var cabecalho = elementos.ToList().FindIndex(e => e is TextoImpressao t && t.Texto.StartsWith("Documento Auxiliar", StringComparison.Ordinal));

        Assert.Equal(2, legendas.Length);
        Assert.Equal(legendas.Select(i => i + 1), pendentes);
        Assert.True(legendas[0] > cabecalho && legendas[0] < identificacao, "a primeira, abaixo do cabeçalho");
        Assert.True(legendas[1] > identificacao, "a segunda, abaixo da identificação da NFC-e");
        Assert.DoesNotContain(elementos.OfType<TextoImpressao>(), t => t.Texto.StartsWith("Protocolo de autorização", StringComparison.Ordinal));
        Assert.All(elementos.OfType<TextoImpressao>().Where(t => t.Texto == "EMITIDA EM CONTINGÊNCIA"),
            t => Assert.True(t.Negrito && t.Alinhamento == AlinhamentoImpressao.Centro));
    }

    [Fact]
    public void ComOProtocoloANotaDeContingenciaNaoTemLegenda()
    {
        var textos = Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.Exemplo3())).Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();

        Assert.DoesNotContain(textos, t => t.Contains("CONTINGÊNCIA", StringComparison.Ordinal) || t.Contains("Pendente", StringComparison.Ordinal));
        Assert.Contains(textos, t => t.StartsWith("Protocolo de autorização:", StringComparison.Ordinal));
    }

    [Fact]
    public void AViaDoEstabelecimentoVemJuntoDaIdentificacao()
    {
        var consumidor = Leiaute.Danfce(NotasDanfce.Lida(NotasDanfce.Exemplo3()), new OpcoesDanfce { Via = ViaDanfce.Consumidor });
        var estabelecimento = Leiaute.Danfce(NotasDanfce.Lida(NotasDanfce.Exemplo3()), new OpcoesDanfce { Via = ViaDanfce.Estabelecimento });
        var normal = Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.UmItem()));
        var normalEstabelecimento = Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.UmItem()), new OpcoesDanfce { Via = ViaDanfce.Estabelecimento });

        static bool Tem(DocumentoImpressao d, string texto) => d.Elementos.OfType<TextoImpressao>().Any(t => t.Texto == texto);

        Assert.True(Tem(consumidor, "Via do Consumidor"));
        Assert.False(Tem(consumidor, "Via do Estabelecimento"));
        Assert.True(Tem(estabelecimento, "Via do Estabelecimento"));
        Assert.False(Tem(estabelecimento, "Via do Consumidor"));
        // A emissão normal não imprime a via (a figura 1 do manual não a traz); a lib não decide quantas vias.
        Assert.False(Tem(normal, "Via do Consumidor") || Tem(normal, "Via do Estabelecimento"));
        Assert.True(Tem(normalEstabelecimento, "Via do Estabelecimento"));
    }

    [Fact]
    public void OTextoDeHomologacaoSoSaiNoAmbienteDeHomologacao()
    {
        const string Texto = "EMITIDA EM AMBIENTE DE HOMOLOGAÇÃO - SEM VALOR FISCAL";
        var producao = Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.UmItem()));
        var nota = NotasDanfce.UmItem();
        nota.Identificacao.Ambiente = Ambiente.Homologacao;
        var homologacao = Leiaute.Danfce(NotasDanfce.Autorizada(nota));

        Assert.DoesNotContain(producao.Elementos.OfType<TextoImpressao>(), t => t.Texto == Texto);
        var texto = Assert.Single(homologacao.Elementos.OfType<TextoImpressao>(), t => t.Texto == Texto);
        Assert.Equal(AlinhamentoImpressao.Centro, texto.Alinhamento);
        Assert.Equal(texto.Texto.ToUpperInvariant(), texto.Texto);
    }

    [Fact]
    public void ODocumentoEPorBobinaESeParaTituloComONumeroESerie()
    {
        var documento = Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.UmItem()));

        Assert.Same(PapelImpressao.Bobina80, documento.Papel);
        Assert.Equal("DANFE NFC-e 000000001 série 001", documento.Titulo);
        Assert.Same(PapelImpressao.Bobina58, Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.UmItem()), new OpcoesDanfce { Papel = PapelImpressao.Bobina58 }).Papel);
    }

    [Fact]
    public void NadaVemDeForaDaNotaOuDasOpcoes()
    {
        // Sem o texto do rodapé, sem o infCpl nem o infAdFisco, sem tributos e em produção: nenhum texto livre.
        var nota = NotasDanfce.UmItem();
        var textos = Leiaute.Danfce(NotasDanfce.Autorizada(nota)).Elementos.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();

        Assert.DoesNotContain(textos, t => t.Contains("Tributos", StringComparison.Ordinal));
        Assert.DoesNotContain(textos, t => t.Contains("HOMOLOGAÇÃO", StringComparison.Ordinal));
        Assert.DoesNotContain(textos, t => t.Contains("ERP", StringComparison.Ordinal));
    }

    // As recusas ----------------------------------------------------------------------------------------------------

    [Fact]
    public void RecusaANotaNula() => Assert.Throws<ArgumentNullException>(() => Leiaute.Danfce(null!));

    [Fact]
    public void RecusaANFeDeModelo55()
    {
        var nota = NotasDanfce.UmItem();
        nota.Identificacao.Modelo = Documento.NFe;
        var lida = new NotaLida(nota, ChaveAcesso.Montar(UF.MG, NotasDanfce.Emissao, NotasDanfce.Cnpj, Documento.NFe, 1, 1, TipoEmissao.Normal, 12345678),
            NotasDanfce.ProtocoloAutorizado(), string.Empty, []);

        var erro = Assert.Throws<ArgumentException>(() => Leiaute.Danfce(lida));

        Assert.Contains("modelo 65", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RecusaANotaSemChave()
    {
        var lida = new NotaLida(NotasDanfce.UmItem(), null, NotasDanfce.ProtocoloAutorizado(), string.Empty, []);
        Assert.Contains("chave", Assert.Throws<ArgumentException>(() => Leiaute.Danfce(lida)).Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RecusaALeituraSemANota() =>
        Assert.Throws<ArgumentException>(() => Leiaute.Danfce(new NotaLida(null, null, null, string.Empty, [])));

    [Theory]
    [InlineData(null, "urlChave")]
    [InlineData("", "urlChave")]
    public void RecusaANotaSemOQrCode(string? qr, string urlChave)
    {
        var nota = NotasDanfce.UmItem();
        nota.Suplementares = new InformacoesSuplementares { QrCode = qr, UrlChave = urlChave };
        Assert.Contains("QR Code", Assert.Throws<ArgumentException>(() => Leiaute.Danfce(NotasDanfce.Autorizada(nota))).Message, StringComparison.Ordinal);

        nota.Suplementares = null;
        Assert.Throws<ArgumentException>(() => Leiaute.Danfce(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void RecusaANotaSemAUrlDeConsulta()
    {
        var nota = NotasDanfce.UmItem();
        nota.Suplementares!.UrlChave = " ";
        Assert.Throws<ArgumentException>(() => Leiaute.Danfce(NotasDanfce.Autorizada(nota)));
    }

    [Fact]
    public void RecusaOPapelQueNaoEBobina()
    {
        var lida = NotasDanfce.Autorizada(NotasDanfce.UmItem());
        Assert.Throws<ArgumentException>(() => Leiaute.Danfce(lida, new OpcoesDanfce { Papel = PapelImpressao.A4Retrato }));
        Assert.Throws<ArgumentException>(() => Leiaute.Danfce(lida, new OpcoesDanfce { Papel = PapelImpressao.A4Paisagem }));
        Assert.Throws<ArgumentException>(() => Leiaute.Danfce(lida, new OpcoesDanfce { Papel = null! }));
    }

    [Fact]
    public void RecusaOProtocoloQueNaoEDeAutorizacao()
    {
        var protocolo = NotasDanfce.ProtocoloAutorizado();
        protocolo.Status = 101;
        Assert.Contains("101", Assert.Throws<ArgumentException>(() => Leiaute.Danfce(NotasDanfce.Lida(NotasDanfce.UmItem(), protocolo))).Message, StringComparison.Ordinal);

        protocolo.Status = 150;
        _ = Leiaute.Danfce(NotasDanfce.Lida(NotasDanfce.UmItem(), protocolo));
    }

    [Fact]
    public void AViaForaDoEnumEhRecusada() =>
        Assert.Throws<ArgumentException>(() => Leiaute.Danfce(NotasDanfce.Autorizada(NotasDanfce.UmItem()), new OpcoesDanfce { Via = (ViaDanfce)7 }));

    [Fact]
    public void AMensagemDeRecusaNaoTrazOsDadosDaNota()
    {
        var nota = NotasDanfce.UmItem();
        nota.Destinatario = new Destinatario { Cpf = "52998224725", Nome = "Fulano de Tal" };
        nota.Suplementares = null;

        var erro = Assert.Throws<ArgumentException>(() => Leiaute.Danfce(NotasDanfce.Autorizada(nota)));

        Assert.DoesNotContain("52998224725", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Fulano", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ODanfceNaoMudaANota()
    {
        var nota = NotasDanfce.Exemplo2();
        var antes = NotasDanfce.Lida(nota);

        _ = Leiaute.Danfce(antes);

        Assert.Equal(2, nota.Itens.Count);
        Assert.Equal(2050m, nota.Total.Icms.ValorNota);
        Assert.Equal(NotasDanfce.QrCodeFixo, nota.Suplementares!.QrCode);
    }

    [Fact]
    public async Task SaiIgualEmParalelo()
    {
        var lida = NotasDanfce.Autorizada(NotasDanfce.Exemplo1());
        var esperado = Texto(lida);

        var resultados = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => Texto(lida), TestContext.Current.CancellationToken)));

        Assert.All(resultados, r => Assert.Equal(esperado, r));
    }
}
