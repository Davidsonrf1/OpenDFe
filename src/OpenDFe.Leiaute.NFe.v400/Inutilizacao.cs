namespace OpenDFe.NFe;

/// <summary>
/// A inutilização de uma faixa de numeração (<c>NFeInutilizacao4</c>, MOC 7.0, Visão Geral, 5.3; docs/CONTRATOS.md 18): o
/// <c>inutNFe</c> assinado. Vai sempre ao autorizador normal da UF do núcleo, com o tipo de emissão normal (a SVC não
/// oferece o serviço). O modelo (55 ou 65) e a UF são os do núcleo.
/// </summary>
/// <remarks>O emitente com CPF não tem inutilização neste schema (o <c>infInut</c> só tem CNPJ): pendente.</remarks>
public sealed class Inutilizacao : ParametroWS<RetornoInutilizacao>
{
    private readonly int? _ano;

    /// <summary>A inutilização da faixa.</summary>
    /// <param name="cnpj">O CNPJ do emitente, com ou sem máscara.</param>
    /// <param name="serie">A série, de 0 a 999.</param>
    /// <param name="numeroInicial">O primeiro número, a partir de 1.</param>
    /// <param name="numeroFinal">O último número, não menor que o inicial.</param>
    /// <param name="justificativa">A justificativa; o tamanho (15 a 255) é do schema.</param>
    /// <exception cref="ArgumentException">
    /// CNPJ inválido, série fora de 0 a 999, número inicial menor que 1, final menor que o inicial ou maior que 999999999,
    /// justificativa nula ou vazia.
    /// </exception>
    public Inutilizacao(string cnpj, int serie, int numeroInicial, int numeroFinal, string justificativa)
    {
        if (!OpenDFe.Cnpj.Valido(cnpj))
            throw new ArgumentException("O CNPJ não é válido.", nameof(cnpj));
        if (serie is < 0 or > 999)
            throw new ArgumentException("A série vai de 0 a 999.", nameof(serie));
        if (numeroInicial < 1)
            throw new ArgumentException("O número inicial começa em 1.", nameof(numeroInicial));
        if (numeroFinal < numeroInicial || numeroFinal > 999_999_999)
            throw new ArgumentException("O número final não pode ser menor que o inicial nem passar de 999999999.", nameof(numeroFinal));
        if (string.IsNullOrEmpty(justificativa))
            throw new ArgumentException("A justificativa da inutilização é obrigatória.", nameof(justificativa));
        Cnpj = OpenDFe.Cnpj.RemoverMascara(cnpj);
        Serie = serie;
        NumeroInicial = numeroInicial;
        NumeroFinal = numeroFinal;
        Justificativa = justificativa;
    }

    /// <summary>O ano da numeração, com 2 dígitos; nulo: o ano do relógio da configuração, no fuso de Brasília.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Fora de 0 a 99.</exception>
    public int? Ano
    {
        get => _ano;
        init
        {
            if (value is { } ano && ano is < 0 or > 99)
                throw new ArgumentOutOfRangeException(nameof(value), value, "O ano tem 2 dígitos.");
            _ano = value;
        }
    }

    /// <summary>O CNPJ do emitente, sem máscara.</summary>
    public string Cnpj { get; }

    /// <summary>A série.</summary>
    public int Serie { get; }

    /// <summary>O primeiro número da faixa.</summary>
    public int NumeroInicial { get; }

    /// <summary>O último número da faixa.</summary>
    public int NumeroFinal { get; }

    /// <summary>A justificativa.</summary>
    public string Justificativa { get; }
}
