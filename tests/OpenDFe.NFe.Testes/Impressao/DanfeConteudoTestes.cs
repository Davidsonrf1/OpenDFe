using System.Globalization;
using OpenDFe.Impressao;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// O DANFE da NF-e conferido sem o golden (etapa 7.3): o conteúdo que independe do texto (a chave do código de barras, os
/// totais, a soma dos itens, o tipo de operação, a ordem dos quadros do Anexo II, o cabeçalho de continuação) e as recusas.
/// </summary>
public sealed class DanfeConteudoTestes
{
    private static readonly LeiauteNFe400 Leiaute = new();

    private static IEnumerable<CampoImpressao> Campos(DocumentoImpressao documento) =>
        documento.Elementos.OfType<SecaoImpressao>().SelectMany(s => s.Linhas).SelectMany(l => l.Campos);

    private static string Valor(DocumentoImpressao documento, string rotulo) => Campos(documento).First(c => c.Rotulo == rotulo).Valor;

    private static decimal Numero(string texto) =>
        decimal.Parse(texto.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.'), CultureInfo.InvariantCulture);

    // Uma NF-e com todos os quadros opcionais: retirada, entrega, cobrança, a reforma e o ISSQN.
    private static NotaFiscal Completa()
    {
        var nota = NotasTeste.NFe();
        nota.Identificacao.Emissao = NotasDanfe.Emissao;
        nota.Identificacao.SaidaEntrada = NotasDanfe.Emissao;
        nota.Identificacao.Modelo = Documento.NFe;
        nota.Identificacao.Ambiente = Ambiente.Producao;
        nota.Identificacao.TipoEmissao = TipoEmissao.Normal;
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeRetrato;
        nota.Suplementares = new InformacoesSuplementares { UrlChave = NotasDanfe.UrlConsulta };
        return nota;
    }

    [Fact]
    public void ACodigoDeBarrasTemAChaveDaNotaCom44Digitos()
    {
        var lida = NotasDanfe.Autorizada(NotasDanfe.Simples());
        var documento = Leiaute.Danfe(lida);

        var barras = documento.Elementos.OfType<CodigoBarrasImpressao>().First();
        Assert.Equal(lida.Chave!.ToString(), barras.Digitos);
        Assert.Equal(44, barras.Digitos.Length);
        Assert.All(barras.Digitos, c => Assert.True(char.IsAsciiDigit(c)));
        Assert.Equal(lida.Chave.ToString(), Valor(documento, "CHAVE DE ACESSO").Replace(" ", string.Empty, StringComparison.Ordinal));
        Assert.Equal(11, Valor(documento, "CHAVE DE ACESSO").Split(' ').Length);
        Assert.All(Valor(documento, "CHAVE DE ACESSO").Split(' '), bloco => Assert.Equal(4, bloco.Length));
    }

    [Fact]
    public void OProtocoloEADataSaoOsDoRetorno()
    {
        var protocolo = NotasDanfe.ProtocoloAutorizado("891234567890123");
        var documento = Leiaute.Danfe(NotasDanfe.Lida(NotasDanfe.Simples(), protocolo));

        Assert.Equal("891234567890123 14/10/2026 09:30:05", Valor(documento, "PROTOCOLO DE AUTORIZAÇÃO DE USO"));
    }

    [Fact]
    public void ADataNaoEConvertidaDeFuso()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.Emissao = new DateTimeOffset(2026, 10, 14, 23, 59, 59, TimeSpan.FromHours(9));
        nota.Identificacao.SaidaEntrada = new DateTimeOffset(2026, 10, 15, 0, 0, 1, TimeSpan.Zero);
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));

        Assert.Equal("14/10/2026", Valor(documento, "DATA DA EMISSÃO"));
        Assert.Equal("15/10/2026", Valor(documento, "DATA DA ENTRADA / SAÍDA"));
        Assert.Equal("00:00:01", Valor(documento, "HORA DA ENTRADA / SAÍDA"));
    }

    [Fact]
    public void OsTotaisExibidosSaoOsDoTotalEASomaDosItensBate()
    {
        var nota = NotasDanfe.Base();
        nota.ItemDanfe("A", "Produto A", 3m, "UN", 10.10m);
        nota.ItemDanfe("B", "Produto B", 2.5m, "KG", 7.33m, ipi: NotasDanfe.IpiTributado(18.33m, 5m));
        nota.ItemDanfe("C", "Produto C", 1m, "UN", 99.99m);
        nota.TotalizarDanfe(desconto: 3.5m, frete: 12m);
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));
        var total = nota.Total.Icms;

        Assert.Equal(total.ValorProdutos, Numero(Valor(documento, "VALOR TOTAL DOS PRODUTOS")));
        Assert.Equal(total.ValorNota, Numero(Valor(documento, "VALOR TOTAL DA NOTA")));
        Assert.Equal(total.Valor, Numero(Valor(documento, "VALOR DO ICMS")));
        Assert.Equal(total.BaseCalculo, Numero(Valor(documento, "BASE DE CÁLCULO DO ICMS")));
        Assert.Equal(total.ValorIpi, Numero(Valor(documento, "VALOR DO IPI")));
        Assert.Equal(total.Frete, Numero(Valor(documento, "VALOR DO FRETE")));
        Assert.Equal(total.Desconto, Numero(Valor(documento, "DESCONTO")));

        var tabela = documento.Elementos.OfType<TabelaImpressao>().Single();
        var coluna = tabela.Colunas.ToList().FindIndex(c => c.Titulo == "VALOR TOTAL");
        Assert.Equal(Numero(Valor(documento, "VALOR TOTAL DOS PRODUTOS")), tabela.Linhas.Sum(l => Numero(l[coluna])));
    }

    [Theory]
    [InlineData(TipoNota.Entrada, "0")]
    [InlineData(TipoNota.Saida, "1")]
    public void OTipoDeOperacaoTemOTextoDoManualEODigitoDoTpNF(TipoNota tipo, string digito)
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.Tipo = tipo;
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));

        var campo = Campos(documento).Single(c => c.Rotulo == "0 - ENTRADA / 1 - SAÍDA");
        Assert.Equal(digito, campo.Valor);
    }

    [Fact]
    public void OsQuadrosSaemNaOrdemDoAnexoII()
    {
        var lida = NotasDanfe.Autorizada(Completa());
        var documento = Leiaute.Danfe(lida);

        var titulos = documento.Elementos
            .Select(e => e switch { SecaoImpressao s => s.Titulo, TabelaImpressao t => t.Titulo, _ => null })
            .Where(t => t is not null)
            .ToArray();

        Assert.Equal(
        [
            "DESTINATÁRIO / REMETENTE",
            "INFORMAÇÕES DO LOCAL DE RETIRADA",
            "INFORMAÇÕES DO LOCAL DE ENTREGA",
            "FATURA / DUPLICATAS",
            "CÁLCULO DO IMPOSTO",
            "TOTAL DO IBS / CBS / IS",
            "TRANSPORTADOR / VOLUMES TRANSPORTADOS",
            "DADOS DOS PRODUTOS / SERVIÇOS",
            "CÁLCULO DO ISSQN",
            "DADOS ADICIONAIS",
        ], titulos);

        // O que vem antes: o canhoto, depois a identificação (emitente, DANFE, código de barras, chave, natureza, IE e CNPJ).
        var antes = documento.Elementos.TakeWhile(e => e is not SecaoImpressao { Titulo: "DESTINATÁRIO / REMETENTE" }).ToArray();
        Assert.IsType<TextoImpressao>(antes[0]);
        Assert.StartsWith("RECEBEMOS DE", ((TextoImpressao)antes[0]).Texto, StringComparison.Ordinal);
        var rotulos = antes.OfType<SecaoImpressao>().SelectMany(s => s.Linhas).SelectMany(l => l.Campos).Select(c => c.Rotulo).ToArray();
        Assert.Equal(
        [
            "DATA DE RECEBIMENTO", "IDENTIFICAÇÃO E ASSINATURA DO RECEBEDOR", "NF-e",
            "0 - ENTRADA / 1 - SAÍDA", "Nº", "SÉRIE", "FOLHA", "CHAVE DE ACESSO", "PROTOCOLO DE AUTORIZAÇÃO DE USO", "NATUREZA DA OPERAÇÃO",
            "INSCRIÇÃO ESTADUAL", "INSCRIÇÃO ESTADUAL DO SUBST. TRIBUTÁRIO", "CNPJ / CPF", "CÓDIGO DO REGIME TRIBUTÁRIO",
        ], rotulos);
    }

    [Fact]
    public void OsQuadrosOpcionaisSomemQuandoANotaNaoOsTem()
    {
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(NotasDanfe.Simples()));

        var titulos = documento.Elementos.OfType<SecaoImpressao>().Select(s => s.Titulo).Where(t => t is not null).ToArray();
        Assert.Equal(["DESTINATÁRIO / REMETENTE", "CÁLCULO DO IMPOSTO", "TRANSPORTADOR / VOLUMES TRANSPORTADOS", "DADOS ADICIONAIS"], titulos);
    }

    [Fact]
    public void OCabecalhoDeContinuacaoTemOQueOAnexoIIManda()
    {
        var lida = NotasDanfe.Autorizada(NotasDanfe.Simples());
        var documento = Leiaute.Danfe(lida, new OpcoesDanfe { Logotipo = NotasDanfe.Logotipo });
        var cabecalho = documento.CabecalhoContinuacao;

        // Anexo II 3.5: o emitente, "DANFE" e "Documento Auxiliar", número, série, tipo de operação e folha, o código de barras,
        // a natureza da operação e a chave, a IE, a IE do substituto e o CNPJ.
        var textos = cabecalho.OfType<TextoImpressao>().Select(t => t.Texto).ToArray();
        Assert.Contains("Distribuidora Fornecedor Ltda", textos);
        Assert.Contains("DANFE", textos);
        Assert.Contains("DOCUMENTO AUXILIAR DA NOTA FISCAL ELETRÔNICA", textos);
        Assert.Equal(lida.Chave!.ToString(), cabecalho.OfType<CodigoBarrasImpressao>().Single().Digitos);
        Assert.Single(cabecalho.OfType<ImagemImpressao>());
        var campos = cabecalho.OfType<SecaoImpressao>().SelectMany(s => s.Linhas).SelectMany(l => l.Campos).ToArray();
        foreach (var rotulo in new[] { "Nº", "SÉRIE", "FOLHA", "0 - ENTRADA / 1 - SAÍDA", "CHAVE DE ACESSO", "NATUREZA DA OPERAÇÃO", "INSCRIÇÃO ESTADUAL",
                     "INSCRIÇÃO ESTADUAL DO SUBST. TRIBUTÁRIO", "CNPJ / CPF" })
            Assert.Contains(campos, c => c.Rotulo == rotulo);

        // A folha leva as fichas, que o renderizador troca página a página.
        var folha = campos.Single(c => c.Rotulo == "FOLHA").Valor;
        Assert.Equal("{pagina}/{paginas}", folha);
        Assert.Equal("2/3", TextoPaginado.Resolver(folha, 2, 3));
        Assert.Equal("1/1", TextoPaginado.Resolver(Valor(documento, "FOLHA"), 1, 1));
    }

    [Fact]
    public void ANotaEmHomologacaoLevaAMarcaSemValorFiscalEACanceladaLevaCancelada()
    {
        var nota = NotasDanfe.Simples();
        Assert.Null(Leiaute.Danfe(NotasDanfe.Autorizada(nota)).MarcaAgua);

        nota.Identificacao.Ambiente = Ambiente.Homologacao;
        Assert.Equal("SEM VALOR FISCAL", Leiaute.Danfe(NotasDanfe.Autorizada(nota)).MarcaAgua);
        Assert.Equal("CANCELADA", Leiaute.Danfe(NotasDanfe.Autorizada(nota), new OpcoesDanfe { Cancelada = true }).MarcaAgua);
        Assert.Contains("SEM VALOR FISCAL", Valor(Leiaute.Danfe(NotasDanfe.Autorizada(nota)), "INFORMAÇÕES COMPLEMENTARES"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(OrigemMercadoria.Nacional, CstIcms.Tributada, "000")]
    [InlineData(OrigemMercadoria.EstrangeiraImportacaoDireta, CstIcms.TributadaSubstituicao, "110")]
    [InlineData(OrigemMercadoria.EstrangeiraMercadoInterno, CstIcms.Suspensao, "250")]
    [InlineData(OrigemMercadoria.NacionalConteudoImportacaoAlto, CstIcms.Outras, "890")]
    public void ACstDaColunaTemAOrigemNaFrente(OrigemMercadoria origem, CstIcms cst, string esperado)
    {
        var nota = NotasDanfe.Base();
        nota.ItemDanfe("A", "Produto", 1m, "UN", 10m, new Icms90 { Origem = origem, Cst = cst });
        nota.TotalizarDanfe();
        var tabela = Leiaute.Danfe(NotasDanfe.Autorizada(nota)).Elementos.OfType<TabelaImpressao>().Single();

        Assert.Equal(esperado, tabela.Linhas[0][tabela.Colunas.ToList().FindIndex(c => c.Titulo == "O/CST")]);
    }

    [Theory]
    [InlineData(CsosnIcms.PermiteCredito, "0101")]
    [InlineData(CsosnIcms.NaoPermiteCredito, "0102")]
    [InlineData(CsosnIcms.Outros, "0900")]
    public void ACsosnDaColunaTemAOrigemNaFrente(CsosnIcms csosn, string esperado)
    {
        var nota = NotasDanfe.Base();
        nota.ItemDanfe("A", "Produto", 1m, "UN", 10m, new IcmsSn102 { Origem = OrigemMercadoria.Nacional, Csosn = csosn });
        nota.TotalizarDanfe();
        var tabela = Leiaute.Danfe(NotasDanfe.Autorizada(nota)).Elementos.OfType<TabelaImpressao>().Single();

        Assert.Equal(esperado, tabela.Linhas[0][tabela.Colunas.ToList().FindIndex(c => c.Titulo == "O/CST")]);
    }

    [Fact]
    public void AsColunasDeValorUsamAsCasasDoAnexoII()
    {
        var nota = NotasDanfe.Base();
        nota.ItemDanfe("A", "Produto", 1234.5m, "KG", 0.123456m, ncm: "09012100");
        nota.TotalizarDanfe();
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));
        var tabela = documento.Elementos.OfType<TabelaImpressao>().Single();
        string Celula(string titulo) => tabela.Linhas[0][tabela.Colunas.ToList().FindIndex(c => c.Titulo == titulo)];

        Assert.Equal("1.234,50", Celula("QUANT."));
        Assert.Equal("0,123456", Celula("VALOR UNITÁRIO"));
        Assert.Equal("152,41", Celula("VALOR TOTAL"));
        Assert.Equal("18,00", Celula("ALÍQ. ICMS"));
        Assert.Equal("5102", Celula("CFOP"));
        Assert.Equal("09012100", Celula("NCM/SH"));
    }

    [Fact]
    public void OFormatoNaoDependeDaCulturaDaThread()
    {
        var anterior = CultureInfo.CurrentCulture;
        var lida = NotasDanfe.Autorizada(NotasDanfe.Simples());
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var ingles = TextoPlano.Renderizar(Leiaute.Danfe(lida));
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var arabe = TextoPlano.Renderizar(Leiaute.Danfe(lida));
            Assert.Equal(ingles, arabe);
        }
        finally
        {
            CultureInfo.CurrentCulture = anterior;
        }
    }

    [Fact]
    public void OsDadosDaNotaDoFormularioDeSegurancaSaoOsDoAnexoII()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaFSIA;
        nota.Total.Icms.ValorSt = 0m;
        var documento = Leiaute.Danfe(NotasDanfe.Lida(nota));

        var dados = documento.Elementos.OfType<CodigoBarrasImpressao>().Last().Digitos;
        // cUF 31 (MG), tpEmis 2, o CNPJ do destinatário, vNF 150,00, ICMS próprio 1, ST 2, dia 14.
        Assert.Equal(36, dados.Length);
        Assert.Equal("31" + "2" + NotasDanfe.CnpjDestinatario + "00000000015000" + "1" + "2" + "14", dados[..35]);
        Assert.Equal(DigitoModulo11(dados[..35]), dados[35] - '0');
        Assert.Equal(dados.Chunk(4).Select(c => new string(c)), Valor(documento, "DADOS DA NF-e").Split(' '));
    }

    [Fact]
    public void OsDadosDaNotaComDestinatarioNoExteriorUsamNoventaENoveEZeros()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaFSDA;
        nota.Destinatario = new Destinatario { IdEstrangeiro = "AR1", Nome = "Cliente", Endereco = new EnderecoDestinatario { Exterior = true, Municipio = "Buenos Aires" } };
        var documento = Leiaute.Danfe(NotasDanfe.Lida(nota));

        var dados = documento.Elementos.OfType<CodigoBarrasImpressao>().Last().Digitos;
        Assert.Equal("99" + "5" + "00000000000000" + "00000000015000" + "1" + "2" + "14", dados[..35]);
        Assert.Equal(DigitoModulo11(dados[..35]), dados[35] - '0');
    }

    [Fact]
    public void OsDadosDaNotaComCpfDoDestinatarioTemOCpfComZerosAEsquerda()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaFSDA;
        nota.Destinatario = new Destinatario { Cpf = "52998224725", Nome = "Fulano", Endereco = new EnderecoDestinatario { UF = UF.SP } };
        var documento = Leiaute.Danfe(NotasDanfe.Lida(nota));

        var dados = documento.Elementos.OfType<CodigoBarrasImpressao>().Last().Digitos;
        Assert.Equal("35" + "5" + "00052998224725", dados[..17]);
    }

    // O módulo 11 do MOC 5.4, escrito de outro jeito: os pesos de 2 a 9 da esquerda para a direita, na ordem invertida.
    private static int DigitoModulo11(string digitos)
    {
        var soma = digitos.Reverse().Select((c, i) => (c - '0') * (2 + i % 8)).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    [Fact]
    public void QuinhentosItensComDescricoesLongasSaemInteirosNoModelo()
    {
        var nota = NotasDanfe.Base();
        var longa = string.Join(' ', Enumerable.Repeat("descrição muito comprida do produto", 12));
        for (var i = 1; i <= 500; i++)
            nota.ItemDanfe($"P{i:0000}", $"{longa} {i}", 1m, "UN", 1m, adicional: $"Informação adicional do item {i}\nsegunda linha");
        nota.TotalizarDanfe();
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));

        var tabela = documento.Elementos.OfType<TabelaImpressao>().Single();
        Assert.Equal(500, tabela.Linhas.Count);
        Assert.Equal(500, tabela.Linhas.Select(l => l[0]).Distinct().Count());
        Assert.StartsWith(longa + " 500\n", tabela.Linhas[499][1], StringComparison.Ordinal);
        Assert.EndsWith("Informação adicional do item 500\nsegunda linha", tabela.Linhas[499][1], StringComparison.Ordinal);
        Assert.Equal(500m, Numero(Valor(documento, "VALOR TOTAL DOS PRODUTOS")));
    }

    [Fact]
    public void ODanfeNaoAlteraANotaEEDeterministico()
    {
        var lida = NotasDanfe.Autorizada(NotasDanfe.Simples());
        var antes = System.Text.Json.JsonSerializer.Serialize(lida.Nota);

        var primeiro = TextoPlano.Renderizar(Leiaute.Danfe(lida));
        var segundo = TextoPlano.Renderizar(Leiaute.Danfe(lida));

        Assert.Equal(primeiro, segundo);
        Assert.Equal(antes, System.Text.Json.JsonSerializer.Serialize(lida.Nota));
    }

    [Fact]
    public async Task SeguroEmParalelo()
    {
        var lida = NotasDanfe.Autorizada(NotasDanfe.Simples());
        var esperado = TextoPlano.Renderizar(Leiaute.Danfe(lida));

        var resultados = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => TextoPlano.Renderizar(Leiaute.Danfe(lida)), TestContext.Current.CancellationToken)));

        Assert.All(resultados, r => Assert.Equal(esperado, r));
    }

    [Fact]
    public void ATituloDoDocumentoTemONumeroEASerie() =>
        Assert.Equal("DANFE NF-e 000000123 série 001", Leiaute.Danfe(NotasDanfe.Autorizada(NotasDanfe.Simples())).Titulo);

    // ---- as recusas ----

    [Fact]
    public void RecusaANotaNulaEALeituraSemNotaOuSemChave()
    {
        Assert.Throws<ArgumentNullException>(() => Leiaute.Danfe(null!));
        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(new NotaLida(null, null, null, string.Empty, [])));
        var lidaIncompleta = new NotaLida(NotasDanfe.Simples(), null, null, string.Empty, []);
        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(lidaIncompleta));
    }

    [Fact]
    public void RecusaANFCe()
    {
        var nota = NotasDanfce.Base();
        var lida = NotasDanfce.Autorizada(nota);

        var erro = Assert.Throws<ArgumentException>(() => Leiaute.Danfe(lida));

        Assert.Contains("modelo 55", erro.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(FormatoImpressao.Nenhum, "tpImp")]
    [InlineData(FormatoImpressao.DanfeSimplificado, "DANFE Simplificado")]
    [InlineData(FormatoImpressao.DanfeNFCe, "DANFE NFC-e")]
    [InlineData(FormatoImpressao.DanfeNFCeMensagem, "DANFE NFC-e")]
    public void RecusaOsFormatosQueNaoSaoDoDanfeDaNFe(FormatoImpressao formato, string trecho)
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.FormatoImpressao = formato;

        var erro = Assert.Throws<ArgumentException>(() => Leiaute.Danfe(NotasDanfe.Autorizada(nota)));

        Assert.Contains(trecho, erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SemTpImpPrecisaDaOrientacaoDasOpcoes()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.FormatoImpressao = null;

        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(NotasDanfe.Autorizada(nota)));
        Assert.Same(PapelImpressao.A4Paisagem,
            Leiaute.Danfe(NotasDanfe.Autorizada(nota), new OpcoesDanfe { Orientacao = OrientacaoDanfe.Paisagem }).Papel);
    }

    [Fact]
    public void RecusaAOrientacaoForaDoEnumEOPapelNoRetrato()
    {
        var lida = NotasDanfe.Autorizada(NotasDanfe.Simples());

        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(lida, new OpcoesDanfe { Orientacao = (OrientacaoDanfe)7 }));
        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(lida, new OpcoesDanfe { Papel = PapelImpressao.A4Retrato }));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(110)]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(205)]
    public void RecusaOProtocoloDeNotaDenegadaOuCancelada(int status)
    {
        var protocolo = NotasDanfe.ProtocoloAutorizado();
        protocolo.Status = status;

        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(NotasDanfe.Lida(NotasDanfe.Simples(), protocolo)));
    }

    [Theory]
    [InlineData(100)]
    [InlineData(150)]
    public void AceitaOProtocoloAutorizadoEOAutorizadoForaDePrazo(int status)
    {
        var protocolo = NotasDanfe.ProtocoloAutorizado();
        protocolo.Status = status;

        Assert.NotNull(Leiaute.Danfe(NotasDanfe.Lida(NotasDanfe.Simples(), protocolo)));
    }

    [Fact]
    public void AMensagemDeErroNaoTrazOsDadosDaNota()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificado;
        nota.Emitente.Nome = "NOME-SECRETO-DO-EMITENTE";

        var erro = Assert.Throws<ArgumentException>(() => Leiaute.Danfe(NotasDanfe.Autorizada(nota)));

        Assert.DoesNotContain("NOME-SECRETO", erro.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(NotasDanfe.Cnpj, erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACnpjAlfanumericoDoDestinatarioNaoGeraOCodigoDeBarrasDosDadosDaNota()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.TipoEmissao = TipoEmissao.ContingenciaFSDA;
        nota.Destinatario!.Cnpj = "12ABC34501DE35";

        Assert.Throws<ArgumentException>(() => Leiaute.Danfe(NotasDanfe.Lida(nota)));
    }

    [Fact]
    public void ODestinatarioAusenteSaiComOsCamposEmBranco()
    {
        var nota = NotasDanfe.Simples();
        nota.Destinatario = null;
        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));

        var campos = documento.Elementos.OfType<SecaoImpressao>().Single(s => s.Titulo == "DESTINATÁRIO / REMETENTE")
            .Linhas.SelectMany(l => l.Campos).ToArray();
        Assert.Equal(string.Empty, campos.Single(c => c.Rotulo == "NOME / RAZÃO SOCIAL").Valor);
        Assert.Equal(string.Empty, campos.Single(c => c.Rotulo == "CNPJ / CPF").Valor);
        Assert.Equal("14/10/2026", Valor(documento, "DATA DA EMISSÃO"));
    }

    [Fact]
    public void AReformaSoTemOBlocoQuandoANotaTemOsTributos()
    {
        var documentoIbs = Leiaute.Danfe(NotasDanfe.Autorizada(Completa()));
        var documentoSimples = Leiaute.Danfe(NotasDanfe.Autorizada(NotasDanfe.Simples()));

        Assert.Contains(documentoIbs.Elementos.OfType<SecaoImpressao>(), s => s.Titulo == "TOTAL DO IBS / CBS / IS");
        Assert.DoesNotContain(documentoSimples.Elementos.OfType<SecaoImpressao>(), s => s.Titulo == "TOTAL DO IBS / CBS / IS");
        Assert.Equal(17, documentoSimples.Elementos.OfType<TabelaImpressao>().Single().Colunas.Count);
        Assert.Equal(20, documentoIbs.Elementos.OfType<TabelaImpressao>().Single().Colunas.Count);
    }

    [Fact]
    public void ASimplificadoTipo2NaoSaiNoA4()
    {
        var nota = NotasDanfe.Simples();
        nota.Identificacao.FormatoImpressao = FormatoImpressao.DanfeSimplificadoTipo2;
        nota.Suplementares!.QrCode = NotasDanfe.QrCodeFixo;

        var documento = Leiaute.Danfe(NotasDanfe.Autorizada(nota));

        Assert.Same(PapelImpressao.Bobina80, documento.Papel);
        Assert.Empty(documento.Elementos.OfType<TabelaImpressao>());
    }
}
