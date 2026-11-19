using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace OpenDFe.NFSe;

/// <summary>
/// O JSON da resposta da Sefin Nacional (docs/NFSE.md 6, item 1; docs/NTS.md 12.4): <c>chaveAcesso</c>, <c>idDps</c>,
/// <c>nfseXmlGZipB64</c> (descompactado aqui), <c>erros</c> e <c>alertas</c> (<c>codigo</c>, <c>descricao</c>,
/// <c>complemento</c>). Os nomes vêm de uma implementação aberta, sem o Swagger: a leitura não distingue maiúsculas e ignora
/// o que não conhece.
/// </summary>
/// <remarks>
/// O corpo que não se lê é <see cref="JsonException"/> ou <see cref="FormatException"/> (o núcleo os torna a resposta
/// ilegível): JSON mal formado, a raiz que não é objeto, um campo com o tipo errado, o Base64 ou o GZip que não abrem.
/// </remarks>
internal sealed class RespostaSefin
{
    private static readonly UTF8Encoding Utf8Estrito = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private RespostaSefin(int status)
    {
        Status = status;
    }

    public int Status { get; }

    public string? ChaveAcesso { get; private init; }

    public string? IdDps { get; private init; }

    /// <summary>A NFS-e, já descompactada.</summary>
    public string? Xml { get; private init; }

    public IReadOnlyList<MensagemSefin> Erros { get; private init; } = [];

    public IReadOnlyList<MensagemSefin> Alertas { get; private init; } = [];

    /// <summary>O evento do registro (<c>tipoEvento</c>, <c>numSeqEvento</c>, <c>eventoXmlGZipB64</c>; 9.3).</summary>
    public EventoSefin? Evento { get; private init; }

    /// <summary>A lista <c>eventos</c> da consulta dos eventos; <see langword="null"/> quando a resposta não a traz (9.3).</summary>
    public IReadOnlyList<EventoSefin>? Eventos { get; private init; }

    /// <summary>O status é 2xx.</summary>
    public bool Sucesso => Status is >= 200 and <= 299;

    /// <summary>O motivo do resultado: o primeiro erro, ou <paramref name="sucesso"/>, ou o status.</summary>
    public string Motivo(string sucesso) =>
        Erros.Count > 0 ? Erros[0].ToString() : Sucesso ? sucesso : $"HTTP {Status}";

    internal static RespostaSefin Ler(RespostaRest resposta)
    {
        if (resposta.Corpo is null)
            return new RespostaSefin(resposta.Status);
        using var json = JsonDocument.Parse(resposta.Corpo);
        var raiz = json.RootElement;
        if (raiz.ValueKind != JsonValueKind.Object)
            throw new FormatException($"A resposta da Sefin (HTTP {resposta.Status}) não é um objeto JSON.");
        var xml = Texto(raiz, "nfseXmlGZipB64");
        return new RespostaSefin(resposta.Status)
        {
            ChaveAcesso = Texto(raiz, "chaveAcesso"),
            IdDps = Texto(raiz, "idDps"),
            Xml = xml is null ? null : Descompactar(xml),
            Erros = Mensagens(raiz, "erros"),
            Alertas = Mensagens(raiz, "alertas"),
            Evento = LerEvento(raiz),
            Eventos = ListaEventos(raiz),
        };
    }

    // O evento no próprio objeto (o 201 do registro): só quando há o XML ou o tipo.
    private static EventoSefin? LerEvento(JsonElement objeto)
    {
        var xml = Texto(objeto, "eventoXmlGZipB64");
        var tipo = Texto(objeto, "tipoEvento");
        if (xml is null && tipo is null)
            return null;
        return new EventoSefin(Texto(objeto, "chaveAcesso"), tipo, Sequencia(objeto), Texto(objeto, "dataHoraProcessamento"),
            xml is null ? null : Descompactar(xml));
    }

    private static EventoSefin[]? ListaEventos(JsonElement objeto)
    {
        if (Campo(objeto, "eventos") is not { } lista)
            return null;
        if (lista.ValueKind != JsonValueKind.Array)
            throw new FormatException("O campo eventos da resposta da Sefin não é uma lista.");
        var eventos = new List<EventoSefin>();
        foreach (var item in lista.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                throw new FormatException("Um item de eventos da resposta da Sefin não é um objeto.");
            var xml = Texto(item, "eventoXmlGZipB64");
            eventos.Add(new EventoSefin(Texto(item, "chaveAcesso"), Texto(item, "tipoEvento"), Sequencia(item),
                Texto(item, "dataHoraProcessamento"), xml is null ? null : Descompactar(xml)));
        }
        return [.. eventos];
    }

    private static int? Sequencia(JsonElement objeto)
    {
        if (Texto(objeto, "numSeqEvento") is not { } texto)
            return null;
        if (!int.TryParse(texto, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var valor))
            throw new FormatException($"O campo numSeqEvento da resposta da Sefin não é um número: {texto}.");
        return valor;
    }

    /// <summary>O Base64 em GZip, de volta ao texto UTF-8.</summary>
    internal static string Descompactar(string base64)
    {
        try
        {
            using var entrada = new MemoryStream(Convert.FromBase64String(base64));
            using var gzip = new GZipStream(entrada, CompressionMode.Decompress);
            using var saida = new MemoryStream();
            gzip.CopyTo(saida);
            return Utf8Estrito.GetString(saida.ToArray());
        }
        catch (Exception e) when (e is InvalidDataException or DecoderFallbackException)
        {
            throw new FormatException($"O documento compactado da resposta não abre: {e.Message}", e);
        }
    }

    private static JsonElement? Campo(JsonElement objeto, string nome)
    {
        foreach (var propriedade in objeto.EnumerateObject())
        {
            if (string.Equals(propriedade.Name, nome, StringComparison.OrdinalIgnoreCase))
                return propriedade.Value.ValueKind == JsonValueKind.Null ? null : propriedade.Value;
        }
        return null;
    }

    private static string? Texto(JsonElement objeto, string nome) => Campo(objeto, nome) switch
    {
        null => null,
        { ValueKind: JsonValueKind.String } valor => valor.GetString(),
        { ValueKind: JsonValueKind.Number } valor => valor.GetRawText(),
        { } valor => throw new FormatException($"O campo {nome} da resposta da Sefin é {valor.ValueKind}, e não texto."),
    };

    private static MensagemSefin[] Mensagens(JsonElement objeto, string nome)
    {
        if (Campo(objeto, nome) is not { } lista)
            return [];
        if (lista.ValueKind != JsonValueKind.Array)
            throw new FormatException($"O campo {nome} da resposta da Sefin não é uma lista.");
        var mensagens = new List<MensagemSefin>();
        foreach (var item in lista.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                throw new FormatException($"Um item de {nome} da resposta da Sefin não é um objeto.");
            mensagens.Add(new MensagemSefin(Texto(item, "codigo") ?? "", Texto(item, "descricao") ?? "", Texto(item, "complemento")));
        }
        return [.. mensagens];
    }
}

/// <summary>Um evento na resposta da Sefin: a chave, o tipo, o número sequencial, o processamento e o XML descompactado.</summary>
internal sealed record EventoSefin(string? ChaveAcesso, string? Tipo, int? Sequencia, string? Processamento, string? Xml);
