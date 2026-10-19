using System.Globalization;

namespace OpenDFe;

/// <summary>
/// Formata os valores como os leiautes os escrevem no XML, sempre na cultura invariável: nada depende de
/// <see cref="CultureInfo.CurrentCulture"/>.
/// </summary>
public static class FormatadorInvariavel
{
    private const string FormatoDataHora = "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz";
    private const string FormatoData = "yyyy'-'MM'-'dd";
    private const string FormatoSignificativo = "0.############################";
    private const int CasasMaximas = 28;

    /// <summary>
    /// O decimal com as casas do campo, ponto como separador e sem separador de milhar
    /// (<c>1234.5m</c> com 2 casas: <c>"1234.50"</c>).
    /// </summary>
    /// <remarks>
    /// O valor nunca é arredondado: se tem mais casas significativas do que o campo (<c>10.123m</c> com
    /// 2 casas), sai com todas (<c>"10.123"</c>), e a validação pelo schema aponta o campo. Zeros à direita
    /// além das casas do campo são descartados (<c>10.1000m</c> com 2 casas: <c>"10.10"</c>).
    /// </remarks>
    /// <param name="valor">O valor.</param>
    /// <param name="casas">As casas decimais do campo, de 0 a 28.</param>
    /// <returns>O valor formatado.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="casas"/> fora de 0 a 28.</exception>
    public static string Numero(decimal valor, int casas)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(casas);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(casas, CasasMaximas);

        if (valor == 0m)
            valor = 0m; // o zero com sinal negativo sai sem sinal

        var significativo = valor.ToString(FormatoSignificativo, CultureInfo.InvariantCulture);
        var ponto = significativo.IndexOf('.', StringComparison.Ordinal);
        var casasSignificativas = ponto < 0 ? 0 : significativo.Length - ponto - 1;
        if (casasSignificativas > casas)
            return significativo;
        return valor.ToString("F" + casas.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// A data e hora com o deslocamento do fuso, no formato do leiaute (<c>"2026-10-19T14:30:00-03:00"</c>).
    /// Frações de segundo são descartadas.
    /// </summary>
    /// <param name="valor">A data e hora, com o deslocamento do fuso em que aconteceu.</param>
    /// <returns>O valor formatado.</returns>
    public static string DataHora(DateTimeOffset valor) =>
        valor.ToString(FormatoDataHora, CultureInfo.InvariantCulture);

    /// <summary>A data, sem hora (<c>"2026-10-19"</c>).</summary>
    /// <param name="valor">A data.</param>
    /// <returns>O valor formatado.</returns>
    public static string Data(DateOnly valor) =>
        valor.ToString(FormatoData, CultureInfo.InvariantCulture);
}
