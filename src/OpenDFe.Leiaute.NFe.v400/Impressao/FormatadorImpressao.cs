using System.Globalization;

namespace OpenDFe.NFe;

/// <summary>
/// Formata os valores do DANFE em português do Brasil <b>sem depender da cultura do sistema</b> (num Linux enxuto o
/// <c>pt-BR</c> pode faltar): vírgula decimal, ponto de milhar, datas <c>dd/MM/yyyy HH:mm:ss</c>.
/// </summary>
internal static class FormatadorImpressao
{
    private const int CasasMaximas = 10;

    private static readonly NumberFormatInfo Numeros = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3],
        NegativeSign = "-",
        NumberNegativePattern = 1,
    };

    /// <summary>O dinheiro com duas casas: <c>1.234,56</c>.</summary>
    internal static string Dinheiro(decimal valor) => valor.ToString("N2", Numeros);

    /// <summary>
    /// A quantidade comercial sem zeros sobrando à direita (<c>2</c>, <c>2,5</c>, <c>1,125</c>, <c>1.000</c>), com até 10
    /// casas: o valor nunca é arredondado.
    /// </summary>
    internal static string Quantidade(decimal valor) => Casas(valor, 0);

    /// <summary>
    /// O valor unitário comercial com ao menos duas casas e sem zeros sobrando além delas (<c>27,64</c>,
    /// <c>0,123456</c>), com até 10 casas.
    /// </summary>
    internal static string Unitario(decimal valor) => Casas(valor, 2);

    /// <summary>A data e hora como a nota traz, no fuso dela e sem converter: <c>10/03/2015 15:03:53</c>.</summary>
    internal static string DataHora(DateTimeOffset valor) =>
        valor.ToString("dd'/'MM'/'yyyy HH':'mm':'ss", CultureInfo.InvariantCulture);

    /// <summary>O CNPJ com a máscara <c>00.000.000/0000-00</c> (vale para o alfanumérico); outro tamanho sai como veio.</summary>
    internal static string Cnpj(string valor) => valor.Length == 14
        ? $"{valor[..2]}.{valor[2..5]}.{valor[5..8]}/{valor[8..12]}-{valor[12..]}"
        : valor;

    /// <summary>O CPF com a máscara <c>000.000.000-00</c>; outro tamanho sai como veio.</summary>
    internal static string Cpf(string valor) => valor.Length == 11
        ? $"{valor[..3]}.{valor[3..6]}.{valor[6..9]}-{valor[9..]}"
        : valor;

    /// <summary>O CEP como <c>88.999-999</c> (o do manual, figura 2); outro tamanho sai como veio.</summary>
    internal static string Cep(string valor) => valor.Length == 8
        ? $"{valor[..2]}.{valor[2..5]}-{valor[5..]}"
        : valor;

    /// <summary>A data como a nota traz, sem converter o fuso: <c>10/03/2015</c>.</summary>
    internal static string Data(DateTimeOffset valor) => valor.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

    /// <summary>A data sem hora (vencimento, fabricação, validade): <c>10/03/2015</c>.</summary>
    internal static string Data(DateOnly valor) => valor.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

    /// <summary>A hora como a nota traz, sem converter o fuso: <c>15:03:53</c>.</summary>
    internal static string Hora(DateTimeOffset valor) => valor.ToString("HH':'mm':'ss", CultureInfo.InvariantCulture);

    /// <summary>O CEP do DANFE, <c>00000-000</c> (Anexo II e NT 2026.010); outro tamanho sai como veio.</summary>
    internal static string CepDanfe(string valor) => valor.Length == 8 ? $"{valor[..5]}-{valor[5..]}" : valor;

    /// <summary>
    /// O telefone com o código de área entre parênteses: <c>(31) 3333-4444</c> (10 dígitos) ou <c>(31) 99999-4444</c> (11);
    /// outro tamanho sai como veio.
    /// </summary>
    internal static string Telefone(string valor) => valor.Length switch
    {
        10 when valor.All(char.IsAsciiDigit) => $"({valor[..2]}) {valor[2..6]}-{valor[6..]}",
        11 when valor.All(char.IsAsciiDigit) => $"({valor[..2]}) {valor[2..7]}-{valor[7..]}",
        _ => valor,
    };

    /// <summary>O número da NF-e com nove dígitos e pontos de milhar: <c>000.000.123</c> (Anexo II 3.9).</summary>
    internal static string NumeroNota(int valor)
    {
        var nove = Zeros(valor, 9);
        return $"{nove[..3]}.{nove[3..6]}.{nove[6..]}";
    }

    /// <summary>O valor com as casas dadas e o ponto de milhar: <c>10,500</c> (três casas, o peso).</summary>
    internal static string Fixo(decimal valor, int casas) => valor.ToString("N" + casas.ToString(CultureInfo.InvariantCulture), Numeros);

    /// <summary>A chave de acesso em blocos de quatro, separados por espaço (manual, 3.1.4).</summary>
    internal static string Chave(string valor)
    {
        var blocos = new List<string>();
        for (var i = 0; i < valor.Length; i += 4)
            blocos.Add(valor.Substring(i, Math.Min(4, valor.Length - i)));
        return string.Join(' ', blocos);
    }

    /// <summary>O número zerado à esquerda, como nas figuras do manual (<c>000000001</c>).</summary>
    internal static string Zeros(int valor, int tamanho) => valor.ToString(CultureInfo.InvariantCulture).PadLeft(tamanho, '0');

    private static string Casas(decimal valor, int minimo)
    {
        var formato = "#,##0." + new string('0', minimo) + new string('#', CasasMaximas - minimo);
        return valor.ToString(formato, Numeros);
    }
}
