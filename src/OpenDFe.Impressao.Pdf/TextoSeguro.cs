using System.Globalization;
using System.Text;

namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// O texto que a fonte do PDF desenha. A emissão nunca pode quebrar por causa de um caractere no nome do produto: o
/// QuestPDF lança se faltar o glifo, então todo texto passa por aqui e o que a fonte não tem vira o equivalente ASCII ou
/// <c>?</c> (docs/CONTRATOS.md 22.4).
/// </summary>
internal static class TextoSeguro
{
    /// <summary>O substituto do caractere que a fonte não tem e que não tem equivalente.</summary>
    public const char Desconhecido = '?';

    // O caractere que se tira do texto (os de formatação invisíveis: espaço de largura zero, marcas de direção).
    private const char Nada = '\0';

    /// <summary>
    /// O texto pronto para a fonte: quebras de linha em <c>\n</c>, os controles trocados por espaço, os de formatação
    /// invisíveis tirados e os caracteres sem glifo trocados pelo equivalente ASCII ou por <c>?</c>.
    /// </summary>
    public static string Limpar(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        var saida = new StringBuilder(texto.Length);
        for (var i = 0; i < texto.Length; i++)
        {
            var c = texto[i];
            if (c == '\r')
            {
                if (i + 1 < texto.Length && texto[i + 1] == '\n')
                    continue;
                saida.Append('\n');
            }
            else if (c == '\n')
            {
                saida.Append('\n');
            }
            else if (char.IsHighSurrogate(c) && i + 1 < texto.Length && char.IsLowSurrogate(texto[i + 1]))
            {
                // Fora do plano básico (emoji, ideogramas raros): a fonte não tem.
                saida.Append(Desconhecido);
                i++;
            }
            else
            {
                var troca = Trocar(c);
                if (troca != Nada)
                    saida.Append(troca);
            }
        }

        return saida.ToString();
    }

    /// <summary>
    /// Se a fonte embutida (Lato) desenha o caractere. O conjunto foi conferido caractere a caractere contra a Lato do QuestPDF
    /// 2026.9.1 (docs/CONTRATOS.md 22.4): o ASCII, o Latin-1 inteiro (o schema da NF-e só aceita até U+00FF), o Latin Extended-A e
    /// B, e a pontuação geral que uma nota digitada pode trazer.
    /// </summary>
    public static bool Aceito(char c) => c switch
    {
        >= ' ' and <= '~' => true,
        >= ' ' and <= 'ɏ' => true,
        >= '\x2010' and <= '\x2022' => true,    // hifens, travessões, aspas, adagas e o marcador
        '\x2026' or '\x2030' or '\x2039' or '\x203A' or '\x20AC' or '\x2122' or '\x2212' => true,
        _ => false,
    };

    private static char Trocar(char c)
    {
        if (Aceito(c))
            return c;
        if (Equivalente(c) is { } equivalente)
            return equivalente;
        if (char.IsControl(c))
            return ' ';
        return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format ? Nada : Desconhecido;
    }

    // Os equivalentes ASCII dos sinais que a fonte não desenha.
    private static char? Equivalente(char c) => c switch
    {
        '\x2023' or '\x2043' => '-',
        '\x2027' => '.',
        '\x2044' => '/',
        '\x2000' or '\x2001' or '\x2002' or '\x2003' or '\x2004' or '\x2005' or '\x2006' or '\x2007' or '\x2008' or '\x2009'
            or '\x200A' or '\x202F' or '\x205F' or '\x3000' => ' ',
        '\x2028' or '\x2029' => '\n',
        _ => null,
    };
}
