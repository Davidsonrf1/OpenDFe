using System.Globalization;
using System.Text.Json;

namespace OpenDFe.NFSe;

// A parametrização municipal no ADN (etapa 9.4; docs/NFSE.md 11, aprovado em 2026-11-19): cinco consultas GET, no destino
// ADN, com as rotas de uma implementação aberta que consulta o ADN de verdade (docs/NTS.md 12.4), a conferir no Swagger
// quando houver o PFX. Cada retorno traz os campos que se conseguiu confirmar e o JSON inteiro ao lado (D13); o campo que
// não se lê fica nulo, com o aviso NFSE-PARAMETRO (P2). O CStat é o status HTTP; o Motivo, a mensagem do erro (P6).

/// <summary>A consulta do convênio do município com o Sistema Nacional NFS-e (<c>GET /{municipio}/convenio</c>).</summary>
public sealed class ConsultaConvenio : ParametroWS<RetornoConvenio>
{
    /// <summary>A consulta do município.</summary>
    /// <param name="municipio">O código IBGE (7 dígitos).</param>
    /// <exception cref="ArgumentOutOfRangeException">O código não tem 7 dígitos.</exception>
    public ConsultaConvenio(int municipio) => Municipio = Parametrizacao.Municipio(municipio, nameof(municipio));

    /// <summary>O código IBGE do município.</summary>
    public int Municipio { get; }
}

/// <summary>O convênio do município: a adesão ao ambiente nacional e ao emissor nacional.</summary>
public sealed class RetornoConvenio
{
    internal RetornoConvenio(bool encontrado, bool? ambiente, bool? emissor, string? mensagem, JsonElement? json)
    {
        Encontrado = encontrado;
        AderenteAmbienteNacional = ambiente;
        AderenteEmissorNacional = emissor;
        Mensagem = mensagem;
        Json = json;
    }

    /// <summary>O ADN respondeu com o convênio (2xx).</summary>
    public bool Encontrado { get; }

    /// <summary>O município aderiu ao ambiente nacional (<c>aderenteAmbienteNacional</c>): a NFS-e dele passa pelo ADN.</summary>
    public bool? AderenteAmbienteNacional { get; }

    /// <summary>O município aderiu ao emissor nacional (<c>aderenteEmissorNacional</c>): a Sefin Nacional emite a NFS-e dele.</summary>
    public bool? AderenteEmissorNacional { get; }

    /// <summary>A mensagem do erro (o 404 do município sem convênio, o 400); nula no sucesso.</summary>
    public string? Mensagem { get; }

    /// <summary>O JSON da resposta inteiro, como veio (D13); nulo sem corpo.</summary>
    public JsonElement? Json { get; }
}

/// <summary>
/// A consulta da alíquota do ISS de um serviço no município: na competência
/// (<c>GET /{municipio}/{servico}/{competencia}/aliquota</c>) ou o histórico, sem ela
/// (<c>GET /{municipio}/{servico}/historicoaliquotas</c>).
/// </summary>
public sealed class ConsultaAliquota : ParametroWS<RetornoAliquota>
{
    /// <summary>A consulta do serviço no município.</summary>
    /// <param name="municipio">O código IBGE (7 dígitos).</param>
    /// <param name="tributacaoNacional">O código de tributação nacional (<c>cTribNac</c>, 6 dígitos).</param>
    /// <param name="complemento">O complemento municipal do código (3 dígitos); o padrão é <c>000</c>.</param>
    /// <exception cref="ArgumentOutOfRangeException">O município não tem 7 dígitos.</exception>
    /// <exception cref="ArgumentException">O código ou o complemento fora da forma.</exception>
    public ConsultaAliquota(int municipio, string tributacaoNacional, string complemento = "000")
    {
        Municipio = Parametrizacao.Municipio(municipio, nameof(municipio));
        Servico = Parametrizacao.CodigoServico(tributacaoNacional, complemento);
    }

    /// <summary>O código IBGE do município.</summary>
    public int Municipio { get; }

    /// <summary>O serviço como vai no caminho: <c>XX.XX.XX.XXX</c>.</summary>
    public string Servico { get; }

    /// <summary>A competência; <see langword="null"/> (o padrão), o histórico das alíquotas.</summary>
    public DateOnly? Competencia { get; init; }
}

/// <summary>As alíquotas do serviço, com a vigência de cada uma.</summary>
public sealed class RetornoAliquota
{
    internal RetornoAliquota(bool encontrado, IReadOnlyList<VigenciaAliquota> aliquotas, string? mensagem, JsonElement? json)
    {
        Encontrado = encontrado;
        Aliquotas = aliquotas;
        Mensagem = mensagem;
        Json = json;
    }

    /// <summary>O ADN respondeu com as alíquotas (2xx).</summary>
    public bool Encontrado { get; }

    /// <summary>As alíquotas do serviço consultado, na ordem da resposta.</summary>
    public IReadOnlyList<VigenciaAliquota> Aliquotas { get; }

    /// <summary>A mensagem do erro; nula no sucesso.</summary>
    public string? Mensagem { get; }

    /// <summary>O JSON da resposta inteiro, como veio (D13); nulo sem corpo.</summary>
    public JsonElement? Json { get; }
}

/// <summary>Uma alíquota e a vigência dela (<c>Aliq</c>, <c>DtIni</c>, <c>DtFim</c>).</summary>
public sealed class VigenciaAliquota
{
    internal VigenciaAliquota(decimal? aliquota, DateTime? inicio, DateTime? fim)
    {
        Aliquota = aliquota;
        Inicio = inicio;
        Fim = fim;
    }

    /// <summary>A alíquota, em percentual (<c>2.5</c> = 2,5%).</summary>
    public decimal? Aliquota { get; }

    /// <summary>O início da vigência, sem fuso.</summary>
    public DateTime? Inicio { get; }

    /// <summary>O fim da vigência, sem fuso; nulo, vigente.</summary>
    public DateTime? Fim { get; }
}

/// <summary>A consulta dos regimes especiais de um serviço no município (<c>GET /{municipio}/{servico}/{competencia}/regimes_especiais</c>).</summary>
public sealed class ConsultaRegimesEspeciais : ParametroWS<RetornoRegimesEspeciais>
{
    /// <summary>A consulta do serviço no município, na competência.</summary>
    /// <param name="municipio">O código IBGE (7 dígitos).</param>
    /// <param name="tributacaoNacional">O código de tributação nacional (<c>cTribNac</c>, 6 dígitos).</param>
    /// <param name="competencia">A competência.</param>
    /// <param name="complemento">O complemento municipal do código (3 dígitos); o padrão é <c>000</c>.</param>
    /// <exception cref="ArgumentOutOfRangeException">O município não tem 7 dígitos.</exception>
    /// <exception cref="ArgumentException">O código ou o complemento fora da forma.</exception>
    public ConsultaRegimesEspeciais(int municipio, string tributacaoNacional, DateOnly competencia, string complemento = "000")
    {
        Municipio = Parametrizacao.Municipio(municipio, nameof(municipio));
        Servico = Parametrizacao.CodigoServico(tributacaoNacional, complemento);
        Competencia = competencia;
    }

    /// <summary>O código IBGE do município.</summary>
    public int Municipio { get; }

    /// <summary>O serviço como vai no caminho: <c>XX.XX.XX.XXX</c>.</summary>
    public string Servico { get; }

    /// <summary>A competência.</summary>
    public DateOnly Competencia { get; }
}

/// <summary>Os regimes especiais do serviço no município.</summary>
public sealed class RetornoRegimesEspeciais
{
    internal RetornoRegimesEspeciais(bool encontrado, IReadOnlyList<RegimeEspecialMunicipal> regimes, string? mensagem, JsonElement? json)
    {
        Encontrado = encontrado;
        Regimes = regimes;
        Mensagem = mensagem;
        Json = json;
    }

    /// <summary>O ADN respondeu com os regimes (2xx), mesmo sem nenhum.</summary>
    public bool Encontrado { get; }

    /// <summary>Os regimes do serviço consultado, cada vigência um item, na ordem da resposta.</summary>
    public IReadOnlyList<RegimeEspecialMunicipal> Regimes { get; }

    /// <summary>A mensagem do erro; nula no sucesso.</summary>
    public string? Mensagem { get; }

    /// <summary>O JSON da resposta inteiro, como veio (D13); nulo sem corpo.</summary>
    public JsonElement? Json { get; }
}

/// <summary>Um regime especial no município: o <c>regEspTrib</c>, a situação e a vigência.</summary>
public sealed class RegimeEspecialMunicipal
{
    internal RegimeEspecialMunicipal(RegimeEspecialTributacao? regime, int? situacao, DateTime? inicio, DateTime? fim)
    {
        Regime = regime;
        Situacao = situacao;
        Inicio = inicio;
        Fim = fim;
    }

    /// <summary>O regime (o código da chave do JSON); nulo se a enumeração não o tem.</summary>
    public RegimeEspecialTributacao? Regime { get; }

    /// <summary>A situação (<c>sit</c>) como veio: o significado não tem fonte (o emulador manda 1, permitido).</summary>
    public int? Situacao { get; }

    /// <summary>O início da vigência, sem fuso.</summary>
    public DateTime? Inicio { get; }

    /// <summary>O fim da vigência, sem fuso; nulo, vigente.</summary>
    public DateTime? Fim { get; }
}

/// <summary>A consulta das retenções do ISS que o município define (<c>GET /{municipio}/{competencia}/retencoes</c>).</summary>
public sealed class ConsultaRetencoes : ParametroWS<RetornoRetencoes>
{
    /// <summary>A consulta do município, na competência.</summary>
    /// <param name="municipio">O código IBGE (7 dígitos).</param>
    /// <param name="competencia">A competência.</param>
    /// <exception cref="ArgumentOutOfRangeException">O município não tem 7 dígitos.</exception>
    public ConsultaRetencoes(int municipio, DateOnly competencia)
    {
        Municipio = Parametrizacao.Municipio(municipio, nameof(municipio));
        Competencia = competencia;
    }

    /// <summary>O código IBGE do município.</summary>
    public int Municipio { get; }

    /// <summary>A competência.</summary>
    public DateOnly Competencia { get; }
}

/// <summary>As retenções do município: a do art. 6º da LC 116 e as que ele autoriza.</summary>
public sealed class RetornoRetencoes
{
    internal RetornoRetencoes(bool encontrado, bool? artigo6, IReadOnlyList<RetencaoMunicipal> retencoes, string? mensagem, JsonElement? json)
    {
        Encontrado = encontrado;
        Artigo6Habilitado = artigo6;
        Retencoes = retencoes;
        Mensagem = mensagem;
        Json = json;
    }

    /// <summary>O ADN respondeu com as retenções (2xx).</summary>
    public bool Encontrado { get; }

    /// <summary>A retenção do art. 6º da LC 116 habilitada no município (<c>art6.habilitado</c>).</summary>
    public bool? Artigo6Habilitado { get; }

    /// <summary>As retenções que o município autoriza (<c>retMun</c>), na ordem da resposta.</summary>
    public IReadOnlyList<RetencaoMunicipal> Retencoes { get; }

    /// <summary>A mensagem do erro; nula no sucesso.</summary>
    public string? Mensagem { get; }

    /// <summary>O JSON da resposta inteiro, como veio (D13); nulo sem corpo.</summary>
    public JsonElement? Json { get; }
}

/// <summary>Uma retenção autorizada pelo município: a descrição, os tipos (<c>tpRetISSQN</c>) e os responsáveis.</summary>
public sealed class RetencaoMunicipal
{
    internal RetencaoMunicipal(string? descricao, IReadOnlyList<RetencaoIssqn> tipos, IReadOnlyList<string> responsaveis)
    {
        Descricao = descricao;
        Tipos = tipos;
        Responsaveis = responsaveis;
    }

    /// <summary>A descrição (<c>desc</c>).</summary>
    public string? Descricao { get; }

    /// <summary>Os tipos de retenção que ela cobre (<c>tpRet</c>), o que a enumeração tem.</summary>
    public IReadOnlyList<RetencaoIssqn> Tipos { get; }

    /// <summary>O CPF ou o CNPJ de cada responsável autorizado (<c>respTrib[].insc</c>).</summary>
    public IReadOnlyList<string> Responsaveis { get; }
}

/// <summary>A consulta de um benefício municipal pelo número (<c>GET /{municipio}/{beneficio}/{competencia}/beneficio</c>).</summary>
public sealed class ConsultaBeneficio : ParametroWS<RetornoBeneficio>
{
    /// <summary>A consulta do benefício no município, na competência.</summary>
    /// <param name="municipio">O código IBGE (7 dígitos).</param>
    /// <param name="numero">O número do benefício (<c>nBM</c>, 14 dígitos).</param>
    /// <param name="competencia">A competência.</param>
    /// <exception cref="ArgumentOutOfRangeException">O município não tem 7 dígitos.</exception>
    /// <exception cref="ArgumentException">O número não tem 14 dígitos.</exception>
    public ConsultaBeneficio(int municipio, string numero, DateOnly competencia)
    {
        Municipio = Parametrizacao.Municipio(municipio, nameof(municipio));
        if (numero is null || numero.Length != 14 || !Parametrizacao.SoDigitos(numero))
            throw new ArgumentException("O número do benefício municipal tem 14 dígitos.", nameof(numero));
        Numero = numero;
        Competencia = competencia;
    }

    /// <summary>O código IBGE do município.</summary>
    public int Municipio { get; }

    /// <summary>O número do benefício.</summary>
    public string Numero { get; }

    /// <summary>A competência.</summary>
    public DateOnly Competencia { get; }
}

/// <summary>O benefício municipal: o tipo, a alíquota diferenciada e a vigência.</summary>
public sealed class RetornoBeneficio
{
    internal RetornoBeneficio(bool encontrado, string? numero, string? descricao, TipoBeneficio? tipo, decimal? aliquota,
        DateTime? inicio, DateTime? fim, string? mensagem, JsonElement? json)
    {
        Encontrado = encontrado;
        Numero = numero;
        Descricao = descricao;
        Tipo = tipo;
        AliquotaDiferenciada = aliquota;
        Inicio = inicio;
        Fim = fim;
        Mensagem = mensagem;
        Json = json;
    }

    /// <summary>O ADN respondeu com o benefício (2xx); o que o município não tem é o 404.</summary>
    public bool Encontrado { get; }

    /// <summary>O número (<c>numBenef</c>).</summary>
    public string? Numero { get; }

    /// <summary>A descrição (<c>desc</c>).</summary>
    public string? Descricao { get; }

    /// <summary>O tipo (<c>tpoBenef</c>).</summary>
    public TipoBeneficio? Tipo { get; }

    /// <summary>A alíquota diferenciada, em percentual (<c>aliqDiferenc</c>), no tipo 4.</summary>
    public decimal? AliquotaDiferenciada { get; }

    /// <summary>O início da vigência, sem fuso.</summary>
    public DateTime? Inicio { get; }

    /// <summary>O fim da vigência, sem fuso; nulo, vigente.</summary>
    public DateTime? Fim { get; }

    /// <summary>A mensagem do erro; nula no sucesso.</summary>
    public string? Mensagem { get; }

    /// <summary>O JSON da resposta inteiro, como veio (D13); nulo sem corpo.</summary>
    public JsonElement? Json { get; }
}

internal sealed class OperacaoConsultaConvenio : IOperacaoRest<ConsultaConvenio, RetornoConvenio>
{
    public PedidoRest Montar(ConsultaConvenio parametro, ContextoOperacao contexto) =>
        Parametrizacao.Pedido(Codigo(parametro.Municipio) + "/convenio");

    public LeituraWS<RetornoConvenio> Ler(ConsultaConvenio parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var l = LeituraParametro.Ler(resposta);
        bool? ambiente = null, emissor = null;
        if (l.Sucesso && l.Grupo("parametrosConvenio", "AderenteAmbienteNacional") is { } grupo)
        {
            ambiente = l.Logico(grupo, "aderenteAmbienteNacional", "parametrosConvenio/aderenteAmbienteNacional", "AderenteAmbienteNacional");
            emissor = l.Logico(grupo, "aderenteEmissorNacional", "parametrosConvenio/aderenteEmissorNacional", "AderenteEmissorNacional");
        }
        return Resultado(l, new RetornoConvenio(l.Sucesso, ambiente, emissor, l.Mensagem, l.Raiz), "Convênio encontrado");
    }

    internal static string Codigo(int municipio) => municipio.ToString(CultureInfo.InvariantCulture);

    internal static LeituraWS<T> Resultado<T>(LeituraParametro l, T retorno, string sucesso) where T : class =>
        new(retorno, l.Status, l.Motivo(sucesso)) { Problemas = l.Problemas };
}

internal sealed class OperacaoConsultaAliquota : IOperacaoRest<ConsultaAliquota, RetornoAliquota>
{
    public PedidoRest Montar(ConsultaAliquota parametro, ContextoOperacao contexto) =>
        Parametrizacao.Pedido(parametro.Competencia is { } competencia
            ? $"{OperacaoConsultaConvenio.Codigo(parametro.Municipio)}/{parametro.Servico}/{Parametrizacao.Competencia(competencia)}/aliquota"
            : $"{OperacaoConsultaConvenio.Codigo(parametro.Municipio)}/{parametro.Servico}/historicoaliquotas");

    public LeituraWS<RetornoAliquota> Ler(ConsultaAliquota parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var l = LeituraParametro.Ler(resposta);
        var aliquotas = new List<VigenciaAliquota>();
        if (l.Sucesso && l.Grupo("aliquotas", "Aliquotas") is { } grupo
            && DoServico(l, grupo, parametro.Servico, "aliquotas", "Aliquotas") is { } lista)
        {
            var caminho = "aliquotas/" + parametro.Servico;
            foreach (var item in l.Lista(lista, caminho, "Aliquotas"))
                aliquotas.Add(new VigenciaAliquota(
                    l.Numero(item, "Aliq", caminho + "/Aliq", "Aliquotas.Aliquota"),
                    l.Data(item, "DtIni", caminho + "/DtIni", "Aliquotas.Inicio"),
                    l.Data(item, "DtFim", caminho + "/DtFim", "Aliquotas.Fim")));
        }
        return OperacaoConsultaConvenio.Resultado(l, new RetornoAliquota(l.Sucesso, aliquotas, l.Mensagem, l.Raiz), "Alíquotas encontradas");
    }

    // O valor do serviço consultado no objeto por serviço; sem ele, o único que houver (com o aviso), ou nada.
    internal static JsonElement? DoServico(LeituraParametro l, JsonElement grupo, string servico, string caminho, string propriedade)
    {
        if (grupo.ValueKind != JsonValueKind.Object)
        {
            l.Avisar(caminho, propriedade, $"O campo {caminho} da resposta do ADN não é um objeto por serviço.");
            return null;
        }
        if (LeituraParametro.Campo(grupo, servico) is { } valor)
            return valor;
        var todos = grupo.EnumerateObject().ToList();
        if (todos.Count == 1)
        {
            l.Avisar(caminho, propriedade, $"A resposta do ADN traz o serviço {todos[0].Name}, e não o {servico} consultado: lido o que veio.");
            return todos[0].Value;
        }
        if (todos.Count > 1)
            l.Avisar(caminho, propriedade, $"A resposta do ADN não traz o serviço {servico} consultado.");
        return null;
    }
}

internal sealed class OperacaoConsultaRegimesEspeciais : IOperacaoRest<ConsultaRegimesEspeciais, RetornoRegimesEspeciais>
{
    public PedidoRest Montar(ConsultaRegimesEspeciais parametro, ContextoOperacao contexto) =>
        Parametrizacao.Pedido($"{OperacaoConsultaConvenio.Codigo(parametro.Municipio)}/{parametro.Servico}/{Parametrizacao.Competencia(parametro.Competencia)}/regimes_especiais");

    public LeituraWS<RetornoRegimesEspeciais> Ler(ConsultaRegimesEspeciais parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var l = LeituraParametro.Ler(resposta);
        var regimes = new List<RegimeEspecialMunicipal>();
        if (l.Sucesso && l.Grupo("regimesEspeciais", "Regimes") is { } grupo
            && OperacaoConsultaAliquota.DoServico(l, grupo, parametro.Servico, "regimesEspeciais", "Regimes") is { } porRegime)
        {
            var caminho = "regimesEspeciais/" + parametro.Servico;
            if (porRegime.ValueKind != JsonValueKind.Object)
                l.Avisar(caminho, "Regimes", $"O campo {caminho} da resposta do ADN não é um objeto por regime.");
            else
                foreach (var regime in porRegime.EnumerateObject())
                {
                    var codigo = int.TryParse(regime.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var numero) ? numero : (int?)null;
                    if (codigo is null)
                        l.Avisar(caminho + "/" + regime.Name, "Regimes.Regime", $"O regime {regime.Name} da resposta do ADN não é um código.");
                    var valor = l.Enumeracao<RegimeEspecialTributacao>(codigo, caminho + "/" + regime.Name, "Regimes.Regime");
                    var onde = caminho + "/" + regime.Name;
                    foreach (var item in l.Lista(regime.Value, onde, "Regimes"))
                        regimes.Add(new RegimeEspecialMunicipal(valor,
                            l.Inteiro(item, "sit", onde + "/sit", "Regimes.Situacao"),
                            l.Data(item, "dtIni", onde + "/dtIni", "Regimes.Inicio"),
                            l.Data(item, "dtFim", onde + "/dtFim", "Regimes.Fim")));
                }
        }
        return OperacaoConsultaConvenio.Resultado(l, new RetornoRegimesEspeciais(l.Sucesso, regimes, l.Mensagem, l.Raiz), "Regimes especiais encontrados");
    }
}

internal sealed class OperacaoConsultaRetencoes : IOperacaoRest<ConsultaRetencoes, RetornoRetencoes>
{
    public PedidoRest Montar(ConsultaRetencoes parametro, ContextoOperacao contexto) =>
        Parametrizacao.Pedido($"{OperacaoConsultaConvenio.Codigo(parametro.Municipio)}/{Parametrizacao.Competencia(parametro.Competencia)}/retencoes");

    public LeituraWS<RetornoRetencoes> Ler(ConsultaRetencoes parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var l = LeituraParametro.Ler(resposta);
        bool? artigo6 = null;
        var retencoes = new List<RetencaoMunicipal>();
        if (l.Sucesso && l.Grupo("retencoes", "Retencoes") is { } grupo)
        {
            if (LeituraParametro.Campo(grupo, "art6") is { } art6)
                artigo6 = l.Logico(art6, "habilitado", "retencoes/art6/habilitado", "Artigo6Habilitado");
            foreach (var item in l.Lista(LeituraParametro.Campo(grupo, "retMun"), "retencoes/retMun", "Retencoes"))
            {
                var tipos = new List<RetencaoIssqn>();
                foreach (var tipo in l.Lista(LeituraParametro.Campo(item, "tpRet"), "retencoes/retMun/tpRet", "Retencoes.Tipos"))
                    if (l.Enumeracao<RetencaoIssqn>(l.Inteiro(tipo, "retencoes/retMun/tpRet", "Retencoes.Tipos"), "retencoes/retMun/tpRet", "Retencoes.Tipos") is { } valor)
                        tipos.Add(valor);
                var responsaveis = new List<string>();
                foreach (var responsavel in l.Lista(LeituraParametro.Campo(item, "respTrib"), "retencoes/retMun/respTrib", "Retencoes.Responsaveis"))
                    if (l.Texto(responsavel, "insc", "retencoes/retMun/respTrib/insc", "Retencoes.Responsaveis") is { Length: > 0 } documento)
                        responsaveis.Add(documento);
                retencoes.Add(new RetencaoMunicipal(l.Texto(item, "desc", "retencoes/retMun/desc", "Retencoes.Descricao"), tipos, responsaveis));
            }
        }
        return OperacaoConsultaConvenio.Resultado(l, new RetornoRetencoes(l.Sucesso, artigo6, retencoes, l.Mensagem, l.Raiz), "Retenções encontradas");
    }
}

internal sealed class OperacaoConsultaBeneficio : IOperacaoRest<ConsultaBeneficio, RetornoBeneficio>
{
    public PedidoRest Montar(ConsultaBeneficio parametro, ContextoOperacao contexto) =>
        Parametrizacao.Pedido($"{OperacaoConsultaConvenio.Codigo(parametro.Municipio)}/{parametro.Numero}/{Parametrizacao.Competencia(parametro.Competencia)}/beneficio");

    public LeituraWS<RetornoBeneficio> Ler(ConsultaBeneficio parametro, string? xmlPedido, RespostaRest resposta, ContextoOperacao contexto)
    {
        var l = LeituraParametro.Ler(resposta);
        RetornoBeneficio retorno;
        if (l.Sucesso && l.Grupo("beneficio", "Numero") is { } b)
            retorno = new RetornoBeneficio(true,
                l.Texto(b, "numBenef", "beneficio/numBenef", "Numero"),
                l.Texto(b, "desc", "beneficio/desc", "Descricao"),
                l.Enumeracao<TipoBeneficio>(l.Inteiro(b, "tpoBenef", "beneficio/tpoBenef", "Tipo"), "beneficio/tpoBenef", "Tipo"),
                l.Numero(b, "aliqDiferenc", "beneficio/aliqDiferenc", "AliquotaDiferenciada"),
                l.Data(b, "dtIni", "beneficio/dtIni", "Inicio"),
                l.Data(b, "dtFim", "beneficio/dtFim", "Fim"),
                null, l.Raiz);
        else
            retorno = new RetornoBeneficio(l.Sucesso, null, null, null, null, null, null, l.Mensagem, l.Raiz);
        return OperacaoConsultaConvenio.Resultado(l, retorno, "Benefício encontrado");
    }
}
