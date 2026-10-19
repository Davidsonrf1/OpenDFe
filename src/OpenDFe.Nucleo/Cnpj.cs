namespace OpenDFe;

/// <summary>
/// CNPJ, numérico ou alfanumérico (regra de 2026): validação, formatação e dígitos verificadores.
/// </summary>
/// <remarks>
/// O CNPJ tem 14 posições: as 12 primeiras (raiz e ordem) são dígitos ou letras maiúsculas, e as duas
/// últimas, os dígitos verificadores, sempre numéricos. Os dígitos verificadores são calculados por
/// módulo 11, com cada caractere valendo o código ASCII menos 48 (no CNPJ numérico, o resultado é o da
/// regra antiga). Aceito sem máscara (<c>12ABC34501DE35</c>) ou com a máscara inteira
/// (<c>12.ABC.345/01DE-35</c>); letras minúsculas não são aceitas, nem as 14 posições iguais.
/// </remarks>
public static class Cnpj
{
    /// <summary>Quantidade de posições do CNPJ sem máscara.</summary>
    public const int Tamanho = 14;

    private const int TamanhoComMascara = 18;
    private const int TamanhoBase = 12;

    /// <summary>Se o valor é um CNPJ válido, com ou sem máscara.</summary>
    /// <param name="valor">O CNPJ, sem máscara ou com a máscara inteira.</param>
    /// <returns>Se o formato e os dígitos verificadores estão certos.</returns>
    public static bool Valido(string? valor)
    {
        Span<char> cnpj = stackalloc char[Tamanho];
        return TirarMascara(valor, cnpj) && ValidoSemMascara(cnpj);
    }

    /// <summary>O CNPJ sem máscara (<c>12ABC34501DE35</c>), como vai no XML.</summary>
    /// <param name="valor">Um CNPJ válido, sem máscara ou com a máscara inteira.</param>
    /// <returns>As 14 posições, sem máscara.</returns>
    /// <exception cref="ArgumentException">O valor não é um CNPJ válido.</exception>
    public static string SemMascara(string valor)
    {
        Span<char> cnpj = stackalloc char[Tamanho];
        if (!TirarMascara(valor, cnpj) || !ValidoSemMascara(cnpj))
            throw new ArgumentException("O valor não é um CNPJ válido.", nameof(valor));
        return new string(cnpj);
    }

    /// <summary>O CNPJ com a máscara (<c>12.ABC.345/01DE-35</c>).</summary>
    /// <param name="valor">Um CNPJ válido, sem máscara ou com a máscara inteira.</param>
    /// <returns>O CNPJ com a máscara.</returns>
    /// <exception cref="ArgumentException">O valor não é um CNPJ válido.</exception>
    public static string Formatar(string valor)
    {
        var c = SemMascara(valor);
        return $"{c[..2]}.{c[2..5]}.{c[5..8]}/{c[8..12]}-{c[12..]}";
    }

    /// <summary>Os dois dígitos verificadores da base (raiz e ordem) de um CNPJ.</summary>
    /// <param name="base">As 12 primeiras posições, sem máscara: dígitos ou letras maiúsculas.</param>
    /// <returns>Os dois dígitos verificadores (<c>"35"</c> para <c>12ABC34501DE</c>).</returns>
    /// <exception cref="ArgumentException">A base não tem 12 posições, dígitos ou letras maiúsculas.</exception>
    public static string CalcularDigitos(string @base)
    {
        ArgumentNullException.ThrowIfNull(@base);
        if (@base.Length != TamanhoBase || !Alfanumerico(@base))
            throw new ArgumentException("A base do CNPJ tem 12 posições, dígitos ou letras maiúsculas.", nameof(@base));

        Span<char> cnpj = stackalloc char[Tamanho];
        @base.AsSpan().CopyTo(cnpj);
        cnpj[12] = (char)('0' + Modulo11.Calcular(cnpj[..12]));
        cnpj[13] = (char)('0' + Modulo11.Calcular(cnpj[..13]));
        return new string(cnpj[12..]);
    }

    private static bool ValidoSemMascara(ReadOnlySpan<char> cnpj)
    {
        if (!Alfanumerico(cnpj[..TamanhoBase]) || !char.IsAsciiDigit(cnpj[12]) || !char.IsAsciiDigit(cnpj[13]))
            return false;
        if (!cnpj.ContainsAnyExcept(cnpj[0]))
            return false;
        return Modulo11.Calcular(cnpj[..12]) == cnpj[12] - '0'
            && Modulo11.Calcular(cnpj[..13]) == cnpj[13] - '0';
    }

    private static bool Alfanumerico(ReadOnlySpan<char> valor)
    {
        foreach (var c in valor)
        {
            if (!char.IsAsciiDigit(c) && !char.IsAsciiLetterUpper(c))
                return false;
        }
        return true;
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
        if (v.Length != TamanhoComMascara || v[2] != '.' || v[6] != '.' || v[10] != '/' || v[15] != '-')
            return false;
        v[..2].CopyTo(destino);
        v[3..6].CopyTo(destino[2..]);
        v[7..10].CopyTo(destino[5..]);
        v[11..15].CopyTo(destino[8..]);
        v[16..].CopyTo(destino[12..]);
        return true;
    }
}
