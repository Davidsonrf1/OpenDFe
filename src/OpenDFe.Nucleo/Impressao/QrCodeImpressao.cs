namespace OpenDFe.Impressao;

/// <summary>
/// O QR Code. O renderizador o desenha com a correção de erro nível M e o texto em UTF-8 (o Manual de Padrões Técnicos
/// do DANFE NFC-e e QR Code, item 4.5).
/// </summary>
/// <remarks>Imutável.</remarks>
public sealed class QrCodeImpressao : ElementoImpressao
{
    private double _ladoMm = 25;

    /// <summary>Cria o QR Code.</summary>
    /// <param name="texto">O texto codificado (a URL do QR Code da NFC-e).</param>
    /// <exception cref="ArgumentNullException">O texto é nulo.</exception>
    /// <exception cref="ArgumentException">O texto é vazio.</exception>
    public QrCodeImpressao(string texto)
    {
        ArgumentException.ThrowIfNullOrEmpty(texto);
        Texto = texto;
    }

    /// <summary>O texto codificado.</summary>
    public string Texto { get; }

    /// <summary>
    /// O lado do quadrado, em milímetros, com a margem de silêncio dentro dele (o manual da NFC-e pede no mínimo 25 mm,
    /// dos quais 22 mm de conteúdo e 3 mm de margem); 25 por padrão.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">O lado não é um número maior que zero.</exception>
    public double LadoMm
    {
        get => _ladoMm;
        init => _ladoMm = ValidarMilimetros(value, nameof(LadoMm));
    }

    /// <summary>O alinhamento; centralizado por padrão.</summary>
    public AlinhamentoImpressao Alinhamento { get; init; } = AlinhamentoImpressao.Centro;

    internal static double ValidarMilimetros(double valor, string nome)
    {
        if (!(valor > 0) || double.IsInfinity(valor))
            throw new ArgumentOutOfRangeException(nome, valor, "A medida deve ser um número maior que zero.");
        return valor;
    }
}

/// <summary>
/// O código de barras Code 128, conjunto C (pares de dígitos): a chave de acesso de 44 dígitos no DANFE.
/// </summary>
/// <remarks>Imutável.</remarks>
public sealed class CodigoBarrasImpressao : ElementoImpressao
{
    private double _alturaMm = 10;

    /// <summary>Cria o código de barras.</summary>
    /// <param name="digitos">Os dígitos, em quantidade par, ao menos dois.</param>
    /// <exception cref="ArgumentNullException">Os dígitos são nulos.</exception>
    /// <exception cref="ArgumentException">Vazio, em quantidade ímpar ou com caractere que não é dígito de 0 a 9.</exception>
    public CodigoBarrasImpressao(string digitos)
    {
        ArgumentException.ThrowIfNullOrEmpty(digitos);
        if (digitos.Length % 2 != 0)
            throw new ArgumentException("O Code 128 conjunto C pede uma quantidade par de dígitos.", nameof(digitos));
        foreach (var c in digitos)
        {
            if (c is < '0' or > '9')
                throw new ArgumentException("O código de barras só aceita os dígitos de 0 a 9.", nameof(digitos));
        }

        Digitos = digitos;
    }

    /// <summary>Os dígitos.</summary>
    public string Digitos { get; }

    /// <summary>A altura das barras, em milímetros; 10 por padrão.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A altura não é um número maior que zero.</exception>
    public double AlturaMm
    {
        get => _alturaMm;
        init => _alturaMm = QrCodeImpressao.ValidarMilimetros(value, nameof(AlturaMm));
    }
}
