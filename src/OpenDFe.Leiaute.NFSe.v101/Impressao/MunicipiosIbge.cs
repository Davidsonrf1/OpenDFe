using System.Globalization;

namespace OpenDFe.NFSe;

/// <summary>
/// Os nomes dos municípios pelo código do IBGE, para o DANFSe (docs/NFSE.md 12, decisão G4): o XML da NFS-e só traz o código
/// no endereço nacional, e a NT 008/2026 manda imprimir o nome. A tabela é o recurso <c>Recursos/MunicipiosIbge.txt</c>
/// (os 5.571 municípios da API de localidades do IBGE, com a data e o SHA-256 em docs/NTS.md 12.3), carregada uma vez.
/// </summary>
internal static class MunicipiosIbge
{
    private const string Recurso = "Recursos/MunicipiosIbge.txt";

    private static readonly Lazy<IReadOnlyDictionary<int, string>> Tabela = new(Carregar);

    /// <summary>A quantidade de municípios da tabela.</summary>
    internal static int Quantidade => Tabela.Value.Count;

    /// <summary>O nome do município; <see langword="null"/> com o código fora da tabela.</summary>
    internal static string? Nome(int codigo) => Tabela.Value.TryGetValue(codigo, out var nome) ? nome : null;

    /// <summary>A UF do código do município (os dois primeiros dígitos); <see langword="null"/> se não é uma UF.</summary>
    internal static UF? Estado(int codigo)
    {
        var uf = (UF)(codigo / 100000);
        return codigo is >= 1000000 and <= 9999999 && Enum.IsDefined(uf) ? uf : null;
    }

    private static Dictionary<int, string> Carregar()
    {
        using var fluxo = typeof(MunicipiosIbge).Assembly.GetManifestResourceStream(Recurso)
            ?? throw new InvalidOperationException($"O recurso {Recurso} não está no assembly.");
        using var leitor = new StreamReader(fluxo);
        var tabela = new Dictionary<int, string>();
        while (leitor.ReadLine() is { } linha)
        {
            if (linha.Length == 0 || linha[0] == '#')
                continue;
            var separador = linha.IndexOf(';', StringComparison.Ordinal);
            tabela.Add(int.Parse(linha.AsSpan(0, separador), NumberStyles.None, CultureInfo.InvariantCulture), linha[(separador + 1)..]);
        }

        return tabela;
    }
}
