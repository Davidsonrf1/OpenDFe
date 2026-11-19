using System.Globalization;
using System.Text.Json;

namespace OpenDFe.NFSe;

/// <summary>
/// A parametrização municipal do ADN na tabela de URLs do leiaute (docs/NFSE.md 11, P1; docs/NTS.md 12.4): a base de cada
/// ambiente, com o nome do serviço <c>"Parametrizacao"</c>, e a forma dos argumentos no caminho.
/// </summary>
internal static class Parametrizacao
{
    /// <summary>O nome do serviço na tabela de URLs.</summary>
    internal const string Servico = "Parametrizacao";

    internal static readonly Uri Producao = new("https://adn.nfse.gov.br/parametrizacao");

    internal static readonly Uri ProducaoRestrita = new("https://adn.producaorestrita.nfse.gov.br/parametrizacao");

    /// <summary>Declara as duas bases.</summary>
    internal static void Declarar(DeclaracaoLeiaute declaracao)
    {
        declaracao.DeclararEndereco(new EnderecoServico(Destino.ADN, Ambiente.Producao, Servico, Producao));
        declaracao.DeclararEndereco(new EnderecoServico(Destino.ADN, Ambiente.Homologacao, Servico, ProducaoRestrita));
    }

    /// <summary>O pedido GET de uma rota da parametrização.</summary>
    internal static PedidoRest Pedido(string caminho) =>
        new(Servico, MetodoRest.Get, caminho) { Destino = Destino.ADN };

    /// <summary>O código IBGE do município (P5): 7 dígitos.</summary>
    internal static int Municipio(int municipio, string nome) =>
        municipio is >= 1_000_000 and <= 9_999_999
            ? municipio
            : throw new ArgumentOutOfRangeException(nome, municipio, "O código IBGE do município tem 7 dígitos.");

    /// <summary>O serviço no caminho (P3): o <c>cTribNac</c> (6 dígitos) e o complemento (3), como <c>XX.XX.XX.XXX</c>.</summary>
    internal static string CodigoServico(string tributacaoNacional, string complemento)
    {
        if (tributacaoNacional is null || tributacaoNacional.Length != 6 || !SoDigitos(tributacaoNacional))
            throw new ArgumentException("O código de tributação nacional (cTribNac) tem 6 dígitos.", nameof(tributacaoNacional));
        if (complemento is null || complemento.Length != 3 || !SoDigitos(complemento))
            throw new ArgumentException("O complemento municipal do serviço tem 3 dígitos.", nameof(complemento));
        return $"{tributacaoNacional[..2]}.{tributacaoNacional[2..4]}.{tributacaoNacional[4..]}.{complemento}";
    }

    /// <summary>A competência no caminho (P4): <c>MM-DD-AAAA</c>.</summary>
    internal static string Competencia(DateOnly competencia) =>
        competencia.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);

    internal static bool SoDigitos(string texto) => texto.Length > 0 && texto.AsSpan().IndexOfAnyExceptInRange('0', '9') < 0;
}

/// <summary>
/// A leitura tolerante do JSON da parametrização (docs/NFSE.md 11, P2): os nomes sem distinguir maiúsculas, os tipos
/// misturados (<c>true</c>, <c>"1"</c>, <c>"S"</c>, <c>"Sim"</c>; o número em texto), e o que não se lê como nulo, com o aviso
/// <c>NFSE-PARAMETRO</c>. O corpo que não é JSON, ou cuja raiz não é objeto, é a resposta ilegível (o núcleo embrulha a
/// <see cref="JsonException"/> e a <see cref="FormatException"/>).
/// </summary>
internal sealed class LeituraParametro
{
    private static readonly string[] FormatosData =
    [
        "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF", "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss",
    ];

    private readonly List<Problema> _problemas = [];

    private LeituraParametro(RespostaRest resposta, JsonElement? raiz)
    {
        Status = resposta.Status;
        Raiz = raiz;
    }

    public int Status { get; }

    /// <summary>O JSON inteiro, clonado (sobrevive ao documento); nulo sem corpo.</summary>
    public JsonElement? Raiz { get; }

    public bool Sucesso => Status is >= 200 and <= 299;

    public IReadOnlyList<Problema> Problemas => _problemas;

    /// <summary>O campo <c>mensagem</c> do erro; nulo no sucesso e sem ele.</summary>
    public string? Mensagem => Sucesso || Raiz is not { } raiz ? null : Texto(raiz, "mensagem", "mensagem", "Mensagem");

    /// <summary>O motivo do resultado: a mensagem, ou <paramref name="sucesso"/>, ou o status.</summary>
    public string Motivo(string sucesso) => Mensagem ?? (Sucesso ? sucesso : $"HTTP {Status}");

    internal static LeituraParametro Ler(RespostaRest resposta)
    {
        if (string.IsNullOrWhiteSpace(resposta.Corpo))
            return new LeituraParametro(resposta, null);
        using var json = JsonDocument.Parse(resposta.Corpo);
        if (json.RootElement.ValueKind != JsonValueKind.Object)
            throw new FormatException($"A resposta do ADN (HTTP {resposta.Status}) não é um objeto JSON.");
        return new LeituraParametro(resposta, json.RootElement.Clone());
    }

    /// <summary>O grupo do sucesso (<c>parametrosConvenio</c>, <c>aliquotas</c>...); sem ele, o aviso.</summary>
    public JsonElement? Grupo(string nome, string propriedade)
    {
        if (Raiz is not { } raiz)
        {
            Avisar(nome, propriedade, $"A resposta do ADN não traz o {nome}.");
            return null;
        }
        var grupo = Campo(raiz, nome);
        if (grupo is null)
            Avisar(nome, propriedade, $"A resposta do ADN não traz o {nome}.");
        return grupo;
    }

    /// <summary>O campo do objeto, sem distinguir maiúsculas; nulo se ausente ou <c>null</c>.</summary>
    public static JsonElement? Campo(JsonElement objeto, string nome)
    {
        if (objeto.ValueKind != JsonValueKind.Object)
            return null;
        foreach (var propriedade in objeto.EnumerateObject())
        {
            if (string.Equals(propriedade.Name, nome, StringComparison.OrdinalIgnoreCase))
                return propriedade.Value.ValueKind == JsonValueKind.Null ? null : propriedade.Value;
        }
        return null;
    }

    /// <summary>Os itens de uma lista; o que não é lista é o aviso, e nenhum item.</summary>
    public IEnumerable<JsonElement> Lista(JsonElement? valor, string caminho, string propriedade)
    {
        if (valor is not { } lista)
            return [];
        if (lista.ValueKind != JsonValueKind.Array)
        {
            Avisar(caminho, propriedade, $"O campo {caminho} da resposta do ADN não é uma lista.");
            return [];
        }
        return lista.EnumerateArray();
    }

    public string? Texto(JsonElement objeto, string nome, string caminho, string propriedade) => Campo(objeto, nome) switch
    {
        null => null,
        { ValueKind: JsonValueKind.String } v => v.GetString(),
        { ValueKind: JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False } v => v.GetRawText(),
        _ => TextoNulo(caminho, propriedade),
    };

    public bool? Logico(JsonElement objeto, string nome, string caminho, string propriedade)
    {
        var valor = Campo(objeto, nome);
        switch (valor?.ValueKind)
        {
            case null:
                return null;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Number when valor.Value.TryGetInt32(out var numero) && numero is 0 or 1:
                return numero == 1;
            case JsonValueKind.String:
                switch (valor.Value.GetString()!.Trim().ToUpperInvariant())
                {
                    case "1" or "S" or "SIM" or "TRUE" or "VERDADEIRO":
                        return true;
                    case "0" or "N" or "NAO" or "NÃO" or "FALSE" or "FALSO":
                        return false;
                }
                break;
        }
        return Nulo<bool>(caminho, propriedade, "sim ou não");
    }

    public decimal? Numero(JsonElement objeto, string nome, string caminho, string propriedade)
    {
        var valor = Campo(objeto, nome);
        if (valor is null)
            return null;
        if (valor.Value.ValueKind == JsonValueKind.Number && valor.Value.TryGetDecimal(out var numero))
            return numero;
        if (valor.Value.ValueKind == JsonValueKind.String
            && decimal.TryParse(valor.Value.GetString()!.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out numero))
            return numero;
        return Nulo<decimal>(caminho, propriedade, "um número");
    }

    public int? Inteiro(JsonElement valor, string caminho, string propriedade)
    {
        if (valor.ValueKind == JsonValueKind.Number && valor.TryGetInt32(out var numero))
            return numero;
        if (valor.ValueKind == JsonValueKind.String
            && int.TryParse(valor.GetString()!.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out numero))
            return numero;
        return Nulo<int>(caminho, propriedade, "um número inteiro");
    }

    public int? Inteiro(JsonElement objeto, string nome, string caminho, string propriedade) =>
        Campo(objeto, nome) is { } valor ? Inteiro(valor, caminho, propriedade) : null;

    /// <summary>A data sem fuso (<c>Kind</c> <see cref="DateTimeKind.Unspecified"/>); com fuso, a hora local dele.</summary>
    public DateTime? Data(JsonElement objeto, string nome, string caminho, string propriedade)
    {
        if (Campo(objeto, nome) is not { } valor)
            return null;
        if (valor.ValueKind == JsonValueKind.String)
        {
            var texto = valor.GetString()!.Trim();
            if (DateTime.TryParseExact(texto, FormatosData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                return DateTime.SpecifyKind(data, DateTimeKind.Unspecified);
            if (DateTimeOffset.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out var comFuso))
                return comFuso.DateTime;
        }
        return Nulo<DateTime>(caminho, propriedade, "uma data");
    }

    /// <summary>O valor da enumeração pelo código; fora dela, o aviso e nulo (P7).</summary>
    public TEnum? Enumeracao<TEnum>(int? codigo, string caminho, string propriedade) where TEnum : struct, Enum
    {
        if (codigo is not { } valor)
            return null;
        var item = (TEnum)Enum.ToObject(typeof(TEnum), valor);
        if (Enum.IsDefined(item))
            return item;
        Avisar(caminho, propriedade, $"O código {valor} do campo {caminho} da resposta do ADN não está em {typeof(TEnum).Name}.");
        return null;
    }

    private T? Nulo<T>(string caminho, string propriedade, string esperado) where T : struct
    {
        Avisar(caminho, propriedade, $"O campo {caminho} da resposta do ADN não é {esperado}: fica nulo, e o JSON está ao lado.");
        return null;
    }

    private string? TextoNulo(string caminho, string propriedade)
    {
        Avisar(caminho, propriedade, $"O campo {caminho} da resposta do ADN não é texto: fica nulo, e o JSON está ao lado.");
        return null;
    }

    public void Avisar(string caminho, string propriedade, string mensagem) =>
        _problemas.Add(new Problema(CodigosNFSe.Parametro, Severidade.Aviso, mensagem)
        {
            Tag = caminho[(caminho.LastIndexOf('/') + 1)..],
            Caminho = caminho,
            Propriedade = propriedade,
        });
}
