using System.Globalization;

namespace OpenDFe.NFSe;

/// <summary>
/// Formata os valores do DANFSe em português do Brasil <b>sem depender da cultura do sistema</b>: vírgula decimal, ponto de
/// milhar, datas <c>dd/MM/yyyy HH:mm:ss</c> como o XML traz. As máscaras são as da NT 008/2026 (2.4.5) e as do DANFE
/// (docs/NFSE.md 12, decisão G12); os leiautes não se referenciam, então a parte comum é uma cópia.
/// </summary>
internal static class FormatadorDanfse
{
    /// <summary>O traço do campo sem informação no XML (NT 008/2026 2.4.5, nota 12).</summary>
    internal const string Ausente = "-";

    private const string Reticencias = "...";

    private static readonly NumberFormatInfo Numeros = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3],
        NegativeSign = "-",
        NumberNegativePattern = 1,
    };

    /// <summary>O dinheiro com duas casas e o símbolo: <c>R$ 1.234,56</c>; o traço sem valor.</summary>
    internal static string Dinheiro(decimal? valor) => valor is { } v ? "R$ " + v.ToString("N2", Numeros) : Ausente;

    /// <summary>O percentual com duas casas: <c>2,00%</c>; o traço sem valor.</summary>
    internal static string Percentual(decimal? valor) => valor is { } v ? v.ToString("N2", Numeros) + "%" : Ausente;

    /// <summary>A data e hora como o XML traz, sem converter o fuso: <c>10/03/2026 15:03:53</c>; o traço sem valor.</summary>
    internal static string DataHora(DateTimeOffset? valor) =>
        valor is { } v ? v.ToString("dd'/'MM'/'yyyy HH':'mm':'ss", CultureInfo.InvariantCulture) : Ausente;

    /// <summary>A data: <c>10/03/2026</c>; o traço sem valor.</summary>
    internal static string Data(DateOnly? valor) =>
        valor is { } v ? v.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture) : Ausente;

    /// <summary>O número inteiro sem zeros à esquerda nem separador; o traço sem valor.</summary>
    internal static string Numero(long? valor) => valor is { } v ? v.ToString(CultureInfo.InvariantCulture) : Ausente;

    /// <summary>O CNPJ (<c>00.000.000/0000-00</c>, o alfanumérico também), o CPF (<c>000.000.000-00</c>) ou o NIF como veio.</summary>
    internal static string Documento(string? cnpj, string? cpf, string? nif)
    {
        if (!string.IsNullOrEmpty(cnpj))
            return cnpj.Length == 14 ? $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}" : cnpj;
        if (!string.IsNullOrEmpty(cpf))
            return cpf.Length == 11 ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}" : cpf;
        return Texto(nif);
    }

    /// <summary>O CEP como <c>00.000-000</c> (NT 008/2026 2.4.5); outro tamanho sai como veio.</summary>
    internal static string Cep(string valor) => valor.Length == 8 ? $"{valor[..2]}.{valor[2..5]}-{valor[5..]}" : valor;

    /// <summary>O telefone com o código de área entre parênteses (10 ou 11 dígitos); outro sai como veio; o traço sem valor.</summary>
    internal static string Telefone(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return Ausente;
        return valor.Length switch
        {
            10 when valor.All(char.IsAsciiDigit) => $"({valor[..2]}) {valor[2..6]}-{valor[6..]}",
            11 when valor.All(char.IsAsciiDigit) => $"({valor[..2]}) {valor[2..7]}-{valor[7..]}",
            _ => valor,
        };
    }

    /// <summary>O código de tributação nacional como <c>nn.nn.nn</c> (6 dígitos); outro sai como veio.</summary>
    internal static string TributacaoNacional(string valor) =>
        valor.Length == 6 ? $"{valor[..2]}.{valor[2..4]}.{valor[4..]}" : valor;

    /// <summary>O código da NBS como <c>n.nnnn.nn.nn</c> (9 dígitos); outro sai como veio; o traço sem valor.</summary>
    internal static string Nbs(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return Ausente;
        return valor.Length == 9 ? $"{valor[..1]}.{valor[1..5]}.{valor[5..7]}.{valor[7..]}" : valor;
    }

    /// <summary>O texto como veio, ou o traço quando vazio.</summary>
    internal static string Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? Ausente : valor;

    /// <summary>
    /// O texto cortado no tamanho do campo da NT (o "Tam. do Campo" do 2.4.5), com as reticências dentro dele: o campo de 40
    /// leva 37 caracteres e as reticências (2.1).
    /// </summary>
    internal static string Cortar(string valor, int tamanho) =>
        valor.Length <= tamanho ? valor : valor[..(tamanho - Reticencias.Length)].TrimEnd() + Reticencias;

    /// <summary>Só as partes que existem, separadas pelo separador dado.</summary>
    internal static string Juntar(string separador, params string?[] partes) =>
        string.Join(separador, partes.Where(p => !string.IsNullOrWhiteSpace(p)));
}
