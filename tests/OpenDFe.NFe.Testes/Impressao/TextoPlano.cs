using System.Text;
using OpenDFe.Impressao;

namespace OpenDFe.Testes;

/// <summary>
/// O renderizador <b>só de teste</b> do modelo de impressão: converte o documento da bobina em texto puro, na largura das
/// colunas do papel, para os golden files do DANFCE (etapa 7.2). Cada linha começa com três marcas e uma barra
/// (<c>B</c> negrito, <c>P</c> pequeno ou <c>D</c> duplo, <c>I</c> invertido), depois o texto alinhado nas colunas do papel.
/// Não é a lib: o PDV (ESC/POS) e o PDF fazem o desenho de verdade.
/// </summary>
internal static class TextoPlano
{
    internal static string Renderizar(DocumentoImpressao documento)
    {
        var colunas = documento.Papel.Colunas;
        var saida = new StringBuilder();
        foreach (var elemento in documento.Elementos)
        {
            foreach (var (marcas, texto) in Linhas(elemento, documento.Papel, colunas))
                saida.Append(marcas).Append('|').Append(texto.TrimEnd()).Append('\n');
        }

        return saida.ToString();
    }

    private static IEnumerable<(string Marcas, string Texto)> Linhas(ElementoImpressao elemento, PapelImpressao papel, int colunas)
    {
        switch (elemento)
        {
            case TextoImpressao texto:
                var largura = papel.ColunasTamanho(texto.Tamanho);
                var marcas = Marcas(texto.Negrito, texto.Tamanho, texto.Invertido);
                foreach (var linha in texto.Quebrar(largura))
                    yield return (marcas, Alinhar(linha, colunas, texto.Alinhamento, linha.Length * colunas / (double)largura));
                break;
            case LinhaImpressao linha:
                foreach (var l in LinhasCelulas(linha, colunas))
                    yield return l;
                break;
            case SeparadorImpressao separador:
                yield return (Marcas(false, TamanhoTexto.Normal, false), new string(separador.Estilo == EstiloSeparador.Cheio ? '=' : '-', colunas));
                break;
            case EspacoImpressao espaco:
                for (var i = 0; i < espaco.Linhas; i++)
                    yield return (Marcas(false, TamanhoTexto.Normal, false), string.Empty);
                break;
            case QrCodeImpressao qr:
                yield return (Marcas(false, TamanhoTexto.Normal, false), $"[QR {qr.LadoMm.ToString(System.Globalization.CultureInfo.InvariantCulture)}mm: {qr.Texto}]");
                break;
            case CodigoBarrasImpressao barras:
                yield return (Marcas(false, TamanhoTexto.Normal, false), $"[BARRAS: {barras.Digitos}]");
                break;
            case ImagemImpressao imagem:
                yield return (Marcas(false, TamanhoTexto.Normal, false),
                    Alinhar($"[IMAGEM {imagem.Imagem.Largura}x{imagem.Imagem.Altura}]", colunas, imagem.Alinhamento));
                break;
            case QuebraPaginaImpressao:
                yield return (Marcas(false, TamanhoTexto.Normal, false), "[QUEBRA DE PÁGINA]");
                break;
            default:
                throw new NotSupportedException($"O renderizador de teste não trata {elemento.GetType().Name}.");
        }
    }

    private static IEnumerable<(string Marcas, string Texto)> LinhasCelulas(LinhaImpressao linha, int colunas)
    {
        var larguras = linha.Larguras(colunas);
        var blocos = new List<IReadOnlyList<string>>();
        for (var i = 0; i < linha.Celulas.Count; i++)
            blocos.Add(larguras[i] == 0 ? [] : linha.Celulas[i].Quebrar(larguras[i]));

        var altura = blocos.Max(b => b.Count);
        var marcas = Marcas(linha.Celulas.Any(c => c.Negrito), TamanhoTexto.Normal, false);
        for (var y = 0; y < altura; y++)
        {
            var texto = new StringBuilder();
            for (var i = 0; i < linha.Celulas.Count; i++)
            {
                var conteudo = y < blocos[i].Count ? blocos[i][y] : string.Empty;
                texto.Append(Alinhar(conteudo, larguras[i], linha.Celulas[i].Alinhamento).PadRight(larguras[i]));
            }

            yield return (marcas, texto.ToString());
        }
    }

    // A largura visual é a que o texto ocupa no papel: o tamanho duplo ocupa o dobro das colunas, e o pequeno, menos.
    private static string Alinhar(string texto, int largura, AlinhamentoImpressao alinhamento, double? visual = null)
    {
        var folga = Math.Max(0, (int)Math.Round(largura - (visual ?? texto.Length), MidpointRounding.ToZero));
        return alinhamento switch
        {
            AlinhamentoImpressao.Centro => new string(' ', folga / 2) + texto,
            AlinhamentoImpressao.Direita => new string(' ', folga) + texto,
            _ => texto,
        };
    }

    private static string Marcas(bool negrito, TamanhoTexto tamanho, bool invertido) =>
        (negrito ? "B" : " ") + (tamanho == TamanhoTexto.Pequeno ? "P" : tamanho == TamanhoTexto.Duplo ? "D" : " ") + (invertido ? "I" : " ");
}
