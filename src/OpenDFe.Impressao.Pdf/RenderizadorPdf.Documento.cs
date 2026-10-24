using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

// A estrutura do PDF: o papel, as páginas, o cabeçalho de continuação, a marca d'água e os metadados (docs/CONTRATOS.md 22.2).
public static partial class RenderizadorPdf
{
    // As duas sobrecargas de Gerar têm o parâmetro opcional (a API da decisão 1 da etapa 7.4). O RS0026 pede cuidado com
    // sobrecargas opcionais por compatibilidade futura; aqui elas diferem no tipo do segundo argumento (OpcoesPdf ou Stream),
    // e a API ainda não foi publicada (Unshipped), então o desenho fica como aprovado.
#pragma warning disable RS0026
    /// <summary>
    /// Renderiza o documento de impressão em PDF, em memória: a bobina sai com a largura do papel e altura contínua; a folha
    /// A4 sai paginada, com a margem do papel (docs/CONTRATOS.md 22).
    /// </summary>
    /// <param name="documento">O documento de impressão (o DANFCE ou o DANFE montado pelo leiaute).</param>
    /// <param name="opcoes">Os metadados e a fonte; <see langword="null"/> usa o padrão.</param>
    /// <returns>Os bytes do PDF.</returns>
    /// <exception cref="ArgumentNullException">O documento é nulo.</exception>
    /// <exception cref="InvalidOperationException">A licença do QuestPDF não foi definida; a mensagem aponta o README do pacote.</exception>
    public static byte[] Gerar(DocumentoImpressao documento, OpcoesPdf? opcoes = null)
    {
        ArgumentNullException.ThrowIfNull(documento);
        LicencaQuestPdf.Exigir();
        return Montar(documento, opcoes ?? new OpcoesPdf()).GeneratePdf();
    }

    /// <summary>
    /// Renderiza o documento de impressão em PDF, no fluxo dado (o fluxo não é fechado nem reposicionado).
    /// </summary>
    /// <param name="documento">O documento de impressão.</param>
    /// <param name="destino">O fluxo onde o PDF é escrito.</param>
    /// <param name="opcoes">Os metadados e a fonte; <see langword="null"/> usa o padrão.</param>
    /// <exception cref="ArgumentNullException">O documento ou o fluxo é nulo.</exception>
    /// <exception cref="InvalidOperationException">A licença do QuestPDF não foi definida; a mensagem aponta o README do pacote.</exception>
    public static void Gerar(DocumentoImpressao documento, Stream destino, OpcoesPdf? opcoes = null)
    {
        ArgumentNullException.ThrowIfNull(documento);
        ArgumentNullException.ThrowIfNull(destino);
        LicencaQuestPdf.Exigir();
        Montar(documento, opcoes ?? new OpcoesPdf()).GeneratePdf(destino);
    }
#pragma warning restore RS0026

    // O documento do QuestPDF (interno: os testes o usam para gerar as imagens de inspeção).
    internal static IDocument Montar(DocumentoImpressao documento, OpcoesPdf opcoes)
    {
        var contexto = new Contexto(documento, opcoes);
        var metadados = new DocumentMetadata
        {
            Title = TextoSeguro.Limpar(opcoes.Titulo ?? documento.Titulo ?? string.Empty),
            Author = opcoes.Autor is null ? string.Empty : TextoSeguro.Limpar(opcoes.Autor),
            Creator = TextoSeguro.Limpar(opcoes.Criador ?? string.Empty),
            Language = "pt-BR",
        };

        return Document.Create(pdf =>
        {
            // A bobina corta em cada QuebraPaginaImpressao: cada trecho é uma página de altura contínua (um bilhete).
            // A folha tem uma só sequência de páginas, e a quebra vira quebra de página dentro dela.
            foreach (var trecho in contexto.Trechos())
            {
                pdf.Page(pagina =>
                {
                    ConfigurarPapel(pagina, contexto);
                    pagina.DefaultTextStyle(estilo => contexto.EstiloBase(estilo));
                    if (documento.MarcaAgua is { } marca)
                        pagina.Background().Element(fundo => MarcaAgua(fundo, marca, contexto));
                    if (documento.CabecalhoContinuacao.Count > 0)
                    {
                        pagina.Header().ShowIf(situacao => situacao.PageNumber > 1)
                            .Element(topo => Empilhar(topo, documento.CabecalhoContinuacao, contexto));
                    }

                    pagina.Content().Element(corpo => Empilhar(corpo, trecho, contexto));
                });
            }
        }).WithMetadata(metadados);
    }

    private static void ConfigurarPapel(PageDescriptor pagina, Contexto contexto)
    {
        var papel = contexto.Documento.Papel;
        if (papel.Tipo == TipoPapel.Bobina)
        {
            // Largura do papel e altura contínua (ContinuousSize): o bilhete cresce com o conteúdo, sem corte de página.
            pagina.ContinuousSize((float)papel.LarguraMm, Unit.Millimetre);
            pagina.MarginHorizontal((float)papel.MargemMm, Unit.Millimetre);
            pagina.MarginVertical(Medidas.BobinaMargemVerticalMm, Unit.Millimetre);
        }
        else
        {
            // A folha: as dimensões do papel (o A4 em pé ou deitado, 210 x 297 mm) e a margem dele (Anexo II 3.6.2).
            var altura = papel.AlturaMm ?? throw new InvalidOperationException("O papel em folha tem altura.");
            pagina.Size(new PageSize((float)(papel.LarguraMm * Medidas.PontosMilimetro), (float)(altura * Medidas.PontosMilimetro)));
            pagina.Margin((float)papel.MargemMm, Unit.Millimetre);
        }
    }

    // A marca d'água: texto cinza muito claro em diagonal, atrás do conteúdo, em todas as páginas. O QuestPDF gira o elemento em
    // torno do centro dele (conferido com o PdfPig: as letras giram em torno do centro da caixa), então a caixa centrada na
    // página continua centrada depois de girar. A caixa tem largura de sobra para o texto nunca quebrar em duas linhas.
    private static void MarcaAgua(IContainer fundo, string marca, Contexto contexto)
    {
        var texto = TextoSeguro.Limpar(marca);
        // A caixa do texto não pode ser mais larga que a área impressa (o QuestPDF não desenha o que não cabe no fundo): cada letra
        // maiúscula em negrito ocupa até 0,8 do tamanho, então o tamanho é o que põe o texto todo na largura útil.
        var larguraUtilPt = (float)(contexto.LarguraUtilMm * Medidas.PontosMilimetro);
        var tamanho = Math.Min(Medidas.MarcaTamanhoMaximo, larguraUtilPt / (Math.Max(1, texto.Length) * Medidas.MarcaLarguraLetra));
        fundo.AlignCenter().AlignMiddle().Width(Math.Max(1, texto.Length) * tamanho * Medidas.MarcaLarguraLetra).Height(tamanho * 1.4f)
            .Rotate(Medidas.MarcaAnguloGraus).AlignCenter().AlignMiddle()
            .Text(texto).FontSize(tamanho).Bold().FontColor(Color.FromHex(Medidas.MarcaCor));
    }
}
