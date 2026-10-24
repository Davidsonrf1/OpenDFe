using System.Diagnostics;
using OpenDFe.Impressao;
using OpenDFe.Impressao.Pdf;
using OpenDFe.Impressao.Pdf.Testes;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

/// <summary>
/// O PDF dos documentos completos do DANFCE (etapa 7.2) e do DANFE (etapa 7.3), os mesmos casos dos golden de texto, lidos de
/// volta pelo PdfPig (etapa 7.4, docs/CONTRATOS.md 22): o papel, o texto que o modelo manda, as chaves e os protocolos, a
/// legenda da contingência, a marca d'água, a paginação e o QR Code e o código de barras como vetor.
/// </summary>
public sealed class PdfDocumentosTestes
{
    private const double MmPt = 72 / 25.4;
    private static readonly LeiauteNFe400 Leiaute = new();

    public static TheoryData<string> Danfces() => [.. CatalogoImpressao.Danfces().Select(c => c.Nome)];

    public static TheoryData<string> Danfes() => [.. CatalogoImpressao.Danfes().Select(c => c.Nome)];

    // Os textos do modelo que o PDF precisa trazer, sem espaços: o texto, as células, os campos (rótulo e valor), os títulos das seções
    // e das tabelas e as células delas. Os que têm ficha de página são conferidos à parte (o número vem da página).
    private static IEnumerable<string> TextosModelo(IEnumerable<ElementoImpressao> elementos)
    {
        foreach (var elemento in elementos)
        {
            switch (elemento)
            {
                case TextoImpressao texto:
                    yield return texto.Texto;
                    break;
                case LinhaImpressao linha:
                    foreach (var celula in linha.Celulas)
                        yield return celula.Texto;
                    break;
                case SecaoImpressao secao:
                    if (secao.Titulo is not null)
                        yield return secao.Titulo;
                    foreach (var campo in secao.Linhas.SelectMany(l => l.Campos))
                    {
                        yield return campo.Rotulo;
                        yield return campo.Valor;
                    }

                    break;
                case TabelaImpressao tabela:
                    if (tabela.Titulo is not null)
                        yield return tabela.Titulo;
                    foreach (var coluna in tabela.Colunas)
                        yield return coluna.Titulo;
                    foreach (var texto in tabela.Linhas.SelectMany(l => l))
                        yield return texto;
                    break;
            }
        }
    }

    private static void ConferirTextoModelo(CasoImpressao caso, PdfLeitor pdf)
    {
        var texto = pdf.TextoCompacto();
        foreach (var esperado in TextosModelo(caso.Documento.Elementos).Where(t => !t.Contains('{', StringComparison.Ordinal)))
        {
            var compacto = PdfLeitor.Compactar(esperado);
            if (compacto.Length > 0)
                Assert.True(texto.Contains(compacto, StringComparison.Ordinal), $"[{caso.Nome}] o PDF perdeu o texto \"{esperado}\"");
        }
    }

    private static string Chave(CasoImpressao caso) => caso.Lida.Chave!.ToString();

    private static void ConferirOQrCode(CasoImpressao caso, PdfLeitor pdf)
    {
        // O QR Code do PDF é o da nota (Suplementares.QrCode), módulo a módulo, no nível M.
        var qrCode = caso.Lida.Nota!.Suplementares!.QrCode!;
        var esperada = PdfVetores.MatrizQrCode(qrCode);
        var (matriz, caixa, modulo) = PdfVetores.MatrizQrCodePdf(pdf, 1, esperada.GetLength(0));
        Assert.True(esperada.Cast<bool>().SequenceEqual(matriz.Cast<bool>()), $"[{caso.Nome}] o QR Code do PDF não é o da nota");
        var lado = Assert.Single(caso.Documento.Elementos.OfType<QrCodeImpressao>()).LadoMm;
        // O lado total, com a margem de silêncio dentro, é o do documento (30 mm, ao menos os 25 mm do manual), e a margem é a do
        // manual (ao menos 3 mm e 10% do lado, 3.4) e a do ISO/IEC 18004 (4 módulos), medida no PDF e não pela função do renderizador.
        Assert.True(lado >= 25);
        var margemMm = (lado - caixa.Largura / MmPt) / 2;
        Assert.True(margemMm >= 3 - 0.01, $"[{caso.Nome}] a margem de silêncio de {margemMm:F2} mm tem menos de 3 mm");
        Assert.True(margemMm >= 0.1 * lado - 0.01, $"[{caso.Nome}] a margem de silêncio tem menos de 10% do lado");
        Assert.True(margemMm >= 4 * modulo / MmPt - 0.01, $"[{caso.Nome}] a margem de silêncio tem menos de 4 módulos");
    }

    // ---- DANFCE -------------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Danfces))]
    public void OsDanfcesViramPdfComOsDadosDaNota(string nome)
    {
        var caso = CatalogoImpressao.Danfces().Single(c => c.Nome == nome);

        var bytes = RenderizadorPdf.Gerar(caso.Documento);
        AmostrasPdf.Gravar("danfce-" + nome, bytes);

        using var pdf = new PdfLeitor(bytes);
        var texto = pdf.TextoCompacto();
        Assert.Equal(1, pdf.Paginas);
        Assert.Equal(caso.Documento.Papel.LarguraMm * MmPt, pdf.Pagina(1).Width, 0.5);
        ConferirTextoModelo(caso, pdf);
        // A chave de 44 dígitos (em 11 blocos de 4), o protocolo e o total.
        Assert.Contains(Chave(caso), texto, StringComparison.Ordinal);
        var nota = caso.Lida.Nota!;
        Assert.Contains("ValortotalR$" + FormatadorImpressao.Dinheiro(nota.Total.Icms.ValorProdutos!.Value), texto, StringComparison.Ordinal);
        if (caso.Autorizada)
        {
            Assert.Contains("Protocolodeautorização:" + caso.Lida.Protocolo!.Numero, texto, StringComparison.Ordinal);
            Assert.DoesNotContain("EMITIDAEMCONTINGÊNCIA", texto, StringComparison.Ordinal);
        }
        else
        {
            // A contingência sem protocolo: a legenda nos dois lugares do manual e nenhum protocolo.
            Assert.Equal(2, Contar(texto, "EMITIDAEMCONTINGÊNCIA"));
            Assert.Equal(2, Contar(texto, "Pendentedeautorização"));
            Assert.DoesNotContain("Protocolodeautorização", texto, StringComparison.Ordinal);
        }

        if (nota.Identificacao.Ambiente == Ambiente.Homologacao)
            Assert.Contains("EMITIDAEMAMBIENTEDEHOMOLOGAÇÃO-SEMVALORFISCAL", texto, StringComparison.Ordinal);
        ConferirOQrCode(caso, pdf);
    }

    [Fact]
    public void ADanfceEmDuasViasTemAViaCertaEAMesmaChave()
    {
        var consumidor = CatalogoImpressao.Danfces().Single(c => c.Nome == "Exemplo3Consumidor");
        var estabelecimento = CatalogoImpressao.Danfces().Single(c => c.Nome == "Exemplo3Estabelecimento");

        using var a = new PdfLeitor(RenderizadorPdf.Gerar(consumidor.Documento));
        using var b = new PdfLeitor(RenderizadorPdf.Gerar(estabelecimento.Documento));

        Assert.Contains("ViadoConsumidor", a.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains("ViadoEstabelecimento", b.TextoCompacto(), StringComparison.Ordinal);
        Assert.DoesNotContain("ViadoEstabelecimento", a.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains(Chave(consumidor), a.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains(Chave(estabelecimento), b.TextoCompacto(), StringComparison.Ordinal);
    }

    [Fact]
    public void ADanfceDe58mmTemAMesmaInformacaoDaDe80mmEOutraLargura()
    {
        var oitenta = CatalogoImpressao.Danfces().Single(c => c.Nome == "Exemplo1");
        var cinquenta = CatalogoImpressao.Danfces().Single(c => c.Nome == "Bobina58Exemplo1");

        using var a = new PdfLeitor(RenderizadorPdf.Gerar(oitenta.Documento));
        using var b = new PdfLeitor(RenderizadorPdf.Gerar(cinquenta.Documento));

        Assert.Equal(80 * MmPt, a.Pagina(1).Width, 0.5);
        Assert.Equal(58 * MmPt, b.Pagina(1).Width, 0.5);
        Assert.Equal(a.TextoCompacto(), b.TextoCompacto());
        Assert.True(b.Pagina(1).Height > a.Pagina(1).Height, "a bobina mais estreita quebra mais linhas e fica mais comprida");
    }

    [Fact]
    public void ANfceDe300ItensSaiNumBilheteSoDeAlturaContinua()
    {
        var nota = NotasDanfce.Base();
        for (var i = 1; i <= 300; i++)
            nota.Item($"I{i:0000}", $"PRODUTO NUMERO {i}", 1m, "UN", 1.10m);
        nota.Totalizar();
        nota.Pagar(MeioPagamento.Dinheiro, 330m);
        var lida = NotasDanfce.Autorizada(nota);
        var caso = new CasoImpressao("Nfce300", lida, Leiaute.Danfce(lida));

        var tempo = Stopwatch.StartNew();
        var bytes = RenderizadorPdf.Gerar(caso.Documento);
        tempo.Stop();
        AmostrasPdf.Gravar("danfce-300itens", bytes);

        using var pdf = new PdfLeitor(bytes);
        Assert.Equal(1, pdf.Paginas);
        Assert.True(pdf.Pagina(1).Height > 300 * 9.2 * 2, "o bilhete cresce com os 600 linhas dos itens");
        for (var i = 1; i <= 300; i++)
            Assert.Contains($"I{i:0000}PRODUTONUMERO{i}", pdf.TextoCompacto(), StringComparison.Ordinal);
        Assert.Contains("ValortotalR$330,00", pdf.TextoCompacto(), StringComparison.Ordinal);
        Assert.True(tempo.Elapsed < TimeSpan.FromSeconds(10), $"300 itens em {tempo.Elapsed}");
        ConferirOQrCode(caso, pdf);
    }

    // ---- DANFE -------------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Danfes))]
    public void OsDanfesViramPdfComOsDadosDaNota(string nome)
    {
        var caso = CatalogoImpressao.Danfes().Single(c => c.Nome == nome);

        var bytes = RenderizadorPdf.Gerar(caso.Documento);
        AmostrasPdf.Gravar("danfe-" + nome, bytes);

        using var pdf = new PdfLeitor(bytes);
        var texto = pdf.TextoCompacto();
        var papel = caso.Documento.Papel;
        ConferirTextoModelo(caso, pdf);
        Assert.Contains(Chave(caso), texto, StringComparison.Ordinal);
        if (caso.Autorizada)
            Assert.Contains(caso.Lida.Protocolo!.Numero!, texto, StringComparison.Ordinal);

        if (papel.Tipo == TipoPapel.Bobina)
        {
            // O Simplificado Tipo 2 no leiaute do DANFCE: bilhete de altura contínua e o QR Code da nota.
            Assert.Equal(1, pdf.Paginas);
            Assert.Equal(papel.LarguraMm * MmPt, pdf.Pagina(1).Width, 0.5);
            Assert.Contains("DANFESimplificadoTipo2", texto, StringComparison.Ordinal);
            ConferirOQrCode(caso, pdf);
            return;
        }

        // O A4: as dimensões da orientação e, em cada página, a folha "k/n" e o código de barras da chave (o cabeçalho de continuação
        // repete o da primeira) e a marca d'água, se há.
        var largura = papel.LarguraMm * MmPt;
        var altura = papel.AlturaMm!.Value * MmPt;
        for (var pagina = 1; pagina <= pdf.Paginas; pagina++)
        {
            Assert.Equal(largura, pdf.Pagina(pagina).Width, 0.5);
            Assert.Equal(altura, pdf.Pagina(pagina).Height, 0.5);
            Assert.Contains($"FOLHA{pagina}/{pdf.Paginas}", pdf.TextoCompacto(pagina), StringComparison.Ordinal);
            var (modulo, caixa) = PdfVetores.ConferirBarras(pdf, pagina, PdfVetores.CaminhoBarras(pdf, pagina, Chave(caso)), Chave(caso));
            // Largura de 11,5 cm, módulo de ao menos 0,2 mm e barras de ao menos 0,8 cm de altura (Anexo II 2).
            Assert.Equal(115 * MmPt, caixa.Largura, 0.3);
            Assert.True(modulo / MmPt >= 0.2);
            Assert.True(caixa.Altura >= 8 * MmPt - 0.01);
            if (caso.Documento.MarcaAgua is { } marca)
            {
                var letras = pdf.Letras(pagina).Where(l => l.Tamanho > 20).ToList();
                Assert.Equal(marca, string.Concat(letras.Select(l => l.Valor)));
            }
        }

        // A contingência com o formulário de segurança traz também o código de barras dos Dados da NF-e (36 dígitos).
        foreach (var barras in caso.Documento.Elementos.OfType<CodigoBarrasImpressao>().Where(b => b.Digitos != Chave(caso)))
            PdfVetores.ConferirBarras(pdf, 1, PdfVetores.CaminhoBarras(pdf, 1, barras.Digitos), barras.Digitos);
    }

    [Theory]
    [InlineData("Simples", 1)]
    [InlineData("Paisagem", 2)]
    public void ODanfeDeUmItemComTodosOsQuadrosEPaginasDeAcordoComOPapel(string nome, int paginas)
    {
        // No retrato os quadros empilhados cabem numa folha; na paisagem (210 mm de altura) o quadro final passa para a folha 2.
        var caso = CatalogoImpressao.Danfes().Single(c => c.Nome == nome);

        using var pdf = new PdfLeitor(RenderizadorPdf.Gerar(caso.Documento));

        Assert.Equal(paginas, pdf.Paginas);
    }

    [Fact]
    public void OCanhotoSoSaiNoDanfeQueOTemEFicaNoAltoDaPrimeiraPagina()
    {
        var com = CatalogoImpressao.Danfes().Single(c => c.Nome == "Simples");
        var sem = CatalogoImpressao.Danfes().Single(c => c.Nome == "SemCanhoto");

        using var a = new PdfLeitor(RenderizadorPdf.Gerar(com.Documento));
        using var b = new PdfLeitor(RenderizadorPdf.Gerar(sem.Documento));

        Assert.Contains("RECEBEMOSDE", a.TextoCompacto(1), StringComparison.Ordinal);
        Assert.DoesNotContain("RECEBEMOSDE", b.TextoCompacto(1), StringComparison.Ordinal);
        Assert.True(a.Achar(1, "RECEBEMOS").Y < a.Achar(1, "DANFE").Y);
    }

    [Fact]
    public void ADescricaoDoItemDe500CaracteresQuebraNaCelulaENaoSeCorta()
    {
        var nota = NotasDanfe.Base();
        var descricao = string.Concat(Enumerable.Range(1, 60).Select(i => $"P{i:000}XXXX "))[..500].TrimEnd();
        nota.ItemDanfe("D1", descricao, 1m, "UN", 10m);
        nota.TotalizarDanfe();
        var lida = NotasDanfe.Autorizada(nota);
        var caso = new CasoImpressao("Descricao500", lida, Leiaute.Danfe(lida));

        using var pdf = new PdfLeitor(RenderizadorPdf.Gerar(caso.Documento));

        Assert.Equal(1, pdf.Paginas);
        Assert.Contains(PdfLeitor.Compactar(descricao), pdf.TextoCompacto(1), StringComparison.Ordinal);
    }

    [Fact]
    public void AsInformacoesComplementaresDe5000CaracteresContinuamNaFolhaSeguinteComACabecalho()
    {
        var nota = NotasDanfe.Simples();
        var texto = string.Join(' ', Enumerable.Range(1, 1000).Select(i => $"T{i:0000}"))[..5000].TrimEnd();
        nota.InformacoesAdicionais = new InformacoesAdicionais { Complementares = texto };
        var lida = NotasDanfe.Autorizada(nota);
        var caso = new CasoImpressao("InfCpl5000", lida, Leiaute.Danfe(lida));

        var bytes = RenderizadorPdf.Gerar(caso.Documento);
        AmostrasPdf.Gravar("danfe-infcpl5000", bytes);

        using var pdf = new PdfLeitor(bytes);
        Assert.True(pdf.Paginas > 1, "5.000 caracteres a 6 pt não cabem na folha 1");
        var tudo = pdf.TextoCompacto();
        foreach (var palavra in texto.Split(' '))
            Assert.Contains(palavra, tudo, StringComparison.Ordinal);
        for (var pagina = 1; pagina <= pdf.Paginas; pagina++)
            Assert.Contains($"FOLHA{pagina}/{pdf.Paginas}", pdf.TextoCompacto(pagina), StringComparison.Ordinal);
        // As palavras saem na ordem: a primeira na folha 1 e a última na última.
        Assert.Contains("T0001", pdf.TextoCompacto(1), StringComparison.Ordinal);
        Assert.Contains(texto.Split(' ')[^1], pdf.TextoCompacto(pdf.Paginas), StringComparison.Ordinal);
    }

    [Fact]
    public void ANfeDe300ItensPaginaComOCabecalhoDaTabelaEmCadaFolhaEOsItensNaOrdem()
    {
        var nota = NotasDanfe.Base();
        for (var i = 1; i <= 300; i++)
            nota.ItemDanfe($"P{i:0000}", $"Produto de teste número {i}", i, "UN", 1.5m + i / 100m);
        nota.TotalizarDanfe();
        var lida = NotasDanfe.Autorizada(nota);
        var caso = new CasoImpressao("Nfe300", lida, Leiaute.Danfe(lida));

        var antes = GC.GetAllocatedBytesForCurrentThread();
        var tempo = Stopwatch.StartNew();
        var bytes = RenderizadorPdf.Gerar(caso.Documento);
        tempo.Stop();
        var alocado = GC.GetAllocatedBytesForCurrentThread() - antes;
        AmostrasPdf.Gravar("danfe-300itens", bytes);

        using var pdf = new PdfLeitor(bytes);
        Assert.True(pdf.Paginas >= 5, $"300 itens em {pdf.Paginas} páginas");
        // O título da coluna uma vez por página, a folha k/n em cada uma e todos os itens, na ordem.
        for (var pagina = 1; pagina <= pdf.Paginas; pagina++)
        {
            Assert.Equal(1, Contar(pdf.TextoCompacto(pagina), "DESCRIÇÃODOSPRODUTOS/SERVIÇOS"));
            Assert.Contains($"FOLHA{pagina}/{pdf.Paginas}", pdf.TextoCompacto(pagina), StringComparison.Ordinal);
        }

        var texto = pdf.TextoCompacto();
        var ultima = -1;
        for (var i = 1; i <= 300; i++)
        {
            var posicao = texto.IndexOf($"P{i:0000}Produtodetestenúmero{i}", StringComparison.Ordinal);
            Assert.True(posicao > ultima, $"o item {i} não está na ordem");
            ultima = posicao;
        }

        Assert.True(tempo.Elapsed < TimeSpan.FromSeconds(10), $"300 itens em {tempo.Elapsed}");
        Assert.True(alocado < 1_500_000_000L, $"300 itens alocaram {alocado / 1_000_000} MB");
        Assert.True(bytes.Length < 5_000_000, $"o PDF de 300 itens tem {bytes.Length} bytes");
    }

    [Fact]
    public void OSimplificadoTipo2NoA4SaiNaFolhaComOsMesmosDados()
    {
        var lida = NotasDanfe.Autorizada(CatalogoImpressao.Tipo2(tributos: true));
        var caso = new CasoImpressao("Tipo2A4", lida, Leiaute.Danfe(lida, new OpcoesDanfe { Papel = PapelImpressao.A4Retrato }));

        using var pdf = new PdfLeitor(RenderizadorPdf.Gerar(caso.Documento));

        Assert.Equal(595.28, pdf.Pagina(1).Width, 0.5);
        Assert.Equal(841.89, pdf.Pagina(1).Height, 0.5);
        ConferirTextoModelo(caso, pdf);
        ConferirOQrCode(caso, pdf);
    }

    // ---- Todos juntos -----------------------------------------------------------------------------------------

    [Fact]
    public async Task DezesseisGeracoesSimultaneasDeNotasDiferentesDaoOMesmoTextoDaSequencial()
    {
        var casos = CatalogoImpressao.Danfces().Take(8).Concat(CatalogoImpressao.Danfes().Take(8)).ToArray();
        var sequencial = casos.Select(c => { using var p = new PdfLeitor(RenderizadorPdf.Gerar(c.Documento)); return (p.Paginas, p.TextoCompacto()); }).ToArray();

        var paralelo = await Task.WhenAll(casos.Select(c => Task.Run(() => RenderizadorPdf.Gerar(c.Documento))));

        Assert.Equal(16, paralelo.Length);
        for (var i = 0; i < casos.Length; i++)
        {
            using var lido = new PdfLeitor(paralelo[i]);
            Assert.Equal(sequencial[i].Paginas, lido.Paginas);
            Assert.Equal(sequencial[i].Item2, lido.TextoCompacto());
        }
    }

    [Fact]
    public void OPdfNaoMudaOQueOLeiauteMontou()
    {
        // O documento é imutável: gerar o PDF duas vezes dá o mesmo texto, e o texto de teste do modelo não muda.
        var caso = CatalogoImpressao.Danfes().Single(c => c.Nome == "Duplicatas");
        var antes = TextoPlano.Renderizar(caso.Documento);

        using var um = new PdfLeitor(RenderizadorPdf.Gerar(caso.Documento));
        using var dois = new PdfLeitor(RenderizadorPdf.Gerar(caso.Documento));

        Assert.Equal(um.TextoCompacto(), dois.TextoCompacto());
        Assert.Equal(antes, TextoPlano.Renderizar(caso.Documento));
    }

    [Fact]
    public void OCatalogoTemAsNotasQueAEtapaPromete()
    {
        Assert.True(CatalogoImpressao.Danfces().Count >= 15);
        Assert.True(CatalogoImpressao.Danfes().Count >= 20);
        Assert.Equal(CatalogoImpressao.Danfces().Count, CatalogoImpressao.Danfces().Select(c => c.Nome).Distinct().Count());
        Assert.Equal(CatalogoImpressao.Danfes().Count, CatalogoImpressao.Danfes().Select(c => c.Nome).Distinct().Count());
    }

    private static int Contar(string texto, string trecho)
    {
        var total = 0;
        for (var i = texto.IndexOf(trecho, StringComparison.Ordinal); i >= 0; i = texto.IndexOf(trecho, i + trecho.Length, StringComparison.Ordinal))
            total++;
        return total;
    }
}
