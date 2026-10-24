using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// O que o desenho de um documento consulta a todo momento: o documento, o tipo de papel, a largura útil e a escala de fonte.
/// Um por geração, só leitura: dois PDFs em paralelo não compartilham nada.
/// </summary>
internal sealed class Contexto
{
    private readonly string[]? _familias;

    public Contexto(DocumentoImpressao documento, OpcoesPdf opcoes)
    {
        Documento = documento;
        Folha = documento.Papel.Tipo == TipoPapel.Folha;
        LarguraUtilMm = documento.Papel.LarguraMm - 2 * documento.Papel.MargemMm;
        // A família pedida, se o aplicativo a registrou (ou o sistema a tem); senão a Lato, a embutida: a emissão não quebra
        // porque a fonte não está instalada (o QuestPDF lança no texto de uma família que não existe).
        _familias = string.IsNullOrWhiteSpace(opcoes.Fonte) || !FamiliaDisponivel(opcoes.Fonte) ? null : [opcoes.Fonte];
    }

    // As fontes que o QuestPDF usa: as registradas (pelo aplicativo ou na pasta de descoberta) e, se o aplicativo ligou
    // Settings.UseSystemFonts, as do sistema. O nome da família não distingue maiúsculas (documentação do QuestPDF).
    private static bool FamiliaDisponivel(string familia) =>
        FontManager.GetRegisteredFonts().Concat(QuestPDF.Settings.UseSystemFonts ? FontManager.GetSystemFonts() : [])
            .Any(fonte => string.Equals(fonte.FamilyName, familia, StringComparison.OrdinalIgnoreCase));

    /// <summary>O documento de impressão.</summary>
    public DocumentoImpressao Documento { get; }

    /// <summary>Se o papel é a folha (o A4); a bobina é falso.</summary>
    public bool Folha { get; }

    /// <summary>A largura da área impressa, em milímetros (o papel menos as duas margens).</summary>
    public double LarguraUtilMm { get; }

    /// <summary>O tamanho da fonte do texto do modelo, em pontos (docs/CONTRATOS.md 22.3).</summary>
    public float Tamanho(TamanhoTexto tamanho) => (Folha, tamanho) switch
    {
        (false, TamanhoTexto.Pequeno) => Medidas.BobinaPequeno,
        (false, TamanhoTexto.Normal) => Medidas.BobinaNormal,
        (false, TamanhoTexto.Duplo) => Medidas.BobinaDuplo,
        (true, TamanhoTexto.Pequeno) => Medidas.FolhaPequeno,
        (true, TamanhoTexto.Normal) => Medidas.FolhaNormal,
        (true, TamanhoTexto.Duplo) => Medidas.FolhaDuplo,
        _ => throw new ArgumentOutOfRangeException(nameof(tamanho), tamanho, "O tamanho do texto não é um dos definidos."),
    };

    /// <summary>O estilo padrão da página: a família pedida, o tamanho normal e o preto.</summary>
    public TextStyle EstiloBase(TextStyle estilo)
    {
        var basico = estilo.FontSize(Tamanho(TamanhoTexto.Normal)).LineHeight(Medidas.Entrelinha).FontColor(Colors.Black);
        return _familias is null ? basico : basico.FontFamily(_familias);
    }

    /// <summary>Os elementos do documento, em trechos: a bobina corta em cada quebra de página, a folha é um trecho só.</summary>
    public IEnumerable<IReadOnlyList<ElementoImpressao>> Trechos()
    {
        if (Folha)
        {
            yield return Documento.Elementos;
            yield break;
        }

        var atual = new List<ElementoImpressao>();
        var emitiu = false;
        foreach (var elemento in Documento.Elementos)
        {
            if (elemento is QuebraPaginaImpressao)
            {
                if (atual.Count > 0)
                {
                    yield return atual;
                    emitiu = true;
                    atual = [];
                }

                continue;
            }

            atual.Add(elemento);
        }

        // O documento sem nenhum elemento sai com uma página em branco (o PDF precisa de ao menos uma).
        if (atual.Count > 0 || !emitiu)
            yield return atual;
    }
}
