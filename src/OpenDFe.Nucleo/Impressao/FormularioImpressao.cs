namespace OpenDFe.Impressao;

/// <summary>As bordas de um <see cref="QuadroFormulario"/>: as linhas de 0,5 ponto em volta dele.</summary>
[Flags]
public enum BordaQuadro
{
    /// <summary>Sem borda.</summary>
    Nenhuma = 0,

    /// <summary>A linha de cima.</summary>
    Cima = 1,

    /// <summary>A linha de baixo.</summary>
    Baixo = 2,

    /// <summary>A linha da esquerda.</summary>
    Esquerda = 4,

    /// <summary>A linha da direita.</summary>
    Direita = 8,

    /// <summary>As quatro linhas.</summary>
    Todas = Cima | Baixo | Esquerda | Direita,
}

/// <summary>A cor do texto de um <see cref="TextoFormulario"/>.</summary>
public enum CorImpressao
{
    /// <summary>O preto sólido (K100).</summary>
    Preto,

    /// <summary>O vermelho sólido (M100/Y100): a "NFS-e SEM VALIDADE JURÍDICA" do DANFSe (NT 008/2026, 2.4.3).</summary>
    Vermelho,
}

/// <summary>
/// Um formulário de posição fixa: quadros com posição e tamanho em milímetros, a partir do canto superior esquerdo da área útil
/// do papel (o papel menos a margem), e a fonte em pontos (o DANFSe, NT 008/2026).
/// </summary>
/// <remarks>
/// O único elemento do modelo com coordenada e fonte (docs/NFSE.md 12, decisão G2): o modelo de impressão é estrutural
/// (docs/CONTRATOS.md 20), e o formulário existe para o documento cuja norma fixa a posição de cada campo. Ocupa a página
/// inteira e nunca se divide; um quadro fora da área útil é recusado por quem renderiza, que conhece o papel. Imutável: a
/// lista passada é copiada.
/// </remarks>
public sealed class FormularioImpressao : ElementoImpressao
{
    private readonly double _bordaPt;
    private readonly string? _marcaAgua;

    /// <summary>Cria o formulário.</summary>
    /// <param name="quadros">Os quadros, na ordem em que se desenham (o de depois por cima); a lista é copiada.</param>
    /// <exception cref="ArgumentNullException">A lista ou um quadro é nulo.</exception>
    public FormularioImpressao(IReadOnlyList<QuadroFormulario> quadros)
    {
        ArgumentNullException.ThrowIfNull(quadros);
        var copia = new QuadroFormulario[quadros.Count];
        for (var i = 0; i < copia.Length; i++)
            copia[i] = quadros[i] ?? throw new ArgumentNullException(nameof(quadros), "A lista tem um quadro nulo.");
        Quadros = Array.AsReadOnly(copia);
    }

    /// <summary>Os quadros, na ordem em que se desenham.</summary>
    public IReadOnlyList<QuadroFormulario> Quadros { get; }

    /// <summary>A espessura da borda da área útil, em pontos (1 no DANFSe, NT 008/2026 2.2.3); 0 sem borda.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A espessura é negativa ou não é um número.</exception>
    public double BordaPt
    {
        get => _bordaPt;
        init => _bordaPt = double.IsFinite(value) && value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "A borda tem de ser um número maior ou igual a zero.");
    }

    /// <summary>
    /// O texto em diagonal, atrás dos quadros (<c>CANCELADA</c>, <c>SUBSTITUÍDA</c>): sem negrito, com 50 pontos no mínimo, em
    /// cinza (K35), como pede a NT 008/2026 (2.5); <see langword="null"/> sem marca. É do formulário, e não do documento
    /// (<see cref="DocumentoImpressao.MarcaAgua"/>), porque o estilo é o da norma do formulário.
    /// </summary>
    /// <exception cref="ArgumentException">O texto é vazio ou só espaços.</exception>
    public string? MarcaAgua
    {
        get => _marcaAgua;
        init
        {
            if (value is not null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("A marca d'água não pode ser vazia.", nameof(value));
            _marcaAgua = value;
        }
    }
}

/// <summary>
/// Um quadro de um <see cref="FormularioImpressao"/>: a posição e o tamanho em milímetros, a partir do canto superior esquerdo
/// da área útil do papel, o fundo e as bordas.
/// </summary>
/// <remarks>O conjunto é fechado: só a biblioteca deriva (o construtor é <see langword="private protected"/>).</remarks>
public abstract class QuadroFormulario
{
    private protected QuadroFormulario(double xMm, double yMm, double larguraMm, double alturaMm)
    {
        XMm = Posicao(xMm, nameof(xMm));
        YMm = Posicao(yMm, nameof(yMm));
        LarguraMm = Medida(larguraMm, nameof(larguraMm));
        AlturaMm = Medida(alturaMm, nameof(alturaMm));
    }

    /// <summary>A distância da borda esquerda da área útil, em milímetros.</summary>
    public double XMm { get; }

    /// <summary>A distância do topo da área útil, em milímetros.</summary>
    public double YMm { get; }

    /// <summary>A largura, em milímetros.</summary>
    public double LarguraMm { get; }

    /// <summary>A altura, em milímetros.</summary>
    public double AlturaMm { get; }

    /// <summary>Se o fundo é cinza claro (5% de densidade, NT 008/2026 2.2.3); o fundo é branco por padrão.</summary>
    public bool Sombreado { get; init; }

    /// <summary>As linhas de 0,5 ponto em volta do quadro; nenhuma por padrão.</summary>
    public BordaQuadro Bordas { get; init; }

    private static double Posicao(double valor, string nome) => double.IsFinite(valor) && valor >= 0
        ? valor
        : throw new ArgumentOutOfRangeException(nome, valor, "A posição tem de ser um número maior ou igual a zero.");

    private protected static double Medida(double valor, string nome) => double.IsFinite(valor) && valor > 0
        ? valor
        : throw new ArgumentOutOfRangeException(nome, valor, "A medida tem de ser um número maior que zero.");
}

/// <summary>
/// Um quadro de texto: o rótulo em cima e o valor embaixo, ou só um dos dois. O texto que não cabe no quadro é cortado por
/// quem renderiza, com reticências: o formulário não cresce.
/// </summary>
/// <remarks>Imutável. A quebra de linha (o caractere de nova linha) no valor é uma quebra de linha no quadro.</remarks>
public sealed class TextoFormulario : QuadroFormulario
{
    private readonly double _tamanhoRotuloPt = 6;
    private readonly double _tamanhoValorPt = 7;

    /// <summary>Cria o quadro de texto.</summary>
    /// <param name="xMm">A distância da borda esquerda da área útil, em milímetros.</param>
    /// <param name="yMm">A distância do topo da área útil, em milímetros.</param>
    /// <param name="larguraMm">A largura, em milímetros.</param>
    /// <param name="alturaMm">A altura, em milímetros.</param>
    /// <param name="rotulo">O rótulo, já formatado; <see langword="null"/> sem rótulo.</param>
    /// <param name="valor">O valor, já formatado; <see langword="null"/> sem valor.</param>
    /// <exception cref="ArgumentOutOfRangeException">Uma posição é negativa, ou uma medida não é maior que zero.</exception>
    public TextoFormulario(double xMm, double yMm, double larguraMm, double alturaMm, string? rotulo, string? valor)
        : base(xMm, yMm, larguraMm, alturaMm)
    {
        Rotulo = rotulo;
        Valor = valor;
    }

    /// <summary>O rótulo; <see langword="null"/> sem rótulo.</summary>
    public string? Rotulo { get; }

    /// <summary>O valor; <see langword="null"/> sem valor.</summary>
    public string? Valor { get; }

    /// <summary>O tamanho do rótulo, em pontos; 6 por padrão (NT 008/2026 2.4.2).</summary>
    /// <exception cref="ArgumentOutOfRangeException">O tamanho não é um número maior que zero.</exception>
    public double TamanhoRotuloPt
    {
        get => _tamanhoRotuloPt;
        init => _tamanhoRotuloPt = Medida(value, nameof(value));
    }

    /// <summary>Se o rótulo está em negrito; verdadeiro por padrão.</summary>
    public bool RotuloNegrito { get; init; } = true;

    /// <summary>O tamanho do valor, em pontos; 7 por padrão (NT 008/2026 2.4.4).</summary>
    /// <exception cref="ArgumentOutOfRangeException">O tamanho não é um número maior que zero.</exception>
    public double TamanhoValorPt
    {
        get => _tamanhoValorPt;
        init => _tamanhoValorPt = Medida(value, nameof(value));
    }

    /// <summary>Se o valor está em negrito; falso por padrão.</summary>
    public bool ValorNegrito { get; init; }

    /// <summary>O alinhamento do rótulo e do valor; à esquerda por padrão.</summary>
    public AlinhamentoImpressao Alinhamento { get; init; }

    /// <summary>A cor do rótulo e do valor; preto por padrão.</summary>
    public CorImpressao Cor { get; init; }
}

/// <summary>
/// Um QR Code (nível M, UTF-8) num quadro quadrado: o símbolo ocupa o lado inteiro, e a zona de silêncio é o espaço em branco
/// em volta, que quem monta o formulário deixa.
/// </summary>
/// <remarks>Imutável.</remarks>
public sealed class QrCodeFormulario : QuadroFormulario
{
    /// <summary>Cria o quadro do QR Code.</summary>
    /// <param name="xMm">A distância da borda esquerda da área útil, em milímetros.</param>
    /// <param name="yMm">A distância do topo da área útil, em milímetros.</param>
    /// <param name="ladoMm">O lado, em milímetros.</param>
    /// <param name="texto">O conteúdo do QR Code.</param>
    /// <exception cref="ArgumentNullException">O texto é nulo.</exception>
    /// <exception cref="ArgumentException">O texto é vazio.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Uma posição é negativa, ou o lado não é maior que zero.</exception>
    public QrCodeFormulario(double xMm, double yMm, double ladoMm, string texto)
        : base(xMm, yMm, ladoMm, ladoMm)
    {
        ArgumentException.ThrowIfNullOrEmpty(texto);
        Texto = texto;
    }

    /// <summary>O conteúdo do QR Code.</summary>
    public string Texto { get; }
}

/// <summary>Uma imagem PNG num quadro, encaixada sem distorcer e centrada (a logomarca da NFS-e no DANFSe).</summary>
/// <remarks>Imutável: os bytes são copiados.</remarks>
public sealed class ImagemFormulario : QuadroFormulario
{
    private static ReadOnlySpan<byte> AssinaturaPng => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>Cria o quadro da imagem.</summary>
    /// <param name="xMm">A distância da borda esquerda da área útil, em milímetros.</param>
    /// <param name="yMm">A distância do topo da área útil, em milímetros.</param>
    /// <param name="larguraMm">A largura, em milímetros.</param>
    /// <param name="alturaMm">A altura, em milímetros.</param>
    /// <param name="png">Os bytes do arquivo PNG (copiados).</param>
    /// <exception cref="ArgumentException">Os bytes não começam pela assinatura do PNG.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Uma posição é negativa, ou uma medida não é maior que zero.</exception>
    public ImagemFormulario(double xMm, double yMm, double larguraMm, double alturaMm, ReadOnlyMemory<byte> png)
        : base(xMm, yMm, larguraMm, alturaMm)
    {
        if (!png.Span.StartsWith(AssinaturaPng))
            throw new ArgumentException("A imagem não é um PNG (falta a assinatura do formato).", nameof(png));
        Png = png.ToArray();
    }

    /// <summary>Os bytes do arquivo PNG.</summary>
    public ReadOnlyMemory<byte> Png { get; }
}
