namespace OpenDFe.Impressao;

/// <summary>O tipo do papel: a bobina (altura contínua) ou a folha (altura fixa, várias páginas).</summary>
public enum TipoPapel
{
    /// <summary>A bobina da impressora térmica: a largura é fixa e a altura acompanha o conteúdo.</summary>
    Bobina,

    /// <summary>A folha, de largura e altura fixas, com quebra em páginas.</summary>
    Folha,
}

/// <summary>
/// O papel do documento de impressão. As instâncias são as fábricas estáticas: não há como criar outro papel.
/// </summary>
/// <remarks>
/// Imutável. Na bobina, as colunas são as de caracteres da fonte A da impressora térmica (12 pontos de largura a
/// 203 dpi): 48 em 80 mm e 32 em 58 mm.
/// </remarks>
public sealed class PapelImpressao
{
    private PapelImpressao(TipoPapel tipo, double larguraMm, double? alturaMm, int colunas, double margemMm)
    {
        Tipo = tipo;
        LarguraMm = larguraMm;
        AlturaMm = alturaMm;
        Colunas = colunas;
        MargemMm = margemMm;
    }

    /// <summary>A bobina de 80 mm: 48 colunas de caracteres, margem lateral de 4 mm (72 mm de área impressa).</summary>
    public static PapelImpressao Bobina80 { get; } = new(TipoPapel.Bobina, 80, null, 48, 4);

    /// <summary>A bobina de 58 mm: 32 colunas de caracteres, margem lateral de 5 mm (48 mm de área impressa).</summary>
    public static PapelImpressao Bobina58 { get; } = new(TipoPapel.Bobina, 58, null, 32, 5);

    /// <summary>A folha A4 em pé, 210 x 297 mm, com margem de 10 mm.</summary>
    public static PapelImpressao A4Retrato { get; } = new(TipoPapel.Folha, 210, 297, 0, 10);

    /// <summary>A folha A4 deitada, 297 x 210 mm, com margem de 10 mm.</summary>
    public static PapelImpressao A4Paisagem { get; } = new(TipoPapel.Folha, 297, 210, 0, 10);

    /// <summary>Se é bobina ou folha.</summary>
    public TipoPapel Tipo { get; }

    /// <summary>A largura do papel, em milímetros.</summary>
    public double LarguraMm { get; }

    /// <summary>A altura do papel, em milímetros; <see langword="null"/> na bobina.</summary>
    public double? AlturaMm { get; }

    /// <summary>Na bobina, as colunas de caracteres da fonte normal; na folha, 0.</summary>
    public int Colunas { get; }

    /// <summary>A margem lateral, em milímetros.</summary>
    public double MargemMm { get; }

    /// <summary>
    /// As colunas de caracteres que cabem numa linha no tamanho do texto: a fonte normal tem <see cref="Colunas"/>; a
    /// pequena (fonte B, 9 pontos de largura) tem 4/3 delas, arredondado para baixo; a dupla (largura dobrada) tem a metade.
    /// </summary>
    /// <param name="tamanho">O tamanho do texto.</param>
    /// <returns>As colunas; 0 na folha, que não tem colunas.</returns>
    /// <exception cref="ArgumentOutOfRangeException">O tamanho não é um dos definidos.</exception>
    public int ColunasTamanho(TamanhoTexto tamanho) => tamanho switch
    {
        TamanhoTexto.Normal => Colunas,
        TamanhoTexto.Pequeno => Colunas * 4 / 3,
        TamanhoTexto.Duplo => Colunas / 2,
        _ => throw new ArgumentOutOfRangeException(nameof(tamanho), tamanho, "O tamanho do texto não é um dos definidos."),
    };
}
