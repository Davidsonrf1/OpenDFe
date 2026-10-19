namespace OpenDFe;

/// <summary>CPF: validação, formatação e dígitos verificadores.</summary>
/// <remarks>
/// Aceito sem máscara (<c>52998224725</c>) ou com a máscara inteira (<c>529.982.247-25</c>). Os números
/// com todos os dígitos iguais (<c>111.111.111-11</c>) passam no cálculo, mas não são CPFs válidos.
/// </remarks>
public static class Cpf
{
    /// <summary>Quantidade de dígitos do CPF sem máscara.</summary>
    public const int Tamanho = 11;

    private const int TamanhoMascarado = 14;
    private const int TamanhoBase = 9;

    /// <summary>Se o valor é um CPF válido, com ou sem máscara.</summary>
    /// <param name="valor">O CPF, sem máscara ou com a máscara inteira.</param>
    /// <returns>Se o formato e os dígitos verificadores estão certos.</returns>
    public static bool Valido(string? valor)
    {
        Span<char> cpf = stackalloc char[Tamanho];
        return TirarMascara(valor, cpf) && ValidoDesmascarado(cpf);
    }

    /// <summary>O CPF sem máscara (<c>52998224725</c>), como vai no XML.</summary>
    /// <param name="valor">Um CPF válido, sem máscara ou com a máscara inteira.</param>
    /// <returns>Os 11 dígitos, sem máscara.</returns>
    /// <exception cref="ArgumentException">O valor não é um CPF válido.</exception>
    public static string RemoverMascara(string valor)
    {
        Span<char> cpf = stackalloc char[Tamanho];
        if (!TirarMascara(valor, cpf) || !ValidoDesmascarado(cpf))
            throw new ArgumentException("O valor não é um CPF válido.", nameof(valor));
        return new string(cpf);
    }

    /// <summary>O CPF com a máscara (<c>529.982.247-25</c>).</summary>
    /// <param name="valor">Um CPF válido, sem máscara ou com a máscara inteira.</param>
    /// <returns>O CPF com a máscara.</returns>
    /// <exception cref="ArgumentException">O valor não é um CPF válido.</exception>
    public static string Formatar(string valor)
    {
        var c = RemoverMascara(valor);
        return $"{c[..3]}.{c[3..6]}.{c[6..9]}-{c[9..]}";
    }

    /// <summary>Os dois dígitos verificadores da base de um CPF.</summary>
    /// <param name="base">Os 9 primeiros dígitos, sem máscara.</param>
    /// <returns>Os dois dígitos verificadores (<c>"25"</c> para <c>529982247</c>).</returns>
    /// <exception cref="ArgumentException">A base não tem 9 dígitos.</exception>
    public static string CalcularDigitos(string @base)
    {
        ArgumentNullException.ThrowIfNull(@base);
        if (@base.Length != TamanhoBase || @base.AsSpan().ContainsAnyExceptInRange('0', '9'))
            throw new ArgumentException("A base do CPF tem 9 dígitos.", nameof(@base));

        Span<char> cpf = stackalloc char[Tamanho];
        @base.AsSpan().CopyTo(cpf);
        cpf[9] = Digito(cpf[..9]);
        cpf[10] = Digito(cpf[..10]);
        return new string(cpf[9..]);
    }

    private static bool ValidoDesmascarado(ReadOnlySpan<char> cpf)
    {
        if (cpf.ContainsAnyExceptInRange('0', '9') || !cpf.ContainsAnyExcept(cpf[0]))
            return false;
        return Digito(cpf[..9]) == cpf[9] && Digito(cpf[..10]) == cpf[10];
    }

    // Pesos decrescentes, de (quantidade de dígitos + 1) até 2, sem recomeçar.
    private static char Digito(ReadOnlySpan<char> digitos)
    {
        var soma = 0;
        for (var i = 0; i < digitos.Length; i++)
            soma += (digitos[i] - '0') * (digitos.Length + 1 - i);
        var resto = soma % 11;
        return (char)('0' + (resto < 2 ? 0 : 11 - resto));
    }

    private static bool TirarMascara(string? valor, Span<char> destino)
    {
        if (valor is null)
            return false;
        var v = valor.AsSpan();
        if (v.Length == Tamanho)
        {
            v.CopyTo(destino);
            return true;
        }
        if (v.Length != TamanhoMascarado || v[3] != '.' || v[7] != '.' || v[11] != '-')
            return false;
        v[..3].CopyTo(destino);
        v[4..7].CopyTo(destino[3..]);
        v[8..11].CopyTo(destino[6..]);
        v[12..].CopyTo(destino[9..]);
        return true;
    }
}
