using System.Globalization;
using System.Text;

namespace OpenDFe.Impressao.Pdf;

/// <summary>
/// Code 128, conjunto C (pares de dígitos): a chave de acesso de 44 dígitos vira 22 símbolos. A sequência é o símbolo
/// de início C (105), os pares, o verificador (módulo 103) e o de fim (106). O SVG sai sem a zona de silêncio (10
/// módulos de cada lado, desenhada por quem usa o código de barras).
/// </summary>
internal static class CodigoBarras128
{
    /// <summary>O valor do símbolo de início do conjunto C.</summary>
    public const int InicioC = 105;

    /// <summary>O valor do símbolo de fim.</summary>
    public const int Fim = 106;

    // Os 107 padrões da tabela do Code 128 (ISO/IEC 15417), do valor 0 ao 106: as larguras, em módulos, de barra,
    // espaço, barra, espaço, barra, espaço (o fim tem um sétimo elemento, a barra final). Cada padrão de dados soma
    // 11 módulos; o fim soma 13.
    private static readonly string[] Padroes =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213",
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132",
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211",
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331",
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111",
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141",
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141",
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112",
    ];

    /// <summary>As larguras (em módulos) dos elementos do padrão do símbolo <paramref name="valor"/> (0 a 106).</summary>
    public static IReadOnlyList<int> Padrao(int valor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valor);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(valor, Fim);
        return [.. Padroes[valor].Select(d => d - '0')];
    }

    /// <summary>
    /// Os valores dos símbolos, nesta ordem: início C, um por par de dígitos, o verificador e o fim. Recusa texto vazio,
    /// com caractere que não seja dígito e com quantidade ímpar de dígitos.
    /// </summary>
    public static int[] Simbolos(string digitos)
    {
        ArgumentException.ThrowIfNullOrEmpty(digitos);
        if (digitos.Length % 2 != 0)
            throw new ArgumentException("O conjunto C do Code 128 codifica pares de dígitos: a quantidade de dígitos tem de ser par.", nameof(digitos));
        foreach (var c in digitos)
            if (c is < '0' or > '9')
                throw new ArgumentException("O conjunto C do Code 128 aceita só os dígitos de 0 a 9.", nameof(digitos));

        var pares = digitos.Length / 2;
        var simbolos = new int[pares + 3];
        simbolos[0] = InicioC;
        var soma = InicioC;
        for (var i = 0; i < pares; i++)
        {
            var valor = (digitos[2 * i] - '0') * 10 + (digitos[2 * i + 1] - '0');
            simbolos[i + 1] = valor;
            soma += valor * (i + 1);
        }
        simbolos[pares + 1] = soma % 103;
        simbolos[pares + 2] = Fim;
        return simbolos;
    }

    /// <summary>
    /// As larguras, em módulos, dos elementos do código de barras todo, de barra, espaço, barra... (sempre começa e
    /// termina por barra).
    /// </summary>
    public static int[] Larguras(string digitos) =>
        [.. Simbolos(digitos).SelectMany(s => Padroes[s].Select(d => d - '0'))];

    /// <summary>
    /// O SVG do código de barras: uma unidade por módulo na largura e <paramref name="altura"/> unidades na altura,
    /// um retângulo por barra num único <c>path</c>.
    /// </summary>
    public static string Gerar(string digitos, double altura = 50)
    {
        if (!(altura > 0) || double.IsInfinity(altura))
            throw new ArgumentOutOfRangeException(nameof(altura), altura, "A altura deve ser um número maior que zero.");
        var c = CultureInfo.InvariantCulture;
        var larguras = Larguras(digitos);
        var caminho = new StringBuilder();
        var x = 0;
        for (var i = 0; i < larguras.Length; i++)
        {
            if (i % 2 == 0) caminho.Append(c, $"M{x} 0h{larguras[i]}v{altura:R}h-{larguras[i]}z");
            x += larguras[i];
        }
        return string.Create(c, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{x}\" height=\"{altura:R}\" viewBox=\"0 0 {x} {altura:R}\" preserveAspectRatio=\"none\" shape-rendering=\"crispEdges\"><path fill=\"#000\" d=\"{caminho}\"/></svg>");
    }
}
