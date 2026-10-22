using System.Text.RegularExpressions;
using OpenDFe.NFe;

namespace OpenDFe.Testes;

// O catálogo das rejeições (REGRAS.md 6 e 10) contra o código: toda regra da RegrasNFe400 marcada "lib" com o mesmo
// identificador, código, modelo e aplicação, as do lote da autorização (LoteNFe.Regras, 4.4) também, e toda linha "lib"
// com a sua regra; e a forma das linhas.
public partial class CatalogoRegrasTestes
{
    private static readonly Lazy<Linha[]> Linhas = new(Ler);

    private sealed record Linha(string Regra, string Modelo, string Codigo, string Aplicacao, string Mensagem, string Fonte, string Situacao);

    [GeneratedRegex(@"^\d{0,2}[A-Z]{1,3}\d{2,3}[a-z]?\d?(\.\d+)?(-\d{1,3}[a-z]?)?A?$")]
    private static partial Regex Identificador();

    [GeneratedRegex(@"^(lib \(\d\.\d\)|a fazer( \(\d(\.\d)?\))?|schema|escrita|SEFAZ|SEFAZ \(tabela\)|outra operação \(\d(\.\d)?\)|excluída)$")]
    private static partial Regex Situacao();

    [GeneratedRegex(@"^(MOC 7\.03|NT \d{4}\.\d{3} v\d\.\d{2}[a-z]?)$")]
    private static partial Regex Fonte();

    private static Linha[] Ler()
    {
        var texto = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "docs", "REGRAS.md"));
        var inicio = Array.FindIndex(texto, l => l.StartsWith("## 10. ", StringComparison.Ordinal));
        Assert.True(inicio > 0, "O REGRAS.md não tem a seção 10 (o catálogo).");
        var linhas = new List<Linha>();
        foreach (var l in texto.Skip(inicio))
        {
            if (!l.StartsWith("| ", StringComparison.Ordinal))
                continue;
            var c = l.Trim('|').Split('|').Select(s => s.Trim()).ToArray();
            if (c.Length != 7 || c[0] == "Regra")
                continue;
            linhas.Add(new Linha(c[0], c[1], c[2], c[3], c[4], c[5], c[6]));
        }
        return [.. linhas];
    }

    private static string Modelo(RegraNFe regra) => regra.Modelos.Count == 2 ? "55/65" : ((int)regra.Modelos[0]).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string Aplicacao(RegraNFe regra) => regra.Aplicacao == AplicacaoRegra.Obrigatoria ? "Obrig." : "Facul.";

    [Fact]
    public void OCatalogoTemAsRegrasDoAnexoEDasNTs()
    {
        Assert.True(Linhas.Value.Length > 1000);
        Assert.Contains(Linhas.Value, l => l.Fonte == "MOC 7.03");
        Assert.Contains(Linhas.Value, l => l.Fonte.StartsWith("NT 2025.002", StringComparison.Ordinal));
    }

    [Fact]
    public void CadaLinhaTemAForma()
    {
        foreach (var l in Linhas.Value)
        {
            Assert.True(Identificador().IsMatch(l.Regra), $"Identificador: {l.Regra}");
            Assert.Contains(l.Modelo, (string[])["55", "65", "55/65"]);
            Assert.Matches(@"^\d{3,4}$", l.Codigo);
            Assert.Contains(l.Aplicacao, (string[])["Obrig.", "Facul.", "Facul. (55), Obrig. (65)"]);
            Assert.False(string.IsNullOrWhiteSpace(l.Mensagem), $"Mensagem vazia: {l.Regra}");
            Assert.True(Fonte().IsMatch(l.Fonte), $"Fonte: {l.Regra} {l.Fonte}");
            Assert.True(Situacao().IsMatch(l.Situacao), $"Situação: {l.Regra} {l.Situacao}");
        }
    }

    [Fact]
    public void CadaRegraECodigoApareceUmaVez()
    {
        var repetidas = Linhas.Value.GroupBy(l => (l.Regra, l.Codigo)).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.Empty(repetidas);
    }

    [Fact]
    public void ARegraImplementadaEstaNoCatalogoComoLib()
    {
        foreach (var regra in RegrasNFe400.Lista)
        {
            var linha = Linhas.Value.SingleOrDefault(l => l.Regra == regra.Identificador && l.Codigo == regra.Codigo);
            Assert.True(linha is not null, $"{regra.Identificador} ({regra.Codigo}) não está no catálogo.");
            Assert.StartsWith("lib ", linha!.Situacao, StringComparison.Ordinal);
            Assert.Equal(Modelo(regra), linha.Modelo);
            Assert.Equal(Aplicacao(regra), linha.Aplicacao);
        }
    }

    [Fact]
    public void ARegraDoLoteEstaNoCatalogoComoLib()
    {
        foreach (var (identificador, codigo) in LoteNFe.Regras)
        {
            var linha = Linhas.Value.SingleOrDefault(l => l.Regra == identificador && l.Codigo == codigo);
            Assert.True(linha is not null, $"{identificador} ({codigo}) não está no catálogo.");
            Assert.Equal("lib (4.4)", linha!.Situacao);
        }
    }

    [Fact]
    public void ALinhaLibTemASuaRegra()
    {
        foreach (var linha in Linhas.Value.Where(l => l.Situacao.StartsWith("lib ", StringComparison.Ordinal)))
        {
            Assert.True(RegrasNFe400.Lista.Any(r => r.Identificador == linha.Regra && r.Codigo == linha.Codigo)
                    || LoteNFe.Regras.Contains((linha.Regra, linha.Codigo)),
                $"{linha.Regra} ({linha.Codigo}) está marcada lib e não está na RegrasNFe400 nem nas regras do lote.");
        }
    }

    [Fact]
    public void AContagemDaSecao10ConfereComAsLinhas()
    {
        var texto = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "docs", "REGRAS.md"));
        var contagem = texto.SkipWhile(l => !l.StartsWith("| Situação | Regras |", StringComparison.Ordinal)).Skip(2)
            .TakeWhile(l => l.StartsWith("| ", StringComparison.Ordinal))
            .Select(l => l.Trim('|').Split('|').Select(s => s.Trim()).ToArray())
            .ToDictionary(c => c[0], c => c[1]);
        Assert.Equal($"**{Linhas.Value.Length}**", contagem["**total**"]);
        foreach (var (situacao, quantidade) in contagem.Where(c => c.Key != "**total**"))
        {
            var esperada = situacao == "lib"
                ? Linhas.Value.Count(l => l.Situacao.StartsWith("lib ", StringComparison.Ordinal))
                : Linhas.Value.Count(l => l.Situacao == situacao);
            Assert.True(esperada.ToString(System.Globalization.CultureInfo.InvariantCulture) == quantidade, $"{situacao}: {quantidade} na tabela, {esperada} nas linhas.");
        }
    }
}
