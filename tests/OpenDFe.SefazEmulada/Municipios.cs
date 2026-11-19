using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenDFe.SefazEmulada;

// A situação do convênio do município com o Sistema Nacional da NFS-e (Anexo I, E0037 e E0038).
[JsonConverter(typeof(JsonStringEnumConverter<SituacaoConvenio>))]
internal enum SituacaoConvenio
{
    [JsonStringEnumMemberName("ativo")] Ativo = 1,
    [JsonStringEnumMemberName("inativo")] Inativo = 2,
    [JsonStringEnumMemberName("inexistente")] Inexistente = 3,
}

// Quem emite no município (Anexo I, E0039): os emissores públicos nacionais (a Sefin) ou o sistema próprio dele.
[JsonConverter(typeof(JsonStringEnumConverter<EmissorMunicipio>))]
internal enum EmissorMunicipio
{
    [JsonStringEnumMemberName("nacional")] Nacional = 1,
    [JsonStringEnumMemberName("proprio")] Proprio = 2,
}

// Um benefício municipal (o nBM da DPS, TCBeneficioMunicipal): o número de 14 dígitos e o tipo, como o tpBM da NFS-e
// (1 isenção, 2 redução da base em percentual, 3 redução da base em valor, 4 alíquota diferenciada). O percentual é a
// alíquota da diferenciada; nos outros tipos, não vale.
internal sealed record BeneficioMunicipio(string Numero, int Tipo, decimal? Aliquota = null);

// Um município como chega no POST /_controle/municipios e como o emulador o usa (E.7). Nulo é o padrão: o município
// não configurado é conveniado, com o emissor nacional e a alíquota padrão (decidido em 2026-11-19, PLANO.md E.7).
//   Codigo            o código IBGE (7 dígitos), com a UF nos dois primeiros
//   Nome              o nome, para o xLocEmi e o xLocPrestacao da NFS-e
//   Convenio          ativo (o padrão), inativo (E0038) ou inexistente (E0037)
//   Emissor           nacional (o padrão) ou proprio (E0039)
//   AliquotaPadrao    a alíquota do ISS (0 a 5) quando o código de tributação nacional não tem a sua
//   Aliquotas         a alíquota por código de tributação nacional (6 dígitos)
//   RegimesEspeciais  os regimes especiais admitidos (o regEspTrib, 1 a 6 e 9); outro na DPS é o E0177
//   Retencoes         os CPFs que o município autoriza como responsáveis pela retenção (E0667 e E0670)
//   Beneficios        os benefícios municipais; outro nBM na DPS é o E0541
//   Paralisado        a Sefin responde 503 para as DPS do município
//   PrazoCancelamento os dias depois da emissão em que o cancelamento (101101) é aceito (E0822); nulo, sem prazo (E.8)
//   PrazoSubstituicao os dias em que a substituição é aceita (E0050), salvo os motivos 01 e 02; nulo, sem prazo (E.8)
internal sealed record Municipio(
    int Codigo,
    string? Nome = null,
    SituacaoConvenio? Convenio = null,
    EmissorMunicipio? Emissor = null,
    decimal? AliquotaPadrao = null,
    IReadOnlyDictionary<string, decimal>? Aliquotas = null,
    IReadOnlyList<int>? RegimesEspeciais = null,
    IReadOnlyList<string>? Retencoes = null,
    IReadOnlyList<BeneficioMunicipio>? Beneficios = null,
    bool Paralisado = false,
    int? PrazoCancelamento = null,
    int? PrazoSubstituicao = null)
{
    // A alíquota do município não configurado e do configurado sem a padrão: o mínimo da LC 116, art. 8º-A.
    public const decimal AliquotaPadraoEmulador = 2.00m;

    // O município com os padrões preenchidos e as listas sem nulo.
    public Municipio Completar() => this with
    {
        Nome = Nome ?? $"Município {Codigo.ToString(CultureInfo.InvariantCulture)}",
        Convenio = Convenio ?? SituacaoConvenio.Ativo,
        Emissor = Emissor ?? EmissorMunicipio.Nacional,
        AliquotaPadrao = AliquotaPadrao ?? AliquotaPadraoEmulador,
        Aliquotas = Aliquotas ?? new Dictionary<string, decimal>(),
        RegimesEspeciais = RegimesEspeciais ?? [],
        Retencoes = Retencoes ?? [],
        Beneficios = Beneficios ?? [],
    };

    // A alíquota do código de tributação nacional: a dele ou a padrão.
    public decimal Aliquota(string tributacaoNacional) =>
        Aliquotas is not null && Aliquotas.TryGetValue(tributacaoNacional, out var aliquota) ? aliquota : AliquotaPadrao ?? AliquotaPadraoEmulador;

    // A sigla da UF pelo código IBGE (os dois primeiros dígitos); nula se não é UF.
    public static string? Uf(int codigo) =>
        codigo is >= 1000000 and <= 9999999 ? UnidadesFederativas.Sigla((codigo / 100000).ToString(CultureInfo.InvariantCulture)) : null;
}

// Os municípios configurados pela rota /_controle/municipios. Em memória, vazio a cada partida; seguro para uso
// concorrente. A persistência fica para a E.9.
internal sealed partial class Municipios
{
    private readonly Lock _trava = new();
    private readonly SortedDictionary<int, Municipio> _municipios = [];

    // Confere (ArgumentException com o que está errado) e cadastra; verdadeiro se é novo, falso se trocou o que havia.
    public bool Cadastrar(Municipio municipio)
    {
        ArgumentNullException.ThrowIfNull(municipio);
        Conferir(municipio);
        lock (_trava)
        {
            var novo = !_municipios.ContainsKey(municipio.Codigo);
            _municipios[municipio.Codigo] = municipio.Completar();
            return novo;
        }
    }

    // O configurado, ou o padrão (conveniado, com o emissor nacional e a alíquota padrão), com Configurado falso.
    public (Municipio Municipio, bool Configurado) Procurar(int codigo)
    {
        lock (_trava)
        {
            return _municipios.TryGetValue(codigo, out var municipio)
                ? (municipio, true)
                : (new Municipio(codigo).Completar(), false);
        }
    }

    public IReadOnlyList<Municipio> Listar()
    {
        lock (_trava)
            return [.. _municipios.Values];
    }

    public bool Remover(int codigo)
    {
        lock (_trava)
            return _municipios.Remove(codigo);
    }

    public void Limpar()
    {
        lock (_trava)
            _municipios.Clear();
    }

    private static void Conferir(Municipio municipio)
    {
        if (Municipio.Uf(municipio.Codigo) is null)
            throw new ArgumentException("O código do município tem 7 dígitos, com o código IBGE da UF nos dois primeiros.", nameof(municipio));
        if (municipio.Nome is { } nome && (string.IsNullOrWhiteSpace(nome) || nome.Length > 150))
            throw new ArgumentException("O nome tem de 1 a 150 caracteres.", nameof(municipio));
        if (municipio.Convenio is { } convenio && !Enum.IsDefined(convenio))
            throw new ArgumentException("O convênio é ativo, inativo ou inexistente.", nameof(municipio));
        if (municipio.Emissor is { } emissor && !Enum.IsDefined(emissor))
            throw new ArgumentException("O emissor é nacional ou proprio.", nameof(municipio));
        if (municipio.AliquotaPadrao is { } padrao)
            ConferirAliquota(padrao, "A alíquota padrão");
        if (municipio.PrazoCancelamento is < 0 or > 3650 || municipio.PrazoSubstituicao is < 0 or > 3650)
            throw new ArgumentException("Os prazos do cancelamento e da substituição vão de 0 a 3650 dias.", nameof(municipio));
        foreach (var (codigo, aliquota) in municipio.Aliquotas ?? new Dictionary<string, decimal>())
        {
            if (!TributacaoNacional().IsMatch(codigo))
                throw new ArgumentException($"O código de tributação nacional tem 6 dígitos: {codigo}.", nameof(municipio));
            ConferirAliquota(aliquota, $"A alíquota do código {codigo}");
        }
        foreach (var regime in municipio.RegimesEspeciais ?? [])
        {
            if (regime is not (>= 1 and <= 6 or 9))
                throw new ArgumentException($"O regime especial é de 1 a 6 ou 9 (o 0, nenhum, é sempre admitido): {regime}.", nameof(municipio));
        }
        foreach (var cpf in municipio.Retencoes ?? [])
        {
            if (!Documentos.CpfValido(cpf))
                throw new ArgumentException($"A retenção é autorizada por CPF, com 11 dígitos e o DV: {cpf}.", nameof(municipio));
        }
        var numeros = new HashSet<string>(StringComparer.Ordinal);
        foreach (var beneficio in municipio.Beneficios ?? [])
        {
            if (beneficio.Numero is null || !NumeroBeneficio().IsMatch(beneficio.Numero))
                throw new ArgumentException($"O número do benefício tem 14 dígitos: {beneficio.Numero}.", nameof(municipio));
            if (!numeros.Add(beneficio.Numero))
                throw new ArgumentException($"Benefício repetido: {beneficio.Numero}.", nameof(municipio));
            if (beneficio.Tipo is not (>= 1 and <= 4))
                throw new ArgumentException("O tipo do benefício é 1 (isenção), 2 (redução em percentual), 3 (redução em valor) ou 4 (alíquota diferenciada).", nameof(municipio));
            if (beneficio.Tipo == 4)
                ConferirAliquota(beneficio.Aliquota ?? throw new ArgumentException("A alíquota diferenciada precisa da alíquota.", nameof(municipio)), "A alíquota do benefício");
            else if (beneficio.Aliquota is not null)
                throw new ArgumentException("A alíquota do benefício só vale para a alíquota diferenciada (tipo 4).", nameof(municipio));
        }
    }

    private static void ConferirAliquota(decimal aliquota, string nome)
    {
        if (aliquota is < 0 or > 5 || decimal.Round(aliquota, 2) != aliquota)
            throw new ArgumentException($"{nome} vai de 0 a 5, com até duas casas.", nameof(aliquota));
    }

    [GeneratedRegex("^[0-9]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex TributacaoNacional();

    [GeneratedRegex("^[0-9]{14}$", RegexOptions.CultureInvariant)]
    private static partial Regex NumeroBeneficio();
}
