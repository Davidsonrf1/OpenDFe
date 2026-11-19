using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace OpenDFe.NFSe;

/// <summary>
/// O identificador da DPS (45 posições): <c>DPS</c> + o município emissor (7) + o tipo de inscrição (1: CPF, 2: CNPJ) + a
/// inscrição de quem emite a DPS (14, o CPF com 000 à esquerda) + a série (5) + o número (15), com zeros à esquerda (Anexo
/// I, linha 101 do leiaute). É o Id do <c>infDPS</c> e o parâmetro da consulta pela DPS; sai dos campos da DPS.
/// </summary>
/// <remarks>
/// A instância padrão (<see langword="default"/>) não é um identificador: <see cref="Valor"/> é vazio e as outras
/// propriedades lançam.
/// </remarks>
public readonly record struct IdentificadorDps
{
    /// <summary>O tamanho do identificador: 45 posições.</summary>
    public const int Tamanho = 45;

    private const long MaiorNumero = 999_999_999_999_999;

    private readonly string? _valor;

    private IdentificadorDps(string valor)
    {
        _valor = valor;
    }

    /// <summary>O identificador, com o <c>DPS</c>.</summary>
    public string Valor => _valor ?? "";

    /// <summary>O código IBGE do município emissor.</summary>
    public int Municipio => int.Parse(Valor.AsSpan(3, 7), NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>Se a inscrição é um CPF (tipo 1); senão, é um CNPJ (tipo 2).</summary>
    public bool InscricaoCpf => Valor[10] == '1';

    /// <summary>A inscrição de quem emite a DPS: o CPF (11 dígitos) ou o CNPJ (14).</summary>
    public string Inscricao => InscricaoCpf ? Valor.Substring(14, 11) : Valor.Substring(11, 14);

    /// <summary>A série, com os 5 dígitos do identificador (<c>"00001"</c>).</summary>
    public string Serie => Valor.Substring(25, 5);

    /// <summary>O número da DPS.</summary>
    public long Numero => long.Parse(Valor.AsSpan(30, 15), NumberStyles.None, CultureInfo.InvariantCulture);

    /// <summary>Monta o identificador a partir dos campos da DPS.</summary>
    /// <param name="municipioEmissor">O código IBGE do município emissor (<c>cLocEmi</c>), 7 dígitos.</param>
    /// <param name="cpfCnpj">O CPF (11 dígitos) ou o CNPJ (14) de quem emite a DPS, só os dígitos.</param>
    /// <param name="serie">A série (<c>serie</c>), de 1 a 5 dígitos.</param>
    /// <param name="numero">O número (<c>nDPS</c>), de 1 a 999.999.999.999.999.</param>
    /// <returns>O identificador.</returns>
    /// <exception cref="ArgumentNullException">O CPF ou CNPJ ou a série é nulo.</exception>
    /// <exception cref="ArgumentOutOfRangeException">O município não tem 7 dígitos; o número está fora da faixa.</exception>
    /// <exception cref="ArgumentException">O documento não tem 11 nem 14 dígitos; a série não tem de 1 a 5 dígitos.</exception>
    public static IdentificadorDps Montar(int municipioEmissor, string cpfCnpj, string serie, long numero)
    {
        ArgumentNullException.ThrowIfNull(cpfCnpj);
        ArgumentNullException.ThrowIfNull(serie);
        ArgumentOutOfRangeException.ThrowIfLessThan(municipioEmissor, 1_000_000);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(municipioEmissor, 9_999_999);
        ArgumentOutOfRangeException.ThrowIfLessThan(numero, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numero, MaiorNumero);
        if (cpfCnpj.Length is not (11 or 14) || !Digitos(cpfCnpj))
            throw new ArgumentException("O documento de quem emite a DPS é um CPF (11 dígitos) ou um CNPJ (14).", nameof(cpfCnpj));
        if (serie.Length is < 1 or > 5 || !Digitos(serie))
            throw new ArgumentException("A série da DPS tem de 1 a 5 dígitos.", nameof(serie));
        var tipo = cpfCnpj.Length == 11 ? '1' : '2';
        var valor = string.Create(CultureInfo.InvariantCulture,
            $"DPS{municipioEmissor:D7}{tipo}{cpfCnpj.PadLeft(14, '0')}{serie.PadLeft(5, '0')}{numero:D15}");
        return new IdentificadorDps(valor);
    }

    /// <summary>Lê o identificador.</summary>
    /// <param name="id">As 45 posições, com o <c>DPS</c>.</param>
    /// <returns>O identificador.</returns>
    /// <exception cref="ArgumentNullException">O identificador é nulo.</exception>
    /// <exception cref="FormatException">
    /// Não é <c>DPS</c> e 42 dígitos; o tipo de inscrição não é 1 nem 2; o CPF não vem precedido de 000; o número é zero.
    /// </exception>
    public static IdentificadorDps Ler(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Conferir(id) is { } motivo ? throw new FormatException(motivo) : new IdentificadorDps(id);
    }

    /// <summary>Lê o identificador, sem lançar.</summary>
    /// <param name="id">As 45 posições, com o <c>DPS</c>.</param>
    /// <param name="resultado">O identificador, quando válido.</param>
    /// <returns>Se o identificador é válido (as conferências de <see cref="Ler"/>).</returns>
    public static bool TentarLer([NotNullWhen(true)] string? id, out IdentificadorDps resultado)
    {
        if (id is not null && Conferir(id) is null)
        {
            resultado = new IdentificadorDps(id);
            return true;
        }
        resultado = default;
        return false;
    }

    /// <summary>As 45 posições.</summary>
    /// <returns>O identificador.</returns>
    public override string ToString() => Valor;

    private static bool Digitos(ReadOnlySpan<char> texto) => texto.IndexOfAnyExceptInRange('0', '9') < 0;

    private static string? Conferir(string id)
    {
        if (id.Length != Tamanho || !id.StartsWith("DPS", StringComparison.Ordinal) || !Digitos(id.AsSpan(3)))
            return "O identificador da DPS é DPS seguido de 42 dígitos.";
        if (id[10] is not ('1' or '2'))
            return "O tipo de inscrição do identificador da DPS é 1 (CPF) ou 2 (CNPJ).";
        if (id[10] == '1' && !id.AsSpan(11, 3).SequenceEqual("000"))
            return "O CPF do identificador da DPS vem precedido de 000.";
        if (id.AsSpan(30, 15).IndexOfAnyExcept('0') < 0)
            return "O número da DPS no identificador não é zero.";
        return null;
    }
}
