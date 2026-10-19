namespace OpenDFe.SefazEmulada;

// Um autorizador da NF-e, como o Portal Nacional os lista ("Relação de Serviços Web", conferida em
// 2026-10-19): a sigla do caminho (/MG/...), o código da UF que responde quando o pedido não diz qual
// (o cUF do retorno), as UFs atendidas e os serviços. Tabela própria, sem o Atendimento da lib (13.5:
// o emulador não reaproveita o que a lib declara, para um erro dela não aparecer igual dos dois lados).
internal sealed class Autorizador
{
    private Autorizador(string sigla, string codigoCasa, IReadOnlyList<string> atendidas, IReadOnlyList<string> servicos)
    {
        Sigla = sigla;
        CodigoCasa = codigoCasa;
        Atendidas = atendidas;
        Servicos = servicos;
    }

    public string Sigla { get; }

    public string CodigoCasa { get; }

    // Os códigos IBGE das UFs atendidas.
    public IReadOnlyList<string> Atendidas { get; }

    // Os serviços que o autorizador tem no Portal; o emulador só responde os que já implementa.
    public IReadOnlyList<string> Servicos { get; }

    public bool Atende(string codigoUF) => Atendidas.Contains(codigoUF, StringComparer.Ordinal);

    public static IReadOnlyList<Autorizador> Todos { get; } = Montar();

    public static Autorizador? Procurar(string sigla) =>
        Todos.FirstOrDefault(a => string.Equals(a.Sigla, sigla, StringComparison.OrdinalIgnoreCase));

    private static List<Autorizador> Montar()
    {
        string[] completos = [Servico.NomeStatus, "NFeAutorizacao4", "NFeRetAutorizacao4", "NFeConsultaProtocolo4", "NFeRecepcaoEvento4", "NFeInutilizacao4"];
        string[] svcRS = [Servico.NomeStatus, "NFeAutorizacao4", "NFeRetAutorizacao4", "NFeConsultaProtocolo4", "NFeRecepcaoEvento4"];
        string[] an = ["NFeDistribuicaoDFe", "NFeRecepcaoEvento4"];

        var lista = new List<Autorizador>();
        foreach (var uf in (string[])["AM", "BA", "GO", "MG", "MS", "MT", "PE", "PR", "RS", "SP"])
            lista.Add(new Autorizador(uf, UnidadesFederativas.Codigo(uf), [UnidadesFederativas.Codigo(uf)], completos));
        lista.Add(new Autorizador("SVAN", "91", Codigos("MA"), completos));
        lista.Add(new Autorizador("SVRS", "43", Codigos("AC", "AL", "AP", "CE", "DF", "ES", "PA", "PB", "PI", "RJ", "RN", "RO", "RR", "SC", "SE", "TO"), completos));
        lista.Add(new Autorizador("SVC-AN", "91", Codigos("AC", "AL", "AP", "CE", "DF", "ES", "MG", "PA", "PB", "PI", "RJ", "RN", "RO", "RR", "RS", "SC", "SE", "SP", "TO"), completos));
        lista.Add(new Autorizador("SVC-RS", "43", Codigos("AM", "BA", "GO", "MA", "MS", "MT", "PE", "PR"), svcRS));
        lista.Add(new Autorizador("AN", "91", [], an));
        return lista;
    }

    private static string[] Codigos(params string[] siglas) => [.. siglas.Select(UnidadesFederativas.Codigo)];
}

// Os códigos IBGE das UFs (MOC 7.0, Visão Geral, 8.1), em tabela própria pelo mesmo motivo.
internal static class UnidadesFederativas
{
    private static readonly Dictionary<string, string> Tabela = new(StringComparer.Ordinal)
    {
        ["RO"] = "11", ["AC"] = "12", ["AM"] = "13", ["RR"] = "14", ["PA"] = "15", ["AP"] = "16", ["TO"] = "17",
        ["MA"] = "21", ["PI"] = "22", ["CE"] = "23", ["RN"] = "24", ["PB"] = "25", ["PE"] = "26", ["AL"] = "27",
        ["SE"] = "28", ["BA"] = "29", ["MG"] = "31", ["ES"] = "32", ["RJ"] = "33", ["SP"] = "35", ["PR"] = "41",
        ["SC"] = "42", ["RS"] = "43", ["MS"] = "50", ["MT"] = "51", ["GO"] = "52", ["DF"] = "53",
    };

    public static string Codigo(string sigla) => Tabela[sigla];

    public static bool Existe(string codigo) => Tabela.ContainsValue(codigo);
}
