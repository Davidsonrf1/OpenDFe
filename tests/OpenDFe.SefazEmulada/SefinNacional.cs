using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenDFe.SefazEmulada;

// A Sefin Nacional da NFS-e (E.7), com os caminhos da real sob o prefixo /SefinNacional (a base de produção é
// https://sefin.nfse.gov.br/SefinNacional, docs/NTS.md 12.4):
//   POST /SefinNacional/nfse            a emissão: {"dpsXmlGZipB64": "..."}; 201 com a NFS-e, 400 com os erros
//   GET  /SefinNacional/nfse/{chave}    a NFS-e pela chave; 404 se não há
//   GET  /SefinNacional/dps/{id}        a chave da NFS-e gerada pela DPS; 404 se não há
//   HEAD /SefinNacional/dps/{id}        200 ou 404, sem corpo
// Cada rota é um serviço do destino SefinNacional para as falhas comandadas (EmissaoNFSe, ConsultaNFSe, ConsultaDps),
// e cada pedido fica nos PedidosRecentes, com o JSON do pedido e da resposta.
internal static partial class SefinNacional
{
    public const string Destino = "SefinNacional";
    public const string Prefixo = "/" + Destino;
    public const string ServicoEmissao = "EmissaoNFSe";
    public const string ServicoConsultaNFSe = "ConsultaNFSe";
    public const string ServicoConsultaDps = "ConsultaDps";

    public static IReadOnlyList<string> Servicos { get; } = [ServicoEmissao, ServicoConsultaNFSe, ServicoConsultaDps];

    public static void Mapear(WebApplication app)
    {
        var grupo = app.MapGroup(Prefixo);
        grupo.MapPost("/nfse", (HttpContext contexto, AtendimentoSefin atendimento) => atendimento.EmitirAsync(contexto));
        grupo.MapGet("/nfse/{chave}", (HttpContext contexto, string chave, AtendimentoSefin atendimento) => atendimento.ConsultarNFSeAsync(contexto, chave));
        grupo.MapMethods("/dps/{id}", [HttpMethods.Get, HttpMethods.Head], (HttpContext contexto, string id, AtendimentoSefin atendimento) =>
            atendimento.ConsultarDpsAsync(contexto, id));
    }

    // A chave (50 dígitos) e o Id da DPS (DPS + 42 dígitos), como os tipos TSChaveNFSe e TSIdDPS.
    public static bool ChaveValida(string chave) => Chave().IsMatch(chave);

    public static bool IdDpsValido(string id) => IdDps().IsMatch(id);

    [GeneratedRegex("^[0-9]{50}$", RegexOptions.CultureInvariant)]
    private static partial Regex Chave();

    [GeneratedRegex("^DPS[0-9]{42}$", RegexOptions.CultureInvariant)]
    private static partial Regex IdDps();
}

// O atendimento das rotas da Sefin: as falhas de transporte comandadas (pelo controle ou pela marca no xInfComp da DPS),
// a paralisação (503), o serviço e o registro nos pedidos recentes. Os 5xx e a conexão derrubada são falha de transmissão
// para a lib (docs/NFSE.md D5); o 400 e o 404 com JSON são resposta.
internal sealed partial class AtendimentoSefin(Controle controle, EmissaoNFSe emissao, RepositorioNFSe repositorio, PedidosRecentes recentes,
    OpcoesEmulador opcoes, IHostApplicationLifetime vida, ILogger<AtendimentoSefin> registro)
{
    public const string TipoJson = "application/json";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public Task EmitirAsync(HttpContext contexto) => AtenderAsync(contexto, SefinNacional.ServicoEmissao, corpo =>
    {
        if (!MediaTypeHeaderValue.TryParse(contexto.Request.ContentType, out var tipo)
            || !string.Equals(tipo.MediaType, TipoJson, StringComparison.OrdinalIgnoreCase))
            return new Preparo(Pronto: new ResultadoSefin(StatusCodes.Status415UnsupportedMediaType, new { }, "HTTP 415: o Content-Type não é JSON"));
        var (aberta, erro) = EmissaoNFSe.Abrir(corpo);
        // A recusa da recepção passa pelas falhas comandadas, como o resto do processamento.
        if (erro is not null)
            return new Preparo(Processar: recebimento => new ResultadoSefin(StatusCodes.Status400BadRequest, EmissaoNFSe.Erro(recebimento, null, erro), "rejeitada: " + erro.Codigo));
        var agora = opcoes.Relogio.GetUtcNow();
        MarcaFalha? marca;
        try
        {
            marca = Marca.LerNFSe(aberta!.Dps);
        }
        catch (FalhaSoapException falha)
        {
            var id = aberta!.Dps.Element(EmissaoNFSe.Namespace + "infDPS")?.Attribute("Id")?.Value;
            return new Preparo(Pronto: new ResultadoSefin(StatusCodes.Status400BadRequest,
                EmissaoNFSe.Erro(agora, id, ErroSefin.Criar(MotivosNFSe.MarcaInvalida, falha.Message)), "marca inválida: " + falha.Message));
        }
        return new Preparo(marca, recebimento => emissao.Emitir(aberta, recebimento));
    });

    public Task ConsultarNFSeAsync(HttpContext contexto, string chave) => AtenderAsync(contexto, SefinNacional.ServicoConsultaNFSe, _ =>
        new Preparo(Processar: recebimento =>
        {
            if (!SefinNacional.ChaveValida(chave))
                return new ResultadoSefin(StatusCodes.Status400BadRequest, EmissaoNFSe.Erro(recebimento, null, ErroSefin.Criar(MotivosNFSe.ConsultaInvalida, "A chave tem 50 dígitos.")), "consulta inválida");
            if (repositorio.Chave(chave) is not { } nota)
                return new ResultadoSefin(StatusCodes.Status404NotFound, EmissaoNFSe.Erro(recebimento, null, ErroSefin.Criar(MotivosNFSe.NFSeInexistente)), "NFS-e não encontrada");
            return new ResultadoSefin(StatusCodes.Status200OK, new RespostaNFSe(2, EmissaoNFSe.VersaoAplicativo, EmissaoNFSe.DataHora(recebimento), nota.Chave,
                EmissaoNFSe.Compactar(nota.Xml)), "NFS-e encontrada");
        }));

    public Task ConsultarDpsAsync(HttpContext contexto, string id) => AtenderAsync(contexto, SefinNacional.ServicoConsultaDps, _ =>
        new Preparo(Processar: recebimento =>
        {
            if (!SefinNacional.IdDpsValido(id))
                return new ResultadoSefin(StatusCodes.Status400BadRequest, EmissaoNFSe.Erro(recebimento, id, ErroSefin.Criar(MotivosNFSe.ConsultaInvalida, "O Id da DPS é DPS e 42 dígitos.")), "consulta inválida");
            if (repositorio.Dps(id) is not { } nota)
                return new ResultadoSefin(StatusCodes.Status404NotFound, EmissaoNFSe.Erro(recebimento, id, ErroSefin.Criar(MotivosNFSe.DpsInexistente)), "DPS sem NFS-e");
            return new ResultadoSefin(StatusCodes.Status200OK, new RespostaDps(2, EmissaoNFSe.VersaoAplicativo, EmissaoNFSe.DataHora(recebimento), id, nota.Chave),
                "DPS com NFS-e: " + nota.Chave);
        }));

    private async Task AtenderAsync(HttpContext contexto, string servico, Func<byte[], Preparo> preparar)
    {
        var inicio = opcoes.Relogio.GetTimestamp();
        var recebimento = opcoes.Relogio.GetUtcNow();
        var transmissor = Transmissor(contexto.Connection.ClientCertificate);
        var cabeca = HttpMethods.IsHead(contexto.Request.Method);
        using var cancelamento = CancellationTokenSource.CreateLinkedTokenSource(contexto.RequestAborted, vida.ApplicationStopping);
        var ct = cancelamento.Token;
        using var memoria = new MemoryStream();
        await contexto.Request.Body.CopyToAsync(memoria, ct);
        var corpo = memoria.ToArray();
        // O pedido guardado: o corpo, ou o método e o caminho da consulta, que não tem corpo.
        var pedido = corpo.Length > 0 ? corpo : Encoding.UTF8.GetBytes($"{contexto.Request.Method} {contexto.Request.Path}");
        void Guardar(int? status, byte[]? resposta, string situacao) =>
            recentes.Registrar(SefinNacional.Destino, servico, recebimento, transmissor, pedido, status, resposta, situacao, opcoes.Relogio.GetElapsedTime(inicio));

        var preparo = preparar(corpo);
        if (preparo.Pronto is { } pronto)
        {
            await ResponderAsync(contexto, servico, pronto, Guardar, cabeca, ct);
            return;
        }

        var (transporte, paralisacao, _) = controle.Consumir(SefinNacional.Destino, servico,
            transporte: preparo.Marca?.Transporte is null, paralisacao: preparo.Marca?.Paralisacao is null);
        transporte = preparo.Marca?.Transporte ?? transporte;
        paralisacao = preparo.Marca?.Paralisacao ?? paralisacao;
        switch (transporte)
        {
            case { Tipo: TipoFalha.Atraso, Milissegundos: { } milissegundos }:
                await Task.Delay(TimeSpan.FromMilliseconds(milissegundos), opcoes.Relogio, ct);
                break;
            case { Tipo: TipoFalha.TempoEsgotado }:
                Guardar(null, null, "tempo esgotado comandado: sem resposta");
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, opcoes.Relogio, ct);
                }
                catch (OperationCanceledException)
                {
                }
                contexto.Abort();
                return;
            case { Tipo: TipoFalha.ConexaoDerrubada, Depois: false }:
                Guardar(null, null, "conexão derrubada comandada, antes de processar");
                contexto.Abort();
                return;
        }

        var resultado = paralisacao is not null
            ? new ResultadoSefin(StatusCodes.Status503ServiceUnavailable, EmissaoNFSe.Indisponivel("Paralisação comandada pelo controle do emulador."), "paralisação comandada: 503")
            : preparo.Processar!(opcoes.Relogio.GetUtcNow());
        var bytes = Serializar(resultado.Corpo);
        var situacao = resultado.Situacao;
        var status = resultado.Status;
        switch (transporte?.Tipo)
        {
            case TipoFalha.ConexaoDerrubada:
                Escrever(servico, situacao + ", e a conexão derrubada");
                // A resposta que sairia fica guardada: é a que a recuperação acha pela consulta da DPS.
                Guardar(null, bytes, situacao + ", e a conexão derrubada antes da resposta");
                contexto.Abort();
                return;
            case TipoFalha.RespostaIlegivel:
                bytes = bytes[..(bytes.Length / 2)];
                situacao += ", com a resposta cortada";
                break;
            case TipoFalha.FalhaSoap:
                status = StatusCodes.Status500InternalServerError;
                bytes = Serializar(new { erros = new[] { new ErroSefin("EMULADA-FALHA", "Erro interno.", "Falha comandada pelo controle do emulador.") } });
                situacao = "erro interno comandado: 500";
                break;
        }
        Escrever(servico, situacao);
        Guardar(status, bytes, situacao);
        await EnviarAsync(contexto, status, bytes, cabeca, ct);
    }

    private async Task ResponderAsync(HttpContext contexto, string servico, ResultadoSefin resultado, Action<int?, byte[]?, string> guardar, bool cabeca, CancellationToken ct)
    {
        var bytes = resultado.Status == StatusCodes.Status415UnsupportedMediaType ? [] : Serializar(resultado.Corpo);
        Escrever(servico, resultado.Situacao);
        guardar(resultado.Status, bytes.Length == 0 ? null : bytes, resultado.Situacao);
        await EnviarAsync(contexto, resultado.Status, bytes, cabeca, ct);
    }

    private static async Task EnviarAsync(HttpContext contexto, int status, byte[] bytes, bool cabeca, CancellationToken ct)
    {
        contexto.Response.StatusCode = status;
        if (bytes.Length == 0)
            return;
        contexto.Response.ContentType = TipoJson + "; charset=utf-8";
        if (cabeca)
            return;
        await contexto.Response.Body.WriteAsync(bytes, ct);
    }

    private static byte[] Serializar(object corpo) => JsonSerializer.SerializeToUtf8Bytes(corpo, corpo.GetType(), Json);

    // Quem transmitiu: o CNPJ ou o CPF do certificado do TLS, ou a impressão digital (como no Atendimento da NF-e).
    private static string Transmissor(X509Certificate2? certificado)
    {
        if (certificado is null)
            return "";
        var titular = CertificadoDigital.LerTitular(certificado);
        return titular?.Cnpj ?? titular?.Cpf ?? certificado.Thumbprint;
    }

    private void Escrever(string servico, string situacao)
    {
        if (opcoes.Registrar)
            RegistrarPedido(registro, SefinNacional.Destino, servico, situacao);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Destino}/{Servico}: {Situacao}")]
    private static partial void RegistrarPedido(ILogger registro, string destino, string servico, string situacao);

    // O que o serviço prepara antes das falhas: a resposta pronta (o 415 e a marca inválida, como na NF-e), ou a
    // marca da DPS e o processamento, que roda depois das falhas de transporte, no instante dado.
    private sealed record Preparo(MarcaFalha? Marca = null, Func<DateTimeOffset, ResultadoSefin>? Processar = null, ResultadoSefin? Pronto = null);
}
