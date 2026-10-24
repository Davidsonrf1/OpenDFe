namespace OpenDFe.Impressao;

/// <summary>
/// A imagem de 1 bit por pixel (o logotipo do emitente e a assinatura do software emissor): a impressora térmica só
/// imprime preto.
/// </summary>
/// <remarks>
/// Imutável: os bits passados são copiados. Os bits são empacotados com o primeiro pixel da linha no bit mais
/// significativo do byte, cada linha em bytes inteiros (os bits que sobram no fim da linha são ignorados), e 1 é preto.
/// </remarks>
public sealed class Imagem1Bit
{
    private readonly byte[] _bits;

    /// <summary>Cria a imagem.</summary>
    /// <param name="largura">A largura em pixels, 1 ou mais.</param>
    /// <param name="altura">A altura em pixels, 1 ou mais.</param>
    /// <param name="bits">
    /// Os bits empacotados: <c>ceil(largura / 8) * altura</c> bytes, a linha de cima primeiro (a lista é copiada).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">A largura ou a altura é menor que 1.</exception>
    /// <exception cref="ArgumentException">O tamanho dos bits não é o da imagem.</exception>
    public Imagem1Bit(int largura, int altura, ReadOnlyMemory<byte> bits)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(largura, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(altura, 1);
        var esperado = (long)BytesLinha(largura) * altura;
        if (bits.Length != esperado)
        {
            throw new ArgumentException(
                $"Os bits de uma imagem {largura}x{altura} têm {esperado} bytes, e vieram {bits.Length}.", nameof(bits));
        }

        Largura = largura;
        Altura = altura;
        _bits = bits.ToArray();
    }

    /// <summary>A largura em pixels.</summary>
    public int Largura { get; }

    /// <summary>A altura em pixels.</summary>
    public int Altura { get; }

    /// <summary>Os bits empacotados (o primeiro pixel no bit mais significativo; 1 é preto).</summary>
    public ReadOnlyMemory<byte> Bits => _bits;

    /// <summary>
    /// Converte uma imagem de 8 bits por pixel (tons de cinza) em 1 bit: o pixel mais escuro que o limiar fica preto.
    /// </summary>
    /// <param name="cinza">Os pixels, linha por linha, <c>largura * altura</c> bytes (0 é preto e 255 é branco).</param>
    /// <param name="largura">A largura em pixels, 1 ou mais.</param>
    /// <param name="altura">A altura em pixels, 1 ou mais.</param>
    /// <param name="limiar">O corte: o pixel com valor menor que ele vira preto; 128 por padrão.</param>
    /// <returns>A imagem de 1 bit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A largura ou a altura é menor que 1.</exception>
    /// <exception cref="ArgumentException">O tamanho dos pixels não é o da imagem.</exception>
    public static Imagem1Bit Limiarizar(ReadOnlySpan<byte> cinza, int largura, int altura, byte limiar = 128)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(largura, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(altura, 1);
        if (cinza.Length != (long)largura * altura)
        {
            throw new ArgumentException(
                $"Uma imagem {largura}x{altura} em tons de cinza tem {(long)largura * altura} bytes, e vieram {cinza.Length}.",
                nameof(cinza));
        }

        var linha = BytesLinha(largura);
        var bits = new byte[linha * altura];
        for (var y = 0; y < altura; y++)
        {
            for (var x = 0; x < largura; x++)
            {
                if (cinza[y * largura + x] < limiar)
                    bits[y * linha + x / 8] |= (byte)(0x80 >> (x % 8));
            }
        }

        return new Imagem1Bit(largura, altura, bits);
    }

    /// <summary>Se o pixel é preto.</summary>
    /// <param name="x">A coluna, de 0 a <see cref="Largura"/> menos 1.</param>
    /// <param name="y">A linha, de 0 a <see cref="Altura"/> menos 1.</param>
    /// <returns><see langword="true"/> se o bit é 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">O pixel está fora da imagem.</exception>
    public bool Preto(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Largura);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Altura);
        return (_bits[y * BytesLinha(Largura) + x / 8] & (0x80 >> (x % 8))) != 0;
    }

    private static int BytesLinha(int largura) => (int)(((long)largura + 7) / 8);
}

/// <summary>Uma imagem de 1 bit no documento.</summary>
/// <remarks>Imutável.</remarks>
public sealed class ImagemImpressao : ElementoImpressao
{
    private readonly double? _larguraMm;

    /// <summary>Cria o elemento.</summary>
    /// <param name="imagem">A imagem.</param>
    /// <exception cref="ArgumentNullException">A imagem é nula.</exception>
    public ImagemImpressao(Imagem1Bit imagem)
    {
        ArgumentNullException.ThrowIfNull(imagem);
        Imagem = imagem;
    }

    /// <summary>A imagem.</summary>
    public Imagem1Bit Imagem { get; }

    /// <summary>O alinhamento; centralizado por padrão.</summary>
    public AlinhamentoImpressao Alinhamento { get; init; } = AlinhamentoImpressao.Centro;

    /// <summary>A largura impressa, em milímetros; <see langword="null"/> é o tamanho natural a 203 dpi.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A largura não é um número maior que zero.</exception>
    public double? LarguraMm
    {
        get => _larguraMm;
        init => _larguraMm = value is null ? null : QrCodeImpressao.ValidarMilimetros(value.Value, nameof(LarguraMm));
    }
}
