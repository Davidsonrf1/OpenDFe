using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

// A pilha de elementos, o texto, a linha de células e o separador (docs/CONTRATOS.md 22.2).
public static partial class RenderizadorPdf
{
    [GeneratedRegex(@"(\{paginas?\})")]
    private static partial Regex Fichas();

    // Os elementos de cima para baixo, na ordem do documento. A coluna do QuestPDF divide entre os elementos, nunca dentro
    // de um quadro (cada quadro se pede indivisível onde a norma exige).
    private static void Empilhar(IContainer container, IReadOnlyList<ElementoImpressao> elementos, Contexto contexto)
    {
        container.Column(coluna =>
        {
            if (elementos.Count == 0)
            {
                // A coluna sem item não gera página: o documento vazio sai com uma linha em branco.
                coluna.Item().Text(" ");
                return;
            }

            foreach (var elemento in elementos)
                Elemento(coluna.Item(), elemento, contexto);
        });
    }

    private static void Elemento(IContainer item, ElementoImpressao elemento, Contexto contexto)
    {
        switch (elemento)
        {
            case TextoImpressao texto:
                Escrever(item, texto.Texto, contexto.Tamanho(texto.Tamanho), texto.Negrito, texto.Alinhamento, texto.Invertido);
                break;
            case LinhaImpressao linha:
                Linha(item, linha, contexto);
                break;
            case SeparadorImpressao separador:
                Separador(item, separador);
                break;
            case EspacoImpressao espaco:
                item.Height(espaco.Linhas * contexto.Tamanho(TamanhoTexto.Normal) * Medidas.Entrelinha);
                break;
            case QrCodeImpressao qr:
                QrCode(item, qr);
                break;
            case CodigoBarrasImpressao barras:
                CodigoBarras(item, barras, contexto);
                break;
            case ImagemImpressao imagem:
                Imagem(item, imagem, contexto);
                break;
            case SecaoImpressao secao:
                Secao(item, secao, contexto);
                break;
            case TabelaImpressao tabela:
                Tabela(item, tabela, contexto);
                break;
            case QuebraPaginaImpressao:
                // Na folha, a quebra de página; na bobina, o corte já separou os trechos (Contexto.Trechos).
                item.PageBreak();
                break;
            default:
                throw new NotSupportedException($"O renderizador PDF não trata o elemento {elemento.GetType().Name}.");
        }
    }

    // Um texto com o estilo dado. Aceita as fichas {pagina} e {paginas}: o número da página do QuestPDF, que vale em cada
    // página, inclusive na primeira e no cabeçalho de continuação.
    private static void Escrever(IContainer container, string texto, float tamanho, bool negrito, AlinhamentoImpressao alinhamento,
        bool invertido = false)
    {
        var seguro = TextoSeguro.Limpar(texto);
        // Linha vazia do modelo é uma linha em branco do papel: o espaço dá a ela a altura da linha.
        if (seguro.Length == 0)
            seguro = " ";
        var fundo = invertido ? container.Background(Colors.Black) : container;
        fundo.Text(descritor =>
        {
            descritor.DefaultTextStyle(estilo =>
            {
                var novo = estilo.FontSize(tamanho);
                if (negrito)
                    novo = novo.Bold();
                return invertido ? novo.FontColor(Colors.White) : novo;
            });
            Alinhar(descritor, alinhamento);
            foreach (var parte in Fichas().Split(seguro))
            {
                if (parte.Length == 0)
                    continue;
                if (parte == TextoPaginado.Pagina)
                    descritor.CurrentPageNumber();
                else if (parte == TextoPaginado.Paginas)
                    descritor.TotalPages();
                else
                    descritor.Span(parte);
            }
        });
    }

    private static void Alinhar(TextDescriptor descritor, AlinhamentoImpressao alinhamento)
    {
        switch (alinhamento)
        {
            case AlinhamentoImpressao.Centro:
                descritor.AlignCenter();
                break;
            case AlinhamentoImpressao.Direita:
                descritor.AlignRight();
                break;
            default:
                descritor.AlignLeft();
                break;
        }
    }

    // A linha de células: cada uma com a largura proporcional ao peso (o PDV usa LinhaImpressao.Larguras em colunas). A fonte do
    // PDF não é de largura fixa (a térmica é): a última célula alinhada à direita (o valor, o total) toma a largura do
    // conteúdo e a de texto cede o resto, para o valor nunca quebrar por causa do peso. Texto longo quebra dentro da célula
    // e a linha cresce: nada é cortado.
    private static void Linha(IContainer item, LinhaImpressao linha, Contexto contexto)
    {
        var tamanho = contexto.Tamanho(TamanhoTexto.Normal);
        var ultima = linha.Celulas.Count - 1;
        item.Row(fileira =>
        {
            for (var i = 0; i < linha.Celulas.Count; i++)
            {
                var celula = linha.Celulas[i];
                var coluna = i == ultima && i > 0 && celula.Alinhamento == AlinhamentoImpressao.Direita
                    ? fileira.AutoItem()
                    : fileira.RelativeItem((float)celula.Peso);
                Escrever(coluna, celula.Texto, tamanho, celula.Negrito, celula.Alinhamento);
            }
        });
    }

    // O separador: tracejado (o "------" da térmica) ou cheio (o "======").
    private static void Separador(IContainer item, SeparadorImpressao separador)
    {
        var linha = item.PaddingVertical(0.8f, Unit.Millimetre).LineHorizontal(separador.Estilo == EstiloSeparador.Cheio ? 1f : 0.6f);
        if (separador.Estilo == EstiloSeparador.Tracejado)
            linha.LineDashPattern([2.5f, 2f]);
    }
}
