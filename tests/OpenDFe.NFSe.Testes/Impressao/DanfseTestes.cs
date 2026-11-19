using System.Security.Cryptography;
using OpenDFe.Impressao;
using OpenDFe.NFSe;

namespace OpenDFe.Testes;

/// <summary>
/// A montagem do DANFSe (etapa 9.5, NT SE/CGNFS-e 008/2026 v1.02; docs/NFSE.md 12): o golden de texto de cada caso do
/// catálogo, as posições contra a tabela do item 2.4.5, os textos da NT (a produção restrita, as frases das supressões, a
/// ordem das informações complementares), as regras dos valores e as recusas.
/// </summary>
public sealed class DanfseTestes
{
    private const double Tolerancia = 0.35;
    private static readonly LeiauteNFSe101 Leiaute = LeiauteNFSe101.Instancia;

    public static TheoryData<string> Casos() => [.. CatalogoDanfse.Todos().Select(c => c.Nome)];

    private static DocumentoImpressao Montar(string caso)
    {
        var achado = CatalogoDanfse.Achar(caso);
        return Leiaute.Danfse(achado.Lida, achado.Opcoes);
    }

    private static IReadOnlyList<QuadroFormulario> Quadros(DocumentoImpressao documento) =>
        Assert.IsType<FormularioImpressao>(Assert.Single(documento.Elementos)).Quadros;

    private static IEnumerable<TextoFormulario> Textos(DocumentoImpressao documento) => Quadros(documento).OfType<TextoFormulario>();

    // O quadro do rótulo; ou, sem rótulo, o do valor (os títulos dos blocos e as frases).
    private static TextoFormulario Campo(DocumentoImpressao documento, string texto) =>
        Assert.Single(Textos(documento), t => t.Rotulo == texto || t.Rotulo is null && t.Valor == texto);

    private static string? Valor(DocumentoImpressao documento, string rotulo) => Campo(documento, rotulo).Valor;

    [Theory]
    [MemberData(nameof(Casos))]
    public void ODanfseDeCadaCasoEOGolden(string caso) =>
        ArquivoEsperado.Conferir("Danfse" + caso, FormularioPlano.Renderizar(Montar(caso)));

    [Theory]
    [MemberData(nameof(Casos))]
    public void TodoQuadroCabeNaAreaUtilDoA4Estreito(string caso)
    {
        var documento = Montar(caso);

        Assert.Same(PapelImpressao.A4RetratoEstreito, documento.Papel);
        Assert.All(Quadros(documento), q =>
        {
            Assert.InRange(q.XMm + q.LarguraMm, 0, 207.001);
            Assert.InRange(q.YMm + q.AlturaMm, 0, 293.95);
        });
    }

    // As posições e os tamanhos da tabela do 2.4.5 (em cm a partir da borda do papel; o quadro, em mm a partir da área útil, que
    // começa a 1,5 mm), com folga de 0,35 mm: as linhas dos blocos andam 0,6465 cm e reproduzem a tabela até 0,02 cm.
    [Theory]
    [InlineData("CHAVE DE ACESSO DA NFS-E", 0.30, 1.48, 15.30, 0.77)]
    [InlineData("NÚMERO DA NFS-E", 0.30, 2.27, 5.09, 0.67)]
    [InlineData("COMPETÊNCIA DA NFS-E", 5.41, 2.27, 5.09, 0.67)]
    [InlineData("DATA E HORA DA EMISSÃO DA NFS-E", 10.51, 2.27, 5.09, 0.67)]
    [InlineData("NÚMERO DA DPS", 0.30, 2.96, 5.09, 0.67)]
    [InlineData("SÉRIE DA DPS", 5.41, 2.96, 5.09, 0.67)]
    [InlineData("DATA E HORA DA EMISSÃO DA DPS", 10.51, 2.96, 5.09, 0.67)]
    [InlineData("EMITENTE DA NFS-E", 0.30, 3.65, 5.09, 0.67)]
    [InlineData("SITUAÇÃO DA NFS-E", 5.41, 3.65, 5.09, 0.67)]
    [InlineData("FINALIDADE", 10.51, 3.65, 5.09, 0.67)]
    [InlineData("PRESTADOR / FORNECEDOR", 0.30, 4.34, 5.09, 0.63)]
    [InlineData("TOMADOR / ADQUIRENTE", 0.30, 6.92, 5.09, 0.63)]
    [InlineData("DESTINATÁRIO DA OPERAÇÃO", 0.30, 8.86, 5.09, 0.63)]
    [InlineData("INTERMEDIÁRIO DA OPERAÇÃO", 0.30, 10.80, 5.09, 0.63)]
    [InlineData("SERVIÇO PRESTADO", 0.30, 12.74, 5.09, 0.63)]
    [InlineData("Código de Tributação Nacional / Municipal", 5.41, 12.74, 5.09, 0.63)]
    [InlineData("Código da NBS", 10.51, 12.74, 5.09, 0.63)]
    [InlineData("Local da Prestação / Sigla UF / País", 15.62, 12.74, 5.09, 0.63)]
    [InlineData("Descrição do Serviço", 0.30, 13.79, 20.40, 0.62)]
    [InlineData("TRIBUTAÇÃO MUNICIPAL (ISSQN)", 0.30, 14.43, 5.09, 0.63)]
    [InlineData("Regime Especial de Tributação do ISSQN", 0.30, 15.08, 5.09, 0.63)]
    [InlineData("Número Processo Suspensão", 15.62, 15.08, 5.09, 0.63)]
    [InlineData("Benefício Municipal", 0.30, 15.73, 5.09, 0.63)]
    [InlineData("BC ISSQN", 0.30, 16.37, 5.09, 0.63)]
    [InlineData("ISSQN Apurado", 15.62, 16.37, 5.09, 0.63)]
    [InlineData("TRIBUTAÇÃO FEDERAL (EXCETO CBS)", 0.30, 17.02, 5.09, 0.63)]
    [InlineData("IRRF", 5.41, 17.02, 5.09, 0.63)]
    [InlineData("PIS - Débito Apuração Própria", 0.30, 17.67, 5.09, 0.63)]
    [InlineData("TRIBUTAÇÃO IBS / CBS", 0.30, 18.32, 5.09, 0.63)]
    [InlineData("CST / cClassTrib", 5.41, 18.32, 5.09, 0.63)]
    [InlineData("Exclusões e Reduções da Base de Cálculo", 0.30, 18.96, 5.09, 0.63)]
    [InlineData("Alíq. Efetiva Municipal - IBS", 0.30, 19.61, 5.09, 0.63)]
    [InlineData("Valor Total Apurado - IBS", 0.30, 20.26, 5.09, 0.63)]
    [InlineData("Valor Total Apurado - CBS", 15.62, 20.26, 5.09, 0.63)]
    [InlineData("VALOR TOTAL DA NFS-E", 0.30, 20.90, 5.09, 0.67)]
    [InlineData("VALOR DA OPERAÇÃO / SERVIÇO", 5.41, 20.90, 5.09, 0.67)]
    [InlineData("Total das Retenções (ISSQN / Federais)", 0.30, 21.59, 5.09, 0.67)]
    [InlineData("VALOR LÍQUIDO DA NFS-e + IBS/CBS", 15.62, 21.59, 5.09, 0.67)]
    [InlineData("INFORMAÇÕES COMPLEMENTARES", 0.30, 22.27, 5.09, 0.39)]
    [InlineData("DATA CIENTIFICAÇÃO:", 0.30, 28.10, 5.09, 0.67)]
    [InlineData("IDENTIFICAÇÃO E ASSINATURA", 5.41, 28.10, 5.09, 0.67)]
    [InlineData("Nº NFS-e / CHAVE NFS-e", 10.51, 28.10, 10.19, 0.67)]
    public void ACompletaTemOsCamposNasPosicoesDaTabelaDaNt(string texto, double x, double y, double largura, double altura)
    {
        var campo = Campo(Montar("Anexo"), texto);

        Assert.Equal((x - 0.15) * 10, campo.XMm, Tolerancia);
        Assert.Equal((y - 0.15) * 10, campo.YMm, Tolerancia);
        Assert.Equal(largura * 10, campo.LarguraMm, Tolerancia);
        Assert.Equal(altura * 10, campo.AlturaMm, Tolerancia);
    }

    [Fact]
    public void OQrCodeALogomarcaEOCabecalhoTemAsMedidasEOTextoDaNt()
    {
        var documento = Montar("CompletaA");
        var chave = CatalogoDanfse.Achar("CompletaA").Lida.Chave!.Value.Valor;

        // O QR Code em X 17,48 e Y 1,67, com 1,52 cm, e a URL da consulta pública com a chave (2.4.3).
        var qr = Assert.Single(Quadros(documento).OfType<QrCodeFormulario>());
        Assert.Equal("https://www.nfse.gov.br/ConsultaPublica/?tpc=1&chave=" + chave, qr.Texto);
        Assert.Equal((173.3, 15.2, 15.2), (qr.XMm, qr.YMm, qr.LarguraMm));
        // A logomarca oficial (docs/NTS.md 12.3), no canto esquerdo, 4,00 x 0,85 cm em X 0,49 e Y 0,44.
        var logo = Assert.Single(Quadros(documento).OfType<ImagemFormulario>());
        Assert.Equal("ab57fa34887929a10ee3b9b4d666084ec9b9465e62bbcc3523b99b23ccac1063", Convert.ToHexStringLower(SHA256.HashData(logo.Png.Span)));
        Assert.Equal((3.4, 2.9, 40d, 8.5), (logo.XMm, logo.YMm, logo.LarguraMm, logo.AlturaMm));
        // O título de 9 pt em negrito, centrado; o município de 8 pt; o ambiente de 6 pt; o texto do QR Code de 6 pt.
        var titulo = Assert.Single(Textos(documento), t => t.Valor == "DANFSe v2.0\nDocumento Auxiliar da NFS-e");
        Assert.Equal((9d, true, AlinhamentoImpressao.Centro), (titulo.TamanhoValorPt, titulo.ValorNegrito, titulo.Alinhamento));
        Assert.Equal(8, Assert.Single(Textos(documento), t => t.Valor == "Município: Belo Horizonte / MG").TamanhoValorPt);
        Assert.Equal(6, Assert.Single(Textos(documento), t => t.Valor == "Ambiente Gerador: Sistema Nacional da NFS-e").TamanhoValorPt);
        Assert.Equal(6, Assert.Single(Textos(documento), t => t.Valor == "Tipo de Ambiente: Homologação").TamanhoValorPt);
        Assert.Equal(6, Assert.Single(Textos(documento), t => t.Valor?.StartsWith("A autenticidade desta NFS-e", StringComparison.Ordinal) == true).TamanhoValorPt);
        // O formulário com a borda de 1 pt (2.2.3) e o título do documento.
        var formulario = Assert.IsType<FormularioImpressao>(documento.Elementos[0]);
        Assert.Equal(1, formulario.BordaPt);
        Assert.Equal("DANFSe NFS-e 42", documento.Titulo);
    }

    [Fact]
    public void AProducaoRestritaTemAFraseEmVermelhoEAProducaoNao()
    {
        var restrita = Assert.Single(Textos(Montar("CompletaA")), t => t.Valor == "NFS-e SEM VALIDADE JURÍDICA");
        Assert.Equal((CorImpressao.Vermelho, 9d, true), (restrita.Cor, restrita.TamanhoValorPt, restrita.ValorNegrito));

        var producao = Montar("Producao");
        Assert.DoesNotContain(Textos(producao), t => t.Valor == "NFS-e SEM VALIDADE JURÍDICA");
        Assert.Contains(Textos(producao), t => t.Valor == "Tipo de Ambiente: Produção");
        Assert.All(Textos(producao), t => Assert.Equal(CorImpressao.Preto, t.Cor));
        // Nenhuma das duas tem marca d'água (a NT não pede na produção restrita, decisão G6).
        Assert.Null(Assert.IsType<FormularioImpressao>(producao.Elementos[0]).MarcaAgua);
        Assert.Null(Assert.IsType<FormularioImpressao>(Montar("CompletaA").Elementos[0]).MarcaAgua);
    }

    [Fact]
    public void OSombreadoEstaNoCabecalhoNosTitulosENosDoisCamposDaNt()
    {
        var documento = Montar("CompletaA");
        var sombreados = Textos(documento).Where(t => t.Sombreado).Select(t => t.Rotulo ?? t.Valor).ToList();

        Assert.Contains("EMITENTE DA NFS-E", sombreados);
        Assert.Contains("VALOR LÍQUIDO DA NFS-e + IBS/CBS", sombreados);
        string[] titulos =
        [
            "PRESTADOR / FORNECEDOR", "TOMADOR / ADQUIRENTE", "DESTINATÁRIO DA OPERAÇÃO", "INTERMEDIÁRIO DA OPERAÇÃO", "SERVIÇO PRESTADO",
            "TRIBUTAÇÃO MUNICIPAL (ISSQN)", "TRIBUTAÇÃO FEDERAL (EXCETO CBS)", "TRIBUTAÇÃO IBS / CBS", "VALOR TOTAL DA NFS-E", "INFORMAÇÕES COMPLEMENTARES",
        ];
        foreach (var titulo in titulos)
        {
            var quadro = Campo(documento, titulo);
            Assert.True(quadro.Sombreado, titulo);
            Assert.Equal((7d, true), (quadro.TamanhoValorPt, quadro.ValorNegrito));
        }

        // O cabeçalho: o quadro sem texto da largura toda, sombreado.
        Assert.Contains(Textos(documento), t => t is { Rotulo: null, Valor: null, Sombreado: true } && Math.Abs(t.LarguraMm - 204) < 0.01);
        Assert.Equal(titulos.Length + 3, sombreados.Count);
    }

    [Fact]
    public void OsBlocosSemDadosSaemComAsFrasesDaNtEmUmaLinha()
    {
        var minima = Montar("Minima");
        Assert.Equal(3.2, Campo(minima, "TOMADOR/ADQUIRENTE DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e").AlturaMm, 0.001);
        Assert.Equal(204, Campo(minima, "DESTINATÁRIO DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e").LarguraMm, 0.001);
        Campo(minima, "INTERMEDIÁRIO DA OPERAÇÃO NÃO IDENTIFICADO NA NFS-e");
        Assert.DoesNotContain(Textos(minima), t => t.Valor == "TOMADOR / ADQUIRENTE");

        var proprio = Montar("DestinatarioTomador");
        Campo(proprio, "O DESTINATÁRIO É O PRÓPRIO TOMADOR/ADQUIRENTE DA OPERAÇÃO");
        Assert.DoesNotContain(Textos(proprio), t => t.Valor == "DESTINATÁRIO DA OPERAÇÃO");

        var naoIncide = Montar("NaoIncidencia");
        Campo(naoIncide, "TRIBUTAÇÃO MUNICIPAL (ISSQN) - OPERAÇÃO NÃO SUJEITA AO ISSQN");
        Assert.DoesNotContain(Textos(naoIncide), t => t.Rotulo == "BC ISSQN");
    }

    [Fact]
    public void AAlturaDosBlocosSuprimidosVaiADescricaoEAsInformacoes()
    {
        var completa = Montar("CompletaA");
        var minima = Montar("Minima");
        var suprimido = Montar("CanhotoSuprimido");

        // A mínima tem três blocos de uma linha (0,34 cm) no lugar de três de três linhas: os blocos de baixo sobem.
        Assert.True(Campo(minima, "TRIBUTAÇÃO MUNICIPAL (ISSQN)").YMm < Campo(completa, "TRIBUTAÇÃO MUNICIPAL (ISSQN)").YMm - 30);
        // As informações complementares (o texto e a linha dos tributos) vão até o canhoto, ou até o fim sem ele.
        var tributos = (DocumentoImpressao d) => Assert.Single(Textos(d), t => t.Valor?.StartsWith("Totais Aproximados", StringComparison.Ordinal) == true);
        Assert.Equal((28.08 - 0.15) * 10, tributos(completa).YMm + tributos(completa).AlturaMm, 0.01);
        Assert.Equal((28.77 - 0.15) * 10, tributos(suprimido).YMm + tributos(suprimido).AlturaMm, 0.01);
        Assert.DoesNotContain(Textos(suprimido), t => t.Rotulo == "DATA CIENTIFICAÇÃO:");
    }

    [Fact]
    public void ALinhaDaNota5SomeSemDadoEADaNota6SoAteACompetenciaDe2026()
    {
        // A mínima não tem regime especial (o "Nenhum" não é dado), imunidade nem suspensão: a linha some.
        Assert.DoesNotContain(Textos(Montar("Minima")), t => t.Rotulo == "Regime Especial de Tributação do ISSQN");
        Assert.Contains(Textos(Montar("CompletaA")), t => t.Rotulo == "Regime Especial de Tributação do ISSQN");

        Assert.Contains(Textos(Montar("CompletaA")), t => t.Rotulo == "PIS - Débito Apuração Própria");
        var depois = Montar("Competencia2027");
        Assert.DoesNotContain(Textos(depois), t => t.Rotulo == "PIS - Débito Apuração Própria");
        Assert.DoesNotContain(Textos(depois), t => t.Rotulo == "Descrição Contrib. Sociais - Retidas");
    }

    [Fact]
    public void AsContribuicoesRetidasSomamOPisEACofinsSoComOTipo1()
    {
        var dps = DpsTeste.CompletaA();
        var federal = dps.Valores.Tributacao.Federal!;
        Assert.Equal(RetencaoPisCofins.PisCofinsRetidos, federal.PisCofins!.Retencao);
        var documento = Montar("CompletaA");
        var soma = federal.RetencaoCsll!.Value + federal.PisCofins.ValorPis!.Value + federal.PisCofins.ValorCofins!.Value;

        Assert.Equal("R$ " + Reais(soma), Valor(documento, "Contribuições Sociais - Retidas"));
        Assert.Equal("R$ 0,00", Valor(documento, "PIS - Débito Apuração Própria"));
        Assert.Equal("R$ 0,00", Valor(documento, "COFINS - Débito Apuração Própria"));
        Assert.Equal("PIS/COFINS Retidos", Valor(documento, "Descrição Contrib. Sociais - Retidas"));

        federal.PisCofins.Retencao = RetencaoPisCofins.TodosRetidos;
        var outro = Leiaute.Danfse(CatalogoDanfse.Ler(dps));
        Assert.Equal("R$ " + Reais(federal.RetencaoCsll.Value), Valor(outro, "Contribuições Sociais - Retidas"));
        Assert.NotEqual("R$ 0,00", Valor(outro, "PIS - Débito Apuração Própria"));
    }

    [Fact]
    public void OPrestadorUsaOsDadosDaDpsEOEmitDaNFSeNaFalta()
    {
        // A mínima: o prestador CPF na DPS, sem nome nem endereço (o ambiente nacional os tira do cadastro): o documento é o da
        // DPS, e o nome e o endereço vêm do emit.
        var minima = Montar("Minima");
        var cpf = DpsTeste.Minima().Prestador.Cpf!;
        Assert.Equal($"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}", Textos(minima).First(t => t.Rotulo == "CNPJ / CPF / NIF").Valor);
        Assert.Equal("PRESTADOR DE TESTE LTDA", Textos(minima).First(t => t.Rotulo == "Nome / Nome Empresarial").Valor);
        Assert.Equal("RUA DOS TESTES, 100, SALA 2, CENTRO", Textos(minima).First(t => t.Rotulo == "Endereço").Valor);
        Assert.Equal("Belo Horizonte / MG", Textos(minima).First(t => t.Rotulo == "Município / Sigla UF").Valor);
        Assert.Equal("3106200 / 30.110-000", Textos(minima).First(t => t.Rotulo == "Código IBGE / CEP").Valor);
    }

    [Fact]
    public void OMunicipioDoEnderecoSaiPeloNomeDaTabelaDoIbge()
    {
        var documento = Montar("CompletaA");
        var destinatario = Textos(documento).Where(t => t.Rotulo == "Município / Sigla UF").Select(t => t.Valor).ToList();

        Assert.Contains("São Paulo / SP", destinatario);
        Assert.Equal(5571, MunicipiosIbge.Quantidade);
        Assert.Equal("Belo Horizonte", MunicipiosIbge.Nome(3106200));
        Assert.Equal("Alta Floresta D'Oeste", MunicipiosIbge.Nome(1100015));
        Assert.Equal("Brasília", MunicipiosIbge.Nome(5300108));
        Assert.Null(MunicipiosIbge.Nome(3199999));
        Assert.Equal(UF.SP, MunicipiosIbge.Estado(3550308));
        Assert.Null(MunicipiosIbge.Estado(9900000));
        Assert.Null(MunicipiosIbge.Estado(123));
    }

    [Fact]
    public void AsInformacoesComplementaresSeguemAOrdemDaNtComPipesEATabelaDosTributos()
    {
        var documento = Montar("CompletaA");
        var dps = DpsTeste.CompletaA();
        var compl = dps.Servico.InformacoesComplementares!;
        var informacoes = Assert.Single(Textos(documento), t => t.Valor?.StartsWith("Inf. Cont.:", StringComparison.Ordinal) == true).Valor!;

        string[] ordem = ["Inf. Cont.:", "NFS-e Subst.:", "Doc. Ref.:", "Cod. Obra:", "Insc. Imob.:", "Cod. Evt.:", "Doc. Tec.:", "Núm. Ped.:", "Item Ped.:", "Inf. A. T. Mun.:"];
        var posicoes = ordem.Select(r => informacoes.IndexOf(r, StringComparison.Ordinal)).ToList();
        Assert.All(posicoes, p => Assert.True(p >= 0));
        Assert.Equal(posicoes.Order(), posicoes);
        Assert.Contains(" | NFS-e Subst.: " + dps.Substituicao!.Chave + " | ", informacoes, StringComparison.Ordinal);
        Assert.Contains("Item Ped.: " + string.Join("; ", compl.ItensPedido!.Itens), informacoes, StringComparison.Ordinal);
        Assert.EndsWith("Inf. A. T. Mun.: Uso do município", informacoes, StringComparison.Ordinal);
        Assert.Equal("Totais Aproximados dos Tributos cfe. Lei nº 12.741/2012: Federais: R$ 100,00 ; Estaduais: R$ 0,00 ; Municipais: R$ 20,00",
            Assert.Single(Textos(documento), t => t.Valor?.StartsWith("Totais Aproximados", StringComparison.Ordinal) == true).Valor);
    }

    [Fact]
    public void OsTextosLongosSaoCortadosNoTamanhoDaNtComReticenciasEOsTributosNao()
    {
        var documento = Montar("TextosLimite");

        var descricao = Valor(documento, "Descrição do Serviço")!;
        Assert.InRange(descricao.Length, 1295, 1300);
        Assert.EndsWith("...", descricao, StringComparison.Ordinal);
        var informacoes = Assert.Single(Textos(documento), t => t.Valor?.StartsWith("Inf. Cont.:", StringComparison.Ordinal) == true).Valor!;
        Assert.InRange(informacoes.Length, 1990, 1997);
        Assert.EndsWith("...", informacoes, StringComparison.Ordinal);
        var nome = Textos(documento).First(t => t.Rotulo == "Nome / Nome Empresarial").Valor!;
        Assert.InRange(nome.Length, 75, 80);
        Assert.Contains(Textos(documento), t => t.Valor?.StartsWith("Totais Aproximados", StringComparison.Ordinal) == true && t.Valor.EndsWith("R$ 20,00", StringComparison.Ordinal));
        // Os campos de descrição longa (37 caracteres e reticências) da NT: o Simples Nacional.
        Assert.Equal("Optante - Microempresa ou Empresa de...", Textos(documento).First(t => t.Rotulo == "Simples Nacional na Data de Competência").Valor);
    }

    [Fact]
    public void ACanceladaEASubstituidaTemAMarcaDAguaEAsDuasJuntasSaoRecusadas()
    {
        Assert.Equal("CANCELADA", Assert.IsType<FormularioImpressao>(Montar("Cancelada").Elementos[0]).MarcaAgua);
        Assert.Equal("SUBSTITUÍDA", Assert.IsType<FormularioImpressao>(Montar("Substituida").Elementos[0]).MarcaAgua);

        var lida = CatalogoDanfse.Achar("CompletaA").Lida;
        var erro = Assert.Throws<ArgumentException>(() => Leiaute.Danfse(lida, new OpcoesDanfse { Cancelada = true, Substituida = true }));
        Assert.Equal("opcoes", erro.ParamName);
    }

    [Fact]
    public void ALeituraSemANFSeOuSemAChaveESemLeituraSaoRecusadas()
    {
        Assert.Throws<ArgumentNullException>(() => Leiaute.Danfse(null!));
        // A DPS sozinha não tem DANFSe.
        var dps = Leiaute.Ler(Leiaute.Escrever(DpsTeste.CompletaA()).Xml);
        Assert.Null(dps.Nota);
        Assert.Equal("nota", Assert.Throws<ArgumentException>(() => Leiaute.Danfse(dps)).ParamName);
        // O Id do infNFSe que não é uma chave.
        var semChave = Leiaute.Ler(NFSeTeste.Xml(NFSeTeste.Nota(DpsTeste.CompletaA())).Replace("Id=\"NFS" + NFSeTeste.Chave, "Id=\"NFS123", StringComparison.Ordinal));
        Assert.Null(semChave.Chave);
        Assert.Equal("nota", Assert.Throws<ArgumentException>(() => Leiaute.Danfse(semChave)).ParamName);
    }

    [Fact]
    public void OMunicipioDoCabecalhoNaoSaiComOItem99EAVersao100TemAsDescricoesDela()
    {
        var item99 = Leiaute.Danfse(CatalogoDanfse.Ler(Ajustar(DpsTeste.CompletaA(), d => d.Servico.Codigo.TributacaoNacional = "990101")));
        Assert.DoesNotContain(Textos(item99), t => t.Valor?.StartsWith("Município:", StringComparison.Ordinal) == true);

        var versao100 = Montar("Versao100");
        Assert.Equal("NFS-e de Substituição Gerada", Valor(versao100, "SITUAÇÃO DA NFS-E"));
        // Sem o grupo do IBS e da CBS, os campos saem com o traço (nota 12).
        Assert.Equal("-", Valor(versao100, "Valor Total Apurado - CBS"));
        Assert.Equal("-", Valor(versao100, "FINALIDADE"));
    }

    [Fact]
    public void OMesmoDocumentoSaiEmParalelo()
    {
        var lida = CatalogoDanfse.Achar("CompletaA").Lida;
        var esperado = FormularioPlano.Renderizar(Leiaute.Danfse(lida));
        var textos = new string[16];
        Parallel.For(0, textos.Length, i => textos[i] = FormularioPlano.Renderizar(Leiaute.Danfse(lida)));
        Assert.All(textos, t => Assert.Equal(esperado, t));
    }

    // O dinheiro sem a cultura do sistema (vírgula decimal e ponto de milhar), como o DANFSe.
    private static string Reais(decimal valor) =>
        valor.ToString("N2", new System.Globalization.NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." });

    private static Dps Ajustar(Dps dps, Action<Dps> ajuste)
    {
        ajuste(dps);
        return dps;
    }
}
