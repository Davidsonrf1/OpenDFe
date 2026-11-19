using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenDFe.SefazEmulada;

// A API de parâmetros municipais do ADN (E.8), respondendo com a configuração dos municípios da E.7. A base de produção
// é https://adn.nfse.gov.br/parametrizacao (docs/NTS.md 12.4); o emulador usa o prefixo /parametrizacao. O manual dos
// contribuintes (1.2.1) dá rotas /parametros_municipais/... que não batem com essa base (docs/NFSE.md D13); as rotas e
// os nomes do JSON abaixo são os de uma implementação aberta que consulta o ADN de verdade (a nfse4j, MIT, docs/NTS.md
// 12.4), a conferir no Swagger quando houver o PFX:
//   GET /parametrizacao/{municipio}/convenio
//   GET /parametrizacao/{municipio}/{servico}/{competencia}/aliquota
//   GET /parametrizacao/{municipio}/{servico}/historicoaliquotas
//   GET /parametrizacao/{municipio}/{servico}/{competencia}/regimes_especiais
//   GET /parametrizacao/{municipio}/{competencia}/retencoes
//   GET /parametrizacao/{municipio}/{beneficio}/{competencia}/beneficio
// O serviço é o código da lista nacional com o complemento municipal (XX.XX.XX.XXX: o cTribNac e 000); a competência é
// MM-DD-AAAA. O emulador não tem vigência: a competência é conferida, mas não escolhe nada; as datas são a do início do
// Sistema Nacional. O município com o convênio inexistente é o 404, como o não conveniado.
internal sealed partial class ParametrizacaoMunicipal(Municipios municipios)
{
    public const string Destino = "ADN";
    public const string Prefixo = "/parametrizacao";
    public const string ServicoConvenio = "ConsultaConvenio";
    public const string ServicoAliquota = "ConsultaAliquota";
    public const string ServicoHistorico = "ConsultaHistoricoAliquotas";
    public const string ServicoRegimes = "ConsultaRegimesEspeciais";
    public const string ServicoRetencoes = "ConsultaRetencoes";
    public const string ServicoBeneficio = "ConsultaBeneficio";

    public static IReadOnlyList<string> Servicos { get; } =
        [ServicoConvenio, ServicoAliquota, ServicoHistorico, ServicoRegimes, ServicoRetencoes, ServicoBeneficio];

    // Os tpRetISSQN que o responsável pela retenção cobre: o tomador (2) e o intermediário (3).
    private static readonly int[] TiposRetencao = [2, 3];

    // O início das vigências que o emulador mostra: o do Sistema Nacional NFS-e (julho de 2022, docs/NFSE.md D8).
    private const string Inicio = "2022-07-01T00:00:00";

    public static void Mapear(WebApplication app)
    {
        var grupo = app.MapGroup(Prefixo);
        grupo.MapGet("/{municipio}/convenio", (HttpContext contexto, string municipio, AtendimentoSefin atendimento, ParametrizacaoMunicipal parametrizacao) =>
            atendimento.ConsultarParametroAsync(contexto, ServicoConvenio, instante => parametrizacao.Convenio(municipio, instante)));
        grupo.MapGet("/{municipio}/{servico}/{competencia}/aliquota", (HttpContext contexto, string municipio, string servico, string competencia,
            AtendimentoSefin atendimento, ParametrizacaoMunicipal parametrizacao) =>
            atendimento.ConsultarParametroAsync(contexto, ServicoAliquota, instante => parametrizacao.Aliquota(municipio, servico, competencia, instante)));
        grupo.MapGet("/{municipio}/{servico}/historicoaliquotas", (HttpContext contexto, string municipio, string servico,
            AtendimentoSefin atendimento, ParametrizacaoMunicipal parametrizacao) =>
            atendimento.ConsultarParametroAsync(contexto, ServicoHistorico, instante => parametrizacao.Aliquota(municipio, servico, null, instante)));
        grupo.MapGet("/{municipio}/{servico}/{competencia}/regimes_especiais", (HttpContext contexto, string municipio, string servico, string competencia,
            AtendimentoSefin atendimento, ParametrizacaoMunicipal parametrizacao) =>
            atendimento.ConsultarParametroAsync(contexto, ServicoRegimes, instante => parametrizacao.Regimes(municipio, servico, competencia, instante)));
        grupo.MapGet("/{municipio}/{competencia}/retencoes", (HttpContext contexto, string municipio, string competencia,
            AtendimentoSefin atendimento, ParametrizacaoMunicipal parametrizacao) =>
            atendimento.ConsultarParametroAsync(contexto, ServicoRetencoes, instante => parametrizacao.Retencoes(municipio, competencia, instante)));
        grupo.MapGet("/{municipio}/{beneficio}/{competencia}/beneficio", (HttpContext contexto, string municipio, string beneficio, string competencia,
            AtendimentoSefin atendimento, ParametrizacaoMunicipal parametrizacao) =>
            atendimento.ConsultarParametroAsync(contexto, ServicoBeneficio, instante => parametrizacao.Beneficio(municipio, beneficio, competencia, instante)));
    }

    // O convênio: aderente ao ambiente nacional (o convênio ativo) e ao emissor nacional.
    public ResultadoSefin Convenio(string codigo, DateTimeOffset instante) => Responder(codigo, null, null, instante, municipio =>
        new { parametrosConvenio = new { aderenteAmbienteNacional = municipio.Convenio == SituacaoConvenio.Ativo, aderenteEmissorNacional = municipio.Emissor == EmissorMunicipio.Nacional } },
        "convênio");

    // A alíquota do serviço (a dele, ou a padrão do município), na competência ou o histórico (competência nula).
    public ResultadoSefin Aliquota(string codigo, string servico, string? competencia, DateTimeOffset instante) => Responder(codigo, servico, competencia, instante, municipio =>
        new
        {
            aliquotas = new Dictionary<string, object[]>
            {
                [servico] = [new AliquotaServico(municipio.Aliquota(servico[..2] + servico[3..5] + servico[6..8]), Inicio, null)],
            },
        }, "alíquota");

    // Os regimes especiais admitidos no município, para o serviço: o regEspTrib, com a situação 1 (permitido).
    public ResultadoSefin Regimes(string codigo, string servico, string competencia, DateTimeOffset instante) => Responder(codigo, servico, competencia, instante, municipio =>
        new
        {
            regimesEspeciais = new Dictionary<string, Dictionary<string, object[]>>
            {
                [servico] = (municipio.RegimesEspeciais ?? []).ToDictionary(r => r.ToString(CultureInfo.InvariantCulture),
                    _ => new object[] { new RegimeServico(1, Inicio, null) }),
            },
        }, "regimes especiais");

    // As retenções: os CPFs que o município autoriza como responsáveis (os tipos 2, tomador, e 3, intermediário, do
    // tpRetISSQN); o art. 6º da LC 116 desabilitado.
    public ResultadoSefin Retencoes(string codigo, string competencia, DateTimeOffset instante) => Responder(codigo, null, competencia, instante, municipio =>
        new
        {
            retencoes = new
            {
                art6 = new { habilitado = false, hist = Array.Empty<object>() },
                retMun = (municipio.Retencoes ?? []).Count == 0 ? Array.Empty<object>() : new object[]
                {
                    new
                    {
                        desc = "Retenção autorizada pelo município (emulada)",
                        tpRet = TiposRetencao,
                        serv = Array.Empty<object>(),
                        respTrib = municipio.Retencoes!.Select(cpf => new { tpInsc = 1, insc = cpf, hist = new[] { new Vigencia(Inicio, null) } }),
                    },
                },
            },
        }, "retenções");

    // O benefício municipal pelo número (14 dígitos); 404 se o município não o tem.
    public ResultadoSefin Beneficio(string codigo, string numero, string competencia, DateTimeOffset instante) => Responder(codigo, null, competencia, instante, municipio =>
        (municipio.Beneficios ?? []).FirstOrDefault(b => b.Numero == numero) is not { } beneficio ? null : new
        {
            beneficio = new
            {
                numBenef = beneficio.Numero,
                desc = $"Benefício municipal {beneficio.Numero} (emulado)",
                dtIni = Inicio,
                tpoBenef = beneficio.Tipo,
                aliqDiferenc = beneficio.Aliquota,
                restAoMun = false,
                serv = Array.Empty<object>(),
                contrib = Array.Empty<object>(),
            },
        }, "benefício");

    // Confere o município (400 sem 7 dígitos e a UF; 404 com o convênio inexistente), o serviço e a competência (400), e
    // monta a resposta; nula é o 404 (o que se procurou não existe).
    private ResultadoSefin Responder(string codigo, string? servico, string? competencia, DateTimeOffset instante, Func<Municipio, object?> montar, string nome)
    {
        if (!CodigoMunicipio().IsMatch(codigo) || Municipio.Uf(int.Parse(codigo, CultureInfo.InvariantCulture)) is null)
            return Erro(StatusCodes.Status400BadRequest, instante, "O código do município tem 7 dígitos, com o código IBGE da UF nos dois primeiros.", "município inválido");
        if (servico is not null && !CodigoServico().IsMatch(servico))
            return Erro(StatusCodes.Status400BadRequest, instante, "O código do serviço é XX.XX.XX.XXX.", "serviço inválido");
        if (competencia is not null && !DateOnly.TryParseExact(competencia, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            return Erro(StatusCodes.Status400BadRequest, instante, "A competência é MM-DD-AAAA.", "competência inválida");
        var (municipio, _) = municipios.Procurar(int.Parse(codigo, CultureInfo.InvariantCulture));
        if (municipio.Convenio == SituacaoConvenio.Inexistente)
            return Erro(StatusCodes.Status404NotFound, instante, $"O município {codigo} não tem convênio com o Sistema Nacional NFS-e.", "município sem convênio");
        if (montar(municipio) is not { } corpo)
            return Erro(StatusCodes.Status404NotFound, instante, $"Não há {nome} com os parâmetros informados.", nome + " não encontrado");
        return new ResultadoSefin(StatusCodes.Status200OK, corpo, $"{nome} do município {codigo}");
    }

    private static ResultadoSefin Erro(int status, DateTimeOffset instante, string mensagem, string situacao) =>
        new(status, new { tipoAmbiente = 2, dataHoraProcessamento = EmissaoNFSe.DataHora(instante), mensagem }, situacao);

    [GeneratedRegex("^[0-9]{7}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodigoMunicipio();

    [GeneratedRegex("^[0-9]{2}\\.[0-9]{2}\\.[0-9]{2}\\.[0-9]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodigoServico();

    // Os itens do JSON, com os nomes da fonte (o "Aliq" e as datas com maiúscula na alíquota).
    private sealed record AliquotaServico(
        [property: JsonPropertyName("Aliq")] decimal Aliquota,
        [property: JsonPropertyName("DtIni")] string Inicio,
        [property: JsonPropertyName("DtFim")] string? Fim);

    private sealed record RegimeServico(
        [property: JsonPropertyName("sit")] int Situacao,
        [property: JsonPropertyName("dtIni")] string Inicio,
        [property: JsonPropertyName("dtFim")] string? Fim);

    private sealed record Vigencia(
        [property: JsonPropertyName("dtIni")] string Inicio,
        [property: JsonPropertyName("dtFim")] string? Fim);
}
