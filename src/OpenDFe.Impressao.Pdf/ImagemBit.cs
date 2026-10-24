namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// Imagem de 1 bit: a representação interna da imagem do modelo de impressão (logotipo, QR Code). Os bits vêm
/// empacotados por linha, o mais significativo primeiro, e cada linha começa num byte novo (os bits que sobram no
/// fim da linha são ignorados). 1 é preto, 0 é branco.
/// </summary>
internal sealed class ImagemBit
{
    /// <summary>Cria a imagem; recusa dimensão não positiva e quantidade de bytes diferente da esperada.</summary>
    public ImagemBit(int largura, int altura, byte[] bits)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(largura, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(altura, 1);
        ArgumentNullException.ThrowIfNull(bits);
        var esperado = (long)BytesLinha(largura) * altura;
        if (bits.Length != esperado)
            throw new ArgumentException($"A imagem {largura}x{altura} tem {esperado} bytes, e vieram {bits.Length}.", nameof(bits));
        Largura = largura;
        Altura = altura;
        Bits = bits;
    }

    /// <summary>Largura em pixels.</summary>
    public int Largura { get; }

    /// <summary>Altura em pixels.</summary>
    public int Altura { get; }

    /// <summary>Os bits empacotados, linha a linha.</summary>
    public byte[] Bits { get; }

    /// <summary>Quantos bytes ocupa cada linha de <paramref name="largura"/> pixels.</summary>
    public static int BytesLinha(int largura) => (largura + 7) / 8;

    /// <summary>Monta a imagem a partir de uma matriz de pixels (<c>true</c> é preto), indexada [linha, coluna].</summary>
    public static ImagemBit Criar(bool[,] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        var altura = pixels.GetLength(0);
        var largura = pixels.GetLength(1);
        var porLinha = BytesLinha(largura);
        var bits = new byte[porLinha * altura];
        for (var y = 0; y < altura; y++)
            for (var x = 0; x < largura; x++)
                if (pixels[y, x])
                    bits[y * porLinha + (x >> 3)] |= (byte)(0x80 >> (x & 7));
        return new ImagemBit(largura, altura, bits);
    }

    /// <summary>Se o pixel da coluna <paramref name="x"/> e da linha <paramref name="y"/> é preto.</summary>
    public bool Preto(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Largura);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Altura);
        return (Bits[y * BytesLinha(Largura) + (x >> 3)] & (0x80 >> (x & 7))) != 0;
    }
}
